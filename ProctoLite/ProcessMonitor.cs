using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using Serilog;

namespace ProctoLite
{
    public class ProcessMonitor
    {
        private readonly List<string> _forbiddenProcesses;
        private readonly Timer _monitorTimer;
        private readonly int _violationThreshold;
        private int _violationCount;

        public Action<string> OnViolationDetected;
        public Action OnThresholdReached;

        public ProcessMonitor(List<string> forbiddenProcesses, int checkIntervalSeconds = 2, int violationThreshold = 3)
        {
            _forbiddenProcesses = forbiddenProcesses ?? new List<string>();
            _violationThreshold = violationThreshold;
            _violationCount = 0;

            _monitorTimer = new Timer(checkIntervalSeconds * 1000);
            _monitorTimer.Elapsed += MonitorTimer_Elapsed;

            Log.Information("ProcessMonitor initialized with {Count} forbidden processes", _forbiddenProcesses.Count);
        }

        public void Start()
        {
            if (_forbiddenProcesses.Count == 0)
            {
                Log.Warning("ProcessMonitor started with empty forbidden processes list");
                return;
            }

            _monitorTimer.Start();
            Log.Information("ProcessMonitor started - checking every {_Interval} seconds", _monitorTimer.Interval / 1000);
        }

        public void Stop()
        {
            _monitorTimer.Stop();
            Log.Information("ProcessMonitor stopped");
        }

        private void MonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            CheckForbiddenProcesses();
        }

        private void CheckForbiddenProcesses()
        {
            try
            {
                var runningProcesses = Process.GetProcesses();

                foreach (var process in runningProcesses)
                {
                    try
                    {
                        var processName = process.ProcessName;
                        var processFileName = process.MainModule?.FileName;

                        // Check by process name (case-insensitive)
                        var isForbidden = _forbiddenProcesses.Any(forbidden =>
                            processName.Equals(forbidden, StringComparison.OrdinalIgnoreCase) ||
                            processName.Equals(System.IO.Path.GetFileNameWithoutExtension(forbidden), StringComparison.OrdinalIgnoreCase));

                        if (isForbidden)
                        {
                            _violationCount++;
                            var message = $"Proses terlarang terdeteksi: {processName}.exe (Kejadian: {_violationCount})";

                            Log.Warning("Forbidden process detected: {ProcessName} (Violation #{Count})", processName, _violationCount);

                            OnViolationDetected?.Invoke(message);

                            if (_violationCount >= _violationThreshold)
                            {
                                Log.Error("Violation threshold reached ({Count}). Triggering threshold handler.", _violationCount);
                                OnThresholdReached?.Invoke();
                            }

                            break; // Only report one violation per check
                        }
                    }
                    catch (Exception ex)
                    {
                        // Some processes may not be accessible
                        Log.Debug(ex, "Could not inspect process");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while checking forbidden processes");
            }
        }

        public void ResetViolationCount()
        {
            _violationCount = 0;
            Log.Information("Violation count reset to 0");
        }
    }
}
