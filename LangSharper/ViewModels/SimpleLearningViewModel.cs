using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
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
            LevelsToChoose = Enumerable.Range(0, 8).ToList();
        }

        public override void OnViewActivate()
        {
            base.OnViewActivate();
            _allWords = new List<ExtendedWord>();
        }

        void ChangeLearningState()
        {
            if (Lesson == null)
            {
                ShowError("ExNoLessonChosen");
                return;
            }

            if (SelectedLevel == null)
            {
                ShowError("ExNoWordsLevelChosen");
                return;
            }

            if (!LessonStarted)
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
                {
                    foreach (var word in db.Table<Database.Word>().Where(w => w.LessonId == Lesson.Id))
                    {
                        _allWords.Add(new ExtendedWord(word) { CorrectlyAnswered = ExtendedWord.AnswerType.NotYet });
                    }
                }
                if (_allWords.Count(w => w.Level == SelectedLevel) == 0)
                {
                    ShowError("ExWrongWordsLevelChosen");
                    _allWords.Clear();
                    return;
                }
                LessonStarted = true;
                Globals.Shuffle(_allWords);

                Debug.WriteLine("Kolejnosc wszystkich: ");
                foreach (var word in _allWords)
                {
                    Debug.WriteLine(word.DefinitionLang1 + " " + word.DefinitionLang2);
                }
                Debug.WriteLine("Koniec");

                Debug.WriteLine("Kolejnosc: ");
                foreach (var word in _allWords.Where(w => w.Level == SelectedLevel))
                {
                    Debug.WriteLine(word.DefinitionLang1 + " " + word.DefinitionLang2); 
                }
                Debug.WriteLine("Koniec");
                _currentWordIndex = _allWords.FindIndex(w => w.Level == SelectedLevel);
                SetRandomSubCollection(Globals.WordsToChooseCount);
            }
            else
            {
                LessonStarted = false;
                _allWords.Clear();
            }
            OnPropertyChanged("LessonStarted");
            OnPropertyChanged("WordToTranslate");
            OnPropertyChanged("WordsRemaining");
        }

        async void ChooseAnswer(object inWord)
        {
            var word = inWord as ExtendedWord;
            Debug.WriteLine("1 " + word.DefinitionLang1 + " " + word.CorrectlyAnswered + " " + WordsRemaining);
            if (word.Id == WordToTranslate.Id)
            {
                if (_allWords[_currentWordIndex].CorrectlyAnswered == ExtendedWord.AnswerType.NotYet)
                {
                    _allWords[_currentWordIndex].CorrectlyAnswered = ExtendedWord.AnswerType.Correct;
                    _allWords[_currentWordIndex].Level = (short)Math.Min(7, _allWords[_currentWordIndex].Level + 1);
                }
                else
                {
                    _allWords[_currentWordIndex].CorrectlyAnswered = ExtendedWord.AnswerType.CorrectButWasIncorrect;
                    _allWords[_currentWordIndex].Level = (short)Math.Min(7, 0);
                }
            }
            else
            {
                _allWords[_currentWordIndex].CorrectlyAnswered = ExtendedWord.AnswerType.Incorrect;
                WordsToChoose.First(w => w.Id == word.Id).CorrectlyAnswered = ExtendedWord.AnswerType.Incorrect;
            }
            WordsToChoose.First(w => w.Id == _allWords[_currentWordIndex].Id).CorrectlyAnswered = ExtendedWord.AnswerType.Correct;

            await Task.Delay((int) PropertyFinder.Instance.Resource["WaitTime"]);
            
            Debug.WriteLine("2 " + word.DefinitionLang1 + " " + word.CorrectlyAnswered + " " + WordsRemaining);

            if (WordsRemaining > 0)
            {
                Debug.WriteLine("Indeksy: " + _currentWordIndex);
                _currentWordIndex = _allWords.FindIndex((_currentWordIndex + 1) % _allWords.Count, 
                                                        w => w.CorrectlyAnswered < ExtendedWord.AnswerType.Correct && w.Level == SelectedLevel);
                if (_currentWordIndex == -1)
                {
                    _currentWordIndex = _allWords.FindIndex(0, w => w.CorrectlyAnswered < ExtendedWord.AnswerType.Correct 
                                                            && w.Level == SelectedLevel);
                }
                Debug.WriteLine(_currentWordIndex);
                Debug.WriteLine("Aktualny stan: ");
                foreach (var wrd in _allWords.Where(w => w.Level == SelectedLevel))
                {
                    Debug.WriteLine(wrd.DefinitionLang1 + " " + wrd.DefinitionLang2 + " " + wrd.CorrectlyAnswered + " " + wrd.Level);
                }
                Debug.WriteLine("Koniec");
                SetRandomSubCollection(Globals.WordsToChooseCount);
                OnPropertyChanged("WordsRemaining");
                OnPropertyChanged("WordToTranslate");
            }
            else
            {
                using (var db = new SQLiteConnection(new SQLitePlatformWin32(), PropertyFinder.Instance.Resource["DatabasePath"].ToString()))
                {
                    db.InsertOrReplaceAll(_allWords, typeof(Database.Word));
                }
                ChangeLearningState(); 
            }
        }

        void SetRandomSubCollection(int n)
        {
            bool isCorrectWordInserted = false;
            int randomIndex, allWordsLength = _allWords.Count;
            WordsToChoose.Clear();
            var allWordsCopy = _allWords.ToList();
            Globals.Shuffle(allWordsCopy);
            int shuffledWordIndex = allWordsCopy.IndexOf(_allWords[_currentWordIndex]);
            // We want current word after n first places in array
            Debug.WriteLine("SRSC");
            Debug.WriteLine(allWordsCopy[n].DefinitionLang1);
            Globals.Swap(allWordsCopy, shuffledWordIndex, n);
            Debug.WriteLine(allWordsCopy[n].DefinitionLang1);

            for (int i = 0; i < n; ++i)
            {
                if (!isCorrectWordInserted && (Globals.Random.NextDouble() < 1.0/n || i == n - 1))
                {
                    WordsToChoose.Add(new ExtendedWord(allWordsCopy[n]) { CorrectlyAnswered = ExtendedWord.AnswerType.NotYet });
                    isCorrectWordInserted = true;
                }
                else
                {
                    WordsToChoose.Add(new ExtendedWord(allWordsCopy[i]) { CorrectlyAnswered = ExtendedWord.AnswerType.NotYet });
                }
            }
        }

        List<ExtendedWord> _allWords;
        int _currentWordIndex;
        public AppCommand ChooseAnswerCmd { get; private set; }
        public AppCommand ChangeLearningStateCmd { get; private set; }
        public bool LessonStarted { get; private set; }
        public ExtendedWord WordToTranslate { get { return LessonStarted ? _allWords[_currentWordIndex] : null; }}
        public int WordsRemaining { get { return _allWords.Count(w => w.Level == SelectedLevel 
                                                                 && w.CorrectlyAnswered < ExtendedWord.AnswerType.Correct); }}
        public List<int> LevelsToChoose { get; private set; }
        public int? SelectedLevel { get; set; }
        public ObservableCollection<ExtendedWord> WordsToChoose { get; private set; }
    }
}
