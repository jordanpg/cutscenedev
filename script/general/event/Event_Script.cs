$Cutscene::ModuleEvent_Script = true;

//CLASS FUNCTIONS
function CutsceneEvent::ActionCheckScript(%this, %nospam)
{
	if(%this.type !$= "Script")
		return false;

	%gross = false;
	//Assign relevant parameters to variables
	%file = %this.getParameter("File");
	if(!isFile(%file)) //If the file doesn't exist
	{
		if(!%nospam)
			schedule(0, 0, warn, "CutsceneEvent::ActionCheckScript -" SPC %this.getName() SPC "has an invalid parameter! (File does not exist)");
		%gross = true;
	}
	return !%gross;
}

function CutsceneEvent::ActionTypeScript(%this)
{
	if(%this.type !$= "Script")
		return false;

	//Assign relevant parameters to variables
	%file = %this.getParameter("File");

	if(!isFile(%file))
		return false;

	//Assign temporary globals for use in scripts
	$SceneTemp::Cutscene = %this.parent;
	$SceneTemp::Viewer = (isObject(%this.parent.viewer) ? %this.parent.viewer : -1);
	$SceneTemp::Event = %this;
	$SceneTemp::SID = %this.sid;
	//Attempt to execute the file; assign boolean returned to %s
	%s = exec(%file);
	//Clear temporary globals
	schedule(0, 0, Cutscene_ClearTempVars);
	
	return %s;
}