$CutsceneModuleServer::Server = true;

//DEPENDENCIES
if(!$CutsceneModule::Main)
	exec("./main.cs");

//SCRIPT LOADING
exec("./script/server/ServerMain.cs");