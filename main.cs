if($CutsceneModule::Main)
	return;
$CutsceneModule::Main = true;

//GLOBALS
$Cutscene::Root = "config/scripts/mod/cutscenedev/";
$Cutscene::Version = "1.0"; //Version numbers beginning on January 30, 2014

//SCRIPT LOADING
exec("./script/dependency.cs");