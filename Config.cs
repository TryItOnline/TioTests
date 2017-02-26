namespace TioTests
{
    public class Config
    {
        private bool _batchMode;
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

        public bool BatchMode
        {
            get { return _batchMode && LocalRun; }
            set { _batchMode = value; }
        }

        public bool Quiet { get; set; }
        public bool DebugDump { get; set; }
    }
}
