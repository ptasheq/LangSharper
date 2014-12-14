using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using LangSharper.Annotations;

namespace LangSharper
{
    public class PropertyFinder : INotifyPropertyChanged
    {
        IDictionary _dict; 
        static PropertyFinder instance;

        PropertyFinder() : this(Application.Current.Properties)
        {
        }

        PropertyFinder(IDictionary dict)
        {
            _dict = dict;
        }

        static public PropertyFinder Instance
        {
            get { return instance ?? (instance = new PropertyFinder()); }
        }

        public object GetResource(string key)
        {
            return _dict[key];
        }

        public IDictionary Resource { get { return _dict; } }

        public ViewModels.BaseViewModel CurrentModel
        {
            get { return _dict["ViewModel"] as ViewModels.BaseViewModel; }
            set { _dict["ViewModel"] = value; OnPropertyChanged(); }
        }

        static public void CreateInstance(IDictionary dict)
        {
            instance = new PropertyFinder(dict);
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
