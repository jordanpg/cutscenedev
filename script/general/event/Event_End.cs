$Cutscene::ModuleEvent_End = true;

//This doesn't need to actually do anything for now. It's the only hard-coded event type.
//CLASS FUNCTIONS
function CutsceneEvent::ActionCheckEnd(%this, %nospam)
{
	if(%this.type !$= "End")
		return false;
	return true;
}

function CutsceneEvent::ActionTypeEnd(%this)
{
	if(%this.type !$= "End")
		return false;
	return true;
}