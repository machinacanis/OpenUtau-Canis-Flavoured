# OpenUtau（machinacanis fork）

本仓库是 **machinacanis 的个人 fork**：GitHub `machinacanis/OpenUtau`，也称 Canis Flavoured（狗味版）。

它**不是**官方项目 `openutau/OpenUtau`（历史上是 `stakira/OpenUtau`）。产品介绍仍见 `README.md`，但 README 里的 Discord / Issue / 下载徽章指向官方树，**不要**把本 fork 的问题、PR 或发行物当成官方的。

当前树跟踪上游架构，另有 Classic 可选 Renderer：`HIFIUTAU`（进程内 ONNX，无 HTTP）和 `CUSTOM_SERVER`（HTTP）。默认仍是 `WORLDLINE-R`。无 Studio 主题。

改动工程状态、渲染、音素或 UI 前先读对应章节。功能、修复、重构走 topic branch，见 Branches。

## Language

术语以代码标识符为准。用户用中文时映射到下列词，不要自造同义词。

**Project**：一份可编辑的歌声工程。磁盘格式是 YAML `.ustx`（当前 `ustxVersion` 0.9）。_Avoid_：song、score 当工程本体。

**Track**：工程里的一条声线。绑定一个 Singer 和一套 `URenderSettings`。

**Part**：轨道上的一段时间块。`UVoicePart` 含音符；`UWavePart` 含音频。_Avoid_：clip、region。

**Note**：钢琴卷帘上的音符。`position`/`duration` 以 tick 计（工程 `resolution` 固定 480），相对 Part 起点。`tone` 是 MIDI 音高（C4 = 60）。`lyric` 是歌词，可带 phonetic hint（`read[r iy d]`）。`+` / `+n` 是延长音符。_Avoid_：event、key。

**Phoneme**：Phonemizer 从 Note 组生成的音素，挂在 `UVoicePart.phonemes` 上，**不写入** `.ustx`。改歌词/音符后必须经 Validate 重生。_Avoid_：phone（代码里 `RenderPhone` 才用这个词）。

**Phonemizer**：把 Note 组变成 `Phoneme[]` 的插件。入口 `Phonemizer.Process`。内置实现在 `OpenUtau.Plugin.Builtin`；第三方 DLL 放 DataPath/`Plugins`。契约见 `OpenUtau.Core/Api/README.md`。

**Singer**：一套可唱的声音资源。类型 `USingerType`：Classic / Enunu / Vogen / DiffSinger / Voicevox。Classic 对应 Voicebank（`character.txt` + `oto.ini`）。_Avoid_：voicebank 当所有引擎的统称；Voicebank 只指 Classic。

**Oto**：Classic Voicebank 里一条采样切片（alias、offset、consonant、cutoff、preutter、overlap）。运行时包装为 `UOto`。

**Expression**：参数，按 `abbr` 注册在 Project 上（`dyn`、`pitd`、`vel`、`vol`、`clr`、`genc`、`xsy`…）。有 note 级和 curve 级。定义在 `OpenUtau.Core/Format/USTx.cs`。

**Renderer**：把 Phrase 合成 PCM 的引擎。Classic 歌手可选 `WORLDLINE-R`、`CLASSIC`、`HIFIUTAU`、`CUSTOM_SERVER`。默认是 `WORLDLINE-R`，仅当偏好 `DefaultRenderer == "Classic"` 时用 `CLASSIC`。`HIFIUTAU` 在同一进程内 ONNX 推理，不走 HTTP；`CUSTOM_SERVER` 把 Phrase JSON POST 到外部服务。两套实现互不引用。_Avoid_：engine 当 Renderer 的同义词。

**Phrase**：连续 Phoneme 组成的渲染单元 `RenderPhrase`。Renderer 只吃 Phrase，不直接吃 Note。

**Resampler / Wavtool**：仅 `CLASSIC` Renderer 使用的外部/内置工具链。Worldline 既是 native 库也是一种 Resampler。

**Command**：可撤销的工程变更。必须包在 `DocManager` 的 UndoGroup 里，经 `ExecuteCmd` 执行。

**Tick**：时间轴单位。与毫秒的换算走 `UProject.timeAxis`，不要手算 BPM。

**DataPath**：用户数据根。Windows 便携模式 = 程序目录；安装模式（存在 `installed.txt`）= `Documents/OpenUtau`；macOS = `~/Library/OpenUtau`；Linux = `$XDG_DATA_HOME/OpenUtau`。Singers、Plugins、prefs 都相对它。Cache 另有 `CachePath`。

## Layout

| 路径                       | 职责                                                              |
| -------------------------- | ----------------------------------------------------------------- |
| `OpenUtau.Core/`           | 领域模型、Command、渲染、格式、Phonemizer API                     |
| `OpenUtau.Core/Ustx/`      | Project / Track / Part / Note / Phoneme / Singer / Expression     |
| `OpenUtau.Core/Commands/`  | 全部 `UCommand`                                                   |
| `OpenUtau.Core/Render/`    | `IRenderer`、`RenderPhrase`、`RenderEngine`                       |
| `OpenUtau.Core/HiFiUtau/`  | 进程内 HiFiUTAU Renderer 与 ONNX 管线                             |
| `OpenUtau.Core/CustomRender/` | CUSTOM_SERVER HTTP Renderer                                    |
| `OpenUtau.Core/Classic/`   | Voicebank、oto、resampler、wavtool、UST                           |
| `OpenUtau.Core/Api/`       | 对外 Phonemizer 插件契约                                          |
| `OpenUtau.Core/Format/`    | `.ustx` 以及 UST / VSQX / MIDI / MusicXML / SVP / ufdata 导入     |
| `OpenUtau.Plugin.Builtin/` | 内置 Phonemizer                                                   |
| `OpenUtau/`                | Avalonia UI：Views / ViewModels / Controls / Colors / Strings     |
| `OpenUtau.Test/`           | xunit v3；音源夹具在 `Files/`                                     |
| `cpp/worldline/`           | Bazel 构建的 native WORLD 合成库，产物在 `runtimes/<rid>/native/` |
| `Misc/sync_strings.py`     | 从英文 `Strings.axaml` 同步其它语言文件                           |

命名空间：`OpenUtau.Core` / `OpenUtau.Core.Ustx` / `OpenUtau.Classic` / `OpenUtau.Api` / `OpenUtau.App`（UI）/ `OpenUtau.App.ViewModels`。

## Mutating the project

1. `DocManager.Inst.StartUndoGroup(...)`
2. `DocManager.Inst.ExecuteCmd(new SomeCommand(...))` — 批量用带 `List<>` 的 Command 重载
3. `DocManager.Inst.EndUndoGroup()`

`ExecuteCmd` 必须在 UI 主线程。Command 在 `lock (Project)` 下 `Execute()`，然后 `Validate`，再 `Publish` 给 `ICmdSubscriber`（多数 ViewModel）。没有 UndoGroup 的 Command 会被丢掉。

`UNotification` 不是撤销项：加载/保存/播放头/校验走通知。

Phoneme 是派生状态。改 Note 后让 Validate/`PhonemizerRunner` 重生，不要手写 `part.phonemes` 当源数据。

批处理宏同样走 UndoGroup，见 `OpenUtau.Core/Editing/README.md`。

## Render

`UTrack.RendererSettings.Renderer` 实现 `IRenderer`：`Layout` 估时值，`Render` 出 `float[] samples`（假定 44100）。Classic 歌手选项由 `Renderers.GetSupportedRenderers` 给出。

Worldline native 变更走 `cpp/` + Bazelisk，不要手改 `runtimes/` 里的 `.so/.dll/.dylib` 当源。构建说明在 `cpp/README.md`。

## UI

Avalonia 12 + ReactiveUI。ViewModel 继承 `ViewModelBase`（`ReactiveObject`）。需要工程事件则实现 `ICmdSubscriber`。`ViewLocator` 把 `FooViewModel` 映射到 `FooView`。

可见字符串：XAML 用资源 key；C# 用 `ThemeManager.GetString("section.key")`。英文源是 `OpenUtau/Strings/Strings.axaml`。改完跑 `python Misc/sync_strings.py`。不要硬编码 UI 文案。

主题：`OpenUtau/Colors/`。颜色走 `ThemeManager` 刷子 + `{DynamicResource ...}`。

**Studio UI**：偏好外观里的 `UseStudioUI`（默认关）。关掉时编辑器、布局、钢琴卷帘绘制保持经典 OpenUtau。打开后才加载 Studio chrome（`Styles/StudioStyles.axaml`）、Studio/WarmSage 内置主题，以及钢琴卷帘波形/音符/歌词外观配置。

- 共享：`NotesViewModel`、Command、hit testing、`NoteEditStates` 仍是唯一编辑逻辑。
- Studio 专属：`OpenUtau/Studio/`。以后只在 Studio 里出现的功能或布局改动放这里，不要在 ViewModel 里散落 `if (UseStudioUI)`。
- 绘制：`NotesCanvas` / `WaveformImage` 读 `ThemeManager`；`ApplyPianoRollStyle` 在开关关闭时回到经典刷子。
- 轨道头：默认高度取 `StudioTrackLayout.DefaultTrackHeight`（Studio 开 63 = 105 − 2×21，关 105）；头像左侧彩色条宽度取 `StudioTrackLayout.ColorBarWidth`（0 / 6）。
- 按钮皮肤：Studio One 风格按钮 = `Styles/StudioOneControls.axaml` 的 `.s1` 类（`mute` / `solo` 角色，`fxOn` 表示非开关按钮的开启态）。新按钮加类名即可复用，不要在控件里另写 chrome 样式；该文件只由 `StudioStyles.axaml` 在 Studio UI 开启时载入。
- 样式优先级：控件自身 `Styles` 高于应用级 `Styles`（`TrackHeader.axaml` 的 `ToggleButton{Background=Transparent}` 就是这样压掉 `Styles.axaml` 的）。所以 Studio 的按钮 chrome 必须放在模板内部（`StudioOneControls.axaml`），而彩色条宽度这类必须走绑定——XAML 局部值高于任何样式 setter。

`OpenUtau.csproj` 开了 `TreatWarningsAsErrors`。

## Build and test

```bash
dotnet restore OpenUtau
dotnet test OpenUtau.Test
dotnet publish OpenUtau -c Release -r linux-x64 --self-contained true
```

目标框架 `net10.0`（Windows 上 UI/测试项目为 `net10.0-windows`）。CI：`.github/workflows/pr-test.yml`（win/mac/linux）。

Phonemizer 测试继承 `OpenUtau.Plugins.PhonemizerTestBase`，夹具是 `OpenUtau.Test/Files/<singer>/character.txt`。

Native：`cd cpp && bazelisk build //worldline`。

## Style

`.editorconfig` 说了算：C# 4 空格、UTF-8 BOM、K&R 花括号（`csharp_new_line_before_open_brace = none`）、行宽指引 100。Nullable 在 Core/Plugin/UI 开启。匹配周围文件，不要把一块改成 Allman。

日志用 Serilog。用户能理解的失败用通知/对话框，不要把栈甩到 UI。

## Fork rules

- **身份**：对外称呼本仓库为 machinacanis 的 OpenUtau fork。官方仓库、官方 Discord、官方 Issue 只用于上游本身存在的问题。
- **Remote**：`origin` = `machinacanis/OpenUtau`。上游是 `openutau/OpenUtau`，需要时加 `upstream` 远程再 fetch/merge。合并上游后先跑测试。
- **上游可合并**：能回馈上游的修放在与上游相同的结构里。本 fork 自有的面单独落点。topic 的 base 与隔离见 Branches。
- **不提交**：个人 `prefs.json`、音源、`bin/`/`obj/`、调试残留。`runtimes/` 里的 worldline 二进制只在正式重建后更新。
- **许可**：MIT（Copyright StAkira）。改文件时保留原版权头。

## Merge logging（上游合并记录）

与上游 `openutau/OpenUtau` 同步并尝试合并冲突代码时，合并结束后必须在仓库根目录 `MERGE_LOG.md` 追加一条记录（无冲突的同步也记录，标注"无冲突"）。这是本 fork 的强制共识，AI agent 与人工合并一视同仁，目的：保证合并行为可溯源。

1. **记录时机**：合并结束（含验证完成）后立即写，随该次合并提交一起提交。
2. **必写内容**：
   - 日期时间（ISO 8601，UTC）与合并的上游基线（完整 SHA + 一句话主题）；
   - 本次引入的上游 commit 列表；
   - 冲突来源：每个冲突文件分别来自哪两个 commit（fork 侧 / 上游侧，完整 SHA + 主题）；
   - 决策记录：逐条说明每个冲突做了什么决策、为什么（保留哪边 / 合并哪边 / 如何组合），以及验证方式与结果（构建、测试、`Misc/sync_strings.py` 同步结果）。
3. **可读性**：每条记录是独立的 `## Merge YYYY-MM-DD <上游SHA前8位>` 块，块间用 `---` 分隔，按时间顺序追加在文件末尾，不改写历史记录。

## Branches

`master` 是 integration branch（不是 `main`），只接收已完成的 topic 合并。产品改动在 topic branch 上做：一个主题一条分支。流程是 GitHub Flow，不是 Git Flow：没有 `develop` / `release/*`。

1. 改产品代码前先看当前分支。若已在 `master`，先建 topic 再改。
2. 命名遵守 Conventional Branch：`feature/<topic>`、`fix/<topic>`、`chore/<topic>`。全小写 kebab-case。重构走 `chore/`。
3. 按最终合入目标选 base。只留在本 fork：从 `origin/master` 拉。打算回馈上游：从 `upstream/master` 拉（或 rebase 上去），不要从本 fork 已分叉的 `master` 或其它 topic 起。
4. 一个 topic 一个逻辑改动，可独立审查、revert、cherry-pick。不为风格或顺手重构改上游文件。topic 保持单一；只在合不干净或上游 API 变了时才把 `master` 合进来。
5. 该任务的提交都落在该 topic；完成（测试可通过、主题完整）后再合进 `master`。
