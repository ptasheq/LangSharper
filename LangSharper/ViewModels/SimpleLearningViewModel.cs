
namespace LangSharper.ViewModels
{
    public class SimpleLearningViewModel : BaseViewModel
    {
        public SimpleLearningViewModel()
        {
            StartLessonCmd = new AppCommand(StartLesson);
        }

        public override void OnViewActivate()
        {
            SelectedLesson = null;
        }

        public void StartLesson()
        {
            
        }

        public Database.Lesson SelectedLesson { get; set; }
        public AppCommand StartLessonCmd { get; private set; }
    }
}
