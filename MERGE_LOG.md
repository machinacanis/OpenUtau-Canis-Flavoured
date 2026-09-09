
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
