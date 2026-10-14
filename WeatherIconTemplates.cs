using Avalonia.Controls.Templates;
using Avalonia.Media;

namespace QWeatherIconsPlugin;

/// <summary>
/// 构造和风天气图标模板。
/// </summary>
internal static class WeatherIconTemplates
{
    /// <summary>线框风格图标包 ID。</summary>
    public const string LineIconPackId = "qweather.weatherIcons.line";

    /// <summary>填充风格图标包 ID。</summary>
    public const string FillIconPackId = "qweather.weatherIcons.fill";

    private static readonly Dictionary<string, Geometry> LineCache = new();
    private static readonly Dictionary<string, Geometry> FillCache = new();

    /// <summary>创建线框风格图标模板。</summary>
    public static IDataTemplate CreateLineTemplate() =>
        CreateTemplate(QWeatherIconData.Line, LineCache);

    /// <summary>创建填充风格图标模板。</summary>
    public static IDataTemplate CreateFillTemplate() =>
        CreateTemplate(QWeatherIconData.Fill, FillCache);

    private static IDataTemplate CreateTemplate(
        IReadOnlyDictionary<string, string> iconPaths,
        Dictionary<string, Geometry> cache)
    {
        // DataTemplate 的 DataContext 就是天气代码字符串（见 WeatherPackIconControl.axaml）
        return new FuncDataTemplate<string>((code, _) => new QWeatherIcon
        {
            Data = Resolve(code, iconPaths, cache),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        });
    }

    private static Geometry Resolve(
        string? weatherCode,
        IReadOnlyDictionary<string, string> iconPaths,
        Dictionary<string, Geometry> cache)
    {
        var iconCode = weatherCode is not null
                       && QWeatherIconData.CodeMap.TryGetValue(weatherCode, out var mapped)
            ? mapped
            : QWeatherIconData.UnknownCode;

        // 和风官方说明图标会增删，映射未命中时回退到「未知」
        if (!iconPaths.ContainsKey(iconCode))
        {
            iconCode = QWeatherIconData.UnknownCode;
        }

        if (cache.TryGetValue(iconCode, out var cached))
        {
            return cached;
        }

        // 路径数据由 tools/gen_icons.py 从和风 SVG 中抽取，格式合法，解析失败时降级为空白
        Geometry geometry;
        try
        {
            geometry = StreamGeometry.Parse(iconPaths[iconCode]);
        }
        catch (Exception)
        {
            geometry = new StreamGeometry();
        }

        cache[iconCode] = geometry;
        return geometry;
    }
}
