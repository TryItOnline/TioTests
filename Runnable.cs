using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using ConsoleApplication;

namespace TioTests
{
    public class Runnable
    {
        public string Language {get; set;}
        public string Header {get; set;}
        public string Footer {get; set;}
        public string Code {get; set;}
        public string Input {get; set;}
        public List<string> CommandLineArguments {get; set;}

        public byte[] GetBytes()
        {
            CommandLineArguments = CommandLineArguments ?? new List<string>();
            Language = Language ?? "";
            Header = Header ?? "";
            Footer = Footer ?? "";
            Code = Code ?? "";
            Input = Input ?? "";
            using(MemoryStream ms = new MemoryStream())
            {
                ms.Write(BitConverter.GetBytes(0).Reverse().ToArray());
                ms.Write(BitConverter.GetBytes(0).Reverse().ToArray());
                ms.Write(BitConverter.GetBytes(1).Reverse().ToArray());
                ms.Write(Encoding.UTF8.GetBytes(Language));
                ms.WriteByte(0);
                ms.Write(BitConverter.GetBytes(Encoding.UTF8.GetBytes(Header).Length).Reverse().ToArray());
                ms.Write(Encoding.UTF8.GetBytes(Header));
                ms.Write(BitConverter.GetBytes(Encoding.UTF8.GetBytes(Code).Length).Reverse().ToArray());
                ms.Write(Encoding.UTF8.GetBytes(Code));
                ms.Write(BitConverter.GetBytes(Encoding.UTF8.GetBytes(Footer).Length).Reverse().ToArray());
                ms.Write(Encoding.UTF8.GetBytes(Footer));
                ms.Write(BitConverter.GetBytes(Encoding.UTF8.GetBytes(Input).Length).Reverse().ToArray());
                ms.Write(Encoding.UTF8.GetBytes(Input));
                ms.Write(BitConverter.GetBytes(CommandLineArguments.Count).Reverse().ToArray());
                foreach(string arg in CommandLineArguments)
                {
                    ms.Write(Encoding.UTF8.GetBytes(arg).Where(x => x!=0).ToArray());
                    ms.WriteByte(0);
                }
                byte[] toCompress = ms.ToArray();
                using(MemoryStream compressed = new MemoryStream())
                using(GZipStream compressor = new GZipStream(compressed, CompressionLevel.Optimal, false))
                {
                    compressor.Write(toCompress, 0, toCompress.Length);
                    compressor.Flush();
                    return compressed.ToArray().Skip(10).ToArray();                    
                }
            }
        }
    }
}