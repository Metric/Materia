using InfinityUI.Core;
using InfinityUI.Interfaces;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL
{
    /// <summary>
    /// This is primarly for handling various
    /// Events related to GlobalEvents
    /// That integrate with AvaloniaUI panes
    /// Such as Parameters Pane etc
    /// That need to be updated per frame, if needed
    /// </summary>
    /// <seealso cref="InfinityUI.Interfaces.IComponent" />
    public class UIGraphEvents : IComponent
    {
        protected UINodePoint previousPoint;

        public UIObject Parent { get; set; }

        public void Awake()
        {

        }

        public void Update()
        {
            if (Parent == null || !Parent.Visible) return;
            UpdateParameterVisibility();
        }

        private void UpdateParameterVisibility()
        {
            if (UINodePoint.SelectedOrigin != null && previousPoint == null)
            {
                var p = UINodePoint.SelectedOrigin;
                //verify it is for this selected graph
                if (p.Node.Graph == Parent)
                {
                    previousPoint = UINodePoint.SelectedOrigin;
                    GlobalEvents.Emit(GlobalEvent.HideParameters, this, previousPoint.Node);
                }
            }
            else if(UINodePoint.SelectedOrigin == null && previousPoint != null)
            {
                GlobalEvents.Emit(GlobalEvent.ShowParameters, this, previousPoint.Node);
                previousPoint = null;
            }
        }

        public void Dispose()
        {

        }
    }
}
