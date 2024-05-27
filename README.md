Materia
===================
An open source alternative to Substance Designer written in C#. 


Alpha RC 0.0.2 - Eye Example
--------------------------
![image](https://github.com/Metric/Materia/blob/master/alpha-rc.0.0.2.png)


What is the current state of it?
=================================
Pre-Alpha as quite a bit is still changing, and not all features are available yet.

Computer Requirements
------------------------
 - .Net 4.6.1 Runtime
 - OpenGL 3.3 compatible video card with as much vram as possible.
    - OpenGL 4.1 compatible video card for real time tessellation displacement
 - Windows 7, 8, or 10 64-Bit
 - Approx Hard Drive Space Needed: 300MB (Not Including .Net 4.6 Runtime)
 - Approx system memory required: 1GB+

Currently only runs on Windows as some Win32Api is used.

.mtg file format for graphs and nodes may change during these initial phases.

Major TODO
============== 
  * Various UI feedback mechanisms
  * Various shape nodes.
  * Keyboard Shortcuts
  * Re-creation of various substance graph instances.
  * More Undo and Redo tracking operations.
  * Thorough testing of function graphs for both CPU and GPU.

Known Bugs
====================
* Color selector magnifier window fails to update on multiple displays when they are different scaling and resolution on Windows 10. This is an internal bug of the .Net framework. Already reported to the .Net developer forum. However, the actual color being selected is the correct color, even if the magnifier window fails to update.
  - Work around for now is to set scaling for all displays to 100% in Windows 10.
  - UPDATE: I just received an email that this issue has now been added to the dotnot WPF github issues tracker at: https://github.com/dotnet/wpf/issues/1320

* Sometimes node graph lines are not properly deleted, when deleting a node where the input is connected.
* For some weird reason, when pasting a node and when connecting the output of it to another node, it does not properly connect in the underly UI / graph.

How-To and Various Info
========================
How-to and various info on available features can be found in the github wiki: https://github.com/Metric/Materia/wiki


Build Dependencies
===================
 * .Net 4.6.1
 * OpenTK 3.0.1 via Nuget
 * OpenTK.GLControl 3.0.1 via Nuget
 * Assimp via Nuget
 * Newtonsoft JSON via Nuget
 * NLog via Nuget
 * All other dependencies are in this repository
 * Uses a custom build of the free ExtendedWpfToolkit that fixes some bugs in AvalonDock, uses proper WPF GridSplitter controls in AvalonDock and customizes AvalonDock Metro Theme.

Editor Build Instructions
=====================
 * Load up the main solution
 * Make sure Math3D Project is referenced in Interfaces project.
 * Make sure Assimp and Math3D project is referenced properly in RSMI (Really Simple Mesh Importer) project
 * Make sure Newtonsoft JSON, NLog, System.Drawing, Math3D Project, RSMI Project, Interfaces Project, and Archive Project is properly referenced in Core project
 * Make sure Core Project is referenced in Exporters Project.
 * Make sure OpenTK, OpenTK control, Newtonsoft JSON, NLog, DDSReader Project, Core Project, Math3D Project, Interfaces Project, Exporters Project, and Archive Project is referenced in Materia project
 * Build
 * May have to copy runtimes folder for assimp from RSMI project build folder to the built materia.exe location
 * May have to copy language folders for Avalon Dock from the Xceed.WPF.AvalanDock build folder to the built materia.exe location.
 * Copy items from AddToOutputDir to where the final built .exe is located
 * Run

Command Line MTG Renderer Build Instructions
=========================
* Note: The command line MTG Renderer does not suppor the new .mtga format yet.
* Load up the main solution
* Make sure OpenTK, OpenTK Control, Newtonsoft JSON, DDSReader Project, RSMI Project, Core Project, Math3D Project, Interfaces Project, Exporters Project, System.Drawing, and System.Windows.Forms is properly referenced in the MTG Renderer project
* Right click MTG Renderer Project -> Build
* May have to copy runtimes folder for assimp over from the RSMI project build folder to the built MTGRenderer.exe location
* Copy item from AddToOutputDir to where the final built .exe is located
* Run via command line with the following arguments:
    - exportType mtgFilePath exportFolderPath
      - exportType is a number from 0-2. 0 = Separate Files, 1 = Unity5 Compacted, 2 = Unreal4 Compacted

License
=========
MIT

I want to support this project, how can I help?
================================================
To Be Determined...

