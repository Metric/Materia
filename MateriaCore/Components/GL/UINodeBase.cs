using InfinityUI.Components;
using InfinityUI.Controls;
using InfinityUI.Core;
using InfinityUI.Interfaces;
using Materia.Nodes;
using Materia.Nodes.Atomic;
using Materia.Rendering.Mathematics;
using Materia.Rendering.Textures;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MateriaCore.Components.GL
{
    public abstract class UINodeBase : MovablePane, IGraphNode
    {
        public event Action<UINodeBase> Restored;
        public event Action<UINodeBase> PreviewUpdated;

        #region Graph Details
        public Node Node { get; protected set; }

        public UIGraph Graph { get; protected set; }

        public string Id { get; protected set; } = Guid.NewGuid().ToString(); //assign a default guid to it

        //note add back in Input Output Nodes here
        protected List<UINodePoint> inputs = new List<UINodePoint>();
        protected List<UINodePoint> outputs = new List<UINodePoint>();
        #endregion

        #region States
        protected bool isMoving = false;
        #endregion

        protected UINodeBase(Vector2 size) : base(size)
        {
        }

        public Box2 GetViewSpaceRect()
        {
            Box2 world = AnchoredRect;
            if (Canvas == null || Graph == null) return world;
            Vector2 pos = Position * Graph.InverseZoom - Canvas.Cam.LocalPosition.Xy;
            return new Box2(pos, Size.X * Graph.InverseZoom, Size.Y * Graph.InverseZoom);
        }

        public GLTexture2D GetActiveBuffer()
        {
            return Node?.GetActiveBuffer();
        }

        public virtual void LoadConnection(UINodePoint output, NodeInput inp)
        {

        }

        public virtual void LoadConnections()
        {

        }

        protected virtual void OnBeforeRestored()
        {

        }

        protected virtual void OnRestored()
        {

        }



        protected void OnPreviewUpdated()
        {
            PreviewUpdated?.Invoke(this);
        }

        /// <summary>
        /// Restores this instance from underlying graph data
        /// that may been updated for the specified node id
        /// </summary>
        public void Restore()
        {
            if (Graph == null || Node == null) return;
            if (Graph.Current == null) return;
            if (!Graph.Current.NodeLookup.TryGetValue(Node.Id, out Node n))
            {
                return;
            }

            OnBeforeRestored();

            Node = n;

            OnRestored();

            Restored?.Invoke(this);
        }

        #region User Input Events
        protected virtual void Selectable_PointerDown(UISelectable arg1, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Left))
            {
                TryAndSelect();
            }
        }

        protected virtual void Selectable_PointerUp(UISelectable arg1, MouseEventArgs e)
        {
            if (isMoving)
            {
                GlobalEvents.Emit(GlobalEvent.MoveComplete, this, this);
                isMoving = false;
            }
        }

        protected virtual void Selectable_Click(UISelectable arg1, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButton.Right))
            {
                ShowContextMenu();
            }
        }

        protected virtual void UINode_DoubleClick(MovablePane obj)
        {
            //set parameter view
            GlobalEvents.Emit(GlobalEvent.ViewParameters, this, Node);

            //set 2d preview node
            GlobalEvents.Emit(GlobalEvent.Preview2D, this, Node);
        }
        protected virtual void UINode_Moved(MovablePane obj, Vector2 delta, MouseEventArgs e)
        {
            isMoving = true;

            Node.ViewOriginX = Position.X;
            Node.ViewOriginY = Position.Y;

            //send event to other node that are multiselected for delta move
            GlobalEvents.Emit(GlobalEvent.MoveSelected, this, delta);
        }
        protected virtual void UINode_MovedTo(MovablePane arg1, Vector2 pos)
        {
            Node.ViewOriginX = Position.X;
            Node.ViewOriginY = Position.Y;
        }

        protected virtual void TryAndSelect()
        {
            if (Graph == null) return;

            if (UI.IsCtrlPressed)
            {
                //add node or remove from multi select
                Graph.ToggleSelect(this);
                return;
            }

            bool selected = Graph.IsSelected(Id);
            if (selected && Graph.Selected.Count > 1)
            {
                return;
            }

            //clear multiselect
            Graph.ClearSelection();

            //toggle graph select this
            Graph.Select(this);
        }

        protected virtual void ShowContextMenu()
        {
            //setup context menu and show it
            if (Node is PixelProcessorNode)
            {

            }
            else if (Node is GraphInstanceNode)
            {

            }
            else if (Node is ImageNode)
            {

            }
            else if (Node is MathNode)
            {

            }
            else if (Node is ItemNode)
            {

            }
        }
        #endregion;
    }
}
