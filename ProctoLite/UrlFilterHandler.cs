using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using CefSharp;
using Serilog;

namespace ProctoLite
{
    public class UrlFilterHandler : IRequestHandler
    {
        private readonly List<string> _allowedUrls;
        private readonly string _configKeyHash;

        public UrlFilterHandler(List<string> allowedUrls, string configKeyHash)
        {
            _allowedUrls = allowedUrls ?? new List<string>();
            _configKeyHash = configKeyHash;

            Log.Information("UrlFilterHandler initialized with {Count} allowed URL patterns", _allowedUrls.Count);
        }

        public bool OnBeforeBrowse(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool userGesture, bool isRedirect)
        {
            var url = request.Url;
            Log.Debug("Navigation attempt to: {Url}", url);

            // Allow about:blank and chrome-extension URLs
            if (url.StartsWith("about:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("chrome-extension:", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("devtools:", StringComparison.OrdinalIgnoreCase))
            {
                return false; // Allow navigation
            }

            // Check if URL is allowed
            if (!IsUrlAllowed(url))
            {
                Log.Warning("Blocked navigation to non-whitelisted URL: {Url}", url);

                // Redirect to blocked page - use chromiumWebBrowser instead of frame
                chromiumWebBrowser.LoadHtml(GetBlockedPageHtml(url), url);
                return true; // Cancel navigation
            }

            Log.Information("Navigation allowed to: {Url}", url);
            return false; // Allow navigation
        }

        private bool IsUrlAllowed(string url)
        {
            // If wildcard is present, allow all
            if (_allowedUrls.Contains("*"))
            {
                return true;
            }

            var uri = new Uri(url);
            var host = uri.Host;

            foreach (var allowedPattern in _allowedUrls)
            {
                if (string.IsNullOrWhiteSpace(allowedPattern))
                    continue;

                // Exact match
                if (allowedPattern.Equals(url, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // Wildcard subdomain match (*.example.com)
                if (allowedPattern.StartsWith("*."))
                {
                    var domain = allowedPattern.Substring(2); // Remove "*."
                    if (host.Equals(domain, StringComparison.OrdinalIgnoreCase) ||
                        host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                // Domain match without wildcard
                else if (host.Equals(allowedPattern, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private string GetBlockedPageHtml(string blockedUrl)
        {
            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='UTF-8'>
    <title>Akses Ditolak</title>
    <style>
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background: #f0f0f4;
            display: flex;
            justify-content: center;
            align-items: center;
            height: 100vh;
            margin: 0;
        }}
        .container {{
            background: white;
            padding: 40px;
            border-radius: 8px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.1);
            max-width: 500px;
            text-align: center;
        }}
        h1 {{
            color: #e03e3e;
            margin-bottom: 16px;
        }}
        p {{
            color: #666;
            line-height: 1.6;
        }}
        .url {{
            background: #f5f5f5;
            padding: 8px 12px;
            border-radius: 4px;
            font-family: monospace;
            font-size: 12px;
            color: #333;
            word-break: break-all;
            margin: 16px 0;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <h1>â›” Akses Ditolak</h1>
        <p>URL yang Anda coba akses tidak termasuk dalam daftar putih ujian.</p>
        <div class='url'>{blockedUrl}</div>
        <p>Silakan kembali ke halaman ujian yang telah ditentukan.</p>
    </div>
</body>
</html>";
        }

        #region IRequestHandler Implementation

        public bool GetAuthCredentials(IWebBrowser chromiumWebBrowser, IBrowser browser, string originUrl, bool isProxy, string host, int port, string realm, string scheme, IAuthCallback callback)
        {
            return false;
        }

        public bool OnOpenUrlFromTab(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, WindowOpenDisposition targetDisposition, bool userGesture)
        {
            return false;
        }

        public IResourceRequestHandler GetResourceRequestHandler(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, IRequest request, bool isNavigation, bool isDownload, string requestInitiator, ref bool disableDefaultHandling)
        {
            // Add Config Key Header for SEB compatibility via ResourceRequestHandler
            if (!string.IsNullOrEmpty(_configKeyHash))
            {
                return new ResourceRequestHandler(_configKeyHash);
            }

            return null;
        }

        public bool OnCertificateError(IWebBrowser chromiumWebBrowser, IBrowser browser, CefErrorCode errorCode, string requestUrl, ISslInfo sslInfo, IRequestCallback callback)
        {
            // Continue on certificate error (for self-signed certs in exam environments)
            callback.Continue(true);
            return true;
        }

        public bool OnSelectClientCertificate(IWebBrowser chromiumWebBrowser, IBrowser browser, bool isProxy, string host, int port, X509Certificate2Collection certificates, ISelectClientCertificateCallback callback)
        {
            // No client certificate selection
            return false;
        }

        public void OnRenderViewReady(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            // Called when render view is ready
        }

        public void OnRenderProcessTerminated(IWebBrowser chromiumWebBrowser, IBrowser browser, CefTerminationStatus status)
        {
            Log.Error("Render process terminated: {Status}", status);
        }

        public void OnDocumentAvailableInMainFrame(IWebBrowser chromiumWebBrowser, IBrowser browser)
        {
            // Called when document is available in main frame
        }

        #endregion
    }
}
