using csso.NodeCore;
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

            Graph graph = new Graph();
            Node node1 = new Node(schemaRgbMix, graph);
            Node node2 = new Node(schemaBitmap, graph);

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
            Point? p1 = Node2.Outputs[0].Control?.TransformToAncestor(Canvas)
                                        .Transform(new Point(0, 0));
            Point? p2 = Node1.Inputs[0].Control?.TransformToAncestor(Canvas)
                                        .Transform(new Point(0, 0));

            if (p1.HasValue && p2.HasValue)
            {
                Line1.X1 = p1.Value.X;
                Line1.Y1 = p1.Value.Y;
                Line1.X2 = p2.Value.X;
                Line1.Y2 = p2.Value.Y;
            }
        }
    }
}
