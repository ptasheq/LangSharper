using System;
using System.Collections.Generic;
using System.IO;
using LangSharper.Views;

namespace LangSharper
{
    public static class Globals
    {
        public const string Path = "../../";
        public const string UiTextFileName = "uitext.ls";
        public const string DatabaseFileName = "database.sqlite";
        public const string AppName = "langsharper";
        public static readonly string ResourcePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), AppName);
        public const int MinWordsForLesson = 5;
        public const int MaxWordsForLesson = 30;
        public const int WordsToChooseCount = 4;
        public readonly static Random Random = new Random();

        public static void DeleteDirIfExists(string path, bool recursive)
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive);
            }
        }

        public static void Shuffle<T>(List<T> array)
        {
            int len = array.Count;
            for (int i = 0; i < len; ++i)
            {
                int randomIndex = i + (int) (Random.NextDouble() * (len - i));
                T tmp = array[i];
                array[i] = array[randomIndex];
                array[randomIndex] = tmp;
            }
        }

        public static void Swap<T>(IList<T> list, int a, int b)
        {
            T tmp = list[a];
            list[a] = list[b];
            list[b] = tmp;
        }
    }
}
