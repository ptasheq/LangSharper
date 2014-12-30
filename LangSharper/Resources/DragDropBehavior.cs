using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace LangSharper.Resources
{
    public static class DragDropBehavior
    {
        static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.RegisterAttached("DropCommand", typeof (ICommand), typeof (DragDropBehavior),
                                                new PropertyMetadata(OnDropCommandPropertyChanged));

        public static void SetDropCommand(this UIElement inUIElement, ICommand inCommand)
        {
            inUIElement.SetValue(DropCommandProperty, inCommand);
        }

        static ICommand GetDropCommand(UIElement inUiElement)
        {
            return (ICommand) inUiElement.GetValue(DropCommandProperty);
        }

        static void OnDropCommandPropertyChanged(DependencyObject inObj, DependencyPropertyChangedEventArgs depArgs)
        {
            UIElement uiElement = inObj as UIElement;
            if (uiElement == null) return;

            uiElement.Drop += (sender, args) =>
            {
                var word = ((sender as FrameworkElement).DataContext as ExtendedWord).Word;
                GetDropCommand(uiElement).Execute(new Tuple<string, string, IDataObject> (word.DefinitionLang1, word.DefinitionLang2, args.Data));
                args.Handled = true;
            };
        }
    }
}
