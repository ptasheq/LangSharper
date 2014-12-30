using System;
using System.IO;

namespace LangSharper
{
    public static class Globals
    {
        public const string Path = "../../";
        public const string UiTextFileName = "uitext.ls";
        public const string DatabaseFileName = "database.sqlite";
        public const string AppName = "langsharper";
        public static readonly string ResourcePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName);
        public const int MaxWordsForLesson = 30;

        public static void DeleteDirIfExists(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }
    }
}
