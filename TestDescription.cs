using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ConsoleApplication;
using Newtonsoft.Json.Linq;

namespace TioTests
{

    public class Statement
    {
        public string Command { get; set; }
        public Dictionary<string, JToken> Payload;
    }

    public class TestDescription
    {
        public List<Statement> Input { get; set; }
        public string Output { get; set; }

        public byte[] GetInputBytes()
        {
            byte[] zero = {0};
            using (MemoryStream ms = new MemoryStream())
            {
                foreach (Statement s in Input)
                {
                    ms.Write(Encoding.UTF8.GetBytes(s.Command));
                    if (s.Payload != null)
                    {
                        foreach (KeyValuePair<string, JToken> pair in s.Payload)
                        {
                            ms.Write(Encoding.UTF8.GetBytes(pair.Key));
                            ms.Write(zero);
                            if (pair.Value.Type == JTokenType.Array)
                            {
                                List<string> values = pair.Value.ToObject<List<string>>();
                                ms.Write(Encoding.UTF8.GetBytes(values.Count.ToString()));
                                ms.Write(zero);
                                foreach (string value in values)
                                {
                                    ms.Write(Encoding.UTF8.GetBytes(value));
                                    ms.Write(BitConverter.GetBytes(0).Reverse().ToArray());
                                }
                            }
                            else if (pair.Value.Type == JTokenType.String)
                            {
                                string value = pair.Value.ToObject<string>();
                                byte[] data = Encoding.UTF8.GetBytes(value);
                                ms.Write(Encoding.UTF8.GetBytes(data.Length.ToString()));
                                ms.Write(zero);
                                ms.Write(data);
                            }
                            else
                            {
                                throw new Exception(
                                    $"Unexpected token type: {pair.Value.Type}, for {pair.Key} command: {s.Command}");
                            }
                        }
                    }
                }
                byte[] toCompress = ms.ToArray();
                using (MemoryStream compressed = new MemoryStream())
                using (GZipStream compressor = new GZipStream(compressed, CompressionLevel.Optimal, false))
                {
                    compressor.Write(toCompress, 0, toCompress.Length);
                    compressor.Flush();
                    return compressed.ToArray().Skip(10).ToArray();
                }
            }

        }
    }
}
