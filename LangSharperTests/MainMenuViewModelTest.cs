using System;
using System.Collections.Generic;
using System.IO;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using LangSharper;

namespace LangSharperTests
{
    [TestClass]
    public class MainMenuViewModelTest
    {
        [TestInitialize]
        public void TestInit()
        {
            File.Delete(Globals.Path + "testdatabase.sqlite");
            var uiTexts = new UiTexts("../../../LangSharper/" + Globals.UiTextFileName);
            PropertyFinder.CreateInstance(new Dictionary<string, object>
            {
                { "UiTexts", uiTexts }, 
                { "DatabasePath", Globals.Path + "testdatabase.sqlite" }
            });
            var d = new Database(PropertyFinder.Instance.Resource["DatabasePath"].ToString());
        }

        [TestMethod]
        public void ChangeViewCmdsTest()
        {
            var vm = new MainMenuViewModel();
            var dict = new Dictionary<AppCommand, Type>()
            {
                {vm.ManageLessonsCmd, typeof (ManageLessonsViewModel)},
                {vm.SimpleLearningCmd, typeof (SimpleLearningViewModel)},
                {vm.WriteLearningCmd, typeof (WriteLearningViewModel)},
                {vm.StatisticsCmd, typeof (StatisticsViewModel)},
                {vm.ReturnToLoginCmd, typeof (StartViewModel)}
            };
            foreach (var pair in dict)
            {
                Assert.IsTrue(pair.Key.CanExecute(0));
                pair.Key.Execute(0);
                Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, pair.Value);
            }
        }
    }
}
