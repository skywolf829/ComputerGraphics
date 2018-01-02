Skylar Wurster
Lab 6
CSE 3541


Fluid Flow Simulation

Inside of the object FluidSimulator, the script FluidSimulation.cs runs a fluid simulation.
I followed instructions as suggested by the professor from https://cg.informatik.uni-freiburg.de/intern/seminar/gridFluids_fluid_flow_for_the_rest_of_us.pdf.

=======HOW TO RUN=========
Adjust center/volume of the bounding box, if you wish. I actually haven't tested this.
Adjust cell size. Recommended 0.1 to start.
Adjust source velocity. Setting velocity components equal to each other will align particles on an axis,
disallowing movement. I.E, if you set the velocity to (1, 0, 0), then the particles will only move
on the x axis, resulting in a boring simulation.

Press play.
Space - Spawns a particle at 0, 0, 0
Enter - Advances field one timestep
C - enters continuous mode, realtime updating

Gizmos are used (see GRID CELLS), so please select the fluidSimulator object in the
heirarchy and enable gizmos on the scene and/or the game screen.

============GRID CELLS============
Grid cell size is adjustable before the simulation begins with the public variable cellSize (adjustable in the inspector).
Smaller grid sizes create a more unstable and slower simulation, and I've found that a size of 0.1 to 0.05 works 
sufficiently. 

Grid cells are updated in the beginning of the update cycle. They are dynamic, and will be removed and added as
the fluid moves, for efficiency. They're stored in a hashtable, using a hash based on the xyz index position in space,
as suggested by the paper.

Cells have a layer, to assist with some coding, and type, to indicate the contents of that cell.

A buffer zone is created around the fluid as well in the beginning of every update in order to always have a cell already
created.

See UpdateGrid() for the code here.

Gizmos are used to draw the type of cell, see Grid.DrawGizmos().


==========TIMESTEP===========
The timestep can be dynamic in order to assure that the fluid never moves through a cell completely, but for coherence
I've disabled that feature in substitution for a constant timestep.


===========FLUID DYNAMICS ALGORITHM=============
AdvanceVelocityField() takes care of adjusting the velocity field for the update cycle.
Each of the cells are update for the terms in the Naiver-Stokes equation.
A solver I found online is used for the sparse matrix solver.

Feel free to read the comments or code, but is too long to explain here.

Some things to note are the variables that can be changed.
Viscosity - lower is less particle to particle attraction
Gravity - acceleration due to gravity
Density - the density of the simulated fluid
AtmPressure - the number of atmospheres of pressure



=============ISSUES===================
Can't figure out why the fluid wont actually stack on itself in a volume.
Have discussed with the professor and Huamin Wang, and used some paper 
references, but I don't think I have the time to implement this.

All projects completed are available in a singule Unity project at 
https://github.com/skywolf829/ComputerGraphics