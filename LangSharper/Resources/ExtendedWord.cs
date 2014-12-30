using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using LangSharper.Annotations;

namespace LangSharper.Resources
{
    public class ExtendedWord : INotifyPropertyChanged
    {
        public Database.Word Word { get; private set; }
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
                if (lesson.Name == null || HasImage == false)
                {
                    return new Uri(Path.GetFullPath(Globals.Path + "NullImage.png"));
                }
                if (_lastImagePath != null && _lastImagePath.OriginalString !=
                    Word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson))
                {
                    File.Move(_lastImagePath.AbsolutePath,
                              Word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson));
                }
                _lastImagePath = new Uri(Word.GetImagePath(PropertyFinder.Instance.Resource["CurrentUser"] as Database.User, lesson));
                return _lastImagePath;
            }
        }

        public void ImagePathChanged()
        {
            OnPropertyChanged("ImagePath"); 
        }

        public bool HasImage
        {
            get { return Word.HasImage; }
            set { Word.HasImage = value; OnPropertyChanged();}
        }

        public ExtendedWord(Database.Word word)
        {
            Word = word;
            _lastImagePath = null;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}