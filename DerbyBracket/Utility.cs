using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Controls;

namespace DerbyBracket
{
    public static class Utility
    {
        private const string VersionUrl = "https://raw.githubusercontent.com/aeshirey/DerbyBracket/master/Version.txt";
        public const string DownloadUrl = "https://raw.githubusercontent.com/aeshirey/DerbyBracket/master/DerbyBracket.zip";

        public static IEnumerable<T> AllItems<T>(this ItemsControl itemControl)
        {
            var items = itemControl.Items;

            for (var i = 0; i < items.Count; i++)
            {
                yield return (T)items[i];
            }
        }

        public static void ForEach<T>(this IEnumerable<T> inputs, Action<T> action)
        {
            foreach (T input in inputs)
            {
                action(input);
            }
        }

        /// <summary>
        /// Perform an HTTP GET action
        /// </summary>
        /// <param name="url">The valid URL to hit</param>
        /// <returns>The content of the response</returns>
        public static string HttpGet(string url)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.AutomaticDecompression = DecompressionMethods.GZip;

            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                var content = reader.ReadToEnd();
                return content;
            }
        }

        public static Version NewestVersion()
        {
            try
            {
                var versionString = HttpGet(VersionUrl);
                var version = new Version(versionString);
                return version;
            }
            catch
            {
                return null;
            }
        }

        public static bool NewVersionExists()
        {
            var thisVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

            var newestVersion = NewestVersion();

            if (newestVersion == null)
                return false;

            var diff = newestVersion.CompareTo(thisVersion);

            return diff > 0;
        }

        public static int CompareTo(this Version left, Version right)
        {
            if (left == right)
            {
                return 0;
            }

            //major,minor,build,revision
            if (left.Major != right.Major) return left.Major < right.Major ? -1 : 1;
            if (left.Minor != right.Minor) return left.Minor < right.Minor ? -1 : 1;
            if (left.Build != right.Build) return left.Build < right.Build ? -1 : 1;
            return left.Revision < right.Revision ? -1 : 1;
        }
    }
}
