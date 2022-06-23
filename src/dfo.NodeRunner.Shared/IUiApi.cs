using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace dfo.NodeRunner.Shared;

public class UiApi {
    public virtual void ShowMessage(string message) {
        MessageBox.Show(message);
    }

    public virtual void ShowImage(BitmapSource bmpSource) {
        var wnd = new Window();
        var img = new Image();
        img.Source = bmpSource;

        wnd.Content = img;
        wnd.Show();
    }
}