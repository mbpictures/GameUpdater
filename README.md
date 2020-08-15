# GameUpdater
With this project, you can add a simple GameUpdater/Launcher to your Unity/UnrealEngine (or whatever game engine you prefer) game. This project uses [AvaloniaUI](https://github.com/AvaloniaUI/Avalonia) to provide an easy and flexible way to support cross-platform GUI.

## Usage
1. Clone the repository or download a release
2. Build the project using ```dotnet build``` or ```dotnet publish -r RUNTIME_IDENTIFIER -c Release /p:PublishSingleFile=true --self-contained false``` (replace RUNTIME_IDENTIFIER with your target platform, e.g. osx-x64, linux-x64, windows-x64,...)
3. Put the [sample INI file](GameUpdater.ini) and the [update.xml](update.xml) under the same hierarchy as the executable (otherwise, the Launcher won't start!)
4. Upload the Patch-Manifest to your web space of choice, for example [this one](http://projects.marius-butz.de/updater/update.xml)
5. Adjust the fields for ```GameExe``` (relative path with your game executable), ```GameDirectory``` (relative path with your game folder, this folder will be deleted when the user uninstalls the game using the Updater), ```LocalManifest``` (relative path with local manifest file) and ```ServerManifest``` (URL to the server manifest file, which you uploaded in step 4). Optionally you can set ```UseUniversalUninstall``` to ```False``` and set the ```MsiName``` to the name of your game installer, this will use the windows native uninstaller to uninstall the game as intended (only works for windows!).
6. Ship your game with the launcher and create a shortcut for the user to the Launcher

## Functionality
Each start the Updater checks, whether the version specified in the local manifest file is the newest available patch. If the server manifest file contains a newer patch, the client will incrementally update the game files by resolving all patches from the newest down until the local version is reached. After that, all files in the ```<files>``` element will be downloaded. To provide the user the opportunity to check his local files for corruption, all files in the server manifest can contain an optional ```<md5>``` or ```<sha1>``` element with the respective hash of the file. The client checks all files for all patches. When at least one hash isn't equal, the file will be added to a list of corrupted files. Should a newer patch updating the file available and the integrity of the file is confirmed, the file will be removed from the list of corrupted files, as it got overridden through a patch.

## Customization
You can customize the wallpaper and the program icon by replacing the corresponding ([LauncherWallpaper.png](Assets/LauncherWallpaper.jpg), [avalonia-logo.ico](Assets/avalonia-logo.ico)) image files. You can customize the overall appearence using the [Theme.xaml](Assets/Theme.xaml) file.

## Disclaimer
This project was primarily created to get myself into C# and cross-platform GUI development using AvaloniaUI. Don't use this project as a template or to learn C# nor AvaloniaUI.
