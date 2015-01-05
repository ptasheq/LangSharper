using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public class ManageLessonsViewModel : BaseLessonListViewModel
    {
        public ManageLessonsViewModel()
        {
            SelectedLesson = null;
            CreateNewLessonCmd = new AppCommand(CreateNewLesson);
            DeleteChosenLessonCmd = new AppCommand(DeleteChosenLesson);
            EditChosenLessonCmd = new AppCommand(EditChosenLesson);
        }

        public AppCommand CreateNewLessonCmd { get; private set; }
        public AppCommand EditChosenLessonCmd { get; private set; }
        public AppCommand DeleteChosenLessonCmd { get; private set; }

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