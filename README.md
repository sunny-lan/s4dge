# S4DGE
Welcome to S4DGE, a Unity extension for rendering of 4D objects and spaces in your games, written in C# and HLSL.

This is an early preview of the project for demonstration purposes only, the project will be publicly available as an open source project in April 2024. It was developed by a group of Software Engineering students at the University of Waterloo as a capstone project.

AUTHORED BY: 
https://github.com/royi-luo
https://github.com/sunny-lan
https://github.com/lukeKlassen
https://github.com/boonboonsiri
https://github.com/RichardYSun

# Overview
The S4DGE system includes two renderers for 4D Game Objects for comparing and verifying rendered results, though both renders have unique advantages. In general, the **Rasterizer** will be more performant and combatible with existing projects.

To set up 4D rendering in a unity scene, an additional `Camera 4D` should be added to the active camera, with either the `Raycast 4D` or `Rasterize Camera` script depending on which renderer you will be using. Note that every 4D related object (including lights and the camera) will have a `Transform 4D` script which will override their 3D `transform`.

The shape of 4D Game Object is based (usually) on a `Tet Mesh Renderer 4D` script with an appropriate tetrahedral mesh file. Similar to a triangle mesh in 3D, a tetrahedral mesh can be used to fully describe 4D objects. Tetrahedral meshes for regular 4D polytopes are available in the project, along with some more unique parametric shapes.

## Raytracer
The raytracing renderer bounces light through the entire scene to determine reflections and shadows. It can naturally determine shadows and reflections as a result.
![raytraced_4d](https://github.com/sunny-lan/se390-1/assets/32170884/ce9d415a-a92d-4a8b-89e2-7e2fbd1c63dd)

![raytraced_lighting](https://github.com/sunny-lan/se390-1/assets/32170884/c57314cd-2897-48ec-be46-f4c1341cfb40)

## Rasterizer
The rasterizing renderer transforms the mesh into a 3D and then 2D image on screen. It requires extra work behind the scenes for shadows and lighting, but can more efficiently render the image.

![helix_hypersphere](https://github.com/sunny-lan/se390-1/assets/32170884/34c9546f-d326-47d7-89f7-039ca4128b89)
