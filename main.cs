if($CutsceneModule::Main)
	return;
$CutsceneModule::Main = true;

//GLOBALS
$Cutscene::Root = "Add-Ons/System_Cutscene/";
$Cutscene::Version = "0.1.0"; //Version counting beginning at February 15, 2014

//SCRIPT LOADING
exec("./script/dependency.cs");