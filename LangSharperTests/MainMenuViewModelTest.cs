﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LangSharper;

namespace LangSharperTests
{
    [TestClass]
    public class MainMenuViewModelTest
    {
        string path;

        [TestInitialize]
        public void TestInit()
        {
            File.Delete(TestGlobals.Path + "testdatabase.sqlite");
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                { "UiTexts", uiTexts }, 
                { "DatabasePath", TestGlobals.Path + "testdatabase.sqlite" },
                { "CurrentUser", new Database.User { Name = "testuser"}},
                { "ViewModelStack", new Stack<BaseViewModel>() } 
            });
            var d = new Database(Globals.AppName, PropertyFinder.Instance.Resource["DatabasePath"].ToString());
            path = Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name); 
        }

        [TestMethod]
        public void ConstructorTest()
        {
            var vm = new MainMenuViewModel();
            Assert.IsTrue(Directory.Exists(path));
            vm = new MainMenuViewModel();
        }

        [TestMethod]
        public void ChangeViewCmdsTest()
        {
            var vm = new MainMenuViewModel();
            PropertyFinder.Instance.CurrentModel = new StartViewModel();
            PropertyFinder.Instance.CurrentModel = vm;
            var dict = new Dictionary<AppCommand, Type>
            {
                {vm.ManageLessonsCmd, typeof (ManageLessonsViewModel)},
                {vm.SimpleLearningCmd, typeof (SimpleLearningViewModel)},
                {vm.WriteLearningCmd, typeof (WriteLearningViewModel)},
                {vm.StatisticsCmd, typeof (StatisticsViewModel)}
            };
            foreach (var pair in dict)
            {
                Assert.IsTrue(pair.Key.CanExecute(0));
                pair.Key.Execute(0);
                Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, pair.Value);
                PropertyFinder.Instance.ReturnToPreviousModel();
            }
            PropertyFinder.Instance.ReturnToPreviousModel();
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof (StartViewModel));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            Directory.Delete(path, true);
        }
    }
}
