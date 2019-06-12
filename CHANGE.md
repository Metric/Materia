CHANGE LOG
=============

Alpha RC 0.0.2
-----------------

Fixes
* Quite a bit of fixes in math nodes
* Quite a few null reference exceptions taken care of
* Quite a few fixes in other nodes
* Fixed issue in blend glsl with alpha not working as expected and adding masking
* Fixed depth.glsl
* Various other fixes that I lost track of...

Additions
* SSS now available in the PBR shader via Thickness map
* You can now see the light source and reposition it
* You can now modify some material settings such as height, SSS, refaction index, etc.
* You can now load custom preview models (They are not saved with the graph currently)
* You can now export and import function graph nodes via right clicking the graph when in the function graph
* You can now add custom variables and custom functions
* Graph instance parameters can now be promoted as well
* Keyboardd shortcuts added for save (ctrl-s), undo (ctrl-z), redo (ctrl-y), copy (ctrl-c), paste (ctrl-v), add comment (c key), add pin (p key), and next pin navigation (tab key)
* Popup shelf added - can be accessed via space bar when focused in graph area
* FXNode added, not quite as powerful as substance's, but getting there
* New graph dialog added
* App icon created and added
* You can now resize multiply nodes at once via the toolbar in the graph
* You can now add comment nodes and pin nodes. Double click a pin node to set color. Double click header of comment to set comment.
* Added logging and a log window

Changes:
* Various UI improvements
* Underlying code restructure for some areas
* Graph instances now keep track of the parameter values differently (Your old .mtg file will not load the pre-existing variables of a graph instance)
    * This was done to save file space and eliminate duplication of data
* Other various changes here and there that I lost track of...

Pre-Alpha 0.0.1
-----------------

Fixes
* Fixed several crashes related to null checks
* Fixed Function Graph shader generation improperly generating shader code in the wrong order
* Fixed graph file missing its width and height properties when saving and loading
* Fixed crash when closing a tab that was not the active tab
* Fixed copy and paste not working on graph instances
* Fixed issue where graph instances within graph instances would fail to connect to their input nodes
* Fixed issue where graph instance width and height was not proportionally scaling
* Fixed some other bugs in graph instances
* Fixed issue where graph instance properties would not show up correctly
* Fixed issue where labels for graph input and outputs were in the way of the node connection
* Fixed issue where shelf UI would not populate from Shelf folder
* Fixed missing GetJson and FromJson for several nodes
* Fixed issues with various blend modes on the blend node
* Fixed blur.glsl; it had a missing + 1 on the averaging part.

Additions
* Most nodes can now promote a property to constant (for graph instance access) or promote to a function graph
    * The following are predefined variables for a function graph: (Float2)pos (Only usable in PixelProcessor Function Graph), (Float2)size, (Float)PI
    * Variables are case sensitive
    * A promoted property function graph also has access to the other properties on the node, except if the other property is promoted to a Function
        * If the property has a space in the name, then the variable name will have no spaces
* The float value of the float constant math node now can be promoted to a constant or function as well (other math nodes that have constant values will be added later on with more testing). 
    * By promoting this to a constant, it can be modified as a graph instance property.
* You can now change the current 3D Hdri lighting of a graph via Edit -> Graph Settings
* Included some more complex example graphs in the Example folder (Note these graph will only work with these latest changes due to various bug fixes etc.)
* Added filters for Curvature from Normal and Curvature Smooth from Normal
* Added simple search to shelf
* Added Gamma node

Changes
* Various rewrite for 3D preview and some classes

Known Issues:
* Undo and Redo seem to be borked in some cases
* Panning of the 3D object in the 3D preview is still borked
* When loading a PixelProcessor function graph, the view is not properly updated sometimes. 
    * The work around for now is to hit the actual size button to reset the view to 100% zoom.