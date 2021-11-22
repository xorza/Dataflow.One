using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using csso.NodeCore;

namespace csso.WpfNode
{
    public class NodeView : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public csso.NodeCore.Node Node { get; private set; }

        private Point _position;

        public Point Position
        {
            get { return _position; }
            set
            {
                if (_position != value)
                {
                    _position = value;
                    OnPropertyChanged();
                }
            }
        }
        public string Name => this.Node.Schema.Name;


        public NodeView(csso.NodeCore.Node node) => this.Node = node;

        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
