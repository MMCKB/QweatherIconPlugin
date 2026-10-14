using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace QWeatherIconsPlugin;

/// <summary>
/// 天气图标矢量渲染控件。
/// <para>
/// 采用自绘方式渲染和风天气图标的矢量路径，不依赖图标字体、图片资源或任何主题资源，
/// 因此可以在插件的隔离加载上下文中稳定工作。
/// </para>
/// <para>
/// <see cref="Foreground"/> 直接复用 <see cref="TextElement.ForegroundProperty"/>（继承属性），
/// 因此 ClassIsland 对雨天设置的 <c>RainyColorBrush</c> 前景色以及主题前景色都能自动生效。
/// </para>
/// </summary>
public sealed class QWeatherIcon : Control
{
    /// <summary>和风天气图标的标准画布边长（其 SVG 使用 16×16 的 viewBox）。</summary>
    private const double ViewBoxSize = 16d;

    /// <summary>图标矢量路径数据。</summary>
    public static readonly StyledProperty<Geometry?> DataProperty =
        AvaloniaProperty.Register<QWeatherIcon, Geometry?>(nameof(Data));

    /// <summary>图标填充色，复用继承属性 <see cref="TextElement.ForegroundProperty"/>。</summary>
    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<QWeatherIcon>();

    static QWeatherIcon()
    {
        AffectsRender<QWeatherIcon>(DataProperty, ForegroundProperty);
        AffectsMeasure<QWeatherIcon>(DataProperty);
    }

    /// <inheritdoc cref="DataProperty"/>
    public Geometry? Data
    {
        get => GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    /// <inheritdoc cref="ForegroundProperty"/>
    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize) => new(ViewBoxSize, ViewBoxSize);

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var geometry = Data;
        if (geometry is null || Foreground is null)
        {
            return;
        }

        var scale = Math.Min(Bounds.Width, Bounds.Height) / ViewBoxSize;
        if (scale <= 0 || double.IsNaN(scale) || double.IsInfinity(scale))
        {
            return;
        }

        // 等比缩放并居中，保持各天气图标之间的大小一致（不做几何紧裁切）
        var offsetX = (Bounds.Width - ViewBoxSize * scale) / 2d;
        var offsetY = (Bounds.Height - ViewBoxSize * scale) / 2d;
        var transform = Matrix.CreateScale(scale, scale) * Matrix.CreateTranslation(offsetX, offsetY);

        using (context.PushTransform(transform))
        {
            context.DrawGeometry(Foreground, null, geometry);
        }
    }
}
