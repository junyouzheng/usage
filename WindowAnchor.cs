using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;

namespace usage;

internal static class WindowAnchor
{
    public const double Width = 400;
    private const double SideMarginPx = 4;

    public static void SnapToWorkAreaEdge(Window window)
    {
        var work = WorkAreaDip(window);
        var dpi = VisualTreeHelper.GetDpi(window);
        var sx = dpi.DpiScaleX <= 0 ? 1 : dpi.DpiScaleX;
        var side = SideMarginPx / sx;
        var availW = Math.Max(1, work.Width - side * 2);
        var availH = Math.Max(1, work.Height);
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

        // Bottom sits on the work-area edge, which is the taskbar. Height changes grow upward.
        var left = Math.Max(work.Left, work.Right - width - side);
        var top = Math.Max(work.Top, work.Bottom - height);

        if (Math.Abs(window.Left - left) > 0.5 || Math.Abs(window.Top - top) > 0.5)
        {
            window.Left = left;
            window.Top = top;
        }
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
