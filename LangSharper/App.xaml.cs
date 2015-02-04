using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Windows;
using LangSharper.ViewModels;

namespace LangSharper
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>

    public partial class App : Application
    {
        private void LoadSettings()
        {
            string[] settingIndices = {"DatabaseLibraryName", "DatabaseFileName", "Path"};
            try
            {
                var settings = ConfigurationManager.AppSettings;
                foreach (var index in settingIndices)
                {
                    if (settings[index] == null)
                    {
                        throw new ConfigurationErrorsException(string.Format("Couldn't find {0} in app settings", index));
                    }
                    Current.Properties[index] = settings[index];
                }
            }
            catch (ConfigurationErrorsException e)
            {
                MessageBox.Show(e.Message, "An error occurred", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(Globals.ErrorCode);
            }
            Current.Properties["UiTexts"] = new UiTexts(Current.Properties["Path"] + Globals.UiTextFileName);
            Current.Properties["Database"] = new Database(Globals.AppName, Current.Properties["Path"].ToString() + Current.Properties["DatabaseFileName"]);
            Current.Properties["DatabasePath"] = Current.Properties["Path"].ToString() + Current.Properties["DatabaseFileName"];
            Current.Properties["ViewModelStack"] = new Stack<BaseViewModel>();
            Current.Properties["WaitTime"] = 1500;
        }

        private void App_OnStartup(object sender, StartupEventArgs e)
        {
            LoadSettings();
        }
    }
}
