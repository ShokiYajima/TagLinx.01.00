using Microsoft.UI.Xaml.Controls.Primitives;
namespace TagLinx._01._00;
public sealed partial class MainWindow
{
    private const double MinLeftWidth = 100;
    private const double MaxLeftWidth = 300;
    //private void QuickAccessZone_Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    //{
    //    double newWidth = QuickAccessZone.ActualWidth + e.HorizontalChange;
    //    if (newWidth < MinLeftWidth) newWidth = MinLeftWidth;
    //    if (newWidth > MaxLeftWidth) newWidth = MaxLeftWidth;
    //    QuickAccessZone.Width = new Microsoft.UI.Xaml.GridLength(newWidth);
    //}
}
