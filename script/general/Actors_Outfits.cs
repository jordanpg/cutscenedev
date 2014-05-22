//CLASS FUNCTIONS
function Player::ExtractOutfit(%this, %fileName, %name, %overwrite, %exec)
{
	if(isFile(%fileName) && !%overwrite)
		return -1;
	%file = new fileObject();
	%file.openForWrite(%fileName);
	%file.writeLine("DATABLOCK" SPC %this.getDatablock().uiName);
	%file.writeLine("NAME" SPC (%name $= "" ? fileBase(%fileName) : %name));
	if(%exec !$= "" && isFile(%exec))
		%file.writeLine("LOAD" SPC %exec);
	if(isFunction("OutfitRule" @ fileBase(%this.getDatablock().shapeFile) @ "Dts"))
		call("OutfitRule" @ fileBase(%this.getDatablock().shapeFile) @ "Dts", %this, %file);

	%file.close();
	%file.delete();
	return %fileName;
}

function GameConnection::ExtractOutfit(%this, %fileName, %name, %overwrite, %exec)
{
	if(!isObject(%this.player))
		return -1;
	return %this.player.ExtractOutfit(%fileName, %name, %overwrite, %exec);
}

function Player::ApplyOutfit(%this, %fileName)
{
	if(!isFile(%fileName))
		return false;

	%file = new fileObject();
	%file.openForRead(%fileName);
	while(!%file.isEOF())
	{
		%line = %file.readLine();
		%cmd = firstWord(%line);
		%params = restWords(%line);
		if(isFunction("OutfitCommand" @ %cmd))
			call("OutfitCommand" @ %cmd, %this, %params);
		echo(%line);
	}
	%file.close();
	%file.delete();
	return true;
}

//OUTFIT COMMANDS
function OutfitCommandDATABLOCK(%this, %params)
{
	%db = getField(%params, 0);
	if(!isObject(%db = $uiNameTable_Player[%db]))
		%db = $uiNameTable_Player["Standard Player"];
	%this.setDatablock(%db);
}

function OutfitCommandNAME(%this, %params)
{
	%name = getField(%params, 0);
	%this.outfitName = %name;
}

function OutfitCommandFACE(%this, %params)
{
	%face = getField(%params, 0);
	%this.setFaceName(%face);
}

function OutfitCommandDECAL(%this, %params)
{
	%decal = getField(%params, 0);
	%this.setDecalName(%decal);
}

function OutfitCommandLOAD(%this, %params)
{
	if(isFile(%params))
		exec(%params);
}

function OutfitCommandNODE(%this, %params)
{
	%name = getField(%params, 0);
	if(%name $= "NONE" || %name $= "")
		return;
	%colour = getField(%params, 1);
	%type = getField(%params, 2);
	if(getSubStr(%name, 0, 1) $= "!")
	{
		%this.hideNode(getSubStr(%name, 1, strLen(%name)-1));
		return;
	}
	if(%type !$= "NONE" && %type !$= "")
	{
		if(isFunction("OC_NODE_RULE" @ fileBase(%this.getDatablock().shapeFile) @ "Dts"))
			call("OC_NODE_RULE" @ fileBase(%this.getDatablock().shapeFile) @ "Dts", %this, %name, %colour, %type);
	}
	%this.unHideNode(%name);
	%this.setNodeColor(%name, %colour);
}

//OUTFIT RULES
function OutfitRulemDts(%this, %file) //Rules for the default player model.
{
	if(fileName(%this.getDatablock().shapeFile) !$= "m.dts" || !isObject(%file))
		return false;
	if(!isObject(%this.client))
		return false;

	if(%this.client.faceName !$= "")
		%file.writeLine("FACE" SPC %this.client.faceName);
	if(%this.client.decalName !$= "")
		%file.writeLine("DECAL" SPC %this.client.decalName);
	%file.writeLine("NODE HEADSKIN" TAB %this.client.headColor);
	%file.writeLine("NODE" SPC strUpr($chest[%this.client.chest]) TAB %this.client.chestColor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($accent[%this.client.accent]) TAB %this.client.accentColor TAB "ACCENT");
	%file.writeLine("NODE" SPC strUpr($hat[%this.client.hat]) TAB %this.client.hatcolor TAB "HAT");
	%file.writeLine("NODE" SPC strUpr($hip[%this.client.hip]) TAB %this.client.hipcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($LLEG[%this.client.lleg]) TAB %this.client.llegcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($RLEG[%this.client.rleg]) TAB %this.client.rlegcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($RHAND[%this.client.rhand]) TAB %this.client.rhandcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($LHAND[%this.client.lhand]) TAB %this.client.lhandcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($PACK[%this.client.pack]) TAB %this.client.packcolor TAB "PACK");
	%file.writeLine("NODE" SPC strUpr($RARM[%this.client.rarm]) TAB %this.client.rarmcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($LARM[%this.client.larm]) TAB %this.client.larmcolor TAB "NONE");
	%file.writeLine("NODE" SPC strUpr($SECONDPACK[%this.client.secondpack]) TAB %this.client.secondpackcolor TAB "SECONDPACK");
	return true;
}

function OutfitRulehorseDts(%this, %file)
{
	if(fileName(%this.getDatablock().shapeFile) !$= "horse.dts" || !isObject(%file))
		return false;
	if(!isObject(%this.client))
		return false;

	%file.writeLine("NODE BODY" TAB %this.client.chestColor TAB "NONE");
	return true;
}

function OC_NODE_RULEmDts(%this, %name, %colour, %type)
{
	if(!isObject(%this.client))
		return;
	switch$(%type)
	{
		case "ACCENT":			if(%name !$= $accent[%this.client.accent]){%this.hideNode($accent[%this.client.accent]);}
		case "HAT":				if(%name !$= $hat[%this.client.hat]){%this.hideNode($hat[%this.client.hat]);}
		case "PACK":			if(%name !$= $pack[%this.client.pack]){%this.hideNode($pack[%this.client.pack]);}
		case "SECONDPACK":		if(%name !$= $secondpack[%this.client.secondpack]){%this.hideNode($secondpack[%this.client.secondpack]);}
	}
}