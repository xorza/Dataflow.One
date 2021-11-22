using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace csso.WpfNode
{
    /// <summary>
    /// Interaction logic for Node.xaml
    /// </summary>
    public partial class Node : UserControl, INotifyPropertyChanged
    {
        private NodeView? _nodeView;
        public NodeView? NodeView
        {
            get { return _nodeView; }
            set
            {
                if (_nodeView != value)
                {
                    _nodeView = value;
                    Panel.DataContext = _nodeView;
                    Refresh();
                }
            }
        }



        public Node()
        {
            InitializeComponent();

            Loaded += Node_Loaded;
        }

        private void Node_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void Refresh()
        {
            InputsStackPanel.Children.Clear();


            if (_nodeView != null)
            {
           
                foreach (var pv in _nodeView.Inputs)
                {
                    //PutView pv = new PutView(item);                    
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.Orientation = Orientation.Horizontal;
                    UIElement pin = new Button() { Content = "x" };
                    pv.Control = pin;
                    stackPanel.Children.Add(pin);
                    stackPanel.Children.Add(new Label() { Content = pv.SchemaPut.Name });
                    InputsStackPanel.Children.Add(stackPanel);
                }

                foreach (var pv in _nodeView.Outputs)
                {
                    //PutView pv = new PutView(item);
                    StackPanel stackPanel = new StackPanel();
                    stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                    stackPanel.Orientation = Orientation.Horizontal;
                    stackPanel.Children.Add(new Label() { Content = pv.SchemaPut.Name });
                    UIElement pin = new Button() { Content = "o" };
                    pv.Control = pin;
                    stackPanel.Children.Add(pin);
                    OutputsStackPanel.Children.Add(stackPanel);
                }
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
