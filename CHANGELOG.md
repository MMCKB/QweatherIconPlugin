# Changelog

本文件的格式基于 [Keep a Changelog](https://keepachangelog.com/zh-CN/1.1.0/)。
版本号严格遵循 ClassIsland 插件要求的 `a.b.c.d` 格式。

## [1.0.0.0] - 2026-10-14

### Added

- 初始版本：为 ClassIsland 提供两套和风天气图标包
  - `qweather.weatherIcons.line` —— 和风天气（线框）
  - `qweather.weatherIcons.fill` —— 和风天气（填充）
- 小米天气代码 → 和风图标编号的完整映射（40 条），其中 3 个小米独有代码
  （32 飑 / 33 龙卷风 / 34 弱高吹雪）使用和风预警类图标兜底
- 矢量几何内联渲染：图标以矢量路径数据内置于程序集，不依赖图标字体或外部图片
- 前景色继承：图标颜色跟随界面前景色，保留 ClassIsland 原有的「雨天变红」行为
- 16×16 点阵风格的插件图标 `icon.png`
- 双目标框架构建（`net8.0` / `net10.0`），对应 ClassIsland 两个 API 世代
- 开发与验证工具：
  - `tools/gen_icons.py` —— 从 SVG 抽取矢量路径生成 `QWeatherIconData.cs`
  - `tools/gen_icon.py` —— 生成点阵插件图标
  - `tools/pack.py` —— 打包 `.cipx`
  - `tools/make_preview.py` —— 生成图标预览图
  - `tests/IconRenderTest` —— 离屏渲染验证
  - `tests/LoadProbe` / `tests/LoadProbe204` —— 复刻插件加载流程的兼容性探测
- GitHub Actions 发布流程：打 `a.b.c.d` Tag 自动构建 net8 包并发布到 Release

### Verified

- 几何数据与源 SVG 逐字节比对：80/80 一致
- 路径解析：80/80 成功；映射 40/40 无缺图
- 前景色继承（红 / 蓝 / 绿）渲染通过
- 插件加载 + `Initialize()` 图标包注册，在 ClassIsland 2.0.4 / 2.1.0 / 2.1.1.1 三个宿主上均通过

### License

- 代码：GNU GPL v3.0
- 图标素材：和风天气图标（QWeather Icons），CC BY 4.0，署名见 `THIRD-PARTY-NOTICES.md`
