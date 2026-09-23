# AsteroidsRedux

A 2D Asteroids clone built in Unity, used as a sandbox for practicing clean, decoupled game architecture patterns (Observer/event channels, finite state machines, object pooling, data-driven design).

## Tech Stack

- **Engine:** Unity 6000.0.62f1 (Unity 6)
- **Render Pipeline:** Universal Render Pipeline (URP) 2D
- **Input:** Unity Input System
- **UI:** TextMeshPro
- **Language:** C#

## Project Structure

```
Assets/
├── Art/            Sprites for ships and projectiles
├── Audio/          SFX for projectiles and UFO
├── Data/           ScriptableObject config assets (Enemies, Ship, Weapons, Events)
├── Prefabs/        Player/enemy/projectile prefabs
├── Scenes/         Game scenes
└── Scripts/
    ├── Combat/       Bullet behavior
    ├── Enemies/      Asteroid, and UFO + its AI state machine
    ├── Events/       ScriptableObject event channels (Int, Vector3, Void)
    ├── Managers/     ScoreManager
    ├── Player/       PlayerController, PlayerShooter, PlayerHealth
    ├── Pooling/      Generic object pooling system
    ├── ScriptableObjects/  Config assets (Ship, Weapon, Asteroid, UFO)
    └── UI/           ScoreUI
```

## Architecture

### Data-driven configuration (ScriptableObjects)

Gameplay tuning values (ship thrust/speed, weapon fire rate, asteroid speed/score, UFO stats) live in `ScriptableObject` assets (`ShipConfig`, `WeaponConfig`, `AsteroidConfig`, `UfoConfigSO`) instead of being hardcoded on components. This lets designers rebalance the game or create variants without touching code or recompiling.

### Observer pattern via event channels

Cross-system communication (score changes, player death) is decoupled using `ScriptableObject`-based event channels (`IntEventChannelSO`, `Vector3EventChannelSO`, `VoidEventChannelSO`) under `Scripts/Events`. Publishers (e.g. `Asteroid`, `UfoController`) raise events on a channel asset; subscribers (e.g. `ScoreManager`, `ScoreUI`) listen without any direct reference to the publisher. This keeps gameplay, scoring, and UI fully independent of one another.

### Enemy AI via finite state machine

The UFO enemy (`UfoController`) drives its behavior through a small interface-based FSM (`IState` / `StateMachine`), with `UfoEntryState` (flies in from off-screen) transitioning to `UfoEngageState` (weaves around and fires at the player) once it's on-screen. Each state owns its own update/movement/exit logic, making it easy to add new behaviors without branching `if` logic in the controller.

### Object pooling

Frequently spawned/destroyed objects (bullets, asteroids, UFOs) are recycled through a generic `ObjectPool` keyed by prefab name, using an `IPoolable` interface (`OnSpawnFromPool` / `OnReturnToPool`) so each object can reset its own state. This avoids per-frame `Instantiate`/`Destroy` allocations and GC spikes during combat.

## Core Gameplay Systems

- **Player** (`PlayerController`, `PlayerShooter`, `PlayerHealth`): Arcade-style thrust/rotate movement with screen wrapping, pooled bullet firing on a cooldown, and trigger-based death handling.
- **Asteroids** (`Asteroid`): Drift and rotate with random velocity, wrap around the screen, and split into smaller asteroids (via `NextSizePrefab`) when destroyed, raising a score event.
- **UFO** (`UfoController` + FSM states): Enters from off-screen, then loops between weaving toward/around the player and firing aimed shots, with engine audio volume that scales based on on/off-screen distance.
- **Scoring** (`ScoreManager`, `ScoreUI`): Listens for score-added events, accumulates total score, and broadcasts score-changed events consumed by the TextMeshPro UI.

## Requirements

- Unity Hub with editor version `6000.0.62f1` installed
- Open the project via Unity Hub, pointing at the repository root
