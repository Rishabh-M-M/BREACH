# 1. Evaluation Techniques

## Technique 1: Observational Methods – Post-task Walkthroughs

**Participants:** 5

**Reason of Choice:** BREACH is a fast-paced game that focuses on resource management and rapid camera switching. If players were to use the "Think-Aloud" technique, it would interrupt their concentration and ruin the atmosphere of the game, which relies on a claustrophobic environment to create tension. Recording gameplay and asking questions afterwards using the video footage is a better, non-interruptive method.

**Playtest Task:** Participants will play through the demo, aiming to reach the end of Wave 5. Their objective is to preserve as much Barrier HP as possible and minimise the number of enemies that breach the Main Control Room by managing the four external rooms. The experimenter will avoid helping unless a technical issue prevents the session from continuing.

During the session, the experimenter will note timestamps where players hesitate, overlook a warning, switch cameras repeatedly, run low on Power, or lose a room. These moments will guide the post-task questions.

## Technique 2: Query Techniques – Questionnaires

**Participants:** 5

**Reason of Choice:** Questionnaires allow us to collect quantitative data about the user interface and difficulty level. Open-ended questions will also help us understand whether players understand the core mechanics and narrative.

**Playtest Task:** Participants will play through the demo, attempting to reach Wave 5, then complete an online structured survey. They should answer based on their own experience rather than what they think the game intends them to do.

---

# 2. Designed Questions

These questions focus on the game's mechanics, UI/UX, difficulty, audio feedback, and narrative. Questions during the post-task walkthrough can be adjusted to match what the participant actually did.

## A. Questions for Post-task Walkthroughs

*(These questions will be asked while reviewing the gameplay video. They can be adapted to the participant's actions.)*

**On Multitasking & Camera Switching:**

> "In this video, you focused on [Room A] and ignored [Room B] for some time. Was that a chosen strategy, or was it difficult to keep track of both rooms?"

**On Resource Management:**

> "At this point, you decided to [save Power / use Power / turn off a turret / purchase an upgrade]. Can you explain your reasoning?"

**On UI & Feedback Awareness:**

> "When [a room came under pressure / a barrier took damage / an alarm appeared], did you notice the UI change straight away? How did it affect your next move?"

**On Combat and Room States:**

> "When [a turret was switched off / a turret was destroyed / a room was lost], what did you think had happened? Was it clear what you could do next?"

**On Audio Feedback:**

> "Could you tell what was happening in rooms you were not currently watching from the sounds alone, or did you need to switch back to the camera to understand?"

**On Difficulty:**

> "Was there a point where the game felt unfair or overwhelming? What was happening at that time?"

## B. Questions for Questionnaires

These questions will be asked through a Google Form after the participant finishes their playthrough.

**Scalar Questions (1 = Strongly Disagree, 5 = Strongly Agree):**

1. **Core Mechanic:** "The notion of Power as an allocatable resource, rather than just another consumable currency, was clear and understandable."
2. **UI/UX Clarity:** "The room sidebar and camera controls made it clear which rooms I was viewing and which rooms needed attention."
3. **Power Feedback:** "It was clear when I had enough Power to activate a device or purchase an upgrade, and when Power was unavailable."
4. **Room State Feedback:** "It was clear when a barrier was damaged, a turret was lost, or a room was no longer safe."
5. **Difficulty Curve:** "The progression from Wave 1 towards Wave 5 felt fair and gave me time to adapt."
6. **Audio and Atmosphere:** "Visual and audio feedback helped me understand what was happening and created a tense atmosphere."
7. **Controls:** "Switching between rooms and managing their defences felt manageable under pressure."

**Open-ended Questions:**

1. "What was the most difficult or annoying part of managing four rooms at once?"
2. "Was there any point where the Power system or an upgrade cost confused you? If so, what happened?"
3. "Were any warnings, room states, or device states easy to miss?"
4. "Did you understand the narrative twist that reveals the player is the AI and the enemies are a containment team? How did this change your understanding of the game?"
5. "If you could change one thing about the demo, what would it be?"

---

# 3. Participants

**Recruitment Strategy:** We will recruit a total of 10 participants: 5 for post-task walkthroughs and 5 for questionnaires. Recruitment will focus on IT and Computer Science students, as well as friends and gamers in Discord.

**Qualifying Criteria:** Participants should have experience with at least one of the following genres:

- **Tower Defense (TD) Games:** such as Plants vs. Zombies or Arknights.
- **Surveillance/Management Games:** such as Five Nights at Freddy's.
- **RTS and Resource Management Games:** such as StarCraft or grand strategy games.

Since BREACH requires mouse interaction, camera switching, and resource management under pressure, participants should be comfortable using a mouse and handling multiple sources of information. We should record participants' relevant game experience so we can consider it when interpreting the results.

---

# 4. Data Collection

**For Post-task Walkthroughs (Observational Data):**

- Use screen-recording software such as **OBS** to record the complete gameplay, including room switching, device toggles, upgrade choices, and visible UI feedback.
- The experimenter will remain quiet during gameplay to preserve the intended pressure and avoid influencing the participant.
- Record timestamps for useful moments, such as hesitation, repeated camera switching, missed warnings, unexpected Power shortages, a barrier being destroyed, a turret being lost, or a room being breached.
- After the playthrough, review selected timestamps with the participant and ask the relevant walkthrough questions.

**For Questionnaires (Querying Data):**

- Use Google Forms to collect the structured ratings and open-ended responses.
- Keep the same 1–5 scale for all scalar questions, with 1 meaning Strongly Disagree and 5 meaning Strongly Agree.
- Export the responses to a spreadsheet so ratings can be summarised and open-ended answers grouped by topic.

---

# 5. Data Analysis & Success Metrics

We will use both qualitative and quantitative feedback to identify problems and decide what to change.

**Qualitative Data Analysis (Walkthroughs & Open-ended Questions):**

- Group comments by topic, such as unclear UI feedback, confusing Power costs, difficulty switching between rooms, unclear room states, combat balance, audio feedback, and narrative understanding.
- Compare what participants say with what happened in their recorded gameplay. For example, if a participant says a warning was clear but repeatedly overlooks it, this may point to a visibility or timing issue.
- Prioritise issues that occur for multiple participants or prevent them from understanding an important mechanic.

**Quantitative Data Analysis & Metrics:**

- Calculate the average score for each scalar question and use it as a baseline for usability and satisfaction.
- **Improvement Threshold:** If the average score for the UI/UX Clarity statement falls below **3 out of 5**, redesigning visual feedback and UI warnings will become a high priority before the final release.
- **Difficulty Tracking:** Record the highest wave reached by each participant and calculate the average wave reached. If most participants are eliminated around Wave 2, investigate whether enemy health, wave pacing, room pressure, or the Power economy makes the difficulty curve too steep.
- **Power Clarity:** If Power feedback receives low ratings or participants repeatedly make decisions based on an incorrect understanding of available Power, prioritise clearer feedback before simply changing the costs.
- **Issue Frequency:** Record how many participants encounter each major issue. Use frequency alongside severity; a single issue that blocks progress may still need urgent attention.

The results should guide changes, but a low score alone will not tell us the cause. We will use the recordings and written feedback to identify what needs to change.

---

# 6. Timeline

| Phase | Dates | Activities |
| --- | --- | --- |
| **Preparation Phase** | Oct 9 – Oct 12 | Ensure the WebGL build is stable and playable. Finalise and test the Google Forms questionnaire. Contact participants and schedule the five walkthrough sessions. Check that recording and the build work before each session. |
| **Execution Phase** | Oct 13 – Oct 18 | Conduct the five in-person post-task walkthroughs, recording gameplay and asking follow-up questions. Distribute the WebGL demo and questionnaire to the other five participants. |
| **Analysis Phase** | Oct 19 – Oct 20 | Review recordings and questionnaire responses. Group recurring issues, calculate average ratings, and identify the most important bugs or design problems. |
| **Iteration Phase** | Oct 20 – Oct 31 | Implement changes based on the results. Focus on combat balance, turret upgrade costs, enemy health and pacing, Power feedback, and UI clarity. Retest changes before the final Milestone 4 build. |

---

# 7. Responsibilities

To distribute the workload and use each team member's role and experience:

- **Rishabh (Combat & GDD Lead):** Design the Google Forms and feedback questions. After evaluation, focus on the turret upgrade system, economy, combat balance, and related feedback.
- **Hitarrth (Player & UI Lead):** Facilitate and conduct three of the in-person post-task walkthroughs. Focus on player controls, camera switching, UI changes, and misleading or hard-to-notice feedback.
- **Marcus (Enemy Lead):** Facilitate the remaining two post-task walkthroughs. Focus on enemy performance, player experience of the difficulty curve, and wave pacing/telegraphing.
- **Rongze (Integration Lead):** Build, test, and deploy a stable WebGL build before evaluation. After data collection, lead the team's data synthesis meeting and assist with integration of the changes.

---

# 8. Playtest Notes

To keep the evaluation consistent:

- Use the same demo build and starting conditions for all participants where possible.
- Do not explain a mechanic during the playthrough unless the participant cannot continue because of a technical problem. The aim is to see what the game communicates on its own.
- Record build/version details and any technical issues that affect a session.
- Separate bugs (something not working as intended) from usability problems (something working but difficult to understand) and balance problems (something working but feeling too easy or too difficult).
- After changes are made, run a short regression test to confirm that the fix has not broken other rooms, turrets, Power allocation, or the WebGL build.

---
