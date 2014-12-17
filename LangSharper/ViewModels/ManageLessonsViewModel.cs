
namespace LangSharper.ViewModels
{
    public class ManageLessonsViewModel : BaseViewModel
    {
        public ManageLessonsViewModel()
        {
            SelectedLesson = null;
            PreviousCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<MainMenuViewModel>(); });
            CreateNewLessonCmd = new AppCommand(() => PropertyFinder.Instance.CurrentModel = GetViewModel<CreateModifyLessonsViewModel>());
            EditChosenLessonCmd = new AppCommand(() => PropertyFinder.Instance.CurrentModel = GetViewModel<CreateModifyLessonsViewModel>());
        }

        public Database.Lesson SelectedLesson { get; set; }

        public AppCommand CreateNewLessonCmd { get; private set; }
        public AppCommand EditChosenLessonCmd { get; private set; }
        public AppCommand DeleteChosenLessonCmd { get; private set; }
        public AppCommand PreviousCmd { get; private set; }
    }
}
