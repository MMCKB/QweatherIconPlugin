using Avalonia.Controls.Templates;
using ClassIsland.Core.Abstractions;
using ClassIsland.Core.Abstractions.Services;
using ClassIsland.Core.Attributes;
using ClassIsland.Core.Extensions.Registry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace QWeatherIconsPlugin;

/// <summary>
/// 和风天气图标插件入口。
/// </summary>
[PluginEntrance]
public class Plugin : PluginBase
{
    public override void Initialize(HostBuilderContext context, IServiceCollection services)
    {
        // 注册后会自动出现在「应用设置 → 天气 → 天气图标包」下拉框中，用户可随时切换。
        // 必须在主机启动前完成注册：WeatherService 在构造时读取一次图标模板注册表。
        RegisterIconPack(
            services,
            WeatherIconTemplates.LineIconPackId,
            "和风天气（线框）",
            WeatherIconTemplates.CreateLineTemplate());

        RegisterIconPack(
            services,
            WeatherIconTemplates.FillIconPackId,
            "和风天气（填充）",
            WeatherIconTemplates.CreateFillTemplate());
    }

    private static void RegisterIconPack(
        IServiceCollection services, string id, string name, IDataTemplate template)
    {
        // IWeatherService.RegisteredTemplates 是静态集合且没有反注册 API，
        // 这里做一次去重，避免插件被重复加载时出现重复项。
        if (IWeatherService.RegisteredTemplates.Any(x => x.Id == id))
        {
            return;
        }

        services.AddWeatherIconTemplate(id, name, template);
    }
}
