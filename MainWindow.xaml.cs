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
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TagLinx._01._00.Controls;
using TagLinx._01._00.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Read = WinUI3.Read;
using Update = WinUI3.Update;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TagLinx._01._00
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        internal PathConfig? pathConfig = new();
        public MainWindow()
        {
            InitializeComponent();
            Closed += MainWindow_Closed;

            pathConfig = Read.FileSystem.GetJsonInClass<PathConfig>(@"D:\CodeLab\Project\TagLinx\TagLinx.01.00\Json\PathConfig.json");
            AddressBar!.Initialize(this,FileList,BreadcrumbAddressBar);
            BreadcrumbAddressBar!.Initialize();
        }

        private void MainWindow_Closed(object sender, WindowEventArgs args)
        {
            try
            {
                AddressBar.UpdateHistory();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"終了時の処理が失敗しました：{ex}");
            }
        }

        //private void Edit_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    // Handle the button click event here
        //    string comment = Comment_TextBox.Text;
        //    // You can add your logic to process the comment or perform any action
        //    // For example, you can display the comment in a message box or save it to a file
        //    // MessageBox.Show($"Comment: {comment}");
        //}
    }
}
