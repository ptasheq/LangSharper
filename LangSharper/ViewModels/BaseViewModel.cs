using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using LangSharper.Annotations;

namespace LangSharper.ViewModels
{
    public abstract class BaseViewModel: INotifyPropertyChanged
    {
        static readonly Dictionary<Type, BaseViewModel> _viewModelsDict = new Dictionary<Type, BaseViewModel>()
        {
            {typeof (StartViewModel), null},
            {typeof (MainMenuViewModel), null},
            {typeof (ManageLessonsViewModel), null},
            {typeof (CreateModifyLessonsViewModel), null},
            {typeof (SimpleLearningViewModel), null},
            {typeof (WriteLearningViewModel), null},
            {typeof (StatisticsViewModel), null}
        };

        public static T GetViewModel<T>() where T : class, new()
        {
            return _viewModelsDict[typeof (T)] as T  ?? (_viewModelsDict[typeof (T)] = new T() as BaseViewModel) as T;
        }

        protected BaseViewModel()
        {
            IsErrorVisible = false;
            HideError = new AppCommand(() => { IsErrorVisible = false; OnPropertyChanged("IsErrorVisible"); });
            PreviousCmd = new AppCommand(() => PropertyFinder.Instance.ReturnToPreviousModel());
        }

        public UiTexts Texts { get { return PropertyFinder.Instance.GetResource("UiTexts") as UiTexts;} } 
        public string ErrorMessage { get; protected set; }
        public bool IsErrorVisible { get; protected set; }

        public AppCommand HideError { get; protected set; }
        public AppCommand PreviousCmd { get; protected set; }
            
        protected void ShowError(string key, params object[] args)
        {
            ErrorMessage = (args.Length > 0) ? string.Format(Texts.Dict[key], args) : Texts.Dict[key];
            IsErrorVisible = true;
            OnPropertyChanged("ErrorMessage");
            OnPropertyChanged("IsErrorVisible");
        }

        public virtual void OnViewActivate()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
