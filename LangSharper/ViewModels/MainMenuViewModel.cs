using System.IO;

namespace LangSharper.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        public MainMenuViewModel()
        {
            var path = Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            ManageLessonsCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<ManageLessonsViewModel>(); });
            SimpleLearningCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<SimpleLearningViewModel>(); });
            WriteLearningCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<WriteLearningViewModel>(); });
            StatisticsCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<StatisticsViewModel>(); });
        }

        public AppCommand ManageLessonsCmd { get; private set; }
        public AppCommand SimpleLearningCmd { get; private set; }
        public AppCommand WriteLearningCmd { get; private set; }
        public AppCommand StatisticsCmd { get; private set; }
    }
}