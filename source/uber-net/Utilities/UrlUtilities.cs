using System;

namespace uber_net.Utilities
{
    public static class UrlUtilities
    {
        public static string FormatUrl(string instanceUrl, string apiVersion, string resourceName)
        {
            if (string.IsNullOrEmpty(instanceUrl)) throw new ArgumentNullException("instanceUrl");
            if (string.IsNullOrEmpty(apiVersion)) throw new ArgumentNullException("apiVersion");
            if (string.IsNullOrEmpty(resourceName)) throw new ArgumentNullException("resourceName");

            return string.Format("{0}/{1}/{2}", instanceUrl, apiVersion, resourceName);
        }

        public static bool CheckUri(string url)
        {
            Uri tempValue;
            return Uri.TryCreate(url, UriKind.Absolute, out tempValue);
        }
    }
}
