using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using LangSharper.Annotations;

namespace LangSharper.ViewModels
{
    public class BaseViewModel: INotifyPropertyChanged
    {
        public static readonly Dictionary<Type, BaseViewModel> ViewModelsDict = new Dictionary<Type, BaseViewModel>()
        {
            {typeof (StartViewModel), new StartViewModel()},
            {typeof (MainMenuViewModel), new MainMenuViewModel()}
        };

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
