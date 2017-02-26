using System;
using System.Collections.Generic;

namespace TioTests
{
    public class DisplayTestResultParams
    {
        public string TestName { set; get; } 
        public string ExpectedOutput { set; get; }
        public string Output { set; get; }
        public string Debug { set; get; }

        public string Result => Success ? "PASS" : "FAIL";
        public ConsoleColor Color => Success ? ConsoleColor.Green : ConsoleColor.Red;
        public bool Success => ExpectedOutput == Output;

        // These three are only relevant when it's remote run:
        // We cannot time individual tests on local run
        // We cannot display progress, so we are not using Counter on local run
        // Finally, Warnings on local run apply to the whole run not to individual test
        public string Time { set; get; }
        public string Counter { set; get; }
        public IList<string> Warnings { set; get; }

    }
}
