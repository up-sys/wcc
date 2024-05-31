namespace WebsiteCertificateChecker;

public class AppConfig
{
    public List<string> Urls { get; }
    public int? ShowRemainingDays { get; private set; }
    public bool ShowElapsedTime { get; private set; }

    public AppConfig(string[] args)
    {
        if (args.Length == 0) ExitWithMessage("Nothing to do. Try to use -h or --help for help.");

        if (args.Contains("-h") || args.Contains("--help")) ShowHelpAndExit();

        ShowElapsedTime = args.Contains("-t") || args.Contains("--elapsed-time");

        Urls = args.Contains("-f") || args.Contains("--file") ? UrlHelper.GetUrlsFromFile(args[^1]) : GetUrlsFromArgs(args);

        if (Urls.Count == 0) ExitWithMessage("No valid URL found.");

        ShowRemainingDays = args.Contains("-14") ? 14 : args.Contains("-30") ? 30 : null;
    }

    private void ExitWithMessage(string message)
    {
        Console.WriteLine(message);
        Environment.Exit(1);
    }

    private void ShowHelpAndExit()
    {
        Console.WriteLine("Usage: wcc [options] <url> [url] [url] ...");
        Console.WriteLine("OPTIONS:");
        Console.WriteLine("-h, --help \t\t Show this help screen");
        Console.WriteLine("-f, --file <path> \t Get urls from file");
        Console.WriteLine("-t, --elapsed-time \t Show elapsed time");
        Console.WriteLine("-14 \t\t\t Show only certificates that expire in 14 days");
        Console.WriteLine("-30 \t\t\t Show only certificates that expire in 30 days");
        Environment.Exit(0);
    }

    private List<string> GetUrlsFromArgs(string[] args)
    {
        return args.Where(arg => !arg.StartsWith('-')).Select(UrlHelper.GetValidUrl).ToList();
    }
}