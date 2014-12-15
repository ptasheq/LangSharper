using System.Windows;
using LangSharper.ViewModels;

namespace LangSharper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            Application.Current.Properties["ViewModel"] = BaseViewModel.GetViewModel<StartViewModel>();
            DataContext = BaseViewModel.GetViewModel<StartViewModel>();
            InitializeComponent();
        }
    }
}
