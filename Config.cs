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
        public int Retries { get; set; }
        public bool LocalRun { get; set; }
        public string LocalRoot { get; set; }
        public string LocalProcess { get; set; }
        public string ArenaHost { get; set; }
        public bool BatchMode { get; set; }
        public bool BatchModeEffective => BatchMode && LocalRun;
        public bool Quiet { get; set; }
        public bool DebugDump { get; set; }
    }
}
