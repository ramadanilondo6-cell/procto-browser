using System.Collections.Generic;

namespace ProctoLite
{
    public class ExamConfig
    {
        public string StartUrl { get; set; } = "https://www.google.com";
        public List<string> AllowedUrls { get; set; } = new List<string> { "*" };
        public List<string> ForbiddenProcesses { get; set; } = new List<string>();
        public bool AllowClipboard { get; set; } = false;
        public bool AllowPrint { get; set; } = false;
        public string QuitPassword { get; set; } = "admin";
        public string BrowserTitle { get; set; } = "Procto";
        public string LogLevel { get; set; } = "Information";
    }
}
