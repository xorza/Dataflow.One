using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace csso.NodeRunner.Shared;

public class UiApi {
    public virtual void ShowMessage(string message) {
        System.Windows.MessageBox.Show(message);
    }

    public virtual void ShowImage(BitmapSource bmpSource) {
        Window wnd = new Window();
        var img = new Image();
        img.Source = bmpSource;

        wnd.Content = img;
        wnd.Show();
    }
}