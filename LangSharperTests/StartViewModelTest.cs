using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using LangSharper;
using LangSharper.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharperTests
{
    [TestClass]
    public class StartViewModelTest
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
            Database d = new Database(PropertyFinder.Instance.Resource["DatabasePath"].ToString());

            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(new Database.User() {Name = "testuser"});
                db.Insert(new Database.User() {Name = "testuser2"});
                db.Insert(new Database.User() {Name = "testuser3"});
            }
        }


        [TestMethod]
        public void ConstructorTest()
        {
            var m = new StartViewModel();
            Assert.AreEqual(3, m.Users.Count);
        }

        [TestMethod]
        public void ShowAddUserControlsCmdTest()
        {
            string receivedEvent = "";
            var m = new StartViewModel();
            m.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                receivedEvent = e.PropertyName;
            };
            m.ShowAddUserControlsCmd.Execute(0);
            StringAssert.Contains(receivedEvent, "AddUserVisible");
        }

        [TestMethod]
        public void AddUserCmdTest()
        {
            string receivedEvent = "";
            string testUserName = "testuser4";
            var m = new StartViewModel();
            Assert.IsFalse(m.AddUserCmd.CanExecute(0));

            m.NewUserName = testUserName;
            m.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                receivedEvent = e.PropertyName;
            };
            m.AddUserCmd.Execute(0);
            Assert.IsTrue(m.Users.Any(u => u.Name == testUserName));
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Table<Database.User>().First(u => u.Name == testUserName);
            }

            Assert.IsFalse(m.AddUserCmd.CanExecute(0));
            m.NewUserName = "testuser5";
            Assert.IsTrue(m.AddUserCmd.CanExecute(0));

            StringAssert.Contains(receivedEvent, "Users");
        }

        [TestMethod]
        public void ChooseProfileCmdTest()
        {
            var m = new StartViewModel();
            m.NewUserName = "testuser4";
            m.AddUserCmd.Execute(0);

            var user = m.Users.First(e => e.Name == "testuser4");
            m.UserIndex = m.Users.IndexOf(user);

            m.ChooseUserCmd.Execute(0);

            Assert.AreEqual(user.Id, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id);
            Assert.IsInstanceOfType(PropertyFinder.Instance.CurrentModel, typeof(MainMenuViewModel));
        }
    }
}
