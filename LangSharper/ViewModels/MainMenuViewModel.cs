
namespace LangSharper.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        public MainMenuViewModel()
        {
            ShowUserCmd = new AppCommand(ShowUser); 
        }

        public AppCommand ShowUserCmd { get; private set; }

        public void ShowUser()
        {
            PropertyFinder.Instance.CurrentModel = ViewModelsDict[typeof(StartViewModel)];
        }
    }
}
