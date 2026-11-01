# 敌对波次生成机制设计（Wave System Design）

> 项目：ProjectBreach（2026s2-project-2-techtonic）
> 状态：设计已确认，待实现
> 本文档为实现前的最终设计定稿，对应实施步骤见文末。

---

## 1. 概述

在现有 `EnemySpawner_Single`（多怪共享目标点版）之上，构建一套**总控指挥多 spawner 实例**的波次生成系统：新增唯一大脑 **WaveManager**，按波次配置指挥场景中的多个 spawner 执行生成；spawner 本身改动极小，只开放一个公开入口。

---

## 2. 已确认的设计决策

| # | 决策点 | 结论 |
|---|--------|------|
| D1 | 突破（Breach）的敌人是否算死亡 | **算**。突破怪计入波次结算（走 `OnEnemyResolved` 通道），伤害链路不变 |
| D2 | 休整期如何结束 | **倒计时 + 玩家按钮提前开波** |
| D3 | 对象池 | **本期不做**，沿用 `Instantiate/Destroy` |
| D4 | Boss / `RequestSpawnAdds` | **本期不涉及**，只有基础小怪 |
| D5 | 架构形态 | 总控（WaveManager）存在，但**执行的仍是现有 EnemySpawner_Single**；波次配置数据在总控手里 |
| D6 | 波次配置载体 | WaveManager 的 **Inspector 序列化数据**（本期不做 ScriptableObject） |
| D7 | UI 打磨（M4：横幅/音效/生成特效） | **本期不做**，只保证提前开波按钮可用 |

---

## 3. 架构与角色分工

```
WaveManager（新增，唯一大脑，Singleton 基建）
 │
 │  持有波次配置 + 结算记账 + 休整倒计时 + 提前开波入口
 │
 │ 直接调用 ↓                        EventRegistry 广播 ↓
 │
EnemySpawner_Single × N（每个入口一个）      UI / GameManager
  只新增一个公开方法：SpawnWave(int count)
  原有参数与 SpawnEnemy、共享 target point 逻辑全部不动
```

- **WaveManager = 唯一发号施令者**：决定什么时候开波、哪个口出多少只、组间错峰、何时清空、何时休整、何时胜利。与现有 `Singleton<T>` 基建一致，放在 `Assets/Scripts/Managers/`。
- **EnemySpawner_Single = 纯执行器**：每个守一条路线/门口，用自己 Inspector 上现成的 `enemyPrefab / targetPoints / spawnInterval / aimPoint` 执行生成。**每个 spawner 管自己那条路出什么怪，总控管节奏和数量。**
- **EnemyMover / EnemyAttack / EnemyTargetPoint：零改动**，完全不知道波次系统的存在。
- `EventRegistry` 中已预留的 `OnWaveStart(int)` 与 `OnAllWavesCleared` 正是为本系统预留的接口，本次直接点亮。

---

## 4. 改动文件清单

| 文件 | 改动量 |
|------|--------|
| `Assets/Scripts/Managers/WaveManager.cs` | **新增**（唯一的新文件） |
| `Assets/Scripts/Entities/Enemy/EnemySpawner_Single.cs` | +1 个 `autoStart` bool 守卫、+1 个公开方法 `SpawnWave(int)` |
| `Assets/Scripts/Entities/Enemy/EnemyHealth.cs` | `Die()` 开头 +1 行广播 |
| `Assets/Scripts/Entities/Enemy/Enemy.cs` | `Breach()` +1 行广播（带 `IsDead` 防重守卫） |
| `Assets/Scripts/Core/EventRegistry.cs` | +1 个事件 `OnEnemyResolved` |
| `Assets/Scripts/Managers/GameManager.cs` | +1 个 `SetWaveClearState()` 小方法 |

**不动**：`EnemySpawner`（独占 target point 版）、`EnemyMover`、`EnemyStats`、`GameOverScreen`（Victory 链路现成，零改动接通）。

---

## 5. WaveManager 参数设计（Inspector 配置）

```csharp
// 全局节奏
firstWaveDelay      // 开局到第 1 波的等待（给玩家摆塔）
restDuration        // 波间休整倒计时时长
earlyStartEnabled   // 是否允许玩家提前开波

// 波次配置
waves : List<Wave>
  Wave      = List<WaveGroup>
             + float duration            // 该波存活时长（时间制节奏核心）
             + bool  advanceEarlyOnClear // 全清后是否提前收波（默认 false = 严格时间制）
  WaveGroup = {
      EnemySpawner_Single spawner;   // 引用现有 spawner 实例
      int   count;                   // 该口该波出几只
      float startDelay;              // 组间错峰（左门先动、右门后动）
  }
```

公开入口：

- `RequestEarlyStart()`（UI 按钮绑此方法，附带 `[ContextMenu]` 便于测试）

---

## 6. EnemySpawner_Single 改动设计

现有字段一个不动（`enemyPrefab / targetPoints / spawnInterval / aimPoint` 语义不变，共享 target point 逻辑保留）：

- 新增 `[SerializeField] private bool autoStart = false;` 守卫现有 `Start()` 自动生成（老 Demo 场景行为兼容）；
- 新增公开方法 `SpawnWave(int count)` —— 内部复用现有 `SpawnRoutine`，跑 `count` 次。

---

## 7. 结算记账机制（唯一的新机制）

`EventRegistry` 新增无参全局事件 **`OnEnemyResolved`**，两个广播点：

1. **`EnemyHealth.Die()` 开头** —— 被塔击杀，死亡瞬间立即计入（不等 `despawnDelay` 的尸体消失动画，避免休整期被拖慢）；
2. **`Enemy.Breach()`** —— 突破冲核的怪也算结算（决策 D1）；先广播结算，再走原 `GameManager.NotifyBreach → MainControlRoom` 伤害链路。

`EnemyHealth` 已有 `IsDead`，`Breach()` 广播前加防重守卫，避免已死怪被二次触发双计数。

WaveManager 记账逻辑：`已下令生成数 == 已结算数` → 本波清空。

**多实例注意**：`OnEnemyResolved` 是全局无参事件，WaveManager 是唯一订阅者，天然没有串账问题。

---

## 8. 运行流程

```
GameManager.StartGame()
      ▼
WaveManager 等 firstWaveDelay
      ▼
┌→ BroadcastWaveStart(i)（EventRegistry 现成事件，点亮）
│    对每个 group：spawner.SpawnWave(count)（组间按 startDelay 错峰）
│    时间制节奏：等 wave.duration 秒，到点即开下一波（不依赖全灭/全结算）
│    ├─ 可选：advanceEarlyOnClear=true 且本波已全清 → 提前收波
│    ├─ 最后一波时间到 → BroadcastAllWavesCleared → GameManager.EndGame(true)
│    │                                        （GameOverScreen 收到 Victory，链路现成）
│    └─ 还有下一波 → GameManager.SetWaveClearState() → 休整倒计时
│                    （可被 RequestEarlyStart 打断）
└─── 倒计时结束 / 玩家按键 → 下一波
```

---

## 9. 具体运行示例（时间轴）

示例配置：`firstWaveDelay = 5s`，`restDuration = 20s`；
Wave 1 / 2 / 3 的 `duration = 30s`，`advanceEarlyOnClear = false`（严格时间制）：

- Wave 1：[ 左门 spawner → 3 只 ]
- Wave 2：[ 左门 spawner → 2 只（delay 0s），右门 spawner → 2 只（delay 4s）]
- Wave 3（最后一波）：[ 左门 spawner → 4 只 ]

```
t=0    Play。GameManager.StartGame() → state=Playing
       两个 spawner 因 autoStart=false 原地待命。

t=5    BroadcastWaveStart(0)
       左 spawner.SpawnWave(3) → t=5 / t=6 / t=7 各出一只（走原 EnemyMover 逻辑）。

t=35   Wave 1 duration(30s) 到点 → 开下一波（无论是否还有怪在场）
       → GameManager.SetWaveClearState() → 休整倒计时 20s

t=55   休整结束（或玩家提前开波）→ BroadcastWaveStart(1)
       左组 SpawnWave(2)；右组等 startDelay 4s 后 SpawnWave(2)。

t=85   Wave 2 duration(30s) 到点 → 休整倒计时（Wave 1/2 残留怪继续被击杀/突破）

t=105  休整结束 → Wave 3（最后一波）：BroadcastWaveStart(2)，左门出 4 只。

t=135  Wave 3 duration(30s) 到点 → BroadcastAllWavesCleared
       → GameManager.EndGame(true)
       → BroadcastGameStateChanged(Victory)
       → GameOverScreen（零改动）timeScale=0、暂停音频、0.75s hit-stop 后弹 VICTORY。
```

> 说明：时间制下，前一波的残留怪会进入下一波继续存在，结算会计因此不再作为“开波”依据，
> 仅当 `advanceEarlyOnClear=true` 时才用于提前收波（且残留怪可能让提前收波提前触发，见字段注释）。

### 边界情况

**核心中途炸掉**：`MainControlRoom.Die() → GameManager.EndGame(false)` → `timeScale = 0` → WaveManager 协程、倒计时、所有 SpawnRoutine 全部原地冻结（都跑在 scaled time 上，天然被冻）→ 弹 GAME OVER。

**玩家 Restart**：`StartGame()` → GameOverScreen 收面板恢复 timeScale → 场景重载 → WaveManager 重建、从 `firstWaveDelay` 重新跑，无需额外重置代码。

---

## 10. 实施步骤

| 步骤 | 内容 |
|------|------|
| **S1** | `EventRegistry` 加 `OnEnemyResolved`；`EnemyHealth.Die()` / `Enemy.Breach()` 各补一行广播（带防重守卫） |
| **S2** | `EnemySpawner_Single` 加 `autoStart` 守卫 + 公开方法 `SpawnWave(int count)` |
| **S3** | 新增 `WaveManager.cs`（Inspector 波次配置、错峰执行、结算记账、休整倒计时、提前开波、Victory 收尾）；`GameManager` 补 `SetWaveClearState()` |

---

## 11. 验收清单

- [ ] Play 模式跑一个 2~3 波的短配置，波次按配置顺序开波；
- [ ] 同一波内多个 spawner 按 `startDelay` 错峰出兵；
- [ ] 波次按 `duration` 时间到点自动开下一波（**不依赖**全灭/全突破）；
- [ ] 残留怪跨波继续存在；`advanceEarlyOnClear` 开启时全清可提前收波；
- [ ] 休整期倒计时正常结束开下一波；按按钮（或 ContextMenu）可立即开下一波；
- [ ] 最后一波清空后弹出 Victory（走 `OnAllWavesCleared → EndGame(true)`）；
- [ ] 核心中途被摧毁 → GAME OVER，一切冻结；Restart 后波次从头跑；
- [ ] 老 Demo 场景里手放的 spawner（autoStart 打开）行为不变。

---

*本文档由设计讨论定稿生成（2026-05），实现时如有出入请回改此文档。*
