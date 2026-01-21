# Arrow Out - Unity Puzzle Game

A mobile puzzle game where players tap arrows to clear them from a grid. Built with Unity 6 and New Input System.

## 📱 Features

- **5 Progressively Harder Levels** - From easy to challenging puzzles
- **Lives System** - 3 lives per level, wrong moves cost lives
- **Level Selection** - Choose unlocked levels from main menu
- **Progress Saving** - Highest completed level saved automatically
- **Camera Controls** - Pinch to zoom, two-finger pan on mobile
- **Tap Wave Effect** - Visual feedback on touch
- **Unity Ads Integration** - Banner and Interstitial ads
- **Debug Toggles** - Inspector-controlled console logging

## 🎮 How to Play

1. Tap an arrow to make it move
2. If the path is clear, the arrow exits and is cleared
3. If blocked by another arrow, it bumps back and you lose a life
4. Clear all arrows to win the level
5. Lose all 3 lives and it's game over

## 📦 Project Structure

```
Assets/
├── Editor/
│   └── LevelDataEditor.cs      # Visual level editor for ScriptableObjects
├── Levels/
│   ├── Level 1.asset           # LevelDataSO ScriptableObjects
│   ├── Level 2.asset
│   ├── Level 3.asset
│   ├── Level 4.asset
│   └── Level 5.asset
├── Scenes/
│   ├── MainMenuScene.unity     # Main menu with level selection
│   └── GameScene.unity         # Main gameplay scene
├── Scripts/
│   ├── Arrow.cs                # Arrow movement, collision, rendering
│   ├── ArrowData.cs            # Arrow configuration data structure
│   ├── ArrowPathData.cs        # Path data for maze levels
│   ├── MazeManager.cs          # Level spawning, game state, lives
│   ├── LevelDataSO.cs          # ScriptableObject for level definitions
│   ├── UIManager.cs            # HUD, win/lose screens
│   ├── MainMenu.cs             # Main menu & level selection
│   ├── AdsManager.cs           # Unity Ads (banner + interstitial)
│   ├── CameraController.cs     # Pinch zoom & pan controls
│   ├── TapWaveEffect.cs        # Touch ripple animation
│   ├── GameSettings.cs         # Frame rate & performance settings
│   └── UI/
│       └── RoundedImage.cs     # Custom UI component for rounded buttons
└── Sprites/
    ├── arrow_up.png
    ├── arrow_down.png
    ├── arrow_left.png
    ├── arrow_right.png
    └── heart.png
```

## 🔧 Key Scripts

| Script | Purpose |
|--------|---------|
| `Arrow.cs` | Handles arrow rendering (LineRenderer + mesh head), movement animation, collision detection, bump & reverse behavior |
| `MazeManager.cs` | Spawns arrows from LevelDataSO, manages lives, handles level completion |
| `LevelDataEditor.cs` | Custom Inspector with visual grid editor for creating levels |
| `CameraController.cs` | Two-finger pan and pinch-to-zoom using Enhanced Touch API |
| `AdsManager.cs` | Unity Ads initialization, banner display, interstitial ads |

## 📢 Unity Ads Configuration

The game uses Unity Ads:
- **Game ID (Android)**: `6028445`
- **Game ID (iOS)**: `6028444`
- **Banner**: `Banner_Android` - Bottom of screen
- **Interstitial**: `Interstitial_Android` - After level completion

### Production Setup
1. Create project at [Unity Dashboard](https://dashboard.unity3d.com/)
2. Enable Monetization
3. Update IDs in `AdsManager.cs`
4. Set `testMode = false`

## 🔧 Building for Android

1. Open **File > Build Settings**
2. Select **Android** platform
3. Click **Switch Platform**
4. Click **Build** or **Build And Run**

### Requirements
- Unity 6 (or Unity 2022.3+)
- Android Build Support module
- Input System package

## 🎨 Level Editor

Create new levels using ScriptableObjects:
1. Right-click in Project > **Create > Game > Level Data**
2. Select the asset and open Inspector
3. Use the visual grid editor to:
   - Add arrows with colored buttons
   - Click cells to draw waypoints
   - Set arrow direction and color
4. Assign to MazeManager level slots

## ⚙️ Debug Mode

All scripts include `enableDebugLog` toggle in Inspector:
- Enable to see console logs for that script
- Disable to silence output
- Useful for isolating issues

## 📄 License

This project was created as an assignment demonstration.
