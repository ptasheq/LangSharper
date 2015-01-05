using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Controls;
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
                { "CurrentUser", new Database.User { Id = 0, Name = "testuser"}}
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
                db.Insert(new ExtendedWord { DefinitionLang1 = "def1", DefinitionLang2 = "def2", LessonId = lesson.Id }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def2", DefinitionLang2 = "def3", LessonId = lesson.Id }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def3", DefinitionLang2 = "def4", LessonId = lesson.Id }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def4", DefinitionLang2 = "def5", LessonId = lesson.Id }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def5", DefinitionLang2 = "def6", LessonId = lesson.Id }, typeof(Database.Word));
                db.Insert(new ExtendedWord { DefinitionLang1 = "def5", DefinitionLang2 = "def6", LessonId = lesson.Id + 1 }, typeof(Database.Word));
            }
            vm.OnViewActivate();
            vm.SelectedLesson = lesson;
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

            vm.SelectedLesson = null;
            vm.OnViewActivate();
            Assert.IsTrue(vm.ChangeLearningStateCmd.CanExecute(0));
            vm.ChangeLearningStateCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoLessonChosen"], vm.ErrorMessage);
            Assert.AreEqual(0, vm.WordsRemaining);
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
            bool[] areConditionsSatisfied = new bool[2];
            int prevWordToTran = vm.WordToTranslate.Id;
            //
            // check if is random
            //
            for (int i = 1; i < 10 && areConditionsSatisfied.Any(c => !c); ++i)
            {
                vm.ChangeLearningStateCmd.Execute(0);
                Debug.WriteLine("index: " + i);
                if (i % 2 == 0)
                {
                    if (!areConditionsSatisfied[0])
                    {
                        areConditionsSatisfied[0] = vm.WordToTranslate.Id != prevWordToTran;
                    }
                    for (int j = 0; j < Globals.WordsToChooseCount && !areConditionsSatisfied[1]; ++j)
                    {
                        areConditionsSatisfied[1] = vm.WordsToChoose[j] != wordsToChooseCopy[j];
                    }
                }
            }
        }

        void PrepareChooseAnswerCmdTest()
        {
            PrepareChangeLearningStateCmdTest();
            vm.ChangeLearningStateCmd.Execute(0);
        }

        [TestMethod]
        public void ChooseAnswerCmdTest()
        {
            PrepareChooseAnswerCmdTest();
            int WordsInitCount = vm.WordsRemaining;
            var word = vm.WordsToChoose.First(w => w.Id == vm.WordToTranslate.Id);
            Assert.IsTrue(vm.ChooseAnswerCmd.CanExecute(word));
            vm.ChooseAnswerCmd.Execute(word);
            Assert.AreEqual(WordsInitCount-1, vm.WordsRemaining);
        }
    }
}