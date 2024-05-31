namespace WebsiteCertificateChecker
{
    public static class UrlHelper
    {
        public static List<string> GetUrlsFromFile(string path)
        {
            var urls = new List<string>();

            if (!File.Exists(path))
            {
                ExitWithMessage($"File {path} not found.");
            }

            using var reader = new StreamReader(path);
            while (reader.ReadLine() is { } line)
            {
                if (line == string.Empty) continue;
                urls.Add(GetValidUrl(line.ToLower()));
            }

            return urls;
        }

        public static string GetValidUrl(string url)
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out _))
            {
                return url.StartsWith("http://") ? url.Replace("http://", "https://") : url;
            }

            if (url.Contains('.'))
            {
                return url.Insert(0, "https://");
            }
            ExitWithMessage($"Invalid URL: {url}");
            return url;
        }

        private static void ExitWithMessage(string message)
        {
            Console.WriteLine(message);
            Environment.Exit(1);
        }
    }
}