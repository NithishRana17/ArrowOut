# Arrow Out - Unity Puzzle Game

A mobile puzzle game where players tap arrows to clear them from a grid. Arrows can only be cleared if their path is unobstructed.

## 📱 Features

- **4 Progressively Harder Levels** - From easy 3x3 grids to challenging 6x5 puzzles
- **Lives System** - 3 lives per level, wrong moves cost lives
- **Grid Toggle** - Show/hide grid lines for visual assistance
- **Win/Lose Screens** - Complete game flow with retry and level progression
- **LevelPlay Ads Integration** - Banner and Interstitial ads using ironSource SDK
- **Sound Effects** - Audio feedback for actions

## 🎮 How to Play

1. Tap an arrow to clear it
2. If the arrow's path is clear, it exits the grid
3. If blocked by another arrow, it flashes red and you lose a life
4. Clear all arrows to win the level
5. Lose all 3 lives and it's game over

## 🚀 Quick Setup (Unity Editor)

1. Open the project in Unity 2022.3+ (Unity 6 recommended)
2. Go to **Arrow Out > Setup Game** in the menu bar
3. Click **"Setup All"** to automatically:
   - Configure sprite import settings
   - Create Arrow and GridCell prefabs
   - Create MainMenu and Game scenes
4. Click **"Configure Android Build Settings"**
5. Open `Assets/Scenes/GameScene.unity`
6. In the **GameManager** object:
   - Assign the **GridManager** reference
   - Assign the **UIManager** reference
7. In the **GridManager** object:
   - Assign Arrow and GridCell prefabs
   - Assign arrow sprites (up/down/left/right)
8. Connect UI elements in **UIManager**

## 📦 Project Structure

```
Assets/
├── Audio/              # Sound effects (add your own)
├── Editor/
│   └── ArrowOutSetup.cs    # Editor setup wizard
├── Levels/             # Level data (unused - levels in code)
├── Prefabs/
│   ├── Arrow.prefab        # Arrow game object
│   └── GridCell.prefab     # Grid cell with patterns
├── Scenes/
│   ├── MainMenuScene.unity
│   └── GameScene.unity
├── Scripts/
│   ├── Arrow.cs            # Arrow behavior
│   ├── GridCell.cs         # Grid cell visuals
│   ├── GridManagerV2.cs    # Grid spawning & management
│   ├── GameManagerV2.cs    # Game state & lives
│   ├── UIManager.cs        # UI handling
│   ├── MainMenu.cs         # Main menu controller
│   ├── AudioManager.cs     # Sound management
│   ├── AdsManager.cs       # LevelPlay ads integration
│   ├── LevelData.cs        # Level data structure
│   └── LevelConfigurations.cs  # Built-in level definitions
└── Sprites/
    ├── arrow_up.png
    ├── arrow_down.png
    ├── arrow_left.png
    ├── arrow_right.png
    ├── grid_cell.png
    └── heart.png
```

## 📢 Unity Ads

The game uses Unity Ads for advertisements:

- **Banner Ads** - Displayed at the bottom of all screens
- **Interstitial Ads** - Shown after each level (win or lose)

### Ads Configuration
The game uses test mode by default. For production:
1. Create a project at [Unity Dashboard](https://dashboard.unity3d.com/)
2. Enable Monetization and get Game IDs
3. Update AdsManager with your IDs
4. Set `Test Mode = false`

## 🔧 Building for Android

1. Go to **File > Build Settings**
2. Select **Android** platform
3. Click **Switch Platform** (if needed)
4. Click **Build** or **Build And Run**

### Build Requirements
- Unity 2022.3+ with Android Build Support
- Android SDK (API 25+)
- JDK 11+ (bundled with Unity)

## 📝 Level Design Notes

All levels are designed to be **solvable without deadlocks**:
- No two arrows face each other
- Edge arrows always have clear exit paths
- Interior arrows have dependencies that can be resolved

## 🎨 Art Assets

Arrow sprites are procedurally generated with:
- Gradient colors (cyan, green, orange, purple)
- Soft glow effects
- Rounded geometric shapes

## 📄 License

This project was created as an assignment demonstration.
