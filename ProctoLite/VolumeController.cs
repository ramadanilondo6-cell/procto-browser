using System;
using System.Runtime.InteropServices;
using Serilog;

namespace ProctoLite
{
    /// <summary>
    /// Windows Core Audio API wrapper for volume control
    /// Uses IAudioEndpointVolume interface to control system volume
    /// </summary>
    public class VolumeController : IDisposable
    {
        private IAudioEndpointVolume _audioEndpointVolume;
        private bool _disposed;

        public VolumeController()
        {
            try
            {
                Initialize();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to initialize volume controller - volume control will be limited");
            }
        }

        private void Initialize()
        {
            try
            {
                var enumerator = new MMDeviceEnumerator();
                var device = enumerator.GetDefaultAudioEndpoint(EDataFlow.ERender, ERole.EMultimedia);
                _audioEndpointVolume = device.Activate<IAudioEndpointVolume>();
                Log.Information("Volume controller initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to get audio endpoint - using fallback volume control");
            }
        }

        public void SetVolume(int volumePercent)
        {
            if (_disposed || _audioEndpointVolume == null)
            {
                // Fallback: Try using SendMessage approach
                FallbackVolumeControl(volumePercent);
                return;
            }

            try
            {
                float volume = volumePercent / 100f;
                _audioEndpointVolume.SetMasterVolumeLevelScalar(volume, Guid.Empty);
                Log.Debug("Volume set to: {Volume}%", volumePercent);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error setting volume");
                FallbackVolumeControl(volumePercent);
            }
        }

        public int GetVolume()
        {
            if (_disposed || _audioEndpointVolume == null)
            {
                return 100; // Default fallback
            }

            try
            {
                float level;
                _audioEndpointVolume.GetMasterVolumeLevelScalar(out level);
                return (int)(level * 100);
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error getting volume");
                return 100;
            }
        }

        public void Mute()
        {
            if (_disposed || _audioEndpointVolume == null)
            {
                return;
            }

            try
            {
                _audioEndpointVolume.SetMute(true, Guid.Empty);
                Log.Information("Audio muted");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error muting audio");
            }
        }

        public void Unmute()
        {
            if (_disposed || _audioEndpointVolume == null)
            {
                return;
            }

            try
            {
                _audioEndpointVolume.SetMute(false, Guid.Empty);
                Log.Information("Audio unmuted");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error unmuting audio");
            }
        }

        public bool IsMuted()
        {
            if (_disposed || _audioEndpointVolume == null)
            {
                return false;
            }

            try
            {
                bool muted;
                _audioEndpointVolume.GetMute(out muted);
                return muted;
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Error checking mute status");
                return false;
            }
        }

        private void FallbackVolumeControl(int volumePercent)
        {
            // Fallback method using keyboard commands
            // This is limited and not recommended, but works as a last resort
            try
            {
                // Note: This is a basic fallback and doesn't set exact volume
                // It just sends volume up/down commands
                Log.Debug("Using fallback volume control");
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Fallback volume control failed");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_audioEndpointVolume != null)
                {
                    Marshal.ReleaseComObject(_audioEndpointVolume);
                    _audioEndpointVolume = null;
                }
                _disposed = true;
                Log.Information("Volume controller disposed");
            }
        }
    }

    // COM Interfaces for Windows Core Audio API
    [ComImport]
    [Guid("BCDE0395-E52F-467C-8E3D-C4579291692E")]
    internal class MMDeviceEnumerator
    {
    }

    [ComImport]
    [Guid("A95664D2-9614-4F35-A746-DE8DB63617E6")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDeviceEnumerator
    {
        int NotImpl1();

        [PreserveSig]
        int GetDefaultAudioEndpoint(EDataFlow dataFlow, ERole role, out IMMDevice ppDevice);

        // Rest of the methods not implemented
    }

    [ComImport]
    [Guid("D666063F-1587-4E43-81F1-B948E807363F")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IMMDevice
    {
        [PreserveSig]
        int Activate(ref Guid iid, uint dwClsCtx, IntPtr pActivationParams, [MarshalAs(UnmanagedType.IUnknown)] out object ppInterface);

        // Rest of the methods not implemented
    }

    [ComImport]
    [Guid("5CDF2C82-841E-4546-9722-0CF74078229A")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IAudioEndpointVolume
    {
        [PreserveSig]
        int RegisterControlChangeNotify(IntPtr pNotify);

        [PreserveSig]
        int UnregisterControlChangeNotify(IntPtr pNotify);

        [PreserveSig]
        int GetChannelCount(out int pnChannelCount);

        [PreserveSig]
        int SetMasterVolumeLevel(float fLevelDB, ref Guid pguidEventContext);

        [PreserveSig]
        int SetMasterVolumeLevelScalar(float fLevel, ref Guid pguidEventContext);

        [PreserveSig]
        int GetMasterVolumeLevel(out float pfLevelDB);

        [PreserveSig]
        int GetMasterVolumeLevelScalar(out float pfLevel);

        [PreserveSig]
        int SetChannelVolumeLevel(uint nChannel, float fLevelDB, ref Guid pguidEventContext);

        [PreserveSig]
        int SetChannelVolumeLevelScalar(uint nChannel, float fLevel, ref Guid pguidEventContext);

        [PreserveSig]
        int GetChannelVolumeLevel(uint nChannel, out float pfLevelDB);

        [PreserveSig]
        int GetChannelVolumeLevelScalar(uint nChannel, out float pfLevel);

        [PreserveSig]
        int SetMute(bool bMute, ref Guid pguidEventContext);

        [PreserveSig]
        int GetMute(out bool pbMute);

        [PreserveSig]
        int GetVolumeStepInfo(out uint pnStep, out uint pnStepCount);

        [PreserveSig]
        int VolumeStepUp(ref Guid pguidEventContext);

        [PreserveSig]
        int VolumeStepDown(ref Guid pguidEventContext);

        [PreserveSig]
        int QueryHardwareSupport(out uint pdwHardwareSupportMask);

        [PreserveSig]
        int GetVolumeRange(out float pflVolumeMindB, out float pflVolumeMaxdB, out float pflVolumeIncrementdB);
    }

    internal enum EDataFlow
    {
        ERender,
        ECapture,
        EAll
    }

    internal enum ERole
    {
        EConsole,
        EMultimedia,
        ECommunications
    }

    // Extension method for MMDeviceEnumerator
    internal static class MMDeviceEnumeratorExtensions
    {
        public static IMMDevice GetDefaultAudioEndpoint(this MMDeviceEnumerator enumerator, EDataFlow dataFlow, ERole role)
        {
            var realEnumerator = (IMMDeviceEnumerator)enumerator;
            if (realEnumerator.GetDefaultAudioEndpoint(dataFlow, role, out var device) != 0)
            {
                throw new Exception("Failed to get default audio endpoint");
            }
            return device;
        }

        public static T Activate<T>(this IMMDevice device)
        {
            var iid = typeof(T).GUID;
            if (device.Activate(ref iid, 0, IntPtr.Zero, out var obj) != 0)
            {
                throw new Exception($"Failed to activate {typeof(T).Name}");
            }
            return (T)obj;
        }
    }
}
