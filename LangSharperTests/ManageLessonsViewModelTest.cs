using System;
using System.Collections.Generic;
using System.IO;
using LangSharper;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

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
                { "CurrentUser", new Database.User { Name = "testuser" }}
            });
            var d = new Database(PropertyFinder.Instance.Resource["DatabasePath"].ToString());
        }

        [TestMethod]
        public void PreviousCmdTest()
        {
            var vm = new ManageLessonsViewModel();
            Assert.IsTrue(vm.PreviousCmd.CanExecute(0));
            vm.PreviousCmd.Execute(0);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof (MainMenuViewModel));
        }
    }
}
