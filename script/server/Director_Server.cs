$Cutscene::ModuleDirector_Server = true;

package DirectorMode
{
	function GameConnection::checkSceneLabel(%this)
	{
		if(!isObject($CutsceneGroup))
			return;

		if(%this.director_label $= "")
		{
			%this.director_scene = Cutscene_New("", %this);
			%this.director_label = %this.director_scene.label;
			return;
		}

		if(isObject(%obj = $CutsceneGroup.scene[%this.director_label]))
			%this.director_scene = %obj;
		else
			%this.director_scene = Cutscene_New(%this.director_label, %this);
	}

	function serverCmdDirectorMode(%this, %label)
	{
		if(%this.directorMode)
			return;
		%this.directorMode = true;
		commandToClient(%this, 'Director_Activate');
		if(%label !$= "")
		{
			commandToClient(%this, 'Director_SetLabel', %label);
			%this.director_label = %label;
		}
		%this.checkSceneLabel();
		messageClient(%this, '', "\c6Welcome to \c3Director Mode\c6! You are now editing \c3" @ %this.director_label @ "\c6.");
	}

	function serverCmdDirectorExit(%this)
	{
		if(!%this.directorMode)
			return;
		%this.director_label = "";
		%this.director_scene = "";
		%this.directorMode = false;
		commandToClient(%this, 'Director_Exit');
		messageClient(%this, '', "\c6You have left Director Mode. The scene you were editing still exists.");
	}

	function serverCmdDirectorSetLabel(%this, %label)
	{
		if(!%this.directorMode)
			return;

		if(isObject(%this.director_scene))
		{
			if(!isObject($CutsceneGroup))
			{
				messageClient(%this, '', "For some reason the cutscene group doesn't exist. Yell loudly at the host for breaking something.");
				return;
			}
			%old = %this.director_scene.label;
			if(isObject($CutsceneGroup.scene[%label]))
			{
				messageClient(%this, '', "\c6A scene of this name already exists in the server!");
				return;
			}

			%s = %this.director_scene.setLabel(%label);
			if(%s)
			{
				commandToClient(%this, 'Director_SetLabel', %label);
				%this.director_label = %label;
				messageClient(%this, '', "\c6Renamed \c3" @ %old SPC "\c6to\c3" SPC %label @ "\c6.");
			}
			else
				messageClient(%this, '', "setLabel returned false for some reason.");
			return;
		}

		if(%label !$= "") //this shouldn't ever happen
		{
			commandToClient(%this, 'Director_SetLabel', %label);
			%this.director_label = %label;
			messageClient(%this, '', "\c6You have changed the scene label to \c3" @ %label @ "\c6.");
			%this.checkSceneLabel();
		}
	}


	function serverCmdShiftBrick(%this, %x, %y, %z)
	{
		parent::serverCmdShiftBrick(%this, %x, %y, %z);
		if(!%this.directorMode || isObject(%this.player.tempBrick))
			return;
		if(%y == 1)
			commandToClient(%this, 'Director_Scrub', "normal", -1);
		else if(%y == -1)
			commandToClient(%this, 'Director_Scrub', "normal", 1);
		else if(%x == 1)
			commandToClient(%this, 'Director_Scrub', "scroll", 1);
		else if(%x == -1)
			commandToClient(%this, 'Director_Scrub', "scroll", -1);
		else if(%z == 1)
			commandToClient(%this, 'Director_Scrub', "fine", 1);
		else if(%z == -1)
			commandToClient(%this, 'Director_Scrub', "fine", -1);
	}

	function serverCmdSuperShiftBrick(%this, %x, %y, %z)
	{
		parent::serverCmdSuperShiftBrick(%this, %x, %y, %z);
		if(!%this.directorMode || isObject(%this.player.tempBrick))
			return;
		if(%y == 1)
			commandToClient(%this, 'Director_Scrub', "fast", -1);
		else if(%y == -1)
			commandToClient(%this, 'Director_Scrub', "fast", 1);
		else if(%x == 1)
			commandToClient(%this, 'Director_Scrub', "scroll", 5);
		else if(%x == -1)
			commandToClient(%this, 'Director_Scrub', "scroll", -5);
		else if(%z == -1)
			commandToClient(%this, 'Director_Scrub', "jump", 0);
		else if(%z == 1)
			commandToClient(%this, 'Director_Scrub', "zoomreset");
	}

	function serverCmdRotateBrick(%this, %val)
	{
		parent::serverCmdRotateBrick(%this, %val);
		if(!%this.directorMode || isObject(%this.player.tempBrick))
			return;
		if(%val < 0)
			commandToClient(%this, 'Director_Scrub', "zoom", -1);
		else if(%val > 0)
			commandToClient(%this, 'Director_Scrub', "zoom", 1);
	}

	function serverCmdDir_SetTimelinePosition(%this, %pos) //it's a weird name because i can imagine someone accidentally breaking things with this
	{
		if(!%this.directorMode)
			return;
		if(%pos < 0)
			%pos = 0;
		%this.director_position = %pos;
	}

	function serverCmdDir_ReqEvents(%this)
	{
		if(!%this.directorMode || !isObject(%this.director_scene))
			return;

		%ct = %this.director_scene.getCount();
		if(%ct <= 0)
			return;
		commandToClient(%this, 'Director_ClearEvents');
		for(%i = 0; %i < %ct; %i++)
		{
			%event = %this.director_scene.getObject(%i);
			commandToClient(%this, 'Director_AddEvent', %event.sid, %event.type, %event.position);
			commandToClient(%this, 'Director_GoodEvent', %event.sid, %event.debugActionCheck(1));
		}
	}
};
activatePackage(DirectorMode);