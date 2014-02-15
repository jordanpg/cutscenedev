if($CutsceneModuleClient::ClientMain)
	return;
$CutsceneModuleClient::ClientMain = true;

//SCRIPT LOADING
exec("./GUI_Methods.cs");
exec("./Director.cs");
exec("./clientCmd.cs");