using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Rendering.Geometry;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UINodePath : UIObject
    {
        protected UINodePoint point1;
        protected UINodePoint point2;
        protected UIPath path;

        protected UIObject textArea;
        protected UIText text;

        protected bool selected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                selected = value;
                Update();
            }
        }

        protected Line leftSide;
        protected Line leftAngle;
        protected Line center;
        protected Line rightAngle;
        protected Line rightSide;

        protected bool isExecute = false;

        protected static Vector4 Red = new Vector4(1, 0.25f, 0.25f, 1);
        protected static Vector4 White = new Vector4(1, 1, 1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="UINodePath"/> class.
        /// </summary>
        /// <param name="p1">p1 should be the nodeoutput</param>
        /// <param name="p2">p2 should be the nodeinput</param>
        /// <param name="execute">if set to <c>true</c> [execute].</param>
        public UINodePath(UINodePoint p1, UINodePoint p2, bool execute = false) : base()
        {
            RelativeTo = Anchor.TopLeft;

            point1 = p1;
            point2 = p2;

            leftSide = new Line(new Vector3(0,0,-2), new Vector3(10,0,-2));
            leftAngle = new Line(new Vector3(10,0,-2), new Vector3(20,5,-2));
            center = new Line(new Vector3(20,5,-2), new Vector3(20,5,-2));
            rightAngle = new Line(new Vector3(20, 5, -2), new Vector3(30, 0,-2));
            rightSide = new Line(new Vector3(30,0,-2), new Vector3(40,0,-2));

            isExecute = execute;

            InitializeComponents();
            Update();
        }

        protected void InitializeComponents()
        {
            if (isExecute)
            {
                textArea = new UIObject();
                textArea.RelativeTo = Anchor.TopLeft;
                text = textArea.AddComponent<UIText>();
                text.Color = new Vector4(0.75f, 0.75f, 0.75f, 1);
                text.FontSize = 12;
                AddChild(textArea);
            }

            path = AddComponent<UIPath>();
            path.Set(new List<Line>(new Line[] { leftSide, leftAngle, center, rightAngle, rightSide }));
        }

        protected void UpdateColors()
        {
            if (selected)
            {
                leftSide.EndColor = leftSide.StartColor = rightSide.EndColor = rightSide.StartColor = Red;
                leftAngle.StartColor = leftAngle.EndColor = rightAngle.EndColor = rightAngle.StartColor = Red;
                center.StartColor = center.EndColor = Red;
            }
            else
            {
                leftSide.StartColor = leftSide.EndColor = point1.Color;
                rightSide.StartColor = rightSide.EndColor = point2.Color;
                leftAngle.StartColor = point1.Color;
                leftAngle.EndColor = White;
                rightAngle.StartColor = White;
                rightAngle.EndColor = point2.Color;
            }
        }

        public override void Update()
        {
            base.Update();

            if (point1 == null || point2 == null) return;

            Vector2 r1 = point1.WorldPosition;
            Vector2 r2 = point2.WorldPosition;

            float midy = (r2.Y + r1.Y) * 0.5f;
            float midx = (r2.X + r1.X) * 0.5f;

            //subtract / add 10 for spacing
            float dist = Vector2.Distance(r1 + new Vector2(10,0), r2 - new Vector2(10,0));

            leftSide.Start = new Vector3(r1.X, r1.Y, -2);
            leftSide.End = leftSide.Start + new Vector3(10, 0, 0);
            leftAngle.Start = leftSide.End;
            leftAngle.End = new Vector3(dist * 0.25f + leftAngle.Start.X, midy, -2);
            center.Start = leftAngle.End;
            center.End = new Vector3(dist * 0.5f + center.Start.X, midy, -2);
            rightAngle.Start = center.End;
            rightAngle.End = new Vector3(rightAngle.Start.X + dist * 0.25f, r2.Y, -2);
            rightSide.Start = rightAngle.End;
            rightSide.End = rightSide.Start + new Vector3(10, 0, 0);

            if (textArea != null)
            {
                textArea.Position = new Vector2(midx, midy);
                textArea.ZOrder = -2;
                text.Text = (point1.GetOutIndex(point2) + 1).ToString();
            }

            UpdateColors();

            path?.Invalidate();
        }

        public override void Dispose(bool disposing = true)
        {
            base.Dispose(disposing);
        }
    }
}
