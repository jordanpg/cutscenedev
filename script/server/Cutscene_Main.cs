$CutsceneModuleServer::Cutscene_Main = true;

//GLOBALS
if($CutsceneEvent::Types $= "")
	$CutsceneEvent::Types = 0;
//$Cutscene::Events::Types = "Camera	PathCamera	Actor	Effect	Caption	Script	End	Control";

//FUNCTIONS
function Cutscene_ClearTempVars()
{
	deleteVariables("$SceneTemp::*");
}

function Cutscene_RegisterEventType(%typename, %script, %uicolour)
{
	if(%typename $= "" || %script $= "" || %uicolour $= "")
	{
		warn("Cutscene_RegisterEventType - wrong number of arguments.");
		warn("usage: (%typename, %script, %uicolour)");
		warn("Registers an event type to the Cutscene Control system.");
		return false;
	}

	if(searchFields($Cutscene::Events::Types, %typename) != -1)
	{
		warn("Cutscene_RegisterEventType - Type name" SPC %typename SPC "is already registered.");
		return false;
	}

	if(!isFile(%script) || fileExt(%script) !$= ".cs")
	{
		warn("Cutscene_RegisterEventType - \'" @ %script @ "\' does not point to a valid .cs file.");
		return false;
	}

	%uicolour = stupidcolours(%uicolour);
	if(getWordCount(%uicolour) < 4)
	{
		warn("Cutscene_RegisterEventType - \'" @ %uicolour @ "\' is not a valid RGBA value.");
		return false;
	}
	%uicolour = getWords(%uicolour, 0, 3);
	%s = exec(%script);
	if(!%s)
	{
		warn("Cutscene_RegisterEventType - Execution of script failed; cannot register type.");
		return false;
	}

	$Cutscene::Events::Types = trim($Cutscene::Events::Types TAB %typename);
	$CutsceneEvent::Type[$CutsceneEvent::Types] = %typename;
	$CutsceneEvent::UIColour[$CutsceneEvent::Types] = %uicolour;
	$CutsceneEvent::Script[$CutsceneEvent::Types] = %script;
	$CutsceneEvent::Types++;
	warn("Cutscene Event Type" SPC %typename SPC "has been successfully registered.");
	return true;
}
Cutscene_RegisterEventType("Camera", $Cutscene::Root @ "script/server/event/Event_Camera.cs", "1 1 0 1");
Cutscene_RegisterEventType("PathCamera", $Cutscene::Root @ "script/server/event/Event_PathCamera.cs", "1 0.7 0 1");
Cutscene_RegisterEventType("End", $Cutscene::Root @ "script/server/event/Event_End.cs", "0.3 0.3 0.3 1");
Cutscene_RegisterEventType("Script", $Cutscene::Root @ "script/server/event/Event_Script.cs", "0.7 0 0 1");
Cutscene_RegisterEventType("Actor", $Cutscene::Root @ "script/server/event/Event_Actor.cs", "0 0.7 0 1");
//Cutscene_RegisterEventType("Effect", $Cutscene::Root @ "script/server/event/Event_Effect.cs", "0.7 0 0.7 1");
//Cutscene_RegisterEventType("Caption", $Cutscene::Root @ "script/server/event/Event_Caption.cs", "0 0.7 0.7 1");
//Cutscene_RegisterEventType("Control", $Cutscene::Root @ "script/server/event/Event_Control.cs", "1 1 0 1");

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
if(!isObject(CutsceneGroup))
	Cutscene_CreateGroup();

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
function Cutscene::NewEvent(%this, %sid, %type)
{
	if(searchFields($Cutscene::Events::Types, %type) == -1)
	{
		warn("Cutscene::NewEvent - Type" SPC %type SPC "cannot be assigned to an event!");
		return false;
	}

	if(%sid $= "")
		%sid = %this.events;

	%obj = new ScriptObject("CutsceneEvent_" @ %this.label @ "_" @ %sid)
			{
				superClass = "CutsceneEvent";
				class = "CE" @ %type;
				parent = %this;
				sid = %sid;

				params = 0;
				eventReady = false;
				position = 0;
				type = %type;
			};
	%this.add(%obj);
	%this.event[%sid] = %obj;
	%this.events++;
	if(isFunction("CE" @ %type, "EventOnAssign"))
		%obj.EventOnAssign();
	return %obj;
}

function Cutscene::setLabel(%this, %label)
{
	if(%label $= "" || isObject($CutsceneGroup.scene[%label]))
		return false;
	$CutsceneGroup.scene[%this.label] = "";
	%this.label = %label;
	$CutsceneGroup.scene[%this.label] = %this;
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

function Cutscene::setControlObject(%this, %obj)
{
	if(%this.viewers == 0 || !isObject(%obj))
		return false;

	%gotit = false;
	%obj.setScopeAlways();
	for(%i = 0; %i < %this.viewers; %i++)
	{
		%client = %this.viewer[%i];
		if(!isObject(%client))
			continue;
		//%obj.scopeToClient(%client);
		%client.schedule(0, setControlObject, %obj);
		%gotit = true;
	}
	return %gotit;
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
		//%client.getControlObject().clearScopeToClient(%client);
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
		%parent.hasObjects[%this.sid] = true;
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
	if(%id > %parent.eventObjects[%this.sid] - 1 || (%id + 0) !$= %id)
	{
		if(isObject(%obj = %parent.eventObjKey[%this.sid, %id]))
			return %obj;
	}
	else if(isObject(%obj = %parent.eventObject[%this.sid, %id]))
		return %obj;
	return -1;
}

function CutsceneEvent::ClearSpawnedObjects(%this)
{
	%parent = %this.parent;
	if((%amt = %parent.eventObjects[%this.sid]) <= 0 || !%parent.hasObjects[%this.sid])
		return;

	for(%i = %amt - 1; %i >= 0; %i--)
	{
		%obj = %parent.eventObject[%this.sid, %i];
		if(isObject(%obj))
			%obj.delete();
		%id = %parent.eventObjectIdentifier[%this.sid, %i];
		%parent.eventObjKey[%this.sid, %id] = "";
		%parent.eventObjectIdentifier[%this.sid, %i] = "";
		%parent.eventObject[%this.sid, %i] = "";
	}
	%parent.eventObjects[%this.sid] = 0;
	%parent.hasObjects[%this.sid] = false;
}

function CutsceneEvent::hasParameter(%this, %name)
{
	for(%i = 0; %i < %this.params; %i++)
	{
		%p = %this.paramS[%i];
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
		if(isFunction("CE" @ %this.type, "EventOnParameter"))
			%this.EventOnParameter(%name, %value, true);
		return true;
	}

	%this.paramS[%this.params] = %name;
	%this.paramV[%name] = %value;
	%this.params++;
	if(isFunction("CE" @ %this.type, "EventOnParameter"))
		%this.EventOnParameter(%name, %value);
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

function CutsceneEvent::DebugEventCheck(%this)
{
	%ready = -1;
	if(isFunction("CE" @ %this.type, "EventCheck"))
		%ready = %this.EventCheck(false);
	else
		echo("No EventCheck method exists for type" SPC %this.type @ ".");
	return %ready;
}

function CutsceneEvent::Execute(%this)
{
	if(!%this.eventReady)
	{
		warn("CutsceneEvent::Execute -" SPC %this.getName() SPC "is not event ready!");
		return;
	}

	if(isFunction("CE" @ %this.type, "EventCheck"))
	{
		%ready = %this.EventCheck(!%this.debug);
		if(%ready !$= "" && !%ready)
		{
			warn("CutsceneEvent::Execute -" SPC "EventCheck method for type" SPC %this.type SPC "returned false for event" SPC %this.getName() @ "!");
			return;
		}
	}

	if(isFunction("CE" @ %this.type, "EventExecute"))
		%val = %this.EventExecute();
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
	if(%this.type $= "PathCamera" && %this.parent.viewers > 0)
		%val = true;
	return %val;
}

function CutsceneEvent::setDebug(%this, %bool)
{
	%this.debug = (%bool == true); //technically unnecessary but it makes things nice and pretty
	return %this.debug;
}

function CutsceneEvent::processSpecialSave(%this, %paramName, %solver)
{
	if(!isObject(%solver) || %paramName $= "")
		return false;
	if(!isFunction(%this.class, "SpecialSave" @ %paramName))
		return false;
	if(%solver.class !$= "CSSolver")
		return false;
	if(%solver.activeScene != %this.parent)
		return false;
	if(%solver.scopeName[%solver.scopeLevel] !$= "PARAM")
		return false;

	%paramVal = %this.getParameter(%paramName);
	if(isFunction(%event.class, "SpecialSave" @ %paramName))
		%r = %event.call("SpecialSave" @ %paramName, %solver, %paramVal);
	else
		return false;
	return %r;
}

// function CutsceneEvent::saveParameter(%this, %param, %file)
// {
// 	if(!%this.hasParameter(%param) || !isObject(%file))
// 		return false;
// 	if(%file.getClassName() !$= "FileObject")
// 		return false;

// 	%r = true;
// 	if(isFunction(%this.class, "SpecialSave" @ %param))
// 	{
		
// 		%r = %this.call("SpecialSave" @ %param, %file);
// 	}
// 	else
// 		%file.writeLine("->MINI\tPARAM" TAB %param TAB %this.getParameter(%param, 1));
// 	return %r;
// }