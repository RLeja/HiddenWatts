# Hidden Watts

A single-player 2D puzzle and deduction game by Rolands Leja (rl23030). Investigate an unusually high electricity bill, compare appliance usage with household habits, and choose fixes for the biggest sources of waste.

## Playable Prototype II

### Play on Windows

1. [Download the Windows build](https://github.com/RLeja/HiddenWatts/releases/download/prototype-ii/HiddenWatts-Prototype-II-Windows-x64.zip).
2. Extract the entire ZIP into a folder.
3. Double-click **HiddenWatts.exe**.

Players do **not** need Godot, Visual Studio, the .NET SDK or a separate .NET installation. The Windows x64 build includes its runtime. Keep the executable, `HiddenWatts.pck` and the accompanying data folder together.

The game targets Windows 10/11 on Intel or AMD 64-bit PCs.

- [Prototype II presentation (PDF)](docs/presentation/HiddenWatts-Prototype-II-Updated.pdf)
- [Editable presentation (PowerPoint)](docs/presentation/HiddenWatts-Prototype-II-Updated.pptx)
- [Scenario data](src/data/scenarios.json)

## Controls

| Input | Action |
| --- | --- |
| WASD / arrow keys | Move |
| E near a highlighted device | Open its free examination |
| E on the initial examination screen | Spend one detailed inspection |
| Mouse | Choose tabs and corrective actions |
| N | Open or close case notes |
| Esc | Close the current page or pause |

## Short demo walkthrough

1. Choose a household and leave **Random situation** selected, or choose a specific case.
2. Select **Begin investigation** and read the situation briefing, then enter the household.
3. Approach a highlighted appliance and press **E**. Its observation and **First impression** are free.
4. Spend an inspection to reveal wattage, operating time and recent history.
5. Compare the evidence with the household routine and choose a corrective action in **Take action**.
6. Find the required major problems within the inspection budget, then review your medal, score and estimated monthly savings.

Saved readings are free to revisit. After using all inspections, you can still act on inspected devices or close the investigation through case notes. **Retry** keeps the same case. **New case** selects another case in that household. **Next home** advances to the next difficulty after solving a case.

## Working features

- Four furnished households with animated movement, furniture collisions, a walkable garden patio and wall occlusion
- Twenty authored cases loaded from JSON, with five cases per household and seven to eleven interactive electronics per map
- Random case assignment with an entry briefing, plus manual case selection
- Free observations, detailed readings, seven-day usage charts and corrective actions
- Medals, personal bests and completed-household markers saved locally between sessions
- A household counts as completed after solving one of its cases; the menu also shows individual case completion counts

| Household | Difficulty | Inspections | Required problems | Score multiplier |
| --- | --- | --- | --- | --- |
| Garden Home | Easy | 6 | 2 | x1 |
| Corner Apartment | Medium | 6 | 3 | x1.25 |
| Terrace House | Hard | 5 | 3 | x1.5 |
| Main House | Expert | 5 | 4 | x2 |

All four households are available from the menu. A device can behave normally in one case and waste electricity in another. Random assignment chooses a complete case, keeping its clues, readings and correct actions consistent. Personal records persist, but an investigation in progress is not saved.

## Social good integration

The core puzzle asks the player to identify avoidable electricity use and select an appropriate corrective action. It supports **SDG 7 - Affordable and Clean Energy** through energy efficiency and **SDG 13 - Climate Action** through the relationship between electricity use and emissions.

The game uses `kWh = watts / 1000 * hours`. Results estimate 30 days of savings using each case's electricity price and emissions factor. Current cases use EUR 0.25/kWh and 0.20 kg CO2/kWh. These are illustrative game assumptions, not measured household savings or a verified environmental assessment.

Scoring awards 100 points per major problem and 20 per unused inspection after resolving a problem. A clean solved case earns another 50 points. Incorrect actions cost 50 and non-major inspections cost 10 before the household multiplier. Gold requires at least 95% of the maximum score with no wrong actions, Silver requires 75%, and other solved cases earn Bronze. Scores cannot fall below zero.

Records save locally in Godot's `user://progress.json`. Savings are separate from the score.
