using csso.Common;
using csso.NodeCore;
using csso.WpfNode;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
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

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Schema schemaRgbMix = new Schema();
            schemaRgbMix.Name = "RGB mix";
            schemaRgbMix.Inputs.Add(new SchemaInput("R", typeof(Int32)));
            schemaRgbMix.Inputs.Add(new SchemaInput("G", typeof(Int32)));
            schemaRgbMix.Inputs.Add(new SchemaInput("B", typeof(Int32)));
            schemaRgbMix.Outputs.Add(new SchemaOutput("RGB", typeof(Int32)));

            Schema schemaBitmap = new Schema();
            schemaBitmap.Name = "Bitmap";
            schemaBitmap.Outputs.Add(new SchemaOutput("R", typeof(Int32)));
            schemaBitmap.Outputs.Add(new SchemaOutput("G", typeof(Int32)));
            schemaBitmap.Outputs.Add(new SchemaOutput("B", typeof(Int32)));
            schemaBitmap.Outputs.Add(new SchemaOutput("RGB", typeof(Int32)));

            csso.NodeCore.Graph graph = new csso.NodeCore.Graph();
            csso.NodeCore.Node node1 = new csso.NodeCore.Node(schemaRgbMix, graph);
            csso.NodeCore.Node node2 = new csso.NodeCore.Node(schemaBitmap, graph);

            Node1.NodeView = new csso.WpfNode.NodeView(node1);
            Node2.NodeView = new csso.WpfNode.NodeView(node2);

            Loaded += (s, ea) => { RefreshLine(); };
            MouseUp += MainWindow_MouseUp;
        }

        private void MainWindow_MouseUp(object sender, MouseButtonEventArgs e)
        {
            RefreshLine();
        }

        private void RefreshLine()
        {
            if (Node1.NodeView == null || Node2.NodeView == null)
            {
                return;
            }

            UpdatePinPositions(Canvas, Node1.NodeView.Inputs);
            UpdatePinPositions(Canvas, Node1.NodeView.Outputs);
            UpdatePinPositions(Canvas, Node2.NodeView.Inputs);
            UpdatePinPositions(Canvas, Node2.NodeView.Outputs);

            PutView input = Node1.NodeView.Inputs[0];
            PutView output = Node2.NodeView.Outputs[0];
            Line1.X1 = input.PinPoint.X;
            Line1.Y1 = input.PinPoint.Y;
            Line1.X2 = output.PinPoint.X;
            Line1.Y2 = output.PinPoint.Y;
        }

        private void UpdatePinPositions(Canvas canvas, IEnumerable<PutView> putViews)
        {
            putViews.ForEach(put =>
            {
                if (put.Control != null)
                {
                    Point upperLeft = put.Control
                        .TransformToAncestor(canvas)
                        .Transform(new Point(0, 0));
                    Point mid = new Point(
                        put.Control.RenderSize.Width / 2,
                        put.Control.RenderSize.Height / 2);
                    put.PinPoint = new Point(upperLeft.X + mid.X, upperLeft.Y + mid.Y);
                }
            });
        }
    }
}
