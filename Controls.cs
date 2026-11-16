using System.Windows;
using System.Windows.Media;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Pen = System.Windows.Media.Pen;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace usage;

public sealed class RingGauge : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(RingGauge),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty ThicknessProperty =
        DependencyProperty.Register(nameof(Thickness), typeof(double), typeof(RingGauge),
            new FrameworkPropertyMetadata(14d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty TrackBrushProperty =
        DependencyProperty.Register(nameof(TrackBrush), typeof(Brush), typeof(RingGauge),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0x2A, 0x2E, 0x36)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillBrushProperty =
        DependencyProperty.Register(nameof(FillBrush), typeof(Brush), typeof(RingGauge),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE2, 0xB6, 0x6A)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    public Brush TrackBrush
    {
        get => (Brush)GetValue(TrackBrushProperty);
        set => SetValue(TrackBrushProperty, value);
    }

    public Brush FillBrush
    {
        get => (Brush)GetValue(FillBrushProperty);
        set => SetValue(FillBrushProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var t = Thickness;
        var size = Math.Min(ActualWidth, ActualHeight);
        if (size <= t) return;

        var radius = (size - t) / 2;
        var center = new Point(ActualWidth / 2, ActualHeight / 2);
        var trackPen = new Pen(TrackBrush, t) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        var fillPen = new Pen(FillBrush, t) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        dc.DrawEllipse(null, trackPen, center, radius, radius);

        var pct = Math.Clamp(Value, 0, 100) / 100.0;
        if (pct <= 0.0001) return;
        if (pct >= 0.999)
        {
            dc.DrawEllipse(null, fillPen, center, radius, radius);
            return;
        }

        var start = -90 * Math.PI / 180;
        var sweep = pct * 2 * Math.PI;
        var geo = new StreamGeometry();
        using (var ctx = geo.Open())
        {
            var p0 = PointOn(center, radius, start);
            var p1 = PointOn(center, radius, start + sweep);
            ctx.BeginFigure(p0, false, false);
            ctx.ArcTo(p1, new Size(radius, radius), 0, sweep > Math.PI, SweepDirection.Clockwise, true, false);
        }
        geo.Freeze();
        dc.DrawGeometry(null, fillPen, geo);
    }

    private static Point PointOn(Point c, double r, double radians) =>
        new(c.X + r * Math.Cos(radians), c.Y + r * Math.Sin(radians));
}

public sealed class PercentBar : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(nameof(Value), typeof(double), typeof(PercentBar),
            new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    public static readonly DependencyProperty FillProperty =
        DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(PercentBar),
            new FrameworkPropertyMetadata(new SolidColorBrush(Color.FromRgb(0xE2, 0xB6, 0x6A)),
                FrameworkPropertyMetadataOptions.AffectsRender));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public Brush Fill
    {
        get => (Brush)GetValue(FillProperty);
        set => SetValue(FillProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        var w = ActualWidth;
        var h = ActualHeight;
        if (w <= 0 || h <= 0) return;
        dc.DrawRoundedRectangle(Palette.Track, null, new Rect(0, 0, w, h), h / 2, h / 2);
        var pct = Math.Clamp(Value, 0, 100) / 100.0;
        var fw = Math.Max(h, w * pct);
        if (pct > 0)
            dc.DrawRoundedRectangle(Fill, null, new Rect(0, 0, fw, h), h / 2, h / 2);
    }
}
