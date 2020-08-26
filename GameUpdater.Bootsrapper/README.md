# GameUpdater Bootstrapper
This project ads the wix bootstrapper pipeline to enable install chains and burn UI. This installer depends on the [GameUpdater.Installer](GameUpdater.Installer) project, but builds the corresponding installer file automatically.
## Usage
1. Clone the repository
2. Publish/build the GameUpdater project ([see](GameUpdater/README.md))
3. Build the installer project using this command: ```msbuild  /p:Configuration=CONFIGURATION /p:Platform="PLATFORM"```. CONFIGURATION: Release|Debug, PLATFORM: x86|x64
