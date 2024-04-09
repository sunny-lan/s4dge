# S4DGE Setup

The easiest way to set up your 4D scene is to check out any of the demo scenes for the renderer you've chosen in `Assets/S4DGE/Scenes`.
The exact scripts required for rendered game objects are as follows, also available on prefabs in `Assets/S4DGE/Prefabs`.

### Raytracer
- Camera: `Scene4D`, `Raycast4D` (with the `Custom/Raytracing` and `Custom/Accumulate` shaders), `Transform4D`
- 4D Game Objects: `Transform4D`, and some `RayTracedShape` inheriting rendering script (eg. `TetMeshRenderer` with a chosen tetrahedral mesh file or `RaytracedHypersphere`)

### Rasterizer
- Camera: `Camera4D`, `Transform4D`, `RasterizeCamera`. Note that the 3D position of the camera will change the projection plane. Recommended 3D position is (0,1,-10)
- Lights: `LightSource4D`, `Transform4D`, `ShadowMapGenerator`
- 4D Game Objects: `Transform4D`, `TriangleMesh` (with 4D material selected), `TetMeshRenderer4D` (with plugged in shaders), and a mesh generating file (eg. `RasterizeTetMeshUnityObj` with a chosen tetrahedral mesh file or `RasterizeHypersphere`)