using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProctoConfigEditor
{
    public class ExamConfig
    {
        [JsonPropertyName("startUrl")]
        public string StartUrl { get; set; } = "https://www.google.com";

        [JsonPropertyName("browserTitle")]
        public string BrowserTitle { get; set; } = "Procto";

        [JsonPropertyName("quitPassword")]
        public string QuitPassword { get; set; } = "admin";

        [JsonPropertyName("allowClipboard")]
        public bool AllowClipboard { get; set; } = false;

        [JsonPropertyName("allowPrint")]
        public bool AllowPrint { get; set; } = false;

        [JsonPropertyName("allowedUrls")]
        public List<string> AllowedUrls { get; set; } = new List<string>();

        [JsonPropertyName("forbiddenProcesses")]
        public List<string> ForbiddenProcesses { get; set; } = new List<string> { "chrome.exe", "msedge.exe", "firefox.exe" };
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=======================================");
            Console.WriteLine(" Procto Configuration Editor (v1.0) ");
            Console.WriteLine("=======================================\n");

            var config = new ExamConfig();

            config.StartUrl = Prompt("Start URL", config.StartUrl);
            config.BrowserTitle = Prompt("Browser Title", config.BrowserTitle);
            config.QuitPassword = Prompt("Quit Password", config.QuitPassword);
            config.AllowClipboard = PromptBool("Allow Clipboard?", config.AllowClipboard);
            config.AllowPrint = PromptBool("Allow Print?", config.AllowPrint);

            Console.WriteLine("\nAllowed URLs (comma separated, leave empty for none):");
            string urls = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(urls))
            {
                config.AllowedUrls = urls.Split(',').Select(u => u.Trim()).ToList();
            }

            Console.WriteLine($"\nForbidden Processes (comma separated, current: {string.Join(", ", config.ForbiddenProcesses)}):");
            string procs = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(procs))
            {
                config.ForbiddenProcesses = procs.Split(',').Select(p => p.Trim()).ToList();
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonOutput = JsonSerializer.Serialize(config, options);

            Console.WriteLine("\n=======================================");
            Console.WriteLine(jsonOutput);
            Console.WriteLine("=======================================\n");

            bool encrypt = PromptBool("Encrypt output with AES-256?", true);

            string finalOutput = jsonOutput;
            if (encrypt)
            {
                finalOutput = "AES:" + EncryptAes(jsonOutput, "ProctoSecureKey2025!");
            }

            string outputPath = "default.procto.json";
            File.WriteAllText(outputPath, finalOutput);

            Console.WriteLine($"\n[SUCCESS] Configuration saved to {outputPath}");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        static string Prompt(string message, string defaultValue)
        {
            Console.Write($"{message} [{defaultValue}]: ");
            string input = Console.ReadLine();
            return string.IsNullOrWhiteSpace(input) ? defaultValue : input;
        }

        static bool PromptBool(string message, bool defaultValue)
        {
            string defStr = defaultValue ? "Y/n" : "y/N";
            Console.Write($"{message} [{defStr}]: ");
            string input = Console.ReadLine()?.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(input)) return defaultValue;
            return input == "y" || input == "yes" || input == "true" || input == "1";
        }

        static string EncryptAes(string plainText, string passPhrase)
        {
            using (var sha256 = SHA256.Create())
            {
                var key = sha256.ComputeHash(Encoding.UTF8.GetBytes(passPhrase));
                using (var aes = Aes.Create())
                {
                    aes.GenerateIV();
                    var iv = aes.IV;

                    using (var encryptor = aes.CreateEncryptor(key, iv))
                    using (var ms = new MemoryStream())
                    {
                        using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (var sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }

                        var encrypted = ms.ToArray();
                        var result = new byte[iv.Length + encrypted.Length];
                        Buffer.BlockCopy(iv, 0, result, 0, iv.Length);
                        Buffer.BlockCopy(encrypted, 0, result, iv.Length, encrypted.Length);

                        return Convert.ToBase64String(result);
                    }
                }
            }
        }
    }
}