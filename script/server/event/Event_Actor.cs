$CutsceneModuleServer::Event_Actor = true;

//DEPENDENCY SCRIPTS
if(!$CutsceneModuleServer::Actors_Outfits)
	exec($Cutscene::Root @ "script/server/Actors_Outfits.cs");
if(!isPackage(AIRecycler))
	exec("./AIRecycler.cs");

//FUNCTIONS
function CreateActorGroup()
{
	if(isObject(ActorGroup))
		return ActorGroup;
	%this = new SimSet(ActorGroup);
	missionCleanup.add(ActorGroup);
	return %this;
}
CreateActorGroup();

function Actor_New(%name, %data, %trans)
{
	if(!isObject(ActorGroup))
		CreateActorGroup();
	if(!isObject(%data) || %data.getClassName() !$= "PlayerData")
		return -1;
	if(%name $= "")
		%name = "Mindless Robot";
	if(%trans $= "")
		%trans = "0 0 2 0 0 0 0";

	%this = new AIPlayer("Actor_" @ (ActorGroup.getCount() + 1))
			{
				datablock = %data;
				position = "0 0 0";

				isActor = true;
				name = %name;
				client = CreateAIClient();
			};
	%this.setTransform(%trans);
	missionCleanup.add(%this);
	ActorGroup.add(%this);
	%this.client.player = %this;
	return %this;
}

function AIPlayer::setOutfit(%this, %fileName, %override)
{
	if((fileBase(%this.getDatablock().shapeFile) $= "m.dts") ^ %override) //Means that only if the player has the default model, hide nodes, unless the override is given. ie, 0 ^ 0 = 0, 1 ^ 1 = 0, 0 ^ 1 = 1, and 1 ^ 0 = 1.
		%this.hideNode("ALL");
	return %this.applyOutfit(%fileName);
}