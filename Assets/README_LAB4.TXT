===============PREY===========================
Spawn with a left mouse click.

Prey will follow their rules and avoid collisions with the obstacles.
Prey have a 180 degree field of view, although this can be changed in
its script. For all rules, some range applies. In general, a radius
is used to keep track of other boids flocking will take place with.

See the script "PreyScript" in scripts folder to adjust settings.

Follows 5 rules.
1. Average boid direction in the area
2. Move away from nearby boids to make space
3. Match toward average boid velocity
4. Run from predators
5. Move toward a goal

==============PREDATOR==========================
Spawn with a mouse right click.

These attempt to attack prey within their vision and range.

See the script "PredatorScript in scripts folder to adjust settings.



==================AGENT MOTION=======================
Prey attempts to avoid predators, see PreyScript's function RunFromPredators();
Predators attempt to chase detected boids. 
	See PredatorScript's function ChaseClosestPrey() in conjunction with InSight()
	and WithinRadius();

==================AGENT VISION=======================
Predators have a small field of vision, see WithinRadius(), InSight(), and the
	variable sightAngle in PredatorScript.
Prey has a large field of vision. Same as above, except in PreyScript.

==================ADDITIONAL FEATURES================
Fixed Obstacles:
	Red barriers serve as obstacles. They're untagged 3D objects, but have a 
	WallScript.cs attached. This script will push away nearby Predators and Prey
	using forces in Unity's built in physics.

	Red barriers can be copied and moved in realtime, though this might cause 
	some boids to move through them as the moving is happening.

	Adjust strength of the wall's repulsion in WallScript.cs

Flocking and Ruleset:
	Prey will flock together, leave room for another, and move with each other's speed
	as seen in AverageBoidCorrection(), MoveAwayFromBoids(), AverageBoidVelocity () in
	the PreyScript.cs. Strengths for each of these rules can be adjusted in their function.

Goal Position:
	Prey will attempt to move toward the green circle since it's tagged "Goal" in Unity.
	see the function MoveTowardGoal() in PreyScript.cs.

	Mutiple goals can be created, Prey will move toward the closest one.
	Strength in which the prey moves towards the goal can be adjusted in
	MoveTowardGoal().

