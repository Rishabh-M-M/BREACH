# 1. Evaluation Techniques

## Technique 1: Observational Methods – Post-task Walkthroughs

**Participants:** 5

**Reason of Choice:** BREACH is a fast-paced game that focuses on resource management and rapid camera switching. If players were to utilize the "Think-Aloud" technique, it would seriously disturb their cognitive flow and concentration. Moreover, it would completely ruin the atmosphere of the game as the game relies heavily on the claustrophobic environment to create tension. Hence, recording the gameplay and asking them about the process later using the video footage will prove to be the best and a non-interruptive method.

**Playtest Task:** The task assigned to participants is to play until the end of the demo (Wave 5). The objective of the player is to maintain the maximum amount of Barrier HP and minimize the number of enemies breaching the Main Control Room by managing the four external rooms.

## Technique 2: Query Techniques – Questionnaires

**Participants:** 5

**Reason of Choice:** Questionnaires allow us to easily collect quantitative data regarding user interface and difficulty level. In addition to that, with the help of open-ended questions, we can figure out whether players understand the core mechanics of the game and the narrative as well.

**Playtest Task:** Participants will be asked to play through the entire demo (attempting to defeat the Wave 5). After completing the test, they will fill out an online structured survey.

---

# 2. Designed Questions

To ensure that we get relevant and useful feedback, we have prepared questions that will address mechanics of the game, its UI/UX, and the narrative of BREACH.

## A. Questions for Post-task Walkthroughs

*(These questions will be asked by the experimenter during review of the gameplay video. They are designed as dynamic questions and may be modified according to participant's actions.)*

**On Multitasking & Camera Switching:**

> "In this video I saw that you focused on [Room A] only and ignored [Room B] for some time. Is it a chosen strategy or you just couldn't handle camera switching in the situation?"

**On Resource Management:**

> "Here, in this timestamp you decided to [e.g., hoard power even when full / use all power / turn off a particular turret]. Can you explain the reasoning behind your Power management in this particular moment?"

**On UI & Feedback Awareness:**

> "Right here, [e.g., one of the rooms is breached / alarm is triggered]. Did you notice UI changes right away and how did that influence your next move?"

**On Enemy Mechanics:**

> "During this phase of the fight when [e.g., shield damaged / the robot breached], you seemed to [e.g., hesitate / react quickly]. What was your immediate reaction to this specific interference, and how did you adapt your strategy?"

## B. Questions for Questionnaires

These questions will be asked via a Google Form after the participant finishes the whole demo, allowing us to collect quantitative usability data and general feedback.

**Scalar Questions (1 = Strongly Disagree, 5 = Strongly Agree):**

1. **Core Mechanic:** "The notion of 'Power' as an allocatable resource instead of just another consumable currency was clear and understandable."
2. **UI/UX Clarity:** "Sidebar interface and View camera button effectively communicated to me where the corridors are that need to be defended or breached already."
3. **Difficulty Curve:** "From Wave 1 to Wave 5 the progression of difficulty and enemies scaling was smooth, fair, and appropriate."
4. **Atmosphere:** "Visual and Audio feedback conveyed a proper level of high tension and claustrophobic feeling."

**Open-ended Questions:**

1. **Identifying Problems:** "What was the most annoying / difficult part of controlling four rooms simultaneously?"
2. **Narrative Understanding:** "Did you understand the narrative twist that revealed you are the AI, and enemies are actually the containment team created? How was this revelation beneficial to the experience in general?"

---

# 3. Participants

In order to collect feedback that will be highly relevant and representative of our target audience, we've developed a recruitment strategy and defined the qualifying criteria.

**Recruitment Strategy:** We will recruit a total of 10 participants (5 dedicated to Post-task Walkthroughs and 5 for Questionnaires). The recruiting process will be directed to IT and Computer Science students—as well as among our own social circles of friends and gamers in Discord.

**Qualifying Criteria:** Consistent with the "Target Audience" specified in our Game Design Document, participants will be recruited on the basis of their gaming experience. In particular, we need participants to have experience in at least one of the following genres of games:

- **Tower Defense (TD) Games:** such as Plants vs. Zombies, Arknights.
- **Surveillance/Management Games:** such as FNAF (Five Nights at Freddy's).
- **RTS and Resource Management Games:** such as Paradox Interactive (Grand Strategy games), Starcraft.

Additionally, due to the fact that the gameplay of BREACH requires extensive fast interaction using UI, camera and toggling of resources, the participants should be proficient in using mouse movements and multitasking under pressure. In this way, we will ensure that all the usability problems identified have to do with the design of our game.

---

# 4. Data Collection

To accurately capture player behavior and controllable feedback, we will use specific tools and methods for the selected evaluation techniques.

**For Post-task Walkthroughs (Observational Data):**

- We will use screen recording software such as **OBS** to record the participants' complete gameplay, focusing on their interaction with the UI, camera switching frequency, and mouse trials.
- During the experiment the experimenter will sit aside and maintain silence, not interfering with the player. By doing this, it preserves the high pressure that is desired.
- The experimenter will record down the timestamps that are valuable, such as: obvious panic, struggle in multitasking, or obvious misplay. These recorded timestamps will be used to aid the questioning process.

**For Questionnaires (Querying Data):**

- We will use Google Form to construct a structured query form. This platform is highly effective and can effectively process Likert scale and open-ended questions.
- Furthermore, Google Form can automatically categorise replies and process them to produce a clean sheet; this will greatly reduce the workload during the analysis process.

---

# 5. Data Analysis & Success Metrics

Once the data are all collected, we will adopt both a qualitative and quantitative approach to guide our subsequent design iterations and amend existing problems.

**Qualitative Data Analysis (Interviews & Open-ended Questions):**

- We will organise the verbal feedback that we get during the Q&A process and the open-ended answers that we get from the form.
- Afterwards, we will analyse the feedback based on topic; these could be topics such as "lack of UI guidance" or "confusing trap mechanics" in order to identify the existing areas for improvement.

**Quantitative Data Analysis & Metrics:**

- For the structured form, we will calculate the average of the Likert scale for each question and construct a baseline for both satisfaction and usability. We have established the following metrics to dictate our next steps:
  - **Improvement Threshold:** If the average score for the "UI/UX Clarity" statement falls below **3 out of 5**, addressing this will become one of our highest development priorities. It will mandate a redesign of the visual feedback and UI alarm systems before the final release.
  - **Difficulty Tracking:** We will track the average waves reached of all 10 participants. If the data shows that most players are eliminated in the second wave, this shows a clear unequal difficulty curve, and as a response we will modify the difficulty such as reducing enemy health and readjusting the economy system to ensure a better experience.

---

# 6. Timeline

We have established a structured timeline to ensure the evaluation is conducted efficiently and leaves spare time for subsequent game adjustment and amendments before the final deadline.

| Phase | Dates | Activities |
| --- | --- | --- |
| **Preparation Phase** | Oct 9 – Oct 12 | Ensure the WebGL build is stable and fully playable. Finalize and test the Google Forms questionnaire. Contact and schedule exact time slots with the 10 recruited participants. |
| **Execution Phase** | Oct 13 – Oct 18 | Conduct the 5 in-person Post-task Walkthrough sessions including OBS recording and interviewing with corresponding recording. Simultaneously, distribute the WebGL demo link and the online questionnaire to the remaining 5 participants. |
| **Analysis Phase** | Oct 19 – Oct 20 | A dedicated data analysis meeting will be held. We will review the OBS footage and Google Forms spreadsheet to synthesize the findings, ultimately identifying the most critical bugs or design flaws that require immediate attention. This includes major dissatisfaction and potential errors that were shown in the gameplay sessions. |
| **Iteration Phase** | Oct 20 – Oct 31 | Implement changes based on the evaluation feedback. This will involve adjusting combat balancing (e.g., tweaking turret upgrade costs and enemy health) and optimizing the UI visual feedback in preparation for the final Milestone 4. |

---

# 7. Responsibilities

To ensure an equal distribution of workload and to leverage each member's specialized knowledge from the development phase, evaluation tasks have been assigned based on our established GDD roles:

- **Rishabh (Combat & GDD Lead):** In charge of designing the Google Forms, structuring specific questions and feedback questions. Post evaluation he will focus on changing the turret upgrade system, economy system and overall battle balance and feedback.
- **Hitarrth (Player & UI Lead):** Responsible for facilitating and conducting 3 of the in-person Post-task Walkthrough sessions. Mainly focus on observing player controls, UI changes and potential misleading effects of UI.
- **Marcus (Enemy Lead):** Responsible for facilitating the remaining 2 Post-task Walkthrough sessions. He will focus on analyzing qualitative feedback regarding the difficulty curve, monitoring enemy performances in the playthrough for items such as unit health, and the pacing/telegraphing of the Waves.
- **Rongze (Integration Lead):** Responsible for building, testing, and deploying a stable WebGL build to the hosting platform prior to the evaluation launch. Following data collection, will lead the team's data synthesis meeting and identify the issues that need to be raised and will further assist in component merge for all the adopted changes.

---
