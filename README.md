# EntitySelection

[![openupm](https://img.shields.io/npm/v/io.github.jonasdem.entityselection?label=openupm&registry_uri=https://package.openupm.com)](https://openupm.com/packages/io.github.jonasdem.entityselection/)

A small package that enables you to select entities in the unity sceneview.
Works with every RenderPipeline.
![EntitySelectionGif](https://jonasdem.github.io/Media/EntitySelection.gif)

## How to select
1. Make sure you're in PlayMode & your focus is on the sceneview (not gameview)
2. Point to the entity with mouse pointer (first selection can take some time, depends on total amount entities with meshes)
3. Press the '1' key (Non Numpad)  
=> Inspector window should show all info for entity.

## How to get this package for your own project

### Install via OpenUPM

The package is available on the [openupm registry](https://openupm.com/). You can install it via [openupm-cli](https://github.com/openupm/openupm-cli#openupm-cli).

```
openupm add io.github.jonasdem.entityselection
```

### Install via git url

1. Click the green "Clone or download" button and copy the url.
2. In Unity go to Window>Package Manager and Press the + sign in the left-top corner of the Package Manager window.
3. Select "Add package from git URL...", Paste the URL and press "Add".
Done!

Or manually add the dependency to the Packages/manifest.json file.

```
{
    "dependencies": {
        "io.github.jonasdem.entityselection": "https://github.com/JonasDeM/EntitySelection.git"
    }
}
```

## Contribution
BugFixes and UX improvements are appreciated.
Performance Improvements are welcome, since I'm not prioritizing those.
No setup should ever be needed.

Thanks to [Tom](https://github.com/Moosichu) for making this repo a package.

## 2019.2 branch
If you want a 2019.2 (And probably 2019.1) compatible version, use the 2019.2 branch.
Use "https://github.com/JonasDeM/EntitySelection.git#2019.2" in the "Install via git url" steps.
