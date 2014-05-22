$Cutscene::ModuleclientCmd = true;

function clientCmdCutscene_SetHUD(%i)
{
	%content = Canvas.getContent().getID();
	if(%content != PlayGUI.getID() && %content != NoHudGUI.getId()) //we just assume the client isn't ingame if this is the case
		return;

	if(%i)
		Canvas.setContent(PlayGUI);
	else
		Canvas.setContent(NoHudGUI);
}

function clientCmdCutscene_Widescreen(%height, %len, %speed, %color)
{
	if(Canvas.getContent().getID() != NoHudGUI.getID())
		return;
	NoHudGUI.Widescreen(%height, %len, %speed, %color);
}

function clientCmdCutscene_Caption(%type, %msg, %time, %i)
{
	if(%content != PlayGUI.getID() && %content != NoHudGUI.getId())
		return;
	switch$(%type)
	{
		case "Bottom":
			if(isEventPending($Cutscene::ReturnBottomPrint))
				cancel($Cutscene::ReturnBottomPrint);

			if(Canvas.getContent().getID() == NoHudGUI.getID())
				NoHudGUI.add(bottomPrintDlg);
			bottomPrint(%msg, %time, %i);
			$Cutscene::ReturnBottomPrint = PlayGUI.schedule((%time * 1000) + 1000, add, bottomPrintDlg);
		case "Center":
			if(isEventPending($Cutscene::ReturnCentrePrint))
				cancel($Cutscene::ReturnCentrePrint);

			if(Canvas.getContent().getID() == NoHudGUI.getID())
				NoHudGUI.add(centerPrintDlg);
			centerPrint(%msg, %time);
			$Cutscene::ReturnCentrePrint = PlayGui.schedule((%time * 1000) + 1000, add, centerPrintDlg);
	}
}

function clientCmdDirector_Activate()
{
	%control = ServerConnection.getControlObject();
	if(!isObject(%control))
		return;
	Director_Activate();
}

function clientCmdDirector_Exit()
{
	if(!$Director::Active)
		return;
	Director_Exit();
}

function clientCmdDirector_AddEvent(%sid, %type, %position)
{
	if(!$Director::Active || !isObject(Director_EventKeeper))
		return;
	Director_EventKeeper.newEvent(%sid, %type, %position);
}

function clientCmdDirector_ClearEvents()
{
	if(!$Director::Active || !isObject(Director_EventKeeper))
		return;
	Director_CreatEventKeeper(Director_EventKeeper.label);
}

function clientCmdDirector_GoodEvent(%sid, %val)
{
	if(!$Director::Active || !isObject(Director_EventKeeper))
		return;
	Director_EventKeeper.setGoodEvent(%sid, %val);
}

function clientCmdDirector_SetLabel(%label)
{
	if(!$Director::Active || !isObject(Director_EventKeeper) || %label $= "")
		return;

	Director_EventKeeper.setLabel(%label);
}

function clientCmdDirector_Scrub(%type, %mult)
{
	if(!$Director::Active)
		return;

	Director_EasyScrub(%type, %mult);
}