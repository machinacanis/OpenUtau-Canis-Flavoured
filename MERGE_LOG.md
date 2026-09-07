
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
