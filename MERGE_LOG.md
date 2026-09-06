# Merge Log（上游合并记录）

本文件按 `AGENTS.md` 的 **Merge logging** 共识记录每次与上游 `openutau/OpenUtau` 同步合并的完整经过，保证合并行为可溯源。

## 规则

- 每次合并结束后立即追加一条记录；无冲突的同步也记录并标注"无冲突"。
- 每条记录是独立的 `## Merge YYYY-MM-DD <上游SHA前8位>` 块，块间用 `---` 分隔。
- 按时间顺序追加在文件末尾，不改写历史记录。
- 每条记录必含：时间（UTC）/ 上游基线 / 引入的上游 commit / 冲突来源（fork 侧与上游侧 commit）/ 逐条决策 / 验证结果。

---

## 2026-09-05 历史重写说明（与上游对齐）

上游 `openutau/OpenUtau` 于本日 force-push 重写 `master`：删除 `e8efca31`（波形 phrase 音频范围背景框）并把 `50daf7cc`/`55da2d9b` 重写为 `f32e4ab5`/`5afde866`。为与本 fork 保持对齐，本地未推送历史已重置至 `24b9a265` 后**重新合并**上游（放弃中间产物 `d9519420` 及其后的 fix/revert 提交），使合并结果与上游一致：**不含 phrase bounds 功能**。旧记录（Merge 55da2d9b、临时修复 7347bd67 等）随历史重写不再存在于提交中，其要点已在下方的 Merge 03a2833f 记录中合并说明。

---

## Merge 2026-09-05 03a2833f（重写后唯一合并记录）

- **时间**（UTC）：`2026-09-05T05:0x:xxZ`（合并提交时间，重写后）
- **合并方式**：`git merge upstream/master`（merge-base `3eb9dd26459fa51dc2c5bf46693a9492d817c130`）
- **上游基线**：`03a2833f88322ad24e3faf0d885995486f7759d1` — Fix IndexOutOfRangeException in worldline phrase rendering

### 引入的上游 commit（20 个，自 fork 点 3eb9dd26 起；上游重写后序列）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | 89acfe8422e3d844bcf37eb654a31a580282d0d0 | SBP Update and Fixes (#2014) |
| 2 | fd4fc95076d597d9113b3dd4f23cc924f08474ec | Improvements to the Note Hover Glow (#2360) |
| 3 | ab771e1ea1fea077f4d71cbad31046a72b4a8ff0 | Added Developer Tools and Diagnostics Support During DEBUG (#2361) |
| 4 | a14212cd8b741cd98c54a73d62b33ee03b724430 | Add Phonetic Hints To Lyrics (#2362) |
| 5 | b26af870d6739c4ec37a093e5002f087f7f9606d | Remove Unnecessary Code (#2357) |
| 6 | 66e310736cc021fdc9b3bb35aef209c28996c9e7 | Toast Notification (#2356) |
| 7 | c6875239fe0847b8ebddff3b630d211d0fbb8f1c | Refactor singer management and improve UI responsiveness in SingersDialog (#2348) |
| 8 | aee74cde2b4523910ab2eb8f201b8d03ba9d5b67 | Fix main window switching to welcome page on Alt+Left in lyric box |
| 9 | 56436e18d5ae98988c09c46fd6a9e29b4b392d4b | Add CUDA rendering support on Linux (#1722) |
| 10 | cd8375e81d4041b98efcd0779b4d09f0a5970417 | Fix phoneme duration error sticking after offset override moved back |
| 11 | bb832af75092d213fb4c4d28d4a3d46a3df601ee | Waveform: leave blank where no phrase has audio |
| 12 | f32e4ab592bf65c44d938a215a0c90c1851b09fd | Merge overlapping render phrases via renderer padding API（重写自 50daf7cc） |
| 13 | 5afde866a23b24786758837ff4987e492f219c40 | Add real-time (live) DiffSinger pitch generation（重写自 55da2d9b） |
| 14 | 80fc0a163c6e468917a5a1fb170c65d9ee25bd98 | prefs crash fix (#2365) |
| 15 | ccc796cb1f40f38b0669dfe47f4f9df3c63ed0a7 | Update .gitignore |
| 16 | 3840138251c09234d07a87364fef2839deba8ddf | Fix merged-phrase timing by rendering inter-phoneme gaps as silence |
| 17 | 873fc77ddef62b4342ad4866be985f495cb792cb | Phoneme canvas: bar-style drawing for DiffSinger tracks |
| 18 | 900bb05003eaa056f3357ed1aee3497531798375 | 1 (#2366) |
| 19 | 8bee25eb1b3fee3fe436de964e111d6d78ccb04b | Fix AutoConvel Track.part bug (#2363) |
| 20 | 03a2833f88322ad24e3faf0d885995486f7759d1 | Fix IndexOutOfRangeException in worldline phrase rendering |

### 冲突文件与来源 commit（5 文件 / 7 个冲突块）

| 文件 | fork 侧 | 上游侧 |
| --- | --- | --- |
| `OpenUtau/Controls/NotesCanvas.cs` | `f22f3a13`（Studio 光晕） | `fd4fc950`（hover glow 改进） |
| `OpenUtau/Controls/WaveformImage.cs` | `f22f3a13`（波形重写） | `bb832af7`（无音频空白） |
| `OpenUtau/Styles/Styles.axaml` | `f22f3a13`（GridSplitter 样式） | `66e31073`（Toast 样式） |
| `OpenUtau/ViewModels/PreferencesViewModel.cs` | fork HiFiUTAU 偏好块 | `56436e18`（CUDA）+ `80fc0a16`（#2365 判空） |
| `OpenUtau/Views/MainWindow.axaml` | `f22f3a13`（MainPageGrid 命名） | `aee74cde`（Alt+Left 修复） |

### 决策记录（逐条）

1. **NotesCanvas.cs**：采纳上游签名（`IBrush brush` 参数）+ 保留 fork 圆角 `radius`。最终调用 `DrawHoverGlow(context, leftTop, size, radius, brush, GetHoverGlow(note))`。
2. **WaveformImage.cs 循环头/循环体（2 块）**：保留 fork 的 `drawWidth`/`colMin`/`colMax` 布局管线，采纳上游 `bb832af7` 的 `phraseRanges`/`covered` 空白跳过逻辑（`columnStartMs` 声明、跳过更新、循环尾更新三处齐备）。**不含 phrase bounds 背景框**——与上游改写后一致（fork 已对齐）。
3. **Styles.axaml**：两段样式都保留（`GridSplitter.panel` + `NotificationCard`），互不依赖。
4. **PreferencesViewModel.cs**：采用上游 #2365 判空方案（`Count > 0 ? FirstOrDefault(...) : new GpuInfo()`）+ CUDA 显示（`ShowOnnxGpu = DirectML || CUDA`）+ fork HiFiUTAU/CustomServer 初始化块；修正上游 `new GpuInfo();` 与 `ShowOnnxGpu` 同行的排版。
5. **MainWindow.axaml**：Grid 标签属性合并（`Name="MainPageGrid"` + `KeyDown="OnCarouselPageKeyDown"`）。

### 已验证

- `dotnet build OpenUtau -c Debug`：0 错误。
- `dotnet test OpenUtau.Test`：305/305 通过。
- `python3 Misc/sync_strings.py`：字符串已同步。
- 无残留冲突标记；`git diff --check` 干净。

---

## 2026-09-05 备注：偏好设置崩溃（OnnxGpuOptions 为空）的最终状态

- 上游 `56436e18`（CUDA）删除了早期 `23af41c1` 添加的空列表兜底（上游回归）；重写后的本次合并直接带入上游 **#2365 判空修复**（`80fc0a16`：`OnnxGpuOptions.Count > 0 ? FirstOrDefault(...) : new GpuInfo()`），`OnnxGpu` 不再越界。
- 历史重写前曾有本地临时修复（`7347bd67` PadGpuList）与回退（`9e2ac075`），已随重写移除；最终采用上游方案（不向 GPU 列表注入假条目）。

---

## Merge 2026-09-06 b863f7be

- **时间**（UTC）：`2026-09-06T01:10:20Z`（合并提交时间）
- **合并方式**：`git merge upstream/master`（merge-base `03a2833f88322ad24e3faf0d885995486f7759d1`，即上次合并记录的上游基线）
- **上游基线**：`b863f7be012a6b4dc23063023529b186d74846e2` — Fix Presamp Static Dictionary Bug and File Encodings (#2081)

### 引入的上游 commit（3 个）

| # | SHA | 主题 |
| --- | --- | --- |
| 1 | `54f4693312a4980d3a5db4407eacdf76c8739abd` | Remove auto-merge rule requiring PRs to come from this repository |
| 2 | `431b228d2f12220b7f89f262915d26d002599fbd` | "Just play" VCCV fix (#2367) |
| 3 | `b863f7be012a6b4dc23063023529b186d74846e2` | Fix Presamp Static Dictionary Bug and File Encodings (#2081) |

### 冲突

**无冲突**（ort 策略自动合并）。变更文件（4 个）：`.github/auto-merge-rules.json`、`.github/scripts/auto-merge.mjs`、`OpenUtau.Core/Classic/Presamp.cs`、`OpenUtau.Plugin.Builtin/EnglishVCCVPhonemizer.cs`。合并后这 4 个文件与 `upstream/master`（`b863f7be`）逐字节一致（`git diff upstream/master HEAD -- <文件>` 为空）。

### 决策记录

- 无冲突块需要逐块决策；本 fork 自有的最近改动（Studio UI 偏好、README 修订）均未触及上述 4 个文件，故全部采纳上游提交，无保留 fork 侧内容。
- 合并后测试与 fork 既有测试集一致（上游 `431b228d` 与 `b863f7be` 均未改动 `OpenUtau.Test`）；`OpenUtau.Test/Files` 夹具未变化。

### 已验证

- `dotnet build OpenUtau -c Debug`：0 错误（3 个既有 AVLN3001 XAML 运行时加载警告，与本次合并无关）。
- `dotnet test OpenUtau.Test`：**315/315 通过**（35 s）。
- `python3 Misc/sync_strings.py`：运行后无任何字符串文件差异。
- `git diff --check`：无残留冲突标记；工作区无未提交改动。合并的 4 个文件与上游 `b863f7be` 逐字节一致（含上游 #2081/#2367 提交自带的尾随空格，按要求不改动上游文件）。