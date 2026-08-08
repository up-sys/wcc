namespace WebsiteCertificateChecker
{
    public class CertificateInfo
    {
        public string Url { get; init; } = null!;
        public string? Issuer { get; init; }
        public DateTime? ExpirationDate { get; init; }

        private string ExpirationDateFormatted => ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A";
        private int DaysRemainingToExpire => (ExpirationDate.GetValueOrDefault() - DateTime.Now).Days;
        private ConsoleColor TextColor => DaysRemainingToExpire switch
        {
            < 14 => ConsoleColor.Red,
            < 30 => ConsoleColor.Yellow,
            _ => ConsoleColor.Green
        };

        public Row ToRow()
        {
            var row = new Row();

            row.AddCell(Url);
            row.AddCell("Expiration date: " + new Text($"{ExpirationDateFormatted}", TextColor));

            if (ExpirationDate != null)
            {
                row.AddCell("Remaining days: " + new Text($"{DaysRemainingToExpire}", TextColor));
            }
            else
            {
                row.AddEmpty();
            }

            if (Issuer != null)
            {
                row.AddCell($"Issuer: {Issuer}");
            }
            else
            {
                row.AddEmpty();
            }

            return row;
        }
    }
}