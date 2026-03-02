using System.CommandLine;

namespace WebsiteCertificateChecker;

public class AppConfig
{
    public List<string> Urls { get; private set; } = [];
    public int? ShowRemainingDays { get; private set; }
    public bool ShowElapsedTime { get; private set; }

    public AppConfig(string[] args)
    {
        if (args.Length == 0) ExitWithMessage("Nothing to do. Try to use -h or --help for help.");

        bool exit = true;

        var urlsArgument = new Argument<string[]>("urls")
        {
            DefaultValueFactory = parseResult => [],
        };
        var fileOption = new Option<FileInfo>("--file", "-f")
        {
            Description = "Get urls from file",
        };
        var elapsedTimeOption = new Option<bool>("--elapsed-time", "-t")
        {
            Description = "Show elapsed time",
        };
        var remainingDaysOption = new Option<int?>("--remaining-days", "-d")
        {
            Description = "Show only certificates that expire in _ days",
        };

        var rootCommand = new RootCommand();
        rootCommand.Add(urlsArgument);
        rootCommand.Add(fileOption);
        rootCommand.Add(elapsedTimeOption);
        rootCommand.Add(remainingDaysOption);

        rootCommand.SetAction(parseResult =>
        {
            exit = false;

            ShowElapsedTime = parseResult.GetValue(elapsedTimeOption);
            ShowRemainingDays = parseResult.GetValue(remainingDaysOption);

            var file = parseResult.GetValue(fileOption);
            var urls = parseResult.GetRequiredValue(urlsArgument);

            Urls = file != null ? UrlHelper.GetUrlsFromFile(file.FullName) : GetUrlsFromArgs(urls);

            if (Urls.Count == 0) ExitWithMessage("No valid URL found.");
        });

        var parseResult = rootCommand.Parse(args);
        var exitCode = parseResult.Invoke();

        if (exit)
        {
            Environment.Exit(exitCode);
        }
    }

    private void ExitWithMessage(string message)
    {
        Console.WriteLine(message);
        Environment.Exit(1);
    }

    private List<string> GetUrlsFromArgs(string[] args)
    {
        return args.Select(UrlHelper.GetValidUrl).ToList();
    }
}