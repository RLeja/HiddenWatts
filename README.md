# Hidden Watts

A single-player 2D puzzle and deduction game by Rolands Leja (rl23030). Investigate an unusually high electricity bill, compare appliance usage with household habits, and choose fixes for the biggest sources of waste.

## Playable Prototype I

The repository contains the playable Godot C# project. Follow the instructions below to run it on Windows. A standalone game download is not required for this source-based submission.

- [Prototype I presentation (PDF)](docs/presentation/HiddenWatts-Prototype-I.pdf)
- [Editable presentation (PowerPoint)](docs/presentation/HiddenWatts-Prototype-I.pptx)
- [Scenario data](src/data/scenarios.json)

## How to run

### Requirements

- Windows PC
- **Godot 4.7.2 .NET** (the C# edition, not the standard Godot editor)
- **.NET 8 SDK** installed and available through `dotnet`
- Internet access for the first NuGet restore

### Godot editor

1. Download the repository as a ZIP and extract it, or clone it:

   ```powershell
   git clone https://github.com/RLeja/HiddenWatts.git
   cd HiddenWatts
   ```

2. Open the Godot .NET project manager, select **Import**, and choose `src/project.godot`.
3. Open the project and wait for the assets to finish importing.
4. Build the C# project using Godot's **Build** button. You can also build from the repository root:

   ```powershell
   dotnet restore src/HiddenWatts.csproj
   dotnet build src/HiddenWatts.csproj
   ```

5. Press **F5** to run the main scene and select **Begin investigation**.

The main scene is `src/scenes/ui/main_menu.tscn`. Run the whole project with F5 so the menu and case selection work together.

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

## Current limitations and Prototype II plan

The prototype currently uses one house layout and three authored cases. It has no saved progression between sessions. Proposed next steps are playtesting and balancing the clue system, adding households and cases, clearer feedback after fixes, and saved case progress.

## Development and AI usage

**Developer:** Rolands Leja (rl23030). Solo project. Responsibilities include the concept, design direction, scene editing, asset placement and playtesting.

**OpenAI Codex:** assisted with C# implementation, debugging, collision and UI fixes, script cleanup, JSON scenarios, device clues, documentation and presentation preparation. The game uses authored scenario data and does not call a generative AI service during play.

**Tools:** Godot .NET, C#, Visual Studio, and existing supplied sprite assets. Font license files accompany the bundled fonts.
