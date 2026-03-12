using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;

namespace Procto
{
    public class ConfigManager
    {
        private static readonly string ConfigFileName = "default.procto.json";
        private ExamConfig _config;
        private string _configPath;

        public ExamConfig Config => _config;
        public string ConfigKeyHash { get; private set; }

        public ConfigManager()
        {
            // Try multiple locations for config file
            var possiblePaths = new[]
            {
                // Current working directory
                Path.Combine(Directory.GetCurrentDirectory(), ConfigFileName),
                // Base directory (where exe is located)
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName),
                // Config folder in current working directory
                Path.Combine(Directory.GetCurrentDirectory(), "config", ConfigFileName),
                // Config folder in base directory
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", ConfigFileName)
            };

            foreach (var path in possiblePaths)
            {
                if (File.Exists(path))
                {
                    _configPath = path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(_configPath))
            {
                _configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config", ConfigFileName);
            }
        }

        public bool Load()
        {
            try
            {
                if (!File.Exists(_configPath))
                {
                    Log.Error("Configuration file not found: {ConfigPath}", _configPath);
                    _config = new ExamConfig();
                    return false;
                }

                var json = File.ReadAllText(_configPath);
                _config = JsonSerializer.Deserialize<ExamConfig>(json);

                if (_config == null)
                {
                    Log.Error("Failed to deserialize configuration file");
                    _config = new ExamConfig();
                    return false;
                }

                // Calculate SHA-256 hash of config file
                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
                    ConfigKeyHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                }

                Log.Information("Configuration loaded successfully from {ConfigPath}", _configPath);
                Log.Information("Config Key Hash: {Hash}", ConfigKeyHash);

                return Validate();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading configuration file");
                _config = new ExamConfig();
                return false;
            }
        }

        public bool Validate()
        {
            if (string.IsNullOrWhiteSpace(_config.StartUrl))
            {
                Log.Error("Configuration validation failed: StartUrl is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.QuitPassword))
            {
                Log.Error("Configuration validation failed: QuitPassword is required");
                return false;
            }

            if (string.IsNullOrWhiteSpace(_config.BrowserTitle))
            {
                _config.BrowserTitle = "Procto";
            }

            Log.Information("Configuration validation passed");
            return true;
        }
    }
}
