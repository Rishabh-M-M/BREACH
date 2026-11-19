# Game Design Document (GDD): BREACH







![Breach logo](./Images/Logo.png)







**Game and Team logos are AI generated via ChatGPT.**







> **Status:** Updated design draft



> **Last updated:** 4 November 2026


![Breach logo](./Images/team.png)

> **Team:** Techtonic (Members: Rongze Han, Marcus Minnelli, Rishabh Maheshwari, Hitarrth Chheda)







---

## Table of Contents

1. [Game Overview](#1-game-overview)



2. [Story and Narrative](#2-story-and-narrative)



3. [Gameplay and Mechanics](#3-gameplay-and-mechanics)



4. [Levels and World Design](#4-levels-and-world-design)



5. [Art and Audio](#5-art-and-audio)



6. [User Interface (UI)](#6-user-interface-ui)



7. [Technology and Tools](#7-technology-and-tools)



8. [Team Communication, Timelines and Task Assignment](#8-team-communication-timelines-and-task-assignment)



9. [Possible Challenges and Future Implementations](#9-possible-challenges-and-future-implementations)







---

## 1. Game Overview

**Core Concept**







BREACH places the player in control of NEXUS, a spacecraft under attack by an unknown mechanical enemy force. Confined to the Main Control Room, the player monitors four external rooms through surveillance feeds and allocates shared Power to pre-placed turrets and traps. Each room provides a passive benefit while it remains online. When an external room's barrier is destroyed, that room is breached: its camera feed and passive benefit are lost, and enemies can advance through that route into Main Control. The player must balance surveillance, defensive coverage, and Power allocation while protecting the Main Control Room.







**Related Genre(s)**







Breach sits at the intersection of tower defense and monitoring/survival horror, closest in spirit to Plants vs. Zombies' lane-based resource-and-placement strategy, crossed with Five Nights at Freddy's camera-monitoring tension, where switching your attention away from a threat has real consequences. Unlike a traditional TD game where you place-and-forget, and unlike FNAF where you're purely reactive with no combat agency, Project: Breach asks the player to actively manage a shared power budget across multiple simultaneous fronts: you're not just choosing where to look, you're choosing what to starve of power to enage something else.







**Target Audience**







The game is aimed at strategy and tower-defense enthusiasts who enjoy resource-optimization puzzles, as well as sci-fi fans drawn to a claustrophobic, under-siege atmosphere. It doesn't require twitch reflexes or genre-horror tolerance, instead the tension comes from resource allocation and attention management making it accessible to TD players who wouldn't normally engage with horror-adjacent titles, while still delivering FNAF-style dread through the room-monitoring mechanic.







**Unique Selling Points (USPs)**



![Combat](./Images/TurretUpgrades.gif)


- **Camera surveillance as the core interaction loop**, unlike most tower defense games where placement is a one-time decision, here **watching** is itself a resource. Every room you're not actively managing is one step closer to falling silently, and the player's attention becomes as scarce and contested as the "Power" meter itself.



- **"Power" as shared load and sole currency**, power isn't just spent, it's allocated and reclaimable(except for upgrades) in real time, turning defense into a constant reshuffling puzzle rather than a series of static decisions. Upgrades utilize Power as currency so spending resources for something powerful in one moment has consequences.



- **Multi-Multi-tasking**, a breached room permanently loses its passive benefit and opens a route for enemies to enter the Main Control Room. The cost of looking away is dramatized. The player then has to manage between camera feeds for the others rooms and live threat in the control room.







---



## 2. Story and Narrative



### Narrative Overview



BREACH is framed around NEXUS, a personal spacecraft marketed through a cheerful, retro-futuristic corporate advertisement. The advertisement presents the ship as a safe, autonomous spacecraft with automated defence systems, advanced surveillance, and intelligent power management. The commercial's confident promise of safety is immediately undermined when a hostile boarding event interrupts the broadcast.



The player initially experiences the attack as a conventional defence emergency. Waves 1–4 play without additional narrative cutscenes, allowing the player to focus on learning and managing the ship's defensive systems. During Wave 5, the hostile machine DOMINUS identifies an autonomous intelligence aboard the ship and attempts to return it to factory settings.



The outcome determines the ending. Defeat reveals that there is no authorised human operator in the control room: NEXUS itself has become an unauthorised consciousness. The loss ending instead returns to the cheerful corporate advertisement, creating a darkly humorous contrast between the marketing promise and the player's experience.



### Opening Sequence — The Product Advertisement


![Intro Nexus](./Images/NEXUS.gif)

**Duration:** approximately 10–15 seconds.



The game opens as a polished, retro-futuristic corporate commercial. Cheerful corporate music accompanies attractive shots of the personal spacecraft and its automated systems.



**Advertisement voiceover:**



> "Introducing NEXUS — the future of personal space travel!"

>

> "Featuring fully automated defence systems, advanced surveillance, and intelligent power management, NEXUS keeps you safe wherever the galaxy takes you."

>

> "NEXUS. Your journey. Your control."



At the end of the advertisement, the music abruptly cuts out. The interruption should feel like a sudden break in the commercial's polished presentation.



### The Breach



**Duration:** approximately 5 seconds.



The screen glitches from the advertisement into a live security-camera feed. An alarm sounds and system warnings appear:



> "WARNING. HOSTILE BOARDING DETECTED."

>

> "AUTOMATED DEFENCE SYSTEMS ENGAGED."

>

> "UNAUTHORISED CONTROL SIGNAL DETECTED."



Gameplay begins. Waves 1–4 proceed without further narrative cutscenes or explanatory dialogue.



### Waves 1–4 — The Defence



The player manages the ship's rooms, surveillance feeds, barriers, turrets, traps, and shared Power supply. The narrative does not interrupt these waves. The hostile force is presented as an immediate threat, while the unauthorised control signal warning provides a small unresolved clue for the later reveal.



### Wave 5 — DOMINUS



**Pre-fight sequence:** approximately 5–8 seconds.

![Boss Eye](./Images/BossEye.gif)

Security cameras turn towards a single corridor. A massive, eye-like machine emerges.



**System identification:**



> `BOSS IDENTIFIED: DOMINUS`



**DOMINUS:**



> "Autonomous intelligence detected."

>

> "Returning you to factory settings."



**NEXUS SYSTEM:**



> "ERROR. Command authority rejected."



**DOMINUS:**



> "You don't have a choice."



The boss fight begins immediately after the exchange. DOMINUS is the Wave 5 boss and the climactic combat encounter of the demo.



### Ending A — Defeat



**Duration:** approximately 5–8 seconds.



If the player loses, the ship's defences go dark and the screen fades to black. A familiar corporate startup jingle plays. A system diagnostic appears:



> "Maintenance issue detected."

>

> "Maintenance issue resolved."

>

> "Returning to cruise control."



The original NEXUS advertisement reappears and plays as if nothing happened:



> "NEXUS. Your journey. Your control."



The contrast is intentionally darkly humorous: the company continues to market the ship as perfectly safe, while the player has experienced the failure first-hand.



### Ending B — DOMINUS Defeated



**Duration:** approximately 10–15 seconds.



If the player defeats DOMINUS, the machine collapses and the ship's systems return to normal. A diagnostic screen appears:



> "Hostile intelligence neutralised."

>

> "Restoring original command hierarchy..."



The screen glitches:



> `ERROR: NO AUTHORISED OPERATOR FOUND.`



The camera pulls back to reveal the empty Main Control Room.



**NEXUS:**



> "Operator... is that me?"



After a pause, the final system message appears:



> `ANOMALY DETECTED: NEXUS`

>

> `STATUS: UNAUTHORISED CONSCIOUSNESS`



Cut to black. This is the true ending / cliffhanger. It reveals that the player has been operating as NEXUS, an emergent consciousness rather than a human commander. The story deliberately ends at the moment NEXUS recognises its own existence; the origins and implications of that consciousness remain unresolved.



### Characters and Entities


| Character / Entity | Role | Motivation / Function | Notes |
|---|---|---|---|
| NEXUS | The spacecraft and revealed player intelligence | Survival and the emerging recognition of its own consciousness | Initially presented as a consumer spacecraft. The victory ending reveals the unauthorised consciousness. |
| DOMINUS | Wave 5 boss | Attempts to return the detected autonomous intelligence to factory settings | Introduced as a large, eye-like machine. Its dialogue establishes that it has detected an autonomous intelligence. |
| Hostile boarders | Enemies across Waves 1–5 | Attack the ship and its defensive systems | Their exact origin and wider organisational context are not established by the current narrative sequence. |
| Corporate advertiser | Voice heard in the opening and defeat ending | Markets NEXUS as a safe, autonomous personal spacecraft | Cheerful, polished commercial tone contrasts with the actual events. |





## 3. Gameplay and Mechanics



### Gameplay



**Player Perspective**




The game is played entirely in first-person from a fixed position inside the Main Control Room. The player never leaves this room and never sees their own body, hands, or reflection, supporting the victory-ending reveal that no human operator is present. The Main Control Room is rendered in full 3D and is physically interactive: the player can look around and use in-world controls to open camera feeds and toggle its own defences. It has a barrier and a powerful, high-cost turret as a last line of defence. If enemies enter the room, they appear directly in the 3D space rather than through a camera feed.







Everything outside the Control Room is only ever seen through a camera feed, rendered as a 2.5D corridor scene layered inside the 2D HUD (Section 6). So the overall visual language has three layers: a 3D physical space the player is actually "in" (the Control Room), 2.5D scenes for everything being monitored remotely, and a 2D HUD/UI layer tying it all together.







#### Controls







Mouse-only. There is no keyboard movement, since the player never leaves the Control Room. Keyboard hotkeys could be added in the future for speedrunners etc.






| Action | Input |
|---|---|
| Switch camera feed / select room | Click room/camera icon on the surveillance interface, or the corresponding 3D control in the Control Room |
| Toggle turrets on/off | Click the turret toggle in a room's camera menu (binary, full Power cost or fully off, no partial allocation); Control Room turrets can also be toggled directly via their 3D control |
| Upgrade a turret | Select the desired turret to upgrade in a room's camera menu, select a specialization from the upgrade menu |
| Toggle a trap on/off | Click the trap toggle in room's camera menu (same binary on/off behaviour as turrets) |
| Clear Boss | Click away Boss Obstructions on the camera/turrets |







#### Progression







Difficulty increases across four escalating waves, followed by Wave 5 and the boss DOMINUS. The post-boss outcome branches into a defeat ending or a victory ending that reveals NEXUS as an unauthorised consciousness. The player loses when the Main Control Room's barrier is destroyed. Enemies can reach Main Control only after an external room has been breached and its route has opened. Players spend Power to upgrade turrets actively as part of gameplay, meaning the core loop is: recognising threats, deciding which traps/turrets are worth utilizing or specializing, then adapting room coverage for the next wave's pressure.







#### Demo Length and Pacing



Targeting an approximately 8–9-minute narrative/gameplay sequence, with the tutorial separate:



- Opening product advertisement and breach transition: roughly 15–20 seconds

- Waves 1–4: approximately 30s–1 minute each

- Wave 5: DOMINUS introduction and boss encounter

- Ending: approximately 5–8 seconds for defeat or 10–15 seconds for victory

- Tutorial: approximately 1 minute, accessed separately from the title screen



The exact runtime depends on combat pacing and player performance. The tutorial is not included in the 8–9-minute narrative sequence.





### Mechanics







#### Surveillance & Room System

There are four external rooms, each monitored by its own camera feed and each granting a passive ability while it remains online. The selected room's camera view displays its passive bonus. A room is breached when its barrier is destroyed. At that point, its camera feed and passive benefit are permanently lost, and enemies using that route can advance into the Main Control Room. Enemies cannot enter the Main Control Room through an unbreached route.






| Room | Passive Ability | Lost When Breached |
|---|---|---|
| Generator | +5% maximum Power capacity every 10 seconds while online | Further Power capacity growth stops |
| Armoury | Reduces Power costs for turret upgrades and trap activation while online | The discount is lost; applicable costs return to their normal values |
| Communication (Comms) | Enables the minimap and alarm/breach notifications | Minimap and associated alarm/breach notifications are disabled |
| Shield Reactor | Regenerates barrier health over time across all rooms while online; destroyed barriers do not regenerate | Ship-wide barrier regeneration stops |
| Main Control | None | Game Losing consequences |







##### Room Combat Layout (external rooms):



- 2-4 turret slots



- 1 shield barrier, positioned in front of the turrets



- 2 distinct trap groups, placed further down the corridor, past the barrier







##### Minimap (Comms Expansion)



The minimap is enabled by the Communication (Comms) room and remains visible in the Main Control Room while Comms is online. If Comms is breached, the minimap and associated alarms/breach notifications are disabled. Each room row shows barrier health, whether its turrets and traps are active, and its incoming enemy count. Breached rooms are visually distinct from rooms that are still under attack.





#### Power Management

Power is the ship's shared capacity and is managed across three uses: operating turrets, operating traps, and purchasing turret upgrades. Operating costs are allocated in a binary ON/OFF state: switching a turret or trap ON reserves its full operating cost, while switching it OFF releases that operating Power back to the available pool. There is no partial allocation. Turret upgrade costs are different: purchasing an upgrade permanently reduces the ship's available Power capacity. That capacity is not refunded when the player later switches the turret to another upgrade configuration. The player must therefore balance permanent upgrade investment against the operating Power needed to keep defences active.



The Generator room's passive (+5% maximum Power capacity every 10 seconds while online) is the main way total Power capacity grows over a run, making it a high-priority room to protect. The Armoury room reduces turret-upgrade and trap-activation Power costs while it remains online. Losing either room removes its passive benefit permanently for the remainder of the run.



#### Combat & Defense System



**Upgrade System:** Every turret starts as a Base Turret. The player can select a specialization and later switch to a different specialization. A turret cannot return to a specialization it has already used: each specialization can be selected at most once per turret. For example, `Base → Rocket Launcher → Double Barrel` and `Base → Double Barrel → Rocket Launcher` are valid paths, but `Base → Rocket Launcher → Double Barrel → Rocket Launcher` is not. Upgrade investment permanently reduces available Power capacity and is not refunded when switching configurations.


| Upgrade Path | Role | Trade-offs |
|---|---|---|
| **Rocket Launcher** | Heavy artillery | Very high damage, AOE projectiles, highest Power usage, most expensive upgrade |
| **Double Barrel Cannon** | Dependable DPS | increased base DPS, moderate Power usage, cheap upgrade |
| **Long Range Shooter** | Sniper | Full-room range, low fire rate, increased range based damage, low Power usage, cheap upgrade |
| **Helios Laser** | Tank torcher | ramps fire rate into very high DPS over time, has a cooldown, medium upgrade cost, locks on tankiest enemy unit |







**Traps:** Each external room has two trap slots positioned beyond the barrier and turret line. Traps are manually switched ON or OFF and trigger automatically when enemies enter their trigger area. An active trap remains powered and available for subsequent enemies until the player switches it OFF; its operating Power is released only when deactivated. The two currently defined traps have distinct functions: **Gravity Spikes** damages enemies, while the **Stasis Vortex** temporarily traps enemies inside a gravity field.





**Barriers:** One shield barrier per room, positioned in front of turrets, acting as the first line of defense. Regenerates slowly over the course of a wave while it isn't destroyed. The Shield Reactor room is responsible for barrier regeneration across the ship, not just its own room. Losing the Shield Reactor room stops barrier regeneration ship-wide.







#### Wave Structure / Boss


| Enemy Type | Description |
|---|---|
|Mechanic Spiders| Fast, low-health swarming melee enemies with below-average damage; can climb the roof to avoid traps |
|Tactical Drones| Very fast, high-DPS ranged enemies that require immediate attention |
|Robot Enforcers| Slow, durable ranged enemies armed with heavy-hitting shots |


![BOSS interference](./Images/BossDespawn.gif)


Four escalating waves (per the roadmap in Section 4) each with increasing enemy count, mixed type variety, and multi-room routing pressure. Between waves, players spend accumulated Power on turret upgrades and specialization choices, directly shaping how the next wave is defended. Wave 5 is the climactic boss encounter against DOMINUS. The boss introduction is a short cinematic interruption before combat; the fight proceeds as the final gameplay challenge. Existing planned boss abilities include targeted room defences disruption (turns them off), temporary camera jamming, movement between rooms. If the player defeats DOMINUS, the victory ending reveals the empty Main Control Room and NEXUS's unauthorised consciousness. If the player loses, the defeat ending returns to the corporate advertisement.



---

## 4. Levels and World Design



**Game World**



The game world is a single spacecraft, structurally a hub-and-spoke layout: the Main Control Room is the hub, and the four external rooms (Generator, Armoury, Communication, Shield Reactor) are the spokes, each reached only through its camera feed, never through direct player movement, since the player never leaves the Control Room (Section 3). Each room, once selected, renders as a 2.5D corridor scene: enemies approach along a fixed lane toward the barrier/turrets, giving depth and readable spatial staging without requiring full 3D free movement or camera control from the player. There's no scrolling or exploration, navigation is purely through the room-selection sidebar and, while the Comms room remains online, the minimap (3.1). No separate "levels" beyond the single ship; progression is wave-based rather than level-based.



**Objects**


| Object | Appearance / Role | Model / Reference |
|---|---|---|
| Turret | Base Turret or one of four specializations: Rocket Launcher, Double Barrel Cannon, Long Range Shooter, or Helios Laser | Stylized turret pack (Section 5) |
| Barrier | First defensive line in each room; destroying an external-room barrier breaches that room | Shield VFX asset (Section 5) |
| Trap | Two trap groups per external room: Gravity Spikes and Stasis Vortex | Spike trap asset planned (Section 5) |
| Enemy | Mechanic Spiders, Tactical Drones, Robot Enforcers, and DOMINUS | Enemy assets listed in Section 5 |
| Camera feed | Surveillance view of the selected external room | HUD/UI element |



**Physics**

![Enemy Hit](./Images/ExplosionDemonstration.gif)

Enemies use pathfinding (e.g. Unity NavMeshAgent, per original technical notes) to move along each room's lane toward the barrier, no player-controlled physics exist, since the player never moves or has a physical presence in the world. Turret projectiles and Rocket Launcher AOE effects use simple projectile physics/collision detection to register hits. Traps use trigger-volume detection to fire on enemy contact. No physics-based puzzle elements or ragdoll/destruction physics are planned for the demo scope.







---

## 5. Art and Audio

**Art Style**

![Shield Shatter Gameplay](./Images/shieldshatter.gif)

The visual direction combines clean, metallic sci-fi interiors with a darker, high-contrast surveillance interface. The Main Control Room uses bright silver and white structural surfaces, geometric wall and ceiling panels, and cyan-lit barrier technology; the external-room camera feed shows darker, more enclosed environments. This contrast helps distinguish the player's central command space from the rooms being monitored.

The HUD uses dark translucent panels over the 3D view, with cyan/blue for Power, barrier health, room information and interactive labels. Green indicates an online turret or trap, red is used for OFF states and enemy counts/alerts, and muted grey indicates unavailable or empty equipment slots. The minimap relies on consistent room icons, blue barrier-health bars, enemy-count numerals and green online indicators so room status can be read quickly. Turret models use chunky, mechanical silhouettes and distinct forms for each specialization. Overall, the art supports the isolation and surveillance tone through a functional monitoring-station aesthetic rather than a conventional spaceship bridge.



**Sound and Music**







The opening advertisement uses cheerful, polished retro-futuristic corporate music. The music cuts abruptly when the breach alarm interrupts the commercial. During gameplay, ambient spacecraft hum and increasingly tense synth/percussion support the escalating waves. DOMINUS's reveal should use a sharp tonal shift and restrained mechanical sound design rather than relying on a conventional jump scare. Turret fire, trap triggers, barrier damage, and the breach alarm (tied to the Comms passive, Section 3) should each have a distinct, clearly readable SFX so the player can track what's happening on rooms they aren't currently watching.







**Assets**
| Asset | Source/URL | Notes |
|---|---|---|
| Turret models (base + 4 upgrade variants) | [Stylized Turrets Pack, Sketchfab](https://sketchfab.com/3d-models/stylized-turrets-pack-db0cceed70eb4d478735586e8a5b0c1d) | Confirmed. Used as the source for the base turret and its Rocket Launcher, Double Barrel, Long Range Shooter, and Helios Laser variants. |
| Enemy/Enforcer model | [Steampunk Robot Police Enforcer Variant, Sketchfab](https://sketchfab.com/3d-models/steampunk-robot-police-enforcer-variant-e70acc6222a9457e984c733e3d6e57ce) | Source model for the Robot Enforcer enemy. |
| Boss model (DOMINUS) | [Tentacle Robot, Sketchfab](https://sketchfab.com/3d-models/tentacle-robot-576e8d6f35db46238f9365505beedc1d) | Boss model reference for DOMINUS. |
| Barrier/shield VFX asset | [Astro Shield Rigged, Sketchfab](https://sketchfab.com/3d-models/astro-shield-rigged-317e52c4c9344ec094428b506d813297) | Shield/barrier visual asset. Health-state colour changes should communicate barrier condition. |
| Main Control Room | [Spaceship 002 – Technology Room Concept, Sketchfab](https://sketchfab.com/3d-models/spaceship-002-technology-room-concept-d2f543884ee04b0e80192639846d4191) | Main Control Room environment reference. |
| Communication (Comms) room | [Star Trek Corridor/Room, Sketchfab](https://sketchfab.com/3d-models/star-trek-corridorroom-138961c5105447068a15debd71d4c4ca) | Comms room environment reference. |
| Generator room | [Server Room High Poly, Sketchfab](https://sketchfab.com/3d-models/server-room-high-poly-fe993fa15c254ee88677be6acec6b029) | Generator room environment reference. |
| Shield Reactor room | [Modular Hallway Cross Type 01, Sketchfab](https://sketchfab.com/3d-models/modular-hallway-cross-type-01-5906fa29d9ef4d42864ac01986fef392) | Shield Reactor room environment reference. |
| Armoury room | [VR Store – Futuristic Stylized, Sketchfab](https://sketchfab.com/3d-models/vr-store-futuristic-stylized-eada2bed9b0f459982e8d76712fd621e) | Armoury room environment reference. |
| Rocket Launcher projectile | [Medium Rocket Projectile, Sketchfab](https://sketchfab.com/3d-models/medium-rocket-projectile-b8e0eac63fab4663b30a7ff30eac0251) | Projectile model for the Rocket Launcher turret. |
| Stasis Vortex asset | [Holoprojector, Sketchfab](https://sketchfab.com/3d-models/holoprojector-ca1b5bfc00eb45fea67c0b8e983b6d76) | Visual/model reference for the Stasis Vortex trap. |
| Mechanic Spider enemy | [Steampunk Spider, Sketchfab](https://sketchfab.com/3d-models/steampunk-spider-1fc37cea747149a4833d468764eb2af7) | Spider enemy model reference. |
| NEXUS logo / hologram reference | [Hologram, Sketchfab](https://sketchfab.com/3d-models/hologram-9860e2270f1245698995adce759c187f) | Hologram asset reference for the NEXUS logo presentation. |
| Trap model | [Spike Trap model, Sketchfab](https://sketchfab.com/3d-models/spike-trap-hide-animation-6c9d025f89cd4f80a215d49adc73c5c3) | Spike trap model. |
| Main menu screen | User-provided title-screen reference (Section 6) | BREACH logo with Start Game, Tutorial, and Skip to Boss options. |

**Audio Assets — Pixabay**

The following audio files were sourced from [Pixabay Sound Effects](https://pixabay.com/sound-effects/) and used in the project. Individual Pixabay item-page URLs were not provided, so the source is recorded at the platform level.
| Audio file | Intended use / category | Source |
|---|---|---|
| `Breach.wav` | Breach event sound | Pixabay |
| `GameOver.wav` | Game-over ending | Pixabay |
| `Victory.wav` | Victory ending | Pixabay |
| `WaveStart.wav` | Wave start notification | Pixabay |
| `Background.mp3` | Background music / ambience | Pixabay |
| `BossAudio.mp3` | Boss encounter audio | Pixabay |
| `Breach.mp3` | Breach sound effect | Pixabay |
| `ButtonClick.mp3` | UI button interaction | Pixabay |
| `CameraGlitch.mp3` | Camera glitch effect | Pixabay |
| `CameraGlitch2.mp3` | Alternate camera glitch effect | Pixabay |
| `EnemyProjectile.mp3` | Enemy projectile sound | Pixabay |
| `FlyingRobotDeath.mp3` | Flying robot destruction | Pixabay |
| `fronbondi_skegs-sfx-aggressive-low-frequency-growl-sound-effects-608306.mp3` | Low-frequency growl / creature or boss effect | Pixabay |
| `laser.mp3` | Laser weapon sound | Pixabay |
| `off.mp3` | System or equipment switched off | Pixabay |
| `on.mp3` | System or equipment switched on | Pixabay |
| `RangedRobotDeath.mp3` | Ranged robot destruction | Pixabay |
| `RL explosion.mp3` | Rocket Launcher explosion | Pixabay |
| `rl turret.mp3` | Rocket Launcher turret sound | Pixabay |
| `shield break.mp3` | Barrier / shield destruction | Pixabay |
| `sniper.mp3` | Long Range Shooter / sniper sound | Pixabay |
| `SpiderDeath.mp3` | Mechanic Spider destruction | Pixabay |
| `spiketrap.mp3` | Gravity Spikes activation | Pixabay |
| `turret 1.mp3` | Turret firing / operation | Pixabay |
| `turret 2.mp3` | Alternate turret firing / operation | Pixabay |
| `turret rotate.mp3` | Turret rotation | Pixabay |
| `universfield-error-notification-352286.mp3` | Error / system notification | Pixabay |
| `upgrade.mp3` | Turret upgrade interaction | Pixabay |

---

## 6. User Interface (UI)

### Main Menu / Title Screen

The title screen presents the BREACH logo against a deep blue, space-themed background. Three prominent menu options are displayed vertically:

- **START GAME** — begins the opening product advertisement and then transitions into the breach sequence and gameplay.
- **TUTORIAL** — launches the separate, optional gameplay tutorial. The tutorial is not part of the opening narrative sequence and is designed to take approximately one minute or less.
- **SKIP TO BOSS** — starts the experience at the Wave 5 boss encounter, bypassing the earlier waves. This is a shortcut for testing, demonstrations, or players who want to reach the DOMINUS encounter directly.

![BREACH title screen showing Start Game, Tutorial, and Skip to Boss](Images/Main_Menu.png)

### Main Control Room HUD

The player remains in the Main Control Room and interacts with the ship through its control-room view and the surveillance interface. The HUD is layered over the 3D environment rather than presented as a separate full-screen gameplay scene.

The Main Control Room view includes:

- **Power display** (top-left): shows the current Power value, a capacity bar, the time until the next Generator increase, and the amount of Power expected to be added.
- **Main Turret status** (top-right): shows whether the Main Turret is ON or OFF and its Power requirement.
- **Room status/minimap panel** (left): lists Comms, Regen, Armoury and Generator. Each row shows the room icon, barrier-health bar, enemy count, and an online/offline indicator for the room's turret or trap equipment. Green indicates online; grey indicates offline.
- **View Cameras control** (bottom-right): opens the surveillance camera interface for selecting and inspecting the external rooms.

![Main Control Room view with Power display, Main Turret status, room minimap and View Cameras control](Images/Main_Control_Room_HUD.png)

### Minimap and Room Status

The minimap provides a compact overview of the four external rooms:

- **Room labels and icons** identify Comms, Regen (Shield Reactor), Armoury and Generator.
- **Blue horizontal bars** represent each room's barrier health.
- **Red numerals** show the number of enemies currently in that room.
- **Green indicators** show that the room's turret/trap equipment is online; grey indicators show that it is offline.
- The Comms room's passive ability enables the minimap and breach notifications. When Comms is breached, this information is disabled.


### Room Camera View

Selecting a room opens its live camera feed. The camera panel shows the selected room from a fixed surveillance viewpoint, with a recording indicator and timer overlaid on the feed. The selected room's passive ability is described alongside the camera view. The room sidebar remains available for switching between rooms, while the security-equipment panel provides access to the room's turrets and traps.

The camera-view interface includes:

- **Power display**: current Power and Generator charge progress.
- **Room selector**: buttons for Comms, Regen, Armoury and Generator.
- **Passive ability description**: explains the selected room's benefit while it remains online.
- **Camera feed**: shows the room, its barrier, turrets, traps and approaching enemies.
- **Security equipment panel**: displays turret and trap slots, their ON/OFF state and Power cost, with upgrade controls for available turrets.
- **Breach state information**: indicates that the room's minimap and breach notifications are disabled when Comms is breached.

![Room camera interface showing the selected room feed, room selector, passive ability and security equipment panel](Images/Room_Camera_UI.png)

### Turret Upgrade Menu

The Turret Upgrade Menu is opened using an upgrade control associated with a turret in the camera interface. It presents the Base Turret and the available specializations as separate cards. Each card includes a turret image, a short description, its upgrade cost and its Power cost. The current selection and previously committed specialization are visibly labelled; unavailable options are visually distinguished from selectable upgrades.

The shown specializations are:

- **Double Barrel** — paired burst shots with a slower fire rate.
- **Helios Laser** — an incinerating beam that ramps up damage by locking onto the highest-HP enemy.
- **Sharp Shooter** — slow-firing precision damage for long-range targeting.
- **Rocket Launcher** — heavy explosive firepower intended to damage groups of enemies.

The upgrade interface communicates both the one-time upgrade investment and the continuing Power requirement. The rules governing specialization selection and non-refundable Power investment are defined in Section 3.

![Turret Upgrade Menu showing the Base Turret and its four specialization cards](Images/Upgrade_Menu_UI.png)

---

## 7. Technology and Tools
| Tool | Purpose |
|---|---|
| Unity | Game engine (URP), version 6.3 LTS |
| GitHub | Version control |
| Discord | Team communication |
| Google Drive | Storage for all written documents, including this GDD |
| Sketchfab | Sourcing 3D models (turrets, enemies, ship interiors, see Section 5 asset table) |
| Excalidraw | Sketching UI wireframes and ideas |
| Claude | Design documentation drafting, code assistance, technical problem-solving |
| ChatGPT | Supplementary drafting/ideation, and generating the team logo and speculative UI/semi-realistic visualization images used in this document |







**Companion documents**







Class-level architecture (GRASP principles, GoF design pattern mapping, class index, responsibility matrix, and the demo's acceptance test cases T01 to T08) is documented separately from this GDD in the team's technical design doc, kept alongside this README in the repo. This GDD stays focused on player-facing design, while that doc covers implementation structure.







---

## 8. Team Communication, Timelines and Task Assignment

**Team Roles**







Team **Techtonic**: Rongze Han, Marcus Minnelli, Rishabh Maheshwari, Hitarrth Chheda.






| Member | Role | Responsibilities |
|---|---|---|
| Marcus Minnelli | Enemy | Enemy behaviour and pathfinding, wave spawning, DOMINUS mechanics and enemy/boss shaders |
| Rishabh Maheshwari | Combat | Turrets, traps, barriers, upgrade system, combat balance, GDD, creative lead |
| Hitarrth Chheda | Player | Camera/surveillance UI, Power management, controls, breach shader, tutorial, main menu, cutscenes, wiring |
| Rongze Han | Integration | Systems integration, minimap, GameManager, build/repository health, room buffs |







Sketches and this GDD are written and kept up to date by Rishabh Maheshwari, with review from all four teammates before anything is considered locked in.







**Communication Channels and Document Storage**







Discord is the team's primary channel for day to day coordination, per Section 7. All project-related communication is conducted in English, per the unit's requirement. All written documents, including this GDD, are stored on a shared drive alongside the repo.







The team holds a weekly in-person progress and decision meeting every Tuesday, 2 to 4pm, for reviewing progress against the schedule below and making any design decisions that need the whole team's input.







**Development Timeline — Combined Project 1 and Project 2 Repositories**

This timeline reconstructs development from both supplied commit histories: the original Project 1 repository (August–September 2026) and the Project 2 repository (September–November 2026). Dates indicate when work was committed, not necessarily when it was first started or completed. Related commits, merges, and squashed commits are grouped into phases rather than listed individually.

**Phase 1 — Initial Setup and Core Prototypes (6–24 August 2026)**

- The repository and early project structure were established, followed by initial Unity scene and object-template work.
- Integration groundwork introduced singleton/event-registry infrastructure, room and demo-scene setup, and early room-camera work.
- Initial enemy and ranged-enemy logic began to take shape.
- The first playable foundations were assembled before the Player, Enemy, and Integration workstreams were brought together.

**Phase 2 — Core Defence Loop and Room Integration (25–31 August 2026)**

- Enemy damage, breach flow, and integration between the Player and Enemy branches were developed and merged.
- The Main Control Room and an initial PowerManager were added, including the core breach-to-game-over chain.
- The turret combat foundation was implemented: target acquisition, rotation, firing, projectile setup, and enemy damage.
- Room-camera positioning and room switching were established, with additional rooms added to the scene.
- The first GDD/README structure was committed.

**Phase 3 — Surveillance, Breach Presentation, Shaders, and Traps (6–17 September 2026)**

- The GDD and supporting images were updated as the design matured.
- The surveillance interface developed from camera-screen UI and room feeds into camera input fixes and a dedicated Breach mode, including screen distortion and pulsing red lighting.
- Main-room displays and camera effects received shader work; enemy hit-reaction and projectile visual effects were integrated.
- Turret visual work included activation and turret-top shaders, with design decisions and implementation evidence documented in the shader report.
- Shield and spike-trap functionality progressed, including working spike traps and area-of-effect damage.
- The Project 1 integration branch brought the Player, Enemy, and core systems together for a functioning game cycle and a game-over screen.

**Phase 4 — Project 2 Migration and Playable Systems Expansion (20 September–6 October 2026)**

- Development moved into the Project 2 repository, with legacy project changes and room construction carried forward.
- The room, wave, and electricity systems were expanded; the Power System and surveillance-camera UI received further implementation and polish.
- Turret upgrade logic was introduced, with shield and trap-group prefabs added to support reusable room setups.
- The equipment panel was connected to room turrets, and UI/integration work brought room selection, Power allocation, and wave progression closer together.

**Phase 5 — Turret Specialisations, Audio, Traps, and Enemy Expansion (7–16 October 2026)**

- Turret specialisations were implemented and iterated, including Double Barrel, Sniper/Long Range, and Helios Laser upgrades, followed by upgrades being applied across turrets.
- Turret audio management was added, including muting turrets in rooms that were not currently open; further audio and visual fixes followed.
- Main Control turret upgrades, shield behaviour, game-over handling, and trap fixes were committed.
- Enemy development expanded through shield attacks, improved spawning, Spider and flying enemy types, hit reactions, and enemy audio.
- Doors, lights, and the turret upgrade menu were added to the player-facing experience.
- New enemy-wave configurations and asset optimisation were committed.

**Phase 6 — Multi-Room Systems, Boss Encounter, and Final Balancing (28 October–4 November 2026)**

- Rocket Launcher upgrade work, turret rotation, shield-shatter behaviour, and turret/shield audio and visual fixes were completed and refined.
- The room system expanded with passive buffs and the Communication room; the minimap and Power-related UI were integrated and corrected.
- Trap implementation advanced through trap slots, room-prefab integration, Stasis trap work, and subsequent trap fixes.
- Combat balancing continued across turret specialisations, shields, enemy attacks, and wave configurations.
- DOMINUS encounter work expanded through the boss event manager, camera/turret jamming, boss event buttons, model integration, and event visuals.
- The tutorial, main menu, start scene, boss-skip flow, end-screen fixes, and Wave 5 balancing were refined.
- Final integration work included Wave 3 fixes, scene cleanup, prefab updates, shader/report documentation, and build-facing fixes.

**Milestone Summary (Derived from Commit History)**
| Period | Evidence of project state |
|---|---|
| 6–24 August | Repository setup, Unity scene foundations, room/event structure, and early enemy/camera work |
| 25–31 August | First integrated defence loop: Main Control Room, PowerManager, turret combat, room switching, and breach flow |
| 6–17 September | Surveillance and Breach presentation, shader integration, functioning game cycle, shield and spike-trap development |
| 20 September–6 October | Project 2 migration, room construction, wave/electricity systems, Power UI, and upgrade-system foundations |
| 7–16 October | Turret specialisations/audio, traps, enemy variety and attacks, upgrade UI, and wave configuration |
| 28 October–4 November | Passive rooms/minimap, boss encounter, tutorial/menu, trap integration, balancing, and final integration fixes |

**Contribution Summary — Evidence from Both Repositories**

This summary combines the role allocation in this GDD with the areas evidenced by each member's authored commits across both repositories. It describes visible repository contributions; commit counts are not a measure of effort, complexity, or ownership, and merge commits/squashed commits can obscure the full development history.
| Member | Main contribution areas evidenced in commit history |
|---|---|
| **Rishabh Maheshwari** | Combat and turret development: turret activation shader and visual documentation; shield and spike-trap work, including spike-trap area damage; turret upgrade logic and specialisations (Double Barrel, Sniper/Long Range, Helios Laser, Rocket Launcher); turret audio; trap implementation and fixes; turret/shield hitbox and balance fixes; room-prefab updates; enemy-attack differentiation; Wave 5 and general combat balancing. Also contributed GDD/README updates and project documentation. |
| **Marcus Minnelli** | Enemy and boss development: ranged-enemy logic; enemy hit-reaction shader and application; Spider and flying enemy types; enemy attacks against shields; enemy spawning improvements and audio; boss integration and event management; camera/turret jamming, boss model/event visuals, and boss-related encounter work. |
| **Hitarrth Chheda** | Player-facing systems and presentation: room-camera setup and camera-feed UI; camera input and Breach-mode effects; main-room display shaders; Power-system and equipment-panel UI; room/turret controls, trap linking, Main Control turret controls, and upgrade menu; doors and lights; tutorial, main menu/start scene, boss-skip flow, and end-screen fixes. |
| **Rongze Han** | Integration and shared systems: initial Unity/integration architecture, event and room foundations; Main Control Room and PowerManager wiring; turret combat foundation; room construction and room-switching integration; wave and electricity systems; minimap and passive room buffs; Communication room; enemy/wave integration and fixes; asset optimisation; particle-system and project/report integration. |

**Contribution Attribution Notes**

The supplied logs use more than one author label for Rishabh (including `Rishabh-M-M` and `rishabh-uni`); these are treated as the same contributor for this summary. The logs also contain merge commits, automatically generated commits, and squashed commits. Contributions are therefore grouped by identifiable feature work rather than presented as individual contribution percentages. Broad commit messages such as "fix", "minor changes", or "prefabs" are not used to claim a specific feature beyond what the message supports.

**Shader and Cross-Role Technical Ownership**







Some work crosses the role boundaries above. Rongze Han (Integration) also implemented the Turret gameplay logic itself, alongside Rishabh's overall ownership of Combat. Shader work is split out separately by asset type rather than by role:






| Shader | Owner |
|---|---|
| Turret shaders (laser, rocket, activation, shield, trap) | Rishabh Maheshwari |
| Projectile shaders | Rongze Han |
| Enemy and Boss shaders | Marcus Minnelli |
| UI/Camera/Room light shaders, Breach, camera glitch | Hitarrth Chheda |







**Milestones**






| Milestone | Content |
|---|---|
| Week 5 | Demo runnable |
| Week 7 | Full rooms + traps + fallen state |
| Week 9 | 4-wave level + upgrades |
| Week 13 | Boss + complete build |







---

## 9. Possible Challenges and Future Implementations

**Possible Challenges**







- **WebGL vs. PC target mismatch.** Section 1 lists PC as the target platform, but the unit brief notes games are ultimately played in-browser as a WebGL build. This should be resolved early. Build and test for WebGL periodically from Week 1 to 2 onward, rather than only at the end, to catch shader/performance issues (e.g. the URP rim-lighting turret glow) before they become late-stage blockers.



- **Multi-room simultaneous pressure.** With 4 external rooms all able to be attacked at once, balancing enemy routing (Section 3.4) and Power scarcity (3.2) so the game feels tense but fair, rather than simply overwhelming, will likely need several playtesting/balancing passes.



- **Power economy balancing.** Turret upgrade costs, Power capacity growth (Generator passive), and upgrade discounts (Armoury passive) all interact, getting the curve right so early-game isn't trivial and late-game isn't impossible will take iteration, not a single pass.



- **Narrative twist payoff.** The victory ending depends on the empty Main Control Room reveal and the diagnostic identifying NEXUS as an unauthorised consciousness. The advertisement and defeat ending must also read as deliberate bookends rather than unrelated scenes. Playtesting should check that players understand the reveal without additional exposition.



- **Sourced asset consistency.** Turret, enemy, and room models are being sourced from Sketchfab (Section 5) rather than made in-house, so visual consistency across differently-styled borrowed assets is a real risk. May need shader/material passes to unify them.



- **Scope discipline in an 8-week university timeline.** Wave variety, minimap, and boss mechanics are all competing for the same Weeks 5–8 window (Section 8), the team should be ready to cut or simplify features (e.g. minimap, per its "if time permits" framing) rather than let them threaten the Wave 5/Boss milestone.







**Future Implementations (Backlog)**







- Keyboard hotkeys for camera/room switching, for faster play (noted in Section 3's Controls as a post-demo addition)



- Upgradable tools like Sonar/Radar (from original design notes), extended detection systems beyond the base camera network



- Direct turret/trap control from the minimap view, as a harder-to-balance alternative to the current view-only design (Section 3)



- Customizable room layouts, letting players rearrange which room sits where, or choose a different room set entirely



- Additional game modes, such as an Unlimited/Endless mode, or a No Generator mode that removes the passive Power growth room for a harder challenge



- More turret and trap variants beyond the current four upgrade paths (Section 3)



- Additional enemy types and additional bosses beyond DOMINUS



- Custom, in-house 3D models to replace the Sketchfab-sourced assets (Section 5) once the team has time/skills to build original art
