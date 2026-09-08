# Studio UI 轨道头改造（实施记录）

主题分支：`feature/studio-track-header`（base = `origin/master` = 3220b3a8）
状态：已实施，`dotnet test OpenUtau.Test` 全绿（见 §7）。

目标：Studio UI 开启时

1. 轨道头默认高度使用紧凑档（图二）；
2. 头像左侧增加轨道彩色条（模仿 Studio One / Studio Pro 8，图三）；
3. S / M / Fx 三个按钮使用 Studio One 风格「体积」外观，且该外观**可复用**到其它按钮。

硬性约束：**Studio UI 关闭时视觉与图一逐像素一致**；不新增用户可见字符串。

---

## 0. 实测事实与优先级结论

截图实测（两张图同为 125% 缩放：头部列 300 逻辑 px = 377 物理 px）：

| 项 | 图一（现状默认） | 图二（目标） |
| --- | --- | --- |
| 轨道间距（物理 px） | 131 | 79 |
| 折合逻辑高度 | 105 = `TrackHeightDefault` | 63 = 105 − 2×21 |
| 可见行 | 名 / 歌手 / 音素器 / 渲染器 / 推子 | 名 / 歌手 / 推子 |

图二实际是**缩小两级**（字面「一级」为 84）。已与用户确认取 **63**，M/S 激活配色取 **M 红 / S 黄**。

可见性门槛在 `OpenUtau/Controls/TrackHeader.axaml.cs:84-86`：
`≥63` 歌手行、`≥84` 音素器行、`≥105` 渲染器行 + 齿轮按钮。

Avalonia 优先级两条硬事实（决定实现路径）：

1. **控件自身 `Styles` > 应用级 `Styles`**。证据：`TrackHeader.axaml:12` 的
   `ToggleButton{Background=Transparent}` 压掉了 `Styles.axaml:173` 的
   `ToggleButton.toolbar{Background=SystemControlBackgroundAltHighBrush}`。
2. **XAML 局部值 > 任何样式 setter**。

⇒ 按钮 chrome 必须由**模板内部**拥有（外部 setter 无从干扰）；
彩色条宽度必须走 VM 属性绑定（不能用 XAML 局部值 + 样式覆盖）。

## 1. 新增 `OpenUtau/Studio/StudioTrackLayout.cs`

```csharp
public static class StudioTrackLayout {
    public const double CompactTrackHeight =
        ViewConstants.TrackHeightDefault - 2 * ViewConstants.TrackHeightDelta;   // 63
    public static double DefaultTrackHeight =>
        StudioUI.IsEnabled ? CompactTrackHeight : ViewConstants.TrackHeightDefault;
    public static double ColorBarWidth => StudioUI.IsEnabled ? 6 : 0;
}
```

## 2. 默认高度

- `ViewModels/TracksViewModel.cs`：`TrackHeight = StudioTrackLayout.DefaultTrackHeight;`
  （`TrackHeightDefault` 全仓仅此一处使用）。
- 运行时切换 Studio UI：订阅 `StudioUIChangedEvent`，**仅当当前高度等于两个已知默认值之一**
  （即用户未手动缩放）时切到新默认，然后 `Notify()` + 夹紧 `TrackOffset`；
  已缩放（84/126…）则保留用户缩放。
- 63px 布局预算：头部内容高 = 63 − 2（`Border Margin=1`）− 2（`BorderThickness=1`）= **59**；
  右列 3 键 `(17+2)×3 = 57` ✓（余 2px）；105px 时 M/S/fx+分隔条+齿轮 `19×5 = 95 ≤ 101` ✓。
  `StackPanel` 默认不裁剪，溢出会压到下一轨道，故该预算由测试守住。

## 3. 头像左侧彩色条

`Controls/TrackHeader.axaml`：`Grid.Column=0` 改为 `ColumnDefinitions="auto,auto"`，
左列是 `Border#TrackColorBar`（`Width="{Binding ColorBarWidth}"`、
`Background="{Binding TrackAccentColor}"`、`VerticalAlignment=Stretch`、
`IsHitTestVisible=False`），右列是原来的头像 + 编号徽章面板（Pointer 处理器原样保留）。

- `TrackHeaderViewModel` 增只读属性 `ColorBarWidth => StudioTrackLayout.ColorBarWidth`，
  并在既有 `ManuallyRaise()` 里 `RaisePropertyChanged`（`StudioUI.NotifyChanged()` 会先发
  `StudioTrackPaletteChangedEvent` → Canvas 已调用 `ManuallyRaise`，切换即时生效）。
- 颜色 `TrackAccentColor`（Studio 模式 = `swatch.HeaderAccent`，随主题/轨道色自动跟随）。
- 宽度 6 为 300px 头部下的折中（Studio One 实测 13px @ 77px 行高），单个数可调。

## 4. 可复用的 Studio One 按钮皮肤

### 4.1 `OpenUtau/Styles/StudioOneControls.axaml`（新增）

由 `Styles/StudioStyles.axaml` 内 `<StyleInclude>` 引入，因此只在 Studio UI 开启时加载。
复用 API（写在文件头注释里）：

| 用法 | 效果 |
| --- | --- |
| `ToggleButton Classes="s1"` | 选中 = 主题强调色 `AccentBrush2` |
| `ToggleButton Classes="s1 mute"` | 选中 = 红 `#E04A4A`，字形白 |
| `ToggleButton Classes="s1 solo"` | 选中 = 琥珀 `#E8B84B`，字形 `#1A1A1A` |
| `Button Classes="s1 fx"` + `Classes.fxOn` | 开启 = 主题强调色 |
| 新增角色 | 加一条 `^:checked.<role> /template/ Border#PART_Chrome` |

**实现要点（与最初设想的差异）**：不用 `ControlTheme`，而是应用级 `.s1` 样式直接给
`Template` 一个**自足模板**（`Border#PART_Chrome` + `ContentPresenter#PART_Content`，
`BorderThickness=1`、`CornerRadius=3`、`Height=18`、`Margin=0`、内容居中）。原因：

- `ControlTheme` 的 Template 会被应用级 `Styles.axaml` 的 `Button{Template}` 压掉
  （fx 按钮就没有 chrome）；控件级 `Button.fxButton{Background=Transparent}` 也会压掉
  ControlTheme 的 Background setter。
- 应用级 `Button.s1` / `ToggleButton.s1` 的 Template setter 排在 `Styles.axaml` 之后
  （`App.ApplyStudioStyles` 追加到 `Application.Styles` 末尾），优先级足够；模板内部
  用固定 brush 画 chrome，`Background`/`BorderBrush` 被外部 setter 覆盖也不影响外观。
- 状态（hover / pressed / checked / 角色）用「伪类/类触发 + `/template/ Border#PART_Chrome`」
  表达，优先级高于普通 setter，故能稳定覆盖。
- M/S 字形颜色单独覆盖 `^:checked.mute Path.filled` / `^:checked.solo Path.filled`
  （Path 不继承 `Foreground`，必须显式设）。
- `ToggleButton.s1` 规则必须排在 `Button.s1` 之后（ToggleButton 也是 Button，两者都匹配，
  后者胜出）。
- `Margin` 不进皮肤（间距属调用点上下文）。

### 4.2 新增固定色（`OpenUtau/Colors/Brushes.axaml` 末尾）

`TrackMuteBrush #E04A4A` / `TrackMuteBorderBrush #F07070` / `OnTrackMuteBrush #FFFFFF` /
`TrackSoloBrush #E8B84B` / `TrackSoloBorderBrush #F5D07A` / `OnTrackSoloBrush #1A1A1A`。
放在恒加载的 `Brushes.axaml` 里，避免经典模式下 DynamicResource 解析告警。

### 4.3 轨道头接线（`Controls/TrackHeader.axaml`）

- M → `Classes="toolbar s1 mute"`、S → `Classes="toolbar s1 solo"`、
  fx → `Classes="toolbar s1 fxButton"`（保留 `Classes.fxOn` 绑定）。
- **保留局部 `Height="17"`**：皮肤里的 18 会被局部值压掉，但这是刻意的——经典模式
  不加载皮肤时 `ToggleButton.toolbar` 会把高度变回 20，从而破坏图一；保留 17 可让
  经典模式逐像素不变，同时 57px 仍在 59px 预算内。皮肤里的 `Height=18` 供其它消费者使用。
- 不加 `studio` 根类、不改 `TrackHeaderCanvas`：Studio 关时 `.s1` 样式不存在，等价于今天。

## 5. 测试与测试基建

新增（均属 `StudioUiSerial` 串行 collection，避免全局偏好互相干扰）：

- `OpenUtau.Test/App/StudioTrackLayoutTest.cs`：63/105、6/0、`(105−63)/21 == 2`、
  `TracksViewModel.TrackHeight` 随模式、未缩放时跟随默认、已缩放时保留。
- `OpenUtau.Test/App/StudioOneControlsTest.cs`（复用性证明）：与轨道头无关的裸
  `ToggleButton`/`Button` 加 `.s1` 后，mute/solo/无角色/fxOn 的 chrome 与字形颜色正确；
  不带 `.s1` 的按钮没有 chrome（证明是显式 opt-in）。
- `OpenUtau.Test/App/TrackHeaderChromeTest.cs`：`TrackColorBar.Width` 6/0；
  M/S/Fx 三个按钮带 `.s1` 且模板生效；`TrackHeight=63` 时右列 StackPanel
  `DesiredSize.Height ≤ 59`（防溢出回归）。
- `OpenUtau.Test/App/StudioUiTestHelpers.cs`：collection 定义 + `StudioUiScope` +
  `StudioSkinScope` + chrome/颜色断言助手。

为让 Avalonia headless 测试可用，附带修了三处测试基建/健壮性问题：

1. `OpenUtau.Test/App/AppTest.cs`：`TestAppBuilder` 补 `.UseReactiveUI(_ => { })`
   （对齐 `Program.BuildAvaloniaApp`，否则 ViewModel 的 `WhenAnyValue` 抛未初始化）；
   `StringsTest` 改为 `[AvaloniaFact]` 并直接用 `Application.Current`（不再在测试线程
   二次 `SetupWithoutStarting`，那会与 headless session 的 dispatcher 线程冲突）。
2. `OpenUtau/App.axaml.cs`：`AttachDeveloperTools` 加静态守卫（同一进程多个 App 实例时
   只能附加一次）；`ApplyStudioStyles` 每次新建 `StyleInclude` 并在关闭时置空
   （`IStyle` 只能属于一个 `Styles` 集合，headless session 会多次构建 App）。
3. `OpenUtau.Test/App/StudioUITest.cs` 加入 `StudioUiSerial` collection。

## 6. 手动验收清单（需在真机 125% DPI 跑一次）

对照图一/图二/图三：Studio Dark 与 Tokyo Day 两套主题；63/84/105/147 四档高度；
静音 / 独奏 / `MuteAllOthers` / fx 开关 / 选中描边 / 拖拽排序；中、日、长歌手名；
100% 与 125% DPI。

## 7. 验证结果

- `dotnet test OpenUtau.Test`：全绿，461 通过 / 1 跳过（其中新增 16 个用例：
  6 布局 + 7 皮肤 + 3 轨道头），连续多轮运行稳定。
- 构建：`OpenUtau.csproj` 开 `TreatWarningsAsErrors`，编译无警告。
- 注：真机验证时 `bin/Debug` 被正在运行的 OpenUtau 占用，测试使用
  `-p:BaseOutputPath=<temp>` 隔离输出；改完需关掉旧实例重新构建才能看到效果。

## 8. 提交拆分

1. `feat(studio ui): compact default track height`
2. `feat(studio ui): track header color bar`
3. `feat(studio ui): reusable studio one button skin`
4. `test(studio ui): headless chrome tests and test app bootstrap`

配套：`AGENTS.md` Studio UI 段落已补 3 条（默认高度/彩色条；`.s1` 皮肤约定；样式优先级）。
`MERGE_LOG.md` 不涉及（非上游合并）。

## 9. 风险与后续

- 皮肤复用的后续候选（本次不做）：钢琴卷帘工具栏、表情面板、走带按钮——届时只加类名。
- 63px 偏紧：按钮可退 `Height=17`（已是），或色条降到 4px（一行常量）。
- 范围外：轨道头整体重排（M/S 移到色条右侧、名字行加波形缩略图）。
