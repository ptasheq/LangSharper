using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LangSharper.Annotations;

namespace LangSharper.ViewModels
{
    public class BaseViewModel: INotifyPropertyChanged
    {
        static readonly Dictionary<Type, BaseViewModel> _viewModelsDict = new Dictionary<Type, BaseViewModel>()
        {
            {typeof (StartViewModel), null},
            {typeof (MainMenuViewModel), null},
            {typeof (ManageLessonsViewModel), null},
            {typeof (SimpleLearningViewModel), null},
            {typeof (WriteLearningViewModel), null},
            {typeof (StatisticsViewModel), null}
        };

        public static BaseViewModel GetViewModel<T>() where T : new()
        {
            if (_viewModelsDict[typeof (T)] == null) {
                _viewModelsDict[typeof (T)] = new T() as BaseViewModel;  
            }
            return _viewModelsDict[typeof (T)];
        }

        protected BaseViewModel()
        {
        }

        public UiTexts Texts {get { return PropertyFinder.Instance.GetResource("UiTexts") as UiTexts;}} 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
