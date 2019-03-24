Materia
===================

An open source alternative to Substance Designer written in C#. 

Why did I start making this?
============================
I got tired of paying the fee for the Substance suite.

What is the current state of it?
=================================
Pre-Alpha as quite a bit is still changing, and not all features are available yet.

Computer Requirements
------------------------
 * .Net 4.6.1 Runtime
 * OpenGL 3.3 compatible video card with as much vram as possible.
 * Windows 7, 8, or 10 64-Bit
 * Approx Hard Drive Space Needed: 300MB (Not Including .Net 4.6.1 Runtime)
 * Approx system memory required: 1GB+

Currently only runs on Windows as some Win32Api is used.

.mtg file format for graphs and nodes may change during these initial phases.

Major TODO
============== 
  * Unload graphs when switching tabs and reloading when going back into the tab. This will help optimize vram usage. Currently all graphs are kept in vram. (Still testing changes)
  * Various UI tweeks and UI feedback mechanisms need adding (Some have been implemented).
  * More keyboard shortcuts.
  * Popup node explorer via space bar with context sensitivity & searching.
  * Search for shelf
  * FxMap style nodes & graph.
  * Various shape nodes.
  * Graph instance node parameter exposure. (Still testing changes)
  * Function creation for node parameters. (Still testing changes)
  * Function graph export & import for sharing.
  * Re-creation of various substance graph instances.
  * Need to add Cartesian to Polar math node.
  * Need to add gradient and gradient mapping nodes
  * New Graph Dialog for setting some initial settings of the graph.
  * Splash screen - because why not.
  * More Undo and Redo tracking operations - currently only handles undo and redo of creating and deleting nodes.
  * Thorough testing of function graphs for both CPU and GPU.
    * Sampler nodes will only work in Pixel Processor Function Graph
  * Icons for function / math nodes.
  * Allow custom icons for shelf display of nodes
  * HDRI Environment Selector
  * Importing custom geometry for 3D preview
  * SSS PBR Shader
  * Option to modify 3D Camera Settings etc.
  * Multi node texture resize capability

Known Bugs
====================
 * Color selector magnifier window fails to update on multiple displays when they are different scaling and resolution on Windows 10. This is an internal bug of the .Net framework. Already reported to the .Net developer forum. However, the actual color being selected is the correct color, even if the magnifier window fails to update.

 * Work around for now is to set scaling for all displays to 100% in Windows 10.

Build Dependencies
===================
 * .Net 4.6.1
 * OpenTK via Nuget
 * OpenTK.GLControl via Nuget
 * Assimp via Nuget
 * All other dependecies are in this repository
 * Uses a custom build of the free ExtendedWpfToolkit that fixes some bugs in AvalonDock, uses proper WPF GridSplitter controls in AvalonDock and customizes AvalonDock Metro Theme.

Build Instructions
=====================
 * Load up the main solution
 * Make sure OpenTK and OpenTK control is referenced properly in Materia project
 * Make sure OpenTK and Assimp is referenced properly in RSMI (Really Simple Mesh Importer) project
 * Build
 * Copy items from AddToOutputDir to where the final built .exe is located
 * Run

 License
 =========
 MIT

 I want to support this project, how can I help?
 ================================================
 To Be Determined...

