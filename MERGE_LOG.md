
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

---

## Merge 2026-09-12 9699944e

- **时间**（UTC）：`2026-09-12T16:41:16Z`（验证完成时间）
- **合并方式**：`git merge --no-ff --no-commit upstream/master`（merge-base `4696de48e7cbf6ab2a4a3c2bdd21552c69f24073`，即上次合并记录的上游基线；合并范围 `4696de48..9699944e`）
- **上游基线**：`9699944ead5a3b27b59bdf5a35f73fada8c11b7b` — Fix null reference when clicking mute (#2390)
- **fork 侧基线**：`392d0510c0db991a8047755588168aab678fdf8d`（Merge pull request #13 from KurotaniTakeo/feature/modeless-track-fx）
- **前置操作**：本 fork 此前未配置 `upstream` 远程，本次新加 `https://github.com/openutau/OpenUtau.git` 后 fetch。

### 引入的上游 commit（9 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `0e74b8d28ca0534297bfd514579b5a09b83e3a19` | ustx: fix crash validating a singer with no subbank colors |
| 2 | `0963623d0d5743ce288a9651aec89b3ce39e4a71` | render: clamp tempo lookup before the first tempo segment |
| 3 | `7684d7068581cebc241ed76fcbd224b8be7666b0` | phonemizer: make the phonemizer factory cache thread-safe |
| 4 | `4be726c52465590a5d73326dcfb34912cbc9635a` | render: write worldline cache synchronously |
| 5 | `2645b69a01178b2481fbf5d3e510137b3eacacd3` | Extension of curve editing tools, addition of editing logic, and UI improvements (#2393) |
| 6 | `d58f6e9c0571da766201931251f777d62fea338e` | Fix SBP handle cluster index bounds, glide positioning, and optional phoneme tokenization (#2382) |
| 7 | `7ef9932876ded2551a627520e5a49f20785cd28c` | feat: `.m4a` audio file import (#2141) |
| 8 | `4b1c4605be395e6d48ddcac2949b967ffbbb4141` | Use action to install NSIS in build workflow |
| 9 | `9699944ead5a3b27b59bdf5a35f73fada8c11b7b` | Fix null reference when clicking mute (#2390) |

修改面（上游侧 25 个文件）：新增 `OpenUtau.Core/Format/AACWaveReader.cs`（+`SharpJaad`/`SharpJaad.AAC` 包引用，`.m4a` 导入）与 `OpenUtau.Test/Core/USTx/UCurveTest.cs`；曲线编辑工具全套（`UCurve.cs`、`CurveViewModel.cs`、`NoteEditStates.cs` +298、`PianoRoll.axaml(.cs)`、`PianoRollStyles.axaml`、`Strings.axaml` 9 个新键）；`TimeAxis` tempo 钳制；`UTrack` 空声部色防护；`PhonemizerFactory` 线程安全；`WorldlineRenderer` 同步写缓存；`SBP`/`SyllableBasedPhonemizer` 系列修复；`MainWindow.axaml.cs` 加 `.m4a`。

### 冲突（3 个文件）

11 个文件两侧均有改动，其中仅 3 个出现冲突标记，其余 8 个由 ort 自动合并（双侧改动互不重叠，已逐条核对）：

| 文件 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- |
| `OpenUtau.Core/Api/PhonemizerFactory.cs` | `41077171`（Make PhonemizerFactory's cache concurrent）+ `891b9cbe`（Address the CodeRabbit review on PR #10） | `7684d706`（phonemizer: make the phonemizer factory cache thread-safe） |
| `OpenUtau/ViewModels/TrackHeaderViewModel.cs` | `32ffb5ec`（fix: rename ToggleMute(bool) overload to SetMute） | `9699944e`（Fix null reference when clicking mute (#2390)） |
| `OpenUtau/Controls/TrackHeaderCanvas.cs` | `32ffb5ec`（同上，调用点） | `9699944e`（同上，调用点） |

### 决策记录

- **`PhonemizerFactory.cs`：采纳上游的 `ConcurrentDictionary` 版本（`git checkout upstream/master --`，与上游逐字节一致）**。~~原决策为保留 fork 的单一 `registryGate` 锁~~。两侧修的是同一个竞态（`Get(Type)` 注册、`Get(string)` 按名查找、`BuildList()` 发布 `orderedFactories` 三者并发）；fork 的锁方案覆盖面更宽（`891b9cbe` 论证过 `ConcurrentDictionary` 下 `BuildList()` 仍可能抢在 `Get(Type)` 写入前取快照），但按 `AGENTS.md` 的 **Merge conflict policy**「同功能双实现一律取上游、并尽量减少该文件与上游的差异」，本处改取上游。fork 该文件现与 `upstream/master` **逐字节相同**（`git diff` 为空），后续同步不再冲突。
- **`TrackHeaderViewModel.cs` / `TrackHeaderCanvas.cs`：采纳上游的 `ToggleMuteWithBool(bool)` 命名**。~~原决策为保留 fork 的 `SetMute` 命名~~。两侧独立发现了同一个 bug：`ToggleMute()` 与 `ToggleMute(bool)` 重载在 ReactiveUI 解析命令绑定时冲突，点 “mute all” 时崩溃。fork `32ffb5ec` 改名 `SetMute`（并给 `ToggleMute` 加了 `TrackMuteVisualEvent`），上游 `9699944e` 改名 `ToggleMuteWithBool` 并把 `if (mute) Mute = true; else Mute = false;` 简化为 `Mute = mute;`。按 Merge conflict policy 取上游命名与写法，`TrackHeaderCanvas.cs` 的 `-1` 分支同步改回 `ToggleMuteWithBool(e.allmute)`，两个文件仅剩下面这一行 fork 差异（不再有命名分歧）。
- **`TrackHeaderViewModel.ToggleMuteWithBool` 内保留 `TrackMuteVisualEvent` 一行**：该事件是 fork 独有的 Studio 静音视觉（`PartsCanvas.cs:118` 消费），上游没有，属 Merge conflict policy 第 3 条「fork 独有功能另起落点」，**不算同功能双实现**，故不随上游删除。已加注释 `// fork-only: drives the Studio mute dimming in PartsCanvas.` 标明归属与理由。
- **`OpenUtau.Test/Core/Api/PhonemizerFactoryTest.cs` 保留**（fork 独有文件，上游无对应测试）。原判断是「改回上游实现后该压测必失败」，**实测不成立**：在上游 `ConcurrentDictionary` 实现下连跑 12 次 `ConcurrentRegistrationNeverProducesAnIncompleteSnapshot` **0 失败**，3 个测试全通过——它断言的不变量（「`GetAll()` 里可见的 factory 必能按名查到」）在上游实现下同样成立（`Get(string)` 读的是同一个并发 map）。故不必删除，仅把类文档注释里「One gate must cover all three」那段 fork 锁设计的描述改为中性的不变量陈述，避免注释与实现不符。
- **自动合并的 8 个重叠文件全部采纳**（`OpenUtau.Core.csproj`、`UTrack.cs`、`Preferences.cs`、`PianoRoll.axaml`、`PianoRoll.axaml.cs`、`Strings.axaml`、`PreferencesViewModel.cs`、`MainWindow.axaml.cs`），并逐文件核对两侧改动均未丢失（方法见“已验证”第 3 条）：
  - `OpenUtau.Core.csproj`：上游新增 `SharpJaad` / `SharpJaad.AAC` 两个包引用，fork 的 `Newtonsoft.Json` 引用与说明注释（HiFiUTAU / Custom Server 仍用 `JsonConvert`）并存，互不重叠。
  - `UTrack.cs`：上游的 `colors.Count > 0` 空声部色防护（`VoiceColorExp` / `VoiceColor2Exp` 两处）与 fork 的 `URenderSettings` 新增字段（`serverUrl`、`endpoint`、`HIFIUTAU_LOCAL/ONLINE` 迁移、`CUSTOM_SERVER` 默认值注入）分处不同区段，两侧均完整。
  - `Preferences.cs`：上游新增 `DefaultSnapCurve = true`（1 行）落在 fork 的 Studio/HiFiUTAU 字段块之外；fork 的 91 行新增字段全在。
  - `PianoRoll.axaml` / `.axaml.cs`：上游曲线工具条扩到 8 项（`pitchLineTool`/`verticalStretchTool`/… + 对应 `PianoRollStyles.axaml` 图标与 `ToolTip.Tip`）与 fork 的 `WaveformImage` `TrackHeight`/`TrackOffset` 绑定、Studio 面板拖拽分隔条（`OnPanelDragPressed/Moved/Released`）分处不同区段，两侧均完整；`CurveTools` 枚举 8 值与 4 个 `Curve*State` 类均已落地。
  - `PreferencesViewModel.cs`：上游的 `DefaultSnapCurve` 属性 + `PersistOn` 持久化（5 行）与 fork 的 697 行 Studio/HiFiUTAU 改动并存；fork 的 `ThemeEditable` 修正（改用 `ThemeManager.IsBuiltIn(themeName)`，避免 Studio/WarmSage 内置主题露出可编辑按钮）仍在。
  - `MainWindow.axaml.cs`：上游的 `AudioExts` 加 `.m4a` 与 fork 的 `OnMenuDawIntegration` 位置、`MixFxWindowManager` 调用并存，无上一轮那样的菜单项/处理函数重复。
  - `Strings.axaml`：取两侧键的并集（fork 137 行 + 上游 9 个曲线键），合并后英文源 937 键、无重复键。
- **`Misc/sync_strings.py` 必须手动恢复 BOM**：脚本以 `encoding='utf8'`（无 BOM）回写全部文件，会剥掉英文源 `Strings.axaml` 的 BOM，与 `.editorconfig` 的 `charset = utf-8-bom` 以及上游该文件的 BOM 冲突。本次运行后已手动把 BOM 补回；22 个语言文件本就无 BOM（脚本产物的既定形态），保持不变。已用隔离 harness 验证：脚本产物对 22 个语言文件**幂等**（重复运行内容不变），仅英文源因 BOM 每次都“变化”。

### 已验证

1. `dotnet build OpenUtau -c Debug`：**0 错误**（1766 条存量警告，与历次同量级）。
2. `dotnet test OpenUtau.Test`（合并结果）：**本机无法跑完全量**。测试宿主在第 ~226 个用例处被 **ONNX Runtime 原生崩溃**终止：`Fatal error. 0xC0000005` at `Microsoft.ML.OnnxRuntime.CompileApi.NativeMethods..ctor(DOrtGetCompileApi)` ← `NativeMethods..cctor()` ← `SessionOptions..ctor` ← `InferenceSession..ctor(Byte[])` ← `OpenUtau.Api.G2pPack.LoadPack` ← `GermanG2p..ctor` ← `GermanVCCVPhonemizer.GetBaseG2ps`，即 `Microsoft.ML.OnnxRuntime`（非 DirectML）的 native DLL 加载失败。**已确认属环境问题、非本合并引入**：在合并前基线 `392d0510`（`git worktree` 独立目录）上以同一命令复现，栈逐字相同，两次崩溃时刻均为 `00:01:01.0x`。机因是 NuGet 缓存只有 `microsoft.ml.onnxruntime.directml` 与 `microsoft.ml.onnxruntime.managed`，**缺 `microsoft.ml.onnxruntime` 本体**（`OpenUtau.Core.csproj` 的 `net10.0` 非 Windows 分支引用它）。上游新增的 `UCurveTest` 在合并结果上单独跑 **10/10 通过**。
3. **两侧改动零丢失核对**（对 11 个重叠文件逐一执行）：分别取“上游侧新增行”与“fork 侧新增行”（base→各自 HEAD 的 `+` 行，去重、忽略 ≤12 字符的短行），在本合并结果文件中逐条做字面匹配。结果：上游侧**全部命中**（3 个冲突文件在按 Merge conflict policy 改取上游版本后已无遗漏）；fork 侧除 `PhonemizerFactory.cs` / `TrackHeaderViewModel.cs` / `TrackHeaderCanvas.cs` 三者**被有意让给上游**的实现（锁方案、`SetMute` 命名）外全部命中（`PreferencesViewModel.cs` 697 行、`Strings.axaml` 137 行、`Preferences.cs` 91 行等全在）。另核对：上游仅有的 2 个新增文件（`AACWaveReader.cs`、`UCurveTest.cs`）均存在；上游无删除文件；3 个 `x:Key` 重复扫描（22 个语言文件）为空；`MainWindow.axaml` 的 DAW 菜单项与处理函数各只有一份。
4. **`Strings.axaml` 与 22 个语言文件的键完整性**：英文源 937 键（唯一、无重复）；合并前 22 个语言文件各缺上游的 9 个曲线键，跑 `sync_strings.py` 后**全部 937/937 一致**（未翻译项以 `<!--<system:String ...>-->` 注释形式存在，属脚本既定形态）。同步为**纯增量**：每个语言文件恰好 +23 行（9 个新键及其区段空行），`Compare-Object` 显示**零行删除**，即既有译文一条未丢；英文源的 `dawintegration.*`、`button.close`、`dialogs.tracksettings.endpoint`、`prefs.hifiutau.*`、`prefs.studioui.*` 等 fork 键全部保留。同步前已把 `OpenUtau/Strings/*.axaml` 备份到临时目录，逐文件比对确认。
5. **UTF-8 BOM 回归自查（本 fork 特有）**：`.editorconfig` 要求 `charset = utf-8-bom`。本次 3 个冲突文件是用文本编辑方式手工解的，编辑过程**剥掉了 `PhonemizerFactory.cs`、`TrackHeaderCanvas.cs`、`TrackHeaderViewModel.cs` 三者的 BOM**（已用逐字节比对合并前 `master` blob 发现并补回，现三者 BOM 与 `master` 一致）。另核实 `Strings.axaml` 与 `PreferencesViewModel.cs` 在合并后**新增 BOM**——这两个是上游侧自己加的（`.editorconfig` 方向一致），予以保留；`Preferences.cs` 两侧均无 BOM，维持原状。
6. **针对性测试**（绕开会崩的 ONNX 路径，全部通过）：`UCurve` 10/10、`Curve` 19/19、`TimeAxis` 18/18、`PhonemizerFactory` 3/3（含 `ConcurrentRegistrationNeverProducesAnIncompleteSnapshot` 压测，在上游 `ConcurrentDictionary` 实现下连跑 12 次 0 失败）、`Studio` 50/50、`TrackHeader` 5/5、`Wave`（m4a/AAC 读取）5/5、`Utau.Core.Render`（HiFiUTAU/Custom Server）8/8、`PhraseSource` 4/4、`Daw` 83 通过 / 1 跳过（跳过项仍是需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`）。注：`StudioOneControlsTest` 本次**全部通过**（上一轮的 6 个 headless 失败未复现）。
7. **未决项（已知、待跟进，非阻塞）**：`OpenUtau.Test/App/AppTest.cs` 的 `StringsTest`（`[AvaloniaFact]`，校验 `App.GetLanguages()` 能加载多语言）在**合并结果**上跑整个 `~OpenUtau.App` 组（55 个用例）时，出现 `[Test Case Cleanup Failure] System.InvalidOperationException : The calling thread cannot access this object because a different thread owns it`（`Avalonia.Headless.XUnit.AvaloniaTestRunner.Run` 的清理阶段）。**断言语义本身通过**——该用例单独跑 3/3 通过、与任一单个 App 测试类配对跑均通过、跑 `AppTest` 整类 2/2 通过；失败只在用例数累积到一定规模后出现（去掉 `StudioTrackPaletteTest` 整类即 37/37 通过，但把该类 19 个用例逐个单独加回时，加到第 17、18 个才复现，说明是**累积/顺序敏感**而非某个具体用例）。**已排除的成因**：与 `Strings.axaml`/语言文件无关（把合并前的 928 键版本整体换回合并树仍复现；反之把合并后的 937 键版本放进合并前基线仍 55/55 通过）；与 BOM 无关（剥掉 BOM 仍复现、且已重建）；与 xUnit 并行化无关（临时关掉 `parallelizeTestCollections` 仍复现）；`App.axaml.cs`、`AppTest.cs`、`ThemeManager.cs` 两棵树逐字节相同；测试产物 `prefs.json` 两棵树内容一致。合并前基线在**同一命令、同一新构建**下 15 次运行 **55/55 全绿**，合并结果 12 次运行 **稳定 1 失败**，故差异确实由本合并的改动触发，但触发点落在 **Avalonia headless 测试宿主的线程亲和性清理**上，而非产品逻辑——`StringsTest` 是唯一失败的用例，且失败发生在断言之后的 teardown。结论：**不阻塞本次合并**，但上游 `2645b69a` 引入的曲线工具改动与 fork 既有的 Studio UI 全局状态（`Application.Current.Styles` / `Preferences.Default`）在 headless 会话下的交互值得单独开 topic 排查；已在此明确记录，避免下一轮误判为“新引入的失败”。
8. `git diff --check`：无冲突标记残留（`^(<<<<<<<|=======|>>>>>>>)` 全库扫描为空）；上游自带的 5 处行尾空格（`EnglishCpVPhonemizer.cs` ×4、`SpanishVCCVPhonemizer.cs` ×1）与本次处理无关。
9. 清理：验证用的 `git worktree`（`ou-baseline`）已在提交前移除；临时 `xunit.runner.json` 已删除；`OpenUtau/Strings/*.axaml` 已从备份恢复为合并结果。

### 政策追溯调整（Merge conflict policy）

`AGENTS.md` 新增 **Merge conflict policy**（同功能双实现一律取上游、尽量减少与上游的差异）后，对本次合并的决策做追溯调整：

| 文件 | 调整前 | 调整后 |
| --- | --- | --- |
| `OpenUtau.Core/Api/PhonemizerFactory.cs` | fork 的 `registryGate` 锁 | 上游 `ConcurrentDictionary`（与上游**逐字节相同**） |
| `OpenUtau/ViewModels/TrackHeaderViewModel.cs` | fork 的 `SetMute(bool)` | 上游 `ToggleMuteWithBool(bool)`，保留 1 行 fork 专有的 `TrackMuteVisualEvent` |
| `OpenUtau/Controls/TrackHeaderCanvas.cs` | `SetMute(e.allmute)` | `ToggleMuteWithBool(e.allmute)`（与上游一致） |
| `OpenUtau.Test/Core/Api/PhonemizerFactoryTest.cs` | 描述 fork 锁设计 | 保留文件，注释改为中性不变量陈述 |

**唯一保留的 fork 差异**：`ToggleMuteWithBool` 内的一行 `MessageBus.Current.SendMessage(new TrackMuteVisualEvent(track.TrackNo));`——fork 独有的 Studio 静音视觉（`PartsCanvas.cs` 消费），不属同功能双实现。

**历史记录不改写**：`## Merge 2026-09-06` / `2026-09-07` / `2026-09-08` / `2026-09-09` / `2026-09-10` 各轮里的同类决策**保留原文未动**，改为在下文「DAW 复核」中给出逐个结论；涉及 fork 独有功能面的（Studio 波形绘制等）按政策第 3 条不在替换范围内。若后续评审认为其中某处应改取上游，再单独开 topic 处理并另起记录。

### DAW 复核（政策追溯第 2 批）

起因：评审指出「DAW 的实现完全不是 fork 做的，是上游之前合入的」。核实结论——**该判断正确，此前记录里的表述有误**，已在下面更正。

**事实核对**（`git diff upstream/master` 逐文件比对）：

| 部分 | 本地 vs `upstream/master` | 归属 |
| --- | --- | --- |
| `OpenUtau.Core/DawIntegration/` 5 个核心文件（`DawManager` / `DawMessages` / `DawServerFinder` / `DawTransport` / `DawAudio`） | **逐字节一致** | 上游 `84344cbd` #2369 |
| `OpenUtau.Test/Core/DawIntegration/` 8 个测试文件 | **逐字节一致** | 上游 `84344cbd` |
| `OpenUtau.Core/DawIntegration/API.md` | 一致 | 上游 |
| `OpenUtau/ViewModels/DawIntegrationViewModel.cs` | 差 1 删 2 增 | 上游 `d4206745` #2376 为底 + 2 处 fork 改动 |
| `OpenUtau/Views/DawIntegrationDialog.axaml` / `.axaml.cs` | 差 3 增 1 删 | **fork 侧 `53b2417f` / `7ab7714a` 自有**（上游同路径另有一份 `d4206745` 实现） |

更正两处此前记录的不准确表述：
1. 2026-09-06 记录称「DAW 集成**核心 + 测试 + 契约文档**按用户指示以上游为基准……fork 侧 Kakaru 版实现被替换」——**准确**，本次复核再次确认 5+8 文件与上游逐字节一致。
2. **但 DAW 的 UI 是 fork 自有的，且上游也有一份自己的 DAW UI**。2026-09-06 记录写过「fork DAW UI 保留并适配：`DawIntegrationViewModel.cs` / `DawIntegrationDialog.axaml(.cs)` / Tools 菜单项 / `dawintegration.*` 字符串为 fork 自有（上游无 UI）」——`d4206745` #2376 恰恰就是上游的 DAW UI，所以「上游无 UI」这句在上游 `d4206745` 合入后已不成立（该轮合并时间线上 `d4206745` 与 fork UI 同轮出现，故当时误记）。此处更正。

**本次处置**（按 Merge conflict policy 逐处判定）：

| 差异 | 判定 | 理由 |
| --- | --- | --- |
| `DawIntegrationDialog.axaml` 三列 `Width="60"/"130"/"110"` | **改回上游 `Width="Auto"`** | 同功能的两种写法，无功能差异；固定像素还比 `Auto` 更差（不随语言/字号自适应）。采纳上游以消除差异 |
| `DawIntegrationDialog.axaml` 的 `button.close` 关闭按钮 + `.axaml.cs` 的 `OnClose` | **保留** | 政策第 3 条：上游 `d4206745` 的对话框**没有**关闭按钮，这是 fork 新增功能，不是同功能双实现。`button.close` 英文键也确认是 fork 独有（上游 `Strings.axaml` 无此键）。已各加 `// fork-only:` 注释标明归属 |
| `DawIntegrationViewModel.cs` 删除 `using DynamicData.Binding;` | **保留删除** | 该文件不使用 DynamicData，属未使用 using，留着会吃警告 |
| `DawIntegrationViewModel.cs` 注释 `PROTOCOL.md §4` → `API.md §4` | **保留** | 政策第 4 条例外（上游实现有明确缺陷）：上游 `OpenUtau.Core/DawIntegration/` 下**只有 `API.md`，没有 `PROTOCOL.md`**，而 5 个核心文件里有 **26 处** `PROTOCOL.md` 引用全部指向不存在的文件。此处让注释指向真实文件；不采纳上游的陈旧引用，已在 `MERGE_LOG.md`（本条）记录该缺陷与证据 |

处置后 `DawIntegrationDialog.axaml` / `.axaml.cs` 的差异只剩「一个 fork 新增的关闭按钮 + 两行标注注释」，`DawIntegrationViewModel.cs` 只剩「未使用 using 的删除 + 一处指对文件的注释」。**DAW 核心与测试与上游逐字节一致，不会再有合并冲突。**

注意（不改写、供后续判断）：其余 26 处 `PROTOCOL.md` 引用位于与上游逐字节一致的 5 个核心文件里，**本 fork 保持原样不动机**——那些是上游自身的陈旧引用，改动它们会让核心文件重新偏离上游，与政策「尽量减少差异」冲突。要不要向上游报这个文档引用问题，属上游事务，另行决定。

### 其余历史决策复核（政策追溯第 3 批）

按「必须确认差异是否在修同一个目标」的要求，对 2026-09-07 / 09-10 剩余的 3 项逐条定性。判定用的取证方法：对每个文件分别取**上游侧 `base..upstream/master`** 与**fork 侧 `base..master`** 的改动，再看**当前文件相对 `upstream/master` 的残余差异**由哪一方贡献；差异行归属到 commit 与作者。

**1. `OpenUtau/Controls/WaveformImage.cs`（2026-09-07 记录）→ 不是同目标修复，保留 fork 版**

- 上游侧改动量：**0**。上游引入 phrase-bounds 绘制的 `ea676948` 已被上游自己的 `2b03ad56`（"Revert piano roll phrase bounds from WaveformImage"）撤销，所以该文件在上游侧净变化为零。
- fork 侧改动量：425 增 / 21 删（相对 `base`）；当前相对上游的残余差异与此同量级（425/21），全部来自 fork 的 `f22f3a13`（Studio UI chrome + 钢琴卷帘外观，author = machinacanis）。
- 该文件依赖的 3 个上游 render commit（`f773f377`、`6196917f`、`7c68a087`）**均为 Sugita Akira 且已合入本地**，不是 fork 分叉。
- 结论：这是 **fork 独有的 Studio 波形重绘**，与上游「phrase bounds」不是同一目标（且上游已自行回退该目标）。政策第 3 条 → 保留。**无可替换的上游实现。**

**2. `OpenUtau/ViewModels/PreferencesViewModel.cs`（2026-09-10 记录）→ 非同一目标，保留**

- 上游侧改动量：**5 增 / 1 删**，内容仅为新增偏好 `DefaultSnapCurve` 的三处接入（`[Reactive]` 属性、构造器初始化、`PersistOn` 持久化）——它是**在既有 `PersistOn` 框架里追加一个偏好**，不是在修「偏好持久化」这个机制。
- fork 侧改动量：997 增 / 3 删；当前相对上游残余 992/2。
- 双方不是同一目标的两种实现：上游在既有框架里加一行偏好，fork 在加**新功能**（Studio UI / HiFiUTAU / Custom Server 偏好页）。上一轮已经把 fork 的偏好改写成上游的 `PersistOn` 形式追加，即**已经在复用上游的结构**，无第二套机制可替换。
- 结论：政策第 3 条 → 保留。**无可替换的上游实现。**

**3. `OpenUtau/Strings/Strings.axaml`（2026-09-10 记录）→ 不是二选一，无替换概念**

- 当前本地 **937** 键、上游 **807** 键：**fork 独有 130，上游独有 0**，本地是上游的**严格超集**——不存在「同一目标的两种实现」，没有可替换项。
- 130 个 fork 独有键按前缀归类，**120 个是 `prefs.*`**（Studio UI / HiFiUTAU / Custom Server 设置），其余为 `noteproperty.*` 5（fork 自有 NoteProperties 控件）、`dialogs.*` 2（`tracksettings.serverurl`/`endpoint`，Custom Server）、`errors.*` 2（HiFiUTAU）、`button.close` 1（DAW 对话框）。
- 结论：全部对应 fork 独有功能面，政策第 3 条 → 保留。**此项无需处置。**

**最终状态汇总（政策追溯后的全套差异）**

| 有差异的对象 | 处置 | 是否同目标双实现 |
| --- | --- | --- |
| `PhonemizerFactory.cs` | 取上游，现逐字节一致 | **是**（同一竞态） |
| `TrackHeaderViewModel.cs` / `.axaml.cs` | 取上游 `ToggleMuteWithBool`，保留 1 行 fork 专有事件 | **是**（同一崩溃） |
| `OpenUtau.Test/Core/Api/PhonemizerFactoryTest.cs` | 保留，注释中立化 | 否（fork 独有测试） |
| `DawIntegrationDialog.axaml(.cs)` | 列宽取上游；保留 fork 的 Close 按钮 | 否（fork 新增功能） |
| `DawIntegrationViewModel.cs` | 保留 2 处（未使用 using 删除 + `API.md` 引对） | 否（后者属政策第 4 条例外） |
| `WaveformImage.cs` | 保留 fork | 否（fork 独有 Studio 功能） |
| `PreferencesViewModel.cs` | 保留 fork | 否（fork 新增偏好页） |
| `Strings.axaml` | 保留 fork 超集 | 否（严格超集，130 键全属 fork 功能） |

**没有发现别的「双方各修同一目标」的遗留项。** 至此政策追溯完成。

### 合并结果完整性复查（2026-09-12，按政策判定流程）

起因：政策追溯后反查「本次合并的冲突处置是否也造成了内容丢失」。结论：**没有丢失任何上游内容，也没有丢失有意的 fork 内容**。取证如下。

**1. 上游内容零丢失（决定性）**：取上游侧 `4696de48..upstream/master` 全部**修改类**文件中新增的每一行（去重、忽略 ≤12 字符的短行），在合并结果里逐条字面匹配——

| 指标 | 值 |
| --- | --- |
| 上游新增行总数 | **435** |
| 未命中 | **0** |

**2. fork 内容零丢失（除政策有意项）**：取合并前 fork 基线 `392d0510` 相对 `4696de48` 新增的每一行，在当前文件中逐条匹配——

| 指标 | 值 |
| --- | --- |
| fork 新增行总数 | **5071** |
| 未命中 | **30** |

30 行**全部**对应本章节记录的、按政策有意让给上游的改动，无一行属意外丢失：
- `PhonemizerFactory.cs` 23 行（fork 的 `registryGate` 锁实现及说明注释）
- `OpenUtau.Test/Core/Api/PhonemizerFactoryTest.cs` 3 行（描述 fork 锁设计的注释）
- `TrackHeaderViewModel.cs` / `TrackHeaderCanvas.cs` 各 1 行（`SetMute` 命名）
- `DawIntegrationDialog.axaml` 3 行（三列固定像素宽度）

**3. 相对上游的差异行逐条确认无内容丢失**（`git diff upstream/master --numstat` 后逐项归因）：

| 文件 | 差异 | 归因 | 判定 |
| --- | --- | --- | --- |
| `MainWindow.axaml.cs` | +20 / −11 | −11 中 9 行是 `OnMenuDawIntegration` **位置移动**（逐字相同），另 2 行是 `OnKeyDown` 重构为 `OnKeyDown => HandleGlobalShortcut(args)`（fork 为 `MixFxWindowManager` 抽的显式入口） | 移动/重构，非删除 |
| `TrackHeaderViewModel.cs` | +22 / −16 | Studio 功能面：`ApplyPaint()` + `StudioTrackPaintCache` 取代直接 `ThemeManager.GetTrackColor`；`MixFxWindowManager.Open` 取代内联 `MixFxDialog`；`ThemeEditable` 用 fork 的修正公式 | fork 功能 |
| `PianoRoll.axaml` | +9 / −3 | fork 的波形布局（`VerticalAlignment="Stretch"`、去掉 `Margin="0,0,0,60" Height="60"`、面板拖拽分隔条） | fork 功能 |
| `PianoRoll.axaml.cs` | +55 / −0 | fork 的 `OnPanelDragPressed/Moved/Released` | fork 功能 |
| `Strings.axaml` | +147 / −15 | 逐键内容级核对：**上游 807 键全部存在，缺失 0，同名键内容不同 0**；−15 行是 BOM 与键排序造成的行级假象 | 无丢失 |
| `Preferences.cs` / `OpenUtau.Core.csproj` | 各 −1 | 仅 **BOM** 差异（上游有 BOM，本地两棵树都没有）——删除 BOM 是 fork 既有状态，**非本次合并造成** | 既有状态 |
| 22 个语言文件 | 各 +187~+202 / 0~−13 | 同上：上游键集合全部存在，差异来自 fork 的人工译文与排序 | 无丢失 |

**4. 关键上游改动逐项确认已落地**（此前列为疑点的删除行，实为上游自身替换）：

| 上游 commit | 合并结果状态 |
| --- | --- |
| `4be726c5` worldline 缓存同步写 | ✅ `Wave.WriteMono16Wav(wavPath, result.samples)` 已同步、`samplesCopy`/`Task.Run` 已移除 |
| `0e74b8d2` 空声部色防护 | ✅ `UTrack.cs` 两处 `if (colors.Count > 0)` 均在 |
| `0963623d` TimeAxis 钳制 | ✅ `TimeAxisTest` 18/18 通过 |
| `d58f6e9c` SBP 修复 | ✅ 上游新增行 100% 落地；`EnablePhonemeTokenization` 在 3 个 Phonemizer 中均在 |
| `2645b69a` 曲线工具 | ✅ `CurveTools` 8 值齐全（`CurveLineTool` 已启用）、4 个 `Curve*State` 类在 |
| `7ef99328` `.m4a` 导入 | ✅ `FilePicker.cs` 与 `MainWindow.axaml.cs` 的扩展名列表均含 `.m4a`；`AACWaveReader.cs` 在 |
| `9699944e` mute 空引用修复 | ✅ `ToggleMuteWithBool` 已启用，`ToggleMuteWithBool` 调用点唯一 |

**5. `ToggleMuteWithBool` 内保留 `TrackMuteVisualEvent` 的复核**：确认 fork 基线 `392d0510` 的 `ToggleMute()` **与** `SetMute(bool)` **两者都发**该事件，故在 `ToggleMuteWithBool` 内保留这一行是**恢复 fork 既有行为**，不是本次合并新增的行为变化。

**复查结论**：本次合并的冲突处置**未造成上游内容丢失，也未造成 fork 功能意外丢失**；合并相对上游的全部差异都能归因到「fork 独有功能」或「政策有意取上游」，且有上文逐条证据。
---

## Fork 改动落点 2026-09-13 fix/onnx-native-availability-guard

本条**不是上游合并记录**，而是按 `## Merge conflict policy` 第 3 条登记一处 **fork 独有实现**，目的是让下一次上游同步能一眼识别这两个上游文件的本地改动，避免误判为可替换的双实现。

### 背景

用户报告：点「工具 → 使用偏好」后进程立即崩溃。取证结论（完整证据见 `plans/onnx-native-guard.md`）：

1. 应用自带的 `runtimes/win-x64/native/onnxruntime.dll`（1.24）因缺少 Microsoft Visual C++ 2015-2022 运行库而加载失败（`LoadLibraryExW` → `err=126`）；
2. Windows 加载器**不报错**，改为绑定 `C:\WINDOWS\SYSTEM32\onnxruntime.dll`（1.17，系统自带 AI 组件）；
3. 托管 ORT 为 1.24.4，去取 1.17 不存在的 `OrtGetCompileApi`；
4. `0xC0000005`，任何托管异常处理与 Serilog 都来不及介入。

实测已复现第 2 步：把应用自带原生库换成不可加载文件后，进程仍报告"加载成功"，实际绑定的是 `SYSTEM32\onnxruntime.DLL 1.17`。

### 引入的 fork 落点

| 文件 | 归属 | 说明 |
| --- | --- | --- |
| `OpenUtau.Core/Util/OnnxNativeAvailability.cs` | **fork 新增**（新文件） | 用绝对路径预加载自带原生库并校验版本；失败时给出可读原因与提示。绝对路径不参与搜索顺序，因此不存在静默回退。 |
| `OpenUtau.Core/Util/Onnx.cs` | 上游文件，**仅插入守卫** | `initializeDevices()` 与 `getGpuInfo()` 各加一处 `if (!OnnxNativeAvailability.IsAvailable)` 提前返回；无其他改动。 |
| `OpenUtau.Core/Util/Preferences.cs` | 上游文件，**仅插入守卫** | 启动时的 `Onnx.getRunnerOptions()` 改走 `GetOnnxRunnerOptionsSafely()`，失败回落 `["CPU"]`；新增一个私有方法，无其他改动。 |

**判定：非同目标双实现。** 上游侧改动量为 0（上游没有原生库可用性探测，也没有让 prefs 加载与 ONNX 解耦）。上游若将来引入等价实现，按政策第 1 条改取上游、删除本 fork 落点。

### 已验证

- 构建：`dotnet build OpenUtau -c Debug` 成功（3 个既有的 AVLN3001 警告，0 错误）。
- 测试：`OpenUtau.Test` 全量 **512 通过 / 0 失败 / 1 跳过**；新增 `OnnxNativeAvailabilityTest` 6 通过。
- 四态实测（Core 侧，调用链与 `PreferencesViewModel` 构造一致）：
  | 状态 | 探测结果 | `getGpuInfo()` | 是否崩溃 |
  | --- | --- | --- | --- |
  | 健康 | 可用，加载自带 1.24 | 1 个设备（AMD Radeon 780M） | 否 |
  | 自带库不可加载（缺 VC++ 等价） | 不可用，`SYSTEM32` **未被装载** | 0 | 否 |
  | 自带库缺失 | 不可用 | 0 | 否 |
  | 自带库为旧版 1.17 | 不可用，理由指出 1.17 < 1.24.4 | 0 | 否 |
- 未覆盖：经由真实 UI 点击「使用偏好」的端到端复跑本次未能完成（自动化无法在被锁定的交互桌面上取得前台焦点）。上一轮修复验证时该路径已确认窗口可正常打开。

---

## Merge 2026-09-16 81637a33

- **时间**（UTC）：`2026-09-16T11:29:03Z`（验证完成时间）
- **合并方式**：`git merge --no-ff --no-commit upstream/master`（merge-base `9699944ead5a3b27b59bdf5a35f73fada8c11b7b`，即上次合并记录的上游基线；合并范围 `9699944e..81637a33`）
- **上游基线**：`81637a33ceeaedd8c407a268ad048e182592508e` — Highlight the part within the piano roll display range (#2416)
- **fork 侧基线**：`9fdee2f068ec89e1005204876f690ab2c76e9207`（Merge pull request #14 from machinacanis/fix/onnx-native-availability-guard）

### 引入的上游 commit（12 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `ec221fa601e3fc6d25296782902cb061a7129e85` | do xUnit tests properly (#2398) |
| 2 | `df0555d808c58b8b6e5f2421e815fc549893d4b4` | Fix Lang ID assignment on Diffsinger singers without IDs (#2412) |
| 3 | `8122872025054a3ff7fc46cb2b8b4b74dd34bd63` | Fix JSON deserialization error in NotePresets (#2411) |
| 4 | `6348ca0d9b8817e81986fb38e6d9a8375dd251d4` | remove undefined curves (#2414) |
| 5 | `7a083786dea033fddfae8e791dc507ba47a359ee` | Fix ClassicSinger FreeMemory() falsely retaining the loaded state (#2408) |
| 6 | `9df74a3f56fdd3ebc41b1d319738e05f178eb92c` | Optimize package size (#2402) |
| 7 | `e3db1f9da65b54960f82727ad7399210cca45d0f` | diffsinger: always retake pitch locally; drop the LocalRetaking option (#2406) |
| 8 | `ceedbe5d1d019ab8f170240edd3532a71960e68c` | Added Guard Clauses, Null/Range Checks, and Fixed Indentation (#2415) |
| 9 | `6df3e30a504d09d92b640521dce2c0ccde3d7803` | Add appimage update support (#2403) |
| 10 | `73cd895712e6eeb944d39c1edf5770ca1b779037` | Collapse built-in phonemizer smoke tests from 18 cases to 6 (#2399) |
| 11 | `fe0894d398312504ca115dc9c68162218dc7544c` | Surface hidden failures: out-of-order phonemes and unhandled exceptions (#2404) |
| 12 | `81637a33ceeaedd8c407a268ad048e182592508e` | Highlight the part within the piano roll display range (#2416) |

修改面（上游侧 27 个文件）：崩溃可见化（`App.axaml.cs` 新增 `TaskScheduler.UnobservedTaskException` + `Dispatcher.UIThread.UnhandledException` 两个钩子、`UPart.cs` 乱序音素检测、新增 `UVoicePartAfterLoadTest.cs`）；钢琴卷帘视口高亮（`PartControl.cs` / `PartsCanvas.cs` / `NotesViewModel.cs` / `TracksViewModel.cs` / `MainWindow.axaml` 五文件联动）；DiffSinger 本地 pitch retake 改为永远启用并**删除 `DiffSingerLocalRetaking` 偏好**（`Preferences.cs`、`PreferencesViewModel.cs`、`PreferencesDialog.axaml`、`DiffSingerRenderer.cs`、`DiffSingerG2pPhonemizer.cs`）；`NotePresets` JSON 反序列化修复；DiffSinger Lang ID 分配修复；`ClassicSinger.FreeMemory()` 状态修复；包体积优化（`OpenUtau.Core.csproj` / `OpenUtau.csproj` / `PathManager.cs`）；appimage 更新支持（新增 `OpenUtau/Assets/AppRun`、`UpdaterViewModel.cs` +34）；phonemizer 冒烟测试 18→6 收敛；`NotePropertiesViewModel.cs` 守卫子句/空值检查与缩进修正。

### 冲突（3 个文件）

13 个文件两侧均有改动，其中仅 3 个出现冲突标记，其余 10 个由 ort 自动合并（双侧改动互不重叠，已逐条核对）。冲突文件及来源：

| 文件 | 上游侧改动量 | fork 侧改动量 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- | --- | --- |
| `OpenUtau/App.axaml.cs` | +37 / −0 | +52 / −2 | `f22f3a13`、`fcb6f2ab`、`c837526a`、`a711ff8b`（Studio UI chrome / 主题 / headless 测试） | `fe0894d3` #2404（隐藏失败可见化） |
| `OpenUtau/Controls/PartControl.cs` | +58 / −14 | +32 / −18 | `c837526a`、`2dcd10ac`、`1b767a77`（Studio 调色板绘制、静音变暗、波形重绘） | `81637a33` #2416（钢琴卷帘视口高亮） |
| `OpenUtau/ViewModels/TracksViewModel.cs` | +36 / −1 | +32 / −1 | `c837526a`、`2dcd10ac`、`1b767a77`、`77d616b4`（Studio 轨道高度、调色板失效、静音视觉） | `81637a33` #2416（钢琴卷帘视口高亮） |

**判定流程取证**（按 `AGENTS.md` 的 Merge conflict policy 判定流程执行，三个文件均已完成）：

1. **量化两侧改动**：见上表（`git diff --numstat <base> upstream/master` 与 `git diff --numstat <base> master`）。
2. **残余差异归属**：`git diff upstream/master -- <file>` 分别为 `53/3`、`34/20`、`33/2`，**上游侧改动量均非 0**，即上游确有实现，不能按「上游已自行回退」直接归入保留。
3. **归属到 commit 与作者**：`git log --oneline -- <file>` 显示三文件的差异**同一文件内混合两侧来源**——上游贡献集中在 `fe0894d3`（异常钩子）与 `81637a33`（视口高亮），fork 贡献集中在 `f22f3a13`/`fcb6f2ab`/`c837526a`/`2dcd10ac`/`1b767a77`/`77d616b4`（Studio UI）。两方作者无重叠。
4. **逐行核对目标**：上游 `81637a33` 是在既有 `Render()` 里**追加**视口高亮矩形；fork 是在 `Render()` 里把硬编码刷子换成 `StudioTrackPaintCache` 调色板（外加 `ClearWaveformBitmap()`）。**两者目标不同**（上游=新增视口指示；fork=主题化绘制），不构成同目标双实现，故**不触发政策第 1 条的替换**，改为合成。`fe0894d3` 与 fork 改的是 `App.axaml.cs` 完全不同的两个区段（异常钩子 vs Studio 主题/样式），同理不替换。

### 决策记录

- **`App.axaml.cs`：并集**。冲突标记只覆盖 `using` 块（上游加 `Avalonia.Threading` / `OpenUtau.Core`，fork 加 `Avalonia.Markup.Xaml.Styling` / `OpenUtau.App.Studio`），两侧其余改动（`RegisterUnhandledExceptionHandlers()`、`developerToolsAttached` 守卫、`SetTheme()` 的 WarmSage/Studio 分支、`ApplyStudioStyles`）落在不同区段且已由 ort 合并。取两侧 `using` 的并集即完整合并，无取舍。
- **`PartControl.cs`：合成上游视口高亮到 fork 调色板绘制之上**（政策第 3 条，两侧功能面不同）。具体：
  - 字段区块：**采纳上游新增的 `private readonly PartsCanvas partsCanvas`**（高亮需要它，且上游已在构造函数里加好 `partsCanvas = canvas;`，该行 ort 已自动合并）；**不保留上游重新加回的 `notePen` 字段**——fork `c837526a` 已把音符缩略图笔画换成调色板派生的 `paint.NoteThumbnailPen`，恢复 `notePen` 会变成未使用字段（`TreatWarningsAsErrors` 下 CS0414 会直接失败）。
  - 音符绘制：采纳上游把 `voicePart.notes.Count > 0` 改为内层 `if` 的结构（使空声部也能走高亮分支），但笔画用 fork 的 `paint.NoteThumbnailPen`，不恢复上游的 `notePen`——保持与该文件其余 fork 绘制一致，且与上游的差异仅为笔画来源一行。
  - 高亮块：**原样采纳上游 `81637a33` 的 `if (voicePart == partsCanvas.PianoRollOpenPart && pianoRollViewViewportTicks > 0)` 整段**（含 inset/vpLeft/vpRight/RoundedRect 数值），放在 fork 音符绘制之后、`else if (part is UWavePart)` 之前，与上游位置一致。
  - fork 专有面全部保留：`StudioTrackPaintCache.ForPart` 取色、静音变暗、`paint.SelectedStrokePen` 描边、`ClearWaveformBitmap()`、`DrawWaveform(..., paint.WaveformPackedRgba)`、`DrawPeak(..., uint packedRgba)`。上游视口高亮用固定的白色半透明填充（`Color.FromArgb(28,255,255,255)`），在 Studio 深色/浅色主题下均为中性指示色，不做主题化改造——理由是**尽量减小与上游差异**（政策第 2 条）。
- **`TracksViewModel.cs`：并集**。仅构造函数内一段订阅区块冲突：fork 的 `StudioUIChangedEvent` 监听（跟随 Studio 开关切换默认轨道高度，且仅当用户未自定义过）与上游的 `PianoRollOpenPartChangedEvent` / `PianoRollViewportChangedEvent` 两个监听**互不依赖**，两块均保留、按 fork 在前上游在后的顺序排列。上游在 `LoadProjectNotification` 分支新增的 `PianoRollOpenPart = null;` 与 fork 的 `StudioTrackPaintCache.Invalidate(StudioTrackPaletteChangeReason.TracksMutated)` 落在同一分支内但不同语句，ort 已自动合并，两侧均完整。
- **采纳上游删除 `using DynamicData;`（政策第 2 条）**：上游 `81637a33` 顺手删掉该未使用 using。fork 侧此前保留着它。核对合并后文件：全文件**不使用任何 bare `DynamicData` 命名空间符号**（无 `SourceList` / `SourceCache` / `ObservableListEx` / `.Connect(` 等），实际使用的是 `DynamicData.Binding`（其 `using` 独立保留）。故采纳上游删除，使该文件与上游差异更小；`DynamicData.Binding` 仍保留（`WhenAnyValue` 等需要）。已用全量构建验证无 CS0246。
- **`Strings.axaml`：取两侧键的并集**，合并后英文源 **936 键、唯一、无重复**。按政策判定流程第 5 条改比**键集合**而非行：上游侧本轮**独有键数为 0**（未新增键，只删除了 `prefs.rendering.diffsingerpitchlocalretaking`），fork 侧为上游的**严格超集**（独有键 130 个，全部对应 fork 功能面：`prefs.studioui.*`、`prefs.hifiutau.*`、`prefs.customrender.*`、`dawintegration.*`、`button.close` 等），故不存在可替换项。
- **删除孤儿字符串键 `prefs.rendering.diffsingerpitchlocalretaking`（随 `sync_strings.py` 自动清除）**：上游 `e3db1f9d` 删除 `DiffSingerLocalRetaking` 偏好后，该键在英文源已不存在，但 22 个语言文件里仍残留（zh-CN 是**活动键**「DiffSinger 音高局部重录」，其余 21 个是注释占位）。运行 `python Misc/sync_strings.py` 后 22 个文件各删除 1 行，英文源键集合未变。属上游删除偏好的**必然下游清理**，非独立决策。
- **`OpenUtau.Test/OpenUtau.Test.csproj`：不采纳上游 `ec221fa6` 的 xunit 版本提升，钉回 3.2.2（政策第 4 条例外）**。上游把 `xunit.v3` / `xunit.v3.common` / `xunit.v3.extensibility.core` / `xunit.runner.visualstudio` 从 `3.2.2` 提到 `4.0.0`。实测后果：**全部 20 个 `[AvaloniaFact]` 用例在发现阶段失败**，`System.MissingMethodException: Method not found: Xunit.v3.TestIntrospectionHelper.GetTestCaseDetails(...)`（`Avalonia.Headless.XUnit.AvaloniaFactDiscoverer.CreateTestCase`）。**缺陷、证据与取舍**：
  - **证据 1（上游自身兼容性边界）**：`Avalonia.Headless.XUnit` 在 NuGet 上最高版本为 **12.1.2**（已查 `api.nuget.org/v3-flatcontainer/avalonia.headless.xunit/index.json`），无支持 xunit.v3 4.x 的版本。
  - **证据 2（上游已知问题）**：AvaloniaUI/Avalonia 议题 **#22072「Headless testing does not work with xUnit.v3 4.0.0」**，维护者答复「That's a new major version of xUnit; it's unsupported for now … it contains breaking changes」。即这是**上游 xunit 提升与 Avalonia headless 适配器之间的已知不兼容**，不是本 fork 的用法错误。
  - **证据 3（上游不会遇到）**：上游 `upstream/master` 的 `OpenUtau.Test/` 目录下**没有任何 `[AvaloniaFact]`**（`git grep -n 'AvaloniaFact' upstream/master -- OpenUtau.Test` 为空；上游 `AppTest.cs` 的 `StringsTest` 是普通 `[Fact]` 并自行 `SetupWithoutStarting()`）。20 个 `[AvaloniaFact]` 全部是 fork 独有（`AppTest.cs` 1 个 + `StudioOneControlsTest.cs` 8 个 + `StudioTrackLayoutTest.cs` 6 个 + `TrackHeaderChromeTest.cs` 5 个），因此上游 CI 结构上无法暴露此问题。
  - **取舍**：把 xunit 线钉回 `3.2.2`（`xunit.v3`、`xunit.v3.common`、`xunit.v3.extensibility.core`、`xunit.runner.visualstudio` 四者同步钉回），并在 csproj 内写明原因与解除条件（待 Avalonia 发布 xunit.v3 4.x 适配器后即可放开）。**保留上游 `ec221fa6` 的另一半改动**：`pr-test.yml` 的命令由 `dotnet test OpenUtau.Test` 改为 `dotnet run --project OpenUtau.Test/OpenUtau.Test.csproj`（Microsoft.Testing.Platform）——实测该命令在 3.2.2 下正常工作，而 `dotnet test` 在 .NET 10 SDK 上直接报错「Testing with VSTest target is no longer supported」，故这部分必须采纳。
  - 该偏离使 `OpenUtau.Test.csproj` 与上游产生 **10/4** 的差异；上游若后续发布兼容版（或自行适配），按政策第 1 条改回上游版本、删除此处注释与钉版。
- **自动合并的 10 个重叠文件全部采纳，两侧改动均未丢失**（`OpenUtau.Core.csproj`、`OpenUtau.Core/Util/PathManager.cs`、`OpenUtau.Core/Util/Preferences.cs`、`OpenUtau/Controls/PartsCanvas.cs`、`OpenUtau/Strings/Strings.axaml`、`OpenUtau/ViewModels/NotePropertiesViewModel.cs`、`OpenUtau/ViewModels/NotesViewModel.cs`、`OpenUtau/ViewModels/PreferencesViewModel.cs`、`OpenUtau/Views/MainWindow.axaml`、`OpenUtau/Views/PreferencesDialog.axaml`）：
  - `PartsCanvas.cs`：上游新增 `PianoRollOpenPart` / `PianoRollViewTickOffset` / `PianoRollViewViewportTicks` 三个 `DirectProperty` 与 `InvalidatePartViewport()`，fork 新增 `StudioTrackPaletteChangedEvent` / `TrackMuteVisualEvent` 两个监听及 `ClearWaveformBitmap()` 调用，分处不同区段，两侧均完整。注意该文件 ort **删除了 `using System.Reactive.Linq;`**（上游侧删除，因其改动后不再使用），但 fork 新增的监听链继续使用 `.Subscribe(...)`——已由全量构建确认无需该 using（`IObservable<T>` 的 `Subscribe` 扩展来自 `System.Reactive` 而非 `.Linq` 子命名空间），编译零错误。
  - `NotesViewModel.cs`：上游新增 `PianoRollOpenPartChangedEvent` / `PianoRollViewportChangedEvent` 发布与 `PublishPianoRollViewport()`，fork 新增 `StudioTrackPaletteChangedEvent` 监听与 `LoadTrackColor` 的 `StudioTrackPaintCache` 分支；两者改的是不同方法，自动合并后共存。
  - `Preferences.cs` / `PreferencesViewModel.cs` / `PreferencesDialog.axaml`：上游删除 `DiffSingerLocalRetaking`（3 文件各 1–4 行）；fork 在 `Preferences.cs` 新增 119 行、`PreferencesViewModel.cs` 新增 992 行、`PreferencesDialog.axaml` 新增 350 行 Studio/HiFiUTAU/Custom Server 偏好。删除与新增分处不同区段，fork 偏好面完整保留。
  - `PathManager.cs`：上游 +6 行（包体积优化的路径处理），fork +1 行，互不重叠。

### 已验证

1. **构建**：`dotnet build OpenUtau -c Debug` → **0 错误**（1769 条存量警告，与历次同量级；含 3 个既有 AVLN3001）。特别确认 `using DynamicData;` 的删除、`notePen` 的移除、`partsCanvas` 的新增均未引入 CS0246/CS0414。
2. **测试**：`dotnet run --project OpenUtau.Test/OpenUtau.Test.csproj -c Debug` → **Total: 505，Failed: 0，Errors: 0，Skipped: 1**（跳过项仍为需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`）。**此数字是在本合并提交 `57b8caa8` 上测得的**；随后合并 `origin/master`（PR #15 `feature/studio-exp-bar`）得到的 `db62c16a` **不是**这个结果，参见下文「并入 PR #15 之后的复测」。
   - **注意测试命令已随上游变更**：`dotnet test OpenUtau.Test` 在 .NET 10 SDK 上会直接失败（`Microsoft.Testing.Platform` 不再支持 VSTest 目标），必须用上游 `pr-test.yml` 的 `dotnet run --project OpenUtau.Test/OpenUtau.Test.csproj`。上一轮记录（`## Merge 2026-09-12`）中的 `dotnet test` 写法已失效。
   - **用例数 512 → 505 的差额已解释，非覆盖丢失**：上游 `73cd8957` 把 `PhonemizerTest<T>` 抽象基类上的 3 个 `[Fact]`（`CreationTest`/`SetSingerTest`/`DummySingerPhonemizeTest`）× 6 个具体派生类 = **18 个用例**，合并为 1 个 `[Theory]` × 6 个 `[InlineData]` = **6 个用例**，净 **−12**。故合并前应为 517，合并后为 505，且跳过项不变。逐项核对：6 个 phonemizer（`DefaultPhonemizer`/`ArpasingPhonemizer`/`JapaneseCVVCPhonemizer`/`JapaneseVCVPhonemizer`/`KoreanCVCPhonemizer`/`KoreanCVVCPhonemizer`）在 `[InlineData]` 中**一个不少**，且每个用例的断言体（可构造、容忍缺失/null singer、以 dummy note+singer 跑 phonemize）三项合一，覆盖等价。已实测确认：在本合并结果上单跑 `-class "OpenUtau.Plugins.PhonemizerTest"` = **6 个用例全通过**。
   - **本合并提交上未复现 `StringsTest` 的 teardown 问题**（上一轮的已知失败项）：505 用例一次跑完 0 失败。但**该问题在并入 PR #15 之后随即复现**，见下节。

### 并入 PR #15（`origin/master` 2fe1feea）之后的复测

本合并提交 `57b8caa8` 完成后，`origin/master` 已被 PR #15（`feature/studio-exp-bar`）推进到 `2fe1feea`，与本地 `master` **从同一 merge-base `9fdee2f0` 分叉**（不是快进）。经 `git merge-tree` 预演为**干净合并**（唯一重叠文件 `OpenUtau/Views/MainWindow.axaml` 两侧改动位于不同区段：PR #15 改 piano-roll 行高与进度条文案，本合并加上 `PianoRollOpenPart` / `PianoRollViewTickOffset` / `PianoRollViewViewportTicks` 三个绑定），实际合并成功，无冲突标记，结果提交 `db62c16a`。合并面 8 个文件（新增 `OpenUtau/Studio/StudioExpLayout.cs`、`OpenUtau.Test/App/StudioExpLayoutTest.cs`；改 `ExpSelector.axaml`、`PianoRoll.axaml(.cs)`、`StudioStyles.axaml`、`PianoRollViewModel.cs`、`MainWindow.axaml`）。

- **构建**：`dotnet build OpenUtau -c Debug` → **0 错误**。
- **测试（`db62c16a`，共 3 次全量）**：**Total 513–514 / Skipped 1 / Failed 0**，但 **Errors 在 0 与 1 之间摆动**：
  | 运行 | Total | Errors | 失败项 |
  | --- | --- | --- | --- |
  | 1 | 513 | **1** | `AppTest.StringsTest` 的 Test Case Cleanup Failure |
  | 2 | 514 | 0 | — |
  | 3 | 513 | **1** | 同上 |
- **失败性质**：`[Test Case Cleanup Failure (OpenUtau.App.AppTest.StringsTest)] System.InvalidOperationException : The calling thread cannot access this object because a different thread owns it`，栈为 `Avalonia.Rendering.DefaultRenderLoop.Add` ← `ServerCompositor..ctor` ← `Avalonia.Headless.XUnit.AvaloniaTestRunner.Run`，即 **headless 宿主 teardown 阶段的线程亲和性问题**，与断言无关。证据：该用例单独跑（`-method "*StringsTest*"`）**Total 1 / Errors 0**，断言本身确定通过。这与上一轮记录（`## Merge 2026-09-12`）的已知问题**同一现象、同一栈**，本轮在并入 PR #15 后**重新出现**。
- **同基线对照（决定性问题）**：在 `origin/master`（`2fe1feea`）的独立 `git worktree` 上以同一命令跑全量 **两次，均为 Total 521 / Errors 0 / Failed 0 / Skipped 1**，稳定。故该 teardown 摆动**不在 PR #15 自身**，而是由「上游合并 + PR #15」组合后触发（PR #15 新增的 Studio 测试与上游新增的 `NotesViewModel.WhenAnyValue(x => x.Part)` → `PianoRollOpenPartChangedEvent` → `TracksViewModel` 监听这条消息链，与 fork 的 headless 测试会话在同一进程内共存）。对照用的 worktree 已在提交前移除。
- **对本次合并的结论**：`db62c16a` 的**构建与断言全部通过**（0 Failed，`StringsTest` 断言在隔离下通过），但**测试套件在合并树上不再稳定**（Errors 0↔1，Total 513↔514）。按上一轮口径该现象「不阻塞合并」，但**不能声称合并后全量 0 错误**——此处如实记录，并把定位工作列为待办的独立 topic（见「未决项」）。

3. **两侧改动零丢失核对**（对 13 个重叠文件逐条核对，方法沿用上一轮）：分别取「上游侧新增行」与「fork 侧新增行」（base→各自 HEAD 的 `+` 行，去重、忽略 ≤12 字符的短行），在合并结果文件中逐条做字面匹配。结果：上游侧**全部命中**（3 个冲突文件在按上文合成后无遗漏，其中 `PartControl.cs` 的 `notePen` 字段行**有意未取**——见决策记录，属设计取舍而非遗漏；上游高亮块的 22 行全部落地）；fork 侧**全部命中**，包括 `App.axaml.cs` 的 `ApplyStudioStyles`/`developerToolsAttached`、`PartControl.cs` 的 `StudioTrackPaintCache`/`ClearWaveformBitmap`/`paint.NoteThumbnailPen`、`TracksViewModel.cs` 的 `StudioUIChangedEvent` 监听与 `StudioTrackPaintCache.Invalidate`、`PartsCanvas.cs` 的两个新监听。另核对：上游 2 个新增文件（`OpenUtau.Test/Core/USTx/UVoicePartAfterLoadTest.cs`、`OpenUtau/Assets/AppRun`）均存在且 A 状态入库；上游本轮**无删除文件**。PR #15 的 2 个新增文件（`StudioExpLayout.cs`、`StudioExpLayoutTest.cs`）同样在 `db62c16a` 中入库。

4. **`Strings.axaml` 与 22 个语言文件的键完整性**：英文源 **936 键（唯一、无重复）**；`Compare-Object` 核对**上游键缺失数 = 0**、fork 独有键 **130 个全部保留**；上游本轮新增键 **0 个**。22 个语言文件按「非注释键」统计，相对英文源的 `Extra` 仅 zh-CN 的 1 个（即上文孤儿键，已由 `sync_strings.py` 删除），**其余全部为 `Extra=0`**；`Missing` 数只反映未翻译项（以 `<!--<system:String ...>-->` 注释占位，属脚本既定形态，各语言存量不同）。因上游本轮无新增键，本次同步为**净删除 23 行**（22 个语言文件各 1 行 + 英文源 BOM 归一），`Compare-Object` 显示无任何翻译内容丢失。

5. **UTF-8 BOM 回归自查（本 fork 特有，上一轮已记录该风险）**：`.editorconfig` 要求 `charset = utf-8-bom`。逐字节比对合并前 `HEAD` blob 与合并结果，确认：
   - 3 个手工解的冲突文件（`App.axaml.cs`、`PartControl.cs`、`TracksViewModel.cs`）在文本编辑过程中**被剥掉了 BOM**（`HEAD` 有、合并结果无），**已发现并全部补回**，现三者 BOM 与 `HEAD` 一致。
   - `Misc/sync_strings.py` 以无 BOM 编码回写，**会剥掉英文源 `Strings.axaml` 的 BOM**（与上一轮记录一致）。本次运行后**已手动补回**；22 个语言文件本就无 BOM（脚本产物的既定形态，与 `HEAD` 一致），保持不变。
   - 其余自动合并文件（`PartsCanvas.cs` 等）BOM 状态与 `HEAD` 一致，未受影响。
6. **`git diff --check`**：无冲突标记残留（`^(<<<<<<<|=======|>>>>>>>)` 全库扫描为空），`db62c16a` 亦然。
7. **清理**：验证过程产生的临时文件（`merge_probe.txt`、`test_run.txt`、`test_run2.txt`、各次 `*_test*.txt`/`merged_run*.txt`）已删除；对照用的 `git worktree`（`ou-origin-check`）已移除（`git worktree list` 只剩主工作树）。

### 未决项（已知、非阻塞）

- **`AppTest.StringsTest` 的 headless teardown 摆动（需独立 topic 定位）**：现象为 `[Test Case Cleanup Failure]` + `InvalidOperationException: The calling thread cannot access this object because a different thread owns it`，栈落在 `Avalonia.Headless.XUnit.AvaloniaTestRunner.Run` 的清理阶段。已确认的边界：**断言本身确定通过**（隔离运行 1/1）；**`origin/master` 自身稳定 521/0**（跑 2 次）；**仅在「上游合并 + PR #15」的合并树上摆动**（3 次：0、1、1 Errors，Total 513/514）。上一轮（`## Merge 2026-09-12`，基线无 PR #15）已把它记为「累积/顺序敏感」，并已排除：与 `Strings.axaml`/语言文件无关、与 BOM 无关、与 xUnit 并行化无关、与 `App.axaml.cs`/`AppTest.cs`/`ThemeManager.cs` 的差异无关。**本轮新增线索**：上游 `fe0894d3` 在 `NotesViewModel` 加入 `this.WhenAnyValue(x => x.Part).Subscribe(...)` → `MessageBus.SendMessage(new PianoRollOpenPartChangedEvent(...))`，而 `MessageBus` 是进程级静态单例、`TracksViewModel` 又监听同一条消息 → 该订阅链跨越 headless 会话存续且无显式释放，与 PR #15 新增的 Studio 测试并发时命中 teardown 的线程亲和性检查。**建议**：单独开 topic（`fix/headless-test-teardown` 或 `chore/`）定位并修复测试宿主层，不要为迁就测试去改产品代码。
- **xunit 钉版需上游跟进**：本 fork 因 20 个 `[AvaloniaFact]` 用例被迫留在 xunit.v3 3.2.2，与上游 `4.0.0` 存在差异。上游若发布适配（或 Avalonia 发布 xunit.v3 4.x 版 `Avalonia.Headless.XUnit`），应按政策第 1 条改回上游并删除 csproj 内的说明注释。跟踪依据：AvaloniaUI/Avalonia#22072。
- **`xunit.runner.visualstudio` 保留 4.0.0**：该包**没有 3.x 版本**（NuGet 上只有 `4.0.0-pre.*` 与 `4.0.0`），上游与 fork 合并前都写的 `3.2.2` 会触发 `NU1603` 并静默解析到 `4.0.0`。本次已把该行改为诚实的 `4.0.0`，`NU1603` 消失；xunit.v3 核心三包仍钉 `3.2.2`（原因见 csproj 注释）。
- **孤儿键清理已顺手完成**：`prefs.rendering.diffsingerpitchlocalretaking` 的 22 处残留已由 `sync_strings.py` 清除，无需后续处理。

---

## Merge 2026-09-19 37ed7d72

- **时间**（UTC）：`2026-09-19T09:29:19Z`（验证完成时间）
- **合并方式**：`git merge --no-ff upstream/master`（merge-base `81637a33ceeaedd8c407a268ad048e182592508e`，即上次合并记录的上游基线；合并范围 `81637a33..37ed7d72`）
- **上游基线**：`37ed7d721c29c3078c8211fa8e6d3825787446c5` — yaml/presamp watcher (#2423)
- **fork 侧基线**：`16ddf7b8d18825d1801a86e9d9efcfa46e8c0684`（Merge upstream/master (81637a33) into master）

### 引入的上游 commit（11 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `37ed7d721c29c3078c8211fa8e6d3825787446c5` | yaml/presamp watcher (#2423) |
| 2 | `db9065b1dbf2df3c6aadeaaccf7fcd72fe3a2214` | Avalonia Optimization (#2410) |
| 3 | `83e02c7e4a4d9ea5fca72806b2aa27c5382be015` | Publish ClassicSinger oto data as an atomic snapshot |
| 4 | `a8ddc510151fc16993e28774f0d8c4014c104d66` | Obsolete OnAsyncInitStarted/Finished; runner reports init progress |
| 5 | `49daf1ed867d8e1a5ee64485cfe23685618685f5` | Simplify parent expression getters in Phonemizer |
| 6 | `2c283d2b228b64873d0479bee307098627a4f276` | Fix ReadDictionaryAndInit, Parent Alt, Voice color, and ToneShift (#2407) |
| 7 | `bbfb26dd0299390acb0a7f95d8c18b8fd1733eb5` | Update build.yml (#2420) |
| 8 | `6c322550be5c3a7f656c33d7c6ca2dfd02345a55` | Fix/diff singer g2p (#2421) |
| 9 | `6430a2bfbae5190c42f86ee0931c4607bc032424` | Added a process to convert the lyric "ん" to the Roman letter "N" (#2422) |
| 10 | `4941bf21e439710c5b5e7bf43265a42dca440057` | Fix Ctrl+LMB Note Selection & Only Show LyricBox if KeyModifiers is None (#2419) |
| 11 | `2615a257018c1698316154cfb92dd81c525573cf` | Fix line ending |

### 修改面

- 上游侧 62 个文件、fork 侧 132 个文件；**双侧都改过的 14 个文件**（3 个冲突 + 11 个由 ort 自动合并），其余均为单侧改动。合并结果本身引入 61 个文件变更。
- 上游主线：**Avalonia compiled bindings 迁移**（`db9065b1` 把 `OpenUtau/OpenUtau.csproj` 的 `AvaloniaUseCompiledBindingsByDefault` 由 `false` 改为 `true`，并给 40 个 `.axaml` 补 `x:DataType` / `xmlns:vm` / `$parent[Type]` 写法，`PianoRoll.axaml.cs` 同步改代码侧）；**ClassicSinger oto 快照化与目录监视**（`83e02c7e` 整块发布采样数据，`37ed7d72` 新增 `OpenUtau.Core/Classic/PresampWatcher.cs` / `YamlWatcher.cs`，`SyllableBasedPhonemizer.cs` / `PhonemizerTestBase.cs` 跟进）；**Phonemizer API 调整**（`a8ddc510` 废除 `OnAsyncInitStarted/Finished` 改由 runner 报进度、`49daf1ed`、`2c283d2b`）；DiffSinger G2P 与「ん→N」（`6c322550`、`6430a2bf`）；钢琴卷帘 Ctrl+LMB 选择与 LyricBox 可见性（`4941bf21`）；`build.yml`（`bbfb26dd`）；行尾统一（`2615a257`）。

### 冲突（3 个文件）

| 文件 | 上游侧改动量 | fork 侧改动量 | fork 侧来源 | 上游侧来源 |
| --- | --- | --- | --- | --- |
| `OpenUtau/Controls/NotePropertyExpression.axaml` | +2 / −1 | +5 / −7 | `8336fc31da81cad5e273e90300bb01432a144c43`（音符属性面板改用 `c:UnboundedSlider`） | `db9065b1`（compiled bindings：加 `x:DataType`） |
| `OpenUtau/Styles/Styles.axaml` | +7 / −5 | +8 / −2 | `f74120d7d558548f3049fd9db53baaef924084af`（`xmlns:app` + Win32 深色标题栏）、`59f48d5341d86c7edec422cf2abfa656bb535c21`、`f22f3a13ff9611b4575edb7440492268510972ff` | `db9065b1`（加 `xmlns:vm` / `xmlns:notif` 与三处 `x:DataType`） |
| `OpenUtau/Views/TrackSettingsDialog.axaml` | +3 / −1 | +15 / −1 | `fa88ca2c7684bd7bafd774fc8ca7916f07e788eb`（HiFiUTAU / Custom Server 的 Server URL 与 Endpoint 输入行，窗口高度 184→214） | `db9065b1`（加 `x:DataType`；`$parent.WindowDecorationMargin` → `$parent[Window].WindowDecorationMargin`） |

**判定流程取证**（三个文件均按 `AGENTS.md` 的 Merge conflict policy 判定流程执行）：

1. **量化两侧改动**：见上表（`git diff --numstat 81637a33 upstream/master -- <file>` 与 `git diff --numstat 81637a33 master -- <file>`）。
2. **残余差异归属**：合并结果相对上游的残余为 `6/9`、`11/7`、`16/4`，**上游侧与 fork 侧改动量均非 0**，即两侧各有实现，不能按「上游已自行回退」直接归入保留。
3. **归属到 commit 与作者**：三文件的上游侧改动**同源于 `db9065b1`**（compiled bindings 迁移），fork 侧分别来自 `8336fc31` / `f74120d7` 等 / `fa88ca2c`；`git merge-base --is-ancestor` 确认这些 fork commit 不在 `upstream/master` 上，两侧作者无重叠。
4. **逐行核对目标**：上游是在既有结构上**追加绑定类型标注**（`x:DataType`、`$parent[Window]`），fork 是**替换控件实现 / 新增命名空间与面板行**。语义目标不同 → **不构成同目标双实现，不触发政策第 1 条的替换**，改为合成。

### 决策记录

- **`NotePropertyExpression.axaml`：合成**。保留 fork 的 `xmlns:c="using:OpenUtau.App.Controls"` 与 `ColumnDefinitions="143,7,*"`（fork 的 `c:UnboundedSlider` 占第 2 列、ComboBox 跨 2 列，列定义与控件实现绑定，不能换成上游的 `143,7,50,20,*`），同时**采纳上游新增的 `x:DataType="vm:NotePropertyExpViewModel"`**（该 VM 两侧都存在，定义在 `NotePropertiesViewModel.cs`）。上游未改该文件的 code-behind（`git diff 81637a33 upstream/master -- OpenUtau/Controls/NotePropertyExpression.axaml.cs` 为空），fork 的 `slider.InnerSlider` / `InnerEditor` 触点逻辑完整保留。
- **`Styles.axaml`：整块取上游，再补回 fork 独有声明**。冲突块只覆盖文件头的根元素声明与 `<Style>` 缩进：上游加 `xmlns:vm` / `xmlns:notif`（其正文新增的 `x:DataType="vm:MenuItemViewModel"` 与 `x:DataType="notif:Notification"` 必须能解析）并把 `<Style>` 缩进对齐为 4 空格，fork 侧是 `xmlns:app="using:OpenUtau.App"`。按政策第 2 条（尽量减小与上游差异）**取上游整块（含上游保留的 UTF-8 BOM），再补回 fork 必需的 `xmlns:app` 一行**；fork 的 `app:Win32TitleBar.Enabled` setter 与 `GridSplitter.panel` 样式（`f74120d7` / `f22f3a13`）落在正文其它区段，由 ort 自动合并，已核对仍在。
- **`TrackSettingsDialog.axaml`：合成**。**保留 fork 的 `Height="214"`**——fork 在面板中插入了 Server URL / Endpoint 两个输入行与「设为默认」按钮（`fa88ca2c`），窗口必须更高；**采纳上游的 `$parent[Window].WindowDecorationMargin`**——切到 compiled bindings 后 `$parent.X`（无类型限定）在 `x:DataType` 下不可用，这是 `db9065b1` 的适配修正，按政策第 2 条优先采纳上游写法。
- **BOM**：`Styles.axaml` 因取上游侧而从「无 BOM」变为「有 BOM」，与 `.editorconfig` 的 `charset = utf-8-bom` 一致，属回归修正而非偏离；另两个手工解决的文件 BOM 状态与合并前 `HEAD` 一致（逐字节核对）。`DawIntegrationDialog.axaml` 的 fork 侧 BOM 保留。
- **上游本轮未新增字符串键**（`git diff 81637a33 upstream/master -- OpenUtau/Strings/` 为空），因此**未运行 `Misc/sync_strings.py`**，fork 的 130 个独有键与 22 个语言文件均未被触碰。
- **fork 独有功能零删改**：`OnnxNativeAvailability.cs` 及其在 `Util/Onnx.cs`（`initializeDevices()` / `getGpuInfo()` 两处守卫）与 `Util/Preferences.cs`（`GetOnnxRunnerOptionsSafely()`）的插入点在本轮合并中均未受影响（上游本轮未改这两个上游文件）；HiFiUtau / CustomRender / Studio 面亦无冲突。

### 已验证

1. **构建**：`dotnet build OpenUtau -c Debug` → **0 错误**（1782 条警告，与历次同量级；含 3 个既有 `AVLN3001`）。compiled bindings 打开后，40 个上游 `.axaml` 与 fork 的 Studio 控件/对话框一起通过 XAML 编译。
2. **测试**：`dotnet test OpenUtau.Test -c Debug` 全量跑 **两次**，均为 **Total 515 / 通过 514 / 失败 0 / 跳过 1**（跳过项仍为需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`）。本轮**未复现**上一轮记录的 `AppTest.StringsTest` headless teardown 摆动（2/2 干净）。
   - **用例数账目闭合**：505（上一轮合并提交，见 `## Merge 2026-09-16 81637a33`）→ 513–514（并入 PR #15 后，同记录）→ **515（本轮）**；上游本轮新增用例**恰为 1 个**（`83e02c7e` 新增 `OpenUtau.Test/Classic/ClassicSingerTest.cs`，内含 1 个 `[Fact]`），`git diff 81637a33 upstream/master -- OpenUtau.Test/` 对该目录只有该新增文件与两处删除（`EnVCCVTest.cs` / `PhonemizerTestBase.cs` 各删一行 `phonemizer.Testing = true;`，非用例），**无用例删除**。
3. **UI 冒烟**：直接启动合并结果 `OpenUtau.exe`（Debug）→ **20 秒内窗口正常存活、stdout/stderr 无输出**（启动期 XAML：`App.axaml` / `Styles.axaml` / `MainWindow` / `PianoRoll` 全部加载成功），随后主动结束进程。
4. **两个手工解决文件的运行时实加载验证**（临时 `[AvaloniaFact]`，验证后已删除）：在 headless Avalonia 会话中构造 `TrackSettingsDialog`（传入自建 `UProject` 的 `UTrack`）与 `NotePropertyExpression`（`DataContext` 为数值型 `NotePropertyExpViewModel`），两者均能 `Show()`，并能按名字取到 `slider`（运行时类型为 fork 的 `UnboundedSlider`）与 `comboBox` → **2 通过 / 0 失败**。这两处正是本轮做「合成」取舍的位置。
5. **双侧零丢失核对**（14 个双侧重叠文件，方法沿用上一轮：取 base→各自 HEAD 的 `+` 行、忽略 ≤12 字符短行，在合并结果中逐条字面匹配）：**上游侧 49 行、fork 侧 1160 行全部命中，0 丢失**。比对中出现的 2 处「未命中」经复核为读取时 `utf-8-sig` 吞 BOM 与行尾造成的**假阳性**（`xmlns:app` 行现处声明块中段故结尾不同；`DawIntegrationDialog.axaml` 首行 BOM 实际存在，已用字节比对确认 `ef bb bf`）。
6. **`git diff --check`**：无冲突标记残留、无空白错误；三个手工解决文件的 BOM 状态已逐字节核对。
7. **清理**：临时 `OpenUtau.Test/App/MergeXamlSmokeTest.cs` 已删除；临时 diff 快照（`%TEMP%/oumerge`）与冒烟脚本（`%TEMP%/ou-smoke.*`）已移除；工作树除本次合并外无残留。

### 未决项（已知、非阻塞）

- **`AppTest.StringsTest` 的 headless teardown 摆动**：上一轮记录的偶发 `[Test Case Cleanup Failure]`（`AvaloniaTestRunner` 清理阶段线程亲和性）在本轮两次全量中**均未复现**。仍按「顺序敏感、需独立 topic 定位」跟踪；若再次出现，按 `## Merge 2026-09-16 81637a33` 的未决项继续处理。
- **xunit 钉版**：本轮上游未改 `OpenUtau.Test.csproj`（`bbfb26dd` 只改 `.github/workflows/build.yml`），fork 的 `xunit.v3` 3.2.2 钉版与 csproj 内说明注释维持原判。
- **`$parent.` 无类型写法**：`TrackSettingsDialog.axaml` 已随上游改用类型限定；`PianoRoll.axaml` / `MainWindow.axaml` 的 `$parent.Bounds.Height` 是**上游自身仍在用**的写法且构建通过，本轮不动（减小与上游差异）。

---

## Merge 2026-09-24 48a308b4

- **时间**（UTC）：`2026-09-23T16:15:48Z`（= 本地 2026-09-24 凌晨 +0800；验证完成时间）
- **合并方式**：`git merge --no-commit --no-ff upstream/master`（merge-base `37ed7d721c29c3078c8211fa8e6d3825787446c5`，即上次合并记录的上游基线；合并范围 `37ed7d72..48a308b4`）
- **上游基线**：`48a308b4839b701fc61f2cf1302f7e4ac577c7f5` — Revert "WorldlineResampler: Expand Expression Support (#2426)" (#2436)
- **fork 侧基线**：`0081c73d`（Merge pull request #18 from machinacanis/fix/ci-cross-platform-tests；相对 merge-base 另有 98 个 fork commit，已全部落在 `origin/master` 上）
- **重做说明（可追溯）**：本轮上游合并先在 fork 基线 `3d0234b0` 上完成过一次（结果提交 `3659f587`，内容与下述一致，仅缺 PR #18 的 5 行）。推 `origin` 前 `git fetch origin` 发现 `origin/master` 已前进到 `0081c73d`（PR #17 `feature/studio-exp-overlay`、PR #18 `fix/ci-cross-platform-tests`）。因 `3d0234b0` 是 `0081c73d` 的祖先，为免多一个无谓 merge commit、也免把「上游同步」与「追平 origin」搅成两件事，改用 `git reset --hard origin/master` 后在 `0081c73d` 上**重做同一次上游合并**，并以重做后的结果做全部验证（下述数字均在最终树上实测，与旧结果无关）。

### 引入的上游 commit（5 个，线性）

| # | SHA | 主题 | 作者 |
| --- | --- | --- | --- |
| 1 | `26eead420ec6339018015d37d524d2f9e59332ef` | Update TrackHeader.axaml (#2427) | Anjo |
| 2 | `d9ef2ad96d5705db1df7c4c6fb521a753f799e51` | WorldlineResampler: Expand Expression Support (#2426) | 黒猫大福 |
| 3 | `475bd6015569e6392b49250fb1564bdc5734dbf6` | Welcome page fixes (#2433) | Mashi |
| 4 | `3306fdf603cbbef374ca73dede3ec07c69a888cb` | Add presamp test and bug fixes (#2430) | Maiko |
| 5 | `48a308b4839b701fc61f2cf1302f7e4ac577c7f5` | Revert "WorldlineResampler: Expand Expression Support (#2426)" (#2436) | StAkira |

**净效果**：`d9ef2ad9` 与 `48a308b4` 互为「加 / 回退」，`git diff 37ed7d72 upstream/master -- OpenUtau.Core/Classic/WorldlineResampler.cs` 与合并结果对该文件的 diff **均为空** → 上游侧该文件净改动为 **0**，文件与 base、与上游逐字节一致；fork 侧也从未改过它，故无取舍、无风险。

### 修改面

- 上游本轮只动 **9 个文件**：`OpenUtau.Core/Classic/Presamp.cs`（+86 / −66）、`OpenUtau/Controls/TrackHeader.axaml`（+2 / −2）、`OpenUtau/Views/MainWindow.axaml.cs`（+38 / −14）、新增 `OpenUtau.Test/Classic/PresampIniTest.cs`（114 行）与 5 个夹具 `OpenUtau.Test/Files/presampini/{default,xsampa,tricky_symbols,blank,empty}/presamp.ini`。
- **双侧都改过的只有 2 个文件**（`TrackHeader.axaml`、`MainWindow.axaml.cs`）；其余 7 个文件 fork 侧改动量为 0，逐字节取上游。
- 上游本轮**未新增/修改任何字符串键**（`git diff 37ed7d72 upstream/master -- OpenUtau/Strings/` 为空）。
- fork 独有面（Studio UI、HiFiUTAU、CUSTOM_SERVER、DAW 集成）**没有一个文件**被本轮上游改动触及。

内容概要：

- **presamp 解析修复 + 测试**（`3306fdf6`）：`VCPAD`/`VCVPAD` 空值不再覆盖默认；补 `[VCPAD]` 节处理与 `%num%/%append%/%pitch%` 组合的合法性校验；`AliasPriorityDefault` 满 5 条时改为「先清空再添加」（原实现会持续增长）；`[VERSION]`/`[LOCALE]`/`[RESAMP]`/`[TOOL]`/`[BATNUM]` 显式忽略；`MakePhonemeList` 注释与几处整理。新增 6 个 `[Fact]`，覆盖 default / xsampa / tricky_symbols / blank / empty / 文件不存在 六种输入。
- **欢迎页修正**（`475bd601`）：启动不再自动建工程（构造函数删除 `viewModel.NewProject()`）；`OnKeyDown` 拆为 `GlobalHotkey` + `EditorHotkey`，编辑类快捷键加 `viewModel.Page == 1` 门控（欢迎页禁用）。
- **TrackHeader 滑条模板绑定修正**（`26eead42`）：`Slider.fader` 在 `:pointerover` / `:pressed` 下 Thumb 背景改用 `$parent[Slider].((vm:TrackHeaderViewModel)DataContext).TrackColor.*`（compiled bindings 下的类型限定写法）。

### 冲突

**无**。两个双侧重叠文件均由 ort 自动合并，未出现冲突标记（全库 `^(<<<<<<<|=======|>>>>>>>)` 扫描为空）。

| 文件 | 上游侧改动量（base→上游） | fork 侧改动量（base→fork） | 合并结果 vs 上游 | 合并结果 vs fork | 上游侧来源 | fork 侧来源 |
| --- | --- | --- | --- | --- | --- | --- |
| `OpenUtau/Controls/TrackHeader.axaml` | +2 / −2 | +30 / −20 | +30 / −20（= 合并前 fork delta） | +2 / −2（= 上游 delta） | `26eead42`（滑块模板内 Thumb 绑定，第 42/50 行） | `55615237`、`8d04547c`、`4fcfe157`、`464705a1`、`a0172184`、`c837526a`（Studio 色条 / 徽标 / 按钮类名，第 104 行起） |
| `OpenUtau/Views/MainWindow.axaml.cs` | +38 / −14 | +24 / −12 | +24 / −12（= 合并前 fork delta） | +38 / −14（= 上游 delta） | `475bd601` | `53b2417f`（Tools 菜单 DAW 项）、`9fd108be`（`HandleGlobalShortcut` 公开入口 + `MixFxWindowManager`）、`b4295a6b`（`Singer.Subbanks` 空值守卫，第 1950 行附近） |

### 判定流程取证（两个重叠文件，按 `AGENTS.md` 的 Merge conflict policy 判定流程执行）

1. **量化两侧改动**：见上表（`git diff --numstat 37ed7d72 upstream/master -- <file>` 与 `git diff --numstat 37ed7d72 origin/master -- <file>`）。
2. **看残余差异由谁贡献**：合并结果对 `upstream/master` 的残余为 +30 / −20 与 +24 / −12，**两侧改动量均非 0**，不属于「上游已自行回退、无上游实现」的情形，必须逐段判定。
3. **归属到 commit 与作者**：`git log --oneline 37ed7d72..origin/master -- <file>` 逐条归属；上游侧分别只来自 `26eead42`（Anjo）与 `475bd601`（Mashi），fork 侧来自上表所列 fork commit，`git merge-base --is-ancestor` 确认这些 commit 均不在 `upstream/master` 上，作者无重叠。
4. **逐行核对目标**：
   - `TrackHeader.axaml`：上游只改**滑块模板内部两行 Thumb 背景绑定的写法**（第 42/50 行），fork 改的是**头像面板 / 色条 / 轨道号徽标 / 按钮类名**（第 104 行起）——两份 diff 的行段完全不相邻，无重叠面。**非同目标双实现，两侧全部采纳**。
   - `MainWindow.axaml.cs`：上游重构的是**快捷键分层与欢迎页门控**（`GlobalHotkey` / `EditorHotkey` / `Page == 1`）并删除启动建工程；fork 做的是**把自己的对话框接进主窗口快捷键**（把 `OnKeyDown` 体抽成公开入口 `HandleGlobalShortcut` 供 `MixFxDialog` 转发）、DAW 菜单项、`MixFxWindowManager` 生命周期与 `Subbanks` 空值守卫。目标不同（上游=分层与欢迎页行为，fork=给非模态 FX 窗口复用快捷键等），不触发政策第 1 条的替换；按第 2 条「尽量减小与上游差异」合成，入口名保留 fork 的。
5. **集合类差异**：本轮无（无 `Strings.axaml` / 语言文件改动）。
6. **结论**：两文件均无替换、按合成处理，并已逐侧清点零丢失（见「已验证」第 4 条）。

### 决策记录

- **`OpenUtau.Core/Classic/Presamp.cs`：逐字节取上游**。fork 侧改动量为 0，无取舍；配套新测试与 5 个 `presamp.ini` 夹具一并入库（`OpenUtau.Test.csproj` 的 `Files\**` 通配已覆盖新目录，无需改 csproj）。
- **`OpenUtau/Controls/TrackHeader.axaml`：两侧全取**。上游 `$parent[Slider]` 限定写法是 compiled bindings 迁移（`db9065b1`，见 `## Merge 2026-09-19 37ed7d72`）的后续修正，必须采纳；fork 的 Studio 色条 / 轨道号徽标 / `.s1` 按钮类名落在第 104 行以后，按政策第 3 条作为 fork 独有功能原样保留。
- **`OpenUtau/Views/MainWindow.axaml.cs`：取上游结构 + 保留 fork 公开入口**。具体合成：
  - 采纳上游的 `GlobalHotkey(args)` 与 `if (viewModel.Page == 1) { EditorHotkey(args); }`（欢迎页禁用编辑快捷键），以及 `viewModel.TracksViewModel.*` 调用点写法；
  - 保留 fork 的 `void OnKeyDown(...) => HandleGlobalShortcut(args);` 与 `public void HandleGlobalShortcut(KeyEventArgs args)`：入口体内就是上游那三行（焦点守卫 + 分组调度），因此 `MixFxDialog.axaml.cs` 中 `mainWindow.HandleGlobalShortcut(args)`（`9fd108be` 的非模态 FX 窗口转发）语义与上游 `OnKeyDown` 完全一致，并自动继承欢迎页门控；
  - 上游删除的 `viewModel.NewProject()` **按上游采纳**（政策第 1 + 2 条）：欢迎页 UI（`MainWindow.axaml` 的 `Carousel SelectedIndex="{Binding Page}"`）两侧一致存在，不自动建工程即上游本轮的设计意图；该 axaml 的 fork 侧 delta 仅 6/5 行（Tools 菜单 DAW 项、面板分隔条、表达式条），与欢迎页无关；
  - fork 的 `OnMenuDawIntegration` 位置、`MixFxWindowManager.CloseAll()` / `CloseFor(removeTrack.track)` 与 `Singer.Subbanks` 空值守卫都落在自动合并区段，逐条核对仍在（零丢失检查覆盖）。
- **fork 独有功能零删改**：本轮合并面 9 个文件中没有 fork 独有文件；Studio UI、HiFiUTAU、CUSTOM_SERVER、DAW 集成与其偏好代码均未被触碰。
- **字符串脚本**：上游本轮无键变更，未产生翻译同步需求。仍执行一次 `python Misc/sync_strings.py`，其唯一副作用是**剥离英文源 `Strings.axaml` 的 BOM**（脚本以无 BOM 回写，历轮已记录该行为），与合并内容无关，已 `git checkout --` 还原 → `Strings.axaml` 与 22 个语言文件**零变化**。

### 已验证

1. **构建**：`dotnet build OpenUtau -c Debug` → **0 错误 / 0 警告**（增量）；测试运行对 `OpenUtau` 与 `OpenUtau.Test` 做了完整编译，同样 **0 错误**（警告为存量分析器提示 `xUnit1051` / `xUnit1031` / `xUnit2013` 等，与本次合并无关，本轮未新增）。
2. **测试（合并结果）**：`dotnet test OpenUtau.Test` → **Total 476 / 通过 475 / 失败 0 / 跳过 1**（跳过项仍为需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`）。本轮**未复现** `AppTest.StringsTest` 的 headless teardown 摆动。
3. **用例数账目闭合（同机、同命令对照）**：在 fork 基线 `0081c73d` 的独立 `git worktree`（已移除）上跑全量 → **Total 470 / 通过 469 / 失败 0 / 跳过 1**；合并后 476，**差额 +6** 恰为上游 `3306fdf6` 新增 `OpenUtau.Test/Classic/PresampIniTest.cs` 的 6 个 `[Fact]`（上游本轮对 `OpenUtau.Test/` **只有新增、无用例删除**）。另单独执行 `dotnet test OpenUtau.Test --filter "FullyQualifiedName~PresampIniTest"` → **6 通过 / 0 失败 / 0 跳过**，确认新用例在合并树上真实执行。
4. **双侧零丢失核对**（两个重叠文件，方法沿用历轮：取 base→各自 HEAD 的 `+` 行、忽略 ≤12 字符短行，在合并结果中逐条字面匹配）：上游侧 `TrackHeader.axaml` 2 行 + `MainWindow.axaml.cs` 25 行、fork 侧 25 行 + 16 行，**全部命中，0 丢失**；交叉核对的数字（合并结果对上游 = 合并前 fork delta，对 fork = 上游 delta）逐项吻合。
5. **BOM 核对**：`Presamp.cs` / `MainWindow.axaml.cs` / `PresampIniTest.cs` 在 HEAD、上游、工作树三处均为 UTF-8 BOM；`TrackHeader.axaml` 两侧一致无 BOM。本轮无手工解冲突，未出现历轮记录过的「编辑过程剥 BOM」问题。
6. **`git diff --check`**：无空白错误；冲突标记全库扫描为空。
7. **UI 冒烟（本轮改了 `MainWindow` 构造函数与启动行为，故必跑）**：以 `hub start` 直接启动合并结果的 `OpenUtau.exe`（Debug）→ **存活 34.6s、stdout/stderr 全程无输出**后主动终止；再以 PowerShell `Start-Process` 启动一次 → 20s 后进程存活、`MainWindowTitle = "OpenUtau v0.0.0.0"`、工作集 207MB，`CloseMainWindow()` 后确认进程已退出（`hasExitedAfterClose=True`，无异常弹窗、无崩溃）。两次启动均未出现欢迎页相关异常。
8. **清理**：对照用 `git worktree`（`../ou-baseline`）已移除（`git worktree list` 仅剩主工作树）；无临时文件残留。

### 未决项（已知、非阻塞）

- **`AppTest.StringsTest` 的 headless teardown 摆动**：本轮合并树与 fork 基线两次全量各 1 次、均未复现。仍按「顺序敏感、需独立 topic 定位」跟踪。
- **xunit 钉版**：上游本轮未改 `OpenUtau.Test.csproj`，fork 的 `xunit.v3` 3.2.2 钉版与 csproj 内说明注释维持原判（同 `## Merge 2026-09-16 81637a33`）。
- **`WorldlineResampler` 表达式支持（#2426）已被上游自我回退**：本 fork 从未跟进该特性，无需跟进；若上游后续以修正版重提，按新 commit 正常合并。

---

## Merge 2026-09-24 5d17f141

- **时间**（UTC）：`2026-09-24T06:38:59Z`（= 本地 2026-09-24 午后 +0800；验证完成时间）
- **合并方式**：`git merge --no-commit --no-ff upstream/master`（merge-base `48a308b4839b701fc61f2cf1302f7e4ac577c7f5`，即上次合并记录的上游基线；合并范围 `48a308b4..5d17f141`）
- **上游基线**：`5d17f141` — Serialize SharpWavtool cache file reads per path (#2442)
- **fork 侧基线**：`eb3cab20`（Merge upstream/master (48a308b4) into master）

### 引入的上游 commit（8 个，线性）

| # | SHA | 主题 | 作者 |
| --- | --- | --- | --- |
| 1 | `3dc7ff51` | Fix Rider errors (#2440) | Anjo |
| 2 | `e7b65e54` | Remove redundant Update call in PartsCanvasPointerReleased (#2439) | StAkira |
| 3 | `5bebe0ce` | Add support for native Linux and MacOS wavtools (#1618) | Mashi |
| 4 | `e9141c2a` | Migrate to slnx (#2432) | Anjo |
| 5 | `8e9e521a` | Generalize DiffSinger bar-style phoneme display to renderers that ignore envelopes (#2441) | 黒猫大福 |
| 6 | `0b8d72b7` | WorldlineResampler: Expression Support_2 (#2437) | 黒猫大福 |
| 7 | `9a2bc125` | Pin Windows DirectML back to 1.23.0 to stop the native crash on session creation (#2443) | Kakaru |
| 8 | `5d17f141` | Serialize SharpWavtool cache file reads per path (#2442) | StAkira |

（`0b8d72b7` 即上一轮被上游自行回退的 #2426 的重提版，见 `## Merge 2026-09-24 48a308b4` 的未决项。）

### 修改面

- 上游 22 个文件（+193 / −103），含 1 个删除（`OpenUtau.sln`）与 2 个新增（`OpenUtau.slnx`、`OpenUtau.Core/Classic/UnixWavtool.cs`）。
- **双侧都改过的 5 个文件**：1 个冲突（`OpenUtau.Core/Util/Preferences.cs`）+ 4 个由 ort 自动合并（`OpenUtau.Core/OpenUtau.Core.csproj`、`OpenUtau/Controls/TrackHeader.axaml`、`OpenUtau/ViewModels/NotesViewModel.cs`、`OpenUtau/Views/MainWindow.axaml.cs`）。
- 上游本轮**未新增/修改字符串键**（`git diff 48a308b4 upstream/master -- OpenUtau/Strings/` 为空），**未新增/删除任何测试用例**（`-- OpenUtau.Test/` 为空）。
- fork 独有面（Studio UI、HiFiUTAU、CUSTOM_SERVER、DAW 集成）**没有一个文件**被本轮上游改动触及。

内容概要：

- **原生 Linux/macOS wavtool**（`5bebe0ce`）：新增 `UnixWavtool`，`ToolsManager` 在非 Windows 下把 `.sh` / 无扩展名的 wavtool 交给它（原来错给 `ExeWavtool`）；`IResampler` 加 `NoWrapperScript`，`ExeResampler` 的 wine 判定由「非 Windows 且扩展名是 .exe/.bat」改为「有 WinePath 且需要 wrapper」；`Preferences.Load()` 在 Windows 上清空 `WinePath`。
- **缓存读锁**（`5d17f141`）：`SharpWavtool` 读 `item.outputFile` 前 `lock (Renderers.GetCacheLock(...))`，与 `ClassicRenderer` 的写锁同键，消除并发渲染共用 resample 缓存时的「文件被占用」；`WorldlineRenderer` 改用同一个 `Renderers.GetCacheLock`。
- **WorldlineResampler 表达式支持**（`0b8d72b7`）：`Manifest` 声明 `ten` / `brea` / `voi`。
- **音素包络显示泛化**（`8e9e521a`）：`IRenderer` 新增 `SupportsPhonemeEnvelope => true`；DiffSinger / Enunu / Voicevox 声明 `false`；`PhonemeUIRender.IsDiffSinger(part)` 改为 `SupportsPhonemeEnvelope(part)`（读 `track.RendererSettings.Renderer`），`PhonemeCanvas` 与 `NotesViewModelHitTest` 据此决定画「条」还是画包络、以及是否命中包络把手。
- **slnx 迁移**（`e9141c2a`）：删 `OpenUtau.sln`、加 `OpenUtau.slnx`（同样的 4 个项目 + Solution Items）。
- **DirectML 钉回 1.23.0**（`9a2bc125`）：1.24.x 的 DirectML 在创建 `InferenceSession` 时可原生杀进程（无托管异常、无日志），1.23.0 稳定。
- **Rider 修正**（`3dc7ff51`）：`TrackHeader.axaml` 去掉上一轮 #2427 多写的一个 `}`（`AccentColorLight}}` → `AccentColorLight}`），`NotesViewModel` 的 `WhenAnyValue(x => x.Part)` 包上 `ObservableMixins.WhereNotNull(...)`。
- **`e7b65e54`**：删掉 `PartsCanvasPointerReleased` 里冗余的 `partEditState.Update(point.Pointer, point.Position)`。

### 冲突（1 个文件）

| 文件 | 上游侧改动量（base→上游） | fork 侧改动量（base→fork） | 合并结果 vs 上游 | 合并结果 vs fork | 上游侧来源 | fork 侧来源 |
| --- | --- | --- | --- | --- | --- | --- |
| `OpenUtau.Core/Util/Preferences.cs` | +1 / −0 | +119 / −1 | +119 / −1（= 合并前 fork delta） | +1 / −0（= 上游新增行） | `5bebe0ce`（`Load()` 里新增 `if (OS.IsWindows()) Default.WinePath = string.Empty;`） | `c89b9623`（ONNX runner 守卫 `GetOnnxRunnerOptionsSafely()`）、Studio / HiFiUTAU / Custom Server 偏好面（`c29b2b10`、`a65e4bac`、`24610b1c`、`b4295a6b` 等） |

其余 4 个双侧重叠文件由 ort 自动合并，两侧改动行段不相邻（明细见下表），无冲突标记。

### 判定流程取证（按 `AGENTS.md` 的 Merge conflict policy 判定流程执行）

1. **量化两侧改动**：见上表与下表（`git diff --numstat 48a308b4 upstream/master -- <file>` 与 `git diff --numstat 48a308b4 HEAD -- <file>`）。
2. **看残余差异由谁贡献**：`Preferences.cs` 合并结果对上游的残余为 +119 / −1，**两侧改动量均非 0**，不属于「上游已自行回退、无上游实现」的情形。
3. **归属到 commit 与作者**：`git log --oneline 48a308b4..upstream/master -- <file>` 对每个文件都**只有一个**上游 commit（见下表），作者与 fork 侧无重叠；fork 侧来源用 `git log --oneline 48a308b4..HEAD -- <file>` 与 `git merge-base --is-ancestor` 确认。
4. **逐行核对目标**：上游对 `Preferences.cs` 只加 **1 行**——在既有 `Load()` 的「加载后校正」框架里追加一条「Windows 上清空 WinePath」的迁移，属于「在既有框架里追加一行」；fork 侧是**新增偏好面**（Studio 外观 / HiFiUTAU / Custom Server）与**ONNX runner 守卫**。两者目标不同 → **不构成同目标双实现，不触发政策第 1 条的替换**，取并集。
5. **集合类差异**：本轮无（无 `Strings.axaml` / 语言文件改动）。
6. **结论**：`Preferences.cs` 取并集；其余 4 个自动合并文件两侧全取（见「已验证」第 4 条的零丢失核对）。

### 决策记录

- **`Preferences.cs`：并集**。保留 fork 的 `GetOnnxRunnerOptionsSafely()`（fork 的 ONNX 原生可用性守卫，`c89b9623`；上游无对应实现，属政策第 3 条的 fork 独有功能），同时采纳上游新增的 `if (OS.IsWindows()) Default.WinePath = string.Empty;`。合成后该行紧邻排布，语义互不干扰（一个校验 ONNX runner 选项、一个清理 Windows 下的 WinePath）。
- **`OpenUtau.Core.csproj`：两侧都取**。fork 的 `Newtonsoft.Json 13.0.3` 注释块（HiFiUTAU / Custom Server 的 phrase JSON 仍用 Newtonsoft）与上游的 `Microsoft.ML.OnnxRuntime.DirectML` 钉回 `1.23.0`（含上游写明原因的注释）共存，已核对两处 `PackageReference` 都在。上游的钉版对 fork **同样必要**：fork 的 HiFiUTAU 亦走 ONNX，`OnnxNativeAvailability` 守卫针对的正是同一类「创建 session 时原生杀进程」现象。
- **`IRenderer.SupportsPhonemeEnvelope`：fork 渲染器不加覆写，保持默认 `true`**（政策第 2/3 条：不在上游文件里塞 fork 分支）。取证：
  - `OpenUtau.Core/HiFiUtau/HifiUtauPhraseJson.cs`（第 85 行读 `phone.envelope`、第 109–149 行读 `nextPhone.envelope` 并输出 `envelope.p0..p4`）与 `OpenUtau.Core/CustomRender/CustomPhraseJson.cs`（第 73 / 95–134 行同构）**都消费包络点**；`HifiF0Utils.cs` / `CustomF0Utils.cs` 还用 `envelope[0].X` 反推 preutter。
  - 即 fork 的两个渲染器**使用**包络，与上游标 `false` 的 DiffSinger / Enunu / Voicevox（忽略包络）语义相反；改标 `false` 会让钢琴卷帘把 fork 渲染器的音素画成条形并屏蔽包络把手拖拽，属功能回归。
  - 运行时佐证见「已验证」第 3 条（HIFIUTAU / CUSTOM_SERVER / WORLDLINE-R / CLASSIC = `true`，DIFFSINGER / ENUNU / VOICEVOX = `false`，7/7 通过）。
- **`IResampler.NoWrapperScript`：无需 fork 适配**。取证：全库 `: IResampler` 实现只有 `ExeResampler` 与 `WorldlineResampler` 两个（均为上游文件，上游本轮已改），fork 未新增实现类。
- **`OpenUtau.sln` → `OpenUtau.slnx`：采纳**。取证：fork 全库无 `OpenUtau.sln` 引用（对 `*.yml` / `*.yaml` / `*.md` / `*.csproj` / `*.ps1` / `*.sh` 的 `grep -rn "OpenUtau\.sln"` 为空）；CI `.github/workflows/pr-test.yml` 走 `dotnet restore OpenUtau -r <rid>` 与 `dotnet run --project OpenUtau.Test/OpenUtau.Test.csproj`，不含 sln；`OpenUtau.slnx` 列的 4 个项目与 fork 完全一致（含 `BuildDependency`）。本机实测 `dotnet build OpenUtau.slnx -c Debug` → 0 错误。
- **`TrackHeader.axaml`：采纳上游修正**。上游 `3dc7ff51` 修掉的是上一轮 `26eead42`（#2427）自己多写的 `}`；上一轮我们按政策取了上游的 `}}`，本轮一并跟随上游改回单 `}`，fork 的 Studio 区段（第 104 行起）未动。
- **`MainWindow.axaml.cs`：采纳上游删除冗余 `Update` 调用**（`e7b65e54`，位于 fork 从未改动的 `PartsCanvasPointerReleased`）；fork 的 DAW 菜单 / `HandleGlobalShortcut` / `MixFxWindowManager` / `Subbanks` 守卫均不受影响。
- **BOM 回归修正（fork 侧既有偏差）**：合并后 `OpenUtau.Core/OpenUtau.Core.csproj` 与 `OpenUtau/ViewModels/NotesViewModel.cs` 在 fork 基线上**缺 UTF-8 BOM**（上游两侧都有），使两者与上游各多出 1 行 diff 且违反 `.editorconfig` 的 `charset = utf-8-bom`；已补回 BOM，两文件对上游的差异回到纯语义面（csproj 现为 +4 / −0 = 纯 fork 的 Newtonsoft 注释与引用；`NotesViewModel.cs` 现为 +20 / −6 = 纯 fork delta）。其余合并面文件 BOM 与两侧一致（`TrackHeader.axaml` 上游本就无 BOM，`UnixWavtool.cs`、`OpenUtau.slnx` 同上游）。
- **字符串脚本**：上游本轮无键变更，未产生同步需求。仍执行一次 `python Misc/sync_strings.py` 确认——唯一副作用是**剥离英文源 `Strings.axaml` 的 BOM**（脚本以无 BOM 回写，历轮已记录），与合并内容无关，已 `git checkout --` 还原 → `Strings.axaml` 与 22 个语言文件**零变化**。

### 已验证

1. **构建**：`dotnet build OpenUtau -c Debug` → **0 错误**（1784 条警告，与历次同量级，上一轮 1782；含既有 `CS0649` / `CS0414` / `xUnit1051` 等存量提示）；`dotnet build OpenUtau.slnx -c Debug`（迁移后的解决方案文件，本机 .NET 10 SDK）→ **0 错误**。
2. **测试**：`dotnet test OpenUtau.Test` → **Total 476 / 通过 475 / 失败 0 / 跳过 1**（跳过项仍为需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`）。本轮**未复现** `AppTest.StringsTest` 的 headless teardown 摆动。
3. **用例数账目闭合（同机、同命令对照，独立 `git worktree`，已移除）**：fork 基线 `eb3cab20` 跑全量 → **Total 476 / 通过 475 / 失败 0 / 跳过 1**；合并后同为 476，**差额 0**——与「上游本轮对 `OpenUtau.Test/` 无任何文件改动」一致（`git diff --stat 48a308b4 upstream/master -- OpenUtau.Test/` 为空），无用例增减。
4. **渲染器包络能力运行时核对（临时 xUnit 用例，验证后已删除）**：对 `Renderers.GetOrCreate(<id>).SupportsPhonemeEnvelope` 断言 → `HIFIUTAU` / `CUSTOM_SERVER` / `WORLDLINE-R` / `CLASSIC` = `true`，`DIFFSINGER` / `ENUNU` / `VOICEVOX` = `false`，**7 通过 / 0 失败**。这条正是本轮唯一需要判断「fork 是否为上游新语义做适配」的位置，结论是**不需要**（见决策记录）。
5. **双侧零丢失核对**（5 个重叠文件，方法沿用历轮：取 base→各自 HEAD 的 `+` 行、忽略 ≤12 字符短行，在合并结果中逐条字面匹配）：**上游侧 12 行、fork 侧 177 行全部命中，0 丢失**。逐文件数字见下表：

| 文件 | 上游侧（base→上游） | fork 侧（base→fork） | 合并结果 vs 上游 | 合并结果 vs fork |
| --- | --- | --- | --- | --- |
| `OpenUtau.Core/Util/Preferences.cs` | +1 / −0 | +119 / −1 | +119 / −1 | +1 / −0 |
| `OpenUtau.Core/OpenUtau.Core.csproj` | +9 / −1 | +5 / −1 | +4 / −0 | +10 / −2（含 BOM 补回） |
| `OpenUtau/Controls/TrackHeader.axaml` | +1 / −1 | +30 / −20 | +30 / −20 | +1 / −1 |
| `OpenUtau/ViewModels/NotesViewModel.cs` | +1 / −1 | +21 / −7 | +20 / −6 | +2 / −2（含 BOM 补回） |
| `OpenUtau/Views/MainWindow.axaml.cs` | +0 / −1 | +24 / −12 | +24 / −12 | +0 / −1 |

（`NotesViewModel.cs` 的 fork delta 由 +21 / −7 变为 +20 / −6，差额 1/1 即 BOM 补回那行；上游新增的 `ObservableMixins.WhereNotNull(...)` 与 fork 的 `StudioTrackPaletteChangedEvent` 监听并存，已逐行确认。）

6. **BOM 核对**：本轮合并面 7 个文件逐个与两侧比对——`Preferences.cs`、`MainWindow.axaml.cs` 三处（HEAD / 上游 / 工作树）均为 UTF-8 BOM；`TrackHeader.axaml` 两侧本就无 BOM；上游新增的 `UnixWavtool.cs` 与 `OpenUtau.slnx` 与上游一致（无 / 无）；`OpenUtau.Core.csproj` 与 `NotesViewModel.cs` 在 fork 基线缺 BOM，已按 `.editorconfig` 的 `charset = utf-8-bom` 补回（见决策记录）。
7. **`git diff --check`**：无空白错误；冲突标记（`^(<<<<<<<|>>>>>>>)`）全库扫描为空（含索引态）。
8. **UI 冒烟（本轮改了钢琴卷帘音素绘制与轨道头 XAML，故必跑）**：以 PowerShell `Start-Process` 直接启动合并结果的 `OpenUtau.exe`（Debug）两次 → 两次均 **存活 30s**、`MainWindowTitle = "OpenUtau v0.0.0.0"`、工作集 221MB / 209MB、**stdout 与 stderr 全为空**，`CloseMainWindow()` 后均确认进程已退出（`hasExitedAfterClose=True`，无异常弹窗、无崩溃）。
9. **清理**：对照用 `git worktree`（`../ou-baseline`）已移除（`git worktree list` 仅剩主工作树）；临时 xUnit 文件 `OpenUtau.Test/Core/Render/TempRendererEnvelopeCapabilityTest.cs` 与临时冒烟脚本/输出（`%TEMP%\ou-smoke*`）均已删除；工作树除本次合并文件外无残留。

### 未决项（已知、非阻塞）

- **`AppTest.StringsTest` 的 headless teardown 摆动**：本轮合并树与 fork 基线两次全量各 1 次、均未复现。仍按「顺序敏感、需独立 topic 定位」跟踪。
- **xunit 钉版**：上游本轮未改 `OpenUtau.Test.csproj`，fork 的 `xunit.v3` 3.2.2 钉版与 csproj 内说明注释维持原判（同 `## Merge 2026-09-16 81637a33`）。
- **`.slnx` 依赖较新 SDK**：上游已删除 `OpenUtau.sln`。fork 的本地命令与 CI 都用项目路径（`dotnet build OpenUtau`、`dotnet run --project OpenUtau.Test/OpenUtau.Test.csproj`），本机 .NET 10 SDK 已验证 `dotnet build OpenUtau.slnx` 可用；若将来有工具/IDE 只认 `.sln`，需再评估。

---

## Sync 2026-09-24 xiaobaijunya (934a390c + 30a69d2b)

- **时间**（UTC）：`2026-09-24T07:14:57Z`（= 本地 2026-09-24 15:1x +0800；验证完成时间）
- **来源仓库**：`xiaobaijunya/OpenUtau-CustomRenderer`（**本 fork 的上游来源仓库**，remote `xiaobaijunya`），同步后 tip `30a69d2b59a0dead0a73d2efa47333e187487f1d`
- **方式**：按用户指示只取该仓库**最新 2 个 commit**（不合并其余 56 个）。先在 topic 分支 `fix/modplus-frq` 上 `git cherry-pick -x 934a390c 30a69d2b`（`-x` 在提交信息里留来源 SHA），验证完成后合入 `master`。
- **fork 侧基线**：`d6ce535c`（`docs(merge-log): fix the garbled BOM audit line in the 5d17f141 entry`）
- **干净性取证**：cherry-pick 前本 fork 的 `OpenUtau.Core/Classic/Frq.cs` 与来源仓库 `75ea0b5a:Frq.cs` **逐字节相同**（blob `b3ddf630`）→ 这两个 commit 是相对本 fork 的干净增量，不是双侧分叉实现。

### 引入的 commit（2 个）

| # | SHA | 主题 | 作者 |
| --- | --- | --- | --- |
| 1 | `934a390c20e4e8b6a0347c7c92eb32e520203dd0` | 优化modplus效果 | xiaobaijunya |
| 2 | `30a69d2b59a0dead0a73d2efa47333e187487f1d` | 修复modplus生成不了的bug | xiaobaijunya |

### 修改面

- **`OpenUtau.Core/Classic/Frq.cs`**：两 commit 合计 +131 / −27，cherry-pick 后与该仓库 tip **逐字节一致**（blob `2035135f`）。
  - 新增 `CenterTrimRatio = 0.30`、`CenterTone()`、`VoicedTones()`、`MinVoicedFreq = 60`：参考音改为「oto 区域 `[offset, cutoff)` 内 f0 帧，去掉最低/最高各 30% 后取音高平均」（音域上平均 = 几何平均），**不再使用 frq 文件里的 `averageF0`** ——后者会被起音滑音、噪声尖峰拉偏，且 `Completion()` 插值出的假音高会被当真音高计入。
  - `OtoFrq` 构造新增前置校验并写 `error` 文案：无 wav 文件 / 无 frq-mrq 文件 / frq 无帧 / **无浊音帧** / 区域算出的音高差数组为空 → `loaded = false`（旧实现此时会让音高数组变成 `-Infinity`/`NaN`，正是 modplus「生成不了」的根因）。
- **`OpenUtau.Core/Render/RenderPhrase.cs`**：+43，modplus 块新增 `LogModPlusOnce`（静态去重，避免逐 phrase/逐次播放刷屏）+ 4 处守住（frq 不可用 / 音高差数组为空 / `frqIntervalTick <= 0` / `stretch` 非有限或非正 / 逐点 `diff` 非有限），并把 catch 日志补上 oto 文件、音素、位置。

### 冲突（1 个文件）

| 文件 | 上游侧来源 | fork 侧来源 | 处置 |
| --- | --- | --- | --- |
| `OpenUtau.Core/Render/RenderPhrase.cs` | `30a69d2b`（基于旧 API：`UProject`/`UTrack`/`UVoicePart` 构造函数、`phoneme.oto`、`project.timeAxis.TemposBetweenTicks`、`phoneme.GetExpression`） | fork 的 pipeline 版 `RenderPhrase(Pipeline.PhraseSource, int, int)`（`PhonemeSource`、`NoteTempos`、`DefaultBpm`、`VelRaw`） | **按 fork API 逐条改写上游语义**（政策第 3 条：fork 独有面单独落点），上游 4 处守住与去重日志一字不落 |

改写映射（20 行冲突全部覆盖）：`phoneme.oto` → `PhonemeSource.Oto`；`project.timeAxis.TemposBetweenTicks(part.position + phoneme.position, part.position + phoneme.End)` → `phoneme.NoteTempos`；`project.tempos[0].bpm` → `source.DefaultBpm`；`phoneme.GetExpression(project, track, Format.Ustx.VEL).Item1` → `phoneme.VelRaw`；其余自动合并进来的旧写法则统一回写到 fork 的 `Oto`/`Phoneme`/`Position` 属性名。

### 已验证

1. **构建**：`dotnet build OpenUtau -c Debug`（独立 `git worktree`，因本机用户实例正运行并锁住 `OpenUtau/bin/Debug`）→ **0 错误**（1785 条警告，与合入前 1784 同量级）。
2. **测试**：`dotnet test OpenUtau.Test` → **Total 476 / 通过 475 / 失败 0 / 跳过 1**，与 `master` 基线**逐项相同**（无用例增减、无回归）。
3. **行为验证（临时 xUnit 6 用例，直接驱动生产 `OtoFrq`，验证后已删除）**：
   - 参考音取自 f0 帧而非 `averageF0`：把 `averageF0` 设成 700 Hz（被离群帧污染），A4 帧的音高差仍**恰为 0**（旧实现会整体偏 −8.1 半音）；
   - 两端裁剪确实生效：区域音高均值 +0.8 半音（非对称离群）时，纯 A4 尾段 100 帧的音高差**全部为 0**；
   - **全无浊音帧** → `loaded = false`、`error = "frq file has no voiced frame"`、两个数组为空（旧实现 `Completion()` 填 0 → `FreqToTone(0) = -Inf` → 音高数组 Inf/NaN → 渲染失败，即本次修复的 bug）；
   - **空音高差**（`consonant == offset`）→ 报 `empty tone diff` 并跳过，不再让绘制侧按 `Length - 1` 索引空数组；
   - 正常区域 → `toneDiffFix` / `toneDiffStretch` 全部有限，真实音高偏差（±12/±24 半音）完整保留；
   - 无 frq 文件 → `error = "no frq / mrq file"`。**6 通过 / 0 失败**。
4. **UI 冒烟**：本机用户实例（PID 5748，`无梦之梦-backup.ustx`）正在运行，`Program.Main` 的单实例守卫会让新进程直接退出（日志实测：`Process OpenUtau already open. Exiting.`），故**不干扰用户实例**，改用重命名副本 `ou-smoke-probe.exe` 启动分支构建 → **存活 30s**、`MainWindowTitle = "OpenUtau v0.0.0.0"`、工作集 236MB、stdout/stderr 全空、`CloseMainWindow()` 后正常退出；副本与临时脚本均已删除。
5. **未覆盖（如实记录）**：仓库内没有任何 `.frq` 夹具（`git ls-files "*.frq"` 计数 0），因此 `RenderPhrase` 内那 4 处守住属于**防御性冗余**（`OtoFrq` 已保证前置条件），仅做了代码审查与编译验证，没有端到端渲染用例覆盖。

### 未决项（已知、非阻塞）

- **该仓库其余 56 个 commit 未同步**（音高提取与「应用音高」窗口 `PitchAudioApplier`/`ApplyPitchDialog`、`ChineseVOCALOID` 音素器、`ViewConstants`、钢琴卷帘/波形/主题改动、appveyor 打包等）——按用户本轮指示「只要新的两个 commit」处理；若后续要它们，需单独定 topic 与取舍（其中 UI 面与 fork 自研 Studio UI 存在直接冲突面，`git merge` 干跑有 30 个冲突文件）。

### 其他

- 新增 remote `xiaobaijunya`（`https://github.com/xiaobaijunya/OpenUtau-CustomRenderer.git`），供后续同步使用；未改动 `origin` / `upstream` 配置。

---

## Merge 2026-09-24 a4d41398

- **时间**（UTC）：`2026-09-24T16:39:10Z`（= 本地 2026-09-25 00:39 +0800；验证完成时间）
- **合并方式**：`git merge --no-commit --no-ff upstream/master`（merge-base `5d17f141d3ff923ddc2c38cb50d91bd8fc0cd39f`，即上次上游合并记录的上游基线；合并范围 `5d17f141..a4d41398`）
- **上游基线**：`a4d41398d1b4f6362476c96883514f9ebf4dcd8f` — Add headless UI smoke tests for the main window (#2445)
- **fork 侧基线**：`85536eea87b8fc4ffdcce92e8ce227260a6789db`（`Merge fix/modplus-frq: sync xiaobaijunya modplus commits (934a390c, 30a69d2b)`）

### 引入的上游 commit（2 个，线性）

| # | SHA | 主题 | 作者 |
| --- | --- | --- | --- |
| 1 | `ce996bac` | Replace the track header singer menu with a singer flyout (#2444) | StAkira |
| 2 | `a4d41398` | Add headless UI smoke tests for the main window (#2445) | StAkira |

### 修改面

- 上游 17 个文件（+1199 / −194）：**13 个 fork 侧未改动**（`SingerFlyout.axaml(.cs)`、`SingerAvatarCache.cs`、`SingerFlyoutViewModel.cs`、`SingerFlyoutOrderTest.cs`、`OpenUtau.UiTest/` 三文件、`pr-test.yml`、`OpenUtau.slnx`、`TrackHeader.axaml.cs`、`MenuItemViewModel.cs`、`UpdaterDialog.axaml.cs`，由 ort 直接落盘），**4 个双侧都改过**（1 冲突 + 3 自动合并）。
- fork 侧自 base 起改动 142 个文件；剔除上述 4 个后，**138 个 fork 独有面文件本轮上游一个都没碰**。
- 上游本轮**未改字符串**（`git diff 5d17f141 upstream/master -- OpenUtau/Strings/` 为空），**新增 3 个测试用例**（`OpenUtau.Test/App/SingerFlyoutOrderTest.cs`）+ **新增 1 个测试工程**（`OpenUtau.UiTest`，2 个用例）。

内容概要：

- **歌手 flyout（`ce996bac`）**：轨道头的歌手选择由 `ContextMenu`（`SingersMenuRes` + `SingerMenuItems`）改为模态 `Flyout`。新增 `SingerFlyout.axaml(.cs)`（瓷砖式列表、收藏星、当前歌手环、安装/打开目录/刷新）、`SingerFlyoutViewModel`（`OrderSingers`：收藏优先 → 近期收藏 → 其余收藏按名 → 近期非收藏 → 按组名再按名；`SingerTileViewModel` 轻量化：瓷砖由 Button 降为 Border、星标由 ToggleButton+Viewbox+Canvas 降为单 Path，大列表快 ~2.5–3×）、`SingerAvatarCache`（共享位图缓存，不 dispose）。`TrackHeaderViewModel` 删掉 `SingerMenuItems` / `RefreshSingersAsync`（−163 行）；`TrackHeader.axaml` 删 `SingersMenuRes` 资源、歌手按钮加 `Name="SingerButton"`、更多按钮加 `Name="MoreButton"`、新增 `FlyoutPresenter.singerFlyout` 样式；`TrackHeader.axaml.cs` 的 `SingerButtonClicked` 改为同步 `ShowSingerFlyout`（锚点 `SingerButton`，从「⋯」飞出时先 `MoreButton.Flyout?.Hide()` 避免叠层）；`MenuItemViewModel.IsFavourite` setter 不再调 `TrackHeaderViewModel.InvalidateSingerMenuCache()`。附带修画布悬停光标：`MainWindow.axaml` 加 `PointerExited`、`axaml.cs` 加 `PartsCanvasPointerExited`（离开部件边缘时清 `Cursor`）。
- **headless UI 冒烟（`a4d41398`）**：新增独立测试工程 `OpenUtau.UiTest`（`Avalonia.Headless` + Skia 真绘制 + 截图 artifact，独立进程，避免污染 `OpenUtau.Test` 的进程级单例），CI 加 `ui test` 步骤；`MainWindow` 构造函数把 `DataContext = viewModel = ...` **挪到 `InitializeComponent()` 之前**（消除每次启动的两次绑定错误）；`UpdaterDialog.CheckForUpdateEnabled` 让测试跳过联网更新检查；`OpenUtau.slnx` 加项目。

### 冲突（1 个文件）

| 文件 | 上游侧改动量（base→上游） | fork 侧改动量（base→fork） | 合并结果 vs 上游 | 合并结果 vs fork | 上游侧来源 | fork 侧来源 |
| --- | --- | --- | --- | --- | --- | --- |
| `OpenUtau/Controls/TrackHeader.axaml` | +11 / −20 | +30 / −20 | +30 / −20（= fork delta） | +11 / −20（= 上游 delta） | `ce996bac`（`Name="SingerButton"` / `Name="MoreButton"` / 删 `SingersMenuRes` / 加 `FlyoutPresenter.singerFlyout`） | `464705a1`（轨道号徽章移出头像、`TrackNameButton` 进 `Grid`）、配合 `c837526a`（`BadgeForeground` 取色）、`55615237`（彩色条）、`4fcfe157` / `8d04547c`（`.s1` 皮肤类名）、`a0172184`（fx 与 M/S 同列） |

冲突块位置 `OpenUtau/Controls/TrackHeader.axaml:114-133`（轨道名 / 歌手按钮所在的 `StackPanel`）。
其余 3 个双侧重叠文件由 ort 自动合并，两侧改动行段不相邻（明细见下表），无冲突标记。

### 判定流程取证（按 `AGENTS.md` 的 Merge conflict policy 判定流程执行）

1. **量化两侧改动**：见上表与下表（`git diff --numstat 5d17f141 upstream/master -- <file>` 与 `git diff --numstat 5d17f141 master -- <file>`）。
2. **看残余差异由谁贡献**：`TrackHeader.axaml` 合并结果对上游的残余为 +30 / −20、对 fork 为 +11 / −20，**两侧改动量均非 0**，不属于「上游已自行回退、无上游实现」的情形。
3. **归属到 commit 与作者**：`git log --oneline 5d17f141..upstream/master -- OpenUtau/Controls/TrackHeader.axaml` 只有 `ce996bac`（StAkira）一条；fork 侧 `git log --no-merges --oneline 5d17f141..master -- <file>` 为 6 条 Studio 轨道头 topic（`464705a1` / `a0172184` / `4fcfe157` / `8d04547c` / `55615237` / `c837526a`，作者 Machinacanis），两侧作者不重叠。
4. **逐行核对目标**：冲突块内**上游**改的是「歌手按钮从 ContextMenu 换成 flyout」——加 `Name="SingerButton"`（`ShowSingerFlyout` 里 `Control anchor = SingerButton;` 依赖该字段）、去掉 `ContextMenu="{StaticResource SingersMenuRes}"`（资源已被上游删除）；**fork** 改的是「轨道号徽章从头像角移到轨道名左侧的 `Grid`，并给按钮起名 `TrackNameButton`」（`464705a1`）。两者一个改歌手入口、一个改徽章布局 → **目标不同，不构成同目标双实现，取并集**。
5. **集合类差异**：本轮无（上游未改 `Strings.axaml` 与任何语言文件）。
6. **记录结论**：见「决策记录」。

### 决策记录

- **`TrackHeader.axaml`：并集。** 保留 fork 的 `<Grid ColumnDefinitions="auto,*">`（`TrackNoBadge` + `TrackNameButton`），采纳上游的 `Name="SingerButton"` 且**不带** `ContextMenu`。取证：取上游整块会丢掉 Studio 徽章与新布局（政策第 3 条的 fork 独有功能面）；保留 fork 的匿名歌手按钮则 `TrackHeader.axaml.cs` 的 `ShowSingerFlyout` 里 `SingerButton` / `MoreButton` 两个字段不存在，**根本不编译**。
- **`MainWindow.axaml.cs` 的 `DataContext` 先于 `InitializeComponent`：保留上游次序**（政策第 2 条：优先减小 diff，不反向改上游）。取证：UiTest 主窗口冒烟在两台路径下均 0 绑定错误（见「已验证」第 3、6 条），fork 的 XAML 无「构造后再绑 `DataContext`」的依赖。
- **另 3 个双侧文件两侧全取，逐段并存**：
  - `TrackHeaderViewModel.cs`：上游删 `SingerMenuItems` 面（−163）与 fork 的 Studio 绘制（`BadgeForeground` / `ColorBarWidth` / `ApplyPaint`）及 `TrackMuteVisualEvent`（`1b767a77`）、`MixFxWindowManager`（`9fd108be`）并存。合并结果的 `RefreshAvatar`（含 `SingerAvatarCache.Get` + `EmptyAvatar` 占位）与上游逐字一致。
  - `MainWindow.axaml`：上游 `ce996bac` 的 `PointerExited="PartsCanvasPointerExited"` 与 fork 的 `Name="MainPageGrid"`（`f4777f8e`）、`RowDefinition Height="Auto"`、`GridSplitter Classes="panel"`、`x:Name="MainProgressText"`（`f22f3a13`）并存。
  - `MainWindow.axaml.cs`：上游 `a4d41398` 的构造次序 + `ce996bac` 的 `PartsCanvasPointerExited` 与 fork 的 DAW 菜单 / `HandleGlobalShortcut`（`53b2417f`、`b4295a6b`）/ `MixFxWindowManager.CloseAll()`（`9fd108be`）/ `Subbanks?` 守卫并存。
- **13 个上游独有文件：直接落盘，无取舍**（fork 侧自 base 起未改动，`git diff --numstat 5d17f141 master -- <file>` 全为空）。其中 `TrackHeader.axaml.cs` 与 fork 的 6 条 Studio 改动**同日不同区段**（fork 只改 `.axaml` 的类名与 `x:Name`，未改 code-behind），合并后该文件与上游逐字节相同。
- **fork 无残留引用**：全库 `grep -rn "SingerMenuItems\|InvalidateSingerMenuCache\|SingersMenuRes\|RefreshSingersAsync"` → 0 命中。
- **字符串脚本**：上游本轮无键变更，未产生同步需求。仍执行一次 `python Misc/sync_strings.py` 确认——唯一副作用是**剥离英文源 `Strings.axaml` 的 BOM**（脚本以无 BOM 回写，历轮已记录），已 `git checkout --` 还原 → `Strings.axaml` 与 22 个语言文件**零变化**。

### 已验证

1. **构建**：`dotnet build OpenUtau.slnx`（本机 .NET 10.0.401 SDK）→ **0 错误**（73 条警告，全部为 `OpenUtau.Test` 的存量 xunit analyzer 提示：`xUnit1051` / `xUnit2013` / `xUnit1031`）。
2. **测试**：`dotnet test OpenUtau.Test` → **Total 479 / 通过 478 / 失败 0 / 跳过 1**（跳过项仍为需真实 DAW 插件的 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`）。**用例数账目闭合**：fork 基线 476（上次记录）→ 合并后 479，差额 **+3** = 上游新增 `SingerFlyoutOrderTest` 的 3 个 `[Fact]`，与「上游对 `OpenUtau.Test/` 只新增该文件」一致。
3. **新增 UI 测试工程**：`dotnet run --project OpenUtau.UiTest/OpenUtau.UiTest.csproj` → **Total 2 / Errors 0 / Failed 0 / Skipped 0**（7.2s，win-x64 `net10.0-windows`）。这是上游为本次合并面（主窗口构造次序 + 启动期绑定）自带的最强端到端信号；**本 fork 首次具备 headless 全窗口冒烟能力**。
4. **冲突解析的运行时取证**（临时 `[Fact] TempSingerButtonOpensFlyout`，验证后已删除；已确认 `OpenUtau.UiTest/MainWindowTest.cs` 与上游**逐字节相同**）：真实 `MainWindow` → 点 `welcome.new` 进编辑器 → `TrackHeader.FindControl<Button>("SingerButton")` 命中（证明 `x:Name` 与生成的字段都在）→ 点击 → 全程 **0 错误**（`ErrorMessageNotification`、错误级日志、Avalonia 绑定错误都进 `ErrorCollector`）→ 截图 `TempSingerButtonOpensFlyout.png` 目视确认：flyout 锚在该按钮下方、空态「此处没有歌手」+ 安装 / 打开目录 / 刷新三按钮渲染正常；轨道头同时正确显示 **6px 彩色条 + 「1」徽章 + Track1 + 选择歌手**（Studio UI 开；本机 `Preferences.UseStudioUI` 默认 `true`，即走的正是 fork 改动那条渲染路径）。3 通过 / 0 失败。
5. **双侧零丢失核对**（4 个重叠文件，方法沿用历轮：取 base→各自 HEAD 的 `+` 行、忽略 ≤12 字符短行，在合并结果中逐条字面匹配）：**上游侧 29 行、fork 侧 66 行全部命中，0 丢失**。逐文件数字见下表：

| 文件 | 上游侧（base→上游） | fork 侧（base→fork） | 合并结果 vs 上游 | 合并结果 vs fork |
| --- | --- | --- | --- | --- |
| `OpenUtau/Controls/TrackHeader.axaml` | +11 / −20 | +30 / −20 | +30 / −20 | +11 / −20 |
| `OpenUtau/ViewModels/TrackHeaderViewModel.cs` | +12 / −163 | +22 / −16 | +22 / −16 | +12 / −163 |
| `OpenUtau/Views/MainWindow.axaml` | +1 / −0 | +6 / −5 | +6 / −5 | +1 / −0 |
| `OpenUtau/Views/MainWindow.axaml.cs` | +11 / −2 | +24 / −12 | +24 / −12 | +11 / −2 |

6. **原生桌面 UI 冒烟**（本轮改了 `MainWindow` 构造次序与轨道头 XAML，故必跑）：以 PowerShell `Start-Process` 直接启动合并结果的 `OpenUtau.exe`（Debug）→ **存活 30s**、`MainWindowTitle = "OpenUtau v0.0.0.0"`、工作集 206MB、**stdout 与 stderr 全为空**、`CloseMainWindow()` 后确认退出（`hasExitedAfterClose=True`，无异常弹窗、无崩溃）；临时输出文件已删除。
7. **BOM 核对**：本轮合并面 17 个文件逐个与上游比对首 3 字节——**17/17 与上游一致**（`TrackHeader.axaml` / `SingerFlyout.axaml(.cs)` / `SingerAvatarCache.cs` / `SingerFlyoutViewModel.cs` / `SingerFlyoutOrderTest.cs` / `OpenUtau.UiTest/*` / `OpenUtau.slnx` / `pr-test.yml` 两侧本就无 BOM；其余 9 个含 BOM），无历轮那种 fork 基线缺 BOM 的偏差。
8. **`git diff --check` 与冲突标记**：索引态无空白错误；全库冲突标记（`^(<<<<<<<|>>>>>>>|=======$)`）扫描为空。
9. **清理**：临时 `[Fact]` 已删除并核对与上游逐字节相同；UiTest 生成的截图与 `prefs.json` 落在 `OpenUtau.UiTest/bin/...`（该工程 `DataPath` 走便携模式 = 程序目录，**不触及用户 `Documents/OpenUtau`**），全部位于 gitignore 覆盖的 `bin/`；临时冒烟脚本与 `%TEMP%\ou-smoke-*.txt` 已删除；工作树除本次合并文件与 `MERGE_LOG.md` 外无残留。

### 未决项（已知、非阻塞）

- **`OpenUtau.UiTest` 的跨平台可用性只在 win-x64 本地实测**；macOS / Linux 由 CI（`pr-test.yml` 新增 `ui test` 步骤）覆盖，本机无法验证。
- **xunit 双版本并存**：`OpenUtau.Test` 仍钉 `xunit.v3` **3.2.2**（fork 自加，因 `Avalonia.Headless.XUnit 12.1.2` 在 xunit.v3 4.x 上 `[AvaloniaFact]` 会 `MissingMethodException`），而新 `OpenUtau.UiTest` 用 **4.0.0**（只写 `[Fact]`，自己用 `HeadlessUnitTestSession` 而非 `[AvaloniaFact]`，恰好绕开该缺陷）。两者是独立工程 / 独立进程，本机实测可同时构建运行；但**若上游将来给 `OpenUtau.UiTest` 引入 `[AvaloniaFact]`，fork 需要同样钉版**。
- **`AppTest.StringsTest` 的 headless teardown 摆动**：本轮全量 1 次未复现，仍按「顺序敏感、需独立 topic 定位」跟踪（同上轮）。
- **CI 触发面**：`pr-test.yml` 仅在 `pull_request` 到 `master` 时跑；直接推到 `master` 不会触发，本 fork 的本地验证仍以 `dotnet test` + `dotnet run --project OpenUtau.UiTest` 为准。

---

## Merge 2026-09-25 ec6c4b2e

- **时间**（UTC）：`2026-09-25T15:09:01Z`（验证完成时间）
- **合并方式**：`git merge --no-commit --no-ff upstream/master`（merge-base `a4d41398d1b4f6362476c96883514f9ebf4dcd8f`，即上次上游合并记录的上游基线；合并范围 `a4d41398..ec6c4b2e`）
- **上游基线**：`ec6c4b2ee1aa0171848d433b79410c6bda26cac6` — Singer flyout search (#2450)
- **fork 侧基线**：`39b6d2169fda7e8432e16f8ffc9084cf6e9c6ddb`（`Merge upstream/master (a4d41398) into master`）

### 引入的上游 commit（2 个，线性）

| # | SHA | 主题 | 作者 |
| --- | --- | --- | --- |
| 1 | `148d65fe63ec9a2217fbf139a0c441e537876e43` | DiffSinger: opt-in piano roll toggle for merging nearby phrases (#2449) | Kakaru |
| 2 | `ec6c4b2ee1aa0171848d433b79410c6bda26cac6` | Singer flyout search (#2450) | StAkira |

### 修改面

- 上游 27 个文件（+828 / −68）：**21 个 fork 侧自 base 起未改动**（歌手搜索面 `SingerFlyout*` / `SingerSectionDividers.cs` / `SingersDialog*` / `SingersViewModel.cs` / `SingerFlyoutViewModel.cs`，音库 `SearchTerms`，`IRenderer.GapOverlapsPadding`，各 Singer 的搜索词，以及 3 个测试文件），由 ort 直接落盘。
- **6 个双侧都改过**，全部自动合并，**0 个冲突标记**。
- fork 侧自 base 起改过的其余文件，本轮上游一个都没碰。

内容概要：

- **#2449 乐句合并改为可选**：`IRenderer.ShouldMergePhrases` 的默认实现抽成 `GapOverlapsPadding`（默认接口方法，fork 的 `HifiUtauRenderer` / `CustomServerRenderer` 不覆盖，无需适配）。`DiffSingerRenderer` 用新偏好 `DiffSingerMergeNearbyPhrases`（默认 `false`）把门，关掉时分组回到 `f32e4ab5` 之前。钢琴卷帘工具条加开关，只在 DiffSinger 轨可见；切换后写偏好、`ValidateProjectNotification` + `PreRenderNotification`。`NotesCanvas` 在 `RenderView` 投递投影时再重绘，避免音高线停在旧分组。
- **#2450 歌手 flyout 搜索**：顶部搜索框，打开即聚焦；匹配忽略大小写、全半角、平假名/片假名。搜索词含显示名、原文名、本地化名、id、文件夹名，以及 `character.yaml` 的 `search_terms`。收藏 / 近期 / 其余之间画分段线（`SingerSectionDividers`），不打乱瓷砖网格。右键菜单：打开位置、编辑搜索词、从近期移除。歌手对话框齿轮菜单也可编辑搜索词，写回 `character.yaml`。

### 冲突

无冲突标记。6 个双侧重叠文件由 ort 自动合并，两侧改动行段不相邻。

| 文件 | 上游侧改动量（base→上游） | fork 侧改动量（base→fork） | 合并结果 vs 上游 | 合并结果 vs fork | 上游侧来源 | fork 侧来源 |
| --- | --- | --- | --- | --- | --- | --- |
| `OpenUtau.Core/Util/Preferences.cs` | +6 / −0 | +119 / −1 | +119 / −1（= fork delta） | +6 / −0（= 上游 delta） | `148d65fe`（`DiffSingerMergeNearbyPhrases = false`） | Studio / HiFiUTAU / Custom Server 偏好（`f22f3a13` 起 9 个 fork commit） |
| `OpenUtau/Controls/NotesCanvas.cs` | +4 / −0 | +106 / −23 | +106 / −23 | +4 / −0 | `148d65fe`（`RenderView.Inst.Observe` 重绘） | Studio 音符绘制（`f22f3a13` / `1b767a77` / `c29b2b10`） |
| `OpenUtau/Controls/PianoRoll.axaml` | +15 / −0 | +79 / −57 | +79 / −57 | +15 / −0 | `148d65fe`（合并开关，接在 live pitch 按钮后） | Studio 表达式条与面板拖拽（`f22f3a13` / `f4777f8e` / `957bf34b`） |
| `OpenUtau/ViewModels/NotesViewModel.cs` | +17 / −0 | +20 / −6 | +20 / −6 | +17 / −0 | `148d65fe`（`MergeNearbyPhrases` + 订阅） | 静音变暗 / 幽灵音符（`1b767a77` 及两次 review fix） |
| `OpenUtau/Strings/Strings.axaml` | +8 / −0 | +147 / −15 | +147 / −15 | +8 / −0 | `148d65fe` + `ec6c4b2e`（8 个新键） | fork 专属键（Studio / DAW / HiFiUTAU） |
| `OpenUtau/Strings/Strings.zh-CN.axaml` | +1 / −0 | +199 / −13 | +199 / −13 | +1 / −0 | `148d65fe`（`pianoroll.toggle.mergephrases` 译文） | fork 译文（`dece045e` 等） |

### 判定流程取证（按 `AGENTS.md` 的 Merge conflict policy 判定流程执行）

1. **量化两侧改动**：见上表（`git diff --numstat a4d41398 upstream/master -- <file>` 与 `git diff --numstat a4d41398 master -- <file>`）。
2. **看残余差异由谁贡献**：6 个文件合并结果对上游的残余**逐字节等于 fork delta**，对 fork 的残余**逐字节等于上游 delta**。两侧改动量均非 0，不属于「上游已自行回退」。
3. **归属到 commit 与作者**：上游侧每个文件只有上表那 1–2 个 commit（Kakaru / StAkira）；fork 侧作者是 Machinacanis（及 DAW 字符串的 KakaruHayate），与上游作者不重叠。
4. **逐行核对目标**：上游是「DiffSinger 乐句合并开关」和「歌手搜索键 / 搜索词字段」，fork 是 Studio UI、HiFiUTAU、Custom Server、静音视觉。目标不同，**不构成同目标双实现，取并集**。`IRenderer` 的改动是把已有默认方法拆出 `GapOverlapsPadding`，不是替换 fork 实现。
5. **集合类差异**：`Strings.axaml` 比键集合。合并后英文源是两侧键的并集（944 键，上游新增 8 个，无重复、无丢失）。fork 侧仍是上游的严格超集加这 8 个新键。
6. **记录结论**：见「决策记录」。

### 决策记录

- **6 个重叠文件全部取并集，不替换任一侧。** 自动合并已把上游增量原样接上，fork 增量一行未丢（>12 字符的新增行字面匹配：上游 6+4+13+14+8+1、fork 114+72+74+16+147+196，**0 丢失**）。
- **`NotesCanvas` 订阅位置**：`RenderView.Inst.Observe` 落在 fork 的 `ThemeChanged` / `StudioTrackPaletteChanged` / `NotesRefreshEvent` 之后、`NotesSelectionEvent` 之前。Studio 绘制订阅保留。
- **`PianoRoll.axaml` 开关位置**：接在两个 live pitch 按钮之后、fork 的面板拖拽 `Border` 之前。Studio 表达式条与拖拽分隔条都在。
- **`NotesViewModel` 订阅**：与上游逐段一致（初值回声跳过，避免启动时重校验）。fork 的静音 / 幽灵音符成员不在这段。
- **21 个上游独有文件：直接落盘，无取舍。** `ShouldMergePhrases` 仍是默认接口方法，fork Renderer 不必实现。
- **字符串**：跑一次 `python Misc/sync_strings.py`。脚本以无 BOM 回写，已把英文源 `Strings.axaml` 的 BOM 补回。同步是纯增量：21 个语言文件各 +8 行注释占位，`zh-CN` +7（`pianoroll.toggle.mergephrases` 的「自动合并临近段落（DiffSinger）」译文保留，不覆盖）。既有译文零删除。英文源 944 键，22 个语言文件键集与英文一致。

### 已验证

1. **构建**：`dotnet build OpenUtau.slnx -c Debug` → **0 错误 / 1860 警告**（存量 nullable / xunit analyzer，与历次同量级）。
2. **测试**：`dotnet test OpenUtau.Test --no-build` → **Total 492 / 通过 490 / 失败 0 / 跳过 2**。账目：fork 基线 479（上次记录）→ 492，差额 **+13** = 上游 `SearchMatches` 的 6 个 `[InlineData]` + 6 个新 `[Fact]` + 1 个跳过的 `MergeAdjacentPhrasesLiveTest`（未设 `OPENUTAU_TEST_SINGERS`）。另一个跳过项仍是 `DawRealPluginTest.RealPluginCompletesTheHandshakeAndPullsAudio`。
3. **UI 测试**：`dotnet run --project OpenUtau.UiTest --no-build` → **Total 3 / Errors 0 / Failed 0**（基线 2，差额 +1 = 上游 `SingerFlyoutSearches`）。
4. **原生桌面冒烟**：启动合并结果的 `OpenUtau.exe`（Debug）→ **存活 20s**、窗口标题 `OpenUtau v0.0.0.0`、stdout 与 stderr 全空，随后终止。未改用户 `Documents/OpenUtau`。
5. **双侧零丢失**：见决策记录。`git diff --numstat` 对 6 个重叠文件的残余与对侧 delta 逐项相等。
6. **BOM**：`NotesViewModel.cs` / `Preferences.cs` / `Strings.axaml` 两侧本就有 BOM，合并后仍在。`NotesCanvas.cs` / `PianoRoll.axaml` 的 fork 基线无 BOM、上游有 BOM，合并保留 fork 基线（未剥、未补）。`Strings.zh-CN.axaml` 两侧均无 BOM。
7. **`git diff --check` 与冲突标记**：索引态与工作区均无空白错误；全库冲突标记扫描为空。
8. **清理**：字符串同步前的备份在 `%TEMP%\ou-strings-backup`（不在仓库内）；冒烟的 stdout/stderr 在 `%TEMP%`，不入库。UiTest 产物在 `bin/`（gitignore）。

### 未决项（已知、非阻塞）

- **`MergeAdjacentPhrasesLiveTest` 本机跳过**：需要 `OPENUTAU_TEST_SINGERS` 指向真实 DiffSinger 音源目录。CI 同样跳过，与上游一致。
- **7 个搜索键尚无译文**（`tracks.nosingermatch` / `openlocation` / `removefromrecent` / `searchsinger` / `searchterms` / `searchterms.edit` / `searchterms.prompt`）。上游只译了 `pianoroll.toggle.mergephrases` 的 zh-CN。其余语言以注释占位，不在本次合并里新译。
- **未推送 `origin`**。本记录随合并提交落在本地 `master`。
