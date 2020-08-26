# GameUpdater Installer
This project adds an installer for the game launcher. Publish the GameUpdater first using the release configuration.
## Usage
1. Clone the repository
2. Publish/build the GameUpdater project ([see](GameUpdater/README.md))
3. Build the installer project using this command: ```msbuild  /p:Configuration=CONFIGURATION /p:Platform="PLATFORM"```. CONFIGURATION: Release|Debug, PLATFORM: x86|x64
