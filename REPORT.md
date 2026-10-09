

### 1. Evaluation Techniques 

**Technique 1：1. Observational Methods - Post-task Walkthroughs**
*  **Participants**: 5 

*  **Reason of Choice**：- BREACH is a high-pressure game that places heavy emphasis on resource management and rapid camera switching. If players were requested to use the "Think-Aloud" method during gameplay, it would severely interrupt their cognitive flow and concentration. More importantly, it would ruin the claustrophobic atmosphere and tension created by the game, negatively affecting their actual gameplay performance. Therefore, recording the gameplay and questioning them afterwards based on the video footage is the most effective, non-intrusive method to observe their true in-game behavior and decision-making.

* **Playtest Task**: Participants are tasked with playing and surviving until the end of the demo (Wave 5). Their core objective is to maintain maximum Barrier HP and minimize the number of enemies breaching the Main Control Room by constantly managing the four external rooms.


**Technique 2. Query Techniques - Questionnaires**
* **Participants**: 5 

*  **Reason of Choice**： Questionnaires allow us to quickly and intuitively collect quantitative data regarding UI clarity, control difficulty, and the game's learning curve in a structured format (e.g., a 1-5 Likert scale). Combined with a few open-ended questions, this method will also help us gauge whether players truly grasp the core "Power allocation" mechanic and understand the narrative.

* **Playtest Task**: - Participants will be asked to play through the entire demo (attempting to defeat the Wave 5. After finishing the play session, they will fill out an online structured survey.


---
### 2. Designed Questions

To ensure our evaluation yields targeted and actionable feedback, we have designed specific questions tailored to the core mechanics, UI/UX, and narrative of *BREACH*.

#### A. Questions for Post-task Walkthroughs
*(These questions will be asked verbally by the experimenter while reviewing the recorded gameplay footage with the participant. The questions are designed as dynamic prompts to be adapted based on the player's specific in-game actions.)*

1. **On Multitasking & Camera Switching:** "In this footage, I noticed you focused heavily on [Room A] and didn't check [Room B] for an extended period. Was this a deliberate strategic choice to prioritize a specific route, or was it simply too overwhelming to manage the camera switching under pressure?"

2. **On Resource Management:** "At this timestamp, you chose to [e.g., hoard power even when full / immediately spend all power / deactivate a specific turret]. Can you walk me through your thought process for managing your Power resource at this exact moment?"

3. **On UI & Feedback Awareness:** "Right here, [e.g., a room was breached / an alarm went off / the minimap disappeared]. Did you notice the UI changes immediately, and how did losing that room's passive ability influence your next move?"

4. **On Enemy Mechanics :** "During this phase of the ight when [e.g., the EMP disabled your power / the camera jammed], you seemed to [e.g., hesitate / react quickly]. What was your immediate reaction to this specific interference, and how did you adapt your strategy?"

#### B. Questions for Questionnaires
These questions will be distributed via a structured Google Form after the participant completes the entire demo, allowing us to collect quantitative usability data and overall impressions.

**Scalar Questions (1 = Strongly Disagree, 5 = Strongly Agree):**
1. **Core Mechanic:** "The concept of 'Power' being a shared, allocatable resource—rather than just a consumable currency—was intuitive and easy to understand."
2. **UI/UX Clarity:** "The sidebar UI and Minimap clearly indicated to me which corridors were currently under attack or successfully breached."
3. **Difficulty Curve:** "The difficulty progression and enemy scaling from Wave 1 to Wave 4 felt smooth, fair, and reasonable."
4. **Atmosphere:** "The visual and audio feedback successfully conveyed a sense of high tension and claustrophobia."

**Open-ended Questions:**
1. **Identifying Pain Points:** "What was the most frustrating or confusing aspect of managing four different rooms simultaneously?"
2. **Narrative Comprehension:** "Did you understand the narrative twist after defeating the Boss (i.e., that you are actually the AI, and the enemies are a containment team sent by the creators)? How did this reveal impact your overall experience?"
3. **General Improvement:** "If you could change or add one feature to make the surveillance or combat system better, what would it be?"

---

### 3. Participants

To ensure the feedback we collect is highly relevant and representative of our intended player base, we have established a clear recruitment strategy and qualifying criteria.

**Recruitment Strategy:**
We will recruit a total of **10 participants** (5 dedicated to the Post-task Walkthroughs, and 5 for the Questionnaires). The recruitment will be specifically targeting IT and computer science students—as well as through our personal circles of friends and gaming communities on Discord.

**Qualifying Criteria:**
To align with the "Target Audience" outlined in our Game Design Document, participants will be screened to ensure they have relevant gaming experience. We are specifically looking for players who are familiar with at least one of the following genres:
*   **Tower Defense (TD) Games:** e.g., *Plants vs. Zombies*, *Arknights*.
*   **Surveillance/Management Games:** e.g., *Five Nights at Freddy's (FNAF)*.
*   **Real-Time Strategy (RTS) & Resource Management Games:** e.g., Paradox Interactive titles ("Grand Strategy" games), *StarCraft*.

Furthermore, because the core gameplay of *BREACH* relies entirely on rapid UI navigation, camera switching, and real-time resource toggling, participants must be comfortable with **high-frequency mouse operations and multitasking** under pressure. This ensures that any usability issues discovered are related to our game's design flaws rather than the participant's lack of basic PC gaming literacy. 


---

### 4. Data Collection

To accurately capture both player behavior and actionable feedback, we will utilize specific tools and approaches tailored to each of our chosen evaluation techniques.

- **For Post-task Walkthroughs (Observational Data):**  
    We will use screen recording software (**OBS Studio**) to capture the participant's complete gameplay footage, specifically tracking their UI interactions, camera switching frequency, and mouse trajectories. During the session, the experimenter will sit nearby but remain completely silent and non-intrusive, preserving the game's intended high-tension atmosphere. The experimenter will silently note down the exact "timestamps" of notable moments—such as when the player displays visible panic, gets flustered by multitasking, or makes obvious mechanical errors. These timestamped notes will directly guide the questions asked during the subsequent video review phase.
    
- **For Questionnaires (Querying Data):**  
    We will distribute our structured survey using **Google Forms**. This platform is highly efficient for this task, as it can smoothly handle both Likert-scale questions and open-ended text fields. Furthermore, Google Forms will automatically compile the submitted responses and convert them into an organized spreadsheet format, significantly streamlining our quantitative data analysis process.

---

### 5. Data Analysis & Success Metrics

Once the data is collected, we will process it using both qualitative and quantitative methods to guide our subsequent design iterations.

- **Qualitative Data Analysis (Interviews & Open-ended Questions):**  
    We will transcribe the verbal feedback from the post-task walkthroughs and the written responses from the open-ended questionnaire fields. We will then perform a thematic analysis to identify core "pain points" by grouping the feedback into actionable themes (e.g., categorizing recurring issues into themes such as "Insufficient UI feedback" or "Confusion regarding trap mechanics").
    
- **Quantitative Data Analysis & Metrics:**  
    For the structured questionnaires, we will calculate the mean scores for all 1-5 Likert-scale questions to establish a baseline of player satisfaction and usability. We have established the following metrics to dictate our next steps:
    
    - **Improvement Threshold:** If the average score for the "UI/UX Clarity" statement falls below **3.5 out of 5**, addressing this will become our highest development priority. It will mandate a redesign of the visual feedback and UI alarm systems before the final release.
        
    - **Difficulty Tracking:** We will track the "Average Wave Reached" across all 10 participants. If the data shows that the majority of players are eliminated by Wave 2, it will clearly indicate an unbalanced difficulty curve. In response, we will adjust the game's economy (e.g., increasing the starting Power capacity) or reduce enemy health pools to ensure a fairer experience.
---

### 6. Timeline

We have established a structured timeline to ensure the evaluation is conducted efficiently and leaves ample time for subsequent game iterations before the final deadline.

* **Oct 10 - Oct 12 (Preparation Phase):** Ensure the WebGL build is stable and fully playable. Finalize and test the Google Forms questionnaire. Contact and schedule exact time slots with the 10 recruited participants.

* **Oct 13 - Oct 16 (Execution Phase):** Conduct the 5 in-person Post-task Walkthrough sessions (including OBS recording and interviewing). Simultaneously, distribute the WebGL demo link and the online questionnaire to the remaining 5 participants.

* **Oct 17 - Oct 18 (Analysis Phase):** Team Techtonic will hold a dedicated data analysis meeting. We will review the OBS footage and Google Forms spreadsheet to synthesize the findings, ultimately identifying the top 3 most critical bugs or design flaws that require immediate attention.

* **Oct 19 - Oct 25 (Iteration Phase):** Implement changes based on the evaluation feedback. This will involve adjusting combat balancing (e.g., tweaking turret upgrade costs and enemy health) and optimizing the UI visual feedback in preparation for the final Milestone 4 release.

---

### 7. Responsibilities

To ensure an equal distribution of workload and to leverage each member's specialized knowledge from the development phase, evaluation tasks have been assigned based on our established GDD roles:

* **Rishabh (Combat & GDD Lead):** Responsible for designing the specific questions and statements for the Google Forms questionnaire. Post-evaluation, he will focus on analyzing player feedback regarding the turret upgrade economy, damage scaling, and overall combat balance.

* **Hitarrth (Player & UI Lead):** Responsible for facilitating and moderating 3 of the in-person Post-task Walkthrough sessions. He will specifically observe and analyze any visual blind spots or usability issues players encounter when interacting with the camera-switching UI and the minimap.

* **Marcus (Enemy Lead):** Responsible for facilitating the remaining 2 Post-task Walkthrough sessions. He will focus on analyzing qualitative feedback regarding the difficulty curve, specifically monitoring enemy pathfinding speed, unit health, and the pacing/telegraphing of the Wave 5 Boss abilities.

* **Rongze (Integration Lead):** Responsible for building, testing, and deploying a stable WebGL build to the hosting platform prior to the evaluation launch. Following data collection, he will lead the team's data synthesis meeting and delegate the resulting iteration tasks across our GitHub development schedule.

---