
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using LangSharper.Resources;
using SQLite.Net;
using SQLite.Net.Platform.Win32;

namespace LangSharper.ViewModels
{
    public class SimpleLearningViewModel : BaseLessonListViewModel
    {
        public SimpleLearningViewModel()
        {
            ChangeLearningStateCmd = new AppCommand(ChangeLearningState);
            ChooseAnswerCmd = new AppCommand(ChooseAnswer);
            WordsToChoose = new ObservableCollection<ExtendedWord>();
        }

        public override void OnViewActivate()
        {
            base.OnViewActivate();
            _allWords = new List<ExtendedWord>();
        }

        void ChangeLearningState()
        {
            if (SelectedLesson == null)
            {
                ShowError("ExNoLessonChosen");
                return;
            }

            if (!LessonStarted)
            {
                LessonStarted = true;
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
                {
                    foreach (var word in db.Table<Database.Word>().Where(w => w.LessonId == SelectedLesson.Id))
                    {
                        _allWords.Add(new ExtendedWord(word) { CorrectlyAnswered = false });
                    }
                }
                Globals.Shuffle(_allWords);
                _currentWordIndex = 0;
                SetRandomSubCollection(Globals.WordsToChooseCount);
            }
            else
            {
                LessonStarted = false;
                _allWords.Clear();
            }
            OnPropertyChanged("LessonStarted");
            OnPropertyChanged("WordsRemaining");
        }

        void ChooseAnswer(object inWord)
        {
            var word = inWord as ExtendedWord;
            if (word.Id == WordToTranslate.Id)
            {
                _allWords[_currentWordIndex].CorrectlyAnswered = true;
            }
        }

        void SetRandomSubCollection(int n)
        {
            bool isCorrectWordInserted = false;
            int randomIndex, allWordsLength = _allWords.Count;
            WordsToChoose.Clear();

            for (int i = 0; i < n; ++i)
            {
                if (!isCorrectWordInserted && (Globals.Random.NextDouble() < 1.0/n || i == n - 1))
                {
                    WordsToChoose.Add(_allWords[_currentWordIndex]);
                    isCorrectWordInserted = true;
                }
                else
                {
                    while ((randomIndex = Globals.Random.Next() % allWordsLength) == _currentWordIndex)
                    {
                    }
                    WordsToChoose.Add(_allWords[randomIndex]);
                }
            }
        }

        List<ExtendedWord> _allWords;
        int _currentWordIndex;
        public AppCommand ChooseAnswerCmd { get; private set; }
        public AppCommand ChangeLearningStateCmd { get; private set; }
        public bool LessonStarted { get; private set; }
        public ExtendedWord WordToTranslate { get { return _allWords[_currentWordIndex]; }}
        public int WordsRemaining { get { return _allWords.Count(w => !w.CorrectlyAnswered); }}
        public ObservableCollection<ExtendedWord> WordsToChoose { get; private set; } 
    }
}
