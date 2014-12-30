using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using LangSharper.Resources;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

// @TODO remove words

namespace LangSharper.ViewModels
{
    public class CreateModifyLessonsViewModel : BaseViewModel
    {
        public CreateModifyLessonsViewModel()
        {
            PreviousCmd = new AppCommand(() => { PropertyFinder.Instance.CurrentModel = GetViewModel<ManageLessonsViewModel>(); });
            ChangeLessonNameCmd = new AppCommand(ChangeLessonName, () => !string.IsNullOrEmpty(NewName));
            ShowChangeLessonNameSectionCmd = new AppCommand(ShowChangeLessonSectionName, () => IsChangeNameVisible.Equals(false));
            DropImageCmd = new AppCommand(DropImage);
            AddWordItemCmd = new AppCommand(AddWordItem, () => ExtendedWords.All(w => !string.IsNullOrEmpty(w.Word.DefinitionLang1) 
                                                                                 && !string.IsNullOrEmpty(w.Word.DefinitionLang2)));
        }

        public AppCommand PreviousCmd { get; private set; }
        public AppCommand ChangeLessonNameCmd { get; private set; }
        public AppCommand ShowChangeLessonNameSectionCmd { get; private set; }
        public AppCommand DropImageCmd { get; private set; }
        public AppCommand AddWordItemCmd { get; private set; }

        public override void OnViewActivate()
        {
            NewName = null;
            IsErrorVisible = false;
            if (GetViewModel<ManageLessonsViewModel>().SelectedLesson == null)
            {
                Lesson = new Database.Lesson { Name = null, UserId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id };
                IsChangeNameVisible = true;
                ExtendedWords = new ObservableCollection<ExtendedWord>
                {
                    new ExtendedWord(new Database.Word { LessonId = Lesson.Id })
                };
            }
            else
            {
                Lesson = GetViewModel<ManageLessonsViewModel>().SelectedLesson;
                IsChangeNameVisible = false;
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
                {
                    ExtendedWords = new ObservableCollection<ExtendedWord>();
                    foreach (var word in db.Table<Database.Word>().Where(w => w.LessonId == Lesson.Id))
                    {
                           ExtendedWords.Add(new ExtendedWord(word));
                    }
                }
            }
        }

        void DropImage(object param)
        {
            if (!(param is Tuple<string, string, IDataObject>))
                return;
            
            string def1 = ((Tuple<string, string, IDataObject>) param).Item1;
            string def2 = ((Tuple<string, string, IDataObject>) param).Item2;
            DataObject dataObject = (DataObject) ((Tuple<string, string, IDataObject>) param).Item3;

            if (dataObject == null || !dataObject.ContainsFileDropList() 
                || dataObject.GetFileDropList().Count != 1 || !dataObject.GetFileDropList()[0].EndsWith(".png"))
            {
                ShowError("ExWrongItemDropped");
                return;
            }

            if (def1 == null || def2 == null)
            {
                ShowError("ExDefinitionsFirst");
                return;
            }

            var currentWord = ExtendedWords.First(w => w.Word.DefinitionLang1 == def1 && w.Word.DefinitionLang2 == def2);
            currentWord.HasImage = true;
            try
            {
                if (File.Exists(currentWord.ImagePath.AbsolutePath))
                {
                    File.Delete(currentWord.ImagePath.AbsolutePath);
                }
                File.Copy(dataObject.GetFileDropList()[0], currentWord.ImagePath.AbsolutePath);
                currentWord.ImagePathChanged();
            }
            catch (Exception e)
            {
                currentWord.HasImage = false;
                ShowError(e.Message); 
            }
        }

        void ChangeLessonName()
        {
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString())) 
            {
                if (db.Table<Database.Lesson>().Any(l => l.Name == NewName 
                    && l.UserId == (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id))
                {
                    ShowError("ExLessonNameDuplicate");
                    return;
                }
            }
            if (Lesson.Name != null)
            {
                if (Lesson.Name == NewName)
                {
                    IsChangeNameVisible = false;
                    OnPropertyChanged("IsChangeNameVisible");
                }
                Directory.Move(Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name, Lesson.Name), 
                               Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name, NewName));
            }
            else
            {
                Directory.CreateDirectory(Path.Combine(Globals.ResourcePath, (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Name, NewName));
            }
            Lesson.Name = NewName;
            OnPropertyChanged("Lesson");
            IsChangeNameVisible = false;
            OnPropertyChanged("IsChangeNameVisible");
        }

        void ShowChangeLessonSectionName()
        {
            IsChangeNameVisible = true;
            OnPropertyChanged("IsChangeNameVisible");
        }

        void AddWordItem()
        {
            if (ExtendedWords.Count == Globals.MaxWordsForLesson)
            {
                ShowError("ExWordNumberLimitReached");
                return;
            }
            ExtendedWords.Add(new ExtendedWord(new Database.Word { LessonId = Lesson.Id }));  
        }

        public string NewName { get; set; }
        public bool IsChangeNameVisible { get; set; }
        public Database.Lesson Lesson { get; private set; }
        public ObservableCollection<ExtendedWord> ExtendedWords { get; private set; }
    }
}