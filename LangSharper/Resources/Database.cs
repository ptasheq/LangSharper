using System;
using System.IO;
using SQLite.Net;
using SQLite.Net.Attributes;
using SQLite.Net.Platform.Win32;

namespace LangSharper
{
    public sealed class Database
    {
        private static string _appName;
        public static string FileName { get; private set; }
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
                        throw new ArgumentException("ExTooShortUserName");
                    _name = value;
                }
            }
        }

        public class Lesson
        {
            int? _userId = null;

            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Indexed(Name = "LessonIndex", Order = 1, Unique = true), MaxLength(40)]
            public string Name { get; set; }

            [Indexed(Name = "LessonIndex", Order = 1, Unique = true), NotNull]
            public int? UserId
            {
                get { return _userId; }
                set { _userId = value; }
            }
            
            [Ignore]
            public int WordCount
            {
                get
                {
                    using (var db = new SQLiteConnection(new SQLitePlatformWin32(), FileName))
                    {
                        return db.Table<Word>().Count(w => w.LessonId == Id);
                    }
                }
            }
        }

        public class Word
        {
            int? _lessonId = null;
            bool _hasImage = false;
            string _definitionLang1 = null;
            string _definitionLang2 = null;
            short _level = 0;

            protected delegate void Del();

            protected Del DefLang1 = () => {};
            protected Del DefLang2 = () => {};
            protected Del HasImg = () => {};

            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Indexed(Name = "Definition", Order = 2, Unique = true), MaxLength(64), NotNull]
            public string DefinitionLang1
            {
                get { return _definitionLang1; } 
                set { _definitionLang1 = value; DefLang1(); }
            }

            [Indexed(Name = "Definition", Order = 3, Unique = true), MaxLength(64), NotNull]
            public string DefinitionLang2
            {
                get { return _definitionLang2; }
                set { _definitionLang2 = value; DefLang2(); }
            }

            [Indexed(Name = "Definition", Order = 1, Unique = true), NotNull]
            public int? LessonId { get { return _lessonId; } set { _lessonId = value; } }

            public String GetImagePath(User user, Lesson lesson)
            {
                if (!HasImage)
                {
                    return null;
                }

                if (lesson == null || lesson.Name == null || lesson.Name.Length < 1)
                {
                    throw new NullReferenceException("ExLessonNotSpecified");
                }

                if (user == null)
                {
                    throw new NullReferenceException("ExUserNotSpecified");
                }

                if (_definitionLang1 == null || _definitionLang2 == null)
                {
                    throw new NullReferenceException("ExWrongViewForAction");
                }
                
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), _appName, user.Name,
                    lesson.Name, _definitionLang1.Replace(" ", "_").RemoveAccent() + "_" + _definitionLang2.Replace(" ", "_").RemoveAccent() + ".png");
            }
            
            public short Level {
                get { return _level; }
                set
                {
                    if (value < 0 || value > 7)
                        throw new ArgumentException("ExWrongWordLevelValue");
                    _level = value;
                } 
            }

            public bool HasImage
            {
                get { return _hasImage; }
                set { _hasImage = value; HasImg(); }
            }
        }

        public Database(string appName, string fileName)
        {
            _appName = appName;
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
