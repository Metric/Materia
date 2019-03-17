/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Linq;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Xceed.Wpf.AvalonDock.Layout;
using System.Windows.Threading;

namespace Xceed.Wpf.AvalonDock.Controls
{
  public abstract class LayoutGridControl<T> : Grid, ILayoutControl where T : class, ILayoutPanelElement
  {
    #region Members

    private LayoutPositionableGroup<T> _model;
    private Orientation _orientation;
    private bool _initialized;
    private ChildrenTreeChange? _asyncRefreshCalled;
    private ReentrantFlag _fixingChildrenDockLengths = new ReentrantFlag();

    #endregion

    #region Constructors

    static LayoutGridControl()
    {
    }

    internal LayoutGridControl( LayoutPositionableGroup<T> model, Orientation orientation )
    {
      if( model == null )
        throw new ArgumentNullException( "model" );

      _model = model;
      _orientation = orientation;

      FlowDirection = System.Windows.FlowDirection.LeftToRight;
    }

    #endregion

    #region Properties

    public ILayoutElement Model
    {
      get
      {
        return _model;
      }
    }   

    public Orientation Orientation
    {
      get
      {
        return ( _model as ILayoutOrientableGroup ).Orientation;
      }
    } 

    private bool AsyncRefreshCalled
    {
      get
      {
        return _asyncRefreshCalled != null;
      }
    }

    #endregion

    #region Overrides

    protected override void OnInitialized( EventArgs e )
    {
      base.OnInitialized( e );

      _model.ChildrenTreeChanged += ( s, args ) =>
          {
            if( _asyncRefreshCalled.HasValue &&
                      _asyncRefreshCalled.Value == args.Change )
              return;
            _asyncRefreshCalled = args.Change;
            Dispatcher.BeginInvoke( new Action( () =>
                  {
                    _asyncRefreshCalled = null;
                    UpdateChildren();
                  } ), DispatcherPriority.Normal, null );
          };

      this.LayoutUpdated += new EventHandler( OnLayoutUpdated );
    }

    #endregion

    #region Internal Methods

    protected void FixChildrenDockLengths()
    {
      using( _fixingChildrenDockLengths.Enter() )
        OnFixChildrenDockLengths();
    }

    protected abstract void OnFixChildrenDockLengths();

    #endregion

    #region Private Methods

    private void OnLayoutUpdated( object sender, EventArgs e )
    {
      var modelWithAtcualSize = _model as ILayoutPositionableElementWithActualSize;
      modelWithAtcualSize.ActualWidth = ActualWidth;
      modelWithAtcualSize.ActualHeight = ActualHeight;

      if( !_initialized )
      {
        _initialized = true;
        UpdateChildren();
      }
    }

    private void UpdateChildren()
    {
      var alreadyContainedChildren = Children.OfType<ILayoutControl>().ToArray();

      DetachOldSplitters();
      DetachPropertChangeHandler();

      Children.Clear();
      ColumnDefinitions.Clear();
      RowDefinitions.Clear();

      if( _model == null ||
          _model.Root == null )
        return;

      var manager = _model.Root.Manager;
      if( manager == null )
        return;


      foreach( ILayoutElement child in _model.Children )
      {
        var foundContainedChild = alreadyContainedChildren.FirstOrDefault( chVM => chVM.Model == child );
        if( foundContainedChild != null )
          Children.Add( foundContainedChild as UIElement );
        else
          Children.Add( manager.CreateUIElementForModel( child ) );
      }

        UpdateRowColDefinitions();
        CreateSplitters();

      AttachNewSplitters();
      AttachPropertyChangeHandler();
    }

    private void AttachPropertyChangeHandler()
    {
      foreach( var child in InternalChildren.OfType<ILayoutControl>() )
      {
        child.Model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler( this.OnChildModelPropertyChanged );
      }
    }

    private void DetachPropertChangeHandler()
    {
      foreach( var child in InternalChildren.OfType<ILayoutControl>() )
      {
        child.Model.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler( this.OnChildModelPropertyChanged );
      }
    }

    private void OnChildModelPropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
    {
      if( AsyncRefreshCalled )
        return;

      if( _fixingChildrenDockLengths.CanEnter && e.PropertyName == "DockWidth" && Orientation == System.Windows.Controls.Orientation.Horizontal )
      {
        if( ColumnDefinitions.Count == InternalChildren.Count )
        {
          var changedElement = sender as ILayoutPositionableElement;
          var childFromModel = InternalChildren.OfType<ILayoutControl>().First( ch => ch.Model == changedElement ) as UIElement;
          int indexOfChild = InternalChildren.IndexOf( childFromModel );
          ColumnDefinitions[ indexOfChild ].Width = changedElement.DockWidth.Value;
        }
      }
      else if( _fixingChildrenDockLengths.CanEnter && e.PropertyName == "DockHeight" && Orientation == System.Windows.Controls.Orientation.Vertical )
      {
        if( RowDefinitions.Count == InternalChildren.Count )
        {
          var changedElement = sender as ILayoutPositionableElement;
          var childFromModel = InternalChildren.OfType<ILayoutControl>().First( ch => ch.Model == changedElement ) as UIElement;
          int indexOfChild = InternalChildren.IndexOf( childFromModel );
          RowDefinitions[ indexOfChild ].Height = changedElement.DockHeight.Value;
        }
      }
      else if( e.PropertyName == "IsVisible" )
      {
        DetachOldSplitters();
        RemoveSplitters();
        UpdateRowColDefinitions();
        CreateSplitters();
        AttachNewSplitters();
      }
    }

    private void RemoveSplitters()
    {
        var splitters = Children.OfType<GridSplitter>().ToArray();
        foreach (var splitter in splitters)
        {
            Children.Remove(splitter);
        }
    }

    private void UpdateRowColDefinitions()
    {
      var root = _model.Root;
      if( root == null )
        return;
      var manager = root.Manager;
      if( manager == null )
        return;

      FixChildrenDockLengths();

      //Debug.Assert(InternalChildren.Count == _model.ChildrenCount + (_model.ChildrenCount - 1));

      #region Setup GridRows/Cols
      RowDefinitions.Clear();
      ColumnDefinitions.Clear();
      if( Orientation == Orientation.Horizontal )
      {
        int iColumn = 0;
        int iChild = 0;
        for( int iChildModel = 0; iChildModel < _model.Children.Count; iChildModel++, iColumn++, iChild++ )
        {
          var childModel = _model.Children[ iChildModel ] as ILayoutPositionableElement;
          ColumnDefinitions.Add( new ColumnDefinition()
          {
            Width = childModel.IsVisible ? childModel.DockWidth.Value : new GridLength( 0.0, GridUnitType.Pixel ),
            MinWidth = childModel.IsVisible ? childModel.DockMinWidth : 0.0
          } );
          Grid.SetColumn( InternalChildren[ iChildModel ], iColumn );

          //append column for splitter
          /*if( iChild < InternalChildren.Count - 1 )
          {
            iChild++;
            iColumn++;

            bool nextChildModelVisibleExist = false;
            for( int i = iChildModel + 1; i < _model.Children.Count; i++ )
            {
              var nextChildModel = _model.Children[ i ] as ILayoutPositionableElement;
              if( nextChildModel.IsVisible )
              {
                nextChildModelVisibleExist = true;
                break;
              }
            }

            ColumnDefinitions.Add( new ColumnDefinition()
            {
              Width = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength( manager.GridSplitterWidth ) : new GridLength( 0.0, GridUnitType.Pixel )
            } );
          }*/
        }
      }
      else //if (_model.Orientation == Orientation.Vertical)
      {
        int iRow = 0;
        int iChild = 0;
        for( int iChildModel = 0; iChildModel < _model.Children.Count; iChildModel++, iRow++, iChild++ )
        {
          var childModel = _model.Children[ iChildModel ] as ILayoutPositionableElement;
          RowDefinitions.Add( new RowDefinition()
          {
            Height = childModel.IsVisible ? childModel.DockHeight.Value : new GridLength( 0.0, GridUnitType.Pixel ),
            MinHeight = childModel.IsVisible ? childModel.DockMinHeight : 0.0
          } );
          Grid.SetRow( InternalChildren[ iChildModel ], iRow );

          //if (RowDefinitions.Last().Height.Value == 0.0)
          //    System.Diagnostics.Debugger.Break();

          //append row for splitter (if necessary)
          /*if( iChild < InternalChildren.Count - 1 )
          {
            iChild++;
            iRow++;

            bool nextChildModelVisibleExist = false;
            for( int i = iChildModel + 1; i < _model.Children.Count; i++ )
            {
              var nextChildModel = _model.Children[ i ] as ILayoutPositionableElement;
              if( nextChildModel.IsVisible )
              {
                nextChildModelVisibleExist = true;
                break;
              }
            }

            RowDefinitions.Add( new RowDefinition()
            {
              Height = childModel.IsVisible && nextChildModelVisibleExist ? new GridLength( manager.GridSplitterHeight ) : new GridLength( 0.0, GridUnitType.Pixel )
            } );
            //if (RowDefinitions.Last().Height.Value == 0.0)
            //    System.Diagnostics.Debugger.Break();
          }*/
        }
      }

      #endregion
    }

    //modified to use the proper gridsplitter control
    private void CreateSplitters()
    {
            double swidth = 2;
            double sheight = 2;

        var root = _model.Root;
        if (root != null)
        { 
            var manager = root.Manager;
            if (manager != null)
            {
                swidth = manager.GridSplitterWidth;
                sheight = manager.GridSplitterHeight;
            }
        }

      for ( int iChild = 1, c = 0; iChild < Children.Count; iChild++, c++)
      {
        var splitter = new GridSplitter();
        splitter.Cursor = this.Orientation == Orientation.Horizontal ? Cursors.SizeWE : Cursors.SizeNS;
        Children.Insert(iChild, splitter);

        if (this.Orientation == Orientation.Horizontal)
        {
            splitter.HorizontalAlignment = HorizontalAlignment.Right;
            splitter.VerticalAlignment = VerticalAlignment.Stretch;
            splitter.Width = swidth;
            Grid.SetColumn(splitter, c);
        }
        else
        {
            splitter.HorizontalAlignment = HorizontalAlignment.Stretch;
            splitter.VerticalAlignment = VerticalAlignment.Bottom;
            splitter.Height = sheight;
            Grid.SetRow(splitter, c);
        }

        iChild++;
      }
    }

    private void DetachOldSplitters()
    {
      /*foreach( var splitter in Children.OfType<LayoutGridResizerControl>() )
      {
        splitter.DragStarted -= new System.Windows.Controls.Primitives.DragStartedEventHandler( OnSplitterDragStarted );
        splitter.DragDelta -= new System.Windows.Controls.Primitives.DragDeltaEventHandler( OnSplitterDragDelta );
        splitter.DragCompleted -= new System.Windows.Controls.Primitives.DragCompletedEventHandler( OnSplitterDragCompleted );
      }*/

       foreach(var splitter in Children.OfType<GridSplitter>())
       {
            splitter.DragCompleted -= Splitter_DragCompleted;
       }
    }

    private void AttachNewSplitters()
    {
      /*foreach( var splitter in Children.OfType<LayoutGridResizerControl>() )
      {
        splitter.DragStarted += new System.Windows.Controls.Primitives.DragStartedEventHandler( OnSplitterDragStarted );
        splitter.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler( OnSplitterDragDelta );
        splitter.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler( OnSplitterDragCompleted );
      }*/

        foreach(var splitter in Children.OfType<GridSplitter>())
        {
            splitter.DragCompleted += Splitter_DragCompleted;
        }
    }

    private void Splitter_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
    {
        if(Orientation == Orientation.Horizontal)
        {
            for(int i = 0; i < _model.Children.Count; i++)
            {
                var element = _model.Children[i] as ILayoutPositionableElement;
                element.DockWidth = ColumnDefinitions[i].Width;
            }
        }
        else
        {
            for (int i = 0; i < _model.Children.Count; i++)
            {
                var element = _model.Children[i] as ILayoutPositionableElement;
                element.DockHeight = RowDefinitions[i].Height;
            }
        }
    }

#endregion
    }
}
