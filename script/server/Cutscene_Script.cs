$CutsceneModuleServer::Cutscene_Script = true;

//GLOBALS
$Cutscene::ScriptVersion = "003";
if($CutsceneScript::ScopeCt $= "")
	$CutsceneScript::ScopeCt = 0;

//FUNCTIONS
function CutsceneScript_registerScope(%name, %saveFunc, %loadFunc, %amt)
{
	%name = strUpr(firstWord(getField(%name, 0)));
	%saveFunc = "Solver" @ %saveFunc;
	%loadFunc = "Solver" @ %loadFunc;
	if(!isFunction("CSSolver", %saveFunc))
		%saveFunc = -1;
	if(!isFunction("CSSolver", %loadFunc))
		%loadFunc = -1;
	if(%amt < 1)
		%amt = -1;

	%val = $CutsceneScript::ScopeCt;
	if(searchFields($CutsceneScript::Scopes, %name) != -1)
	{
		warn("CutsceneScript_registerScope - Scope" SPC %name SPC "has already been registered, overwriting");
		%noReg = true;
		%val = $CutsceneScript::ScopeLookup[%name];
	}

	$CutsceneScript::ScopeName[%val] = %name;
	$CutsceneScript::ScopeSave[%val] = %saveFunc;
	$CutsceneScript::ScopeLoad[%val] = %loadFunc;
	$CutsceneScript::ScopeAllowed[%val] = %amt;
	$CutsceneScript::ScopeLookup[%name] = %val;
	if(!%noReg)
		$CutsceneScript::Scopes = trim($CutsceneScript::Scopes TAB %name);
	return true;
}

//SOLVER METHODS
function New_CSSolver(%name, %mode, %path)
{
	%this = new ScriptObject(%name)
			{
				class = "CSSolver";

				activeScopes = "GLOBAL";
				scopeLevel = 0;
				scopeName0 = "GLOBAL";
				scopeID0 = "root";
				scope["GLOBAL root"] = 0;
				fileMode = -1;
			};
	if(%mode $= "0")
		%this.openForRead(%path);
	else if(%mode $= "1")
		%this.openForWrite(%path);
	else if(%mode $= "2")
		%this.openForAppend(%path);

	return %this;
}

function CSSolver::openForWrite(%this, %path)
{
	if(isObject(%this.file))
	{
		warn("CSSolver::openForWrite - Got an open request while a file was still open, closing fileObject...");
		%this.file.close();
	}
	else
		%this.file = new fileObject();
	%this.file.openForWrite(%path);
	%this.fileMode = 1;
	return %this.file;
}

function CSSolver::openForRead(%this, %path)
{
	if(!isFile(%path))
		return -1;

	if(isObject(%this.file))
	{
		warn("CSSolver::openForWrite - Got an open request while a file was still open, closing fileObject...");
		%this.file.close();
	}
	else
		%this.file = new fileObject();
	%this.file.openForRead(%path);
	%this.fileMode = 0;
	return %this.file;
}

function CSSolver::openForAppend(%this, %path)
{
	if(!isFile(%path))
		return -1;

	if(isObject(%this.file))
	{
		warn("CSSolver::openForWrite - Got an open request while a file was still open, closing fileObject...");
		%this.file.close();
	}
	else
		%this.file = new fileObject();
	%this.file.openForAppend(%path);
	%this.fileMode = 2;
	return %this.file;
}

function CSSolver::close(%this)
{
	if(!isObject(%this.file))
		return false;
	%this.file.close();
	%this.file.delete();
	%this.fileMode = -1;
	return true;
}

function CSSolver::writeLine(%this, %line)
{
	if(!isObject(%this.file) || %this.fileMode < 1)
		return false;
	for(%i = 0; %i < %this.scopeLevel; %i++)
		%tabs = %tabs TAB "";
	%this.file.writeLine(%tabs @ %line);
	return true;
}

function CSSolver::readLine(%this)
{
	if(!isObject(%this.file) || %this.fileMode > 0)
		return "";
	return ltrim(%this.file.readLine());
}

function CSSolver::openSaveScope(%this, %scopeName, %id, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16)
{
	if(%scopeName $= "" || %id $= "")
	{
		warn("CSSolver::openSaveScope - wrong number of arguments");
		warn("CSSolver::openSaveScope(%this, %scopeName, %id, %v1, ...);");
		return false;
	}
	if(%this.fileMode < 1)
	{
		warn("CSSolver::openSaveScope - Attempted to open a save scope in read mode");
		return false;
	}
	%scopeName = strUpr(firstWord(getField(%scopeName, 0)));
	if(searchFields($CutsceneScript::Scopes, %scopeName) == -1)
	{
		warn("CSSolver::openSaveScope - Attempted to open non-existant save scope" SPC %scopeName);
		return false;
	}

	%func = $CutsceneScript::ScopeSave[$CutsceneScript::ScopeLookup[%scopeName]];
	if(isFunction("CSSolver", %func))
		%r = %this.call(%func, 1, %id, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16);
	if(!%r)
		return false;
	%this.activeScopes = trim(%this.activeScopes TAB %scopeName SPC %id);
	%this.scopeLevel++;
	%this.scopeName[%this.scopeLevel] = %scopeName;
	%this.scopeID[%this.scopeLevel] = %id;
	%this.scope[%scopeName SPC %id] = %this.scopeLevel;
	%this.writeLine("!BEGIN" TAB %scopeName TAB %id TAB combineToFields(%v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16));
	return true;
}

function CSSolver::closeSaveScope(%this, %scopeName, %id, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16)
{
	if(%scopeName $= "" || %id $= "")
	{
		warn("CSSolver::closeSaveScope - wrong number of arguments");
		warn("CSSolver::closeSaveScope(%this, %scopeName, %id, %v1, ...);");
		return false;
	}
	%scopeName = strUpr(firstWord(getField(%scopeName, 0)));
	if((%index = searchFields(%this.activeScopes, %scopeName SPC %id)) == -1 || %this.fileMode < 1)
		return false;

	%func = $CutsceneScript::ScopeSave[$CutsceneScript::ScopeLookup[%scopeName]];
	if(isFunction("CSSolver", %func))
		%r = %this.call(%func, 0, %id, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16);
	if(!%r)
		return false;

	%file.writeLine("!END" TAB %scopeName TAB %id TAB combineToFields(%v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16));
	%this.activeScopes = removeField(%this.activeScopes, %index);
	for(%i = %this.scopeLevel; %i >= %this.scope[%scopeName SPC %id]; %i--)
		%this.closeSaveScope(%this.scopeName[%i], %this.scopeID[%i], %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16);
	return true;
}

// Originally, there was going to be a shorthanded syntactical sugar type thing for more simple scopes, but decided against it due to possible issues with usage.
// function CSSolver::miniSaveScope(%this, %scopeName, %id, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16)
// {
// 	if(%scopeName $= "" || %id $= "")
// 	{
// 		warn("CSSolver::miniSaveScope - wrong number of arguments");
// 		warn("CSSolver::miniSaveScope(%this, %scopeName, %id, %v1, ...);");
// 		return false;
// 	}
// 	if(%this.fileMode < 1)
// 	{
// 		warn("CSSolver::miniSaveScope - Attempted to open a save scope in read mode");
// 		return false;
// 	}
// 	%scopeName = strUpr(firstWord(getField(%scopeName, 0)));
// 	if(searchFields($CutsceneScript::Scopes, %scopeName) == -1)
// 	{
// 		warn("CSSolver::miniSaveScope - Attempted to open non-existant save scope" SPC %scopeName);
// 		return false;
// 	}

// 	%func = $CutsceneScript::ScopeSave[$CutsceneScript::ScopeLookup[%scopeName]];
// 	if(isFunction("CSSolver", %func))
// 		%r = %this.call(%func, 1, %id, %v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16);
// 	if(!%r)
// 		return false;

// 	%this.activeScopes = trim(%this.activeScopes TAB %scopeName SPC %id);
// 	%this.scopeLevel++;
// 	%this.scopeName[%this.scopeLevel] = %scopeName;
// 	%this.scopeID[%this.scopeLevel] = %id;
// 	%this.scope[%scopeName SPC %id] = %this.scopeLevel;
// 	%this.writeLine("!MINI" TAB %scopeName TAB %id TAB combineToFields(%v1, %v2, %v3, %v4, %v5, %v6, %v7, %v8, %v9, %v10, %v11, %v12, %v13, %v14, %v15, %v16));
// }

//SOLVER SCOPES
function CSSolver::ScopeSCENE(%this, %open, %id, %label)
{
	if(%open)
	{
		%obj = %id;
		if(isObject(%this.activeScene))
		{
			error("CSSolver::ScopeSCENE - A new SCENE scope cannot be opened inside of another!");
			return false;
		}
		if(!isObject(%obj) || %obj.class !$= "Cutscene")
		{
			error("CSSolver::ScopeSCENE - The SCENE scope requires a Cutscene object as its ID!");
			return false;
		}
		if(%label $= "")
		{
			error("CSSolver::ScopeSCENE - The SCENE scope requires a string as its first parameter (%label)!");
			return false;
		}
		%this.activeScene = %obj;
		%this.activeSceneScope = %this.scope["SCENE" SPC %id];
		return true;
	}
	else
	{
		if(!isObject(%this.activeScene))
		{
			error("CSSolver::ScopeSCENE - No SCENE scope exists to be closed!");
			return false;
		}
		if(%this.activeScene !$= %id)
		{
			error("CSSolver::ScopeSCENE - The given ID does not coorespond to the active SCENE scope!");
			return false;
		}
		%this.activeScene = "";
		%this.activeSceneScope = "";
		return true;
	}
}

function CSSolver::ScopeEVENTS(%this, %open, %id, %exclude)
{
	if(%open)
	{
		%obj = %this.activeScene;
		if(!isObject(%obj))
		{
			error("CSSolver::ScopeEVENTS - No SCENE scope is currently active!");
			return false;
		}
		if((%events = %obj.getCount()) == 0)
			return true;
		for(%i = 0; %i < %events; %i++)
		{
			%event = %obj.getObject(%i);
			if(searchFields(%exclude, %event.sid) != -1)
				continue;
			%this.openSaveScope("EVENT", %this.getID() SPC %event.sid, %event.type, %event.position, %event.sid);
		}
		return true;
	}
	else
	{
		return true;
	}
}

function CSSolver::ScopeEVENT(%this, %open, %id, %type, %position, %sid)
{
	if(%open)
	{
		%obj = %this.activeScene;
		%parentscope = %this.scopeName[%this.scopeLevel - 1];
		if(!isObject(%obj))
		{
			error("CSSolver::ScopeEVENT - No SCENE scope is currently active!");
			return false;
		}
		if(%parentscope !$= "EVENT")
		{
			error("CSSolver::ScopeEVENT - Parent scope is not EVENTS!");
			return false;
		}
		%event = %obj.findEventBySID(%sid);
		if(!isObject(%event))
		{
			error("CSSolver::ScopeEVENT - Could not find the given event!");
			return false;
		}

		if(%event.params > 0)
		{
			for(%i = 0; %i < %event.params; %i++)
			{
				//%event.saveParameter(%event.paramS[%i], %this.file);
				%paramName = %event.paramS[%i];
				%paramVal = %event.getParameter(%paramName);
				%this.openSaveScope("PARAM", %this.getID() SPC %event.sid SPC %paramName, %event, %paramName, %paramVal);
				// %r = true;
				// if(isFunction(%event.class, "SpecialSave" @ %paramName))
				// 	%r = %event.call("SpecialSave" @ %paramName, %this, %paramVal);
				// else
				%event.processSpecialSave(%paramName, %this);
				%this.closeSaveScope("PARAM", %this.getID() SPC %event.sid SPC %paramName);
			}
		}
	}
	else
	{
		return true;
	}
}

//wo working stuff
function Cutscene::ExportToScript(%this, %path, %solver)
{
	if(!isObject(%solver))
		%solver = New_CSSolver(1);
	else if(%solver.fileMode != 1)
		%file = %solver.openForWrite(%path);
	else
		%file = %solver.file;
	%file.writeLine("//CUTSCENESCRIPT" TAB $Cutscene::ScriptVersion);
	%file.writeLine("//CUTSCENECONTROL" TAB $Cutscene::Version);
	%file.writeLine("//NOTE: Newer versions of CutsceneScript may be incompatible with older ones. If this is the case and no converter exists, please yell at ottosparks in the appropriate forum topic.");
	// %file.writeLine("!BEGIN\tSCENE" TAB %this.label);
	// %file.writeLine("!BEGIN\tEVENTS" TAB %this.getCount());
	%this.openSaveScope("SCENE", %this.getID(), %this.label);
	%this.openSaveScope("EVENTS", %this.getID());
	%events = %this.getCount();
	for(%i = 0; %i < %events; %i++)
	{
		%event = %this.getObject(%i);
		%file.writeLine("#EVENT" TAB %event.type TAB %event.sid TAB %event.position);
		if(%event.params > 0)
		{
			for(%p = 0; %p < %event.params; %p++)
				%event.saveParameter(%event.paramS[%p], %file);
		}
		%file.writeLine("");
	}
	%file.writeLine("!END\tEVENTS");
	%file.writeLine("!END\tSCENE");
	%file.close();
	%file.delete();
}

function loadCutsceneScript(%path)
{
	if(!isFile(%path) || !$CutsceneModuleServer::Cutscene_Main || !isObject(CutsceneGroup))
	{
		echo("Could not load cutscene at \'" @ %path @ "\'!");
		return;
	}

	%file = new fileObject();
	%file.openForRead(%path);
	%cscriptv = %file.readLine();
	%cctrlv = %file.readLine();
	%file.readLine();

	$CS::EVENTS = 0;
	while(!%file.isEOF())
	{
		%line = %file.readLine();

		%command = getField(%line, 0);
		switch$(%command)
		{
			case "@VAR":
				%name = getField(%line, 1);
				%val = getFields(%line, 2, (getFieldCount(%line) - 1));
				$CS::TEMP[%name] = %val;
			case "#EVENT":
				%type = getField(%line, 1);
				%sid = getField(%line, 2);
				%pos = getField(%line, 3);
				$CS::SID[$CS::EVENTS] = %sid;
				$CS::TYPE[$CS::EVENTS] = %type;
				$CS::POS[$CS::EVENTS] = %pos;
				$CS::PARAMS[$CS::EVENTS] = 0;
				$CS::CURR = $CS::EVENTS;
				$CS::EVENTS++;
			case "->PARAM":
				if($CS::CURR $= "")
					continue;
				%name = getField(%line, 1);
				%val = getFields(%line, 2, (getFieldCount(%line) - 1));
				$CS::PARAM[$CS::CURR, $CS::PARAMS[$CS::CURR]] = %name;
				$CS::PARAMVAL[$CS::CURR, $CS::PARAMS[$CS::CURR]] = %val;
				$CS::PARAMS[$CS::CURR]++;
			case "//END":
				break;
			default:
				continue;
		}
	}
	%file.close();
	%file.delete();

	%label = $CS::TEMPLABEL;
	%this = Cutscene_New(%label);
	if(!isObject(%this))
		return -1;

	for(%i = 0; %i < $CS::EVENTS; %i++)
	{
		%sid = $CS::SID[%i];
		%type = $CS::TYPE[%i];
		%event = %this.newEvent(%sid, %type);
		%pos = $CS::POS[%i];
		%event.setTimed("Definite", %pos);
		if($CS::PARAMS[%i] > 0)
		{
			for(%p = 0; %p < $CS::PARAMS[%i]; %p++)
			{
				%name = $CS::PARAM[%i, %p];
				%val = $CS::PARAMVAL[%i, %p];
				%event.setParameter(%name, %val);
			}
		}
		if(%event.EventCheck(1))
			%event.setEventReady(true);
	}
	echo("Loaded cutscene" SPC %label SPC "with" SPC $CS::EVENTS SPC "events as \'" @ %this.label @ "\'.");
	deleteVariables("$CS::*");
	return %this;
}

function 