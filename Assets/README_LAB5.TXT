Skylar Wurster
Lab 5
CSE 3541


================INTERPOLATION====================
View the function ComputePointOnCatmullRomCurve for intpolation.
That will use the given u value to compute the position on
segment segmentNumber

================REPARAMETERIZATION BY ARC LENGTH================
In the start function, a lookup table is created to store the
u values for corresponding arc lengths. 
In the Update function, findUForDistance() is called based
on a distance given, which is updated by distanceStep each cycle

===============EASE IN EAST OUT=====================
In update, using the function findDistForTime() gives a 
distance for corresponding time, following the forumula 
d(t) = -2t^3 + 3t^2. Update uses the time.deltaTime as the
time increment. 

==============ORIENTATION================
Orientation is handled in the ComputePointOnCatmullRomCurve at the 
end, where transform.LookAt is called.