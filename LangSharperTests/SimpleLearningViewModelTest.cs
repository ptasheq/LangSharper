using System.Collections.Generic;
using System.IO;
using LangSharper;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace LangSharperTests
{
    [TestClass]
    public class SimpleLearningViewModelTest
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
                { "CurrentUser", new Database.User { Id = 0, Name = "testuser"}}
            });
            var d = new Database(Globals.AppName, PropertyFinder.Instance.Resource["DatabasePath"].ToString());    
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
            
        }
    }
}
