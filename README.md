Materia .Net Core 3.1 Branch
===================
This branch is not currently functional. UI Components are still being rebuilt for AvaloniaUI. Thus, not a full working app yet.

It will also be constantly changing.

Current Branch Stage
=========================

* Refactored file locations for rendering classes mainly. Graph and Node classes remain unaffected. Updated to use .Net Core 3.1 and .Net Standard 2.1. Also uses the new MathF where needed.
* Using less sub projects for overall building
* Embedding data in the .Net Standard 2.1 Libraries (Vertex Shader, Etc)
	- This will help with cleaner and easier building of the project
* Using AvaloniaUI for cross-platform capabilities. May have to build directly from github repo, since they have fixed memory issues and etc that are not included in the Nuget package currently.
* Will be incorporating OpenTK 4.0 for various bug fixes and cleaner separation of code
* Added HDR file support. Will now convert the HDR to a cube map for irradiance and prefilter for PBR shading at runtime.
* Added VCDiff for keeping differiental undo / redo changes of the graph at runtime.
* Will be a different format window layout wise, since avalonia does not create Hwnd handles for each UserControl, and thus cannot embed 3D / 2D GL Preview into a control. Therefore, the 3D Preview and 2D Preview will need to be their own windows.
	- The menus for these UserContols in the previous version, will be moved to the main windows menu layout for this version.
	- The layers, shelf, and properties will also become their own windows as well
	- Will need to recreate the tab interface for documents in the main window, since we can no longer use AvalonDock.
	- Will need a new method for storing window position / size info for settings.











