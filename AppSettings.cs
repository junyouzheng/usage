using System.IO;
using System.Text.Json;
using Microsoft.Win32;
using Path = System.IO.Path;
using File = System.IO.File;
using Directory = System.IO.Directory;

namespace usage;

internal static class AppSettings
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private static readonly string Dir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "usage");
    private static readonly string PathFile = Path.Combine(Dir, "settings.json");
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string RunName = "usage";
    private const string LegacyRunName = "GrokUsageMonitor";

    static AppSettings()
    {
        var legacyDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GrokUsageMonitor");
        if (!Directory.Exists(Dir) && Directory.Exists(legacyDir))
            Directory.Move(legacyDir, Dir);
    }

    public static string DataDir => Dir;

    public static MonitorSettings Load()
    {
        try
        {
            if (File.Exists(PathFile))
                return JsonSerializer.Deserialize<MonitorSettings>(File.ReadAllText(PathFile)) ?? new();
        }
        catch
        {
            // keep defaults
        }

        return new MonitorSettings();
    }

    public static void Save(MonitorSettings settings)
    {
        Directory.CreateDirectory(Dir);
        var tmp = PathFile + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(settings, JsonOpts));
        File.Copy(tmp, PathFile, overwrite: true);
        File.Delete(tmp);
        ApplyStartup(settings.StartWithWindows);
    }

    public static void ApplyStartup(bool enabled)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                        ?? Registry.CurrentUser.CreateSubKey(RunKey);
        if (enabled)
        {
            var exe = Environment.ProcessPath ?? System.Windows.Forms.Application.ExecutablePath;
            key.SetValue(RunName, $"\"{exe}\"");
        }
        else if (key.GetValue(RunName) is not null)
        {
            key.DeleteValue(RunName, throwOnMissingValue: false);
        }

        if (key.GetValue(LegacyRunName) is not null)
            key.DeleteValue(LegacyRunName, throwOnMissingValue: false);
    }
}

internal static class SnapshotStore
{
    private static readonly JsonSerializerOptions JsonOpts = new() { WriteIndented = true };
    private static readonly string PathFile = Path.Combine(AppSettings.DataDir, "last-snapshot.json");

    public static void Save(UsageSnapshot snap)
    {
        if (snap.Error is not null) return;
        Directory.CreateDirectory(AppSettings.DataDir);
        var dto = new StoredSnapshot
        {
            FetchedAt = snap.FetchedAt,
            Email = snap.Email,
            DisplayName = snap.DisplayName,
            Tier = snap.Tier,
            UsedPercent = snap.UsedPercent,
            PeriodStart = snap.PeriodStart,
            PeriodEnd = snap.PeriodEnd,
            PeriodType = snap.PeriodType,
            UnifiedPool = snap.UnifiedPool,
            PrepaidUsd = snap.PrepaidUsd,
            Products = snap.Products.Select(p => new ProductShare
            {
                Id = p.Id,
                Label = p.Label,
                Percent = p.Percent
            }).ToList()
        };
        var tmp = PathFile + ".tmp";
        File.WriteAllText(tmp, JsonSerializer.Serialize(dto, JsonOpts));
        File.Copy(tmp, PathFile, overwrite: true);
        File.Delete(tmp);
    }

    public static UsageSnapshot? Load()
    {
        try
        {
            if (!File.Exists(PathFile)) return null;
            var dto = JsonSerializer.Deserialize<StoredSnapshot>(File.ReadAllText(PathFile), JsonOpts);
            if (dto is null) return null;
            return new UsageSnapshot
            {
                FetchedAt = dto.FetchedAt,
                Email = dto.Email ?? "",
                DisplayName = dto.DisplayName ?? "",
                Tier = dto.Tier ?? "",
                UsedPercent = dto.UsedPercent,
                PeriodStart = dto.PeriodStart,
                PeriodEnd = dto.PeriodEnd,
                PeriodType = string.IsNullOrWhiteSpace(dto.PeriodType) ? "每周" : dto.PeriodType,
                UnifiedPool = dto.UnifiedPool,
                PrepaidUsd = dto.PrepaidUsd,
                Products = dto.Products ?? []
            };
        }
        catch
        {
            return null;
        }
    }

    private sealed class StoredSnapshot
    {
        public DateTimeOffset FetchedAt { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Tier { get; set; }
        public double UsedPercent { get; set; }
        public DateTimeOffset? PeriodStart { get; set; }
        public DateTimeOffset? PeriodEnd { get; set; }
        public string? PeriodType { get; set; }
        public bool UnifiedPool { get; set; }
        public decimal PrepaidUsd { get; set; }
        public List<ProductShare>? Products { get; set; }
    }
}
