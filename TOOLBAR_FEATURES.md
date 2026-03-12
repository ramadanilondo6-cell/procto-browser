# Procto - Toolbar Features Guide

## ðŸŽ¯ New Toolbar Features (Version 2.1.0)

### Bottom Toolbar Overview

A new bottom toolbar has been added with the following controls:

```
â”Œâ”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”
â”‚  [ðŸ ] [ðŸ”„]                              â˜€ï¸ [====|====] 100%  ðŸ”Š [====|====] 100%  â”‚
â””â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”˜
```

### Features

#### 1. ðŸ  Home Button
- **Icon**: House emoji (ðŸ )
- **Function**: Navigates back to the start URL defined in config
- **Usage**: Click to return to the exam home page
- **Shortcut**: None (button only for security)

#### 2. ðŸ”„ Reload Button
- **Icon**: Refresh emoji (ðŸ”„)
- **Function**: Reloads the current page
- **Usage**: Click to refresh the exam page
- **Shortcut**: F5 is blocked for students, use this button instead

#### 3. â˜€ï¸ Brightness Control
- **Icon**: Sun emoji (â˜€ï¸)
- **Type**: Slider (0-100%)
- **Function**: Adjusts screen brightness
- **Range**: 0% (darkest) to 100% (brightest)
- **Default**: 100%
- **Implementation**: Smooth transition using timer-based opacity adjustment
- **Use Case**: Reduce eye strain during long exams

#### 4. ðŸ”Š Volume Control
- **Icon**: Speaker emoji (ðŸ”Š)
- **Type**: Slider (0-100%)
- **Function**: Controls system volume
- **Range**: 0% (mute) to 100% (maximum)
- **Default**: 100%
- **Implementation**: Windows Core Audio API (IAudioEndpointVolume)
- **Use Case**: Adjust audio for listening comprehension tests

## ðŸŽ¨ Design Features

### Visual Style
- **Background**: Dark theme (#1e1e2e)
- **Height**: 56 pixels
- **Button Size**: 44x44 pixels
- **Button Style**: Rounded corners (8px)
- **Hover Effect**: Blue highlight (#4a90d9)
- **Pressed Effect**: Darker blue (#3a7bc8)
- **Shadow**: Drop shadow for depth

### Slider Design
- **Track**: Dark background (#2d2d3a) with rounded corners
- **Fill**: Blue progress indicator (#4a90d9)
- **Thumb**: 16px circle with white stroke
- **Value Display**: Percentage text next to slider

## ðŸ”§ Technical Implementation

### Files Modified

1. **MainWindow.xaml**
   - Added bottom toolbar Grid row
   - Added Home and Reload buttons
   - Added Brightness and Volume sliders
   - Defined new button and slider styles

2. **MainWindow.xaml.cs**
   - Added `BtnHome_Click()` handler
   - Added `BtnReload_Click()` handler
   - Added `BrightnessSlider_ValueChanged()` handler
   - Added `VolumeSlider_ValueChanged()` handler
   - Added brightness timer for smooth transitions
   - Added volume controller integration

3. **VolumeController.cs** (NEW)
   - Windows Core Audio API wrapper
   - Uses IAudioEndpointVolume interface
   - Provides SetVolume(), GetVolume(), Mute(), Unmute()
   - Graceful fallback if audio device unavailable

### Brightness Control Implementation

```csharp
// Smooth brightness transition
private void BrightnessTimer_Tick(object sender, EventArgs e)
{
    double difference = _targetBrightness - _currentBrightness;
    
    if (Math.Abs(difference) < 0.01)
    {
        _currentBrightness = _targetBrightness;
        _brightnessTimer.Stop();
    }
    else
    {
        _currentBrightness += difference * 0.1; // Smooth interpolation
    }
    
    Browser.Opacity = _currentBrightness;
}
```

### Volume Control Implementation

```csharp
// Uses Windows Core Audio API
public void SetVolume(int volumePercent)
{
    float volume = volumePercent / 100f;
    _audioEndpointVolume.SetMasterVolumeLevelScalar(volume, Guid.Empty);
}
```

## ðŸ“‹ Configuration

The toolbar is always visible and cannot be disabled by students. However, you can customize the default values in the code:

### Default Settings (in MainWindow.xaml.cs)
```csharp
private double _currentBrightness = 1.0;  // 100%
// Volume default is set in XAML: Value="100"
```

### To Change Defaults
Edit `MainWindow.xaml`:
```xml
<Slider x:Name="BrightnessSlider" Value="80"/>  <!-- 80% default -->
<Slider x:Name="VolumeSlider" Value="50"/>      <!-- 50% default -->
```

## ðŸ”’ Security Considerations

### What Students Can Access
- âœ… Home button
- âœ… Reload button
- âœ… Brightness slider
- âœ… Volume slider

### What Students Cannot Access
- âŒ Browser address bar
- âŒ Browser navigation buttons (back/forward)
- âŒ Context menu (right-click disabled)
- âŒ Keyboard shortcuts (Ctrl+R, F5, etc.)
- âŒ Developer tools
- âŒ Settings menu

### Toolbar Security
- Toolbar is part of the locked window
- Cannot be hidden or minimized
- Always visible on top
- Window is topmost and non-resizable

## ðŸ› Known Limitations

### Brightness Control
- **Limitation**: Uses opacity overlay, not actual monitor brightness
- **Effect**: Makes content appear darker, but doesn't reduce backlight
- **Alternative**: For hardware brightness control, would require monitor DDC/CI API

### Volume Control
- **Limitation**: Requires Windows Vista or later
- **Dependency**: Windows Core Audio API
- **Fallback**: Logs volume change if API unavailable
- **Platform**: Windows only (not cross-platform)

## ðŸŽ¯ Usage Scenarios

### Scenario 1: Exam with Listening Comprehension
1. Teacher sets volume to appropriate level before exam
2. Students can adjust volume if needed during audio playback
3. Volume setting persists across page reloads

### Scenario 2: Long Exam Session
1. Student finds screen too bright
2. Adjusts brightness slider to comfortable level
3. Reduces eye strain during 2-3 hour exam

### Scenario 3: Page Load Issue
1. Exam page fails to load properly
2. Student clicks Reload button
3. Page refreshes without leaving exam

### Scenario 4: Navigation Needed
1. Student accidentally navigates away
2. Or exam system redirects incorrectly
3. Student clicks Home button to return to start URL

## ðŸ“Š Comparison with Safe Exam Browser (SEB)

| Feature | Procto | SEB |
|---------|-------------|-----|
| Home Button | âœ… | âœ… |
| Reload Button | âœ… | âœ… |
| Brightness Control | âœ… | âŒ |
| Volume Control | âœ… | âŒ |
| Touch-friendly UI | âœ… | âš ï¸ |
| Dark Theme | âœ… | âš ï¸ |
| Customizable | âœ… | âš ï¸ |

## ðŸ”® Future Enhancements

Potential future additions:
- [ ] Fullscreen toggle (if allowed)
- [ ] Zoom control
- [ ] Contrast adjustment
- [ ] Color filter (for accessibility)
- [ ] On-screen keyboard toggle
- [ ] Calculator popup
- [ ] Timer display
- [ ] Network status indicator
- [ ] Battery level indicator

## ðŸ“ Testing Checklist

Before deployment, test:
- [ ] Home button navigates to correct URL
- [ ] Reload button refreshes page
- [ ] Brightness slider adjusts smoothly
- [ ] Volume slider controls system volume
- [ ] All buttons show hover effects
- [ ] Sliders display correct percentage
- [ ] Toolbar visible on all screen sizes
- [ ] Toolbar doesn't overlap browser content
- [ ] Brightness/Volume persist during session
- [ ] No console errors in browser

## ðŸš€ Build Instructions

```bash
# Build with new toolbar features
build-single.cmd both

# Output locations
publish\win-x64\  (64-bit version)
publish\win-x86\  (32-bit version)
```

---

**Version**: 2.1.0  
**Release Date**: March 11, 2026  
**Author**: Procto Team
