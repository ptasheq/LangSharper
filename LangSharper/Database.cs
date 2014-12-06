using System;
using System.Diagnostics;
using System.IO;
using System.Net.Mime;
using System.Windows;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.Win32;

namespace LangSharper
{
    public sealed class Database
    {
        public string FileName { get; private set; }
        const int UserNameMinLength = 3;

        public class User
        {
            string _name = null;            

            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Unique, MaxLength(20), NotNull]
            public string Name
            {
                get { return _name; }
                set
                {
                    if (value.Length < UserNameMinLength)
                        throw new ArgumentException(
                            (PropertyFinder.Instance.GetResource("UiTexts") as UiTexts).GetText("ExTooShortUserName"));
                    _name = value;
                }
            }
        }

        public class Lesson
        {

            int? _userId = null;

            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Unique, MaxLength(40)]
            public string Name { get; set; }

            [NotNull]
            public int? UserId
            {
                get { return _userId; }
                set { _userId = value; }
            }
        }

        public class Word
        {

            int? _lessonId = null;
            bool _hasImage = false;
            string _definitionLang1 = null;
            string _definitionLang2 = null;
            short _level = 0;

            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Indexed(Name = "Definition", Order = 1, Unique = true), MaxLength(64), NotNull]
            public string DefinitionLang1
            {
                get { return _definitionLang1; } 
                set { _definitionLang1 = value; }
            }

            [Indexed(Name = "Definition", Order = 2, Unique = true), MaxLength(64), NotNull]
            public string DefinitionLang2
            {
                get { return _definitionLang2; }
                set { _definitionLang2 = value; }
            }

            public short Level {
                get { return _level; }
                set
                {
                    if (value < 0 || value > 7)
                        throw new ArgumentException(
                            (PropertyFinder.Instance.GetResource("UiTexts") as UiTexts).GetText("ExWrongWordLevelValue"));
                    _level = value;
                } 
            }

            public bool HasImage
            {
                get { return _hasImage; }
                set { _hasImage = value; }
            }

            [NotNull]
            public int? LessonId { get { return _lessonId; } set { _lessonId = value; } }
        }

        public Database(string fileName)
        {
            FileName = fileName;
            if (File.Exists(fileName))
                return;

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), fileName))
            {
                db.CreateTable<User>();
                db.CreateTable<Lesson>();
                db.CreateTable<Word>();
            }
        }

    }
}
