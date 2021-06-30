using Avalonia.Controls;
using InfinityUI.Components;
using InfinityUI.Core;
using Materia.Graph;
using Materia.Rendering.Mathematics;
using MateriaCore.Components.Dialogs;
using MateriaCore.Utils;
using System;
using System.Collections.Generic;
using System.Text;

namespace MateriaCore.Components.GL.Menu
{
    //note: there should only ever be one instanc of this
    public class MainMenu : UIObject
    {
        private static MainMenu Current { get; set; }
        private const float HEIGHT = 32;
        private static Localization.Local local = new Localization.Local();

        #region components
        UIMenu menu;
        UIMenuItem editItem;
        UIMenuItem undoItem;
        UIMenuItem redoItem;
        #endregion

        public MainMenu() : base()
        {
            Current = this;
            InitializeComponents();
            BuildMenu();
        }

        private void InitializeComponents()
        {
            RelativeTo = Anchor.TopHorizFill;
            ZOrder = -997;
            Size = new Vector2(1, HEIGHT);
        }

        private void BuildMenu()
        {
            menu = new UIMenuBuilder()
                    .Add(local.Get("File"))
                    .StartSubMenu()
                    .Add(local.Get("New"), async (b) =>
                    {
                        UI.Focus = null;
                        await UIDocuments.TryAndCreate();
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .Add(local.Get("Open"), async (b) =>
                    {
                        UI.Focus = null;
                        await UIDocuments.TryAndOpen();
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .Separator()
                    .Add(local.Get("CloseAll"), (b) => {
                        GlobalEvents.Emit(GlobalEvent.CloseAllDocuments, b, null);
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .Separator()
                    .Add(local.Get("Save")) //todo: add in global event for save
                    .Add(local.Get("SaveAs")) //todo: add in global event for save as
                    .Add(local.Get("SaveAll"), (b) => {
                        GlobalEvents.Emit(GlobalEvent.SaveAllDocuments, b, null);
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .Separator()
                    .Add(local.Get("Recent"))
                    .DynamicSubMenu(() => new List<UIMenuItem>()) //temporary replace with actual data source function
                    .Separator()
                    .Add(local.Get("Export")) //todo: show export dialog, todo: recreate export dialgo in avalonia ui
                    .FinishSubMenu()
                    .Add(local.Get("Edit"))
                    .StartSubMenu()
                    .Add(local.Get("Undo"), (b) =>
                    {
                        UIDocuments.Current?.ActiveGraph?.TryAndUndo();
                        UpdateUndoRedoStates();
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .Add(local.Get("Redo"), (b) =>
                    {
                        UIDocuments.Current?.ActiveGraph?.TryAndRedo();
                        UpdateUndoRedoStates();
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .Separator()
                    .Add(local.Get("GraphSettings"), (b) =>
                    {
                        GlobalEvents.Emit(GlobalEvent.ViewParameters, UIDocuments.Current?.ActiveGraph, UIDocuments.Current?.ActiveGraph?.Root);
                        UIDocuments.Current?.ActiveGraph?.Focus();
                    })
                    .FinishSubMenu()
                    .Add(local.Get("Windows"))
                    .StartSubMenu()
                    //todo make this a toggle group menu items
                    .Add(local.Get("2DPreview")) //todo: add in global toggle event for this
                    .Add(local.Get("3DPreview")) //todo: add in global toggle event for this
                    .Add(local.Get("Shelf")) //todo: add in global toggle event for this
                    .Add(local.Get("Log")) //todo: add in global toggle event for this
                    .Separator()
                    .Add(local.Get("CloseAll")) //todo: add in global event to close all windows
                    .FinishSubMenu()
                    .Finilize();

            editItem = menu.Children.Find(m => (m as UIMenuItem) != null && (m as UIMenuItem).Text == local.Get("Edit")) as UIMenuItem;

            undoItem = editItem.SubMenu.Children.Find(m => (m as UIMenuItem) != null && (m as UIMenuItem).Text == local.Get("Undo")) as UIMenuItem;
            redoItem = editItem.SubMenu.Children.Find(m => (m as UIMenuItem) != null && (m as UIMenuItem).Text == local.Get("Redo")) as UIMenuItem;

            editItem.Focused += EditItem_Focused;

            AddChild(menu);
        }

        //update edit / redo enabled states on focus
        private void EditItem_Focused(InfinityUI.Controls.Button arg1, InfinityUI.Interfaces.FocusEvent arg2)
        {
            UpdateUndoRedoStates();
        }

        public static void TryAndUpdateState()
        {
            Current?.UpdateUndoRedoStates();
        }

        private void UpdateUndoRedoStates()
        {
            int? undoCount = UIDocuments.Current?.ActiveGraph?.Current?.UndoCount;
            int? redoCount = UIDocuments.Current?.ActiveGraph?.Current?.RedoCount;

            undoItem.GetComponent<UISelectable>().Enabled = undoCount != null && undoCount.Value > 0;
            redoItem.GetComponent<UISelectable>().Enabled = redoCount != null && redoCount.Value > 0;
        }
    }
}
