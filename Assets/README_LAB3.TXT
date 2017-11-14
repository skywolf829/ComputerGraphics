=============CAMERA MOVEMENT==========
Move the camera around with WSAD and QE to move down and up.
Click and drag to change where the camera is looking.

==========PARTICLE CREATION=============
Two particle systems exist. One is the ParticleCreator, the other is 
inside of GameLogic.

ParticleCreator:
Allows adjusting of paramters to control the spawning of particles that will move
from a start position to an end position, changing color along the way.
Controlled randomness allows the user to select variances for each value.
Particles have a lifespan and will be destroyed when they reach that time.

GameLogic:
Pressing Space at any time will create the current selected spring
particle system in the script in GameLogic. Here you can create
a sheet, hanging sheet, or cube, all based on springParticles.
Size, particle #, and spring constants can be change. Dampening is 
added as a choice, with the best results coming between 0.5 and 2,
depending on what you're trying to model.
Gravity is adjustable for each creation.
Pin Corner Particles allows the object to stay suspended in air
so you can pull on pieces to see it react, without the
collisions being in the way.
***Can click and drag any spring particle created to see how it affects
the other particles***

===========PARTICLE MOVEMENT=============
All particles use force, acceleration, and velocity to adjust 
the transform.position. 

==========PARTICLE COLLISION=================
Only collision with planes is implemented. Any (large) plane of any
angle will work. Plane must be tagged "Collidable" in Unity.
See PointPlaneIntersection() and CheckCollisions()
methods in the spring classes. 

==========PARTICLE COLLISION RESPONSE=================
See CheckCollisions() method. Reflects the particle off of the
plane with a dampened effect (currently hard coded at 0.9 of the 
calculated velocity).

