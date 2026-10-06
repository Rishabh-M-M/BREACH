# M1 Debug 工程单 — Project Breach

> **来源：** 2026-10-03 全量代码审核（33 个 .cs / 约 4900 行）
> **用法：** 每条 bug = 症状 → 根因（文件:行）→ 补丁代码 → 验证步骤 → 签收录。
> **代码纪律：** 所有补丁注释为英文（团队硬性规定）；本文件是团队内部文档，**不进作业仓库**。
> ⚠️ 场景类结论基于同步副本的 YAML 静态分析，动手前在 Unity 里先确认同步是最新的。

---

## 修复总览

| ID | 严重度 | 问题 | 负责建议 | 状态 |
|---|---|---|---|---|
| S1 | 🔴 | 友军误伤链（Turret/Trap/Projectile 不过滤敌我） | 战斗 | ☐ |
| S2 | 🔴 | 电力双倍返还（Turret.Die 直接 Refund） | 战斗 | ☐ |
| S3 | 🔴 | Compose 场景接线全缺（3 Manager/Trap/usePowerSystem/菜单） | 综合 | ☐ |
| S4 | 🔴 | 数据资产为零（UpgradeData/音频） | 战斗+综合 | ☐ |
| M1 | 🟡 | PurchaseDefault 只扣点不加成 | 战斗 | ☐ |
| M2 | 🟡 | 调试键 B 留在 Build 里 | 综合 | ☐ |
| M3 | 🟡 | RoomExitZone 漏检子物体 collider | 敌人 | ☐ |
| M4 | 🟡 | MainMenu 音量初始化方向反了 | 玩家 | ☐ |
| M5 | 🟡 | 热路径 Debug.Log 刷屏 | 全员 | ☐ |
| M6 | 🟡 | GameOver 后鼠标视角仍响应 | 玩家 | ☐ |
| N1 | 🟢 | 陈旧注释/数值漂移集中清理 | 全员 | ☐ |

---

## S1 🔴 友军误伤链

**症状：** 炮塔锁定并射击己方炮塔/主控室；陷阱触发时把范围内己方炮塔炸掉 20 血。
**根因：** `IDamageable` 由 Turret/Enemy/MainControlRoom 共同实现，而三处索敌/判定均无阵营过滤：
- `Turret.cs` `AcquireTarget()` — `Physics.OverlapSphere(transform.position, attackRange)` 无 LayerMask，取"最近存活 IDamageable"
- `Trap.cs` `OnTriggerEnter()` + `TriggerTrap()` — 触发与爆炸 OverlapSphere 同样无过滤
- `Projectile.cs` 碰撞结算对任何 IDamageable 生效（风险较低，随 S1 主修受益）

### 补丁（LayerMask 方案）

**第 0 步（编辑器）：** 新建 `Enemy` 层 → 两个机器人 prefab 根物体设为该层（子弹/炮塔不需要改）。

**Turret.cs：**

```csharp
[Header("Target Filtering")]
[Tooltip("Only colliders on these layers can be targeted. Set to the Enemy layer so
         turrets can never lock onto friendly structures (friendly IDamageables exist:
         other turrets and the MainControlRoom).")]
[SerializeField] private LayerMask targetMask;

private void AcquireTarget()
{
    Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, targetMask);
    float nearestDist = float.MaxValue;
    IDamageable nearest = null;

    foreach (var hit in hits)
    {
        // Never target ourselves, regardless of layer setup.
        if (hit.transform.root == transform.root) continue;

        if (hit.TryGetComponent(out IDamageable target) && !target.IsDead)
        {
            float dist = Vector3.Distance(transform.position, hit.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = target;
            }
        }
    }

    currentTarget = nearest;
}
```

**Trap.cs**（两处）：

```csharp
[Header("Target Filtering")]
[Tooltip("Trigger and blast only affect these layers (Enemy). Without this the AOE
         damages friendly turrets standing inside the blast radius.")]
[SerializeField] private LayerMask targetMask;

private void OnTriggerEnter(Collider other)
{
    if (_state != TrapState.Armed || !IsActive)
    {
        return;
    }

    // Layer gate first: friendly structures inside the trigger volume must not set it off.
    if ((targetMask.value & (1 << other.gameObject.layer)) == 0)
    {
        return;
    }

    IDamageable triggerer = other.GetComponentInParent(typeof(IDamageable)) as IDamageable;
    if (triggerer == null || triggerer.IsDead)
    {
        return;
    }

    TriggerTrap();
}

// In TriggerTrap():
Collider[] hits = Physics.OverlapSphere(transform.position, triggerRadius, targetMask);
```

**验收：** ① 同房双炮塔通电，全程无互相开火；② 陷阱旁放一座炮塔，触发后炮塔 HP 不变、敌人正常受伤减速。

---

## S2 🔴 电力双倍返还

**症状：** 炮塔被毁后电力返还两次（40 电的炮塔返 80），关停中的炮塔被毁也返钱。
**根因：** `Turret.cs` `Die()` 自己调了 `PowerManager.Instance.Refund(powerCost)`，随后 `OwnerRoom.NotifyStructureDestroyed(this)` 里 `Room` 又 Refund 一次。TeamDoc 契约：返还统一走 Room 中介（且 Room 会检查 `IsActive`）。

### 补丁（Turret.cs `Die()`）

```csharp
public void Die()
{
    // Power refund is owned by Room.NotifyStructureDestroyed (Indirection), which
    // refunds only when this turret was active. Refunding here as well double-pays
    // the player and silently inflates the power budget.
    OwnerRoom?.NotifyStructureDestroyed(this);

    Destroy(gameObject);
}
```

**验收（T06 回归）：** 激活 40 电炮塔 → 电力 40/50 → 敌人打爆 → 返还后 **40→0**，不是 40→0 溢出/负数补偿；未激活炮塔被毁**不返钱**。

---

## S3 🔴 场景接线（编辑器操作，无代码补丁）

> 当前同步的 `Compose.unity` 中 PowerManager / UpgradeManager / AudioManager / Trap **全部为 0 命中**；`usePowerSystem = 0`；根 `StartScene.unity` 无 MainMenu。

**步骤：**
1. ☐ Compose：`Managers` 物体上 AddComponent → `PowerManager`（数值定案：TeamDoc 100/炮塔 50 或现值 50/40，全队对齐一个版本）
2. ☐ Compose：同物体加 `UpgradeManager`（startingPoints 100 / pointsPerKill 10）
3. ☐ Compose：同物体加 `AudioManager`（暂不挂 clip 也能跑，null-safe）
4. ☐ Compose：选中所有 `TurretToggleButton` → **勾上 `usePowerSystem`** → 过 T02/T03/T04
5. ☐ 制作 Trap prefab（触发 collider + Trap 组件 + 外观）并在各房间摆放；确认玩家有布防/通电的 UI 入口（现有 TurretToggleButton 只认 Turret 类型，需要 Trap 版按钮或自动通电策略）
6. ☐ 根 `Assets/StartScene.unity`：建 Canvas + MainMenu 组件，`gameScenePath = Assets/Scenes/Compose.unity`
7. ☐ `File > Build Settings`：加入 `Assets/StartScene.unity`（index 0）和 `Assets/Scenes/Compose.unity`
8. ☐ 检查 UIManager 的 powerBar/powerText 引用、各 HUD 引用已拖入

**验收：** 空引用 zero；电力条随开关炮塔实时变化；菜单→开始→进 Compose。

---

## S4 🔴 数据资产

1. ☐ Project 右键 `Create > ProjectBreach > UpgradeData` 建两个资产：
   - `Turret_Damage_Up`：cost 50 / damageBonus 10 / maxLevel 3
   - `Turret_HP_Up`：cost 50 / hpBonus 20 / maxLevel 3
   拖到 `UpgradePointsHUD.defaultUpgrade`（及商店各槽位）。
2. ☐ **先完成 C 类压缩**（MainRoom.fbx 29M / server_room glb 34M，见 `M1Sprint_ZH.md` §3），再导入音频（预算 5–10MB）：
   - 必备 6 条：开火 / 命中 / 波次开始(键 `WaveStart`) / 突破警报(键 `Breach`) / 胜利(键 `Victory`) / 失败(键 `GameOver`)
   - 填入 AudioManager 的 Clip Table（key 必须与 Event Bindings 的键一致）

---

## M1 🟡 PurchaseDefault 只扣点不加成

**根因：** `UpgradeManager.cs` `PurchaseDefault()` 扣了点数但从未调用 `target.TryApplyUpgrade`。

```csharp
[Header("Defaults")]
[Tooltip("Asset applied by PurchaseDefault, so the fallback path grants a real bonus
         instead of only deducting points.")]
[SerializeField] private UpgradeData defaultUpgrade;

public bool PurchaseDefault(IUpgradable target)
{
    // Route through Purchase: affordability/level checks and the actual bonus
    // application stay in one place.
    return Purchase(target, defaultUpgrade);
}
```

---

## M2 🟡 调试键 B 泄漏到 Build

**根因：** `BreachController.cs` `Update()` 每帧检测 `KeyCode.B`，玩家按 B 即触发入侵特效。

```csharp
#if UNITY_EDITOR
// Manual test key kept editor-only so a shipped build can never trigger it.
if (Input.GetKeyDown(KeyCode.B))
{
    StartBreach();
}
#endif
```

---

## M3 🟡 RoomExitZone 漏检

**根因：** `RoomExitZone.cs` 用 `other.TryGetComponent(out Enemy)`，敌人 collider 在子物体时漏检；`BreachZone` 已修过同样的问题但没同步到这里。

```csharp
// GetComponentInParent so the trigger still finds the Enemy when the collider
// belongs to a child object (same fix BreachZone already has).
Enemy enemy = other.GetComponentInParent<Enemy>();
if (enemy == null)
{
    return;
}
```

---

## M4 🟡 MainMenu 音量初始化方向反了

**根因：** `MainMenu.cs` `Start()` 注释写"从 AudioManager 读取"，代码却把滑块默认值推给 AudioManager，打开菜单即重置音量。

**两步修：**

```csharp
// AudioManager.cs — add a getter so UI can read the live value.
public float MasterVolume => masterVolume;
```

```csharp
// MainMenu.cs Start() — seed the slider FROM the manager (was: the other way round).
if (AudioManager.Instance != null && masterVolumeSlider != null)
{
    masterVolumeSlider.value = AudioManager.Instance.MasterVolume;
}
```

---

## M5 🟡 热路径日志刷屏

**根因：** Turret 每发/每伤、Projectile Awake/命中、Room 注册均 `Debug.Log`——WebGL 下 console I/O 是实打实的帧率税。

**建议模式（全工程统一）：**

```csharp
// Verbose combat logging is a WebGL performance tax (console I/O every shot).
// Keep it behind a flag so it can be toggled for debugging without a rebuild.
private static bool verboseLogging = false;

// then, at each hot-path log site:
if (verboseLogging)
{
    Debug.Log($"[Turret] Fired at target, damage: {damage}");
}
```

优先清理：`Turret.Shoot/TakeDamage`、`Projectile.Awake/Initialize/HitTarget`、`Room.RegisterStructure`。

---

## M6 🟡 GameOver 后鼠标视角仍响应

**根因：** `CameraController.Update()` 每帧轮询鼠标，timeScale=0 不停输入。

```csharp
// CameraController.cs — gate mouse look on game state.
private void OnEnable()  { EventRegistry.OnGameStateChanged += HandleState; }
private void OnDisable() { EventRegistry.OnGameStateChanged -= HandleState; }

private void HandleState(GameState state)
{
    // Look input only makes sense while a run is actually playing.
    enabled = (state == GameState.Playing || state == GameState.WaveClear);
}
```

（注意 `enabled` 切换会停掉 `Update`，即视角冻结；菜单/结算时由 UI 接管光标。）

---

## N1 🟢 注释与数值清理（顺手包）

- `DefensiveStructure.cs` 头注释 "Temporary shell, to be deleted..." → 已是生产基类，改为正式描述
- `PowerManager.cs` 头注释 "Mock simplified version..." → 同上
- 数值对齐一次过：TeamDoc 100/50 vs 现 50/40；`Turret` 注释 "Range 80m" 实为 8f；`EnemyAttack` 10 dmg、`RangedEnemy` 硬编码 0.5s/10 dmg
- `EnemyMover.ApplySlow` 注释 "strongest wins" 实为 "latest wins"——改注释或取 `Mathf.Min`
- `GameEnums.cs` 删掉多余的 `using UnityEngine;`

---

## 总回归清单（全部修复后按序过）

1. ☐ T01–T04：切换视角 / 通电占电 / 电力不足弹窗 / 手动关停返还
2. ☐ T05 炮塔自动索敌击杀（**且不攻击己方**——S1 验收）
3. ☐ T06 战损返还**单次**（S2 验收）
4. ☐ 陷阱：通电 Armed → 触发 AOE+减速（不伤己方）→ 冷却 → 自动 re-arm
5. ☐ 升级：击杀 +10 点 → 购买 +伤害/+HP 生效；Breach 敌人**不**产点
6. ☐ Breach：传送主控室 → 拦截/警报音效 → CoreHP 扣 10（HUD 同步）
7. ☐ 4 波全清 → Victory；CoreHP 0 → GameOver；Restart / 回菜单正常
8. ☐ `M1UserStory.md` 15 步全流程走查
9. ☐ WebGL 构建 + Gradescope 试提交（30fps、全屏、autograder 绿）
