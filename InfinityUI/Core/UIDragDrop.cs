using InfinityUI.Components;
using InfinityUI.Interfaces;
using Materia.Rendering.Mathematics;
using System;
using System.Collections.Generic;
using System.Text;

namespace InfinityUI.Core
{
    public class UIDragDrop
    {
        public UICanvas Canvas { get; protected set; }

        public UIObject Source
        {
            get => dragElement;
        }

        public object DropData
        {
            get => dropData;
        }

        protected object dropData;
        protected UIObject dragElement;
        protected UIObject dragParent;
        protected Anchor dragAnchor;
        protected Vector2 dragPosition;
        protected int dragParentIndex;
        protected bool dragRaycastTarget;
        protected int dragZOrder;
        protected Vector2 dragSize;

        protected bool isDragging = false;


        public UIDragDrop(UICanvas c)
        {
            Canvas = c;
        }

        private void AddUIEvents()
        {
            UI.MouseMove += UI_MouseMove;
            UI.MouseUp += UI_MouseUp;
        }

        private void UI_MouseUp(Interfaces.MouseEventArgs e)
        {
            if (isDragging)
            {
                TryAndDrop(e);
                Complete();
            }
        }

        private void UI_MouseMove(Interfaces.MouseEventArgs e)
        {
            if (isDragging)
            {
                dragElement.Position += e.Delta * Canvas.Scale / dragElement.Scale;
            }
        }

        private void RemoveUIEvents()
        {
            UI.MouseMove -= UI_MouseMove;
            UI.MouseUp -= UI_MouseUp;
        }

        public void Restore()
        {
            dragElement.RaycastTarget = dragRaycastTarget;
            dragElement.RelativeTo = dragAnchor;
            dragElement.ZOrder = dragZOrder;
            dragParent?.InsertChild(dragParentIndex, dragElement);
            dragElement.Position = dragPosition;
            dragElement.Size = dragSize;
        }

        private void TryAndDrop(MouseEventArgs e)
        {
            Restore();

            var newArgs = new MouseEventArgs
            {
                Delta = e.Delta,
                Position = e.Position,
                Button = e.Button,
            };

            dragElement.SendMessageUpwards("OnMouseUp", newArgs);

            Vector2 pos = UI.MousePosition;
            UI.Pick(ref pos);
            var target = UI.Selection;
            if (target == null)
            {
                return;
            }

            UIDropEvent evt = new UIDropEvent
            {
                dragDrop = this
            };

            target.SendMessageUpwards("OnDrop", evt);
        }

        private void Complete()
        {
            RemoveUIEvents();
            isDragging = false;
            dragElement = null;
        }

        public void Begin(UIObject ele, object data)
        {
            if (isDragging) return;
            if (Canvas == null || Canvas.Parent == null) return;

            isDragging = true;

            dragElement = ele;
            dragParent = ele.Parent;
            dragPosition = ele.Position;
            dragRaycastTarget = ele.RaycastTarget;
            dragZOrder = ele.ZOrder;
            dragSize = ele.Size;

            if (ele.Parent != null)
            {
                dragParentIndex = ele.Parent.IndexOf(ele);
            }
            else
            {
                dragParentIndex = -1;
            }

            dragAnchor = ele.RelativeTo;

            dropData = data;


            ele.ZOrder = -4;
            ele.Size = ele.AnchorSize;

            Canvas.Parent.AddChild(ele);

            ele.RaycastTarget = true;
            ele.RelativeTo = Anchor.TopLeft;
            ele.Position = UI.MousePosition * Canvas.Scale / ele.Scale - ele.Size * 0.5f;

            AddUIEvents();
        }
    }
}
