using System.Collections.Concurrent;
using System.Diagnostics;
using WebsiteCertificateChecker;

Console.Title = "Website Certificate Checker";

var config = new AppConfig(args);

var certificateInfos = new ConcurrentBag<CertificateInfo>();

var stopwatch = Stopwatch.StartNew();
Parallel.ForEach(config.Urls, new ParallelOptions { MaxDegreeOfParallelism = 20 }, url =>
{
    certificateInfos.Add(CertificateChecker.GetCertificateInfo(url));
});
stopwatch.Stop();

var finalCertificateInfos = certificateInfos.ToList();

if (config.ShowRemainingDays.HasValue)
{
    finalCertificateInfos =
    [
        .. finalCertificateInfos
            .Where(c => c.ExpirationDate.GetValueOrDefault().Date <
                        DateTime.Now.AddDays(config.ShowRemainingDays.Value))
    ];
}

var table = new Table();

foreach (var certificateInfo in finalCertificateInfos.OrderByDescending(o => o.ExpirationDate).ToList())
{
    table.AddRow(certificateInfo.ToRow());
}

table.Write();

if (config.ShowElapsedTime)
{
    Console.WriteLine();
    Console.WriteLine($"Time elapsed: {stopwatch.Elapsed.TotalSeconds:0}s {stopwatch.Elapsed.Milliseconds}ms");
}

