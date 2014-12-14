using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LangSharper;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharperTests
{
    [TestClass]
    public class DatabaseTest
    {
        [TestMethod]
        public void ConstructorTest()
        {
            File.Delete(Globals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.Path + "testdatabase.sqlite");
            Assert.IsTrue(File.Exists(Globals.Path + "testdatabase.sqlite"));

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), d.FileName))
            {
                Assert.AreEqual(0, db.Table<Database.User>().Count()); 
                Assert.AreEqual(0, db.Table<Database.Lesson>().Count()); 
                Assert.AreEqual(0, db.Table<Database.Word>().Count());
            }
        }

        [TestMethod]
        public void UserInsertTest()
        {
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object>(){{"UiTexts", uiTexts}});
            File.Delete(Globals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), d.FileName))
            {
                db.Insert(new Database.User() {Name = "testuser"});

                try
                {
                    db.Insert(new Database.User() {Name = "testuser"});
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                    StringAssert.Contains(e.Message, "Constraint");
                }

                db.Insert(new Database.User() {Name = "testuser2"});

                try
                {
                    db.Insert(new Database.User());
                    Assert.Fail("Exception should appear");
                }
                catch (NotNullConstraintViolationException e)
                {
                }

                try
                {
                    db.Insert(new Database.User() {Name = ""});
                    Assert.Fail("Exception should appear");
                }
                catch (ArgumentException e)
                {
                    StringAssert.Contains(e.Message, uiTexts.GetText("ExTooShortUserName"));
                }
                Assert.AreEqual(2, db.Table<Database.User>().Count());

            }
        }
        [TestMethod]
        public void LessonInsertTest() {
            PropertyFinder.CreateInstance(new Dictionary<string, object>(){{"UiTexts", new UiTexts("../../../LangSharper/" + Globals.UiTextFileName)}});
            File.Delete(Globals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), d.FileName))
            {
                var correctUser = new Database.User() {Name = "testuser"};
                db.Insert(correctUser);
                var correctUser2 = new Database.User() {Name = "testuser2"};
                db.Insert(correctUser2);

                db.Insert(new Database.Lesson() {Name = "testlesson", UserId = correctUser.Id});

                try
                {
                    db.Insert(new Database.Lesson() {Name = "testlesson", UserId = correctUser.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                    StringAssert.Contains(e.Message, "Constraint");
                }

                try
                {
                    db.Insert(new Database.Lesson() {Name = "testlesson", UserId = correctUser2.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                    StringAssert.Contains(e.Message, "Constraint");
                }

                try
                {
                    db.Insert(new Database.Lesson() {Name = "testlesson2" });
                }
                catch (NotNullConstraintViolationException e) {}

                db.Insert(new Database.Lesson() {Name = "testlesson2", UserId = correctUser.Id});

                Assert.AreEqual(2, db.Table<Database.Lesson>().Count());
            }
        }

        [TestMethod]
        public void InsertWordTest()
        {
            PropertyFinder.CreateInstance(new Dictionary<string, object>(){{"UiTexts", new UiTexts("../../../LangSharper/" + Globals.UiTextFileName)}});
            File.Delete(Globals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), d.FileName))
            {
                var correctUser = new Database.User() {Name = "testuser"};
                db.Insert(correctUser);

                var correctLesson = new Database.Lesson() {Name = "testlesson", UserId = correctUser.Id};
                db.Insert(correctLesson);

                var correctWord = new Database.Word() {DefinitionLang1 = "kot", DefinitionLang2 = "a cat", LessonId = correctLesson.Id};
                db.Insert(correctWord);

                try
                {
                    db.Insert(new Database.Word() {DefinitionLang1 = "kot", DefinitionLang2 = "a cat", LessonId = correctLesson.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                   StringAssert.Contains(e.Message, "Constraint"); 
                }

                try
                {
                    db.Insert(new Database.Word() {DefinitionLang1 = "pies", LessonId = correctLesson.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (NotNullConstraintViolationException e)
                {
                }

                try
                {
                    db.Insert(new Database.Word() {DefinitionLang2 = "a dog", LessonId = correctLesson.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (NotNullConstraintViolationException e)
                {
                }

                Assert.AreEqual(1, db.Table<Database.Word>().Count());

            }
        }

        [TestMethod]
        public void InsertCorrectDataWordTest()
        {
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object>(){{"UiTexts", uiTexts}});
            File.Delete(Globals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), d.FileName))
            {
                var correctUser = new Database.User() {Name = "testuser"};
                db.Insert(correctUser);

                var correctLesson = new Database.Lesson() {Name = "testlesson", UserId = correctUser.Id};
                db.Insert(correctLesson);

                db.Insert(new Database.Word()
                {
                    DefinitionLang1 = "pies",
                    DefinitionLang2 = "a dog",
                    HasImage = true,
                    LessonId = correctLesson.Id,
                    Level = 3
                });

                // if entry doesn't exist we get exception
                db.Table<Database.Word>().Where(w => w.DefinitionLang1 == "pies" && w.DefinitionLang2 == "a dog" && w.HasImage && w.LessonId == correctLesson.Id && w.Level == 3).First();

                try
                {
                    db.Insert(new Database.Word() {DefinitionLang1 = "wąż", DefinitionLang2 = "a snake", Level = -1, LessonId = correctLesson.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (ArgumentException e)
                {
                    StringAssert.Contains(e.Message, uiTexts.GetText("ExWrongWordLevelValue"));
                }

                try
                {
                    db.Insert(new Database.Word() {DefinitionLang1 = "wąż", DefinitionLang2 = "a snake", Level = 8, LessonId = correctLesson.Id});
                    Assert.Fail("Exception should appear");
                }
                catch (ArgumentException e)
                {
                    StringAssert.Contains(e.Message, uiTexts.GetText("ExWrongWordLevelValue"));
                }
            }
        }
    }
}
