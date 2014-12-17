using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public class CreateModifyLessonsViewModel : BaseViewModel
    {
        public CreateModifyLessonsViewModel()
        {
            if (GetViewModel<ManageLessonsViewModel>().SelectedLesson == null)
            {
                Lesson = new Database.Lesson { Name = null, UserId = PropertyFinder.Instance.Resource["CurrentUserId"] as int?  };
                IsChangeNameVisible = true;
            }
            else
            {
                Lesson = GetViewModel<ManageLessonsViewModel>().SelectedLesson;
                IsChangeNameVisible = false;
            }
            PreviousCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<ManageLessonsViewModel>(); });
            ChangeLessonNameCmd = new AppCommand(ChangeLessonName, () => !string.IsNullOrEmpty(NewName));
            ShowChangeLessonNameSectionCmd = new AppCommand(ShowChangeLessonSectionName, () => IsChangeNameVisible.Equals(false));
        }

        public AppCommand PreviousCmd { get; private set; }
        public AppCommand ChangeLessonNameCmd { get; private set; }
        public AppCommand ShowChangeLessonNameSectionCmd { get; private set; }

        void ChangeLessonName()
        {
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString())) 
            {
                if (db.Table<Database.Lesson>().Any(l => l.Name == NewName && l.UserId == (int?) PropertyFinder.Instance.Resource["CurrentUserId"]))
                {
                    ShowError("ExLessonNameDuplicate");
                    return;
                }
            }
            Lesson.Name = NewName;
            OnPropertyChanged("Lesson");
            IsChangeNameVisible = false;
            OnPropertyChanged("IsChangeNameVisible");
        }

        void ShowChangeLessonSectionName()
        {
            IsChangeNameVisible = true;
            OnPropertyChanged("IsChangeNameVisible");
        }

        public string NewName { get; set; }
        public bool IsChangeNameVisible { get; set; }
        public Database.Lesson Lesson { get; private set; }
    }
}
