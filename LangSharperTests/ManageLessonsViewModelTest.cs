using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using LangSharper;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharperTests
{
    [TestClass]
    public class ManageLessonsViewModelTest
    {
        [TestInitialize]
        public void TestInit()
        {
            File.Delete(Globals.Path + "testdatabase.sqlite");
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                { "UiTexts", uiTexts }, 
                { "DatabasePath", Globals.Path + "testdatabase.sqlite" },
                { "CurrentUser", new Database.User { Id = 1, Name = "testuser" }}
            });
            var d = new Database(Globals.AppName, PropertyFinder.Instance.Resource["DatabasePath"].ToString());
        }

        [TestCleanup]
        public void TestClean()
        {
            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = null;
            Globals.DeleteDirIfExists(Path.Combine(Globals.ResourcePath,
                                      (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name), true);
        }


        [TestMethod]
        public void ContructorAndOnViewActivateTest()
        {
            int userId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id;
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(new Database.Lesson { Name = "testlesson1", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson2", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson3", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson4", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson5", UserId = userId+1 });
            }
            var vm = new ManageLessonsViewModel();
            vm.OnViewActivate();
            Assert.AreEqual(4, vm.Lessons.Count);
        }

        Database.Lesson PrepareModifyingTests()
        {
            int userId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id;
            var lesson = new Database.Lesson { Name = "testlesson1", UserId = userId };
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(lesson);
            }
            PropertyFinder.Instance.CurrentModel = BaseViewModel.GetViewModel<ManageLessonsViewModel>();
            return lesson;
        }

        [TestMethod]
        public void CreateNewLessonCmdTest()
        {
            Database.Lesson lesson = PrepareModifyingTests();

            var vm = BaseViewModel.GetViewModel<ManageLessonsViewModel>();
            vm.SelectedLesson = null;
            Assert.IsTrue(vm.CreateNewLessonCmd.CanExecute(0));
            vm.CreateNewLessonCmd.Execute(0);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof(CreateModifyLessonsViewModel));
            Assert.IsFalse(vm.IsErrorVisible);

            PropertyFinder.Instance.CurrentModel = vm;
            vm.SelectedLesson = lesson;
            Assert.IsTrue(vm.CreateNewLessonCmd.CanExecute(0));
            vm.CreateNewLessonCmd.Execute(0);
            Assert.IsNull(vm.SelectedLesson);
            Assert.IsFalse(vm.IsErrorVisible);
            Assert.IsNull(BaseViewModel.GetViewModel<CreateModifyLessonsViewModel>().Lesson.Name);
        }

        [TestMethod]
        public void DeleteChosenLessonCmdTest()
        {
            Database.Lesson lesson = PrepareModifyingTests();

            var vm = BaseViewModel.GetViewModel<ManageLessonsViewModel>();
            vm.SelectedLesson = null;
            Assert.IsTrue(vm.DeleteChosenLessonCmd.CanExecute(0));
            vm.DeleteChosenLessonCmd.Execute(0);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof(ManageLessonsViewModel));
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoLessonChosen"], vm.ErrorMessage);

            vm.SelectedLesson = lesson;
            Assert.IsTrue(vm.DeleteChosenLessonCmd.CanExecute(0));
            vm.DeleteChosenLessonCmd.Execute(0);
            Assert.IsFalse(vm.Lessons.Contains(lesson));
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                Assert.AreEqual(0, db.Table<Database.Lesson>().Count(l => l.Id == lesson.Id));
            }
        }

        [TestMethod]
        public void EditChosenLessonCmdTest()
        {
            Database.Lesson lesson = PrepareModifyingTests();

            var vm = BaseViewModel.GetViewModel<ManageLessonsViewModel>();
            vm.SelectedLesson = null;
            Assert.IsTrue(vm.EditChosenLessonCmd.CanExecute(0));
            vm.EditChosenLessonCmd.Execute(0);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof(ManageLessonsViewModel));
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoLessonChosen"], vm.ErrorMessage);

            vm.SelectedLesson = lesson;
            Assert.IsTrue(vm.EditChosenLessonCmd.CanExecute(0));
            vm.EditChosenLessonCmd.Execute(0);
            Assert.AreSame(vm.SelectedLesson, BaseViewModel.GetViewModel<CreateModifyLessonsViewModel>().Lesson);
        }

        [TestMethod]
        public void PreviousCmdTest()
        {
            var vm = new ManageLessonsViewModel();
            Assert.IsTrue(vm.PreviousCmd.CanExecute(0));
            vm.PreviousCmd.Execute(0);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof(MainMenuViewModel));
        }
    }
}
