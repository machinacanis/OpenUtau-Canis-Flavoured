# ONNX 原生库加载可靠性与崩溃防护 —— 修复可行性报告

日期：2026-09-13
状态：**已实现并验证**。分支 `fix/onnx-native-availability-guard`（阶段 A + B + C 已落地）
结论范围：`OpenUtau.csproj` 构建布局（原议题 1）、`OnMenuPreferences` 崩溃防护（原议题 2）

> 实施结果见文末「§7 实施与验证记录」。第 1 节结论未变：议题 1 **不需要**改代码。

---

## 0. 结论摘要

| 议题 | 结论 | 是否可修 |
| --- | --- | --- |
| 1. `OpenUtau.csproj:86` 只在带 RID 时把原生库拷到输出根目录 | **不是本次故障的原因，且对 `dotnet run` 场景基本是空操作**。真正把 `onnxruntime.dll` 放进输出目录的是 NuGet 资产解析，不是该 ItemGroup。当前没有可复现的缺陷需要修。 | 不需要修（可保留作发布路径保险） |
| 2. ONNX 原生加载失败会杀死进程，偏好窗口无防护 | **成立，且已在本机复现故障模式**。可在任何 ORT 调用之前用"绝对路径预加载"做可靠探测，失败则降级；`DllImportResolver` 路线经实测**无效**。 | 可修，约 2 小时，风险低 |

一句话：**缺 VC++ 只是触发条件；真正的产品缺陷是"应用自带的原生库加载失败后，系统静默回退到 `System32` 的旧版 `onnxruntime.dll`，而没有任何一层能把这件事变成可处理的错误"。**

---

## 1. 议题 1：构建布局，深入取证

### 1.1 先看事实

`OpenUtau.csproj:83-100`：

```xml
<ItemGroup Condition="'$(Configuration)'=='DEBUG'">
  <None Include="..\runtimes\**" CopyToOutputDirectory="PreserveNewest" LinkBase="runtimes\" />
</ItemGroup>
<ItemGroup Condition="'$(RuntimeIdentifier)' == 'win-x64'">
  <None Include="..\runtimes\win-x64\native\**" CopyToOutputDirectory="PreserveNewest" LinkBase="." />
</ItemGroup>
```

仓库 `runtimes/` 下**只有 6 个 worldline 产物**：

```
runtimes\linux-arm64\native\libworldline.so
runtimes\linux-x64\native\libworldline.so
runtimes\osx\native\libworldline.dylib
runtimes\win-arm64\native\worldline.dll
runtimes\win-x64\native\worldline.dll
runtimes\win-x86\native\worldline.dll
```

`runtimes/win-x64/native/onnxruntime.dll` **在仓库里不存在**（实测 `ABSENT`）。

`dotnet build -getItem:None` 的求值结果：`None` 项共 23 个，其中路径含 `runtimes` 的**恰好就是上面 6 个 worldline**，`LinkBase=runtimes\`；**没有任何一项的 `Link` 等于 `onnxruntime.dll`**。

### 1.2 那 `bin` 里的 `runtimes\win-x64\native\` 是谁放的

实测 `OpenUtau\bin\Debug\net10.0-windows\runtimes\win-x64\native\` 里同时有：

```
onnxruntime.dll                   17328152
onnxruntime.lib                       2834   <-- 注意这个
onnxruntime_providers_shared.dll     22040
SDL3.dll
worldline.dll
```

- 哈希比对：`bin\runtimes\win-x64\native\onnxruntime.dll` == NuGet 缓存里的同名文件（`E7EEDEC6A6F26DC3…`）。
- `onnxruntime.lib` 是 C++ 链接用的导入库，MSBuild 里**没有任何一项引用它**，却出现在输出中。
- 结论：这棵树由 **NuGet 资产解析**（`project.assets.json` 的 `runtimeTargets`）填充，与 `..\runtimes\**` 通配无关。

补充：`Microsoft.ML.OnnxRuntime.DirectML` 的 `build/netstandard2.0/Microsoft.ML.OnnxRuntime.DirectML.props` 确实有 `Link=onnxruntime.dll` + `CopyToOutputDirectory` 的项，但它**没有被导入**（`-getItem:None` 里 0 条匹配），因为项目 TFM 是 `net10.0-windows`。所以那条路径也不成立。

### 1.3 `dotnet run` 到底能不能找到原生库：**能**

我在临时工程里做了对照实验（`net10.0-windows`，只引用 `Microsoft.ML.OnnxRuntime.DirectML` 1.24.4，无 RID、无自定义拷贝规则）：

```
[A] no resolver registered
[A] File.Exists(app-local) = True
[A] OrtGetApiBase() = 0x7FFB761DDA58
[A] loaded: ...\bin\Debug\net10.0-windows\runtimes\win-x64\native\onnxruntime.DLL ver=1.24.20260316.9.2d92497
```

即：**.NET 主机的原生探测确实会命中 `runtimes/{rid}/native/`**，`dotnet run` 无需 RID、无需 csproj 额外规则。

### 1.4 议题 1 判定

- 我此前把它归因为"只在带 RID 时才拷到根目录"的隐患，**取证后不成立**：`dotnet run`（无 RID）路径下原生库本来就能解析成功。
- `RuntimeIdentifier` 条件项在 `dotnet run` 里不生效（`runtimeconfig.json` 只列共享框架、`project.assets.json` 的 `runtimeTarget.name` 为空），但对**发布**路径是必要的：发布时 RID 已设定，该 ItemGroup 把仓库自带的 worldline 原生库放到输出根。
- **建议：不动它。** 唯一可做的是加注释说明"根目录拷贝是给 RID/发布路径用的，`dotnet run` 靠 `runtimes/` 探测"，避免下一个人重复这次误判。

---

## 2. 议题 2：真正的缺陷与可修性

### 2.1 故障链条（全部实测）

关键一步以前只是推断，现在有了直接证据。把 `runtimes\win-x64\native\onnxruntime.dll` 换成 4 KB 垃圾（模拟"自带原生库加载失败"，与缺 VC++ 时 `err=126` 同类），在无 resolver 的对照工程里：

```
########## VARIANT B: garbage app-local, no resolver ##########
[A] File.Exists(app-local) = True
[A] OrtGetApiBase() = 0x7FFBA38DBAA8
[A] loaded: C:\WINDOWS\SYSTEM32\onnxruntime.DLL ver=1.17.250417-0100.1.os-germanium.7c460ce
exit=0
```

**这是本次事故的机制本身**：

1. 进程优先尝试应用自带的 `runtimes\win-x64\native\onnxruntime.dll`（1.24）；
2. 该加载失败（缺 `MSVCP140.dll` / `MSVCP140_1.dll`，`LoadLibraryExW` → `err=126`）；
3. 系统加载器**不报错**，继续搜索并命中 `C:\WINDOWS\SYSTEM32\onnxruntime.dll`（1.17）；
4. 加载"成功"，托管层是 1.24，去取 1.17 不存在的 `OrtGetCompileApi`；
5. `0xC0000005`，进程直接死。

要点：**第 3 步让"失败"变成了"看起来成功"**。因此任何基于"文件存在吗""加载成功了吗"的判断都无效 —— 上面 `OrtGetApiBase()` 明明拿到了非零指针。

另外确认：`Microsoft.AI.DirectML` 1.15.4 包在 win-x64 下**不含任何原生 DLL**，所以应用目录里**没有** DirectML 的本地副本可回退，这也解释了为什么最终落到 `System32\DirectML.dll`。

### 2.2 `DllImportResolver` 路线：实测无效，应排除

我一度认为可以给托管 ORT 程序集注册 `DllImportResolver` 来接管解析。实测否定：

```
[R] resolver registered; app-local present = True
[R] OrtGetApiBase() = 0x7FFB761DDA58
[R] resolver call count = 0        <-- 一次都没被调用
[R] loaded: ...\runtimes\win-x64\native\onnxruntime.DLL ver=1.24.20260316.9.2d92497
```

托管 ORT 1.24.4 自行加载原生库（IL 元数据中 `PinvokeImpl` 方法数为 0），**不走 CLR 的 P/Invoke 解析**，所以 resolver 永远不会被触发。**不要把 resolver 写进方案。**

### 2.3 可用的探测方案：绝对路径预加载

思路：在任何 ORT 调用之前，用**绝对路径**主动加载应用自带的原生库。绝对路径不走搜索顺序，因此**要么成功，要么明确失败，不存在静默回退**。

在三态下实测（`native\onnxruntime.dll` 分别健康 / 垃圾 / 缺失）：

| 状态 | `NativeLibrary.TryLoad(绝对路径)` | `LoadLibraryExW` | 探测结论 | System32 是否被装载 |
| --- | --- | --- | --- | --- |
| 健康 | `True`（handle 非零） | `0x…` `err=0` | 可用 | 否 |
| 垃圾（模拟缺 VC++） | `False` | `0x0` `err=193` | 不可用 | **否** |
| 缺失 | `False` | `0x0` `err=126` | 不可用 | **否** |

对照 2.1 的变体 B：不做预加载时 System32 被装载且 `err=0`；做预加载时 System32 **完全没被装载**。探测与"静默回退"互斥，这正是需要的性质。

`err=126` 也直接对应当初 VC++ 缺失的现象，可作为给用户的诊断提示（"缺少 Microsoft Visual C++ 2015-2022 运行库"）。

### 2.4 崩溃面：谁会被它带走

`Onnx` 的静态初始化是**类型锁触发**的，第一次触碰类型就执行：

```csharp
// OpenUtau.Core/Util/Onnx.cs:28-40
private static bool cudaAvailable = OS.IsLinux() && …;
private static readonly Dictionary<int, OrtEpDevice> devices = initializeDevices();  // ← 这里就会碰原生
```

入口清点：

| # | 位置 | 触发时机 | 后果 |
| --- | --- | --- | --- |
| 1 | `Preferences.cs:121` `Onnx.getRunnerOptions()` | **`Preferences` 静态构造 → 启动时**（存在 `prefs.json` 即触发） | 应用**启动即死**，连主窗口都没有 |
| 2 | `PreferencesViewModel.cs:358/361` | 点「工具 → 使用偏好」 | 偏好窗口打不开；`MainWindow.axaml.cs:750-757` 的 `try/catch` **完全拦不住**（原生故障不是托管异常） |
| 3 | `WorldlineRenderer.cs:147/166`、`HiFiUtauModelStore.cs:199`、`VogenRenderer.cs:146/170`、`DiffSinger*`、`Rmvpe.cs:224`、`Some.cs:49`、各 `G2p` 构造 | 渲染 / 音素化时 | 渲染进程崩溃 |

注意第 3 项：`WorldlineRenderer`（默认 `WORLDLINE-R` 之后的一档）**也吃 ONNX**，所以"我没用 HiFiUTAU" 不等于安全。

第 1 项比偏好窗口更严重：本机装 VC++ 前 `prefs.json` 存在，理论上每次启动都会走 `Preferences.cs:121`。这是一条独立于 UI 的崩溃路径，值得单独修。

### 2.5 参考：机上 `prefs.json` 与日志现状

- `OpenUtau\bin\Debug\net10.0-windows\prefs.json` 存在 → 启动即会走 `Onnx.getRunnerOptions()`。
- `Logs\log20260913.txt` 当前 22 KB；安装 VC++ 后再次打开偏好窗口，无新增崩溃记录。

---

## 3. 修复方案（分阶段，含工作量与风险）

### 阶段 A（推荐，必做，约 1 小时）原生可用性探测 + 优雅降级

新增文件，属 **fork 自有面**，不污染上游文件结构：

```
OpenUtau.Core/Util/OnnxNativeAvailability.cs        (新)
```

- 暴露 `bool IsAvailable`（`Lazy<bool>` 或 `lock` + 缓存，只探测一次）。
- 探测：按当前 RID 拼绝对路径（`win-x64` → `runtimes/win-x64/native/onnxruntime.dll`，`osx` → `libonnxruntime.dylib`，`linux-*` → `libonnxruntime.so`），`NativeLibrary.TryLoad(fullPath, out handle)`。
- 失败时用 `Log.Warning` 输出可读原因（含 `err=126` → 提示安装 VC++ 运行库）。
- 把已加载的 `handle` 保留住（`GC.KeepAlive` / 静态字段），确保模块常驻，避免后续被卸载。

调用点（唯一必须改的上游文件，1–2 行）：

```csharp
// OpenUtau.Core/Util/Onnx.cs  initializeDevices() 开头
if (!OnnxNativeAvailability.IsAvailable) {
    return new Dictionary<int, OrtEpDevice>();
}
```

以及 `getGpuInfo()` 在不可用时直接返回空列表（`PreferencesViewModel.cs:362-364` 已有 `Count > 0` 分支，**UI 侧不需要改**）。

**为什么这样安全**：探测为假时根本不会调用 `OrtEnv.Instance()`，因此不存在"加载了旧版原生"的可能；探测为真时原生模块已被绝对路径装载，后续不会再落到 `System32`。

风险与未验证点（诚实标注）：
- 需实测确认"预加载模块已被装载时，托管 ORT 的自加载会复用它而不是再加载一份"。变体 B / 状态表已证明预加载能阻止回退，但**尚未**在真实 ORT 调用序列（`OrtEnv.Instance()` → `GetEpDevices()`）上端到端跑过。**这是实施时的第一个验证点。**

### 阶段 B（强烈建议，约 30 分钟）把启动路径从 ONNX 解耦

`Preferences.cs:121` 只是为了校验 `OnnxRunner` 是否在候选列表里。它不应该有能力杀死启动。

- 方案：把该行改为在 ONNX 不可用时短路，或挪到首次使用 ONNX 的路径上延迟校验。
- 收益：**消除"启动即死"这一条路径**，把最坏后果限制在"偏好窗口打开失败"。

### 阶段 C（建议，约 30 分钟）版本一致性检查

即使原生能加载，也可能加载到版本不匹配的 `onnxruntime.dll`（例如用户 `PATH` 上有一份）。`Onnx.getGpuInfo()` 里 `devices[i] = device`（`Onnx.cs:96`）在设备集合与字典键不一致时会抛 `KeyNotFoundException` —— 这是可捕获的托管异常，属于版本偏差的症状。

- 方案：探测通过后比对原生文件版本与托管 ORT 版本（`FileVersionInfo` / `OrtApi.GetVersionString`），主次版本不一致就判定不可用。
- 这条不能防住"原生过旧导致 0xC0000005"的全部情形，但能覆盖已知的 1.17/1.24 组合。

### 阶段 D（可选，约 2 小时）外进程自检

只有托管代码能做的探测都有理论边界（原生代码可能在**任何**调用点崩）。若要做到"在任何情况下都能优雅降级"：

- 在 `Program.Main` 最早处支持隐藏参数（如 `--onnx-selfcheck`），在**子进程**里跑一次 `OrtEnv.Instance()` + `GetEpDevices()`，父进程读退出码。
- 代价：每次启动多一次进程创建（可用"上次结果 + 原生文件哈希"缓存）。
- 结论：**阶段 A/B/C 已能覆盖本次事故**，D 只在要求"绝对不崩"时才值得。

### 明确不建议

- ✗ `NativeLibrary.SetDllImportResolver` 方案 —— §2.2 实测无效。
- ✗ 基于"文件是否存在 / `NativeLibrary.Load` 是否成功"的判断 —— §2.1 证明会误判为成功。
- ✗ 为了绕开本问题把原生库改成一定拷到根目录 —— 不解决"加载失败仍静默回退"，且发布路径已经覆盖。

---

## 4. 验证计划（实施后必须逐条跑）

1. **健康态**：`dotnet run` 打开偏好窗口，「GPU」下拉有 `[0] AMD Radeon 780M Graphics (GPU)`，`OnnxRunnerOptions = CPU,DirectML`。
2. **垃圾态**：备份后用 4 KB 垃圾替换 `bin\...\runtimes\win-x64\native\onnxruntime.dll`；预期：**应用不崩**，偏好窗口能开，GPU 列表为空，日志有明确 warning。
3. **缺失态**：删除该文件；预期同上（`err=126`）。
4. **启动态**（阶段 B 之后）：保留 `prefs.json` 的情况下重复 2/3，应用必须能正常启动。
5. **回归**：`OpenUtau.Test` 全量。注意：`AppTest.StringsTest` 在单元测试会话里有已知的 teardown `Dispatcher` 报错（headless 夹具问题，非产品缺陷）。
6. **渲染回归**：至少跑一次 `WORLDLINE-R` 渲染，确认探测未误伤健康路径。

---

## 5. 附：本次调查的实测证据索引

| 证据 | 命令/位置 | 结果 |
| --- | --- | --- |
| `None` 项求值 | `dotnet build OpenUtau -c Debug -getItem:None` | 23 项，`runtimes` 相关只有 6 个 worldline；无 `onnxruntime` |
| 仓库原生库清单 | `Get-ChildItem runtimes -Recurse` | 只有 6 个 worldline 产物 |
| 输出树原生库 | `bin\Debug\net10.0-windows\runtimes\win-x64\native` | 含 `onnxruntime.dll` + `.lib` + providers |
| 哈希归属 | `Get-FileHash` 对比 NuGet 缓存 | 与包内文件一致（非仓库文件） |
| 无 RID 可解析 | 对照工程变体 A | app-local 1.24 成功装载 |
| **静默回退复现** | 对照工程变体 B（垃圾原生） | 装载 `SYSTEM32\onnxruntime.DLL` 1.17，`err=0` |
| resolver 无效 | 对照工程变体 R | `resolver call count = 0` |
| 预加载探测三态 | 对照工程变体 T | 健康→True；垃圾→`err=193`；缺失→`err=126`；System32 均未被装载 |
| 装机现状 | `System32\msvcp140*.dll` | 已随 VC++ 14.51.36247 就位 |

临时工程位于系统临时目录，**已删除**；仓库 `git status` 为空，`master` 未改动。

---

## 6. 分支与提交建议

- 议题 2 属产品改动，须走 topic branch：`fix/onnx-native-availability-guard`（base `origin/master`）。
- 新增 `OnnxNativeAvailability.cs` 为 fork 自有落点；对 `Onnx.cs` / `Preferences.cs` 的改动保持最小插入行，并在 `MERGE_LOG.md` 记录"仅加可用性守卫"以便后续同步上游时识别。
- 议题 1 **不需要**代码改动；若加注释，并入同一 topic。

---

## 7. 实施与验证记录

### 7.1 实际落地的改动

| 文件 | 类型 | 内容 |
| --- | --- | --- |
| `OpenUtau.Core/Util/OnnxNativeAvailability.cs` | 新增（fork 落点） | 绝对路径预加载探测（应用根 + `runtimes/<rid>/native`，按进程架构推导 rid）、版本守卫、可读原因与提示；探测结果进程内缓存一次 |
| `OpenUtau.Core/Util/Onnx.cs` | 上游文件，2 处插入 | `initializeDevices()`、`getGpuInfo()` 各加一处 `IsAvailable` 提前返回 |
| `OpenUtau.Core/Util/Preferences.cs` | 上游文件，1 处替换 + 1 个私有方法 | 启动期 `Onnx.getRunnerOptions()` 改走 `GetOnnxRunnerOptionsSafely()`，失败回落 `["CPU"]` |
| `OpenUtau.Test/Core/Util/OnnxNativeAvailabilityTest.cs` | 新增测试 | 6 个用例覆盖候选路径集合、缺失/不可加载/可加载判定、诊断身份一致性 |

阶段 C（版本守卫）已合并进阶段 A 的实现：探测通过后比对原生文件版本与托管包装版本，原生**更旧**即判定不可用。这样能拦住"自带库其实是旧版"这一独立风险，而不只是"加载失败"。

### 7.2 验证结果

构建：`dotnet build OpenUtau -c Debug` → **成功**（0 错误；3 个既有的 `AVLN3001` 警告与本次改动无关）。

测试：`OpenUtau.Test` 全量 → **512 通过 / 0 失败 / 1 跳过**（跳过的是需要外部 bridge-host 的 `DawRealPluginTest`）。新增用例 6/6 通过。

四态实测（Core 侧，调用序列与 `PreferencesViewModel` 构造函数一致：`getRunnerOptions()` → `getGpuInfo()`）：

| 状态 | 探测 | 版本守卫 | `getGpuInfo()` | System32 是否被装载 | 结果 |
| --- | --- | --- | --- | --- | --- |
| 健康 | 可用，加载自带 1.24 | 通过 | 1 个设备（AMD Radeon 780M） | 否 | 不崩 |
| 自带库不可加载（缺 VC++ 等价） | 不可用 | — | 0 | **否** | 不崩，给出装 VC++ 提示 |
| 自带库缺失 | 不可用 | — | 0 | 否 | 不崩 |
| 自带库为旧版（放入 `System32` 的 1.17） | 可加载 | **拒绝**：`1.17.250417.100 < 1.24.4.0` | 0 | 否 | 不崩，给出升级提示 |

第 2 行是本次修复的核心：**探测失败后 `SYSTEM32\onnxruntime.dll` 完全没有被装载**，与修复前"静默绑定 1.17"形成对照。第 4 行证明版本守卫能独立拦住"能加载但版本不对"的情形 —— 这正是原始事故的版本组合。

### 7.3 未覆盖项（如实记录）

- **经由真实 UI 点击「工具 → 使用偏好」的端到端复跑本次未完成**：自动化脚本无法在被锁定的交互桌面上取得前台焦点（`SetForegroundWindow` / `SwitchToThisWindow` 均被系统拒绝，主窗口从未成为前台），因此菜单无法展开。上一轮修复验证时该路径已经人工确认过窗口能正常打开。
- 构建期发现一个**与本改动无关的环境缺陷**，记录备查：`OpenUtau` / `OpenUtau.Test` 的 MSBuild 还原会**静默失败**（输出 `0 个警告 0 个错误` 但退出码 1）。真因是 `Avalonia.BuildServices` 的遥测任务无法创建 `%LOCALAPPDATA%\.avalonia-build-tasks`（受限环境），异常被 MSBuild 吞掉。设置 `AVALONIA_TELEMETRY_OPTOUT=1` 后错误正常上报、构建通过。另外 `OpenUtau.csproj` 的 `TreatWarningsAsErrors=true` 会把离线环境下的 `NU1900`（无法获取漏洞库）提升为错误，导致该项目的还原失败；完整还原需要网络或本地源。

### 7.4 复现命令

```powershell
$env:AVALONIA_TELEMETRY_OPTOUT='1'
dotnet build OpenUtau -c Debug
dotnet build OpenUtau.Test -c Debug --no-restore -m:1
OpenUtau.Test\bin\Debug\net10.0-windows\OpenUtau.Test.exe -noColor
```

