using System.Windows;

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
            Application.Current.Properties["Database"] = new Database(Globals.Path + Globals.DatabaseFileName);
            Application.Current.Properties["DatabasePath"] = Globals.Path + Globals.DatabaseFileName;
        }
    }
}
