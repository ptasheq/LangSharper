using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using LangSharper;
using LangSharper.Resources;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharperTests
{
    [TestClass]
    public class SimpleLearningViewModelTest
    {
        SimpleLearningViewModel vm;
        int _propertyChangedCount;
        [TestInitialize]
        public void TestInit()
        {
            File.Delete(Globals.Path + "testdatabase.sqlite");
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                { "UiTexts", uiTexts }, 
                { "DatabasePath", Globals.Path + "testdatabase.sqlite" },
                { "CurrentUser", new Database.User { Id = 0, Name = "testuser"}},
                { "WaitTime", 0 }
            });
            var d = new Database(Globals.AppName, PropertyFinder.Instance.Resource["DatabasePath"].ToString());    
            vm = new SimpleLearningViewModel();
            _propertyChangedCount = 0;
        }

        [TestCleanup]
        public void TestClean()
        {
            Globals.DeleteDirIfExists(Path.Combine(Globals.ResourcePath,
                                      (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name), true);
        }

        [TestMethod]
        public void ConstructorAndOnViewActivateTest()
        {
            Assert.IsFalse(vm.LessonStarted); 
        }

        void PrepareChangeLearningStateCmdTest()
        {
            int userId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id;
            var lesson = new Database.Lesson { Name = "testlesson1", UserId = userId };
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(lesson);
                db.Insert(new ExtendedWord { DefinitionLang1 = "def1", DefinitionLang2 = "def2", LessonId = lesson.Id, Level = 1 }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def2", DefinitionLang2 = "def3", LessonId = lesson.Id, Level = 1 }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def3", DefinitionLang2 = "def4", LessonId = lesson.Id, Level = 1 }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def4", DefinitionLang2 = "def5", LessonId = lesson.Id, Level = 1 }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def5", DefinitionLang2 = "def6", LessonId = lesson.Id, Level = 1 }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def6", DefinitionLang2 = "def7", LessonId = lesson.Id, Level = 2 }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def5", DefinitionLang2 = "def6", LessonId = lesson.Id + 1 }, typeof(Database.Word));
            }
            vm.OnViewActivate();
            vm.Lesson = lesson;
            vm.SelectedLevel = 1;
            vm.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == "LessonStarted" || args.PropertyName == "WordsRemaining") 
                    ++_propertyChangedCount;
            };
        }

        [TestMethod]
        public void ChangeLeariningStateCmdTest_StartLearning()
        {
            PrepareChangeLearningStateCmdTest();
            vm.ChangeLearningStateCmd.Execute(0);
            Assert.IsFalse(vm.IsErrorVisible);
            Assert.IsTrue(vm.LessonStarted);
            Assert.AreEqual(Globals.WordsToChooseCount, vm.WordsToChoose.Count);
            Assert.AreEqual(5, vm.WordsRemaining);
            Assert.AreEqual(2, _propertyChangedCount);
            Assert.AreEqual(4, vm.WordsToChoose.Count);
            Assert.IsNotNull(vm.WordToTranslate);

            vm.SelectedLevel = null;
            Assert.IsTrue(vm.ChangeLearningStateCmd.CanExecute(0));
            vm.ChangeLearningStateCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoWordsLevelChosen"], vm.ErrorMessage);
            vm.SelectedLevel = 0;
            vm.HideError.Execute(0);

            vm.Lesson = null;
            vm.OnViewActivate();
            Assert.IsTrue(vm.ChangeLearningStateCmd.CanExecute(0));
            vm.ChangeLearningStateCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoLessonChosen"], vm.ErrorMessage);
            Assert.AreEqual(0, vm.WordsRemaining);
        }

        [TestMethod]
        public void ChangeLearningStateCmdTest_LevelWithZeroWords()
        {
            PrepareChangeLearningStateCmdTest();
            vm.SelectedLevel = 5;
            vm.ChangeLearningStateCmd.Execute(0);
            Assert.IsFalse(vm.LessonStarted);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExWrongWordsLevelChosen"], vm.ErrorMessage);
        }

        [TestMethod]
        public void ChangeLearningStateCmdTest_StopLearning()
        {
            PrepareChangeLearningStateCmdTest();
            vm.ChangeLearningStateCmd.Execute(0);
            vm.ChangeLearningStateCmd.Execute(0);
            Assert.IsFalse(vm.IsErrorVisible);
            Assert.IsFalse(vm.LessonStarted);
            Assert.AreEqual(0, vm.WordsRemaining);
            Assert.AreEqual(4, _propertyChangedCount);
        }

        [TestMethod]
        public void ChooseLearningStateCmdTest_Random()
        {
            PrepareChangeLearningStateCmdTest();
            vm.ChangeLearningStateCmd.Execute(0);
            var wordsToChooseCopy = vm.WordsToChoose.ToArray();
            bool[] areConditionsSatisfied = {true, true, true};
            int prevWordToTran = vm.WordToTranslate.Id;
            //
            // check if is random
            //
            for (int i = 1; i < 10; ++i)
            {
                vm.ChangeLearningStateCmd.Execute(0);
                if (i % 2 == 0)
                {
                    areConditionsSatisfied[0] = areConditionsSatisfied[0] || (vm.WordToTranslate.Id != prevWordToTran);
                    Debug.WriteLine("=== Test ===");
                    for (int j = 0; j < Globals.WordsToChooseCount; ++j)
                    {
                        Debug.WriteLine(vm.WordsToChoose[j].DefinitionLang1);
                        areConditionsSatisfied[1] = areConditionsSatisfied[1] || (vm.WordsToChoose[j] != wordsToChooseCopy[j]);
                        areConditionsSatisfied[2] = areConditionsSatisfied[2] && (vm.WordsToChoose.Count(w => w.Id == vm.WordsToChoose[j].Id) == 1);
                    }
                }
            }
            Assert.IsTrue(areConditionsSatisfied[0]);
            Assert.IsTrue(areConditionsSatisfied[1]);
            Assert.IsTrue(areConditionsSatisfied[2]);
        }

        void PrepareChooseAnswerCmdTest()
        {
            PrepareChangeLearningStateCmdTest();
            vm.ChangeLearningStateCmd.Execute(0);
            _propertyChangedCount = 0;
        }

        [TestMethod]
        public void ChooseAnswerCmdTest_CorrectAnswers()
        {
            PrepareChooseAnswerCmdTest();
            int WordsInitCount = vm.WordsRemaining;
            List<Database.Word> words = new List<Database.Word>();
            
            for (int i = 0; i < WordsInitCount; ++i)
            {
                Assert.AreEqual(-1, words.IndexOf(vm.WordToTranslate));
                Assert.AreEqual(1, vm.WordsToChoose.Count(w => w.Id == vm.WordToTranslate.Id));
                words.Add(vm.WordsToChoose.First(w => w.Id == vm.WordToTranslate.Id));
                (words[i] as ExtendedWord).PropertyChanged +=
                    delegate(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == "CorrectlyAnswered")
                            Assert.AreEqual(ExtendedWord.AnswerType.Correct, (words[i] as ExtendedWord).CorrectlyAnswered);
                    };
                Assert.IsTrue(vm.ChooseAnswerCmd.CanExecute(words[i]));
                vm.ChooseAnswerCmd.Execute(words[i]);
            }
            Assert.AreEqual(0, vm.WordsRemaining);
            Assert.IsFalse(vm.LessonStarted);
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                int i;
                foreach (var word in db.Table<Database.Word>().Where(w => w.LessonId == vm.Lesson.Id))
                {
                    if ((i = words.IndexOf(word)) > -1)
                    {
                        Assert.AreEqual(words[i].Level + 1, word.Level);
                    }
                }
            }
        }

        [TestMethod]
        public void ChooseAnswerCmdTest_WrongAnswer()
        {
            PrepareChooseAnswerCmdTest();
            int wordsInitCount = vm.WordsRemaining;
            List<Database.Word> words = new List<Database.Word>();

            for (int i = 0; i < wordsInitCount; ++i)
            {
                if (i != 1)
                {
                    vm.ChooseAnswerCmd.Execute(vm.WordToTranslate);
                }
                else
                {
                    words.Add(vm.WordToTranslate);
                    var word = vm.WordsToChoose.First(w => w.Id == vm.WordToTranslate.Id);
                    Debug.WriteLine("Wrong answer " + word.DefinitionLang1);
                    word.PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
                    {
                        if (args.PropertyName == "CorrectlyAnswered")
                        {
                            Assert.AreEqual(ExtendedWord.AnswerType.Correct, word.CorrectlyAnswered);
                            word = vm.WordsToChoose.First(w => w.Id != vm.WordToTranslate.Id); 
                            Assert.AreEqual(ExtendedWord.AnswerType.Incorrect, word.CorrectlyAnswered);
                        }
                    };
                    vm.ChooseAnswerCmd.Execute(vm.WordsToChoose.First(w => w.Id != vm.WordToTranslate.Id));
                }
                Debug.WriteLine("loop");
            }
            Assert.AreEqual(1, vm.WordsRemaining);
            Debug.WriteLine(vm.WordToTranslate.DefinitionLang1);
            Debug.WriteLine(words[0].DefinitionLang1);
            Assert.AreNotEqual(-1, words.IndexOf(vm.WordToTranslate));
            vm.ChooseAnswerCmd.Execute(vm.WordsToChoose.First(w => w.Id == vm.WordToTranslate.Id));
            Assert.AreEqual(0, vm.WordsRemaining);
            Assert.IsFalse(vm.LessonStarted);
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                int i;
                foreach (var word in db.Table<Database.Word>().Where(w => w.LessonId == vm.Lesson.Id))
                {
                    if ((i = words.IndexOf(word)) > -1)
                    {
                        Assert.AreEqual(0, word.Level);
                    }
                }
            }
            

        }
    }
}