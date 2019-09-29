# Overview
[![Build status](https://ci.appveyor.com/api/projects/status/nhryaguc3murmq8q?svg=true)](https://ci.appveyor.com/project/RomanShapiro/opensora)

Various experiments with game "Trails in the sky".

For now it consists only from OpenSora.Viewer, which is utility to view "Trails in the Sky FC" textures and models.

![](/images/OpenSora.png)

# Usage
1. Download latest binary release(OpenSora.v.v.v.v.zip) from https://github.com/rds1983/OpenSora/releases
2. Unpack it
3. Run "OpenSora.Viewer.exe"
4. Click `Change...` button and choose folder with Sora FC(I.e. `D:\Games\Steam\steamapps\common\Trails in the Sky FC`)
5. Enjoy

# Camera Controls
Use W, A, S, D to move camera.
Hold right button and move mouse to rotate it.

# Building from source code
1. `git clone https://github.com/rds1983/OpenSora.git`
2. `git submodule update --init --recursive`
3. Open solution from the build folder in VS 2017+

# Credits
* [uyjulian](https://github.com/uyjulian)
* [MonoGame](http://www.monogame.net/)
* [Myra](https://github.com/rds1983/Myra)
* [Nursia](https://github.com/rds1983/Nursia)
