# ManifestTool
With this command line tool you can create your server manifest files faster!

## Usage
1. Clone the repository or download a release
2. Navigate to the ManifestTool project folder and build using ```dotnet build```
3. Run the executeable file with the necessary parameters

## Parameters
* -f, --f: File or Folder to add to patch
* -v, --version: Version of the patch
* -d, --depends-on: Version on which this patch depends on
* -u, --url: URL of the top level folder which contains all files relative to it
* -x, --xml: Path to current server manifest xml file (default: update.xml)
* -s, --save-location: Path to the generated server manifest xml file (source xml file will be overriden, if not provided)
* --md5: Should a md5 hash be created to provide checksum validating
* --sha1: Should a sha1 hash be created to provide checksum validating
* --zip: Creates an ZIP of the provided folder and creates a .checksum.xml file inside it, which contains the checksums of all files
