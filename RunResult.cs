using System.Collections.Generic;

namespace TioTests
{
    public class RunResult
    {
        public List<string> Output { get; set; }
        public List<string> Debug { get; set; }
        public List<string> Warnings { get; set; }
        public bool HttpFailure { get; set; }
    }
}