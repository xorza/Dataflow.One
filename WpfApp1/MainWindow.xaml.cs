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
using Debug = csso.Common.Debug;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private readonly csso.NodeCore.Graph _graph;
        private csso.WpfNode.GraphView? _graphView;

        public MainWindow()
        {
            InitializeComponent();

            _graph = new csso.NodeCore.Graph();

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

            csso.NodeCore.Node node1 = new csso.NodeCore.Node(schemaRgbMix, _graph);
            csso.NodeCore.Node node2 = new csso.NodeCore.Node(schemaBitmap, _graph);

            Loaded += (s, ea) => { RefreshLine(); };
            MouseUp += MainWindow_MouseUp;
            MouseMove += MainWindow_MouseMove;

            Node1.PinClick += Node_PinClick;
            Node2.PinClick += Node_PinClick;

            RefreshGraphView();
        }

        private void RefreshGraphView()
        {
            _graphView = new csso.WpfNode.GraphView(_graph);
            _graphView.Edges.CollectionChanged += Edges_CollectionChanged;
            Node1.NodeView = _graphView.Nodes[0];
            Node2.NodeView = _graphView.Nodes[1];
            DataContext = _graphView;
        }

        private void Edges_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            RedrawEdges();
        }

        private void MainWindow_MouseMove(object sender, MouseEventArgs e)
        {
            RefreshLine();
        }

        PutView? pv1 = null;
        PutView? pv2 = null;
        private void Node_PinClick(object sender, PinClickEventArgs e)
        {
            if (pv1 == null)
            {
                pv1 = e.Put;
            }
            else if (pv2 == null)
            {
                pv2 = e.Put;
                Commit();
            }
            else
            {
                pv1 = e.Put;
                pv2 = null;
            }

            RefreshLine();
        }

        private void Commit()
        {
            Debug.Assert.NotNull(pv1);
            Debug.Assert.NotNull(pv2);

            if (pv1!.SchemaPut.PutType == pv2!.SchemaPut.PutType)
            {
                pv1 = null;
                pv2 = null;
                return;
            }
            if (pv1!.NodeView == pv2!.NodeView)
            {
                pv1 = null;
                pv2 = null;
                return;
            }

            PutView input = pv1.SchemaPut.PutType == PutType.In ? pv1 : pv2;
            PutView output = pv1.SchemaPut.PutType == PutType.Out ? pv1 : pv2;

            Debug.Assert.True(pv1 != pv2);

            OutputBinding binding = new OutputBinding(
                input.NodeView.Node,
                (SchemaInput)input.SchemaPut,
                output.NodeView.Node,
                (SchemaOutput)output.SchemaPut);

            Int32 indexOfBinding = input.NodeView.Node.Schema.Inputs.IndexOf((SchemaInput)input.SchemaPut);
            input.NodeView.Node.Inputs[indexOfBinding] = binding;

            RefreshGraphView();
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


            //if (pv1 != null && pv2 != null)
            //{
            //    Line1.X1 = pv1.PinPoint.X;
            //    Line1.Y1 = pv1.PinPoint.Y;
            //    Line1.X2 = pv2.PinPoint.X;
            //    Line1.Y2 = pv2.PinPoint.Y;
            //    Line1.Visibility = Visibility.Visible;
            //}
            //else
            {
                Line1.Visibility = Visibility.Collapsed;
            }

            RedrawEdges();
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

        private void RedrawEdges()
        {
            EdgesCanvas.Children.Clear();

            foreach (var edge in _graphView!.Edges)
            {
                Line line = new Line();
                line.X1 = edge.P1.X;
                line.Y1 = edge.P1.Y;
                line.X2 = edge.P2.X;
                line.Y2 = edge.P2.Y;
                line.StrokeThickness = 2.0;
                line.Stroke = Brushes.Black;

                EdgesCanvas.Children.Add(line);
            }
        }
    }
}
