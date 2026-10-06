using Microsoft.UI.Content;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml.XPath;
using Windows.Foundation;
using Windows.Foundation.Collections;
using static WinUI3.Common;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagLinx._01._00.Controls
{
    public sealed partial class BreadcrumbAddressBar : UserControl
    {
        public BreadcrumbAddressBar()
        {
            InitializeComponent();
        }

        internal void Initialize()
        {
            PathBar.ItemsSource = CreateBreadcrumbItems(CurrentPath);
        }

        internal void Show()
        {
            PathBar.Visibility = Visibility.Visible;
        }

        internal void Hide()
        {
            PathBar.Visibility = Visibility.Collapsed;
        }

        internal void SetPathBar()
        {
            PathBar.ItemsSource = CreateBreadcrumbItems(CurrentPath);
        }

        private void PathBar_ItemClicked(BreadcrumbBar sender, BreadcrumbBarItemClickedEventArgs args)
        {
            if (args.Item is Crumbs item)
            {
                CurrentPath = item.FullPath;                
            }
        }

        private static List<Crumbs> CreateBreadcrumbItems(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException(
                    "パスを指定してください。", nameof(path));
            }

            string fullPath = Path.GetFullPath(path);

            string root = Path.GetPathRoot(fullPath)
                ?? throw new ArgumentException(
                    "ルートを取得できません。", nameof(path));

            var items = new List<Crumbs>
            {
                new Crumbs
                {
                    Name = root,
                    FullPath = root
                }
            };

            string remaining = fullPath.Substring(root.Length);

            string[] parts = remaining.Split(
                new[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar },
                StringSplitOptions.RemoveEmptyEntries);

            string currentPath = root;

            foreach (string part in parts)
            {
                currentPath = Path.Combine(currentPath, part);

                items.Add(new Crumbs
                {
                    Name = part,
                    FullPath = currentPath
                });
            }
            return items;
        }

        // **************************************************
        //  プロパティ関連
        // **************************************************
        public static readonly DependencyProperty CurrentPathProperty =
        DependencyProperty.Register
            (
                nameof(CurrentPath),
                typeof(string),
                typeof(BreadcrumbAddressBar),
                new PropertyMetadata(@"C:\", OnCurrentPathChanged)
            );

        public string CurrentPath
        {
            get => (string)GetValue(CurrentPathProperty);
            set => SetValue(CurrentPathProperty, value);
        }

        private static void OnCurrentPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            using var log = MethodLog.Start<BreadcrumbAddressBar>();
            if (d is BreadcrumbAddressBar control)
            {
                control.SetPathBar();
            }
        }
    }
    internal class Crumbs
    {
        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
    }
}
