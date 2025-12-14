# Tree Collision System Setup Guide

This guide will help you set up the tree collision system with collision detection and counter display.

## Prerequisites

All scripts have been created:
- `Manager.cs` - Extended with tree counter
- `TreeCollision.cs` - Tree collision detection and pushing
- `PlayerPhysics.cs` - Enhanced with tree collision handling
- `TreeCollisionSetup.cs` - Editor utility for tree setup

## Setup Steps

### 1. Set Up Tree Colliders and Components

**Option A: Using the Editor Tool (Recommended)**
1. In Unity, go to **Tools > Setup Tree Collisions**
2. Enter the tree name pattern (default: "trees-new-try")
3. Assign the GameManager GameObject (the one with the Manager script)
4. Configure collider settings:
   - Use Capsule Collider (recommended for trees)
   - Radius: 1.0 (adjust based on tree size)
   - Height: 5.0 (adjust based on tree height)
5. Click **"Setup All Trees"**
6. This will automatically:
   - Add colliders to trees that don't have them
   - Add TreeCollision components to all trees
   - Link the GameManager reference

**Option B: Manual Setup**
1. Select each tree GameObject in the scene
2. Add a Collider component:
   - **CapsuleCollider** (recommended) or **BoxCollider**
   - Set `isTrigger = false` (important for physics collision)
   - Adjust size to match tree dimensions
3. Add the `TreeCollision` component
4. Assign the GameManager GameObject to the `Game Manager` field
5. Adjust `Push Force` if needed (default: 5)
6. Adjust `Collision Cooldown` if needed (default: 1 second)

### 2. Configure PlayerPhysics (Optional but Recommended)

1. Select the XR Rig GameObject (the one with `PlayerPhysics` component)
2. In the Inspector, find the `PlayerPhysics` component
3. Set the **Tree Layer** field:
   - Create a new layer for trees (e.g., "Trees") in **Edit > Project Settings > Tags and Layers**
   - Assign all tree GameObjects to this layer
   - Set the Tree Layer mask in PlayerPhysics to match
4. Adjust settings if needed:
   - **Tree Check Distance**: How far ahead to check for trees (default: 1.0)
   - **Tree Push Force**: Force applied when tree detected ahead (default: 3.0)

### 3. Set Up Tree Counter UI

1. **Locate existing score UI elements:**
   - Find the Canvas in your scene
   - Locate the existing score text GameObjects (ScoreTextHead and ScoreTextButton)

2. **Create new TextMeshProUGUI for tree counter:**
   - Right-click on the Canvas in Hierarchy
   - Select **UI > Text - TextMeshPro**
   - Rename it to "ScoreTextTree"

3. **Configure the text:**
   - Set text to "Score: 0"
   - Position it near the other score texts (e.g., below ScoreTextButton)
   - Set font size to 25 (to match existing counters)
   - Choose a color (e.g., brown or green to represent trees)
   - Set alignment to match other score texts

4. **Link to Manager:**
   - Select the GameObject with the Manager script
   - In the Inspector, find the Manager component
   - Drag the "ScoreTextTree" GameObject into the **Score Text Tree** field

### 4. Configure TreeCollision Component Settings

For each tree (or use the editor tool to set defaults):

1. **Game Manager**: Must be assigned (the GameObject with Manager script)
2. **Push Force**: How hard to push player away (default: 5)
   - Increase for stronger push
   - Decrease for gentler push
3. **Collision Cooldown**: Time between collision triggers (default: 1 second)
   - Prevents counter from incrementing too rapidly
   - Increase if counter increments multiple times per collision
4. **Player Tag**: Tag name for XR Rig (default: "Player")
   - If your XR Rig has a different tag, set it here
   - Or leave empty to use automatic detection

### 5. Test the System

1. **Play the scene**
2. **Walk into a tree** - You should:
   - Be pushed to the side (left or right based on approach direction)
   - See the tree counter increment in the UI
   - Not be able to walk through the tree

3. **Troubleshooting:**
   - **Player goes through trees**: Check that colliders are not set as triggers
   - **No counter increment**: Verify GameManager is assigned in TreeCollision components
   - **Player gets stuck**: Increase push force or check collider sizes
   - **Counter increments too fast**: Increase collision cooldown

## Configuration Summary

### Tree Setup Checklist
- [ ] All trees have Collider components (non-trigger)
- [ ] All trees have TreeCollision components
- [ ] GameManager is assigned in all TreeCollision components
- [ ] Trees are on a specific layer (optional, for PlayerPhysics)

### UI Setup Checklist
- [ ] ScoreTextTree GameObject created
- [ ] ScoreTextTree assigned to Manager.scoreTextTree field
- [ ] Text displays "Score: 0" initially

### PlayerPhysics Setup Checklist (Optional)
- [ ] Tree layer created and assigned to trees
- [ ] Tree Layer mask set in PlayerPhysics component
- [ ] Tree Check Distance and Push Force adjusted if needed

## Notes

- The system uses two collision detection methods:
  1. **TreeCollision.cs**: Detects collisions and pushes player away (primary method)
  2. **PlayerPhysics.cs**: Proactively checks for trees ahead (optional enhancement)

- Push direction is calculated based on which side of the tree the player approaches from:
  - Approach from right → Push to left
  - Approach from left → Push to right

- The collision cooldown prevents the counter from incrementing multiple times if the player stays in contact with a tree.
