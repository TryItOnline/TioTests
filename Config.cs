namespace TioTests
{
    public class Config
    {
        public string RunUrl { get; set; }
        public string Test { get; set; }
        public bool TrimWhitespacesFromResults { get; set; }
        public bool DisplayDebugInfoOnError { get; set; }
        public bool DisplayDebugInfoOnSuccess { get; set; }
        public bool UseConsoleCodes { get; set; }
        public string CheckUrl { get; set; }
    }
}
