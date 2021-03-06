﻿using System.Collections.ObjectModel;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public abstract class BaseLessonListViewModel : BaseViewModel
    {
        public override void OnViewActivate()
        {
            Lesson = null;
            var userId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id;
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                Lessons = new ObservableCollection<Database.Lesson>(db.Table<Database.Lesson>().Where(l => l.UserId == userId));
            }
            OnPropertyChanged("Lesson");
        }

        public Database.Lesson Lesson { get; set; }
        public ObservableCollection<Database.Lesson> Lessons { get; protected set; }
    }
}
