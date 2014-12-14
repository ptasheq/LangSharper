using System;
using System.Collections.ObjectModel;
using System.Linq;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public class StartViewModel : BaseViewModel
    {
        public StartViewModel()
        {
            AddUserVisible = false; 
            ShowAddUserControlsCmd = new AppCommand(() => { AddUserVisible = true; OnPropertyChanged("AddUserVisible"); });
            AddUserCmd = new AppCommand(AddUser, CanExecuteAddUser);
            ChooseUserCmd = new AppCommand(ChooseUser, CanExecuteChooseUser);
            Users = new ObservableCollection<Database.User>();
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                foreach (var user in db.Table<Database.User>())
                {
                    Users.Add(user);        
                }
            }
        }

        private void AddUser()
        {
            var newUser = new Database.User() { Name = NewUserName };
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                db.Insert(newUser);
                Users.Add(newUser);
                OnPropertyChanged("Users");
            }   
        }

        private bool CanExecuteAddUser()
        {
            return NewUserName != null && NewUserName.Length > 2 && Users.All(u => u.Name != NewUserName);
        }

        private void ChooseUser()
        {
            PropertyFinder.Instance.Resource["CurrentUserId"] = Users[UserIndex].Id;
            PropertyFinder.Instance.CurrentModel = ViewModelsDict[typeof (MainMenuViewModel)];
        }

        private bool CanExecuteChooseUser()
        {
            return UserIndex > -1;
        }

        public bool AddUserVisible { get; private set; }
        public String NewUserName { get; set; }
        public int UserIndex { get; set; }
        public ObservableCollection<Database.User> Users { get; set; } 
        public AppCommand ShowAddUserControlsCmd { get; private set; }
        public AppCommand AddUserCmd { get; private set; }
        public AppCommand ChooseUserCmd { get; private set; }
    }
}
