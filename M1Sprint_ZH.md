# M1 冲刺工程文档 — Project Breach（星舰覆写）

> **版本日期：** 2026-10-03（基于当日 Assets 静态扫描 + 场景 GUID 引用分析）
> **里程碑：** Milestone 1 — User Evaluation Launch
> **截止：** Week 10 周五 **10 月 9 日 23:59（墨尔本时间）**，剩余 **6 天**
> **配套文件：** 试玩脚本见 `M1UserStory.md` | 架构设计见 `TeamDoc_ZH.md`
> ⚠️ 本文件为团队内部文档，**不要提交进作业仓库**（仓库里只需要 `README.md` GDD + `REPORT.md` + 工程）。

---

## 1. M1 要交什么（官方要求）

| # | 交付物 | 形式 |
|---|---|---|
| 1 | **Evaluation Demo** | WebGL 构建，浏览器（含全屏）从头到尾可玩；不需 polish，但核心玩法必须功能级完整 |
| 2 | **Evaluation Plan** | 填在仓库顶层 `REPORT.md` 对应小节：≥1 种 querying + ≥1 种 observational 技术，每种 ≥5 名参与者 |
| 3 | **Gradescope 提交** | `metadata.json` 加 `gameTitle` + `gameBlurb`（50–1500 字符）；提交 `main` 分支；全员验证 WebGL 链接可玩 |

M1 Demo 目标完成度 = TeamDoc **W6 级**：全房间 + 陷阱 + 沦陷 + 4 波 + 升级 + 完整 UI/音效。
**已决策推迟：** Boss（W7，但代码已超前完成）、Room Passive 房间被动增益（M4 前补回，GDD 需注明）。

---

## 2. 当前进度盘点（2026-10-03 扫描结果）

### 2.1 ✅ 已完成

| 模块 | 内容 |
|---|---|
| 核心层 | `Singleton` / `EventRegistry`（静态类）/ `GameEnums` / `IDamageable` / `IPowerable` / `IUpgradable` |
| 敌人体系 | Enemy / EnemyMover（路点式，无 NavMesh 依赖）/ EnemyAttack / EnemyHealth / **RangedEnemy** / **Boss（超前完成 W7）** / Spawner ×7（Compose 场景） |
| 波次系统 | WaveManager 完整：波次循环、休整期 20s、`RequestEarlyStart()` 提前开始、胜利判定 → `EndGame(true)` |
| 房间 | 5 个 Room 组件（A 走廊 / B 气闸 / C 走廊 / D 商店 / E 服务器机房）+ MainControlRoom（CoreHP 50，Breach 扣血） |
| 战斗基础 | Turret 索敌/射击/受伤/死亡 → 返还链路（T05/T06 逻辑在） |
| 监控 UI | 相机切换 + RenderTexture 监控墙 + 电力条 + 消息弹窗（电力不足） |
| 结算 | GameOverScreen：Victory / GameOver / 重新开始 |
| Shader | 5 个手写 shader（PlasmaProjectile / BreachRoom / HitReaction / TurretBase / TurretTop），M4 储备 |

### 2.2 🔴 Blocker（不修 M1 不成立）

| # | 问题 | 证据 | 负责人 |
|---|---|---|---|
| B1 | **Compose 场景缺 PowerManager** | 场景 GUID 扫描 0 命中；Singleton 无懒创建；只有旧 Demo 场景有 | 综合 |
| B2 | **电力系统被旁路** | `TurretToggleButton.cs:27` `usePowerSystem = false` | 玩家 |
| B3 | **Assets 体积余量仅 ~2MB** | 298→248MB（A 类已删 50MB），音频还没加 | 综合 |
| B4 | **入口场景是空壳** | 官方要求 `Assets/StartScene.unity`（根目录）只有 21KB 模板占位；游戏在 `Scenes/Compose.unity`（320KB，含 WaveManager/5 Room/7 Spawner） | 综合 |

### 2.3 🟡 功能缺口

| # | 缺什么 | 现状 | TeamDoc 归属 |
|---|---|---|---|
| F1 | **Trap 全套**（Offline/Armed/Cooling 状态机、AOE+减速、prefab、冷却 UI 数据源） | 只有 `TrapState` 枚举 | 战斗 W4 |
| F2 | **升级系统**（UpgradeManager / UpgradeData / 商店 UI / 点数经济） | `IUpgradable` 在 `Turret.cs:11` 被注释（"removed for W2 Demo"） | 战斗+玩家 W6 |
| F3 | **音频为零**（AudioManager + 音效 + 音量控制） | 全工程 0 个音频文件 | 综合（M1 官方明确要求音效） |
| F4 | **HUD 缺口**（波次横幅、休整倒计时+提前开始按钮、CoreHP 显示） | 后端事件全部现成，只差 UI | 玩家 W5 |
| F5 | **主菜单流程**（主菜单 → 开始 → Compose） | 只有结算画面 | 综合 |

### 2.4 🔵 合规/技术债

- 中文注释 6 文件待清理（违反团队硬性规定）：`Singleton.cs`（含 Debug 中文字符串）、`TurretToggleButton.cs`、`RangedEnemy.cs`、`EnemyHealth.cs`、`Boss.cs`、`Turret.cs`
- 无对象池：Turret/RangedEnemy/Spawner 全 `Instantiate`（§12.6 自定的坑，WebGL 性能隐患，M1 后补）
- 拼写：`BreachContoller.cs`、场景物体 `Tuerrt 1/2 Button`
- `EnemyStats` SO 无实例（数值直接写 prefab）——可接受，TeamDoc/GDD 注明即可

---

## 3. Assets 瘦身状态

| 阶段 | 体积 | 说明 |
|---|---|---|
| 起始 | 298MB | 超 250MB 硬限制 |
| A 类已删（10-03） | **248MB** | stylized-turrets-pack 目录 28M（0 引用，GLB 版内嵌贴图在用）、Untitled.fbx 9.5M、Walls.png 6.5M、TMP Examples 6.5M、avif、_Recovery、StartScene_old、.DS_Store；删除后全工程 GUID 引用扫描确认零误伤 |
| C 类待做 | 目标 ≤225MB | 音频需 5–10MB 余量。**优先两项：** `MainRoom.fbx`（29M）重导出剥内嵌贴图；`server_room_high_poly.glb`（34M）Draco/减面。备选：4K 贴图批量降 2K（ImageMagick `mogrify -resize 2048x2048`）、`Police Enforcer Var.fbx`（26M）重导出、动画三件套（各 14M）重采样 |

⚠️ 250MB 按 **GitHub 仓库**算：删除和压缩都必须 **commit** 才生效。Unity 导入设置的压缩**不算数**，必须改小源文件本身。

---

## 4. 六天冲刺计划（10/3 → 10/9）

### D1 · 10/3（今天）— 通血脉
- [ ] 【综合】B1：Compose 场景 Managers 物体挂 `PowerManager`，确认 UIManager 电力条引用接通
- [ ] 【玩家】B2：`usePowerSystem` 打开，过 T02/T03/T04（分配/不足弹窗/手动关停返还）
- [ ] 【综合】定 B4 方案：根 `StartScene.unity` 重建为主菜单（加载 Compose），Build Settings 注册两场景
- [ ] 【综合】启动 C 类压缩（MainRoom.fbx / server_room glb 重导出）

### D2 · 10/4 — 陷阱 + HUD
- [ ] 【战斗】F1：Trap 全套（§6.3：AOE 3m / 伤害 20 / 减速 50%×2s / 冷却 8s / 触发后 Refund）
- [ ] 【玩家】F4：波次横幅（订 `BroadcastWaveStart`）、休整倒计时 + 提前开始按钮（调 `RequestEarlyStart`）、CoreHP 显示
- [ ] 【全员】中文注释清理（6 文件，英文注释解释 why）

### D3 · 10/5 — 升级 + 音频
- [ ] 【战斗】F2：解开 Turret 的 `IUpgradable` 注释；`UpgradeManager` + `UpgradeData`（§8.5/9.4：点数 100 起、cost 50 / +20HP / +10dmg）；击杀 → 点数接线
- [ ] 【玩家】升级商店 UI（波次间隙可用）
- [ ] 【综合】F3：AudioManager（§8.7）+ 音效资产入库（开火/命中/警报/结算/BGM），音量控制

### D4 · 10/6 — 端到端打通
- [ ] 【敌人】4 波 WaveData 数值（多路线、压力曲线；校验 Spawner 与 WaveManager 配比）
- [ ] 【综合】按 `M1UserStory.md` 15 步全流程走查：Breach → 主控室拦截 → CoreHP → GameOver / 4 波全清 → Victory
- [ ] 【综合】StartScene 主菜单定稿；沦陷房间监控黑屏反馈确认

### D5 · 10/7 — 构建 + 评估计划
- [ ] 【综合】WebGL 本地构建 + Gradescope **试提交**（autograder 全绿、链接可玩、全屏正常、30fps+）
- [ ] 【全员】评估会议：REPORT.md Evaluation Plan 定稿（querying=问卷；observational=出声思维观察；各 ≥5 人；任务直接用 `M1UserStory.md` 步骤 + 观察点表）
- [ ] 【全员】修 bug

### D6 · 10/8 — 提交日（提前一天，留缓冲）
- [ ] 【综合】`metadata.json` 加 `gameTitle` / `gameBlurb`（50–1500 字符）
- [ ] 【综合】正式提交 main 分支 → Gradescope，队长加全员进 submission
- [ ] 【全员】各自验证：提交可见、tests 全绿、**WebGL 链接实机可玩+全屏**、REPORT.md 已填
- [ ] 【综合】README.md（GDD）同步到当前状态（标注 Room Passive 为后续实现）

### 10/9（截止日）
- 不改代码。只做最终确认。仓库在 deadline 后自动锁写。

---

## 5. 验收对照表

| 官方 M1 要求 | 对应本工程 | 状态 |
|---|---|---|
| Game environment | 5 房间 + 主控室 + 路点巡逻路线 | ✅（待 B4 入口修复） |
| Gameplay mechanics | 电力分配（B1+B2）、炮塔、陷阱（F1）、波次胜负、Breach、升级（F2） | ⚠️ 半成品 |
| UI and sounds | 监控墙/电力条 ✅；波次横幅/CoreHP（F4）；音效（F3） | ⚠️ 缺音频 |
| playable start to finish | WaveManager→Victory / CoreHP→GameOver 链路在 | ⚠️ 待 D4 走查 |
| WebGL + 全屏 + Gradescope | 未验证 | ❌ D5 |
| GDD 同步 | README.md 在仓库根（本机无副本） | ❌ D6 |

---

## 6. 备注

- 本机只有 Assets 同步副本（`Breach - Unity Actual/`），`README.md` / `REPORT.md` / `metadata.json` / ProjectSettings 不在扫描范围内，以仓库实际状态为准。
- 场景结论来自 YAML GUID 静态分析，B1/B2/B4 需在 Unity 编辑器里最终确认。
- 旧 `Scenes/Demo.unity`（W2 遗产，含唯一旧 PowerManager）在 B1 修完验证后再删。
- IOA 预警：M2 口试每人 5 分钟讲 Demo + 评估计划，各自负责的模块必须能讲清（考官会假装测试用户，走 `M1UserStory.md` 流程）。
