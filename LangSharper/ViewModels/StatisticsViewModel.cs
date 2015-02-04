using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public class StatisticsViewModel : BaseLessonListViewModel
    {
        const float BarWidth = 3.5f;

        public struct Bar
        {
            public int Level { get; set; }
            public int Width { get { return (int) (Percentage * BarWidth); } }
            public float Percentage { get; set; }
        }

        public StatisticsViewModel()
        {
            ChangeLessonChosenCmd = new AppCommand(ChangeLessonChosen);
            Bars = new ObservableCollection<Bar>();
        }

        public override void OnViewActivate()
        {
            base.OnViewActivate();
            LessonChosen = false;
            OnPropertyChanged("LessonChosen");
        }


        void ChangeLessonChosen()
        {
            if (Lesson == null)
            {
                ShowError("ExNoLessonChosen");
                return;
            }

            if (!LessonChosen)
            {

                Bars.Clear();
                List<Database.Word> words;
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
                {
                    words = db.Table<Database.Word>().Where(w => w.LessonId == Lesson.Id).ToList();
                }
                foreach (int level in words.OrderBy(w => w.Level).Select(w => w.Level).Distinct())
                {
                    Bars.Add(new Bar { Level = level, Percentage = (float)words.Count(w => w.Level == level)*100 / words.Count});
                }
            }
            LessonChosen = !LessonChosen;
            OnPropertyChanged("LessonChosen");
        }

        public AppCommand ChangeLessonChosenCmd { get; private set; }
        public ObservableCollection<Bar> Bars { get; private set; } 
        public bool LessonChosen { get; private set; }
    }
}
