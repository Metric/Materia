using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Materia.Nodes;
using System.Windows;

namespace Materia.UI
{
    public interface IUIGraphNode
    {
        double Scale
        {
            get;
        }

        Node Node { get; set; }

        UIGraph Graph { get; set; }

        string Id { get; set; }

        List<UINodePoint> InputNodes { get; set; }
        List<UINodePoint> OutputNodes { get; set; }

        Rect UnscaledBounds
        {
            get;
        }

        Rect Bounds
        {
            get;
        }

        Point Origin
        {
            get;
        }

        void LoadConnection(string id);
        void LoadConnections(Dictionary<string, IUIGraphNode> lookup);

        bool ContainsPoint(Point p);
        bool IsInRect(Rect r);

        void UpdateScale(double sc);
        void MoveTo(double sx, double sy);
        void Move(double dx, double dy);
        void Offset(double dx, double dy);
        void OffsetTo(double sx, double sy);
        void ResetPosition();
        void HideBorder();
        void ShowBorder();
        void DisposeNoRemove();
        void Dispose();
    }
}
