Materia
===================

An open source alternative to Substance Designer written in C#. 

A simple setup
-----------------
![image](https://github.com/Metric/Materia/blob/master/screenshot1.png)

Pre-existing Textures
---------------------
![image](https://github.com/Metric/Materia/blob/master/screenshot2.png)


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
  * Various UI tweeks and UI feedback mechanisms need adding (Some have been implemented).
  * More keyboard shortcuts.
  * Popup node explorer via space bar with context sensitivity & searching.
  * FxMap style nodes & graph.
  * Various shape nodes.
  * Function graph export & import for sharing.
  * Re-creation of various substance graph instances.
  * Need to add gradient graphs
  * New Graph Dialog for setting some initial settings of the graph.
  * Splash screen - because why not.
  * More Undo and Redo tracking operations - currently only handles undo and redo of creating and deleting nodes.
  * Thorough testing of function graphs for both CPU and GPU.
    * Sampler nodes will only work in Pixel Processor Function Graph
  * Icons for function / math nodes.
  * Allow custom icons for shelf display of nodes
  * Importing custom geometry for 3D preview
  * SSS PBR Shader
  * Option to modify various material settings: height scale, refractive index, etc.
  * Multi node texture resize capability
  * Add a real log file logger
  * Add a log window, allow log window verbosity to be modified
  * Keep track of most recently opened / used graphs properly

Known Bugs
====================
 * Color selector magnifier window fails to update on multiple displays when they are different scaling and resolution on Windows 10. This is an internal bug of the .Net framework. Already reported to the .Net developer forum. However, the actual color being selected is the correct color, even if the magnifier window fails to update.

 * Work around for now is to set scaling for all displays to 100% in Windows 10.

 * Undo and redo do not work in function graphs currently

How-To and Various Info
========================
How-to and various info on available features can be found in the github wiki: https://github.com/Metric/Materia/wiki


Build Dependencies
===================
 * .Net 4.6.1
 * OpenTK via Nuget
 * OpenTK.GLControl via Nuget
 * Assimp via Nuget
 * Newtonsoft JSON via Nuget
 * All other dependencies are in this repository
 * Uses a custom build of the free ExtendedWpfToolkit that fixes some bugs in AvalonDock, uses proper WPF GridSplitter controls in AvalonDock and customizes AvalonDock Metro Theme.

Build Instructions
=====================
 * Load up the main solution
 * Make sure OpenTK, OpenTK control, and Newtonsoft JSON is referenced properly in Materia project
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

