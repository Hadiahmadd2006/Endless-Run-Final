# Endless Run Final

Arcade-style three-lane endless runner built in Unity 6000.2.2f1. Dodge obstacles, grab collectibles, and chase a high score with simple lane-switching and jump controls.

## Features
- Smooth lane swapping and jump physics with animation triggers on run, jump, and death.
- Randomized obstacle/collectible rows driven by `SpawnManager` (per-lane spawn chances, grounding, and despawn settings).
- Looping environment tiles via `EnvironmentLooper` to fake infinite track length.
- HUD with live score and survival timer, plus main menu, pause, description, and game-over panels managed by `GameManager`.
- Audio sliders wired through `AudioManager` to control multiple `AudioSource` volumes.

## Project Layout
- `Assets/Scenes/EndlessRunner.unity` — main playable scene.
- `Assets/Scripts/` — gameplay scripts (`PlayerController`, `GameManager`, `SpawnManager`, `EnvironmentLooper`, `Collectible`, `Obstacle`, `ScoreManager`, `AudioManager`, etc.).
- `Assets/Obstacles And Collectibles/` — prefabs and ScriptableObject data for pickups and hazards (create more via `Create > EndlessRunner > Collectible` or `Obstacle`).
- `Assets/InputSystem_Actions.inputactions` — input map (currently using keyboard keys in `PlayerController`).

## Requirements
- Unity 6000.2.2f1 (or newer in the 6000.2 line).
- TextMeshPro (already part of the project).
- New Input System package enabled if you use the provided input actions asset.

## How to Play (Editor)
1. Open the project in Unity 6000.2.2f1.
2. Load `Assets/Scenes/EndlessRunner.unity`.
3. Press Play.
4. Controls: `A`/`Left Arrow` = move left, `D`/`Right Arrow` = move right, `Space` = jump. Collect coins, avoid obstacles. Use on-screen buttons to start, pause, resume, or restart.

## Key Tuning Points
- `GameManager` (in the scene): hook UI references, toggle `autoStart`, adjust `gameOverDelay`.
- `SpawnManager`: tweak `spawnInterval`, `perLaneCollectibleChance`, `perLaneObstacleChance`, lane spacing, grounding, and scroll speed.
- `EnvironmentLooper`: adjust `scrollSpeed`, `segmentLength`, and recycle buffer to match your track meshes.
- `PlayerController`: set lane offsets, jump force, ground check distance, and animation clip names; assign jump/death SFX.
- `CollectibleData` / `ObstacleData`: set prefabs, point values, bonus duration, and hit/pickup SFX.

## Build
1. `File > Build Settings…`
2. Add `Assets/Scenes/EndlessRunner.unity` to Scenes In Build.
3. Choose target platform, set build path, and click **Build** (or **Build And Run**).

## Notes
- Time scale is paused whenever menus (main, description, pause, game over) are shown; HUD resumes normal time.
- Score and timer reset on every restart; game over is triggered by hitting an obstacle or falling below the Y clamp in `PlayerController`.
