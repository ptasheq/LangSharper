using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using LangSharper;
using LangSharper.Resources;
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
                { "CurrentUser", new Database.User { Id = 0, Name = "testuser"}}
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
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();

            Assert.IsTrue(vm.IsChangeNameVisible);
            Assert.AreEqual(null, vm.Lesson.Name);
            Assert.AreEqual((PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id, vm.Lesson.UserId);

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = new Database.Lesson()
            {
                Id = 1, 
                Name = "testlesson",
                UserId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id
            };
            vm.OnViewActivate();

            Assert.IsFalse(vm.IsChangeNameVisible);
            StringAssert.Contains(BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson.Name, vm.Lesson.Name);
            Assert.AreEqual((PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id, vm.Lesson.UserId);
        }

        [TestMethod]
        public void OnViewActivate_WordsQueryTest()
        {
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            Assert.AreEqual(1, vm.ExtendedWords.Count);
            Assert.IsTrue(vm.ExtendedWords[0].IsNew);

            var newLesson = new Database.Lesson()
            {
                Name = "lessontestname",
                UserId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id
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
            Assert.AreEqual(3, vm.ExtendedWords.Count);
            foreach (var w in vm.ExtendedWords)
            {
                Assert.AreEqual(newLesson.Id, w.LessonId);
                Assert.IsFalse(w.IsNew);
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

            Assert.IsTrue(Directory.Exists(Path.Combine(Globals.ResourcePath, 
                                                        (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name, vm.NewName)));
            StringAssert.Contains(vm.Lesson.Name, vm.NewName);
            Assert.AreEqual(2, propertyChangedCount);

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = new Database.Lesson
            {
                Id = 1, 
                Name = "testlesson",
                UserId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id
            };
            vm.OnViewActivate();
            Assert.IsFalse(vm.ChangeLessonNameCmd.CanExecute(0));

            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson.Name = vm.NewName;
            vm.OnViewActivate();

            var oldName = "testlesson";
            vm.NewName = "testlesson2";
            Assert.IsTrue(vm.ChangeLessonNameCmd.CanExecute(0));
            vm.ChangeLessonNameCmd.Execute(0);
            Assert.IsTrue(Directory.Exists(Path.Combine(Globals.ResourcePath,
                                           (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name, vm.NewName)));
            Assert.IsFalse(Directory.Exists(Path.Combine(Globals.ResourcePath,
                                            (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name, oldName)));
        }

        [TestMethod]
        public void ChangeLessonNameCmdTest_DuplicateName()
        {
            PropertyFinder.Instance.Resource["CurrentUser"] = new Database.User { Id = 5, Name = "testuser" };
            var vm = new CreateModifyLessonsViewModel();
            vm.OnViewActivate();
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(new Database.Lesson() { Name = "lessontestname", UserId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id });
            }

            vm.NewName = "lessontestname";
            Assert.IsTrue(vm.ChangeLessonNameCmd.CanExecute(0));
            vm.ChangeLessonNameCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            StringAssert.Contains(vm.ErrorMessage, vm.Texts.Dict["ExLessonNameDuplicate"]);

            vm.OnViewActivate();

            (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id = 6;
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

        CreateModifyLessonsViewModel PrepareVm()
        {
            var vm = new CreateModifyLessonsViewModel();
            PropertyFinder.Instance.CurrentModel = vm;
            vm.NewName = "testlesson";
            vm.ChangeLessonNameCmd.Execute(0);
            return vm;
        }

        [TestMethod]
        public void DropImageTest()
        {
            var vm = PrepareVm();
            var item = new ExtendedWord(new Database.Word{ DefinitionLang1 = "blabla", DefinitionLang2 = "awefw", HasImage = false });
            vm.ExtendedWords.Add(item);
            Assert.IsTrue(vm.DropImageCmd.CanExecute(0));

            Assert.IsFalse(vm.IsErrorVisible);
            vm.DropImageCmd.Execute(null);
            Assert.IsFalse(vm.IsErrorVisible);
            Assert.AreEqual(item, vm.ExtendedWords[1]);

            //
            // Wrong data passed to command cases
            //
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, null));
            Assert.AreEqual(vm.Texts.Dict["ExWrongItemDropped"], vm.ErrorMessage);
            Assert.IsTrue(vm.IsErrorVisible);
            vm.HideError.Execute(0);

            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, new DataObject()));
            Assert.IsTrue(vm.IsErrorVisible);
            vm.HideError.Execute(0);

            var daOb = new DataObject();
            daOb.SetFileDropList(new StringCollection { Globals.Path + "test_image.png" });

            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, null, daOb));
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExDefinitionsFirst"], vm.ErrorMessage);
            vm.HideError.Execute(0);

            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(null, item.DefinitionLang2, daOb));
            Assert.IsTrue(vm.IsErrorVisible);
            vm.HideError.Execute(0);

            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(null, null, daOb));
            Assert.IsTrue(vm.IsErrorVisible);
            vm.HideError.Execute(0);
            Assert.AreEqual(item, vm.ExtendedWords[1]);


            //
            // Wrong file type
            //
            daOb.SetFileDropList(new StringCollection { Globals.Path + "uitexttest.ls" });
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExWrongItemDropped"], vm.ErrorMessage);
            vm.HideError.Execute(0);

            //
            // Correct data
            //
            int propChangedCount = 0;
            vm.ExtendedWords[1].PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
            {
                if (e.PropertyName == "HasImage" || e.PropertyName == "ImagePath")
                {
                    ++propChangedCount;
                }
            };
            daOb.SetFileDropList(new StringCollection { Globals.Path + "test_image.png" });
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsFalse(vm.IsErrorVisible);
            Assert.AreEqual(item.DefinitionLang1, vm.ExtendedWords[1].DefinitionLang1);
            Assert.AreEqual(item.DefinitionLang2, vm.ExtendedWords[1].DefinitionLang2);
            Assert.IsTrue(vm.ExtendedWords[1].HasImage);
            Assert.AreEqual(Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name,
                                         vm.Lesson.Name, item.DefinitionLang1 + "_" + item.DefinitionLang2 + ".png"), item.ImagePath.OriginalString);
            Assert.IsTrue(File.Exists(vm.ExtendedWords[1].ImagePath.AbsolutePath));
            Assert.AreEqual(2, propChangedCount);

            //
            // Correct data - add second time
            //
            daOb.SetFileDropList(new StringCollection { Globals.Path + "Nullimage.png" });
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsFalse(vm.IsErrorVisible);
            Assert.AreEqual(Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name,
                                         vm.Lesson.Name, item.DefinitionLang1 + "_" + item.DefinitionLang2 + ".png"), item.ImagePath.OriginalString);
            Assert.AreEqual(new FileInfo(Globals.Path + "Nullimage.png").Length, new FileInfo(vm.ExtendedWords[1].ImagePath.AbsolutePath).Length);

            //
            // Wrong view
            //
            PropertyFinder.Instance.CurrentModel = new StartViewModel();
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExWrongViewForAction"], vm.ErrorMessage);
            Assert.IsFalse(vm.ExtendedWords[1].HasImage);
            vm.HideError.Execute(0);
        }

        [TestMethod]
        public void DropImageTest_DefinitionsChange()
        {
            var vm = PrepareVm();
            var item = new ExtendedWord { DefinitionLang1 = "def1", DefinitionLang2 = "def2", HasImage = false };
            vm.ExtendedWords.Add(item);

            var daOb = new DataObject();
            daOb.SetFileDropList(new StringCollection { Globals.Path + "test_image.png" });
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            var tmpPath = item.ImagePath.AbsolutePath; 

            item.DefinitionLang1 = "blabl";
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsTrue(File.Exists(item.ImagePath.AbsolutePath));
            Assert.IsFalse(File.Exists(tmpPath));
        }

        [TestMethod]
        public void DropImageTest_DuplicateDefinitions()
        {
            var vm = PrepareVm();
            var item = new ExtendedWord { DefinitionLang1 = "def1", DefinitionLang2 = "def2", HasImage = false };
            vm.ExtendedWords.Add(item);
            vm.ExtendedWords.Add(item);

            var daOb = new DataObject();
            daOb.SetFileDropList(new StringCollection { Globals.Path + "test_image.png" });
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExDropImageDefinitions"], vm.ErrorMessage);
            vm.HideError.Execute(0);

            vm.ExtendedWords[1] = new ExtendedWord { DefinitionLang1 = "def1", DefinitionLang2 = "ddef2", HasImage = false };
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(item.DefinitionLang1, item.DefinitionLang2, daOb));
            Assert.IsFalse(vm.IsErrorVisible);
        }

        [TestMethod]
        public void AddWordItemTest()
        {
            var vm = PrepareVm(); 

            Assert.IsFalse(vm.AddWordItemCmd.CanExecute(0));
            vm.ExtendedWords[0].DefinitionLang1 = "def1";
            Assert.IsFalse(vm.AddWordItemCmd.CanExecute(0));
            vm.ExtendedWords[0].DefinitionLang2 = "def2";
            Assert.IsTrue(vm.AddWordItemCmd.CanExecute(0));

            vm.ExtendedWords[0].DefinitionLang1 = "";
            Assert.IsFalse(vm.AddWordItemCmd.CanExecute(0));
            vm.ExtendedWords[0].DefinitionLang1 = "def1";
            vm.AddWordItemCmd.Execute(0);
            Assert.AreEqual(2, vm.ExtendedWords.Count);
            Assert.AreEqual(vm.Lesson.Id, vm.ExtendedWords[1].LessonId);
            Assert.IsNull(vm.ExtendedWords[1].DefinitionLang1);
            Assert.IsNull(vm.ExtendedWords[1].DefinitionLang2);
            Assert.IsFalse(vm.ExtendedWords[1].HasImage);
            Assert.AreEqual(Path.GetFullPath(Globals.Path + "NullImage.png"), vm.ExtendedWords[1].ImagePath.OriginalString);

            //
            // max word number
            //
            for (int i = 2; i < Globals.MaxWordsForLesson; ++i)
            {
                vm.AddWordItemCmd.Execute(0);
                vm.ExtendedWords[i].DefinitionLang1 = "def1";
                vm.ExtendedWords[i].DefinitionLang2 = "def2";
            }
            Assert.AreEqual(30, vm.ExtendedWords.Count);
            vm.AddWordItemCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExWordNumberLimitReached"], vm.ErrorMessage);
            Assert.AreEqual(30, vm.ExtendedWords.Count);
        }

        [TestMethod]
        public void ConfirmChangesTest()
        {
            var vm = PrepareVm();
            
            vm.ExtendedWords[0].DefinitionLang1 = "def1";
            vm.ExtendedWords[0].DefinitionLang2 = "def2";
            vm.ConfirmChangesCmd.Execute(0);
            Assert.IsFalse(vm.IsErrorVisible);
            vm.ConfirmChangesCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoChangesToConfirm"], vm.ErrorMessage);
            vm.HideError.Execute(0);
            int id1 = vm.ExtendedWords[0].Id;

            //
            // adding new word
            //
            vm.AddWordItemCmd.Execute(0);
            vm.ExtendedWords[1].DefinitionLang1 = "ddef1";
            vm.ExtendedWords[1].DefinitionLang2 = "ddef2";
            vm.ConfirmChangesCmd.Execute(0);
            Assert.IsFalse(vm.IsErrorVisible);
            int id2 = vm.ExtendedWords[1].Id;

            //
            // view change
            //
            vm.PreviousCmd.Execute(0);
            BaseViewModel.GetViewModel<ManageLessonsViewModel>().SelectedLesson = vm.Lesson;
            PropertyFinder.Instance.CurrentModel = vm;
            Assert.AreEqual(2, vm.ExtendedWords.Count);
            Assert.AreEqual(id1, vm.ExtendedWords[0].Id);
            Assert.AreEqual(id2, vm.ExtendedWords[1].Id);

            //
            // drop image
            //
            var daOb = new DataObject();
            daOb.SetFileDropList(new StringCollection { Globals.Path + "test_image.png" });
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(vm.ExtendedWords[0].DefinitionLang1, 
                                                                           vm.ExtendedWords[0].DefinitionLang2, daOb));
            vm.ConfirmChangesCmd.Execute(0);
            Assert.IsFalse(vm.IsErrorVisible);

            //
            // drop image - unsuccessful
            //
            vm.DropImageCmd.Execute(new Tuple<string, string, IDataObject>(null, null, daOb));
            vm.HideError.Execute(0);
            vm.ConfirmChangesCmd.Execute(0);
            Assert.IsTrue(vm.IsErrorVisible);
            Assert.AreEqual(vm.Texts.Dict["ExNoChangesToConfirm"], vm.ErrorMessage);
        }
    }
}