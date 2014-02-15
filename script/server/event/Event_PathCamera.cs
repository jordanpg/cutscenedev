$CutsceneModuleServer::Event_PathCamera = true;

//garbage testing functions
function CEPathCamera::addCamStack(%this, %client, %inc)
{
	if(!isObject(%stack = %client.camShapeStack))
		return;

	%ct = %stack.getCount();
	for(%i = 0; %i < %ct; %i++)
		%this.PCAddNode(%inc * %i, %stack.getObject(%i).getTransform(), "Normal", "Linear");
}

//TYPE-SPECIFIC DEPENDENCIES
function getPathCamSpeed(%posA, %posB, %secs)
{
	%secs = mAbs(%secs); //Make seconds positive
	if(%secs == 0) //If it's zero (or a string)...
		%secs = 1; //Set it to one second
	%dist = VectorDist(%posA, %posB); //Speed to get to destination on one second
	%speed = %dist / %secs; //Divide it by the seconds to get it where we want it
	return %speed;
}

//TYPE-SPECIFIC METHODS
function CEPathCamera::PCAddNode(%this, %position, %transform, %type, %path)
{
	if(%this.type !$= "PathCamera" || %position $= "" || getWordCount(%transform) <= 0)
		return;

	if(!isObject(%group = %this.getSpawnedObject("NodeGroup")))
		%group = %this.EventOnAssign();

	if(%type !$= "Normal" && %type !$= "Position Only" && %type !$= "Kink")
		%type = "Normal";

	if(%path !$= "Linear" && %path !$= "Spline")
		%path = "Linear";

	if(%position < 0)
		%position = 0;

	if(getWordCount(%transform) < 7)
		%type = "Position Only";

	%group.node[%group.nodes] = %position TAB %transform TAB %type TAB %path;
	%group.nodes++;

	if(%position < %group.earliestPosition)
		%group.earliestPosition = %position;
	if(%position > %group.latestPosition)
		%group.latestPosition = %position;

	return %group.nodes;
}

function CEPathCamera::PCCreateCam(%this)
{
	%parent = %this.parent;
	%group = %this.getSpawnedObject("NodeGroup");
	if(%this.type !$= "PathCamera" || %parent.viewers == 0 || !isObject(%group) || %group.nodes <= 0)
		return -1;

	%transform = getField(%group.node0, 1);
	%position = getWords(%transform, 0, 2);
	if(getField(%group.node0, 2) !$= "Position Only")
		%rotation = getWords(%transform, 3, 6);
	else
		%rotation = "0 0 0 0";
		
	%cam = new PathCamera("PathCam_" @ %parent.label @ "_" @ %this.sid)
			{
				datablock = "CutsceneCamera";

				event = %this;

				position = %position;
				rotation = %rotation;
			};
	%this.registerSpawnedObject(%cam, "PathCamera");
	return %cam;
}

function CEPathCamera::PCRemoveNode(%event, %rem)
{
	%this = %event.getSpawnedObject("NodeGroup");
	if(!isObject(%this))
		return false;
	%ct = %this.getCount();
	if(%rem > (%ct - 1) || %ct <= 0)
		return false;

	%this.node[%rem] = "";
	for(%i = (%ct - 1); %i >= %rem; %i--)
	{
		%this.node[%i] = %this.node[%i+1];
		%this.node[%i+1] = "";
	}
	return true;
}

function CEPathCamera::PCInsertNode(%event, %add, %node)
{
	%this = %event.getSpawnedObject("NodeGroup");
	if(!isObject(%this))
		return false;
	%ct = %this.getCount();
	if(%add > %ct || getFieldCount(%node) < 4)
		return false;

	for(%i = (%ct - 1); %i >= %rem; %i--)
		%this.node[%i+1] = %this.node[%i];
	%this.node[%rem] = %node;
	return true;
}

function CEPathCamera::PCPushBack(%this, %node) //idk why ok mom....
{
	%this.PCInsertNode(0, %node);
}

function CEPathCamera::PCSortNodes(%this)
{
	%group = %this.getSpawnedObject("NodeGroup");
	if(!isObject(%group))
		return false;

	for(%i = 0; %i < %group.nodes; %i++)
	{
		for(%k = 0; %k < (%ct - %i - 1); %k++)
		{
			%posA = getField(%group.node[%k], 0);
			%posB = getField(%group.node[%k + 1], 0);
			if(%posA > %posB)
			{
				%tmp = %group.node[%k];
				%group.node[%k] = %group.node[%k + 1];
				%group.node[%k + 1] = %tmp;
			}
		}
	}
	return true;
}

//EVENT TYPE METHODS
function CEPathCamera::EventOnParameter(%this, %name, %value, %newval)
{
	if(%this.type !$= "PathCamera")
		return;

	%group = %this.getSpawnedObject("NodeGroup");
	if(!isObject(%group))
		%group = %this.EventOnAssign();

	if(%name $= "StartTransform" && !%newval)
		%group.PCPushBack(%this.position TAB %value TAB "Normal" TAB "Linear");
	else if(%name $= "StartTransform")
		%group.node0 = %this.position TAB %value TAB "Normal" TAB "Linear";

}

function CEPathCamera::EventOnAssign(%this)
{
	if(%this.type !$= "PathCamera")
		return;

	if(!isObject(%this.getSpawnedObject("NodeGroup")))
	{
		%group = new ScriptObject("NodeGroup_" @ %this.parent.label @ "_" @ %this.sid)
					{
						class = "NodeGroup";

						nodes = 0;
						earliestPosition = 0;
						latestPosition = 0;
					};
		%this.registerSpawnedObject(%group, "NodeGroup");
		return %group;
	}
}

function CEPathCamera::EventCheck(%this, %nospam)
{
	if(%this.type !$= "PathCamera")
		return false;

	%gross = false;
	//no necessary parameters for this lol
	%nodeGroup = %this.getSpawnedObject("NodeGroup");
	if(!isObject(%nodeGroup))
	{
		if(!%nospam)
			schedule(0, 0, warn, "CEPathCamera::EventCheck -" SPC %this.getName() SPC "hasn't been initialized for some reason!");
		%gross = true;
	}
	return !%gross;
}

function CEPathCamera::EventExecute(%this)
{
	if(%this.type !$= "PathCamera" || %this.parent.viewers == 0)
		return false;

	%group = %this.getSpawnedObject("NodeGroup");

	%cam = %this.getSpawnedObject("PathCamera");
	if(isObject(%cam))
		%cam.delete();

	%cam = %this.PCCreateCam();

	if(!isObject(%cam))
		return false;

	%this.PCSortNodes();
	%cam.reset();
	%cam.pushBack(%cam.getTransform(), 1.0, "Normal", "Linear");
	for(%i = 1; %i < %group.nodes; %i++) //skip the first node since it's used for initial positioning
	{
		%node = %group.node[%i];
		%pos = getField(%node, 0);
		%trans = getField(%node, 1);
		%type = getField(%node, 2);
		%path = getField(%node, 3);
		%lastNode = %group.node[%i - 1];
		if(%lastNode !$= "")
		{
			%lastPos = getField(%lastNode, 0);
			%lastTrans = getField(%lastNode, 1);
			%posA = getWords(%trans, 0, 2);
			%posB = getWords(%lastTrans, 0, 2);
			%sec = (%pos - %lastPos) / 1000;
			%speed = getPathCamSpeed(%posA, %posB, %sec);
		}
		else
			%speed = 1.0;
		%cam.pushBack(%trans, %speed, %type, %path);
		echo(%trans TAB %speed TAB %type TAB %path);
	}
	%this.parent.schedule(0, setControlObject, %cam);
	//localClientConnection.setControlObject(%cam);
	//%cam.schedule(0, setTarget, 1.0);
	%cam.schedule(0, setState, "forward");
	return true;
}