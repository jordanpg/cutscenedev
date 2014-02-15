$CutsceneModuleServer::Director_Server = true;

$Cutscene::DirectorMode::MaxCameraShapes = 50;
$Cutscene::DirectorMode::CameraShapeTimeout = 1;

function PathCamera::testmovie(%this, %stack, %s)
{
	cancel(%stack.unhide);
	%stack.HideCamShapes(1);
	%ct = %stack.getCount();
	%this.reset();
	%this.setState("forward");
	%this.pushBack(%stack.getObject(0).getTransform(), getPathCamSpeed(%this.getPosition(), %stack.getObject(0).getPosition(), 0.1), "Normal", "Linear");
	for(%i = 1; %i < %ct; %i++)
		%this.pushBack(%stack.getObject(%i).getTransform(), getPathCamSpeed(%this.getPosition(), %stack.getObject(%i).getPosition(), %s), "Normal", "Linear");
	%len = (%ct - 1) * (%s * 1000);
	%stack.unhide = %stack.schedule(%len, HideCamShapes, 0);
}

function SimGroup::HideCamShapes(%this, %bool)
{
	if(%this.className !$= "CamStack")
		return;
	%ct = %this.getCount();
	for(%i = 0; %i < %ct; %i++)
		%this.getObject(%i).setHidden(%bool);
}

function StaticShape::camRemoveEffect(%this)
{
	%obj = new Projectile()
	{
		dataBlock = CameraDestroyProjectile;
		initialPosition = %this.getPosition();
	};
	MissionCleanup.add(%obj);
	%obj.explode();
}

function StaticShape::camSelectEffect(%this)
{
	%obj = new Projectile()
	{
		dataBlock = CameraSelectProjectile;
		initialPosition = %this.getPosition();
	};
	MissionCleanup.add(%obj);
	%obj.explode();
}

function StaticShape::camCreateEffect(%this)
{
	%obj = new Projectile()
	{
		dataBlock = CameraCreationProjectile;
		initialPosition = %this.getPosition();
	};
	MissionCleanup.add(%obj);
	%obj.explode();
}

package DirectorMode
{
	function GameConnection::autoAdminCheck(%this)
	{
		%p = parent::autoAdminCheck(%this);
		commandToClient(%this, 'Director_Handshake');
		return %p;
	}

	function serverCmdDirector_Handshake(%this)
	{
		warn("Player" SPC %this.getPlayerName() SPC "has given the director's handshake! Sending them stuff about our stuff...");
		%this.directorHandshake = true;
		for(%i = 0; %i < $CutsceneEvent::Types; %i++)
			commandToClient(%this, 'Director_AddType', $CutsceneEvent::Type[%i], $CutsceneEvent::UIColour[%i]);
	}

	function PathCamera::onNode(%this, %node)
	{
		parent::onNode(%this, %node);
		echo(%node);
	}

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
			%this.director_label = %label;
		%this.checkSceneLabel();
		commandToClient(%this, 'Director_SetLabel', %this.director_label);
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

	function serverCmdDirectorSelectEvent(%this, %sid)
	{
		if(!%this.directorMode)
			return;
		if(!isObject(%this.director_scene))
		{
			messageClient(%this, '', "\c6You don't have an active scene!");
			return;
		}
		if(%this.director_scene.getCount() == 0)
		{
			messageClient(%this, '', "\c6Scene has no events!");
			return;
		}
		if(!isObject(%event = %this.director_scene.findEventBySID(%sid)))
		{
			messageClient(%this, '', "\c6Event doesn't exist!");
			return;
		}
		%this.director_active = %event;
		commandToClient(%this, 'Director_SetActiveEvent', %sid);
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
				%this.director_label = %label;
				messageClient(%this, '', "\c6Renamed \c3" @ %old SPC "\c6to\c3" SPC %label @ "\c6.");
			}
			else
				messageClient(%this, '', "setLabel returned false for some reason.");
			commandToClient(%this, 'Director_SetLabel', %this.director_label);
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

	function serverCmdPlantBrick(%this)
	{
		parent::serverCmdPlantBrick(%this);
		if(!%this.directorMode || isObject(%this.player.tempBrick))
			return;

		if(%this.instantUseData == nameToID(BrickCameraData) && isObject(%this.player))
		{
			if(!isObject(%this.camShapeStack))
			{
				if(isObject(%obj = "CamStack_" @ %this.bl_id))
					%this.camShapeStack = %obj;
				else
					%this.camShapeStack = new SimGroup("CamStack_" @ %this.bl_id){className = "CamStack";};
			}
			if(%this.camShapeStack.getCount() >= $Cutscene::DirectorMode::MaxCameraShapes)
			{
				%this.centerPrint("\c6You cannot place any more camera shapes! (Limit is \c3" @ $Cutscene::DirectorMode::MaxCameraShapes @ "\c6)", 3);
				return;
			}
			if($Sim::Time - %this.director_lastcam < $Cutscene::DirectorMode::CameraShapeTimeout)
				return;

			%eye = %this.getControlObject().getEyeTransform();
			%shape = new StaticShape()
					{
						datablock = CameraShapeData;
						client = %this;
						bl_id = %this.bl_id;

						position = "0 0 0";
						rotation = "0 0 0 0";

						isCamShape = true;
					};
			MissionCleanup.add(%shape);
			%this.camShapeStack.add(%shape);
			%shape.setTransform(%eye);
			serverPlay3D(BrickPlantSound, getWords(%eye, 0, 2));
			%this.director_lastcam = $Sim::Time;
			%shape.camCreateEffect();
			//do some other stuff to make it functional when that part of development comes
		}
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

		commandToClient(%this, 'Director_FlushEventKeeper');
		%ct = %this.director_scene.getCount();
		for(%i = 0; %i < %ct; %i++)
		{
			%event = %this.director_scene.getObject(%i);
			commandToClient(%this, 'Director_AddEvent', %event.sid, %event.type, %event.position);
			commandToClient(%this, 'Director_GoodEvent', %event.sid, %event.EventCheck(1));
		}
		if(isObject(%this.director_active))
			commandToClient(%this, 'Director_SetActiveEvent', %this.director_active.sid);
	}
};
activatePackage(DirectorMode);