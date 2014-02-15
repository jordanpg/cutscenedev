if($CutsceneModuleServer::ServerMain)
	return;
$CutsceneModuleServe::ServerMain = true;

//SCRIPT LOADING
exec("./datablocks.cs");
exec("./Cutscene_Main.cs");
exec("./Cutscene_Script.cs");
exec("./Actors_Outfits.cs");
// exec("./event/Event_Camera.cs");
// exec("./event/Event_End.cs");
// exec("./event/Event_Script.cs");
exec("./Director_Server.cs");
exec("./serverCmd.cs");

package Cutscene_Server
{
	function destroyServer()
	{
		%r = parent::destroyServer();
		deleteVariables("$Cutscene::*");
		deleteVariables("$CutsceneModuleServer::*");
		deleteVariables("$CutsceneEvent::*");
		deleteVariables("$CutsceneModule::*");
		CutsceneGroup.delete();
		return %r;
	}	
};
activatePackage(Cutscene_Server);