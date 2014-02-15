$CutsceneModuleServer::Event_Camera = true;

//DEPENDENCY FUNCTIONS
function Cutscene::SetCameraOrbit(%this, %type, %target, %dist, %mat, %min, %max, %freeze)
{
	if(%this.viewers == 0)
		return false;
	%gotit = false;
	for(%i = 0; %i < %this.viewers; %i++)
	{
		%client = %this.viewer[%i];
		if(!isObject(%client) || !isObject(%client.camera))
			continue;
		switch$(%type)
		{
			case "Point": %client.camera.setOrbitPointMode(%target, %dist);
			case "Object": %client.camera.setOrbitMode(%target, %mat, %min, %max, %dist, false);
			default: continue;
		}
		%client.camera.mode = "Observer";
		%client.setControlObject(%client.camera);
		if(%freeze && isObject(%client.dummyCamera))
			%client.camera.setControlObject(%client.dummyCamera);
		%gotit = true;
	}
	return %gotit;
}

function Cutscene::SetCameraTransform(%this, %transform, %freeze)
{
	if(%this.viewers == 0)
		return false;
	%gotit = false;
	for(%i = 0; %i < %this.viewers; %i++)
	{
		%client = %this.viewer[%i];
		if(!isObject(%client) || !isObject(%client.camera))
			continue;
		%client.camera.setTransform(%transform);
		%client.camera.mode = "Observer";
		%client.setControlObject(%client.camera);
		if(%freeze && isObject(%client.dummyCamera))
			%client.camera.setControlObject(%client.dummyCamera);
		%gotit = true;
	}
	return %gotit;
}

//CLASS FUNCTIONS
function CECamera::EventCheck(%this, %nospam) //here we go
{
	if(%this.type !$= "Camera")
		return false;

	%gross = false;
	//Assign all the relevant parameters to local variables.
	%mode = %this.getParameter("Mode");
	%targetType = %this.getParameter("TargetType");
	%target = %this.getParameter("Target");
	%minDist = %this.getParameter("minDist");
	%maxDist = %this.getParameter("maxDist");
	%dist = %this.getParameter("Dist");
	%mat = %this.getParameter("Mat");
	%id = %this.getParameter("Identifier");

	//Highest-level switch for the camera mode (Parameters may mean different things depending on mode and target type)
	switch$(%mode)
	{
		case "Orbit": //Mode using the setOrbit(Point)Mode method.
			switch$(%targetType)
			{
				case "Point": //Used to orbit around a specified position
					if(getWordCount(%target) < 3)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Point3F expected for Target; got \'" @ %target @ "\')");
						%gross = true;
					}
					if(%dist $= "" || %dist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for Dist; got \'" @ %dist @ "\')");
						%gross = true;
					}
				case "Object": //Used to orbit a specific object
					if(!isObject(%target))
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Non-existent object for Target; got \'" @ %target @ "\')");
						%gross = true;
					}
					else if(%target.getPosition() $= "")
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Object given has no position)");
						%gross = true;
					}
					if(%mat $= "")
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Transform expected for Mat)");
						%gross = true;
					}
					if(%minDist $= "" || %minDist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for minDist; got \'" @ %minDist @ "\')");
						%gross = true;
					}
					if(%maxDist $= "" || %maxDist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for maxDist; got \'" @ %maxDist @ "\')");
						%gross = true;
					}
					if(%dist $= "" || %dist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for Dist; got \'" @ %dist @ "\')");
						%gross = true;
					}
				case "SID": //Used to orbit an object created by an event occuring before this one
					if(!isObject(%event = %this.parent.findEventBySID(%target)))
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (SID given points to a non-existent event)");
						%gross = true;
					}
					else if(!%event.EventSpawnsStuff())
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Event pointed to does not spawn an object)");
						%gross = true;
					}
					else if(%this.position < %event.position)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Position prior to execution of event pointed to)");
						%gross = true;
					}
					if(%mat $= "")
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Transform expected for Mat)");
						%gross = true;
					}
					if(%minDist $= "" || %minDist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for minDist; got \'" @ %minDist @ "\')");
						%gross = true;
					}
					if(%maxDist $= "" || %maxDist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for maxDist; got \'" @ %maxDist @ "\')");
						%gross = true;
					}
					if(%dist $= "" || %dist < 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Positive float expected for Dist; got \'" @ %dist @ "\')");
						%gross = true;
					}
					if(%id $= "")
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (No event object identifier given)");
						%gross = true;
					}
				default:
					if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Target type is not Point, Object, or SID; got \'" @ %targetType @ "\')");
					%gross = true;
			}
		case "Return":
			switch$(%targetType)
			{
				case "Player":
					if(%this.parent.viewers == 0)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (There are no viewers)");
						%gross = true;
					}
				case "Object":
					if(%this.parent.viewers > 1)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Only one viewer allowed for control to object)");
						%gross = true;
					}
					if(!isObject(%target))
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Target doesn't exist)");
						%gross = true;
					}
					if(%target.getPosition() $= "")
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Target has no position)");
						%gross = true;
					}
				case "SID":
					if(%this.parent.viewers > 1)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Only one viewer allowed for control to object)");
						%gross = true;
					}
					if(!isObject(%event = %this.parent.findEventBySID(%target)))
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (SID given points to a non-existent event)");
						%gross = true;
					}
					else if(!%event.EventSpawnsStuff())
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Event pointed to does not spawn an object)");
						%gross = true;
					}
					else if(%this.position < %event.position)
					{
						if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Position prior to execution of event pointed to)");
						%gross = true;
					}
				default:
					if(!%nospam)
							schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Target type is not Player, Object, or SID; got \'" @ %targetType @ "\')");
					%gross = true;
			}
		case "Simple":
			if(getWordCount(%target) < 7)
			{
				if(!%nospam)
					schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Transform expected for Target; got \'" @ %target @ "\')");
				%gross = true;
			}
		default:
			if(!%nospam)
					schedule(0, 0, warn, "CECamera::EventCheck -" SPC %this.getName() SPC "has an invalid parameter! (Mode is not Orbit, Simple, or Return; got \'" @ %mode @ "\')");
			%gross = true;
	}
	return !%gross;
}

function CECamera::EventExecute(%this)
{
	if(%this.type !$= "Camera")
		return false;
	if(!isObject(%camera = %this.parent.viewer.camera))
		return false;

	//Assign relevant parameters to variables
	%mode = %this.getParameter("Mode");
	%targetType = %this.getParameter("TargetType");
	%target = %this.getParameter("Target");
	%minDist = %this.getParameter("minDist");
	%maxDist = %this.getParameter("maxDist");
	%dist = %this.getParameter("Dist");
	%mat = %this.getParameter("Mat");
	%id = %this.getParameter("Identifier");
	%freeze = !%this.getParameter("NoFreeze");

	switch$(%mode)
	{
		case "Orbit":
			switch$(%targetType)
			{
				case "Point":
					if(getWordCount(%target) < 3) //Quick sloppy way to detect a Vector3f value.
						return false;
					if(%dist $= "" || %dist < 0) //Making sure distance is a positive number.
						return false;
					%this.parent.SetCameraOrbit("Point", %target, %dist);
				case "Object":
					if(!isObject(%target))
						return false;
					if(%target.getPosition() $= "") //If the target doesn't have a position, AKA can't be orbited
						return false;
					if(%minDist $= "" || %minDist < 0)
						return false;
					if(%maxDist $= "" || %maxDist < 0)
						return false;
					if(%dist $= "" || %dist < 0)
						return false;
					if(%mat $= "") //Mat is a transform value, but afaik it's not completely necessary anyway.
						return false;
					%this.parent.SetCameraOrbit("Object", %target, %dist, %mat, %minDist, %maxDist, %freeze);
				case "SID":
					if(!isObject(%event = %this.parent.findEventBySID(%target))) //Making sure the SID points to an existing event
						return false;
					if(%event.position > %this.position) //If we come before the event pointed to
						return false;
					if(!isObject(%obj = %event.GetSpawnedObject(%id))) //If the object we're looking for wasn't spawned by the event
						return false;
					if(%obj.getPosition() $= "") //The same as Object from here on out
						return false;
					if(%minDist $= "" || %minDist < 0)
						return false;
					if(%maxDist $= "" || %maxDist < 0)
						return false;
					if(%dist $= "" || %dist < 0)
						return false;
					if(%mat $= "")
						return false;
					%this.parent.SetCameraOrbit("Object", %obj, %dist, %mat, %minDist, %maxDist, %freeze);
				default:
					return false;
			}
		case "Return":
			switch$(%targetType)
			{
				case "Player":
					if(%this.parent.viewers > 0)
						%this.parent.ReturnPlayerControl();
					else
						return false;
				case "Object":
					if(%this.parent.viewers > 1)
						return false;
					if(!isObject(%target))
						return false;
					if(%target.getPosition() $= "")
						return false;
					%this.parent.viewer0.setControlObject(%target);
				case "SID":
					if(%this.parent.viewers > 1)
						return false;
					if(!isObject(%event = %this.parent.findEventBySID(%target))) //Making sure the SID points to an existing event
						return false;
					if(%event.position > %this.position) //If we come before the event pointed to
						return false;
					if(!isObject(%obj = %event.GetSpawnedObject(%id))) //If the object we're looking for wasn't spawned by the event
						return false;
					if(%obj.getPosition() $= "") //The same as Object from here on out
						return false;
					%this.parent.viewer0.setControlObject(%obj);
				default:
					return false;
			}
		case "Simple":
			if(getWordCount(%target) < 7)
				return false;
			%this.parent.setCameraTransform(%target, %freeze);
		default:
			return false;
	}
	return true;
}