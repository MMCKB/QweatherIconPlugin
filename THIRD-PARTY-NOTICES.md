# 第三方组件与许可声明（Third-Party Notices）

本插件包含来自第三方的素材，其许可条款如下。分发本插件（包括打包为 `.cipx` 分发给他人）时，
请连同本文件与 `README.md` 一起保留。

---

## 1. 和风天气图标（QWeather Icons）

**本插件内置的全部天气图标图形素材来自和风天气图标项目。**

### 署名信息（CC BY 4.0 要求）

| 项目 | 内容 |
|---|---|
| **作品名称** | 和风天气图标（QWeather Icons） |
| **作者 / 权利人** | QWeather（和风天气），Copyright © QWeather |
| **来源** | https://icons.qweather.com ・ https://github.com/qwd/Icons |
| **许可** | 知识共享署名 4.0 国际（CC BY 4.0）<br>许可全文：https://creativecommons.org/licenses/by/4.0/legalcode.zh-hans<br>中文摘要：https://creativecommons.org/licenses/by/4.0/deed.zh |
| **本插件所做的修改** | 见下方「修改说明」 |
| **免责声明** | 该作品按「原样」提供，权利人不对本项目提供任何担保、背书或承诺，也不保证其毫无缺陷或必定修复缺陷。详见 https://www.qweather.com/terms/disclaimer |

### 修改说明

原始素材为 SVG 文件（`icons/<图标编号>.svg`）。本插件做了以下处理：

1. 从每个 SVG 中抽取全部 `<path>` 元素的 `d` 属性值，并拼接为单条复合路径数据；
2. 把该路径数据以字符串常量形式内联进程序集源码（`QWeatherIconData.cs`），
   运行时通过 Avalonia 的 `StreamGeometry.Parse` 解析为矢量几何并绘制。

**图形本身未做任何改动**——坐标、形状、路径数量均与原文件一致。
未改变图标颜色：原 SVG 使用 `fill="currentColor"`，本插件同样让图标颜色继承界面前景色。

原始 SVG 文件副本一并保留在仓库的 `Assets/Svg/` 目录中，便于核对。

### 关于「和风天气」的常见误解

- 本插件**只使用和风天气的图标素材，不调用和风天气的任何 API 服务**。
  插件展示的天气数据来自 ClassIsland 自身的数据源（小米天气）。
- 因此**无需**注册和风天气开发者账号、无需申请 API Key、**无需购买商用授权**。
  只需遵守上述图标许可（CC BY 4.0）中的署名义务即可。
- 和风天气图标项目官方说明：「和风天气图标和图标字体（QWeather Icons）是开源项目，
  **任何人都可以免费使用、修改或分发**。」

### 如何满足署名义务

CC BY 4.0 允许商用、允许修改、允许再分发，**唯一的核心义务是署名**。
法律条文 Section 3(a)(1) 要求保留/提供以下信息（本文件已全部覆盖）：

1. 创作者标识 —— QWeather（和风天气）
2. 版权声明 —— Copyright © QWeather
3. 指向本许可的声明 —— CC BY 4.0
4. 指向免责声明的声明 —— 见上表
5. 素材来源 URI —— https://icons.qweather.com
6. 是否做了修改的说明 —— 见「修改说明」
7. 明确标注素材采用 CC BY 4.0，并附许可全文链接

**实践上你只需要做一件事**：在分发插件时，保留本文件与 `README.md` 中的署名段落。
ClassIsland 会在「应用设置 → 插件」页面渲染插件的 `README.md`，因此署名对最终用户是可见的。

---

## 0. 本插件自身的许可

**本仓库的代码采用 [GNU GPL v3.0](LICENSE)。**

但请注意：**GPL-3.0 只覆盖本插件的程序代码**，不覆盖下文第 1 节中和风天气的图标素材
（后者为 CC BY 4.0）。`QWeatherIconData.cs` 中的图标矢量数据不属于 GPL-3.0 授权范围，
请按 CC BY 4.0 的要求保留署名。

> 许可兼容性说明：FSF 官方明确 **CC BY 4.0「compatible with all versions of the GNU GPL」**
> （见 https://www.gnu.org/licenses/license-list.html#ccby ），
> 因此 GPL-3.0 代码中嵌入 CC BY 4.0 素材是允许的，前提是完整保留 CC BY 的署名与许可声明
> （对应 GPLv3 §7 允许的「保留署名声明」附加条款）。

## 2. ClassIsland 插件 SDK

本插件基于 [ClassIsland](https://github.com/ClassIsland/ClassIsland) 插件 SDK 开发，
该 SDK 采用 **GNU Lesser General Public License v3.0（LGPL-3.0-only）**。

插件以动态加载方式引用 SDK，未修改其源码，也未将其静态链接进插件产物
（`ClassIsland.PluginSdk` 的 `runtime` 资产已通过 `ExcludeAssets` 排除，运行时由 ClassIsland 本体提供）。

GPL-3.0 与 LGPL-3.0 兼容：LGPL-3.0 §2 允许将合并后的作品按 GPL-3.0 条款发布。
