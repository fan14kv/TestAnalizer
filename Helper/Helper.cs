namespace TestAnalyzer.Helper
{
    public class Helper
    {
        public Helper()
        {
            throw new InvalidOperationException("Constructor Error");
        }

        // CA1304: Non-localized string
        public void ShowMessage()
        {
            Console.WriteLine("Hardcoded message");
        }
    }
}
