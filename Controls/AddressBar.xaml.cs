using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Xaml.Shapes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using TagLinx._01._00.Json;
using Windows.ApplicationModel.UserDataTasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Protection.PlayReady;
using Windows.Storage.Search;
using Read = WinUI3.Read;
using Update = WinUI3.Update;
using MethodLog = WinUI3.Common.MethodLog;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagLinx._01._00.Controls;

public sealed partial class AddressBar : UserControl
{
    private string oldCurrentPath = string.Empty;

    // **************************************************
    //  初期化
    // **************************************************
    public AddressBar()
    {
        InitializeComponent();
    }
    private MainWindow? mainWindow;
    private FileList? fileList;
    private BreadcrumbAddressBar? breadcrumbAddressBar;
    internal void Initialize(MainWindow mainWindow,FileList fileList,BreadcrumbAddressBar breadcrumbAddressBar)
    {
        using var log = MethodLog.Start<AddressBar>();
        this.mainWindow = mainWindow;
        this.fileList = fileList;
        this.breadcrumbAddressBar = breadcrumbAddressBar;
        if (this.mainWindow.pathConfig is null || string.IsNullOrWhiteSpace(this.mainWindow.pathConfig.History))
        {
            return;
        }
        ReadHistory();
        ReadHistoryList();
        // 候補を設定
        AddressBar_AutoSuggestBox.ItemsSource = historyList;
    }

    // **************************************************
    //  イベントハンドラー
    // **************************************************    
    /// <summary>
    /// 選択時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AddressBar_AutoSuggestBox_GettingFocus(UIElement sender, GettingFocusEventArgs args)
    {
        using var log = MethodLog.Start<AddressBar>();
        // oldCurrentPathをセット
        oldCurrentPath = CurrentPath;
        choiceFlag = false;
        SubmittedFlag = false;
    }

    /// <summary>
    /// 選択時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddressBar_AutoSuggestBox_GotFocus(object sender, RoutedEventArgs e)
    {
        using var log = MethodLog.Start<AddressBar>();
        // このコントロールの UI スレッドで実行する処理を、キューに登録
        AddressBar_AutoSuggestBox.DispatcherQueue.TryEnqueue(() =>
        {
            TextBox? textBox = FindTextBox(AddressBar_AutoSuggestBox);
            if (textBox is null || textBox.FocusState == FocusState.Unfocused)
            {
                return;
            }
            // 入力開始時に最新の履歴を候補へ反映
            AddressBar_AutoSuggestBox.ItemsSource = historyList.ToArray();

            // 全選択
            textBox.SelectAll();

            // パンくずリストを非表示
            breadcrumbAddressBar!.Hide();

            // 候補を開く
            AddressBar_AutoSuggestBox.IsSuggestionListOpen = true;
        });
    }

    /// <summary>
    /// 選択解除時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void AddressBar_AutoSuggestBox_LostFocus(object sender, RoutedEventArgs e)
    {
        using var log = MethodLog.Start<AddressBar>();
        try
        {
            if (choiceFlag || SubmittedFlag)
            {
                Debug.WriteLine($"{choiceFlag}:{SubmittedFlag}");
                return;
            }
            Debug.WriteLine($"{oldCurrentPath}");
            AddressBar_AutoSuggestBox.Text = oldCurrentPath;
        }
        finally
        {
            breadcrumbAddressBar!.Show();
        }
    }

    /// <summary>
    /// テキスト変更時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AddressBar_AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    
    {
        using var log = MethodLog.Start<AddressBar>();
        if (!choiceFlag && !SubmittedFlag)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                sender.ItemsSource = historyList.Where(item => item.Contains(sender.Text, StringComparison.OrdinalIgnoreCase)).ToList();
            }
            return;
        }

        if (choiceFlag && SubmittedFlag)
        {
            //fileList!.ThisFocus();
            return;
        }
    }

    private bool choiceFlag = false;
    /// <summary>
    /// 候補選択時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AddressBar_AutoSuggestBox_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
    {
        using var log = MethodLog.Start<AddressBar>();
        sender.Text = args.SelectedItem.ToString();
        choiceFlag = true;
    }

    private bool SubmittedFlag = false;
    /// <summary>
    /// 確定時
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="args"></param>
    private void AddressBar_AutoSuggestBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        using var log = MethodLog.Start<AddressBar>();
        if (Directory.Exists(sender.Text))
        {
            EditHistory(sender.Text);
            UpdateHistoryList(sender.Text);
            SubmittedFlag = true;
            CurrentPath = sender.Text;
        }
        else
        {
            if (File.Exists(sender.Text))
            {
                // ファイルを開く
            }
            SubmittedFlag = false;
        }
        //fileList!.ThisFocus();
    }

    // **************************************************
    //  プロパティ関連
    // **************************************************
    public static readonly DependencyProperty CurrentPathProperty =
    DependencyProperty.Register
        (
            nameof(CurrentPath),
            typeof(string),
            typeof(AddressBar),
            new PropertyMetadata(@"C:\", OnCurrentPathChanged)
        );

    public string CurrentPath
    {
        get => (string)GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }

    private static async void OnCurrentPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        using var log = MethodLog.Start<AddressBar>();
        if (d is AddressBar control)
        {

        }
    }
    // **************************************************
    //  履歴関連
    // **************************************************
    internal History? history = new();
    private List<string> historyList { get; set; } = new();
    
    // Json

    private void ReadHistory()
    {
        history = Read.FileSystem.GetJsonInClass<History>(mainWindow!.pathConfig!.History);
    }
    private void EditHistory(string newitem)
    {
        if (history?.AddressBar is not { } addressBar)
        {
            return;
        }
        addressBar.S9 = addressBar.S8;
        addressBar.S8 = addressBar.S7;
        addressBar.S7 = addressBar.S6;
        addressBar.S6 = addressBar.S5;
        addressBar.S5 = addressBar.S4;
        addressBar.S4 = addressBar.S3;
        addressBar.S3 = addressBar.S2;
        addressBar.S2 = addressBar.S1;
        addressBar.S1 = newitem;
    }
    internal void UpdateHistory()
    {
        Update.FileSystem.SetJsonInClass<History>(mainWindow!.pathConfig!.History, history!);
    }

    // List

    private void ReadHistoryList()
    {
        if (history?.AddressBar is not { } addressBar)
        {
            return;
        }
        historyList.Clear();
        foreach (string path in new[]
        {
            addressBar.S1,
            addressBar.S2,
            addressBar.S3,
            addressBar.S4,
            addressBar.S5,
            addressBar.S6,
            addressBar.S7,
            addressBar.S8,
            addressBar.S9
        })
        {
            if (!string.IsNullOrWhiteSpace(path))
            {
                historyList.Add(path);
            }
        }
    }
    private void UpdateHistoryList(string newitem)
    {
        if (historyList.Count == 9)
        {
            historyList.RemoveAt(8);
        }
        historyList.Insert(0, newitem);
    }
    // **************************************************
    //  Commmon
    // **************************************************
    /// <summary>
    /// Control内のTextBoxを取得
    /// </summary>
    /// <param name="parent"></param>
    /// <returns></returns>
    private static TextBox? FindTextBox(DependencyObject parent)
    {
        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
        {
            DependencyObject child = VisualTreeHelper.GetChild(parent, i);

            if (child is TextBox textBox)
            {
                return textBox;
            }

            TextBox? result = FindTextBox(child);

            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }
}
