using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static WinUI3.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagLinx._01._00.Controls;

public sealed partial class FileList : UserControl
{
    // **************************************************
    //  初期化
    // **************************************************
    public FileList()
    {
        InitializeComponent();

        // ListViewとItemを接続
        FileListView.ItemsSource = Items;
    }

    // **************************************************
    //  イベントハンドラー
    // **************************************************
    private void FileListView_ItemClick(object sender, ItemClickEventArgs e)
    {
        using var log = MethodLog.Start<FileList>();
        if (e.ClickedItem is not FileItem item)
            return;

        //フォルダなら移動
        if (item.IsFolder)
        {
            CurrentPath = item.FullPath;
        }
    }

    // **************************************************
    //  主軸
    // **************************************************
    public ObservableCollection<FileItem> Items { get; } = new();
    public async Task LoadDirectory(string path)
    {
        using var log = MethodLog.Start<FileList>();
        Items.Clear();
        if (!Directory.Exists(path))
            return;
        try
        {
            // フォルダ
            foreach (string dir in Directory.GetDirectories(path))
            {
                DirectoryInfo info = new(dir);
                Items.Add(new FileItem
                {
                    Icon = await FileSystemItemInfoProvider.GetSmallImageSourceAsync(info.FullName),
                    Name = info.Name,
                    FullPath = info.FullName,
                    WriteTime = info.LastWriteTime,
                    Type = FileSystemItemInfoProvider.GetTypeName(info.FullName),
                    IsFolder = true,
                    Size = 0,
                    Tag = "",
                    Comment = ""
                });
            }
            // ファイル
            foreach (string file in Directory.GetFiles(path))
            {
                FileInfo info = new(file);
                Items.Add(new FileItem
                {
                    Icon = await FileSystemItemInfoProvider.GetSmallImageSourceAsync(info.FullName),
                    Name = info.Name,
                    FullPath = info.FullName,
                    WriteTime = info.LastWriteTime,
                    Type = FileSystemItemInfoProvider.GetTypeName(info.FullName),
                    IsFolder = false,
                    Size = info.Length,
                    Tag = "",
                    Comment = ""
                });
            }
        }
        catch
        {
        }
    }

    // **************************************************
    //  プロパティ関連
    // **************************************************
    public static readonly DependencyProperty CurrentPathProperty =
        DependencyProperty.Register
        (
            nameof(CurrentPath),
            typeof(string),
            typeof(FileList),
            new PropertyMetadata("", OnCurrentPathChanged)
        );
    public string CurrentPath
    {
        get => (string)GetValue(CurrentPathProperty);
        set => SetValue(CurrentPathProperty, value);
    }
    private static async void OnCurrentPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        using var log = MethodLog.Start<FileList>();
        if (d is FileList control)
        {
            // itemsに追加
            await control.LoadDirectory(e.NewValue?.ToString() ?? @"C:\");
        }
    }

    // **************************************************
    //  Common
    // **************************************************
    internal void ThisFocus()
    {
        using var log = MethodLog.Start<FileList>();
        FileListView.Focus(FocusState.Programmatic);
        FileListView.SelectedIndex = 0;
    }
}

/// <summary>
/// ファイル・フォルダの情報
/// </summary>
public class FileItem
{
    /// <summary>アイコン</summary>
    public BitmapImage? Icon { get; set; }

    /// <summary>名前</summary>
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;

    /// <summary>更新日時</summary>
    public DateTime WriteTime { get; set; }
    public string WriteTimeText
    {
        get
        {
            return WriteTime.ToString("yyyy/MM/dd HH:mm");
        }
    }

    /// <summary>種類</summary>
    public string Type { get; set; } = string.Empty;
    public bool IsFolder { get; set; }

    /// <summary>サイズ</summary>
    public long Size { get; set; }
    public string SizeText
    {
        get
        {
            if (Size == 0)
            {
                return "";
            }
            return $"{Size / 1024:N0} KB";
        }
    }

    /// <summary>タグ</summary>
    public string Tag { get; set; } = string.Empty;

    /// <summary>コメント</summary>
    public string Comment { get; set; } = string.Empty;
}