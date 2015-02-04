using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq.Expressions;
using LangSharper.ViewModels;
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
            File.Delete(TestGlobals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.AppName, TestGlobals.Path + "testdatabase.sqlite");
            Assert.IsTrue(File.Exists(TestGlobals.Path + "testdatabase.sqlite"));

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), Database.FileName))
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
            PropertyFinder.CreateInstance(new Dictionary<string, object> {{"UiTexts", uiTexts}});
            File.Delete(TestGlobals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.AppName, TestGlobals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), Database.FileName))
            {
                db.InsertOrReplace(new Database.User { Name = "testuser"});

                try
                {
                    db.Insert(new Database.User { Name = "testuser"});
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                    StringAssert.Contains(e.Message, "Constraint");
                }

                db.Insert(new Database.User { Name = "testuser2"});

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
                    db.Insert(new Database.User { Name = ""});
                    Assert.Fail("Exception should appear");
                }
                catch (ArgumentException e)
                {
                    StringAssert.Contains(uiTexts.GetText(e.Message), uiTexts.GetText("ExTooShortUserName"));
                }
                Assert.AreEqual(2, db.Table<Database.User>().Count());

            }
        }
        [TestMethod]
        public void LessonInsertTest() {
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                {"UiTexts", new UiTexts("../../../LangSharper/" + Globals.UiTextFileName)}
            });
            File.Delete(TestGlobals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.AppName, TestGlobals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), Database.FileName))
            {
                var correctUser = new Database.User { Name = "testuser"};
                db.Insert(correctUser);
                var correctUser2 = new Database.User { Name = "testuser2"};
                db.Insert(correctUser2);

                db.Insert(new Database.Lesson { Name = "testlesson", UserId = correctUser.Id });

                try
                {
                    db.Insert(new Database.Lesson { Name = "testlesson", UserId = correctUser.Id });
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                    StringAssert.Contains(e.Message, "Constraint");
                }

                try
                {
                    db.Insert(new Database.Lesson { Name = "testlesson2" });
                }
                catch (NotNullConstraintViolationException e) {}

                db.Insert(new Database.Lesson { Name = "testlesson2", UserId = correctUser.Id });

                Assert.AreEqual(2, db.Table<Database.Lesson>().Count());
            }
        }

        [TestMethod]
        public void InsertWordAndWordCountTest()
        {
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                { "UiTexts", new UiTexts("../../../LangSharper/" + Globals.UiTextFileName) }
            });
            File.Delete(TestGlobals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.AppName, TestGlobals.Path + "testdatabase.sqlite");

            Database.Lesson correctLesson;
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), Database.FileName))
            {
                var correctUser = new Database.User { Name = "testuser"};
                db.Insert(correctUser);

                correctLesson = new Database.Lesson { Name = "testlesson", UserId = correctUser.Id };
                db.Insert(correctLesson);

                var correctWord = new Database.Word { DefinitionLang1 = "kot", DefinitionLang2 = "a cat", LessonId = correctLesson.Id };
                db.Insert(correctWord);
                db.Insert(new Database.Word { DefinitionLang1 = "wąż", DefinitionLang2 = "a snake", LessonId = correctLesson.Id });

                try
                {
                    db.Insert(new Database.Word { DefinitionLang1 = "kot", DefinitionLang2 = "a cat", LessonId = correctLesson.Id });
                    Assert.Fail("Exception should appear");
                }
                catch (SQLiteException e)
                {
                    StringAssert.Contains(e.Message, "Constraint"); 
                }

                try
                {
                    db.Insert(new Database.Word { DefinitionLang1 = "pies", LessonId = correctLesson.Id });
                    Assert.Fail("Exception should appear");
                }
                catch (NotNullConstraintViolationException e)
                {
                }

                try
                {
                    db.Insert(new Database.Word { DefinitionLang2 = "a dog", LessonId = correctLesson.Id });
                    Assert.Fail("Exception should appear");
                }
                catch (NotNullConstraintViolationException e)
                {
                }

                db.Insert(new Database.Word { DefinitionLang1 = "pies", DefinitionLang2 = "a dog", LessonId = correctLesson.Id+1 });
                Assert.AreEqual(3, db.Table<Database.Word>().Count());
            }
            Assert.AreEqual(2, correctLesson.WordCount);
        }

        [TestMethod]
        public void InsertCorrectDataWordTest()
        {
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object> {{"UiTexts", uiTexts}});
            File.Delete(TestGlobals.Path + "testdatabase.sqlite");
            Database d = new Database(Globals.AppName, TestGlobals.Path + "testdatabase.sqlite");

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), Database.FileName))
            {
                var correctUser = new Database.User { Name = "testuser"};
                db.Insert(correctUser);

                var correctLesson = new Database.Lesson { Name = "testlesson", UserId = correctUser.Id };
                db.Insert(correctLesson);

                db.Insert(new Database.Word
                {
                    DefinitionLang1 = "pies",
                    DefinitionLang2 = "a dog",
                    HasImage = true,
                    LessonId = correctLesson.Id,
                    Level = 3
                });

                // if entry doesn't exist we get exception
                db.Table<Database.Word>().Where(w => w.DefinitionLang1 == "pies" && w.DefinitionLang2 == "a dog" 
                    && w.HasImage && w.LessonId == correctLesson.Id && w.Level == 3).First();

                db.Insert(new Database.Word
                {
                    DefinitionLang1 = "pies",
                    DefinitionLang2 = "a dog",
                    HasImage = true,
                    LessonId = correctLesson.Id+1,
                    Level = 3
                });

                try
                {
                    db.Insert(new Database.Word { DefinitionLang1 = "wąż", DefinitionLang2 = "a snake", Level = -1, LessonId = correctLesson.Id });
                    Assert.Fail("Exception should appear");
                }
                catch (ArgumentException e)
                {
                    Assert.AreEqual(uiTexts.GetText(e.Message), uiTexts.GetText("ExWrongWordLevelValue"));
                }

                try
                {
                    db.Insert(new Database.Word { DefinitionLang1 = "wąż", DefinitionLang2 = "a snake", Level = 8, LessonId = correctLesson.Id });
                    Assert.Fail("Exception should appear");
                }
                catch (ArgumentException e)
                {
                    Assert.AreEqual(uiTexts.GetText(e.Message), uiTexts.GetText("ExWrongWordLevelValue"));
                }
            }
        }
        [TestMethod]
        public void GetImagePathTest()
        {
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                { "DatabasePath", TestGlobals.Path + "testdatabase.sqlite" },
                { "CurrentUser", new Database.User { Name = "testuser"} }
            });
            Database d = new Database(Globals.AppName, TestGlobals.Path + "testdatabase.sqlite");
            var word = new Database.Word { DefinitionLang1 = "kot", DefinitionLang2 = "a cat", LessonId = 1, HasImage = true };
            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                       Globals.AppName, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name,
                                       "testlesson", word.DefinitionLang1.Replace(" ", "_") + "_" + word.DefinitionLang2.Replace(" ", "_") + ".png");

            try
            {
                word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, null);
                Assert.Fail("Exception should appear");
            }
            catch (NullReferenceException e)
            {
                Assert.AreEqual("ExLessonNotSpecified", e.Message);
            }

            try
            {
                word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, new Database.Lesson { Name = "" });
                Assert.Fail("Exception should appear");
            }
            catch (NullReferenceException e)
            {
                Assert.AreEqual("ExLessonNotSpecified", e.Message);
            }
            
            var lesson = new Database.Lesson { Id = 5, Name = "testlesson" };
            try
            {
                word.GetImagePath(null, lesson);
                Assert.Fail("Exception should appear");
            }
            catch (NullReferenceException e)
            {
                Assert.AreEqual("ExUserNotSpecified", e.Message);
            }

            try
            {
                var word2 = new Database.Word{ HasImage = true };
                word2.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson);
                Assert.Fail("Exception should appear");
            }
            catch (NullReferenceException e)
            {
                Assert.AreEqual("ExWrongViewForAction", e.Message);
            }

            Assert.AreEqual(path, word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson));

            word.HasImage = false;
            Assert.AreEqual(null, word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson));
        }
    }
}