using System.Reflection;
using System.Runtime.Loader;
using ClassIsland.Core.Abstractions;

namespace LoadProbe;

/// <summary>
/// 复刻 ClassIsland 的插件加载流程（PluginService.InitializePlugins + PluginLoadContext），
/// 用于在本地验证插件包能否被正确加载、入口类能否被发现。
/// </summary>
internal static class Program
{
    public static int Main(string[] args)
    {
        var dll = args.Length > 0
            ? args[0]
            : throw new ArgumentException("用法: LoadProbe <插件 dll 路径>");

        dll = Path.GetFullPath(dll);
        Console.WriteLine($"探测插件：{dll}");
        Console.WriteLine($"宿主 ClassIsland.Core：{typeof(PluginBase).Assembly.FullName}");
        Console.WriteLine($"宿主 PluginBase：{typeof(PluginBase).AssemblyQualifiedName}");
        Console.WriteLine(new string('-', 70));

        var context = new ProbeLoadContext(dll);
        var name = new AssemblyName(Path.GetFileNameWithoutExtension(dll));

        try
        {
            var asm = context.LoadFromAssemblyName(name);
            Console.WriteLine($"程序集加载成功：{asm.FullName}");

            var refs = asm.GetReferencedAssemblies().OrderBy(x => x.Name).ToList();
            Console.WriteLine($"引用程序集 {refs.Count} 个：");
            foreach (var r in refs)
            {
                Console.WriteLine($"    {r.Name} {r.Version}");
            }

            Console.WriteLine(new string('-', 70));

            // 复刻 PluginService 的入口类查找逻辑
            var exported = asm.ExportedTypes.ToList();
            Console.WriteLine($"导出类型 {exported.Count} 个：");
            foreach (var t in exported)
            {
                Console.WriteLine($"    {t.FullName}  →  BaseType = {t.BaseType}");
            }

            var entrance = exported.FirstOrDefault(x =>
                x.BaseType == typeof(PluginBase) ||
                x.GetCustomAttributes().Any(a => a.GetType() == typeof(ClassIsland.Core.Attributes.PluginEntrance)));

            Console.WriteLine(new string('-', 70));
            if (entrance is null)
            {
                Console.WriteLine("结果：找不到插件入口类（ClassIsland 会静默跳过）");
                return 1;
            }

            Console.WriteLine($"找到入口类：{entrance.FullName}");

            if (Activator.CreateInstance(entrance) is not PluginBase obj)
            {
                Console.WriteLine("结果：入口类无法实例化为 PluginBase");
                return 1;
            }

            Console.WriteLine($"入口类实例化成功：{obj.GetType().FullName}");

            // 触发图标控件的静态构造函数：这里会调用 Avalonia 的
            // AvaloniaProperty.Register 与 TextElement.ForegroundProperty.AddOwner。
            // 若插件编译时所用的 Avalonia 与宿主版本签名不兼容，此处会抛异常。
            var iconType = exported.FirstOrDefault(x => x.Name == "QWeatherIcon");
            if (iconType is not null)
            {
                var dataProperty = iconType.GetField(
                    "DataProperty", BindingFlags.Public | BindingFlags.Static);
                if (dataProperty is null)
                {
                    Console.WriteLine("结果：找不到 QWeatherIcon.DataProperty");
                    return 1;
                }

                var value = dataProperty.GetValue(null);
                Console.WriteLine($"图标控件静态构造成功：DataProperty = {value?.GetType().Name}");
            }

            // 真正调用 Initialize，验证图标包是否注册进宿主的注册表
            var before = ClassIsland.Core.Abstractions.Services.IWeatherService
                .RegisteredTemplates.Count;

            var initialize = entrance.GetMethod("Initialize")!;
            var hostContext = new Microsoft.Extensions.Hosting.HostBuilderContext(
                new Dictionary<object, object>());
            var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
            initialize.Invoke(obj, [hostContext, services]);

            var registered = ClassIsland.Core.Abstractions.Services.IWeatherService
                .RegisteredTemplates;
            Console.WriteLine($"Initialize 执行成功，注册表 {before} → {registered.Count} 项：");
            foreach (var item in registered)
            {
                Console.WriteLine($"    {item.Id}  「{item.Name}」  模板={item.Template?.GetType().Name}");
            }

            if (registered.Count <= before)
            {
                Console.WriteLine("结果：Initialize 未注册任何图标包");
                return 1;
            }

            Console.WriteLine("结果：加载流程通过");
            return 0;
        }
        catch (Exception ex)
        {
            Console.WriteLine("结果：加载失败");
            Console.WriteLine(ex.ToString());
            return 1;
        }
    }
}

/// <summary>与 ClassIsland.PluginLoadContext 行为一致的加载上下文。</summary>
internal sealed class ProbeLoadContext : AssemblyLoadContext
{
    private readonly AssemblyDependencyResolver _resolver;

    public ProbeLoadContext(string componentAssemblyPath)
        : base("ClassIsland.PluginLoadContext[probe]")
    {
        _resolver = new AssemblyDependencyResolver(componentAssemblyPath);
    }

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var path = _resolver.ResolveAssemblyToPath(assemblyName);
        return path is null ? null : LoadFromAssemblyPath(path);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        var path = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
        return path is null ? IntPtr.Zero : LoadUnmanagedDllFromPath(path);
    }
}
