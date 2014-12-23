﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Dynamic;
using System.IO;
using LangSharper;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharperTests
{
    [TestClass]
    public class CreateModifyLessonsViewModelTest
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
                { "CurrentUserId", 0}
            });
            var d = new Database(PropertyFinder.Instance.Resource["DatabasePath"].ToString());
        }

        [TestCleanup]
        public void TestClean()
        {
            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = null;
        }

        [TestMethod]
        public void ContructorAndOnViewActivateTest()
        {
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();


            Assert.IsTrue(vm.IsChangeNameVisible);
            Assert.AreEqual(null, vm.Lesson.Name);
            Assert.AreEqual((int) PropertyFinder.Instance.Resource["CurrentUserId"], vm.Lesson.UserId);

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = new Database.Lesson()
            {
                Id = 1, 
                Name = "testlesson",
                UserId = (int) PropertyFinder.Instance.Resource["CurrentUserId"]
            };
            vm.OnViewActivate();

            Assert.IsFalse(vm.IsChangeNameVisible);
            StringAssert.Contains(BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson.Name, vm.Lesson.Name);
            Assert.AreEqual((int) PropertyFinder.Instance.Resource["CurrentUserId"], vm.Lesson.UserId);
        }

        [TestMethod]
        public void OnViewActivate_WordsQueryTest()
        {
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            Assert.AreEqual(1, vm.Words.Count);

            var newLesson = new Database.Lesson()
            {
                Name = "lessontestname",
                UserId = (int) PropertyFinder.Instance.Resource["CurrentUserId"]
            };

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(newLesson);
                db.Insert(new Database.Word { DefinitionLang1 = "a", DefinitionLang2 = "b", LessonId = newLesson.Id });
                db.Insert(new Database.Word { DefinitionLang1 = "b", DefinitionLang2 = "c", LessonId = newLesson.Id });
                db.Insert(new Database.Word { DefinitionLang1 = "c", DefinitionLang2 = "d", LessonId = newLesson.Id });
                db.Insert(new Database.Word { DefinitionLang1 = "c", DefinitionLang2 = "d", LessonId = newLesson.Id+1 });
            }

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = newLesson;
            vm.OnViewActivate();
            Assert.AreEqual(3, vm.Words.Count);
            foreach (Database.Word w in vm.Words)
            {
                Assert.AreEqual(newLesson.Id, w.LessonId);
            }

        }

        [TestMethod]
        public void PreviousCmdTest()
        {
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            Assert.IsTrue(vm.PreviousCmd.CanExecute(0));
            vm.PreviousCmd.Execute(0);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof (ManageLessonsViewModel));
        }

        [TestMethod]
        public void ChangeLessonNameCmdTest()
        {
            int propertyChangedCount = 0;
            PropertyChangedEventHandler del = delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "Lesson" || e.PropertyName == "IsChangeNameVisible")
                    ++propertyChangedCount;
            };
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            Assert.IsFalse(vm.ChangeLessonNameCmd.CanExecute(0));

            vm.NewName = "a";
            Assert.IsTrue(vm.ChangeLessonNameCmd.CanExecute(0));

            vm.NewName = "testname";
            Assert.IsTrue(vm.ChangeLessonNameCmd.CanExecute(0));
            vm.PropertyChanged += del;
            vm.ChangeLessonNameCmd.Execute(0);
            StringAssert.Contains(vm.Lesson.Name, vm.NewName);
            Assert.AreEqual(2, propertyChangedCount);

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = new Database.Lesson()
            {
                Id = 1, 
                Name = "testlesson",
                UserId = (int) PropertyFinder.Instance.Resource["CurrentUserId"]
            };
            vm.OnViewActivate();
            Assert.IsFalse(vm.ChangeLessonNameCmd.CanExecute(0));
        }

        [TestMethod]
        public void ChangeLessonNameCmdTest_DuplicateName()
        {
            PropertyFinder.Instance.Resource["CurrentUserId"] = 5;
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(new Database.Lesson() { Name = "lessontestname", UserId = (int) PropertyFinder.Instance.Resource["CurrentUserId"] });
            }

            vm.NewName = "lessontestname";
            Assert.IsTrue(vm.ChangeLessonNameCmd.CanExecute(0));
            vm.ChangeLessonNameCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            StringAssert.Contains(vm.ErrorMessage, vm.Texts.Dict["ExLessonNameDuplicate"]);

            vm.OnViewActivate();
            PropertyFinder.Instance.Resource["CurrentUserId"] = 6;
            vm.NewName = "lessontestname";
            Assert.IsTrue(vm.ChangeLessonNameCmd.CanExecute(0));
            vm.ChangeLessonNameCmd.Execute(0);
            Assert.IsFalse(vm.IsErrorVisible);
        }

        [TestMethod]
        public void ShowChangeLessonSectionCmdTest()
        {
            int propertyChangedCount = 0;
            PropertyChangedEventHandler del = delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "IsChangeNameVisible")
                    ++propertyChangedCount;
            };

            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            Assert.IsFalse(vm.ShowChangeLessonNameSectionCmd.CanExecute(0));
            Assert.IsTrue(vm.IsChangeNameVisible);

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = new Database.Lesson()
            {
                Id = 1, 
                Name = "testlesson",
            };
            vm.OnViewActivate();
            Assert.IsTrue(vm.ShowChangeLessonNameSectionCmd.CanExecute(0));
            Assert.IsFalse(vm.IsChangeNameVisible);
            vm.PropertyChanged += del;
            vm.ShowChangeLessonNameSectionCmd.Execute(0);
            Assert.AreEqual(1, propertyChangedCount);
            Assert.IsTrue(vm.IsChangeNameVisible);
        }
    }
}