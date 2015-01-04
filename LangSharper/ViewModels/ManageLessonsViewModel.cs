using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        public ManageLessonsViewModel()
        {
            SelectedLesson = null;
            PreviousCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<MainMenuViewModel>(); });
            CreateNewLessonCmd = new AppCommand(CreateNewLesson);
            DeleteChosenLessonCmd = new AppCommand(DeleteChosenLesson);
            EditChosenLessonCmd = new AppCommand(EditChosenLesson);
        }

        public override void OnViewActivate()
        {
            int userId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id;
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                Lessons = new ObservableCollection<Database.Lesson>(db.Table<Database.Lesson>().Where(l => l.UserId == userId));
            }
        }

        public Database.Lesson SelectedLesson { get; set; }
        public ObservableCollection<Database.Lesson> Lessons { get; private set; }

        public AppCommand CreateNewLessonCmd { get; private set; }
        public AppCommand EditChosenLessonCmd { get; private set; }
        public AppCommand DeleteChosenLessonCmd { get; private set; }
        public AppCommand PreviousCmd { get; private set; }

        public void CreateNewLesson()
        {
            SelectedLesson = null;
            PropertyFinder.Instance.CurrentModel = GetViewModel<CreateModifyLessonsViewModel>();
        }

        public void DeleteChosenLesson()
        {
            if (SelectedLesson == null)
            {
                ShowError("ExNoLessonChosen");
                return;
            }
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Delete(SelectedLesson);
            }
            Lessons.Remove(SelectedLesson);
            SelectedLesson = null;
        }

        public void EditChosenLesson()
        {
            if (SelectedLesson == null)
            {
                ShowError("ExNoLessonChosen");
                return;
            }
            PropertyFinder.Instance.CurrentModel = GetViewModel<CreateModifyLessonsViewModel>();
        }
    }
}