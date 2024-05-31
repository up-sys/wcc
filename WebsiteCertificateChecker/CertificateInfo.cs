namespace WebsiteCertificateChecker
{
    public class CertificateInfo
    {
        public string Url { get; init; } = default!;
        public DateTime? ExpirationDate { get; init; }

        private string ExpirationDateFormatted => ExpirationDate?.ToString("yyyy-MM-dd") ?? "N/A";
        private int DaysRemainingToExpire => (ExpirationDate.GetValueOrDefault() - DateTime.Now).Days;
        private ConsoleColor TextColor => DaysRemainingToExpire switch
        {
            < 14 => ConsoleColor.Red,
            < 30 => ConsoleColor.Yellow,
            _ => ConsoleColor.Green
        };

        public void ShowCertificateInfo(int maxUrlLength)
        {
            Console.Write($"{Url.PadRight(maxUrlLength)}  Expiration date: ");
            SetAndResetConsoleColor(TextColor, $"{ExpirationDateFormatted}");

            if (ExpirationDate != null)
            {
                Console.Write(" Remaining days: ");
                SetAndResetConsoleColor(TextColor, $"{DaysRemainingToExpire}");
            }

            Console.WriteLine();
        }

        private void SetAndResetConsoleColor(ConsoleColor color, string text)
        {
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ResetColor();
        }
    }
}