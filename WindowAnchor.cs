using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

namespace usage;

internal static class WindowAnchor
{
    public const double Width = 400;
    private const double EdgeMarginPx = 4;

    public static void SnapToWorkAreaEdge(Window window)
    {
        var work = WorkAreaDip(window);
        var margin = MarginDip(window);
        var availW = Math.Max(1, work.Width - margin.Width * 2);
        var availH = Math.Max(1, work.Height - margin.Height * 2);
        var width = Math.Min(Width, availW);
        if (Math.Abs(window.Width - width) > 0.5)
            window.MinWidth = window.MaxWidth = window.Width = width;

        var height = window.ActualHeight;
        if (height < 1 || double.IsNaN(height))
            height = window.DesiredSize.Height;
        if (height > availH)
        {
            window.MaxHeight = availH;
            window.Height = availH;
            height = availH;
        }

        var left = Math.Max(work.Left, work.Right - width - margin.Width);
        var top = Math.Max(work.Top, work.Bottom - height - margin.Height);

        if (Math.Abs(window.Left - left) > 1 || Math.Abs(window.Top - top) > 1)
        {
            window.Left = left;
            window.Top = top;
        }
    }

    private static Size MarginDip(Window window)
    {
        var dpi = VisualTreeHelper.GetDpi(window);
        var sx = dpi.DpiScaleX <= 0 ? 1 : dpi.DpiScaleX;
        var sy = dpi.DpiScaleY <= 0 ? 1 : dpi.DpiScaleY;
        return new Size(EdgeMarginPx / sx, EdgeMarginPx / sy);
    }

    private static Rect WorkAreaDip(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var screen = hwnd == IntPtr.Zero
            ? System.Windows.Forms.Screen.PrimaryScreen ?? System.Windows.Forms.Screen.AllScreens[0]
            : System.Windows.Forms.Screen.FromHandle(hwnd);
        var wa = screen.WorkingArea;
        var topLeft = PixelToDip(window, wa.Left, wa.Top);
        var bottomRight = PixelToDip(window, wa.Right, wa.Bottom);
        return new Rect(topLeft, bottomRight);
    }

    private static Point PixelToDip(Window window, double x, double y)
    {
        var source = PresentationSource.FromVisual(window);
        if (source?.CompositionTarget is { } ct)
            return ct.TransformFromDevice.Transform(new Point(x, y));
        var dpi = VisualTreeHelper.GetDpi(window);
        return new Point(x / dpi.DpiScaleX, y / dpi.DpiScaleY);
    }
}
