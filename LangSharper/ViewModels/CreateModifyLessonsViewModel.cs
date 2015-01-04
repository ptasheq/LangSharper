using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
            AddWordItemCmd = new AppCommand(() => AddWordItem(), () => ExtendedWords.All(w => !string.IsNullOrEmpty(w.DefinitionLang1) 
                                                                                         && !string.IsNullOrEmpty(w.DefinitionLang2)));
            ConfirmChangesCmd = new AppCommand(ConfirmChanges);
        }

        public AppCommand PreviousCmd { get; private set; }
        public AppCommand ChangeLessonNameCmd { get; private set; }
        public AppCommand ShowChangeLessonNameSectionCmd { get; private set; }
        public AppCommand DropImageCmd { get; private set; }
        public AppCommand AddWordItemCmd { get; private set; }
        public AppCommand ConfirmChangesCmd { get; private set; }

        public override void OnViewActivate()
        {
            NewName = null;
            IsErrorVisible = false;
            if (GetViewModel<ManageLessonsViewModel>().SelectedLesson == null)
            {
                Lesson = new Database.Lesson { Name = null, UserId = (PropertyFinder.Instance.Resource["CurrentUser"] as Database.User).Id };
                IsChangeNameVisible = true;
                ExtendedWords = new ObservableCollection<ExtendedWord> { new ExtendedWord() }; 
            }
            else
            {
                Lesson = GetViewModel<ManageLessonsViewModel>().SelectedLesson;
                IsChangeNameVisible = false;
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
                {
                    ExtendedWords = new ObservableCollection<ExtendedWord>();
                    foreach (var word in db.Table<Database.Word>().Where(w => w.LessonId == Lesson.Id).OrderBy(e => e.Id))
                    {
                        AddWordItem(word);
                    }
                }
            }
            _areChangesSaved = true;
        }

        void ConfirmChanges()
        {
            if (_areChangesSaved)
            {
                ShowError("ExNoChangesToConfirm");
                return;
            }

            if (ExtendedWords.Count < Globals.MinWordsForLesson)
            {
                ShowError("ExNotEnoughWords", Globals.MinWordsForLesson);
                return;
            }

            try
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString())) 
                {
                    db.RunInTransaction(() =>
                    {
                        db.InsertOrReplaceAll(ExtendedWords.Where(w => !w.IsNew), typeof(Database.Word));
                        foreach (var word in ExtendedWords.Where(w => w.IsNew))
                        {
                            db.Insert(word, typeof(Database.Word));
                            word.IsNew = false;
                        } 
                    });
                }
                _areChangesSaved = true;
            }
            catch (SQLiteException e)
            {
                ShowError(e.Message.Contains("Constraint") ? "ExDefinitionsFirst" : "ExUnknownError");
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

            List<ExtendedWord> currentWords = ExtendedWords.Where(w => w.DefinitionLang1 == def1 && w.DefinitionLang2 == def2).ToList();
            if (currentWords.Count() > 1)
            {
               ShowError("ExDropImageDefinitions");
               return;
            }

            var areChangesSavedTmp = _areChangesSaved;
            currentWords[0].HasImage = true;
            try
            {
                if (File.Exists(currentWords[0].ImagePath.AbsolutePath))
                {
                    File.Delete(currentWords[0].ImagePath.AbsolutePath);
                }
                File.Copy(dataObject.GetFileDropList()[0], currentWords[0].ImagePath.AbsolutePath);
                currentWords[0].ImagePathChanged();
            }
            catch (Exception e)
            {
                currentWords[0].HasImage = false;
                _areChangesSaved = areChangesSavedTmp;
                ShowError(e.Message); 
            }
        }

        void ChangeLessonName()
        {
            var user = PropertyFinder.Instance.Resource["CurrentUser"] as Database.User;
            using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
            {
                if (db.Table<Database.Lesson>().Any(l => l.Name == NewName && l.UserId == user.Id))
                {
                    ShowError("ExLessonNameDuplicate");
                    return;
                }
                if (Lesson.Name != null)
                {
                    if (Lesson.Name == NewName)
                    {
                        IsChangeNameVisible = false;
                        OnPropertyChanged("IsChangeNameVisible");
                    }
                    Directory.Move(Path.Combine(Globals.ResourcePath, user.Name, Lesson.Name),
                                   Path.Combine(Globals.ResourcePath, user.Name, NewName));
                    Lesson.Name = NewName;
                    db.InsertOrReplace(Lesson);
                }
                else
                {
                    Directory.CreateDirectory(Path.Combine(Globals.ResourcePath, user.Name, NewName));
                    Lesson.Name = NewName;
                    db.Insert(Lesson);
                    ExtendedWords.RemoveAt(0);
                    AddWordItem();
                }
            }
            OnPropertyChanged("Lesson");
            IsChangeNameVisible = false;
            OnPropertyChanged("IsChangeNameVisible");
        }

        void ShowChangeLessonSectionName()
        {
            IsChangeNameVisible = true;
            OnPropertyChanged("IsChangeNameVisible");
        }

        void AddWordItem(Database.Word word = null)
        {
            if (ExtendedWords.Count == Globals.MaxWordsForLesson)
            {
                ShowError("ExWordNumberLimitReached");
                return;
            }
            ExtendedWords.Add(word == null ? new ExtendedWord(true) { LessonId = Lesson.Id } : new ExtendedWord(word));  
            ExtendedWords[ExtendedWords.Count-1].PropertyChanged += delegate(object sender, PropertyChangedEventArgs args)
            {
                if (args.PropertyName == "DefinitionLang1" 
                    || args.PropertyName == "DefinitionLang2" || args.PropertyName == "HasImage")
                {
                    _areChangesSaved = false;
                }
            };
        }

        bool _areChangesSaved;
        public string NewName { get; set; }
        public bool IsChangeNameVisible { get; set; }
        public Database.Lesson Lesson { get; private set; }
        public ObservableCollection<ExtendedWord> ExtendedWords { get; private set; }
    }
}