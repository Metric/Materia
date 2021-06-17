using InfinityUI.Components;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Nodes;
using Materia.Rendering.Geometry;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    public class UINodePath : UIObject, ILayout
    {
        //this does nothing on this one
        public bool NeedsUpdate { get; set; }

        /// <summary>
        /// Gets or sets the primary point.
        /// </summary>
        /// <value>
        /// The primary point.
        /// </value>
        public UINodePoint PrimaryPoint
        {
            get => point1;
            set => point1 = value;
        }

        protected Vector2 secondaryPoint;
        /// <summary>
        /// Gets or sets the secondary point.
        /// Used for generating previews primarly
        /// without knowing the secondary node
        /// </summary>
        /// <value>
        /// The secondary point.
        /// </value>
        public Vector2 SecondaryPoint
        {
            get => secondaryPoint;
            set => secondaryPoint = value;
        }

        protected UINodePoint point1;
        protected UINodePoint point2;
        protected UIPath path;

        protected UIObject textArea;
        protected UIText text;

        protected bool selected = false;

        private bool previousSelected = false;
        public bool Selected
        {
            get => selected;
            set
            {
                previousSelected = selected;
                selected = value;
            }
        }

        protected Vector2 previousPoint1;
        protected Vector2 previousPoint2;

        protected Line leftSide;
        protected Line leftAngle;
        protected Line center;
        protected Line rightAngle;
        protected Line rightSide;

        protected bool isExecute = false;

        protected static Vector4 Red = new Vector4(1, 0.25f, 0.25f, 1);
        protected static Vector4 White = new Vector4(1, 1, 1, 1);

        public UINodePath(bool execute = false) : base()
        {
            leftSide = new Line(new Vector3(0, 0, -2), new Vector3(10, 0, -2));
            leftAngle = new Line(new Vector3(10, 0, -2), new Vector3(20, 5, -2));
            center = new Line(new Vector3(20, 5, -2), new Vector3(20, 5, -2));
            rightAngle = new Line(new Vector3(20, 5, -2), new Vector3(30, 0, -2));
            rightSide = new Line(new Vector3(30, 0, -2), new Vector3(40, 0, -2));

            isExecute = execute;

            InitializeComponents();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="UINodePath"/> class.
        /// </summary>
        /// <param name="p1">p1 should be the nodeoutput</param>
        /// <param name="p2">p2 should be the nodeinput</param>
        /// <param name="execute">if set to <c>true</c> [execute].</param>
        public UINodePath(UINodePoint p1, UINodePoint p2, bool execute = false) : this(execute)
        {
            point1 = p1;
            point2 = p2;
        }

        protected void InitializeComponents()
        {
            if (isExecute)
            {
                textArea = new UIObject();
                text = textArea.AddComponent<UIText>();
                text.Color = new Vector4(0.75f, 0.75f, 0.75f, 1);
                text.FontSize = 16;
                AddChild(textArea);
            }

            path = AddComponent<UIPath>();
            path.Set(new List<Line>(new Line[] { leftSide, leftAngle, center, rightAngle, rightSide }));
            RaycastTarget = false; //disable raycast on these
        }

        protected void UpdateColors()
        {
            if (selected)
            {
                leftSide.EndColor = leftSide.StartColor = rightSide.EndColor = rightSide.StartColor = Red;
                leftAngle.StartColor = leftAngle.EndColor = rightAngle.EndColor = rightAngle.StartColor = Red;
                center.StartColor = center.EndColor = Red;
            }
            else if (point1 != null && point2 != null)
            {
                leftSide.StartColor = leftSide.EndColor = point1.Color;
                rightSide.StartColor = rightSide.EndColor = point2.Color;
                leftAngle.StartColor = point1.Color;
                leftAngle.EndColor = point1.Color;
                rightAngle.StartColor = point2.Color;
                rightAngle.EndColor = point2.Color;
                center.StartColor = point1.Color;
                center.EndColor = point2.Color;
            }
            else if(point1 != null && point2 == null && point1.NodePoint is NodeOutput)
            {
                leftSide.StartColor = leftSide.EndColor = point1.Color;
                rightSide.StartColor = rightSide.EndColor = White;
                leftAngle.StartColor = point1.Color;
                leftAngle.EndColor = point1.Color;
                rightAngle.StartColor = White;
                rightAngle.EndColor = White;
                center.StartColor = point1.Color;
                center.EndColor = White;
            }
            else if(point1 != null && point2 == null && point1.NodePoint is NodeInput)
            {
                leftSide.StartColor = leftSide.EndColor = White;
                rightSide.StartColor = rightSide.EndColor = point1.Color;
                leftAngle.StartColor = White;
                leftAngle.EndColor = White;
                rightAngle.StartColor = point1.Color;
                rightAngle.EndColor = point1.Color;
                center.StartColor = White;
                center.EndColor = point1.Color;
            }
        }

        public void Invalidate()
        {
            UpdateColors();
            TryAndInvalidatesNodes();
            TryAndInvalidatePositions();
            path?.Invalidate(); //force invalidate on same frame
        }

        private void TryAndInvalidatePositions()
        {
            if (point1 == null || point2 != null || path == null) return;

            //forgot to check proper alignment for start and ends based on
            //point1 node point type, if is nodeoutput assume normal order
            //otherwise flip them if is nodeinput
            Vector2 r1 = point1.NodePoint is NodeOutput ? point1.WorldPosition : secondaryPoint;
            Vector2 r2 = point1.NodePoint is NodeOutput ? secondaryPoint : point1.WorldPosition;

            if (r1 == previousPoint1 && r2 == previousPoint2 && previousSelected == selected)
            {
                return;
            }

            previousSelected = selected;

            previousPoint1 = r1;
            previousPoint2 = r2;

            CalculateLines(ref r1, ref r2, point1.NodePoint is NodeInput, point1.NodePoint is NodeOutput);

            path.NeedsUpdate = true;
        }

        private void TryAndInvalidatesNodes()
        {
            if (point1 == null || point2 == null || path == null) return;

            Vector2 r1 = point1.WorldPosition;
            Vector2 r2 = point2.WorldPosition;

            if (r1 == previousPoint1 && r2 == previousPoint2 && previousSelected == selected)
            {
                return;
            }

            previousSelected = selected;

            previousPoint1 = r1;
            previousPoint2 = r2;

            CalculateLines(ref r1, ref r2);

            path.NeedsUpdate = true;
        }

        private void CalculateLines(ref Vector2 r1, ref Vector2 r2, bool isPointBased1 = false, bool isPointBased2 = false)
        {
            float midy = (r2.Y + r1.Y) * 0.5f;
            float midx = (r2.X + r1.X) * 0.5f;

            float horizDist = (r2.X - UINodePoint.DEFAULT_SIZE) - (r1.X + UINodePoint.DEFAULT_SIZE);

            float xDir = MathF.Sign(horizDist);

            horizDist = MathF.Abs(horizDist);

            if (!isPointBased1)
            {
                leftSide.Start = new Vector3(r1.X + UINodePoint.DEFAULT_SIZE, r1.Y + UINodePoint.DEFAULT_SIZE * 0.5f, 0);
            }
            else
            {
                leftSide.Start = new Vector3(r1.X, r1.Y, 0);
            }

            leftSide.End = leftSide.Start + new Vector3(10, 0, 0);
            leftAngle.Start = leftSide.End;
            leftAngle.End = new Vector3(leftAngle.Start.X + horizDist * 0.05f * xDir, midy, 0);
            center.Start = leftAngle.End;
            center.End = new Vector3(center.Start.X + horizDist * 0.90f * xDir, midy, 0);
            rightAngle.Start = center.End;

            if (!isPointBased2)
            {
                rightAngle.End = new Vector3(rightAngle.Start.X + horizDist * 0.05f * xDir, r2.Y + UINodePoint.DEFAULT_SIZE * 0.5f, 0);

            }
            else
            {
                rightAngle.End = new Vector3(rightAngle.Start.X + horizDist * 0.05f * xDir, r2.Y, 0);
            }

            rightSide.Start = rightAngle.End;
            rightSide.End = rightSide.Start + new Vector3(10, 0, 0);

            if (textArea != null)
            {
                textArea.Position = new Vector2(midx, midy);
                textArea.ZOrder = -2;
                if (point2 != null)
                {
                    text.Text = (point1.GetOutIndex(point2) + 1).ToString();
                }
                else
                {
                    text.Text = "";
                }
            }
        }

        public override void Update()
        {
            base.Update();

            if (!Visible) return;

            Invalidate();
        }
    }
}
