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
    public class PinClickEventArgs : RoutedEventArgs
    {
        public PutView Put { get; private set; }

        public PinClickEventArgs(PutView put)
        {
            Put = put;
        }
    }
    public delegate void PinClickEventHandler(object sender, PinClickEventArgs e);


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

        public event PinClickEventHandler? PinClick;

        public Node()
        {
            InitializeComponent();
        }

        public bool UpdatePinPositions(Canvas canvas)
        {
            bool updated = false;

            _nodeView?.Inputs.ForEach(put =>
            {
                if (put.Control != null)
                {
                    Point upperLeft = put.Control
                        .TransformToAncestor(canvas)
                        .Transform(new Point(0, 0));
                    Point mid = new Point(
                        put.Control.RenderSize.Width / 2,
                        put.Control.RenderSize.Height / 2);

                    Point newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                    if (put.PinPoint != newPinPoint)
                    {
                        put.PinPoint = newPinPoint;
                        updated = true;
                    }
                }
            });

            _nodeView?.Outputs.ForEach(put =>
            {
                if (put.Control != null)
                {
                    Point upperLeft = put.Control
                        .TransformToAncestor(canvas)
                        .Transform(new Point(0, 0));
                    Point mid = new Point(
                        put.Control.RenderSize.Width / 2,
                        put.Control.RenderSize.Height / 2);
                    //Point mid = new Point();
                    Point newPinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                    if (put.PinPoint != newPinPoint)
                    {
                        put.PinPoint = newPinPoint;
                        updated = true;
                    }
                }
            });

            return updated;
        }

        private void Refresh()
        {
            InputsStackPanel.Children.Clear();
            OutputsStackPanel.Children.Clear();

            if (_nodeView == null)
            {
                return;
            }

            foreach (var pv in _nodeView.Inputs)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                Button button = new Button();
                pv.Control = button;
                button.Click += PinButton_Click;
                button.Tag = pv;
                stackPanel.Children.Add(button);
                stackPanel.Children.Add(new Label() { Content = pv.SchemaPut.Name });
                InputsStackPanel.Children.Add(stackPanel);
            }

            foreach (var pv in _nodeView.Outputs)
            {
                StackPanel stackPanel = new StackPanel();
                stackPanel.HorizontalAlignment = HorizontalAlignment.Right;
                stackPanel.Orientation = Orientation.Horizontal;
                stackPanel.Children.Add(new Label() { Content = pv.SchemaPut.Name });
                Button button = new Button();
                pv.Control = button;
                button.Click += PinButton_Click;
                button.Tag = pv;
                stackPanel.Children.Add(button);
                OutputsStackPanel.Children.Add(stackPanel);
            }

        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            PutView pv = (PutView)((Button)sender).Tag;
            PinClick?.Invoke(sender,
                new PinClickEventArgs(pv)
                {
                    RoutedEvent = e.RoutedEvent,
                    Source = e.Source,
                    Handled = true
                });
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
