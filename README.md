# 🏮 Lotus-Lantern: Pure Cultivation 2D Puzzle Game

A procedurally-generated puzzle adventure game built with Unity, featuring grid-based movement, hazard avoidance, and strategic item collection mechanics.

## 📋 Table of Contents

- [Quick Start](#-quick-start)
- [Project Overview](#-project-overview)
- [Game Features](#-game-features)
- [Architecture](#-architecture)
- [Project Structure](#-project-structure)
- [Getting Started](#-getting-started)
- [Development](#-development)
- [Documentation](#-documentation)
- [Project Status](#-project-status)

---

## 🚀 Quick Start

1. **Clone & Open**: Open `My project.sln` in Visual Studio or `My project.slnx` in VS Code
2. **Review Architecture**: Read [PHASE_1_ARCHITECTURE.md](PHASE_1_ARCHITECTURE.md)
3. **Implementation Guide**: Follow [UNITY_STEP_BY_STEP_IMPLEMENTATION.md](UNITY_STEP_BY_STEP_IMPLEMENTATION.md)
4. **Build & Run**: Unity Editor → Play mode

---

## 📖 Project Overview

**Lotus-Lantern** is a 2D grid-based puzzle game where players navigate procedurally-generated levels, collect items, avoid hazards, and manage protective lanterns. The project demonstrates modern game architecture patterns including Service Locator pattern, procedural content generation, and decoupled gameplay systems.

### Game Loop
1. **Procedural Generation** → Level baker creates unique levels using hill-climbing algorithm
2. **Level Selection** → Player chooses difficulty/parameters
3. **Gameplay** → Navigate grid, collect items, manage lantern fuel
4. **Level Progression** → Complete levels to advance

---

## 🎮 Game Features

### Core Mechanics
- **Grid-Based Movement**: 8-directional movement on grid system
- **Procedural Level Generation**: Hill-climbing PCG algorithm with constraint validation
- **Item Collection System**: Lotus, Lighter, Key items with unique mechanics
- **Lantern/Lamp Mechanics**: Protective light source with fuel management
- **Miasma Hazards**: Environmental threats requiring strategic avoidance/protection
- **Level Progression**: Multiple difficulty levels and procedurally varied content

### Gameplay Elements
- **Player Movement & Animation**: Smooth grid-aligned character movement with state-based animations
- **BFS Pathfinding**: Validates level solvability using breadth-first search
- **Audio System**: Ambient and interactive sound management
- **UI Management**: Menu systems, HUD, and level selection interface
- **Save System**: Game state persistence

---

## 🏗️ Architecture

### Phase 1: Core Gameplay
- **PlayerController**: Main player input and state management
- **PlayerMovementComponent**: Grid-based movement logic
- **PlayerAnimationComponent**: Animation state management
- **ItemComponent**: Item pickup and management

### Phase 2: Procedural Content Generation (PCG)
- **PCGLevelBaker**: Generates levels using hill-climbing algorithm
- **LevelValidator**: BFS validation ensures level solvability
- **LevelLoader**: Instantiates generated levels in scene
- **CompletePCGImplementationGuide**: Reference implementation for PCG workflow

### Service Architecture
- **Service Locator Pattern**: Centralized dependency management
- **GameManager**: Game state and orchestration
- **AudioManager**: Sound effect and music management
- **UIManager**: UI screen and menu management
- **LevelService**: Level data management and caching

### Configuration
- **GameConfig**: Scriptable asset storing all game parameters
- **LevelPcg Settings**: PCG algorithm parameters and constraints
- **PlayerSettings**: Movement speed, animation, input configuration

---

## 📁 Project Structure

```
My project/
├── Assets/
│   ├── Game/
│   │   ├── _Scripts/
│   │   │   ├── Core/                    # Core systems
│   │   │   │   ├── GameManager.cs
│   │   │   │   ├── KeybindingManager.cs
│   │   │   │   ├── ServiceLocator.cs
│   │   │   │   ├── LevelService.cs
│   │   │   │   ├── LevelPcg/          # PCG implementation
│   │   │   │   │   ├── PCGLevelBaker.cs
│   │   │   │   │   ├── LevelValidator.cs
│   │   │   │   │   └── ...
│   │   │   │   └── ...
│   │   │   ├── Gameplay/              # Gameplay systems
│   │   │   │   ├── Player/
│   │   │   │   ├── Items/
│   │   │   │   ├── Environment/
│   │   │   │   └── ...
│   │   │   ├── UI/                    # UI and menus
│   │   │   ├── Audio/                 # Audio management
│   │   │   ├── Debug/                 # Debug utilities
│   │   │   └── ...
│   │   ├── Prefabs/                   # Game prefabs
│   │   ├── Scenes/                    # Game scenes
│   │   ├── Audio/                     # Sound files
│   │   ├── Sprites/                   # 2D art assets
│   │   └── Config/                    # Configuration assets
│   └── ...
├── Documentation/                      # Additional documentation
├── PHASE_1_ARCHITECTURE.md            # Phase 1 design doc
├── PHASE_2_INTEGRATION_GUIDE.md       # Phase 2 PCG guide
├── UNITY_STEP_BY_STEP_IMPLEMENTATION.md  # Implementation walkthrough
├── FIXES_APPLIED_SUMMARY.md           # Bug fixes and improvements
└── README.md                           # This file
```

---

## 🛠️ Getting Started

### Prerequisites
- **Unity** (2022.3 LTS or later recommended)
- **Visual Studio Code** or **Visual Studio** for C# editing
- **.NET SDK** (included with Visual Studio)

### Setup Instructions

1. **Open in Unity Editor**
   ```
   1. Launch Unity Hub
   2. Click "Open Project"
   3. Navigate to "My project" directory
   4. Open the project
   ```

2. **Review Key Files**
   - [GameConfig.cs](Assets/Game/_Scripts/Core/GameConfig.cs) - Main configuration
   - [GameManager.cs](Assets/Game/_Scripts/Core/GameManager.cs) - Game orchestration
   - [ServiceLocator.cs](Assets/Game/_Scripts/Core/ServiceLocator.cs) - Dependency injection

3. **Run the Game**
   - Press Play in Unity Editor
   - Navigate through PCG level generation
   - Test gameplay mechanics

### Configuration
- Edit **GameConfig** ScriptableObject in Unity Inspector
- Adjust PCG parameters: map size, item quantities, difficulty
- Configure player movement speed and animation timing
- Set audio volumes and music preferences

---

## 👨‍💻 Development

### Code Organization
- **Namespaces**: `Game.Core`, `Game.Gameplay`, `Game.UI`, `Game.Audio`
- **Design Patterns**: Service Locator, Observer (events), Component-based architecture
- **Coding Standards**: PascalCase for classes, camelCase for fields/properties

### Key Classes

#### Core Systems
- `GameManager`: Main game orchestrator and state machine
- `ServiceLocator`: Static registry for all services
- `LevelService`: Level data and caching

#### Gameplay
- `PlayerController`: Player input and state management
- `PlayerMovementComponent`: Movement logic
- `ItemComponent`: Item pickup system
- `PCGLevelBaker`: Level generation

#### UI
- `UIManager`: Screen management and menu coordination
- `PauseMenuUI`: Pause menu implementation

#### Audio
- `AudioManager`: Sound and music management

### Common Tasks

#### Adding a New GameManager Service
```csharp
// 1. Create interface in Game/Core
public interface IMyService { }

// 2. Implement the service
public class MyService : MonoBehaviour, IMyService { }

// 3. Register in GameManager.Start()
ServiceLocator.Register<IMyService>(GetComponent<MyService>());
```

#### Modifying PCG Parameters
1. Select **GameConfig** asset in Inspector
2. Adjust **LevelPcg** settings
3. Tweak algorithm parameters in **CompletePCGImplementationGuide.cs**

#### Adding New Items
1. Create item prefab in `Assets/Game/Prefabs/Items/`
2. Add ItemType enum to `ItemComponent.cs`
3. Register in `LevelService`

---

## 📚 Documentation

| Document | Purpose |
|----------|---------|
| [PHASE_1_ARCHITECTURE.md](PHASE_1_ARCHITECTURE.md) | Core architecture overview and design patterns |
| [PHASE_2_INTEGRATION_GUIDE.md](PHASE_2_INTEGRATION_GUIDE.md) | PCG system integration and level generation |
| [UNITY_STEP_BY_STEP_IMPLEMENTATION.md](UNITY_STEP_BY_STEP_IMPLEMENTATION.md) | Hands-on implementation guide |
| [FIXES_APPLIED_SUMMARY.md](FIXES_APPLIED_SUMMARY.md) | Bug fixes and improvements applied |
| [KEYBINDING_SETUP_GUIDE.md](KEYBINDING_SETUP_GUIDE.md) | Input system configuration |
| [DEBUG_KEYBINDING_GUIDE.md](DEBUG_KEYBINDING_GUIDE.md) | Debug key bindings reference |

---

## 📊 Project Status

### ✅ Completed
- [x] Core player movement and animation system
- [x] Item collection mechanics
- [x] Procedural level generation (PCG)
- [x] BFS-based level validation
- [x] Level loading and caching system
- [x] UI framework and menus
- [x] Audio management system
- [x] Service Locator architecture
- [x] Configuration system
- [x] Save/Load system framework
- [x] Keybinding system
- [x] Debug utilities

### 🎯 Features
- Grid-based pathfinding
- Lantern/lamp mechanics
- Miasma hazard system
- Multiple difficulty levels
- Procedurally varied content
- Smooth state-based animations

### 🔧 Technical Highlights
- Event-driven architecture
- Dependency injection via Service Locator
- Scriptable object-based configuration
- Component-based gameplay systems
- BFS pathfinding validation
- Hill-climbing PCG algorithm

---

## 🐛 Known Issues & Limitations

- See [FIXES_APPLIED_SUMMARY.md](FIXES_APPLIED_SUMMARY.md) for resolved issues
- Debug utilities (PrefabDebugger, LevelLoadingDebug) present but not essential

---

## 📝 License

This project is part of the Pure Cultivation 2D Puzzle Game development.

---

## 🤝 Contributing

For development guidelines, refer to:
- Code organization: [PHASE_1_ARCHITECTURE.md](PHASE_1_ARCHITECTURE.md)
- PCG modifications: [PHASE_2_INTEGRATION_GUIDE.md](PHASE_2_INTEGRATION_GUIDE.md)
- Implementation steps: [UNITY_STEP_BY_STEP_IMPLEMENTATION.md](UNITY_STEP_BY_STEP_IMPLEMENTATION.md)

---

## 📞 Support

For issues or questions:
1. Review [PHASE_1_ARCHITECTURE.md](PHASE_1_ARCHITECTURE.md) for design questions
2. Check [UNITY_STEP_BY_STEP_IMPLEMENTATION.md](UNITY_STEP_BY_STEP_IMPLEMENTATION.md) for setup/workflow questions
3. See [FIXES_APPLIED_SUMMARY.md](FIXES_APPLIED_SUMMARY.md) for known resolved issues

---

**Last Updated**: May 29, 2026  
**Project Status**: Complete & Ready for Production