namespace csso.NodeRunner.Shared; 

public class UiApi {
    public virtual void ShowMessage(string message) {
        System.Windows.MessageBox.Show(message);
    }
}