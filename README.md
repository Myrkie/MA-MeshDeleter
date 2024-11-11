# MA-Mesh Deleter

## Overview
MA-MeshDeleter is a simple Modular avatar based plugin that will delete the associated vertices used by a BlendShape on build

## Installation

### Option 1: Git Package (Recommended)
1. Ensure you have Git installed on your system and registered in the `$PATH`.
2. Copy the Git URL: https://github.com/Myrkie/MA-MeshDeleter.git #versiontag
3. Open Unity and navigate to `Window` > `Package Manager`.
4. Click on the `+` button in the top-left corner of the Package Manager window.
5. Select `Add package from git URL...`.
6. Paste the copied Git URL into the input field.
7. Click `Add` to import the package into your Unity project.

[UPMGit Extension](https://github.com/mob-sakai/UpmGitExtension) is recommended if you wish to seamlessly change versions

### Option 2: Manual Download
1. Download or clone the source code of this repository.
2. Extract the downloaded ZIP file into your unity your projects **Packages** folder


## Usage
After installation, you can add the new component Skinned Mesh BlendShape Remover Behavior and select a BlendShape to be used as the target, this component must be place on the skin mesh you plan to modify.


## Compatibility
MeshDeleter is compatible with Unity versions 2022.3.6f1. Ensure that your Unity project meets the minimum version requirements to utilize these enhancements.

## Requirements
Mesh Deleter requires Modular Avatar it is a hard requirement, if you wish to use this with Chillout VR you will need [chillaxins](https://docs.hai-vr.dev/docs/products/chillaxins) by Haï~ VR
## Contributing
Contributions to MeshDeleter are welcome! feel free to submit a pull request or open an issue on this repository.

## License
This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements
MA-MeshDeleter utilizes [Modular avatar](https://modular-avatar.nadena.dev/)
