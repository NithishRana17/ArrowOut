# Arrow Out - Unity Setup Guide

## Step-by-Step Instructions to Complete the Project

Follow these steps in Unity Editor to get the game running:

---

## Step 1: Install Unity Ads Package

1. Go to **Window > Package Manager**
2. Click **+ dropdown** (top left) → **Add package by name**
3. Enter: `com.unity.ads`
4. Click **Add**
5. Wait for installation to complete

---

## Step 2: Remove LevelPlay (Optional - if causing errors)

If you see LevelPlay errors:
1. Go to **Window > Package Manager**
2. Find **LevelPlay** in the list
3. Click **Remove** button
4. Delete the folder: `Assets/LevelPlay`
5. Delete the folder: `Assets/MobileDependencyResolver`

---

## Step 3: Run Setup Wizard

1. In Unity menu bar, go to **Arrow Out > Setup Game**
2. Click **"1. Setup Sprite Import Settings"**
3. Click **"2. Create Arrow Prefab"**
4. Click **"3. Create Grid Cell Prefab"**
5. Click **"4. Create Main Menu Scene"**
6. Click **"5. Create Game Scene"**
7. Click **"Configure Android Build Settings"**

---

## Step 4: Configure GameScene

1. Open `Assets/Scenes/GameScene.unity`
2. Select **GameManager** in Hierarchy
3. In Inspector, assign:
   - **Grid Manager**: Drag `GridManager` from Hierarchy
   - **UI Manager**: Drag `UIManager` from Hierarchy

4. Select **GridManager** in Hierarchy
5. In Inspector, assign:
   - **Arrow Prefab**: Drag from `Assets/Prefabs/Arrow.prefab`
   - **Grid Cell Prefab**: Drag from `Assets/Prefabs/GridCell.prefab`
   - **Arrow Up**: `Assets/Sprites/arrow_up.png`
   - **Arrow Down**: `Assets/Sprites/arrow_down.png`
   - **Arrow Left**: `Assets/Sprites/arrow_left.png`
   - **Arrow Right**: `Assets/Sprites/arrow_right.png`

6. Select **UIManager** in Hierarchy
7. In Inspector, assign UI references:
   - **Level Text**: Drag LevelText from Canvas/HUD
   - **Life Icons**: Add 3 Image elements for hearts
   - **Grid Toggle Button**: Drag GridToggleButton
   - **Win Screen Panel**: Drag WinScreen
   - **Lose Screen Panel**: Drag LoseScreen
   - Wire all button references

---

## Step 5: Configure Arrow Prefab

1. Open `Assets/Prefabs/Arrow.prefab`
2. In Inspector, assign sprites:
   - **Up Sprite**: `arrow_up.png`
   - **Down Sprite**: `arrow_down.png`
   - **Left Sprite**: `arrow_left.png`
   - **Right Sprite**: `arrow_right.png`
3. Save prefab

---

## Step 6: Add Build Scenes

1. Go to **File > Build Settings**
2. Click **Add Open Scenes** (or drag scenes from Project)
3. Ensure order is:
   - 0: MainMenuScene
   - 1: GameScene

---

## Step 7: Test in Editor

1. Open `GameScene`
2. Press **Play** button
3. Test clicking arrows:
   - Clear path → Arrow exits smoothly
   - Blocked → Flashes red, loses life
4. Test win/lose conditions
5. Test grid toggle button

---

## Step 8: Build Android APK

1. **File > Build Settings**
2. Select **Android**
3. Click **Switch Platform** (wait for recompile)
4. Click **Player Settings**:
   - Company Name: Assignment
   - Product Name: Arrow Out
   - Package Name: `com.assignment.arrowout` (already set)
5. Click **Build**
6. Choose output folder
7. Wait for APK to generate

---

## Troubleshooting

### "Type or namespace 'IronSourceAdInfo' not found"
- Run Step 2 to remove LevelPlay
- The new AdsManager uses Unity Ads instead

### Sprites appear white/blank
- Run Step 3 in Setup Wizard to configure import settings
- OR: Select sprites → Set Texture Type to "Sprite (2D and UI)"

### Arrows don't respond to clicks
- Ensure Arrow prefab has BoxCollider2D
- Ensure camera is Orthographic
- Check Arrow.cs is attached to prefab

### UI not working
- Ensure EventSystem exists in scene
- Check UIManager references are assigned

---

## Unity Ads Info

The game uses Unity Ads in **test mode** by default:
- Android Test Game ID: `5801702`
- iOS Test Game ID: `5801703`

For production:
1. Go to [Unity Dashboard](https://dashboard.unity3d.com/)
2. Create a project and enable Monetization
3. Get your Game IDs
4. Update in AdsManager Inspector
5. Set `Test Mode = false`
