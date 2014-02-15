$CutsceneModuleClient::Client = true;

//DEPENDENCIES
if(!$CutsceneModule::Main)
	exec("./main.cs");

//SCRIPT LOADING
exec("./script/client/ClientMain.cs");