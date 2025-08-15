# Sound Effects Setup Instructions

## Overview
I've added a comprehensive sound system to your Count Master Clone game with the following features:
- **Positive Gate Sound**: Plays when soldiers are added (addition/multiplication)
- **Negative Gate Sound**: Plays when soldiers are removed (subtraction/division) 
- **Soldier Death Sound**: Plays whenever a soldier dies (from obstacles or math operations)

## Files Created/Modified

### New Files:
- `SoundManager.cs` - Handles all game audio with singleton pattern

### Modified Files:
- `MathGate.cs` - Added sound effects for math operations
- `ArmyManager.cs` - Added death sounds when soldiers are removed

## Setup Instructions

### 1. Create SoundManager GameObject
1. Create an empty GameObject in your scene
2. Name it "SoundManager"
3. Add the `SoundManager` component to it
4. The AudioSource will be automatically added

### 2. Assign Sound Clips
In the SoundManager inspector, assign these audio clips from your `Casual Game Sounds U6/CasualGameSounds/` folder:

**Recommended Sound Assignments:**
- **Positive Gate Sound** (Add/Multiply): `levelUp.wav` or `Pop.wav`
- **Negative Gate Sound** (Subtract/Divide): `DM-CGS-43.wav` or `Swoosh.wav`  
- **Soldier Death Sound**: `Kill.wav` or `DM-CGS-44.wav`

### 3. Volume Settings
Adjust the volume levels in the SoundManager:
- **Gate Volume**: 0.7 (default)
- **Death Volume**: 0.5 (default)

### 4. Make SoundManager Persistent (Optional)
The SoundManager is set up as a singleton and will persist across scenes automatically.

## How It Works

### Math Gate Sounds
- **Addition (+)** and **Multiplication (×)**: Plays positive sound
- **Subtraction (-)** and **Division (÷)**: Plays negative sound

### Soldier Death Sounds  
- Plays whenever a soldier dies from:
  - Obstacles
  - Math gate operations that reduce army size
  - Manual soldier removal

## Testing
1. Run the game
2. Go through math gates to hear gate sounds
3. Hit obstacles with soldiers to hear death sounds
4. Use gates that subtract/divide to hear both death and gate sounds

## Customization
You can easily customize sounds by:
- Changing the assigned AudioClips in the SoundManager
- Adjusting volume levels
- Adding new sound effects by extending the SoundManager class

The system is designed to be flexible and expandable for future audio needs!
