Materia
===================

An open source alternative to Substance Designer written in C#. 

Alpha RC 0.0.2 - Eye Example
--------------------------
![image](https://github.com/Metric/Materia/blob/master/alpha-rc.0.0.2.png)

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
  * Various UI feedback mechanisms
  * Various shape nodes.
  * Keyboard Shortcuts
  * Re-creation of various substance graph instances.
  * Need to add gradient graphs.
  * More Undo and Redo tracking operations.
  * Thorough testing of function graphs for both CPU and GPU.
  * Icons for math nodes.
  * Allow custom icons for shelf display
  * Keep track of most recently opened

Known Bugs
====================
 * Color selector magnifier window fails to update on multiple displays when they are different scaling and resolution on Windows 10. This is an internal bug of the .Net framework. Already reported to the .Net developer forum. However, the actual color being selected is the correct color, even if the magnifier window fails to update.

  * Work around for now is to set scaling for all displays to 100% in Windows 10.

 * Hue, Saturation, Luminosity, and Color Blend modes are not working as expected.
 * The gradient editor positions are very finicky when trying to move them.
  * The gradient editor positions do not update their color icon in real time properly.
 * Various other finicky movements related to some other UI areas.

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
 * NLog via Nuget
 * All other dependencies are in this repository
 * Uses a custom build of the free ExtendedWpfToolkit that fixes some bugs in AvalonDock, uses proper WPF GridSplitter controls in AvalonDock and customizes AvalonDock Metro Theme.

Build Instructions
=====================
 * Load up the main solution
 * Make sure OpenTK, OpenTK control, Newtonsoft JSON, and NLog is referenced properly in Materia project
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

