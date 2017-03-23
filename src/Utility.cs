using System.IO;
using System.Linq;

namespace TioTests
{
    public static class Utility
    {
        public static string GetAncestor(string path, int depth)
        {
            string result = path;
            for(int i=0;i<depth;i++)
            {
                result = Path.GetDirectoryName(result);
            }
            return result;
        }
        public static void ClearLine(string testName)
        {
            Logger.Log($"\r{string.Empty.PadLeft(testName.Length + 40)}\r", true);
        }

        public static string TrimWhitespaces(string s)
        {
            return s?.Trim("\n\r\t ".ToCharArray());
        }

        public static void Dump(string comment, byte[] data, string fileName)
        {
            using (FileStream fs = File.Open(fileName, FileMode.Append, FileAccess.Write, FileShare.Read))
            using (StreamWriter sw = new StreamWriter(fs))
            {
                sw.WriteLine(comment);
                sw.WriteLine(Hex.Dump(data));
                sw.WriteLine();
                sw.Flush();
            }
        }

        public static int SearchBytes(byte[] haystack, byte[] needle)
        {
            var len = needle.Length;
            var limit = haystack.Length - len;
            for (var i = 0; i <= limit; i++)
            {
                var k = 0;
                for (; k < len; k++)
                {
                    if (needle[k] != haystack[i + k]) break;
                }
                if (k == len) return i;
            }
            return -1;
        }

        public static string GetProcess(Config config)
        {
            if (config.LocalProcess != null)
            {
                return config.LocalProcess;
            }
            return Path.Combine(config.LocalRoot, "tio.run/cgi-bin/run");
        }

        public static string GetArenaHost(Config config)
        {
            if (config.ArenaHost != null)
            {
                return config.ArenaHost;
            }
            return Path.GetFileName(Directory.GetFiles(Path.Combine(config.LocalRoot, "etc/run.d")).OrderBy(x=>x).FirstOrDefault());
        }
    }
}
