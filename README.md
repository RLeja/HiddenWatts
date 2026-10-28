# Hidden Watts

A single-player 2D puzzle and deduction game by Rolands Leja (rl23030). Investigate an unusually high electricity bill, compare appliance usage with household habits, and choose fixes for the biggest sources of waste.

## Playable Prototype I

### Play on Windows

1. [Download the Windows build](https://github.com/RLeja/HiddenWatts/releases/latest/download/HiddenWatts-Prototype-I-Windows-x64.zip).
2. Extract the entire ZIP into a folder.
3. Double-click **HiddenWatts.exe**.

Players do **not** need Godot, Visual Studio, the .NET SDK or a separate .NET installation. The Windows x64 build includes its runtime. Keep the executable, `HiddenWatts.pck` and the accompanying data folder together.

The game targets Windows 10/11 on Intel or AMD 64-bit PCs.

- [Prototype I presentation (PDF)](docs/presentation/HiddenWatts-Prototype-I.pdf)
- [Editable presentation (PowerPoint)](docs/presentation/HiddenWatts-Prototype-I.pptx)
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

1. Enter the household and approach an appliance with a glowing outline.
2. Press E to read its observation and **First impression**. This costs nothing. Visit other devices to compare clues before deciding where to inspect.
3. Spend an inspection to reveal wattage, operating time and recent history.
4. Compare the evidence with the household routine and choose an action in **Take action**.
5. Find and fix **three major problems** within **six detailed inspections**.
6. Review the score and estimated monthly electricity, money and emissions savings.

Saved readings are free to revisit. After all inspections are used, the player can still act on inspected devices or close the investigation through the notes page. A restart keeps the current case. Returning to the menu selects a different case.

## Working features

- Furnished house, animated character movement, furniture collisions and house boundaries
- Eleven appliances with local E interactions
- Free case-specific observations before committing an inspection
- Readings, seven-day usage charts, household history and corrective actions
- Collected evidence notes, inspection limits and score penalties
- Results with estimated savings, pause, restart and menu navigation
- Three complete cases loaded from JSON: **After hours**, **Empty rooms**, and **After the guests**

The same appliance can be normal in one case and wasteful in another. Cases keep their clues, readings and correct actions together.

## Social good integration

The core puzzle asks the player to identify avoidable electricity use and select an appropriate corrective action. It supports **SDG 7 - Affordable and Clean Energy** through energy efficiency and **SDG 13 - Climate Action** through the relationship between electricity use and emissions.

The game uses `kWh = watts / 1000 * hours`. Results estimate 30 days of savings using each case's electricity price and emissions factor. Current cases use EUR 0.25/kWh and 0.20 kg CO2/kWh. These are illustrative game assumptions, not measured household savings or a verified environmental assessment.

Scoring awards 100 points per major problem resolved and 10 points per unused inspection after resolving a problem. Each incorrect action costs 50 points and inspecting a non-major device costs 10 points. Scores cannot fall below zero.
