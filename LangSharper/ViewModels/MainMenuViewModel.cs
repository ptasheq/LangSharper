﻿
namespace LangSharper.ViewModels
{
    public class MainMenuViewModel : BaseViewModel
    {
        public MainMenuViewModel()
        {
            ManageLessonsCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<ManageLessonsViewModel>(); });
            SimpleLearningCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<SimpleLearningViewModel>(); });
            WriteLearningCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<WriteLearningViewModel>(); });
            StatisticsCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<StatisticsViewModel>(); });
            ReturnToLoginCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<StartViewModel>(); });
        }

        public AppCommand ManageLessonsCmd { get; private set; }
        public AppCommand SimpleLearningCmd { get; private set; }
        public AppCommand WriteLearningCmd { get; private set; }
        public AppCommand StatisticsCmd { get; private set; }
        public AppCommand ReturnToLoginCmd { get; private set; }
    }
}