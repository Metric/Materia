Materia
===================
An open source alternative to Substance Designer written in C#. 

Want to ask a question, found a bug, need help, or have a suggestion?
===========================================================
Then feel free to drop into the official discord channel: https://discord.gg/VEW5cP7


Alpha RC 0.0.2 - Eye Example
--------------------------
![image](https://github.com/Metric/Materia/blob/master/alpha-rc.0.0.2.png)


What is the current state of it?
=================================
Pre-Alpha as quite a bit is still changing, and not all features are available yet.

Computer Requirements
------------------------
 - .Net 4.6.1 Runtime
 - OpenGL 4.4 compatible video card with as much vram as possible.
 - Windows 7, 8, or 10 64-Bit
 - Approx Hard Drive Space Needed: 300MB (Not Including .Net 4.6 Runtime)
 - Approx system memory required: 1GB+

Currently only runs on Windows as some Win32Api is used.

.mtg file format for graphs and nodes may change during these initial phases.

Help with Localization
=======================
If you are able to, please help localize the UI to your native language. The base language is English. For all current known data to localize: https://github.com/Metric/Materia/blob/master/Materia/Properties/Resources.resx

I could totally push this through google translate, but it is just horrible results sometimes. I would prefer an actual person to do the localization that knows the language they are translating to.

The modifed resource file should be saved as follows: Resources.Language Tag.resx. For example Spanish Brazil would be Resources.es-BR.resx or for general Spanish would be Resources.es.resx.

For Language Tag to use refer to: https://docs.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c?redirectedfrom=MSDN

If you don't want to do a pull request with the resource file change. Please post the file in Discord chat.

Major TODO
============== 
  * Various UI feedback mechanisms
  * Various shape nodes.
  * Keyboard Shortcuts
  * Add menu item to toggle Layers window / pane
  * Re-creation of various substance graph instances.
  * More Undo and Redo tracking operations.
  * Thorough testing of function graphs for both CPU and GPU.

Intel Processor Warning
========================
If you are using a laptop with both an Intel processor and a discrete GPU. Please make sure in your discrete GPU control panel, Materia is set to use the discete GPU, rather than Intel processor. Intel processor GPU is not supported in some cases, or may produce incorrect results, compared to a proper NVIDIA or Radeon discrete GPU.

Known Bugs
====================
* Splatter circle pivot of min and max is not working as expected anymore, due to FX using compute shader pivot point differently. Splatter circle center pivot is working as expected.
* MTG Renderer is no longer working. Needs to be updated to take into account the new way graphs are processed.
* AND and OR nodes do not add extra inputs as needed. Will be added back in later.
* Max and Min nodes do not add extra inputs as needed. Will be added back in later.
* Sequence Node does not add extra outputs as needed. Will be added back in later.
* The gradient editor positions, curves min/max and levels multi range sliders are still finicky to move with the mouse.

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
* Note: The command line MTG Renderer does not support the new .mtga format yet.
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

