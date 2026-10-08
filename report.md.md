
---
### 1. Evaluation Method

**Technique 1：Observational Methods - Post-task Walkthroughs**
*  **Participant**：5

* **Reason of Choice**： Breach is high pressured game, it emphasises resources management and camera changes. If the player was request  to "Think-Aloud" during the proccess is will greatly disrup their train of thought and focuus. more importantly, it will disrupt the claustrophobic and tense atmosphere, hence having negative influences for the gameplay. Therefore, by adopting Post-task Walkthroughs, recording the gameplay first and then question the playwards  is the most effective, non-intrusive method to observe their true in-game behavior and decision-making.

* **Playtest Task**：Participant will begin with tutproal and try to live until the end of wave 5. the core aiom is to maintain constant control of the four outer room  nad the control room, maintaining the HP of all rooms and reduce the number of enemy breached into the main control room.



**Technique 2. Query Techniques - Questionnaires**
* **Participant**：5

* **Reason of Choice**：Questionnaries allows structural and quantative method of breaking down the game's aspects. effectively and directly collect feedback on items such as UI clarity, control difficult, learning curves and etc, and corperating it with several open ended responce this can produce a quantative performance benchmark of the participants feedback

* **Playtest Task**：Participant it to complete the entire demo(defeat 5 wave) and fill in  they will fill out an online structured survey.

---
### 2. Designed Questions

To ensure our evaluation yields targeted and actionable feedback, we have designed specific questions tailored to the core mechanics, UI/UX, and narrative of *BREACH*


#### A. Questions for Post-task Walkthroughs
*(The following question will be asked verbally by experimenter while watching the recording game  foortage with the participant. The questions are designed to be dynamic prompts, as adaptation may be needed based on specific scenarios)*



1. **On Multitasking & Camera Switching:** "In this footage, I noticed you focused heavily on [Room A] and didn't check [Room B] for an extended period. Was this a deliberate strategic choice to prioritize a specific route, or was it simply too overwhelming to manage the camera switching under pressure?"

2. **On resource management**: "At this moment, you chose  [e.g., hoard power even when full / immediately spend all power / deactivate a specific turret]. Can you explain to me what was your thought process for managing your Power resource at this exact moment?"

3. **On UI & Feedback Awareness:** " here, [e.g., a room was breached / an alarm went off / the minimap disappeared]. Did you notice the UI changes immediately, and how did losing that room's passive ability affected your up coming decision?""

4. **On Enemy Mechanics :** "During this phase of the ight when [e.g., The enemy was at a unexpected spot], you seemed to [e.g., hesitate / react quickly]. What was your immediate reaction to this specific interference, and how did you adapt your strategy?""


#### B. Questions for Questionnaires
These questions will be distributed via a structured Google Form after the participant completes the entire demo, allowing us to collect quantitative usability data and overall impressions.

**Scalar Questions (1 = Strongly Disagree, 5 = Strongly Agree):**
1. **Core Mechanic:** "The concept of 'Power' being a shared, allocatable resource—rather than just a consumable currency—was intuitive and easy to understand."
2. **UI/UX Clarity:** "The sidebar UI and Minimap clearly indicated to me what every button did and was easily operated"
3. **Difficulty Curve:** "The difficulty progression and enemy scaling from Wave 1 to Wave 5 felt smooth, fair, and reasonable."
4. **Atmosphere:** "The visual and audio feedback successfully conveyed a sense of high tension and claustrophobia."

**Open-ended Questions:**
1. **Identifying Pain Points:** "What was the most frustrating or confusing aspect of managing four different rooms simultaneously?"

2. **General Improvement:** "If you could change or add one feature to make the surveillance or combat system better, what would it be?"

---
### 3. Participants
To ensure that the collected feedback are highly relavent and can represent the targets player base, we have established a clear recruitment strategy and qualifying criteria.

**Recruitment Strategy：**

We will recruit in total of 10 participants (5 for each technical). The recruitment targets will be mainly facing IT related and Computer Science student, while also seeking for participants in online gaming communities such as the ones on discord.

**qualifying criteria：**


**Qualifying Criteria:**
To align with the "Target Audience" outlined in our Game Design Document, participants will be screened to ensure they have relevant gaming experience. We are specifically looking for players who are familiar with at least one of the following genres:
*   **Tower Defense (TD) Games:** e.g., *Plants vs. Zombies*, *Arknights*.
*   **Surveillance/Management Games:** e.g., *Five Nights at Freddy's (FNAF)*.
*   **Real-Time Strategy (RTS) & Resource Management Games:** e.g., Paradox Interactive titles ("Grand Strategy" games), *StarCraft*.

In addition to this,  because the core gameplay of *BREACH* relies entirely on rapid UI navigation, camera switching, and real-time resource toggling, participants must be comfortable with **high-frequency mouse operations and multitasking** under pressure. This ensures that any usability issues discovered are related to our game's design flaws rather than the participant's lack of basic PC gaming experience. 

---
### 4. Data Collection
To accurately capture player behaviours and actionable feedback, we will use specific tools and method  tailored to each of our chosen evaluation techniques.


**For Post-task Walkthroughs (Observational Data):**  
- We will use screen capoturing softwares such as **OBS Studio** to record the participants complete playgame, focusing on UI interaction, camera switch frequence and mouse trails. During the proccess, the experimenter will sit aside and stay quite, performing no interfereance in order to maintain the atmosphere. The experimental will note down important stand point. eg. Panics, obvcious mistakes, struggles. Afterwards these time stamps will be used to question the participants during the replay reviewing stage

**For Questionnaires (Querying Data):**  
- We will distribute out structued survey using **Google Forms**. This platform is highly efficeint for this task, as it can smoothly handle both likert-scale questions and open-ended tex fields.  Furthermore, Google Forms will automatically compile the submitted responses and convert them into an organized spreadsheet format, significantly streamlining our quantitative data analysis process.

---
### 6. Timeline

We have established a structured timeline to ensure the evaluation is conducted efficiently and leaves ample time for subsequent game iterations before the final deadline.

* **Oct 8 - Oct 9 (Preparation Phase):** Ensure the WebGL build is stable and fully playable. Finalize and test the Google Forms questionnaire. Contact and schedule exact time slots with the 10 recruited participants.

* **Oct 13 - Oct 16 (Execution Phase):** Conduct the 5 in-person Post-task Walkthrough sessions (including OBS recording and interviewing). Simultaneously, distribute the WebGL demo link and the online questionnaire to the remaining 5 participants.

* **Oct 20 (Analysis Phase):** Team Techtonic will hold a dedicated data analysis meeting. We will review the OBS footage and Google Forms spreadsheet to synthesize the findings, ultimately identifying the top 3 most critical bugs or design flaws that require immediate attention.

* **Oct 21 - Oct 28 (Iteration Phase):** Implement changes based on the evaluation feedback. This will involve adjusting combat balancing (e.g., tweaking turret upgrade costs and enemy health) and optimizing the UI visual feedback in preparation for the final Milestone 4 release.

---
### 7. Responsibilities

To ensure an equal distribution of workload and to leverage each member's specialized knowledge from the development phase, evaluation tasks have been assigned based on our established GDD roles:

* **Rishabh (Combat & GDD Lead):** Responsible for designing the specific questions and statements for the Google Forms questionnaire. Post-evaluation, he will focus on analyzing player feedback regarding the turret upgrade economy, damage scaling, and overall combat balance.

* **Hitarrth (Player & UI Lead):** Responsible for facilitating and moderating 3 of the in-person Post-task Walkthrough sessions. He will specifically observe and analyze any visual blind spots or usability issues players encounter when interacting with the camera-switching UI and the minimap.

* **Marcus (Enemy Lead):** Responsible for facilitating the remaining 2 Post-task Walkthrough sessions. He will focus on analyzing qualitative feedback regarding the difficulty curve, specifically monitoring enemy pathfinding speed, unit health, and the pacing/telegraphing of the Wave 5 Boss abilities.

* **Rongze (Integration Lead):** Responsible for building, testing, and deploying a stable WebGL build to the hosting platform prior to the evaluation launch. Following data collection, he will lead the team's data synthesis meeting and delegate the resulting iteration tasks across our GitHub development schedule.