using System.Collections.Generic;
using System.IO;

namespace LangSharper
{
    public class UiTexts
    {
        public UiTexts(string path)
        {
            Dict = new Dictionary<string, string>();
            StreamReader streamReader = new StreamReader(path);
            string [] buf;
            while (!streamReader.EndOfStream)
            {
                buf = streamReader.ReadLine().Split(':');
                if (buf.Length == 2 && !Dict.ContainsKey(buf[0]) && buf[0] != "" && buf[1] != "")
                {
                    Dict.Add(buf[0], buf[1]);     
                }

            }
            streamReader.Close();
        }

        public int GetLength()
        {
            return Dict.Count;
        }

        public Dictionary<string, string> Dict { get; private set; }

        public string GetText(string key)
        {
            return Dict[key];
        }

    }
}
