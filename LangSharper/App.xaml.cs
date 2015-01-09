using System.Collections.Generic;
using System.Windows;
using LangSharper.ViewModels;

namespace LangSharper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            Application.Current.Properties["UiTexts"] = new UiTexts(Globals.Path + Globals.UiTextFileName);
            Application.Current.Properties["Database"] = new Database(Globals.AppName, Globals.Path + Globals.DatabaseFileName);
            Application.Current.Properties["DatabasePath"] = Globals.Path + Globals.DatabaseFileName;
            Application.Current.Properties["ViewModelStack"] = new Stack<BaseViewModel>();
            Application.Current.Properties["WaitTime"] = 1500;
        }
    }
}
