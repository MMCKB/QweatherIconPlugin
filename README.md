# 和风天气图标 · ClassIsland 插件

![Vibe Coding](https://img.shields.io/badge/Vibe%20Coding-100%25-8B5CF6?style=for-the-badge&logo=githubcopilot&logoColor=white)
![License](https://img.shields.io/badge/License-GPLv3-2D7D46?style=for-the-badge)
![ClassIsland](https://img.shields.io/badge/ClassIsland-2.0%2B-4A90E2?style=for-the-badge)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey?style=for-the-badge)

用 [和风天气图标（QWeather Icons）](https://icons.qweather.com) 替换 ClassIsland 的天气图标。

---

> [!IMPORTANT]
> **这是一个纯 Vibe Coding 项目 —— 100% 由 AI 编写，人类没有逐行审阅过代码。**
>
> - 从可行性评估、天气代码映射表、插件实现、离屏渲染验证、加载兼容性探测，到 CI 脚本、
>   分发合规文档，全部由 AI 生成，作者没有逐行 review，也没有做人工设计稿。
> - 它经过了不少自动化验证（几何数据逐字节比对、`Initialize()` 真实注册测试、
>   三个 ClassIsland 宿主版本的加载探测），**但「验证过」不等于「设计过」**。
> - 请以「能跑就行」的心态使用它。如果它帮到你了，很好；如果它炸了，也别太意外。
> - 欢迎提 issue 和 PR —— 包括让 AI 继续写。🙏

---

## 效果预览

![和风天气图标预览](https://raw.githubusercontent.com/MMCKB/QweatherIconPlugin/main/docs/preview.png)

安装后进入 **应用设置 → 天气 → 天气图标包**，可切换：

- **和风天气（线框）**
- **和风天气（填充）**

随时可以切回内置的 Lucide / Fluent Design / SF Symbols / 纯文本。

图标颜色跟随界面前景色，因此雨天仍会按 ClassIsland 的原有行为变为红色。

## 安装

**直接装 `QWeatherIconsPlugin-1.0.0.0-net8.cipx` 即可**（在 [Releases](../../releases) 下载）。

该包经实测覆盖 **ClassIsland 2.0.0 ~ 2.1.1+**：在 2.0.4、2.1.0、2.1.1.1 三个宿主上，
插件加载、入口类发现、**图标包注册**、图标渲染均已验证通过。

| 包 | 说明 |
|---|---|
| `...-net8.cipx` | **推荐**。编译基线为 ClassIsland 2.0.0（.NET 8 + Avalonia 11.3.6），靠 .NET 的向上版本绑定覆盖 2.0.0 ~ 2.1.1+ |
| `...-net10.cipx` | 备选。编译基线为 ClassIsland 2.1.1（.NET 10 + Avalonia 12.1.0），**仅适用于 ClassIsland 2.1.1 及以上** |

**安装步骤**：在 ClassIsland 中打开 **应用设置 → 插件**，把 `.cipx` 文件拖入插件页面，然后**重启 ClassIsland**。

## 生效范围

主界面「天气简报」组件、天气设置页预览、天气预报提醒、小时级天气预报提醒、迷你信息提供方。

气象预警图标（由天气服务下发的网络图片）与 AQI 图标不受本插件影响。

## 天气代码映射

ClassIsland 的天气数据来自小米天气，其天气代码与和风天气不同，因此插件内置了一张映射表
（小米代码 → 和风图标编号），共 40 条。其中 3 个小米独有的天气代码在和风「天气现象」图标中
没有对应项，使用和风预警类图标兜底：

| 小米代码 | 天气 | 和风图标 |
|:---:|---|---|
| 32 | 飑 | 2201 飑 |
| 33 | 龙卷风 | 1002 龙卷风 |
| 34 | 弱高吹雪 | 2216 吹雪 |

未命中映射的天气代码会回退到和风 `999`（未知）。

---

## 版权与许可

> ⚠️ **本仓库是「代码 + 第三方素材」的混合许可，请勿混为一谈。**

### 本插件代码

**GNU GPL v3.0**，详见 [LICENSE](LICENSE)。

### 天气图标素材

**本插件内置的天气图标来自 [和风天气图标（QWeather Icons）](https://icons.qweather.com)。**

> 和风天气图标（QWeather Icons）
> 版权所有 © [QWeather](https://www.qweather.com/)（和风天气）
> 来源：https://icons.qweather.com ・ https://github.com/qwd/Icons
> 采用 [知识共享署名 4.0 国际（CC BY 4.0）](https://creativecommons.org/licenses/by/4.0/deed.zh) 许可。
>
> 本插件对原素材做了如下处理：抽取 SVG 中 `<path>` 的路径数据并拼接为单条复合路径，
> 以字符串常量内联进 `QWeatherIconData.cs`，运行时解析为矢量几何绘制。
> **图形本身未做任何改动。**
>
> 该作品按「原样」提供，权利人不对本项目提供任何担保、背书或承诺。

完整声明见 [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md)。

**要点：**

- GPL-3.0 **只覆盖代码**，不覆盖图标素材；图标数据仍为 CC BY 4.0
- FSF 官方明确 CC BY 4.0 **兼容所有版本的 GNU GPL**，两者可以共存，前提是保留 CC BY 署名
- 本插件只使用和风的**图标素材**，不调用和风的 API 服务（天气数据来自 ClassIsland 自身的数据源），
  因此**无需**注册和风开发者账号、申请 API Key 或购买商用授权
- CC BY 4.0 允许商用与再分发，**唯一的义务是保留署名**
- 分发 `.cipx` 时请**不要**删掉包内的 `LICENSE` 与 `THIRD-PARTY-NOTICES.md`

---

## 构建

```bash
dotnet build QWeatherIconsPlugin.csproj -c Release
python tools/pack.py                  # 输出到 ./cipx/
```

| 目标框架 | 引用的 SDK | 依赖 | 清单 |
|---|---|---|---|
| `net8.0` | `ClassIsland.PluginSdk 2.0.0.1` | .NET 8 + Avalonia 11.3.6 | `manifest.legacy.yml`（apiVersion 2.0.0.0） |
| `net10.0` | `ClassIsland.PluginSdk 2.1.1` | .NET 10 + Avalonia 12.1.0 | `manifest.yml`（apiVersion 2.1.1.0） |

> **为什么按最低版本编译？**
> .NET 的程序集绑定只允许「引用低版本、运行在高版本」，反向会抛
> `FileNotFoundException: Could not load file or assembly 'ClassIsland.Core, Version=x'`。
> 所以两个包都按各自 API 世代的最低支持版本编译，靠向上 roll-forward 覆盖整段版本区间。

### 发布

推送符合 `a.b.c.d` 格式的 Tag 即触发 CI，自动构建 net8 包并发布到 Release：

```bash
git tag 1.0.0.0
git push origin 1.0.0.0
```

> Tag **必须严格是 `a.b.c.d`**，不能加 `v` 前缀，否则 ClassIsland 的索引生成器无法识别版本。

## 开发说明

- `tools/gen_icons.py`：从 `Assets/Svg/*.svg` 抽取矢量路径，生成 `QWeatherIconData.cs`
- `tools/gen_icon.py`：生成 16×16 点阵风格的插件图标 `icon.png`
- `tools/pack.py`：打包为 `.cipx`
- `tools/make_preview.py`：生成带天气名称的图标预览图
- `tests/IconRenderTest`：离屏渲染验证（几何解析、映射完整性、前景色继承）
- `tests/LoadProbe` / `tests/LoadProbe204`：复刻 ClassIsland 的插件加载流程做加载验证

## 上架插件市场

如需上架 ClassIsland 内置插件市场，请见 [docs/plugin-index.yml](docs/plugin-index.yml)，
把该文件放到 `ClassIsland/PluginIndex` 仓库的 `index/plugins-v2/` 目录并提 PR。

> 注意：市场一个插件只支持**一个**包，建议用覆盖面最广的 `-net8` 包。
