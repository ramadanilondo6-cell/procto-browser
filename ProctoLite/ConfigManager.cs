using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Serilog;

namespace ProctoLite
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

        private static string DecryptAes(string cipherText, string passPhrase)
        {
            var fullCipher = Convert.FromBase64String(cipherText);
            var iv = new byte[16];
            var cipher = new byte[fullCipher.Length - 16];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipher, 0, cipher.Length);

            using (var sha256 = SHA256.Create())
            {
                var key = sha256.ComputeHash(Encoding.UTF8.GetBytes(passPhrase));
                using (var aes = Aes.Create())
                {
                    using (var decryptor = aes.CreateDecryptor(key, iv))
                    using (var ms = new MemoryStream(cipher))
                    using (var cs = new System.Security.Cryptography.CryptoStream(ms, decryptor, System.Security.Cryptography.CryptoStreamMode.Read))
                    using (var sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
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

                var rawText = File.ReadAllText(_configPath);
                string json = rawText;

                // Simple check if it's an AES encrypted config payload
                if (rawText.StartsWith("AES:"))
                {
                    try
                    {
                        string cipherText = rawText.Substring(4);
                        // Using a hardcoded internal key for simple extraction protection
                        // (Can be improved with per-school keys in the future)
                        json = DecryptAes(cipherText, "ProctoSecureKey2025!");
                        Log.Information("Successfully decrypted configuration file");
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Failed to decrypt configuration file. Ensure it is a valid Procto config.");
                        _config = new ExamConfig();
                        return false;
                    }
                }

                _config = JsonSerializer.Deserialize<ExamConfig>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
