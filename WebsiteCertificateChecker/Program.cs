using System.Collections.Concurrent;
using WebsiteCertificateChecker;

Console.Title = "Website Certificate Checker";

var config = new AppConfig(args);

var certificateInfos = new ConcurrentBag<CertificateInfo>();

var startTime = DateTime.Now;
Parallel.ForEach(config.Urls, url =>
{
    certificateInfos.Add(new CertificateInfo
    {
        Url = url,
        ExpirationDate = CertificateChecker.GetCertificateExpirationDate(url)
    });
});
var endTime = DateTime.Now;

var finalCertificateInfos = certificateInfos.ToList();

if (config.ShowRemainingDays.HasValue)
{
    finalCertificateInfos = finalCertificateInfos
    .Where(c => c.ExpirationDate.GetValueOrDefault().Date < DateTime.Now.AddDays(config.ShowRemainingDays.Value))
    .ToList();
}

foreach (var certificateInfo in finalCertificateInfos.OrderByDescending(o => o.ExpirationDate).ToList())
{
    certificateInfo.ShowCertificateInfo(config.Urls.Max(u => u.Length));
}

if (config.ShowElapsedTime)
{
    Console.WriteLine($"Time elapsed: {(endTime - startTime).Seconds}s {(endTime - startTime).Milliseconds}ms");
}

