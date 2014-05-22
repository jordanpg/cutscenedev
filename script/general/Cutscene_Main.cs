$Cutscene::ModuleCutscene_Main = true;

//GLOBALS
$Cutscene::Events::Types = "Camera	PathCamera	Actor	Effect	Caption	Script	End";
$Cutscene::Events::ResetOnAssignment = false;

//FUNCTIONS
function Cutscene_ClearTempVars()
{
	deleteVariables("$SceneTemp::*");
}

//OBJECT FUNCTIONS
function Cutscene_CreateGroup()
{
	if(isObject(CutsceneGroup))
	{
		warn("CutsceneGroup already exists!");
		return -1;
	}

	$CutsceneGroup = new SimGroup(CutsceneGroup);
	return $CutsceneGroup;
}

function SimGroup::getNextUntitled(%this)
{
	if(%this.untitled $= "")
	{
		%this.untitled = 2;
		return "Untitled";
	}
	%label = "Untitled_" @ %this.untitled;
	%this.untitled++;
	return %label;
}

function Cutscene_New(%sceneLabel, %viewer)
{
	if(!isObject($CutsceneGroup))
		Cutscene_CreateGroup();

	if(%sceneLabel $= "" || isObject($CutsceneGroup.scene[%sceneLabel]))
		%sceneLabel = $CutsceneGroup.getNextUntitled();

	%this = new ScriptGroup("Cutscene_" @ %sceneLabel)
			{
				class = "Cutscene";

				label = %sceneLabel;
				events = 0;
				schedules = 0;
				viewers = 0;
			};
	$CutsceneGroup.add(%this);
	$CutsceneGroup.scene[%sceneLabel] = %this;
	if(isObject(firstWord(%viewer)))
		%this.addViewer(%viewer);
	return %this;
}

//CLASS FUNCTIONS
function Cutscene::NewEvent(%this, %sid)
{
	if(%sid $= "")
		%sid = %this.events;

	%obj = new ScriptObject("CutsceneEvent_" @ %this.label @ "_" @ %sid)
			{
				class = "CutsceneEvent";
				parent = %this;
				sid = %sid;

				params = 0;
				eventReady = false;
				position = 0;
				type = "";
			};
	%this.add(%obj);
	%this.event[%sid] = %obj;
	%this.events++;
	return %obj;
}

function Cutscene::setLabel(%this, %label)
{
	if(%label $= "" || isObject($CutsceneGroup.scene[%label]))
		return false;
	$CutsceneGroup.scene[%this.label] = "";
	%this.label = %label;
	$CutsceneGroup.scene[%this.label] = %this;
	%this.setName("Cutscene_" @ %label);
	return true;
}

function Cutscene::addViewer(%this, %viewers)
{
	%ct = getWordCount(%viewers);
	for(%i = 0; %i < %ct; %i++)
	{
		%client = getWord(%viewers, %i);
		if(!isObject(%client) || %client.getClassName() !$= "GameConnection" || !isObject(%client.camera) || isObject(%client.viewing))
			continue;
		%this.viewer[%this.viewers] = %client;
		%this.viewers++;
		%client.viewing = %this;
	}
	return %this.viewers;
}

function Cutscene::clearViewers(%this)
{
	if(%this.viewers == 0)
		return;
	for(%i = %this.viewers; %i >= 0; %i--)
	{
		%this.viewer[%i].viewing = "";
		%this.viewer[%i] = "";
	}
	%this.viewers = 0;
}

function Cutscene::ReturnPlayerControl(%this)
{
	if(%this.viewers == 0)
		return false;
	%gotit = false;
	for(%i = 0; %i < %this.viewers; %i++)
	{
		%client = %this.viewer[%i];
		if(!isObject(%client) || !isObject(%client.player))
			continue;
		%client.setControlObject(%client.player);
		%gotit = true;
	}
	return %gotit;
}

function Cutscene::findEventBySID(%this, %sid)
{
	if(!isObject(%event = %this.event[%sid]))
		return -1;
	return %event;
}

function Cutscene::Enqueue(%this, %sched)
{
	%this.schedule[%this.schedules] = %sched;
	%this.schedules++;

	return %this.schedules;
}

function Cutscene::CancelQueue(%this)
{
	if(%this.schedules <= 0)
		return false;

	for(%i = %this.schedules-1; %i >= 0; %i--)
	{
		if(isEventPending(%this.schedule[%i]))
			cancel(%this.schedule[%i]);
	}
	%this.schedules = 0;
	return true;
}

function Cutscene::onCutsceneFinished(%this)
{
	if(isEventPending(%this.clearSched))
		cancel(%this.clearSched);

	%this.CancelQueue();
	%this.clearViewers();
	%this.schedule(0, ReturnPlayerControl);
}

function Cutscene::CancelSchedule(%this, %id)
{
	if(%id >= %this.schedules)
		return false;

	if(isEventPending(%this.schedule[%id]))
		cancel(%this.schedule[%id]);
	return true;
}

function Cutscene::GetQueueLength(%this)
{
	if(%this.schedules == 0)
		return 0;
	%val = 0;
	for(%i = 0; %i < %this.schedules; %i++)
	{
		%sched = %this.schedule[%i];
		if(!isEventPending(%sched))
			continue;
		%time = getTimeRemaining(%sched);
		if(%time > %val)
			%val = %time;
	}
	return %val;
}

function Cutscene::OrganiseTimeline(%this) //Probably causes lag for longer/more complex cutscenes; should be used for finalization, not initialization.
{
	if(%this.getCount() == 0)
		return false;
	%len = %this.GetQueueLength();
	%temp = new ScriptGroup(); //Objects can only belong to one group at a time
	for(%l = 0; %l <= %len; %l++) //For every possible position from zero to the longest event we can find
	{
		for(%i = %this.getCount() - 1; %i >= 0; %i--) //Loop through the events and add them to the temporary scriptgroup if they match the current position
		{
			%event = %this.getObject(%i);
			if(%event.position != %l)
				continue;
			%temp.add(%event);
		}
	}

	for(%i = %temp.getCount() - 1; %i >= 0; %i--) //Add the events back to the scriptgroup with new ordering
		%this.add(%temp.getObject(%i));
	%temp.delete();
	return true;
}

function Cutscene::ExecuteEvent(%this, %sid)
{
	if(!isObject(%event = %this.findEventBySID(%sid)))
		return;

	return %event.Execute();
}

function Cutscene::EnqueueEvents(%this)
{
	if((%count = %this.getCount()) == 0)
		return false;

	echo("_" @ %count);
	%events = 0;
	%longestLen = 0;
	for(%i = 0; %i < %count; %i++)
	{
		%event = %this.getObject(%i);

		echo("0");
		if(!%event.timedProperly())
			continue;

		echo("1");
		%len = 0;
		switch$(%event.timedType)
		{
			case "Definite":
				%len = %event.position;
				%sched = %this.schedule(%len, ExecuteEvent, %event.sid);
				%this.Enqueue(%sched);
				%events++;
			case "Relative":
				if(!isObject(%obj = %this.findEventBySID(%this.timedParameter)))
					continue;
				%len = %obj.position + %event.position;
				%sched = %this.schedule(%len, ExecuteEvent, %event.sid);
				%this.Enqueue(%sched);
				%events++;
		}
		if(%event.type $= "End") //If an end event is reached, this is the end of the cutscene.
		{
			%longestLen = %len;
			break;
		}
		else if(%len > %longestLen)
			%longestLen = %len;
	}

	%this.clearSched = %this.schedule(%longestLen, onCutsceneFinished);

	return %events;
}

function Cutscene::Execute(%this, %viewers)
{
	if(%this.getCount() == 0)
		return false;

	if(isObject(firstWord(%viewers)))
		%this.addViewer(%viewers);

	return %this.EnqueueEvents();
}

function CutsceneEvent::RegisterSpawnedObject(%this, %obj, %identifier)
{
	if(!isObject(%obj))
		return false;
	%parent = %this.parent;
	if(%parent.hasObjects[%this.sid])
		%parent.eventObject[%this.sid, %parent.eventObjects[%this.sid]] = %obj;
	else
	{
		%parent.eventObjects[%this.sid] = 0;
		%parent.eventObject[%this.sid, %parent.eventObjects[%this.sid]] = %obj;
		%parent.hasObjects[%this.sid] = 1;
	}

	if(%identifier !$= "")
	{
		%parent.eventObjIdentifier[%this.sid, %parent.eventObjects[%this.sid]] = %identifier;
		%parent.eventObjKey[%this.sid, %identifier] = %obj;
	}
	%parent.eventObjects[%this.sid]++;
	return true;
}

function CutsceneEvent::GetSpawnedObject(%this, %id)
{
	%parent = %this.parent;
	if(%id > %parent.eventObjects[%this.sid] - 1)
	{
		if(!isObject(%obj = %parent.eventObjKey[%this.sid, %id]))
			return -1;
		else
			return %obj;
		return -1;
	}
	else if(isObject(%obj = %parent.eventObject[%this.sid, %id]))
		return %obj;
	return -1;
}

function CutsceneEvent::AssignType(%this, %type)
{
	if(searchFields($Cutscene::Events::Types, %type) == -1)
	{
		warn("CutsceneEvent::AssignType - Type" SPC %type SPC "cannot be assigned to" SPC %this.getName() @ "!");
		return false;
	}

	if(%this.type !$= "" && $Cutscene::Events::ResetOnAssignment)
		%this.ResetParameters();

	%this.type = %type;
	return true;
}

function CutsceneEvent::hasParameter(%this, %name)
{
	for(%i = 0; %i < %this.params; %i++)
	{
		%p = %this.params[%i];
		if(%p $= %name)
			return true;
	}
	return false;
}

function CutsceneEvent::SetParameter(%this, %name, %value)
{
	if(%this.type $= "")
		warn("CutsceneEvent::SetParameter - Event" SPC %this.getName() SPC "has no assigned type!");

	if(%this.hasParameter(%name))
	{
		%this.paramV[%name] = %value;
		return true;
	}

	%this.paramS[%this.params] = %name;
	%this.paramV[%name] = %value;
	%this.params++;
	return true;
}

function CutsceneEvent::ResetParameters(%this)
{
	if(%this.params == 0)
		return 0;
		
	for(%i = %this.params-1; %i >= 0; %i--)
	{
		%name = %this.paramS[%i];
		%this.paramV[%name] = "";
		%this.paramS[%i] = "";
	}
	%this.params = 0;
	return %i;
}

function CutsceneEvent::GetParameter(%this, %name, %isnumber)
{
	if(%this.paramV[%name] $= "" || %isnumber)
	{
		if(%this.paramS[%name] !$= "")
			%name = %this.paramS[%name];
		else
			return "";
	}

	return %this.paramV[%name];
}

function CutsceneEvent::DebugActionCheck(%this, %b)
{
	%ready = -1;
	if(isFunction(CutsceneEvent, "ActionCheck" @ %this.type))
		%ready = eval("%this.ActionCheck" @ %this.type @ "(!%b);");
	else
		echo("No ActionCheck method exists for type" SPC %this.type @ ".");
	return %ready;
}

function CutsceneEvent::Execute(%this)
{
	if(!%this.eventReady)
	{
		warn("CutsceneEvent::Execute -" SPC %this.getName() SPC "is not event ready!");
		return;
	}

	if(isFunction(CutsceneEvent, "ActionCheck" @ %this.type))
	{
		%ready = eval("%this.ActionCheck" @ %this.type @ "(!%this.debug);");
		if(%ready !$= "" && !%ready)
			warn("CutsceneEvent::Execute -" SPC "ActionCheck method for type" SPC %this.type SPC "returned false for event" SPC %this.getName() @ "! Attempting to run anyway...");
	}

	if(isFunction(CutsceneEvent, "ActionType" @ %this.type))
		%val = eval("%this.ActionType" @ %this.type @ "();");
	return %val;
}

function CutsceneEvent::SetEventReady(%this, %val)
{
	if(%this.type $= "" || searchFields($Cutscene::Events::Types, %this.type) == -1)
	{
		warn("CutsceneEvent::SetEventReady -" SPC %this.getName() SPC "cannot be made event ready: no valid type has been assigned!");
		return false;
	}

	if(%val $= "")
		%val = true;

	if(%this.params == 0 && %val)
		warn("CutsceneEvent::SetEventReady -" SPC %this.getName() SPC "has no parameters, setting ready anyway...");

	%this.eventReady = %val;
	return true;
}

function CutsceneEvent::SetTimed(%this, %type, %time, %val)
{
	if(!isObject(%this.parent))
		warn("CutsceneEvent::SetTimed -" SPC %this.getName() SPC "has no parent object and cannot be a timed event, setting parameters anyway...");

	if(%type !$= "Relative" && %type !$= "Definite")
	{
		warn("CutsceneEvent::SetTimed -" SPC %type SPC "is not a valid timing type! Use either \'Relative\' or \'Definite\'. (" @ %this.getName() @ ")");
		return false;
	}

	if(%time < 0)
	{
		warn("CutsceneEvent::SetTimed - Got negative timeline position, assuming zero... (" @ %this.getName() @ ")");
		%time = 0;
	}

	%this.timed = true;
	%this.position = %time;
	%this.timedType = %type;
	%this.timedParameter = %val;

	return true;
}

function CutsceneEvent::UnTime(%this)
{
	if(!%this.timed)
		return false;

	%this.position = "";
	%this.timedType = "";
	%this.timedParameter = "";
	%this.timed = false;

	return true;
}

function CutsceneEvent::timedProperly(%this)
{
	if(!%this.timed)
		return false;
	if(%this.position < 0 || %this.position $= "")
		return false;
	if(%this.timedType $= "" || (%this.timedType !$= "Relative" && %this.timedType !$= "Definite"))
		return false;
	if(%this.timedParameter $= "" && %this.timedType $= "Relative")
		return false;
	return true;
}

function CutsceneEvent::EventSpawnsStuff(%this)
{
	%val = false;
	if(%this.type $= "Actor" && %this.getParameter("Mode") $= "Spawn")
		%val = true;
	return %val;
}

function CutsceneEvent::setDebug(%this, %bool)
{
	%this.debug = (%bool == true); //technically unnecessary but it makes things nice and pretty
	return %this.debug;
}