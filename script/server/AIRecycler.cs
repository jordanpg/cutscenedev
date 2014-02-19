//AI RECYCLER -- RESOURCE BY XALOS
//http://forum.blockland.us/index.php?topic=241284.0
package AIRecycler
{
	function GameConnection::onDeath(%cl, %col, %killer, %damageType, %damageArea)
	{
		if(%cl.getClassName() $= "AIConnection")
		{
			if(!isObject(AIRecycler))
				new SimSet(AIRecycler);
			%cl.player.client = "";
			if(!%cl.persistent)
				%cl.recycle();
		}
		Parent::onDeath(%cl, %col, %killer, %damageType, %damageArea);
	}
};
activatePackage("AIRecycler");

function CreateAIClient(%persistent)
{
	if(isObject(AIRecycler))
	{
		if(AIRecycler.getCount() != 0)
		{
			%cl = AIRecycler.getObject(0);
			%cl.persistent = %persistent;
			AIRecycler.remove(%cl);
			return %cl;
		}
	}
	return new AIConnection() { persistent = %persistent; };
}

//.recycle courtesy of Greek2Me
function AiConnection::recycle(%this)
{
	if(isObject(%this.minigame))
		%this.minigame.removeMember(%this);

	if(isObject(%this.camera))
		%this.camera.delete();
	if(isObject(%this.player))
		%this.player.delete();
	if(isObject(%this.tempBrick))
		%this.tempBrick.delete();
	if(isObject(%this.brickGroup) && %this.brickGroup.client == %this)
		%this.brickGroup.client = -1;

	%index = 0;
	while((%field = %this.getTaggedField(%index)) !$= "")
	{
		//some fields cannot be changed once set.... Thanks, Badspot.
		if(%lastField $= %field)
		{
			%index ++;
			continue;
		}
		%lastField = %field;
		%field = getField(%field,0);

		//Prevent people from breaking things
		if(%field !$= stripChars(%field," `~!@#$%^&*()-=+[{]}\\|;:\'\",<.>/?"))
		{
			error("ERROR (AiConnection::recycle): Invalid field! Skipping...");
			%index ++;
			continue;
		}

		eval(%this @ "." @ %field SPC "= \"\";");
	}

	if(!isObject(aiRecycler))
	{
		new SimSet(aiRecycler);
		missionCleanup.add(aiRecycler);
	}

	aiRecycler.add(%this);
}