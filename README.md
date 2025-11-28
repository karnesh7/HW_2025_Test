# Doofus Adventure — HW_2025_Test

Unity Version: **6000.0.62f1**

## How to Run
1. Open the project in Unity.
2. Load the scene: `Assets/Scenes/Start.unity`.
3. Press Play.
4. Controls: **WASD / Arrow Keys** to move Doofus.

## Game Flow
- Start Screen → Main Game.
- Two pulpits are always active at a time.
- Step on a new pulpit to score.
- Falling results in Game Over (Restart/Quit available).

## Gameplay Logic (from assignment requirements)
- Player speed & pulpit timing loaded from JSON (`Assets/StreamingAssets/DoofusDiary.json`).
- Pulpits spawn adjacent but not on the same previous position.
- Each pulpit has a random destroy timer between `minDestroy` and `maxDestroy`.
- New pulpits spawn with overlap lead from JSON.
- Score increments only when stepping onto a *new* pulpit.

## Folder Notes
- Screenshots: `/Screenshots/`
- Gameplay video: `/Gameplay/`
- Main scripts: `/Assets/Scripts/`

## Features Implemented (Level 1–3)
- Player movement using speed from JSON  
- Pulpit lifecycle (spawn, destroy, countdown UI)  
- Adjacent pulpit placement with no immediate repeats  
- Score system + visited pulpit tracking  
- Start Screen & Game Over Screen UI  
- JSON-driven gameplay parameters  