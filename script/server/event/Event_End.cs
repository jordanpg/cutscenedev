$CutsceneModuleServer::Event_End = true;

//This doesn't need to actually do anything for now. It's the only hard-coded event type.
//CLASS FUNCTIONS
function CEEnd::EventCheck(%this, %nospam)
{
	if(%this.type !$= "End")
		return false;
	return true;
}

function CEEnd::EventExecute(%this)
{
	if(%this.type !$= "End")
		return false;
	return true;
}