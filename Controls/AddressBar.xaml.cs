using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagLinx._01._00.Controls;

public sealed partial class AddressBar : UserControl
{
    #region instance field
    #endregion

    #region static field
    public static readonly DependencyProperty CurrentPathProperty =
    DependencyProperty.Register
        (
            nameof(CurrentPath),
            typeof(string),
            typeof(AddressBar),
            new PropertyMetadata(@"C:\")
        );
    #endregion

    #region const field
    #endregion

    #region instance property
    public string CurrentPath
    {
        get => (string)GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }
    #endregion

    #region static property
    #endregion

    #region const property
    #endregion

    private List<string> Cats = new List<string>()
    {
        "Abyssinian",
        "Aegean",
        "American Bobtail"
    };

    public AddressBar()
    {
        InitializeComponent();
    }

    private void AddressBar_AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        // Since selecting an item will also change the text,
        // only listen to changes caused by user entering text.
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            var suitableItems = new List<string>();
            var splitText = sender.Text.ToLower().Split(" ");
            foreach (var cat in Cats)
            {
                var found = splitText.All((key) =>
                {
                    return cat.ToLower().Contains(key);
                });
                if (found)
                {
                    suitableItems.Add(cat);
                }
            }
            if (suitableItems.Count == 0)
            {
                suitableItems.Add("No results found");
            }
            sender.ItemsSource = suitableItems;
        }

    }

    private void AddressBar_AutoSuggestBox_KeyDown(object sender, KeyRoutedEventArgs e)
    {

    }

    private void AddressBar_AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        AddressBar_AutoSuggestBox.Text = args.SelectedItem.ToString();
    }
}
