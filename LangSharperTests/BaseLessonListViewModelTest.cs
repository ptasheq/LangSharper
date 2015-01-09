using System.Collections.Generic;
using System.IO;
using LangSharper;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharperTests
{
    [TestClass]
    public class BaseLessonListViewModelTest
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
            BaseViewModel.GetViewModel<ManageLessonsViewModel>().Lesson = null;
            Globals.DeleteDirIfExists(Path.Combine(Globals.ResourcePath,
                                      (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name), true);
        }

        [TestMethod]
        public void OnViewActivateTest()
        {
            // base class is abstract, to instantiate we us ManageLessonsViewModel
            BaseLessonListViewModel vm = new ManageLessonsViewModel();
            vm.OnViewActivate();
            Assert.IsNull(vm.Lesson);
            Assert.AreEqual(0, vm.Lessons.Count);

            int userId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id;
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(new Database.Lesson { Name = "testlesson1", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson2", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson3", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson4", UserId = userId });
                db.Insert(new Database.Lesson { Name = "testlesson5", UserId = userId + 1 });
            }
            vm.OnViewActivate();
            Assert.AreEqual(4, vm.Lessons.Count);
        }
    }
}
