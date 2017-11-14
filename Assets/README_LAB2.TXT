=============MODEL GENERATION==========
The model was created from a scirpt called GenerateMesh.cs. This is meant to be
an editor script, generating an object before the game starts. It parses an
FBX file (head.txt). Vertices, normals, and triangles are gathered from the
file, and a mesh is constructed. The asset is instantly added to the active
scene when it creates it's own gameobject. 

To use this, add the GenerateMesh.cs script onto any gameObject in the scene.
From GenerateMeshEditor.cs, a custom editor was built to more intuitively
click a "generate" button to create the mesh. Any FBX file can be read this
way.

==========MODEL MOTION=============
Motion is controlled with MovementScript.cs. It is attached to the object we'd 
like to move. In this case: the generated head object. Every update cycle,
check WSAD to see what directions the head should move. Each button
adds their corresponding direction (times a scalar) to a "dir" vector.
At the end of the update cycle, the head's position is moved toward "dir"
simply by adding it to its position. Since this isn't physics based, the
head must then be adjusted to be above the terrain, using a built in function.
Lastly, the head looks toward the position it moved, using transform.lookAt().

The head does have a collider, and so it won't move through the trees on 
the terrain.

Position can be reset by pressing 'r'.

===========TERRAIN GENERATION=============
During my 2016-2017 year at OSU, I research with Dr. Roger Crawfis and Paul Kim
in the CSE Graphics department. We worked on a new maze generation algorithm,
and I made a few implementations of code that would be useful once the 
algorithm is created. Scripts inside of Editor>Scripts are all relevant to that.

I used my earliest implementation (which was later scrapped) to generate the
terrain we see in this scene. This is MazeGenerator.cs.

MazeGenerator.cs: 
Open via Window>Maze GeneratorUnity editor script that 
generates a single terrain (Use created Tiles doesn't work). Maze 
width and height can be selected. VEBP and HEBP stand for Vertical edge
bit vector and horizontal edge bit vector, defining a maze. Randomize
Bit Vectors button will generate a new spanning tree and turn that into 
a VEBP and HEBP. As width and height scale, this algorithm takes exponentially
longer. I recommend staning below 15x15. Prim's algorithm is used on
a library I built.

Now aesthetic choices come in to play. Since this is a maze defined,
select maze attributes. I selected 50 for the width and length, and
60 for the wall height. Textured and Smoothed heightmap was
selected. Then, Texturing base on heightmap was selected and use trees.
Click Add texture once. Select 2 textures, one for the path and one
for the walls. Choose a height level for each (I use 0 for path and 20
for walls, since smoothing will reduce height). Select a tree gameobject.
Select a texture number to put the tree on. Minimum distance was 
set to 0.5, and probability to create is 0.015. Smoothing can
be played with (as will all the other numbers), but r=3 and 1 pass
is generally good.

No need to enter a path to save, but it will overwrite the last
one generated unless you change the terrainData object name after
creation (I have for the current one in the scene, so feel free
to create your own). Create will instantly add the new terrain
into the active scene and save the terrain data to the Assets.

Other scripts are available, most notably the newer one that
I presented with at the Denman in March 2017, but that'll take
more time to explain. Feel free to test stuff out! Contact me
if you have questions.

==========SCENE ROTATION=========
RotateScript.cs is added onto the camera object. Since I used
Unity's built in terrain, we can't rotate that. Instead, I 
rotate the camera. Pressing 0 rotates the camera about the
orgin of the scene, which is where the terrain is centered.
If the terrain wasn't centered, this would look funny. 
Note: movement will continue in the axis it previously went, 
which may be unintuitive. 