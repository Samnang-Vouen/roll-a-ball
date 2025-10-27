# Roll A Ball (Timer & Lives)

A simple Unity game where you roll a ball, collect all the pickups, and win before the timer runs out. If time expires, you can Continue (use a life) or start a New Game (reset lives).

## How to Play

- Move the ball to collect all pickup objects.
- Win by collecting every pickup before the timer reaches 00:00.
- If time runs out, a panel appears with:
  - Continue: Lose one life and retry the level with a fresh timer.
  - New Game: Reset lives to 3 and restart with a fresh timer.

## Controls

- Move: WASD or Arrow Keys
- After winning: Press R to start a New Game (same as clicking the New Game button)

## UI

- Count (top-left): shows collected pickups vs total, e.g., `Count: 0 / 12`.
- Time (top-right): shows remaining time as `Time: mm:ss`.
- Lives (top-right): shows remaining lives, e.g., `Lives: 3`.
- Timeout Panel: appears when time expires with Continue and New Game buttons.

The timer/lives texts are placed at the top-right to avoid overlapping the Count display in the top-left.

## Timer & Lives

- Starting lives: 3
- Time limit: 1 minute (60 seconds)
- Continue: consumes 1 life and restarts the current scene with a fresh 1-minute timer.
- New Game: resets lives to 3 and the timer to 1 minute, then restarts the scene.
- After you win, the timer stops and the timeout panel won’t ever show.

## Project Setup

1. Open this folder in Unity (use the provided `Roll A Ball.sln` only if opening from an IDE; Unity loads the project from the folder).
2. Open the scene under `Assets/Scenes`.
3. Press Play.

No scene wiring is required:
- A `GameManager` is auto-created if it’s not in the scene.
- A Canvas + basic UI (timer, lives, timeout panel) and an EventSystem are auto-created if missing.

## Customization

You can change key settings in `Assets/Scripts/GameManager.cs`:

- `startingLives` — default 3
- `levelTimeSeconds` — default 60 (1 minute)

## Folder Highlights

- `Assets/Scripts/`
  - `PlayerController.cs` — movement, pickup count, win handling.
  - `GameManager.cs` — lives, timer, timeout UI (Continue/New Game), scene reloads.
- `Assets/Prefabs/` — sample prefabs for scene setup.
- `Assets/Scenes/` — the playable scene(s).

[Click here to view the design](https://www.canva.com/design/DAG26iv9D4M/aCv2jhmzM7PIdRo9lFIP6g/edit?utm_content=DAG26iv9D4M&utm_campaign=designshare&utm_medium=link2&utm_source=sharebutton)
