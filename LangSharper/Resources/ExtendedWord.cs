using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using LangSharper.Annotations;

namespace LangSharper.Resources
{
    public class ExtendedWord : Database.Word, INotifyPropertyChanged
    {
        Uri _lastImagePath;

        public Uri ImagePath {
            get
            {
                Type type = PropertyFinder.Instance.CurrentModel.GetType();
                if (type.GetProperty("Lesson") == null)
                {
                    throw new Exception("ExWrongViewForAction");
                }

                var lesson = type.GetProperty("Lesson").GetValue(PropertyFinder.Instance.CurrentModel, null) as Database.Lesson;
                if (lesson == null || lesson.Name == null || HasImage == false)
                {
                    return new Uri(Path.GetFullPath(Globals.Path + "NullImage.png"));
                }
                if (_lastImagePath != null && _lastImagePath.OriginalString !=
                    GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson))
                {
                    File.Move(_lastImagePath.AbsolutePath,
                              GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson));
                }
                _lastImagePath = new Uri(GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson));
                return _lastImagePath;
            }
        }

        public void ImagePathChanged()
        {
            OnPropertyChanged("ImagePath"); 
        }

        public ExtendedWord()
        {
            _lastImagePath = null;
            DefLang1 = () => OnPropertyChanged("DefinitionLang1");
            DefLang2 = () => OnPropertyChanged("DefinitionLang2");
            HasImg = () => OnPropertyChanged("HasImage");
        }

        public ExtendedWord(bool isNew) : this()
        {
            IsNew = isNew;
        }

        public ExtendedWord(Database.Word w) : this()
        {
            Id = w.Id;
            DefinitionLang1 = w.DefinitionLang1;
            DefinitionLang2 = w.DefinitionLang2;
            HasImage = w.HasImage;
            LessonId = w.LessonId;
            Level = w.Level;
        }

        public bool IsNew { get; set; }
        public bool CorrectlyAnswered { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}