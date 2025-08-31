# Sound Queue Management System

## Overview
The sound queue management system prevents multiple sounds from playing simultaneously when users perform rapid actions, ensuring a clean and non-chaotic audio experience.

## Features

### 1. Global Sound Queue
- **Centralized Management**: All sound requests go through `SoundController.Instance.RequestPlaySound()`
- **Queue Processing**: Sounds are queued and played sequentially with configurable cooldowns
- **Conflict Prevention**: Only one sound plays at a time, preventing audio chaos

### 2. Sound Cooldown System
- **Default Cooldown**: 0.3 seconds between any sounds
- **Configurable Timing**: Adjustable via `SetSoundCooldown(float cooldown)`
- **Minimum Cooldown**: 0.1 seconds minimum to prevent audio artifacts

### 3. Sound Priority Management
- **Queue-Based**: First-in-first-out processing of sound requests
- **Automatic Clearing**: Queue clears when sound is globally muted
- **Smart Fallback**: Direct playback if SoundController unavailable

## Implementation Details

### Core Components

#### SoundController (Enhanced)
```csharp
// Request sound playback through queue system
SoundController.Instance.RequestPlaySound(() => {
    // Sound playing logic here
}, soundDuration);

// Check if sound system is busy
bool isBusy = SoundController.Instance.IsSoundSystemBusy();

// Configure cooldown timing
SoundController.Instance.SetSoundCooldown(0.5f);

// Enable/disable queue system
SoundController.Instance.SetSoundQueueEnabled(true);

// Clear pending sounds
SoundController.Instance.ClearSoundQueue();
```

#### Sound Controllers Integration
All specialized sound controllers now use the queue system:
- `NavigationSoundController` - Navigation instructions and started sounds
- `ToggleNavigationSound` - Navigation visibility toggle sounds
- `NavigationModeSound` - Line/arrow mode switching sounds
- `ToggleLineAdjustSound` - Line height slider toggle sounds
- `QrCodeScannedSound` - QR code scanning success sounds

### Sound Playback Pattern
```csharp
public void PlaySomeSound()
{
    // Use queue system for coordinated playback
    if (SoundController.Instance != null)
    {
        SoundController.Instance.RequestPlaySound(() => {
            if (audioSource != null && soundClip != null)
            {
                audioSource.clip = soundClip;
                audioSource.volume = volume;
                audioSource.Play();
                Debug.Log("Sound played through queue system");
            }
        }, soundClip.length);
    }
    else
    {
        // Fallback for direct playback
        audioSource.clip = soundClip;
        audioSource.volume = volume;
        audioSource.Play();
        Debug.Log("Sound played (fallback)");
    }
}
```

## Configuration Options

### In Inspector (SoundController)
- **Sound Cooldown**: Time between sounds (default: 0.3s)
- **Enable Sound Queue**: Toggle queue system on/off (default: true)

### Runtime Configuration
```csharp
// Adjust cooldown for slower/faster sound transitions
SoundController.Instance.SetSoundCooldown(0.5f); // Half-second cooldown

// Temporarily disable queue for emergency sounds
SoundController.Instance.SetSoundQueueEnabled(false);

// Check system status
bool isBusy = SoundController.Instance.IsSoundSystemBusy();
```

## Usage Scenarios

### Rapid User Actions
- **Problem**: User quickly toggles navigation, changes mode, adjusts line height
- **Solution**: Sounds queue automatically, playing one after another
- **Result**: Clean audio feedback without overlap or chaos

### Priority Handling
- **Navigation Started**: High priority, played immediately
- **UI Toggles**: Normal priority, queued if conflicts occur
- **Mode Changes**: Normal priority, respects cooldown timing

### Global Mute Integration
- **Muted State**: All sounds blocked, queue automatically cleared
- **Unmuted State**: Queue processing resumes normally
- **State Changes**: Smooth transition without audio artifacts

## Benefits

1. **Audio Clarity**: No overlapping sounds creating audio chaos
2. **User Experience**: Predictable audio feedback timing
3. **Performance**: Efficient queue processing with minimal overhead
4. **Flexibility**: Configurable timing and queue behavior
5. **Robustness**: Fallback mechanisms for edge cases
6. **Integration**: Seamless with existing sound systems

## Technical Notes

### Singleton Pattern
- `SoundController.Instance` provides global access
- Persists across scene changes with `DontDestroyOnLoad`
- Automatic duplicate instance cleanup

### Coroutine Management
- Queue processing uses coroutines for timing control
- Automatic cleanup when queue empties
- Safe handling of component destruction

### Memory Management
- Queue cleared when appropriate (mute, disable)
- No memory leaks from queued actions
- Efficient lambda expressions for sound actions

### Error Handling
- Graceful fallback when SoundController unavailable
- Null checks for all audio components
- Informative debug logging for troubleshooting

## Integration Status

✅ **NavigationSoundController** - Navigation instructions + started sound
✅ **ToggleNavigationSound** - Navigation visibility toggle
✅ **NavigationModeSound** - Line/arrow mode switching  
✅ **ToggleLineAdjustSound** - Line height slider toggle
✅ **QrCodeScannedSound** - QR code scanning success
✅ **SoundController** - Global mute/unmute + queue management

All sound systems now work together harmoniously, preventing audio conflicts during rapid user interactions.
