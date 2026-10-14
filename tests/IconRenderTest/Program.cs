using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Templates;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Controls.Documents;
using QWeatherIconsPlugin;
using System.Reflection;

namespace IconRenderTest;

/// <summary>
/// 离屏渲染验证：把 40 个天气代码的「线框」「填充」两套图标全部画出来，
/// 并复现 ClassIsland WeatherPackIconControl 的场景，验证前景色继承（雨天变红）。
/// 产物：overview.png、inheritance.png（由 tools/verify_render.py 分析）
/// </summary>
internal static class Program
{
    private const int Cell = 64;

    private static readonly string[] CodeOrder =
    [
        "0", "1", "2", "3", "4", "5", "6", "7", "8", "9",
        "10", "11", "12", "13", "14", "15", "16", "17", "18", "19",
        "20", "21", "22", "23", "24", "25", "26", "27", "28", "29",
        "30", "31", "32", "33", "34", "35", "53", "301", "302", "99"
    ];

    public static int Main(string[] args)
    {
        AppBuilder.Configure<TestApp>()
            .UseSkia()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false })
            .SetupWithoutStarting();

        // 外部程序集模式：加载指定插件 DLL（例如 net8.0 构建产物），
        // 在当前 Avalonia 12 宿主下渲染，用于验证跨版本兼容性。
        if (args.Length >= 2 && args[0] == "--external")
        {
            return RenderExternal(args[1]);
        }

        var line = QWeatherIconData.Line;
        var fill = QWeatherIconData.Fill;

        var parseFailures = new List<string>();
        foreach (var (code, path) in line.Concat(fill))
        {
            try
            {
                var geo = StreamGeometry.Parse(path);
                if (geo.Bounds.Width <= 0 || geo.Bounds.Height <= 0)
                {
                    parseFailures.Add($"{code}: 包围盒为空 {geo.Bounds}");
                }
            }
            catch (Exception ex)
            {
                parseFailures.Add($"{code}: {ex.Message}");
            }
        }

        Console.WriteLine($"几何数据：线框 {line.Count} 个 / 填充 {fill.Count} 个 / 映射 {QWeatherIconData.CodeMap.Count} 条");
        Console.WriteLine($"路径解析失败：{parseFailures.Count}");
        foreach (var f in parseFailures)
        {
            Console.WriteLine("  " + f);
        }

        // 映射完整性：每个小米代码都要能取到几何数据
        var missing = CodeOrder
            .Where(c => !line.ContainsKey(QWeatherIconData.CodeMap[c])
                        || !fill.ContainsKey(QWeatherIconData.CodeMap[c]))
            .ToList();
        Console.WriteLine($"映射缺图：{missing.Count} {string.Join(",", missing)}");

        var overview = RenderGrid(line, fill, out var cells);
        Save(overview, "overview.png");
        Console.WriteLine($"总览图：{cells} 格");

        var inherit = RenderInheritance();
        Save(inherit, "inheritance.png");
        Console.WriteLine("前景色继承图：已生成");

        return parseFailures.Count == 0 && missing.Count == 0 ? 0 : 1;
    }

    /// <summary>
    /// 加载外部插件程序集（例如 net8.0 构建产物），用它的 <c>QWeatherIcon</c> 控件
    /// 在当前宿主（Avalonia 12）下渲染，输出 external.png 供与 overview.png 逐像素比对。
    /// </summary>
    private static int RenderExternal(string dllPath)
    {
        dllPath = Path.GetFullPath(dllPath);
        Console.WriteLine($"加载外部程序集：{dllPath}");

        var asm = System.Reflection.Assembly.LoadFrom(dllPath);
        Console.WriteLine($"  程序集：{asm.FullName}");

        var iconType = asm.GetType("QWeatherIconsPlugin.QWeatherIcon", throwOnError: true)!;
        var dataProp = iconType.GetProperty("Data", BindingFlags.Public | BindingFlags.Instance)!;

        var dataType = asm.GetType("QWeatherIconsPlugin.QWeatherIconData", throwOnError: true)!;
        var line = (System.Collections.IEnumerable)dataType
            .GetField("Line", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;

        var geometryByCode = new Dictionary<string, string>();
        foreach (var item in line)
        {
            var t = item.GetType();
            geometryByCode[(string)t.GetProperty("Key")!.GetValue(item)!] =
                (string)t.GetProperty("Value")!.GetValue(item)!;
        }

        var codeMap = (System.Collections.IEnumerable)dataType
            .GetField("CodeMap", BindingFlags.Public | BindingFlags.Static)!.GetValue(null)!;
        var map = new Dictionary<string, string>();
        foreach (var item in codeMap)
        {
            var t = item.GetType();
            map[(string)t.GetProperty("Key")!.GetValue(item)!] =
                (string)t.GetProperty("Value")!.GetValue(item)!;
        }

        Console.WriteLine($"  读取到 {geometryByCode.Count} 个图标，{map.Count} 条映射");

        var grid = new UniformGrid { Columns = 10, Background = Brushes.White };
        var failures = 0;

        foreach (var weatherCode in CodeOrder)
        {
            if (!map.TryGetValue(weatherCode, out var iconCode)
                || !geometryByCode.TryGetValue(iconCode, out var path))
            {
                Console.WriteLine($"  缺少映射：小米 {weatherCode}");
                failures++;
                continue;
            }

            var icon = Activator.CreateInstance(iconType)!;
            dataProp.SetValue(icon, StreamGeometry.Parse(path));

            var host = new Viewbox
            {
                Width = Cell,
                Height = Cell,
                Stretch = Stretch.Uniform,
                Child = (Control)icon
            };
            grid.Children.Add(host);
        }

        var bitmap = Render(grid);
        var outPath = Path.Combine(AppContext.BaseDirectory, "external.png");
        bitmap.Save(outPath);
        Console.WriteLine($"  已输出 {outPath}  {bitmap.PixelSize.Width}×{bitmap.PixelSize.Height}");

        return failures == 0 ? 0 : 1;
    }

    /// <summary>渲染线框 + 填充两套图标总览。</summary>
    private static Bitmap RenderGrid(
        IReadOnlyDictionary<string, string> line,
        IReadOnlyDictionary<string, string> fill,
        out int cells)
    {
        var grid = new UniformGrid { Columns = 10, Background = Brushes.White };
        cells = 0;

        foreach (var code in CodeOrder)
        {
            grid.Children.Add(Cell2(code, line));
            cells++;
        }

        foreach (var code in CodeOrder)
        {
            grid.Children.Add(Cell2(code, fill));
            cells++;
        }

        return Render(grid);
    }

    /// <summary>
    /// 复现 WeatherPackIconControl.axaml 的渲染结构：
    /// Viewbox → ContentPresenter（设置 TextElement.Foreground）→ 图标模板。
    /// 每组依次用 红 / 蓝 / 绿 三种前景色，验证颜色是否被图标继承。
    /// </summary>
    private static Bitmap RenderInheritance()
    {
        var brushes = new IBrush[] { Brushes.Red, Brushes.Blue, Brushes.Green };
        var codes = new[] { "0", "7", "14", "10" };

        var grid = new UniformGrid { Columns = brushes.Length, Background = Brushes.White };

        foreach (var code in codes)
        {
            foreach (var brush in brushes)
            {
                // WeatherPackIconControl 的外层 Viewbox
                var viewbox = new Viewbox
                {
                    Stretch = Stretch.Uniform,
                    StretchDirection = StretchDirection.Both,
                    HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                    VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
                    Width = Cell,
                    Height = Cell,
                    Child = Cell2(code, QWeatherIconData.Line, brush)
                };
                grid.Children.Add(viewbox);
            }
        }

        return Render(grid);
    }

    private static ContentPresenter Cell2(
        string code, IReadOnlyDictionary<string, string> icons, IBrush? foreground = null)
    {
        var presenter = new ContentPresenter
        {
            Width = Cell,
            Height = Cell,
            Content = code,
            ContentTemplate = BuildTemplate(icons)
        };

        if (foreground is not null)
        {
            TextElement.SetForeground(presenter, foreground);
        }

        return presenter;
    }

    private static IDataTemplate BuildTemplate(IReadOnlyDictionary<string, string> icons)
    {
        return new FuncDataTemplate<string>((code, _) => new QWeatherIcon
        {
            Data = ResolveGeometry(code, icons),
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch
        });
    }

    private static Geometry ResolveGeometry(
        string? weatherCode, IReadOnlyDictionary<string, string> icons)
    {
        var iconCode = weatherCode is not null
                       && QWeatherIconData.CodeMap.TryGetValue(weatherCode, out var mapped)
            ? mapped
            : QWeatherIconData.UnknownCode;

        if (!icons.ContainsKey(iconCode))
        {
            iconCode = QWeatherIconData.UnknownCode;
        }

        return StreamGeometry.Parse(icons[iconCode]);
    }

    private static Bitmap Render(Control content)
    {
        var window = new Window
        {
            SizeToContent = SizeToContent.WidthAndHeight,
            Background = Brushes.White,
            Content = content
        };

        window.Show();
        return window.CaptureRenderedFrame()
               ?? throw new InvalidOperationException("未能捕获渲染帧");
    }

    private static void Save(Bitmap bitmap, string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, fileName);
        bitmap.Save(path);
        Console.WriteLine($"  {fileName}  {bitmap.PixelSize.Width}×{bitmap.PixelSize.Height}");
    }
}

internal sealed class TestApp : Application;
