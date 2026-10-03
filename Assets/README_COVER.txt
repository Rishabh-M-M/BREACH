Project Breach — M1 Sprint 覆盖包（2026-10-03，无陷阱版）
================================================================

【用法 — 3 步】

1. 关闭 Unity（或保证它不在写文件）。
2. 把本包里的【全部内容】（Scripts / Prefabs / Data / Audio / Scenes /
   StartScene.unity / Data.meta / Audio.meta）整体拖进你项目的
   Assets 文件夹，全部选"覆盖/替换"。
   ※ 包内目录结构与 Assets 完全对应：包根 = Assets 根。
3. 打开 Unity，等编译完成。

【为什么大部分文件不带 .meta】
.cs / .unity / .prefab 的 .meta 被故意排除——你们的 .meta 里有场景引用
正在使用的 GUID，覆盖它们会导致全线 Missing Script。包里没有这些 .meta，
你们的 GUID 原样保留，引用不断。
例外：Data/ 和 Audio/ 是全新资产，必须连 .meta 一起复制（GUID 已被
Compose 场景引用，不能改）。

【必须手动删除的文件】（在 Unity 的 Project 窗口里删，meta 会自动清理）
  □ Assets/TextMesh Pro/Examples & Extras        （整个文件夹）
  □ Assets/Art/Models/stylized-turrets-pack      （整个文件夹，28MB）
  □ Assets/Art/Models/Robot_Melee/Untitled.fbx
  □ Assets/Art/Backgrounds/Walls.png
  □ Assets/Materials/dark-abstract-background_1048-1920.jpg.avif
  □ Assets/_Recovery                             （整个文件夹）
  □ Assets/Scenes/StartScene_old.unity
  □ Assets/Scripts/Player/BreachContoller.cs     （旧拼写残留！若存在必删，
                                                   否则 CS0101 重复类报错）
  □ 各处的 .DS_Store（如有）

【覆盖包包含什么】
  · 17 个 .cs：全部代码补丁（S1 友军误伤双保险 / S2 电力单返还 / M1–M6）
    + 5 个地基文件新版（EventRegistry 11 事件 / IUpgradable.TryApplyUpgrade /
    WaveManager.EarlyStartEnabled / EnemyMover.ApplySlow / EnemyHealth 双广播）
    + 全工程中文注释清零
  · 2 个炮塔 prefab：powerCost 10 → 50（对齐 maxPower 100，第 3 座必弹"电力不足"）
  · Scenes/Compose.unity：GameManager 物体已挂 PowerManager(100) /
    UpgradeManager / AudioManager；usePowerSystem ×2 已打开；
    AudioManager Clip Table 已接 4 条音效；无陷阱（已按决策移除）
  · StartScene.unity（根）：EventSystem + Canvas + START 按钮 + MainMenu
  · Data/：Turret_Damage_Up / Turret_HP_Up（50 点 / maxLv3，已接到 UpgradeManager）
  · Audio/：4 条占位音效（键名 WaveStart/Breach/Victory/GameOver，
    后续可用真实音效同名替换）

【覆盖后必做 4 步】
  1. Console 确认 0 编译错误（Burst/cecil 那类报错是级联，编译通了就消失）。
  2. File > Build Settings：把 Assets/StartScene.unity 拖为 index 0，
     Assets/Scenes/Compose.unity 拖进列表——不注册则菜单/结算无法跳场景。
  3. Play Compose 验证：电力条 0/100 → 开炮塔 50/100 → 第 3 座弹"不足" →
     关停返还 → 同房炮塔不互射 → 完整打一局（胜/负各一次）→ 听 4 条音效。
  4. git add -A && commit && push，全员 pull。
     ※ 同步文件夹今天三次回滚过 .cs，只有 git 能终结重复劳动。

【应急】
  · 若某物体出现 Missing Script：说明那个脚本的 GUID 与你们仓库不一致，
    把报错截图/文件名发回，单独处理，不要大面积乱拖。
  · 若 UI 按钮没反应：检查 Assets 根目录是否有
    InputSystem_Actions.inputactions（Unity 6 模板自带）；缺失则在
    Project Settings > Input System 里点 "Create Actions Asset"。
