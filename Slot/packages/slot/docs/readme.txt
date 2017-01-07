Release 0.1

Release 0.1 is a first public release of Slot. It offers only a small subset of the planned functionality. This is a preview release which can be used to get to know the basic Slot functions and the way it works. The most notable omissions from this release include build system/tasks support, linting, integrated output and macros. Extensions API is not yet stabilized. Autocomplete works but only using one generic strategy which is not yet configurable per mode.

Version 0.1 provides support for all basic editing functions (including multiple carets and multiple selections, rectangluar selections, multiple level undo and redo), indenting, folding, autocomplete and syntax highlighting. Slot 0.1.0 is shipped with the following language modes out of box: C#, JSON, HTML, JavaScript and XML.

Other notable features include:

    * Split editing of the same buffer (command v-n)
    * Open multiple files at once and switch between them instantly (Ctrl+Tab by default)
    * Automatically save editing sessions
    * Fully control file encodings when saving/reading files (UTF-8 or UTF-8 No BOM is assumed by default)
    * Workspaces with an ability to override settings per workspace level by placing settings.json file in the workspace subfolder under the name of '.slot'
    * Match brackets configurable per mode
    * Match words configurable per mode
    * Incremental search with support for regular expressions
    * Indicators in the vertical scroll bar (such as search results, current carets positions, etc.)
    * Support for visual themes

Slot 0.1 comes with two visual themes out of box - 'Default Dark' (enabled by default) and 'Default Light'. You can switch between themes using command 'theme'.

Please report any found issues and make feature requests at GitHub:
https://github.com/vorov2/slot/issues

Release history:
0.1.1 - Stabilization release, bug fixes
0.1.0 - Initial release
