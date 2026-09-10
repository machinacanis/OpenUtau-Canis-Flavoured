
---

## Merge 2026-09-06 56eafb70

- **时间**（UTC）：`2026-09-06T10:13:34Z`（验证完成时间）
- **合并方式**：`git merge --no-commit --no-ff upstream/master`（merge-base `b863f7be012a6b4dc23063023529b186d74846e2`，即上次合并记录的上游基线；合并范围 `b863f7be..eaaf2e88`）
- **上游基线**：`eaaf2e88a0bed2c7d4f64b24a24f585eba924301` — Piano roll: keep the real curve's fill anchored to the canvas bottom (#2371)

### 引入的上游 commit（11 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `eaaf2e88a0bed2c7d4f64b24a24f585eba924301` | Piano roll: keep the real curve's fill anchored to the canvas bottom (#2371) |
| 2 | `56eafb7060443cddcd9d7da240be2dd92c216713` | Output 0 instead of blank in Oto (#2372) |
| 3 | `57567b5b4baebffa6eca028db264ba9d3e21a6f0` | fix legacy validation (#2370) |
| 4 | `549794a5b22a3351548c1c066c134c7d61143ba7` | auto-merge: label PR with auto-merge when posting the rules comment |
| 5 | `ea6769483b33486f3c19c96b9dbeeb18502ad829` | EN2JA+ merge to En to Ja (#2364) |
| 6 | `23779f5dfd9d2372522ee5b58ea529b9bb110803` | Add a metronome (#2341) |
| 7 | `84344cbdecbb8a1bdc9da58fd2f87d55587242e7` | Add the DAW integration API (OpenUtau.Core/DawIntegration) (#2369) |
| 8 | `fe42e4438a56e68094dffd2451a2c849109fc7a5` | Enable daily automatic alpha builds |
| 9 | `069520c0b573b5863c36d355b455577a56dc4c30` | ci: grant contents:write permission to release-cleanup job |
| 10 | `3f213e8993ca792c3e6f8958c92ab27eae78eac5` | ci: grant contents:write permission to build job for release creation |
| 11 | `0d752259fed1e236ea471538fc2a5bbea928edcc` | 1 (#2368) |

### 冲突

本次 21 个文件两侧均有改动，其中 7 个出现冲突标记，其余由 ort 自动合并（双侧改动互不重叠）。冲突文件及来源：

| 文件 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- |
| `OpenUtau.Core/DawIntegration/DawManager.cs`、`DawMessages.cs`、`DawServerFinder.cs`、`DawTransport.cs`（`DawAudio.cs` 两侧内容一致，自动合并） | `feature/daw-integration`（Kakaru 提交 cherry-pick：`5cb523bd` v1.2 等 16 个 + fork 适配） | `84344cbd` #2369 |
| `OpenUtau.Test/Core/DawIntegration/DawRealPluginTest.cs`、`DawTransportTest.cs`（其余 6 个测试文件两侧内容一致，自动合并） | 同上（fork 适配：xUnit v3） | `84344cbd` #2369 |
| `OpenUtau/Controls/WaveformImage.cs` | `f22f3a13` Studio 波形重写（TrackHeight/TrackOffset）+ `59f48d53`（fork 明确不含 phrase bounds） | `ea676948` #2364（顺带加入 phrase bounds 绘制） |

### 决策记录

- **DAW 集成（核心 + 测试 + 契约文档）按用户指示以上游为基准**：5 个核心文件与 8 个测试文件全部取上游 `84344cbd`（`git checkout --theirs`），与 `upstream/master` 逐字节一致；fork 侧 Kakaru 版实现被替换。上游 xunit.v3 与 fork 同为 3.2.2，测试无需再适配。
- **契约文档**：删除 fork 自带的 `PROTOCOL.md`（309 行）与 `DEVELOPMENT_PLAN.md`（140 行）——上游只发布 `API.md`（RFC 2119 规格，v1.2），fork 版协议文档已被取代；上游源码注释中的 PROTOCOL.md 引用为其自身陈旧引用，保持原样不动。
- **fork DAW UI 保留并适配**：`DawIntegrationViewModel.cs` / `DawIntegrationDialog.axaml(.cs)` / Tools 菜单项 / `dawintegration.*` 字符串为 fork 自有（上游无 UI），全部保留；上游 DawManager 公开 API（`StateChanged(Action<DawConnectionState>)`、`ConnectionsChanged`、`ConnectionLost(Action<string>)`、`ConnectAsync(DawServer)`、`DisconnectAsync(int)`、`Connections: IReadOnlyList<DawConnectionInfo>`、`ServerName`）与 fork VM 既有引用一一对应，编译零改动；仅将 VM 注释中的 `PROTOCOL.md §4` 改为 `API.md §4`。
- **fork Studio 波形特性保留**：`WaveformImage.cs` 冲突取 fork 侧（`--ours`）；上游 #2364 附带的 phrase-bounds 绘制不采纳——fork 波形实现为自研 Studio 外观，且 fork 在 `59f48d53` 已明确选择不含 phrase bounds，该改动与 DAW 集成无关。
- **自动合并的其余文件两侧内容都保留**：`Preferences.cs`（fork Studio/波形/HiFiUTAU 偏好 + 上游节拍器偏好）、`PianoRoll.axaml.cs`（fork 面板拖拽 + 上游 #2368 删除调试日志）、`Strings.axaml`（fork `dawintegration.*` + 上游 metronome 键）、`PreferencesViewModel.cs` / `PreferencesDialog.axaml(.cs)`（fork Studio 外观设置 + 上游节拍器设置）、`MainWindow.axaml`（fork DAW 菜单项 + 上游节拍器按钮）。
- **字符串同步**：`python Misc/sync_strings.py` 运行一次——英文 `Strings.axaml` 键序归一（`metronome.volume` 按字母序归位），19 个语言文件以注释占位补入 4 个节拍器键（未翻译惯例）。
- **Non-DAW 上游改动全部采纳**：节拍器（#2341，`PlaybackManager` / `MetronomeEngine` / `MetronomeScheduler` / 偏好 / UI）、EN2JA+（#2364）、Oto 0 输出（#2372）、legacy 校验（#2370）、CI/auto-merge（4 个）、#2368 清理、#2371 曲线填充锚定（`ExpressionCanvas.cs`，与 fork 波形无交集）。

### 已验证

- `dotnet build OpenUtau -c Debug`：0 错误（存量警告：Enunu CS0649/CS0414、AVLN3001 等，与本次合并无关）。
- `dotnet test OpenUtau.Test`：**401 通过 / 0 失败 / 1 跳过**（跳过的为 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`，需真实 DAW 插件，与上游行为一致）。
- `python Misc/sync_strings.py`：已执行（见决策记录）。
- `git diff --check`：无冲突标记。上游文件自带的尾随空格（`ENtoJAPhonemizer.cs`/`en2ja.template.yaml`/`fil2ja.template.yaml`/`Resources.Designer.cs`）按本 fork 一贯约定不改动；`OpenUtau.Core/DawIntegration/` 与 `OpenUtau.Test/Core/DawIntegration/` 与 `upstream/master` 逐字节一致。

---

## Merge 2026-09-07 2b03ad56

- **时间**（UTC）：`2026-09-07T06:23:42Z`（合并提交 `11f98b1a` 完成时间）
- **合并方式**：`git merge --no-ff --no-commit upstream/master`（merge-base `eaaf2e88`，即上次合并记录的上游基线；合并范围 `eaaf2e88..2b03ad56`）
- **上游基线**：`2b03ad562fa6ee2937fdcfe24e790ad92c58064c` — Revert piano roll phrase bounds from WaveformImage

### 引入的上游 commit（2 个，线性）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `f590e0830414714543fd69ab3ec3d615b0702f40` | ci: gate release publication on all build legs succeeding |
| 2 | `2b03ad562fa6ee2937fdcfe24e790ad92c58064c` | Revert piano roll phrase bounds from WaveformImage |

### 冲突

仅 1 个文件有冲突标记（3 处）；其余 1 个文件（`build.yml`）由 ort 自动合并。冲突文件及来源：

| 文件 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- |
| `OpenUtau/Controls/WaveformImage.cs`（3 处冲突：常量区 / drawWidth 逐列绘制区 / DrawPeak 签名） | fork 自研波形实现：`f22f3a13` Studio 波形重写 + `bb832af7` 无音频留白 + `59f48d53`（明确不含 phrase bounds） | `ea676948` #2364（引入 phrase bounds 绘制，上游 `2b03ad56` 自身 revert 撤销） |

### 决策记录

- **`WaveformImage.cs` 全部 3 处冲突取 fork 侧（`--ours`）**：上游 `2b03ad56` 只是 revert 掉上游 `ea676948` #2364 顺带引入的 phrase-bounds 背景/边框绘制，并恢复旧 `DrawPeak`；这两段代码 fork 从未采纳（fork 波形是自研重写，`59f48d53`/`ff48a513` 已明确选择不含 phrase bounds）。冲突本质是"上游删一段 fork 没有的代码"与"fork 自研实现占住相同行号区域"的重叠。保留 fork 侧后，语义与上游 revert 意图一致——fork 波形同样不画 phrase bounds（已 grep 验证无 `WaveformBorderBrush`/`DrawGeometry`/bounds 绘制代码）。
- **`build.yml` 自动合并，全部采纳**：上游 `f590e083` 重构发布流程（release job 门控 + artifact 常传 + alpha 标题保留 4 段版本号）。fork 从未改过此文件，合并零冲突。
- **字符串同步**：本次未触碰任何 `Strings.*.axaml`，无需运行 `Misc/sync_strings.py`。

### 已验证

- `dotnet build OpenUtau -c Debug`：0 错误（1735 个警告为存量：Enunu CS0649/CS0414、AVLN3001 等，与上次合并记录一致）。
- `dotnet test OpenUtau.Test`：**351 通过 / 60 失败 / 1 跳过**——与合并前 fork master `0afa4739` 的基线完全一致（在干净 master 上复跑全量测试得到相同 60 失败）。60 个失败（集中于 `EnToJaTest`、`PluginRunnerTest`、完整套件时序下的 `StringsTest` 等）为 fork master 存量问题，与本次合并无关：本次合并对 C# 代码零改动，仅 `build.yml`。`AppTest`（StringsTest）单独运行通过，失败仅在完整套件并行/时序下出现，属另一待查议题，不在本次合并范围。
- `git diff --check`：无冲突标记残留。

---

## Merge 2026-09-08 7c68a087

- **时间**（UTC）：`2026-09-08T05:37:18Z`
- **合并方式**：`git merge --no-ff --no-commit upstream/master`（merge-base `bfb01058821a01e0f59b8196e42eb34390f9acae`，即 HEAD `84c90c22` 的第二父提交；合并范围 `bfb01058..7c68a087`）
- **上游基线**：`7c68a0876052f1e3befe3cc01674e8853f9cd75a` — render: precomputed phrase layout and a coalesced projection read seam

### 引入的上游 commit（3 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `6196917f599a3e2fabc4ca0ed3f3f77fb8c55e22` | audio: replace WaveSource transport with frozen slot planner |
| 2 | `f773f377e15fa12e4aabf8276301d515e38b1539` | render: document-driven waveform reads and render-pass safety fixes |
| 3 | `7c68a0876052f1e3befe3cc01674e8853f9cd75a` | render: precomputed phrase layout and a coalesced projection read seam |

补注：同批上游历史里的 `994d55a1800a9cda91991b52ee8ab444bf5a3cd2`（fix c+v #2375）与 `bfb01058821a01e0f59b8196e42eb34390f9acae`（Fix Memory Leaks #2299）已由合并提交 `84c90c22`（Merge branch 'openutau:master' into master）先行引入，故本次不重复列出；`84c90c22` 当时未写 MERGE_LOG，属历史缺口，此处补注不改写旧记录。

### 冲突

仅 2 个文件出现冲突标记；其余 40 个文件自动合并（21 M / 16 A / 3 D）。冲突文件及来源：

| 文件 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- |
| `OpenUtau.Core/Render/RenderPhrase.cs`（1 处） | `fa88ca2c7684bd7bafd774fc8ca7916f07e788eb`（HiFiUTAU/Custom Server note-level 字段：phonemeType/stretchMode/spliceMode/stretchMs + readonly `oto`，经 `84c90c22` 汇入 master） | `f773f377e15fa12e4aabf8276301d515e38b1539`（`oto` 改 `{ get; private set; }` 以支持新增 `WithOto()`；`7c68a087` 继续改 RenderPhrase） |
| `OpenUtau/Controls/WaveformImage.cs`（5 处） | `f22f3a13ff9611b4575edb7440492268510972ff`（Studio 波形重写）+ `bb832af75092d213fb4c4d28d4a3d46a3df601ee`（无音频留白）等 fork Studio 波形实现 | `f773f377e15fa12e4aabf8276301d515e38b1539` + `7c68a0876052f1e3befe3cc01674e8853f9cd75a`（RenderView/MixPlanner 数据管线重写） |

另有非文本冲突的编译适配 1 处：`OpenUtau/ViewModels/NotesViewModel.cs`——上游 `7c68a087` 删除了 `WaveformRefreshEvent` 类与旧的 `PartRenderedNotification → MessageBus` 通知路径；fork 的 `PreferencesViewModel`/`WaveformImage` 仍以 MessageBus 使用该类，故在 fork UI 代码处恢复该类定义（其余上游改动如 `MixPlanner.EvictPart` 全部采纳）。

### 决策记录

- **`RenderPhrase.cs`：合并两侧**。保留 fork 4 个 HiFi/Custom Server note-level 字段；`oto` 采纳上游 `public UOto oto { get; private set; }`（合并后同文件内并入上游 `WithOto`/`PhraseLayout` 用法，需允许类内克隆赋值）。
- **`WaveformImage.cs`：数据层取上游、绘制层取 fork（手动重组合并 5 处冲突）**：
  - H1 字段区：保留 fork 的 `refreshTimer`/`mixUnlockTime`/`wasRendering` 等字段。
  - H2 构造函数：新增 `RenderView.Inst.Observe(_ => InvalidateVisual())` 作为渲染完成刷新源，同时保留 fork 50ms `DispatcherTimer` + `MessageBus`（主题 / Studio UI / 偏好）订阅。
  - H3 样本填充：删除对已删 API（`IsWaveformBlanked`/`LiveWaveformCache`/`part.Mix`）的引用，改为上游 `RenderView.Current(part)` → `MixPlanner.TryGetPartPlacements` → `SampleSlot[]` → `SlotMixSource.Mix`；保留 `snapEase`（播放停止回位）与 `needsAnotherFrame` 重绘。
  - H4 列统计：保留 fork 写入 `colMin/colMax`（供后续 Studio 布局绘制），不采用上游立即 `DrawPeak` 的极简画法。
  - H5 布局绘制：整段保留 fork 的 `DrawClassicStrip`/`DrawFixed`/`DrawFollowSmart`/逐 note 绘制与主题色/alpha/淡入淡出。
  - 效果：上游极简经典波形外观不采纳（fork Studio 波形特性保留）；因 `LiveWaveformCache` 已删，fork 原 300ms 渐显动画移除，波形改为随 MixPlanner 发布逐段出现。
- **`NotesViewModel.cs`**：恢复 `WaveformRefreshEvent` 类（见冲突表下说明），其余采纳上游（`ClearPhraseCache` 改用 `MixPlanner.EvictPart` 等）。
- **自动合并的其余文件全部采纳**：音频传输/渲染重构（MixPlanner/SampleSlot/Frozen/ThreadGuard/WaveformRefresh/RenderView/RenderProjection/PhraseLayout 等）、删除 RenderCache/WaveSource 及旧测试、DAW 测试适配、上游新增测试全部保留。
- **字符串同步**：本次未触碰任何 `Strings.*.axaml`，无需运行 `Misc/sync_strings.py`。

### 已验证

- `dotnet build OpenUtau -c Debug`：0 错误（存量 warning 与历次记录一致）。
- `dotnet test OpenUtau.Test`：**413 通过 / 4 失败 / 1 跳过**。4 个失败均为存量/环境问题，与本次合并无关：3 个 `PluginRunnerTest.ExecuteTest` 因沙箱 `~/.cache/OpenUtau` 只读无法写 `temp.tmp`（单独复跑同样失败）；1 个 `StringsTest` 仅在完整套件并行/时序下触发 Avalonia 线程问题（单独运行通过，与 2b03ad56 合并记录中的已知现象一致）。上游新增测试（RenderViewTest、MixPlanTest、WaveformRefreshTest、FrozenTest、XsyVariantTest 等）全部通过；跳过项为 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`（需真实 DAW 插件，与上游一致）。
- `git diff --check`：无冲突标记残留。

---

## Merge 2026-09-09 17bf25e7

- **时间**（UTC）：`2026-09-09T17:03:45Z`（验证完成时间）
- **合并方式**：`git merge --no-ff --no-commit upstream/master`（merge-base `7c68a0876052f1e3befe3cc01674e8853f9cd75a`，即上次合并记录的上游基线；合并范围 `7c68a087..17bf25e7`）
- **上游基线**：`17bf25e7f78c5f88a6c5bce437bf6d592f012e46` — render: run phrase build off the UI thread from immutable snapshots

### 引入的上游 commit（1 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `17bf25e7f78c5f88a6c5bce437bf6d592f012e46` | render: run phrase build off the UI thread from immutable snapshots |

合并前先把本地 `master` 快进到 `origin/master`（`3220b3a8..3a31b1d6`，10 个 fork commit，含 PR #9/#10）。

### 冲突

仅 1 个文件出现冲突标记；其余 16 个文件自动合并（6 A / 10 M）。冲突文件及来源：

| 文件 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- |
| `OpenUtau.Core/Render/RenderPhrase.cs`（1 处，`RenderPhone` 构造函数体） | `fa88ca2c7684bd7bafd774fc8ca7916f07e788eb`（HiFiUTAU / Custom Server note-level 字段 `phonemeType`/`stretchMode`/`spliceMode`/`stretchMs` 的取值与 `Hash()` 写入；同 commit 的 `lowcut`/`warmth`/`hcmp`/`breathLow`/`breathHigh` 曲线数组） | `17bf25e7`（`RenderPhone`/`RenderPhrase` 改吃 `Pipeline.PhraseSource` 不可变快照，表达式解析上移到快照层） |

### 决策记录

- **`RenderPhrase.cs`：构造体取上游、fork 的 4 个字段改为读快照**。冲突段（旧 `UProject`/`UTrack`/`UPhoneme` 取值逻辑）整体丢弃，采纳上游 `oto = phoneme.Oto; oto2 = phoneme.Oto2;`；fork 的 4 个字段改由 `PhonemeSource` 提供（`phonemeType = phoneme.PhonemeType;` 等）。`Hash()` 中 4 个字段的写入与 `Hash(true)` 中 5 个曲线数组保持 fork 原样，位置不变。
- **`OpenUtau.Core/Pipeline/PhraseSource.cs`（上游新文件）：就地加 fork 字段**。在 `PhonemeSource` 上新增 `PhonemeType`/`StretchMode`/`SpliceMode`/`StretchMs`，在构造函数里按上游既有的“快照时解析表达式”模式（同 `ENG`/`MODP` 写法）用 `TryGetExpDescriptor` 取值，descriptor 缺失即 0 —— 与 fork 快照前 `RenderPhone` 的行为逐字一致。这样 fork 的字段落在上游同构结构里，未来可回馈。
- **`OpenUtau.Core/Render/Renderers.cs`：`GetOrCreate` 对 `CUSTOM_SERVER` 绕开实例缓存**。上游新增“按 renderer id 缓存实例”（理由是 renderer 无状态），但 fork 的 `CustomServerRenderer.ServerUrl`/`Endpoint` 是**每轨可变状态**（`URenderSettings.Validate` 写入、轨道设置对话框读回）；共用实例会让多轨的服务器地址互相覆盖。故 `GetOrCreate` 对 `CUSTOM_SERVER` 仍返回新实例，其余 renderer 照用上游缓存。`HIFIUTAU` 全静态，可安全共享。
- **`OpenUtau.Test/Core/Pipeline/PhraseSourceHashTest.cs`（上游新文件）：golden 值改为 fork 值并加注说明**。该测试锁定 `phrase.hash`/`phone.hash` 的字节级取值。fork 的 `RenderPhone.Hash()` 额外覆盖 phtp/strt/splc/stms、`RenderPhrase.Hash(true)` 额外覆盖 lowc/warm/hcmp/brel/breh，故所有取值与上游不同。做法：先**实验验证**——只从合并后的代码里删掉这两处 fork 专属写入，测试即逐字节复现上游 golden 值（`p 421f4de4…` / `h 4b02f6dc…` 等），证明除 fork 专属输入外字节布局与上游完全一致；随后把 golden 换成 fork 的实际值（`p 871a0b45…` 等），等价于“锁定 fork 迁移前的取值”。
- **自动合并的其余文件全部采纳**：`Pipeline/Identities.cs`、`PhraseSourceBuilder.cs`、`Snapshots.cs`（新增快照/身份/命令影响集）、`DocManager.cs`（`DocRevision` + 命令影响集）、`UPart.cs`（part 身份 + 快照回填槽 + 生成门）、`UCommand.cs`/`ExpCommands.cs`/`NoteCommands.cs`（影响集声明）、`RenderEngine.cs`（项目锁外等 `WaitPhraseSource`）、`DawAudio.cs`、`ThreadGuard.cs`、`UTrack.cs`（`Renderer = Renderers.GetOrCreate(...)` + `DocumentSnapshotStore.SetTrack`；fork 的 `serverUrl`/`endpoint`/`HIFIUTAU_LOCAL|ONLINE` 归一化自动保留）、`Renderers.cs` 的上游部分（`GetOrCreate` + `ConcurrentDictionary`）。`Renderers.cs` 与 `UTrack.cs` 是唯二两侧都改的自动合并文件，已逐个核对 fork 与上游改动都在。
- **字符串同步**：本次未触碰任何 `Strings.*.axaml`，无需运行 `Misc/sync_strings.py`。

### 已验证

- `dotnet build OpenUtau -c Debug`：0 错误 / 0 警告（增量）。
- `dotnet test OpenUtau.Test`：**475 通过 / 0 失败 / 1 跳过**（跳过项仍为 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`，需真实 DAW 插件，与上游一致）。上游新增测试 `PhraseSourceBuilderTest`、`PhraseSourceHashTest` 全部通过；fork 的 `HifiUtauCustomServerRendererTest`（断言 `phone.phonemeType` 等 4 个字段）经新快照路径通过，`TrackSettingsViewModel`/`CustomServerRenderer` 相关行为未回归。
- 哈希等价实验：见决策记录第 4 条（临时删除 fork 专属写入 → 逐字节复现上游 golden → 恢复 → fork golden 通过）。
- `git diff --check`：无冲突标记残留；本次无 `Strings.*.axaml` 变更，未运行 `Misc/sync_strings.py`。

---

## Merge 2026-09-10 4696de48

- **时间**（UTC）：`2026-09-10T08:24:54Z`（验证完成时间）
- **合并方式**：`git merge --no-ff --no-commit upstream/master`（merge-base `17bf25e7f78c5f88a6c5bce437bf6d592f012e46`，即上次合并记录的上游基线；合并范围 `17bf25e7..4696de48`）
- **上游基线**：`4696de48e7cbf6ab2a4a3c2bdd21552c69f24073` — Fix pitch spike at merged-phrase gaps (DiffSinger) (#2388)
- **fork 侧基线**：`6636147e3ad670f6960a3134472830d6c42eae30`（Merge upstream/master (17bf25e7) into master）

### 引入的上游 commit（18 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `31c7c5d407b250dde2d6686ef1571c8a922ed44f` | 1 (#2387)（Avalonia/ReactiveUI 版本提升、`TopLevel.GetTopLevel` 换用） |
| 2 | `a255be420d8a211215503cd1d097218a12ffc2d6` | 1 (#2386)（`Phonemizer.GetParentVoiceColor` 越界保护、`UTrack` 声部色默认值钳制） |
| 3 | `4febd495b86d91753c37f85cea3fa719cad59560` | Add retry loop to NSIS Chocolatey install step |
| 4 | `c5977c13a838560cf24524361e103acf5816ff30` | Fix Singer Selector Errors (#2383) |
| 5 | `64fedd61362cd53109da15ea60a9229706ca2d70` | Fix EditTools Shortcut (#2377) |
| 6 | `d53af641294f943024e12a8b41de6a982cb34bd6` | Fix for crashes when selecting a singer (#2374) |
| 7 | `d420674510d439c31dccdfaca96a385d66d08700` | Wire the DAW integration API into the UI (#2376) |
| 8 | `680959d9a72c0910e3d9e533f2a6a27a98eb3e1a` | Adds scroll function to BPM (#2385) |
| 9 | `27b09aa73cda119bb007ec55e085c6580ec7b7ec` | Update translations (#2373) |
| 10 | `0c934958560a9864d90f90233f27fbee9e2ac575` | Duplicate notes with Alt + dragging (#2267) |
| 11 | `99f1def2e294493b64c17f37eb55a4065a699c9c` | Chinese VCV phonemizer BY樗儿 support (#2225) |
| 12 | `07f4bdc4e99c4ede5d010446766ac885493cf889` | Guard TimeAxis against non-finite tempo values (#2347) |
| 13 | `820928a54319a2d6558a5a0728e16b8fd82c19fc` | Fix PreferencesViewModel resave-on-open more robustly |
| 14 | `535e6857e62b9b34f727bfd60ab9c1879925f3cf` | Migrate from Newtonsoft.Json to System.Text.Json (#2189) |
| 15 | `468939a6f7620bb06e94050145aad5bbbe573b6d` | Rename default phonemizer menu label to "Select Phonemizer" (#2151) |
| 16 | `138e3af3a1eae85ad5f52fc13c842b1874a169f6` | Remove auto-merge workflow |
| 17 | `9138af6e52620968bb5e5becdc50be90b1c2d230` | Serialize WorldlineRenderer cache file access per path |
| 18 | `4696de48e7cbf6ab2a4a3c2bdd21552c69f24073` | Fix pitch spike at merged-phrase gaps (DiffSinger) (#2388) |

### 冲突（24 个文件）

`git merge` 报 24 个冲突文件；另有 2 处 git 未标记、但语义重复的自动合并（见决策记录第 2、3 条）。

| 文件 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- |
| `OpenUtau/Strings/Strings.axaml`（4 处） | `7ab7714a77eb2161db5be09eb16eb15d1f569406`（DAW 集成字符串）、`6a3d3c7cb1f457f356e795e5c54e1bce244a897f`（HiFiUTAU 偏好字符串）、`39c12f7774b4e515b199f0b0975bed7e91d1c154` / `a65e4bac9d6f0eb3ffcb446217b266d6596b910a` / `c29b2b100c7ef67652a16fc23c9c7c9a0ce8a073`（Studio UI 外观键） | `d4206745`（DAW UI 字符串，与 fork 同名同文本）、`27b09aa7`（Crowdin 翻译更新）、`468939a6`（`command.note.duplicate` 等） |
| `OpenUtau/Strings/Strings.de-DE / es-ES / es-MX / fi-FI / fr-FR / id-ID / it-IT / ja-JP / ko-KR / nl-NL / pl-PL / pt-BR / ro-RO / ru-RU / th-TH / tr-TR / vi-VN / zh-CN / zh-TW.axaml`（19 个） | `4f52cceecebc50f4df2db2a40d05578814c364e1`（sync_strings.py 全语言补键）、`dece045ef5c37853f5c2aa11d48dd38e82caaf25`（zh-CN 译文）及上述 Studio UI 提交 | `27b09aa7`（整份重译，行级全量冲突） |
| `OpenUtau/ViewModels/DawIntegrationViewModel.cs`（4 处，add/add） | `53b2417f10935b397b6c2e7b303b59ac0177519a`、`7ab7714a`、`d44e7b69293954db771393fdd34bde8e8cde7cae`、`530d83141fe1bb770778a9ba08ca2ebcb65148f3` | `d4206745`（上游独立实现的同一 UI） |
| `OpenUtau/Views/DawIntegrationDialog.axaml`（2 处，add/add） | `7ab7714a`（列宽 + Close 按钮）、`53b2417f` | `d4206745` |
| `OpenUtau/Views/DawIntegrationDialog.axaml.cs`（1 处，add/add） | `53b2417f` | `d4206745` |
| `OpenUtau/ViewModels/PreferencesViewModel.cs`（4 处） | `fcb6f2ab182eb86de4df05a1489e798e3b9483ab`、`39c12f77`、`a65e4bac`、`c29b2b10`（Studio UI 偏好 + HiFiUTAU 偏好） | `820928a5`（`PersistOn` 重构）、`31c7c5d4`（版本提升附带 1 行） |
| `OpenUtau/Views/MainWindow.axaml` / `.axaml.cs`（自动合并后语义重复） | `53b2417f`（DAW 菜单项 + `OnMenuDawIntegration`） | `d4206745`（同一菜单项 + 同名处理函数）、`680959d9`（BPM 滚轮） |
| `OpenUtau.Core/OpenUtau.Core.csproj`（自动合并后语义冲突） | 无（fork 未改此文件） | `535e6857`（移除 `Newtonsoft.Json` 包引用） |

### 决策记录

- **`PreferencesViewModel.cs`：取上游 `PersistOn` 重构，fork 的 6 个偏好改写成 `PersistOn` 形式追加**。冲突段 1（构造器初始化）保留 fork 的 HiFiUTAU/Custom Server 初始化，删除手写的 `ShowOnnxGpu = (...)`——上游已把它改成由 `OnnxRunner` 派生的 `ObservableAsPropertyHelper`。冲突段 2 删除 fork 的 `ThemeEditable = ...` 手写赋值。冲突段 4 保留 fork 的 `SetHifiUtauSplicerPath`/`SetHifiUtauHnsepPath`/`ResetHifiUtauPaths`/`RefreshHifiUtauStatus`（`PreferencesDialog.axaml(.cs)` 仍在调用），删除 `ToggleOnnxGpuDisplay`（`ShowOnnxGpu` 现在只读派生，唯一调用点在冲突段 3 中已随上游版本消失）。冲突段 3（最大段）采纳上游 40 行 `PersistOn(...)` 列表，并追加 fork 的 `HifiUtauEmbedded`/`HifiUtauPreload`/`HifiUtauSplicerPath`/`HifiUtauHnsepPath`/`HifiUtauIntraOpThreads`/`CustomServerUrl`——这样 fork 偏好同样获得上游的 `Skip(1)`（打开对话框不再回写 prefs）语义；`CustomServerUrl` 保留“空值不写”的判断，放在回调内。**另改一处上游公式**：`ThemeEditable` 的派生表达式改用 fork 的 `ThemeManager.IsBuiltIn(themeName)`（= Light/Dark/Studio/WarmSage），而不是上游的字面 `themeName != "Light" && themeName != "Dark"`——否则 fork 的 Studio/WarmSage 内置主题会被判成可编辑主题，露出自定义主题的编辑/删除按钮。校验脚本比对：fork 侧 122 个 `Preferences.Default.*` 赋值在合并结果中**一个不少**，上游 50 个也全部在内。
- **DAW 集成 UI（3 个 add/add 文件）：以 fork 版为底，采纳上游的启用/断开判定**。两侧是同一功能的两次独立实现：fork 版有 `button.close` 关闭按钮、固定列宽、注释指向 fork 的 `API.md`；上游版有更稳的状态判定。故 `DawIntegrationDialog.axaml`/`.axaml.cs` 整取 fork 版（保留 Close 按钮与 `OnClose`）；`DawIntegrationViewModel.cs` 取 fork 版但换成上游的 `ConnectEnabled`（要求 `Disconnected`，避免重连中重复连接）、`DisconnectEnabled`（按 `DawManager.Inst.Connections` 实际状态判断，重连中也能断开、选中未连接项不再阻塞）与 `DisconnectAsync`（只断开确实有连接的端口，否则全断）。上游的 `using DynamicData.Binding;` 未采纳——fork 版该文件不使用 DynamicData，且 fork 此前已删（保留会留无用 using）。
- **`MainWindow.axaml` / `.axaml.cs`：删除重复的 DAW 菜单项与同名处理函数**。git 未报冲突，但两侧各自加了一行菜单项和一份同名 `OnMenuDawIntegration`（两份函数体逐字相同）。保留上游位置（Full Screen 之后 / 上游函数所在处），删除 fork 那份，避免菜单出现两个 “DAW Integration...” 与编译期重复成员。
- **`OpenUtau.Core.csproj`：把上游删掉的 `Newtonsoft.Json` 包引用加回**。上游 `535e6857` 把 Core 迁到 `System.Text.Json` 并移除了包引用，但 fork 的 `HiFiUtau/*`（`HifiUtauPhraseJson`、`SynthesisEngine`、`PhraseData`、`HifiUtauRenderer`）与 `CustomRender/*`（`CustomPhraseJson`、`CustomServerRenderer`）仍用 `JsonConvert`。加回时附注释说明原因；Core 其余部分照上游走 `Json.Serialize/Deserialize`。
- **19 个语言文件：整取上游重译版，再回填 fork 专属键的既有译文**。做法：先 `git checkout --theirs` 取上游 `27b09aa7` 的重译，再把 fork 侧“上游没有、且 fork 已翻译（值≠英文）”的键追加回去（共 215 条：zh-CN 136、zh-TW 10、id-ID/nl-NL 各 8、fr-FR/ru-RU/pt-BR/vi-VN 各 7、it-IT 5、de-DE/es-MX/pl-PL/th-TH 各 4、ja-JP/ko-KR 各 2）。这样上游的 Crowdin 重译与 fork 的人工译文都不丢——若只跑 `sync_strings.py`，这 215 条会被英文占位符覆盖。
- **`Strings.axaml`（英文源）：取两侧键的并集**，4 处冲突全部保留 fork 键块、补入上游独有的 `command.note.duplicate`（同键文本两侧本已逐字一致）。随后跑 `Misc/sync_strings.py` 归一化（排序、给每个语言补齐缺失键并以注释形式标出未翻译项），脚本已把 fork 的 130 个专属键分发到全部 22 个语言文件。
- **自动合并的其余文件全部采纳**：Avalonia 12.1.2 / ReactiveUI 24.2.0 / SourceGenerators 3.2.0 版本提升与 `TopLevel.GetTopLevel` 换用、`Json.cs` 包装与各格式/工具类的 JSON 迁移、`TimeAxis` 非有限 BPM 防护及其新测试、Alt 拖拽复制音符（`NoteEditStates`/`NoteBatchEdits`）、DiffSinger 合并乐句间隙的 pitch spike 修复与 `RenderPitchResult.voiced`、WorldlineRenderer 缓存文件按路径串行、歌手选择器崩溃修复（`UTrack` 色值钳制、`TrackHeaderViewModel` 空 ID 过滤 + `GetParentVoiceColor` 越界保护）、EditTools 快捷键重排、BPM 滚轮、新增 `ChineseVCVPhonemizer` 与 `ar-SA`/`cs-CZ`/`uk-UA` 三个新语言文件、删除 auto-merge workflow。核对脚本确认：fork 改过的 `UTrack.cs`、`TrackHeaderViewModel.cs`、`TrackHeader.axaml.cs`、`PianoRoll.axaml.cs`、`Preferences.cs` 中 fork 侧的成员/字段声明**零丢失**。

### 已验证

- `dotnet build OpenUtau -c Debug`：**0 错误**（完整重建，1765 条存量警告，与历次一致）。
- `dotnet test OpenUtau.Test`：**488 通过 / 6 失败 / 1 跳过**（总计 495）。6 个失败全部是 `OpenUtau.App.StudioOneControlsTest` 的 `NullReferenceException`（Studio 按钮皮肤的 headless 资源查找，`StudioUiTestHelpers.AssertResourceColor` 行 59）。为确认与本合并无关，用 `git worktree` 在合并前基线 `6636147e` 上跑同一过滤器：**同样 6 失败 / 2 通过**，堆栈逐字相同——属环境（headless 下 DynamicResource 不解析）导致的存量失败，非本次合并引入。跳过项仍为需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`。上游新增测试（`TimeAxisTest`、`LoadRenderedPitchTest` 等）全部通过；fork 的 Studio UI / HiFiUTAU / Custom Server / DAW 集成测试未回归。
- 字符串校验脚本：英文源 928 键；fork 键与上游键**并集完整**（无缺失、无多出）；22 个语言文件与英文键集一致（未翻译项以 `<!--<system:String ...>-->` 注释形式存在）；抽样确认 `prefs.appearance.noteefx.advanced`、`dialogs.tracksettings.endpoint` 等 fork 译文仍在，`dawintegration.*`、`button.close` 等键在英文源中存在。
- `git diff --check`：无冲突标记残留；仅上游自带的 4 处行尾空格（`Strings.ko-KR`/`Strings.zh-CN` 译文、`MainWindow.axaml.cs` 上游代码）与本次处理无关。
- `Misc/sync_strings.py`：已运行，输出即上述归一化结果。
