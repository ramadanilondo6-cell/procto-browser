# SafeExamCEF - Changelog

## Version 2.1.0 (March 2026) - Toolbar Update

### 🎯 New Features

#### Bottom Toolbar
- **Home Button (🏠)**: Navigate back to start URL
- **Reload Button (🔄)**: Refresh current page
- **Brightness Control (☀️)**: Adjust screen brightness (0-100%)
- **Volume Control (🔊)**: Control system volume (0-100%)

#### Volume Controller
- New `VolumeController.cs` class
- Uses Windows Core Audio API (IAudioEndpointVolume)
- Proper COM interface implementation
- Graceful fallback if audio unavailable

#### Brightness Control
- Smooth transition animation using timer
- Opacity-based brightness adjustment
- 50ms timer for fluid updates
- Interpolation algorithm for smooth changes

### 🎨 UI Enhancements

#### Toolbar Design
- Dark theme matching status bar (#1e1e2e)
- 56-pixel height
- 44x44 pixel buttons with emoji icons
- Rounded corners (8px)
- Blue hover effects (#4a90d9)
- Drop shadow for depth
- Custom slider design with blue progress indicator

#### Button Styles
- New `ToolBarButtonStyle` for consistent appearance
- Hover and pressed state animations
- ToolTip support for accessibility

#### Slider Styles
- Custom `VolumeSliderStyle` template
- Dark track (#2d2d3a) with blue fill (#4a90d9)
- 16px circular thumb with white stroke
- Percentage value display

### 🐛 Bug Fixes

#### KeyboardHook
- **Fixed**: Missing F1-F12 key constants
- **Added**: All function key constants (VK_F1 through VK_F12)
- **Fixed**: Alphabetical ordering of key constants
- **Added**: VK_Q constant for Ctrl+Q blocking
- **Fixed**: Duplicate key code definitions
- **Improved**: Code organization and readability

#### Code Quality
- Removed duplicate constant definitions
- Organized constants alphabetically
- Added proper disposal of volume controller
- Improved resource cleanup in Window_Closing

### 📝 Documentation

#### New Files
- `TOOLBAR_FEATURES.md` - Complete toolbar feature guide
- Updated `BUILD_INSTRUCTIONS.md` with toolbar info
- Updated `CHANGELOG.md` with all changes

### 🔧 Technical Changes

#### MainWindow.xaml
- Added bottom toolbar Grid row (56px height)
- Added navigation buttons (Home, Reload)
- Added brightness and volume sliders
- Defined new control styles

#### MainWindow.xaml.cs
- Added `_brightnessTimer` for smooth transitions
- Added `_volumeController` for audio control
- Added `_targetBrightness` and `_currentBrightness` fields
- Implemented event handlers for all toolbar controls
- Added proper resource cleanup

#### VolumeController.cs (NEW)
- COM interface definitions for Windows Audio API
- MMDeviceEnumerator for device selection
- IAudioEndpointVolume for volume control
- Methods: SetVolume, GetVolume, Mute, Unmute, IsMuted
- Proper IDisposable implementation

### 📊 Statistics

- **Files Added**: 1 (VolumeController.cs)
- **Files Modified**: 3 (MainWindow.xaml, MainWindow.xaml.cs, KeyboardHook.cs)
- **Lines Added**: ~400
- **New Controls**: 4 (Home, Reload, Brightness, Volume)
- **New APIs**: Windows Core Audio API

---

## Version 2.0.0 (March 2026)

### 🎨 UI Improvements

#### MainWindow
- **Modern Dark Theme**: Updated status bar with professional dark color scheme (#1e1e2e)
- **Enhanced Status Bar**: 
  - Added app icon/logo with graduation emoji (🎓)
  - Added "Ujian Aktif" status indicator with green badge
  - Improved clock display with background container
  - Better spacing and visual hierarchy
- **Button Styling**:
  - Custom hover/press effects
  - Rounded corners (6px radius)
  - Smooth transitions
  - Improved "Keluar Ujian" button with red accent

#### QuitDialog
- **Complete Redesign**:
  - Larger dialog (420x260px)
  - Modern card-based layout with rounded corners (12px)
  - Drop shadow effect for depth
  - Icon-based header with door emoji (🚪)
  - Improved password input with custom styling
  - Better button layout and sizing
  - Enhanced color scheme (#1a1a2e background)

#### AlertDialog
- **Visual Enhancements**:
  - Larger size (480x280px)
  - Warning icon with background container (⚠️)
  - Bordered design with warning color (#f5a623)
  - Message box with subtle background
  - Improved button styling
  - Better text hierarchy

### 🔒 Security Enhancements

#### Keyboard Hook Improvements
- **Fixed Modifier Key Tracking**: 
  - Replaced unreliable boolean flags with `GetAsyncKeyState()` API
  - Proper detection of Ctrl, Alt, Win, and Shift states
  - Supports left/right modifier keys separately

- **New Blocked Shortcuts**:
  - **Windows Keys**: Win+E, Win+R, Win+D, Win+M, Win+I, Win+X, Win+A, Win+C, Win+S, Win+L, Win+P, Win+V, Win+T, Win+B, Win+H, Win+K
  - **Browser Shortcuts**: Ctrl+R, Ctrl+P, Ctrl+S, Ctrl+V, Ctrl+C, Ctrl+X, Ctrl+A, Ctrl+Z, Ctrl+Y
  - **Tab Management**: Ctrl+T, Ctrl+W, Ctrl+N, Ctrl+O, Ctrl+Tab, Ctrl+Shift+Tab
  - **Developer Tools**: F12, Ctrl+Shift+I, Ctrl+Shift+J, Ctrl+Shift+C, Ctrl+U, Ctrl+I
  - **Navigation**: Ctrl+L, Ctrl+K, Ctrl+D, Ctrl+H, Ctrl+J, Ctrl+F, Ctrl+G, Ctrl+B
  - **System Keys**: PrintScreen, ScrollLock, Pause, Insert, Delete, Home, End, PageUp, PageDown
  - **Task Manager**: Ctrl+Shift+Esc
  - **Other**: F1, F2, F3, F6, F10, F11, Alt+Left/Right

#### Lockdown Engine
- Maintains existing functionality with improved keyboard hook
- Process monitoring for forbidden applications
- URL whitelist filtering
- Config key hash for SEB compatibility

### 🏗️ Architecture Support

#### Multi-Architecture Builds
- **Project File Updates**:
  - Added support for x86, x64, and AnyCPU platforms
  - Configured runtime identifiers: `win-x64`, `win-x86`
  - Added application manifest for Windows compatibility

- **Single-File Publishing**:
  - Enabled `PublishSingleFile=true` for bundled executables
  - Configured `IncludeNativeLibrariesForSelfExtract=true` for CefSharp compatibility
  - Enabled `EnableCompressionInSingleFile=true` for smaller file sizes
  - Created `build-single.cmd` script for easy single-file builds

- **Build Script Improvements**:
  - New `build.bat` with multi-architecture support
  - New `build-single.cmd` optimized for single-file publishing
  - Command-line arguments: `both`, `x64`, `x86`
  - Separate publish directories for each architecture
  - Automatic configuration file copying
  - Better error handling and logging

#### Build Output Structure
```
publish/
├── win-x64/                  # 64-bit version
│   ├── SafeExamCEF.exe       ← Single-file executable (~70MB)
│   ├── CefSharp.dll          ← Required (CefSharp dependencies)
│   ├── libcef.dll            ← Required (Chromium core)
│   ├── *.pak                 ← Required (resource files)
│   ├── *.bin                 ← Required (dictionary files)
│   └── config/
│       └── default.safeexam.json
└── win-x86/                  # 32-bit version
    └── (same structure as x64)
```

**Note**: CefSharp cannot be fully bundled into a single file. The main executable is single-file, but CefSharp dependencies must remain as separate files.

### 📝 Documentation

#### New Files
- `BUILD_INSTRUCTIONS.md` - Comprehensive build guide
- `CHANGELOG.md` - This changelog
- `app.manifest` - Windows application manifest

#### Updated Files
- `DEVELOPMENT.md` - Updated with completed features

### 🐛 Bug Fixes

1. **Keyboard Hook Modifier Tracking**
   - **Before**: Used boolean flags that could get out of sync
   - **After**: Uses `GetAsyncKeyState()` for reliable real-time state detection

2. **Missing Key Codes**
   - Added all necessary virtual key codes for comprehensive shortcut blocking

3. **Build Configuration**
   - Fixed project file to support multiple architectures
   - Removed hardcoded single-architecture settings

### ⚙️ Configuration

No changes to configuration file format. Existing `default.safeexam.json` files remain compatible.

### 📦 Dependencies

Same as v1.0.0:
- CefSharp.Wpf.NETCore 120.1.110
- Serilog 3.1.1
- Serilog.Sinks.File 5.0.0
- Newtonsoft.Json 13.0.3
- .NET 6.0

### 🚀 Migration Guide

#### For Users
1. Download the appropriate version for your system (x64 for most modern PCs)
2. Copy your existing `default.safeexam.json` to the new installation directory
3. Run `SafeExamCEF.exe`

#### For Developers
1. Update your local .NET SDK to .NET 6 if not already installed
2. Run `build.bat both` to build for all architectures
3. Check output in `publish/win-x64/` and `publish/win-x86/`

### 📊 Statistics

- **Files Modified**: 8
- **Lines Added**: ~600
- **Lines Removed**: ~150
- **New Shortcuts Blocked**: 60+
- **UI Components Enhanced**: 3

### ✅ Completed Tasks

- [x] UI modernization with Safe Exam Browser-like design
- [x] Fix keyboard hook modifier key tracking
- [x] Block comprehensive list of keyboard shortcuts
- [x] Support for x86 and x64 architectures
- [x] Improved build script
- [x] Enhanced documentation
- [x] Windows application manifest

### 🔮 Future Enhancements

- [ ] Encrypted configuration files
- [ ] Remote proctoring features
- [ ] Screenshot detection
- [ ] Network monitoring
- [ ] Advanced logging and reporting
- [ ] Auto-update mechanism
- [ ] Custom themes support

---

**Build Date**: March 11, 2026  
**Target Framework**: .NET 6.0  
**Supported Platforms**: Windows 7, 8, 10, 11 (x86 and x64)
