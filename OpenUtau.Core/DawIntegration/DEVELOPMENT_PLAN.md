# OpenUtau DAW 集成开发计划(API-first v1)

> 配套契约文档:`OpenUtau.Core/DawIntegration/PROTOCOL.md`(英文,协议唯一权威来源)
> 本文件用途:开发路线图 + 下一正式开发会话的启动 prompt

---

## 一、下个会话 Prompt(复制到新会话直接使用)

```text
你是 Kakaru 的技术伙伴,开始一个新的正式开发会话。任务:为 OpenUtau 实现 DAW 集成的主仓库 API(里程碑 M1)。

## 背景
- 有用户向 OpenUtau 请求"提供 VST"功能,要求是 ACE Studio Bridge 式(DAW 里放薄 VST 插件当桥,编辑留在 OpenUtau 主窗口,音频/工程同步进 DAW),不是 Synthesizer V 式整软件内嵌。
- 管理员意见:改动过多但需求合理 → 主仓库只建立 API 形式引入,插件功能放独立项目。
- 既有 PR #2187(openutau/OpenUtau, OPEN, head=add/vst-integration@455d7f9c)已做完整 bridge 原型,但经评审决定**抛弃其代码与线格式**,仅借鉴机制:发现文件、XXH64 哈希去重、missingAudios 拉取、心跳、重连退避、playbackStarted flush。

## 协议关键事实(已定稿,详细见 PROTOCOL.md)
- 拓扑:OpenUtau = TCP 客户端(主动连),DAW 插件 = TCP 服务端(监听 127.0.0.1 动态端口)。
- 发现:%TEMP%/OpenUtau/PluginServers/<name>.json 含 {port, name, apiVersion}(1.0)。OpenUtau 扫描 + 端口探测判活,apiVersion major 不匹配拒绝连接。
- 双平面协议(同一 TCP 连接):
  - 控制面:行式 UTF-8 JSON,`request:<uuid>:<kind> <json>\n` / `response:<uuid> {success,data,error}` / `notification:<kind>` / 裸 `close`。
  - 数据面:音频二进制帧 `audio <hash> <length>\n` + length 字节 raw float32 PCM(44.1kHz stereo interleaved little-endian,引擎固定,不协商、不压缩、不 base64)。
- 消息集:init(返回全量 USTX + apiVersion)/ updateUstx(通知)/ updatePartLayout(请求,返回 missingAudios)/ getAudio(请求,响应为数据面帧)/ updateTracks(通知);插件→OpenUtau:ping(5s)/ playbackStarted(触发 flush pending)。
- 哈希:XXH64,一律十进制字符串序列化(如 "13507256038857166760"),禁止 JSON number(2^53 精度)。
- 参数:init 超时 5s、请求 10s、心跳死判 15s、重连退避 500ms/1s/2s;debounce:USTX/Tracks 1s、PartLayout/音频 5s。
- MIDI 输入方向 v1 不做(预留);tempo 同步 v1.1。

## 本会话交付物(里程碑 M1:主仓库 API)
在 H:\GitHub\OpenUtau 新建分支,实现 OpenUtau.Core/DawIntegration/ 下:
1. DawMessages.cs —— 控制面消息类与 DawResult 包装
2. DawTransport.cs —— TCP 客户端:行解析 + 数据面帧分用 + 请求/通知 + 超时 + 心跳监控
3. DawServerFinder.cs —— 发现目录扫描 + 端口探测 + apiVersion 协商
4. DawAudio.cs —— part.Mix → float32 PCM + XXH64 + 数据面帧构造/解析
5. DawManager.cs —— 订阅 DocManager 命令流,debounce 1s/5s,playbackStarted flush,重连退避
6. 连接入口 UI(极简对话框,参照 PR #2187 的 DawIntegrationTerminalDialog)
7. 测试:DawTransportTest(帧/超时/心跳)、DebounceTest、ConformanceClient(扮演插件 TCP server 的端到端测试工具)

## 硬约束
- 遵循本仓库 .gitattributes(*.cs 应 LF);不要触碰用户未提交的 Game.cs 等现有改动。
- 不修改 OpenUtau 渲染管线采样率(44.1kHz 固定)与 USTX 格式。
- 协议实现必须与 PROTOCOL.md 一致;发现文件路径、header 格式、XXH64 字符串序列化不可自行改动。
- 依赖:K4os.Hash.xxHash(需 XXH64;若 csproj 未引用则添加)。
- 完成标准:dotnet build 通过、单测全绿、ConformanceClient 与实现完成一次 init→updatePartLayout→getAudio→playbackStarted 全流程验证。
- 完成后:本地 commit → push → 开 PR(target 分支由 Kakaru 指定),先总结再逐项验证。

先读 PROTOCOL.md 全文与本 plan 的第三~九节,再开始实现。
```

---

## 二、背景与已定决策(摘要)

| 决策 | 结论 |
|---|---|
| 功能形态 | ACE Bridge 式:薄 VST 插件当桥,编辑留 OpenUtau,音频/工程同步进 DAW |
| 引入方式 | 主仓库仅建 API(server 侧),插件放独立项目(管理员意见) |
| PR #2187 | 抛弃代码与线格式,保留机制(见下) |
| 音频传输 | 二进制数据面帧,不走 JSON(禁 base64/gzip-in-JSON) |
| 采样率 | 44.1kHz stereo 固定(引擎限制,`PlaybackManager.cs:34`) |
| 哈希 | XXH64,十进制字符串序列化,接收方校验帧 length |
| 发现机制 | %TEMP% 文件只做端口广播,数据全部走 TCP |
| MIDI 输入 | v1 预留不做 |

从 PR #2187 保留的机制:发现文件+端口探测、哈希去重+missingAudios 拉取、心跳+重连退避、playbackStarted flush、part-layout 同步模型。

## 三、里程碑 M1:主仓库 API(下一会话目标)

实现顺序(每步可编译):

1. **M1.1 骨架**:`OpenUtau.Core/DawIntegration/` 目录 + csproj 确认/添加 `K4os.Hash.xxHash` 依赖。
2. **M1.2 DawMessages.cs**:抽象消息基类 + `DawResult<T>` 包装 + 消息集(init/updateUstx/updatePartLayout/getAudio/updateTracks/ping/playbackStarted)。
3. **M1.3 DawTransport.cs**:TCP 连接、控制面行解析(`request:`/`notification:` 接收 + `response:` 匹配 pending)、数据面帧分用、请求超时 10s、心跳监控(2s 轮询 / 15s 死判)。
4. **M1.4 DawServerFinder.cs**:扫 `%TEMP%/OpenUtau/PluginServers/*.json`,端口探测判活,apiVersion 校验。
5. **M1.5 DawAudio.cs**:`part.Mix` 抽取 → float32 PCM(44.1kHz stereo),XXH64,数据面帧头构造;getAudio 响应路径。
6. **M1.6 DawManager.cs**:`DocManager` 订阅(ICmdSubscriber)、debounce 1s/5s、playbackStarted flush、重连退避 500ms/1s/2s、断开 final update。
7. **M1.7 连接 UI**:极简连接对话框(扫描结果列表 + 连接/断开),参照 PR #2187 `DawIntegrationTerminalDialog` 思路自研。
8. **M1.8 测试**:
   - `DawTransportTest`:帧解析、超时、心跳、双平面分用
   - `DebounceTest`:flush 语义
   - `ConformanceClient`:独立测试工具,扮演插件 TCP server,回放/断言消息转录(含二进制帧),端到端验证 init→updatePartLayout→getAudio→playbackStarted 全流程。

**M1 验收**:`dotnet build` 通过;单测全绿;ConformanceClient 完成端到端验证;不破坏现有功能(音量/声像变更、渲染完成事件不误触发)。

## 四、里程碑 M2:独立插件项目(后续会话)

- 新建 repo(建议名 `openutau-daw-bridge`),从 PR #2187 `DawPlugin/` 的 DPF 结构 fork 但**重写协议层**。
- 组件:TCP server(127.0.0.1 动态端口)+ 双平面分用、发现文件发布、请求分发(init/updatePartLayout/getAudio)、音频缓存(hash→PCM,LRU)、DAW 播放输出 + 轨道预览 UI、MIDI 回调预留。
- CI:Windows/macOS/Linux × x64/ARM64 VST3/AU 构建(独立 workflow)。
- 验收:DAW 加载插件 → 连接 OpenUtau → 工程/音频同步 → DAW 播放跟随。

## 五、里程碑 M3:v1.1 预留(暂不排期)

- MIDI 输入方向(消息族如 `notification:midiNotes` / `request:recordMidi`)
- tempo/拍号同步(`notification:tempoMap`)
- ARA 扩展(评估可行性)

## 六、剩余 Open Questions(来自 PROTOCOL.md §14)

1. `apiVersion` 字段实现细节(发现文件 + init 回显)
2. 音频拉取模型:getAudio 请求-响应(已倾向)vs 推送
3. 音频缓存归属:插件侧全权(已倾向)
4. MIDI 方向:v1 明确不做,确认范围
5. 发现文件名规范:`<plugin-name>-<instance>.json`
6. 多客户端:单连接/实例
7. tempo 同步预留位

已解决:XXH64 + 十进制字符串 + length 校验(§14 Resolved)。

## 七、仓库组织建议

```
OpenUtau.Core/DawIntegration/
├── PROTOCOL.md          # 协议契约(v1,权威)
├── DEVELOPMENT_PLAN.md  # 本文件
├── DawMessages.cs
├── DawTransport.cs
├── DawServerFinder.cs
├── DawAudio.cs
└── DawManager.cs
```

插件侧(独立 repo `openutau-daw-bridge`):DPF 插件 + 协议实现(重写)+ CI,不引用主仓库代码。

## 八、已确认的引擎事实(避免重复调研)

- 采样率硬编码:`PlaybackManager.cs:34` `WaveFormat.CreateIeeeFloatWaveFormat(44100, 2)`。
- 渲染输出:`part.Mix.Mix(samplePos, floatBuffer, 0, sampleCount)`,float32 立体声,44.1kHz(参考 PR #2187 `DawManager.UpdateAudio`)。
- 哈希依赖:K4os.Hash.xxHash(`XXH32.DigestOf` 现用于 PR;v1 用 XXH64)。
- 服务发现路径:`%TEMP%/OpenUtau/PluginServers/`(跨平台 per-user temp)。
- 本项目 git 注意:本地主仓库真实 git 目录为 E:/Github/OpenUtau/.git(junction),PortableGit 下给 git 传路径须用 Windows 显式路径;`*.cs` 遵循 .gitattributes(eol=crlf,blob=LF),提交前避免 CRLF blob。

## 九、交接文件清单

| 文件 | 内容 |
|---|---|
| `OpenUtau.Core/DawIntegration/PROTOCOL.md` | 协议契约 v1(实现依据,唯一权威) |
| `OpenUtau.Core/DawIntegration/DEVELOPMENT_PLAN.md` | 本 plan + 下会话 prompt |
| 会话 memory(`.workbuddy/memory/2026-09-01.md`) | 调研过程与决策记录 |
| PR #2187(`add/vst-integration@455d7f9c`) | 机制参考(勿复制代码) |
