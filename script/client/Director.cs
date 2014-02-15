$CutsceneModuleClient::Director = true;

//GLOBALS
$Cutscene::Director::DefaultStepSize = 1000;
$Cutscene::Director::ScrubPerc = 0.03;
$Cutscene::Director::FineScrubPerc = 0.008;
$Cutscene::Director::ZoomSpeed = 250;
$Cutscene::Director::MaxZoomLevel = ($Cutscene::Director::ZoomSpeed * 20);

//Dot colour codes for timeline
$DMTYPES = 0;
$DMCOLOR_[""]		= "1 1 1 1";
//Dot picture key
$DMIMG = 	"data/unevent" TAB
			"data/badevent" TAB
			"data/event";

function clientCmdDirector_AddType(%name, %color)
{
	if(%name $= "" || %color $= "")
		return;

	if(searchFields($DMTYPENAMES, %name) != -1)
		return;

	$DMTYPE[$DMTYPES] = %name;
	$DMCOLOR_[strUpr(%name)] = %color;
	$DMTYPENAMES = trim($DMTYPENAMES TAB %name);
}

//some GUI profiles that we're going to just stick here because why not i guess
if(!isObject(DirectorTextProfile))
{
	new GuiControlProfile(DirectorTextProfile)
	{
		opaque = false;
		fontType = "Impact";
		fontSize = 16;
		fontColor = "255 255 255";
		doFontOutline = true;
		fontOutlineColor = "0 0 0";
		justify = "center";
		border = false;
	};
}

//pointless testing functions
function woowee(%i, %add, %s)
{	
	if(isEventPending($wowe))
		cancel($wowe);
	DirectorEvent_h.position = eval("return" SPC %add @ ";");
	directorTimeLineDlg.updateElements();
	if(%i)
		$wowe = schedule(%s, 0, woowee, %i, %add, %s);
}

function directorrandomevent(%good)
{
	%type = getField($DMTYPENAMES, getRandom(0, (getFieldCount($DMTYPENAMES) - 1)));
	%position = getRandom($Director::StartTime, $Director::EndTime);
	%this = Director_EventKeeper.newEvent("", %type, %position);
	if(%good !$= "")
		%this.good = eval("return" SPC %good @ ";");
	else
		%this.good = 1;
	directorTimeLineDlg.updateElements();
	return %this;
}

function phdance(%i, %add, %s)
{
	if(isEventPending($phdance))
		cancel($phdance);
	$Director::Playhead += %add;
	if($Director::Playhead > $Director::EndTime || $Director::Playhead < $Director::StartTime)
	{
		%add *= -1;
		$Director::Playhead += %add;
	}
	directorTimeLineDlg.updateElements();
	if(%i)
		$phdance = schedule(%s, 0, phdance, %i, %add, %s);
}

function Director_CreateEventKeeper(%label)
{
	if(%label $= "")
		%label = "n/a";

	if(isObject(Director_EventKeeper))
		Director_EventKeeper.delete();

	$Director::EventKeeper = new ScriptGroup(Director_EventKeeper)
								{
									label = %label;
									events = 0;
								};
	return Director_EventKeeper;
}

function Director_EventKeeper::NewEvent(%this, %sid, %type, %pos)
{
	if(searchFields($DMTYPENAMES, %type) == -1) //This would indicate some kind of failure in extension of the event system.
		%type = "";

	if(%sid $= "" || isObject(%this.eventkey[%sid]))
		%sid = %this.events;
	%event = new ScriptObject("DirectorEvent_" @ %sid)
				{
					class = "DirectorEvent";
					type = %type;
					sid = %sid;
					position = %pos;
					good = -1;
				};
	%this.eventkey[%sid] = %event;
	%this.eventsid[%this.events] = %sid;
	%this.eventspos[%pos] = trim(%this.eventspos[%pos] TAB %sid);
	%this.events++;
	%this.add(%event);

	if($Director::Active)
		directorTimeLineDlg.updateElements();
	return %event;
}

function Director_EventKeeper::GetRange(%this, %start, %end)
{
	%ct = %this.getCount();
	%list = "";
	for(%i = 0; %i < %ct; %i++)
	{
		%event = %this.getObject(%i);

		%pos = %event.position;
		if(%pos < %start)
			continue;
		if(%pos > %end)
			continue;
		%list = trim(%list SPC %event);
	}
	return %list;
}

function Director_EventKeeper::SetGoodEvent(%this, %sid, %bool)
{
	if(!isObject(%event = %this.eventkey[%sid]))
		return false;

	%event.good = (%bool ? true : false); //resolves to a boolean if we're given something else
	if($Director::Active)
		directorTimeLineDlg.updateElements();
	return true;
}

function Director_EventKeeper::SetLabel(%this, %label)
{
	%this.label = %label;
	if($Director::Active)
		directorTimeLineDlg.updateElements();
}

function Director_EventKeeper::selectEvent(%this, %sid)
{
	if(!isObject(%event = %this.eventkey[%sid]))
		return false;

	$Director::ActiveEvent = %event;
	if($Director::Active)
		directorTimeLineDlg.updateElements();
	return true;
}

function Director_BuildTimeLineGUI()
{
	if(isObject(directorTimeLineDlg))
		directorTimeLineDlg.delete();

	%res = getRes();
	%resX = getWord(%res, 0);

	new GuiSwatchCtrl(directorTimeLineDlg)
	{
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		extent = %resX SPC "52";
		minExtent = "600 52";
		enabled = true;
		visible = true;
		clipToParent = true;
		color = "0 0 0 0";
		position = "0 0";

		new GuiBitmapCtrl(directorBanner)
		{
			profile = "GuiDefaultProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "0 0";
			extent = %resX SPC "52";
			minExtent = "800 52";
			enabled = true;
			visible = true;
			clipToParent = true;
			bitmap = $Cutscene::Root @ "data/banner";
			wrap = false;
			lockAspectRatio = false;
			alignLeft = false;
			alignTop = false;
			overflowImage = false;
			keepCached = false;
			mColor = "255 255 255 255";
			mMultiply = false;

			new GuiSwatchCtrl(directorTimeLineSwatch)
			{
				profile = "GuiDefaultProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				extent =  "600 52";
				minExtent = "600 52";
				enabled = true;
				visible = true;
				clipToParent = true;
				color = "0 0 0 0";
				position = ((%resX / 2) - 300) SPC "0";

				new GuiBitmapCtrl(directorTimeLine)
				{
					profile = "GuiDefaultProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = "60 0";
					extent = "480 32";
					minExtent = "480 32";
					enabled = true;
					visible = true;
					clipToParent = true;
					bitmap = $Cutscene::Root @ "data/timeline";
					wrap = false;
					lockAspectRatio = true;
					alignLeft = false;
					alignTop = false;
					overflowImage = false;
					keepCached = false;
					mColor = "255 255 255 255";
					mMultiply = false;
		
					new GuiSwatchCtrl(directorEventSwatch)
					{
						profile = "GuiDefaultProfile";
						horizSizing = "right";
						vertSizing = "bottom";
						extent = "480 32";
						minExtent = "480 32";
						enabled = true;
						visible = true;
						clipToParent = true;
						color = "0 0 0 0";
						position = "0 0";
					};
				};
		
				new GuiBitmapCtrl(directorStartBack)
				{	
					profile = "GuiDefaultProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = "0 0";
					extent = "60 20";
					minExtent = "60 20";
					enabled = true;
					visible = true;
					clipToParent = true;
					bitmap = $Cutscene::Root @ "data/starttime";
					wrap = false;
					lockAspectRatio = true;
					alignLeft = false;
					alignTop = false;
					overflowImage = false;
					keepCached = false;
					mColor = "255 255 255 255";
					mMultiply = false;
		
					new GuiMLTextCtrl(directorStartTime)
					{
						profile = "DirectorTextProfile";
						horizSizing = "right";
						vertSizing = "bottom";
						position = "0 1";
						extent = "60 18";
						minExtent = "60 18";
						enabled = true;
						visible = true;
						clipToParent = true;
						allowColorChars = true;
						text = "<just:center>0:00:000";
					};
				};
		
				new GuiBitmapCtrl(directorEndBack)
				{	
					profile = "GuiDefaultProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = "540 0";
					extent = "60 20";
					minExtent = "60 20";
					enabled = true;
					visible = true;
					clipToParent = true;
					bitmap = $Cutscene::Root @ "data/endtime";
					wrap = false;
					lockAspectRatio = true;
					alignLeft = false;
					alignTop = false;
					overflowImage = false;
					keepCached = false;
					mColor = "255 255 255 255";
					mMultiply = false;
		
					new GuiMLTextCtrl(directorEndTime)
					{
						profile = "DirectorTextProfile";
						horizSizing = "right";
						vertSizing = "bottom";
						position = "0 1";
						extent = "60 18";
						minExtent = "60 18";
						enabled = true;
						visible = true;
						clipToParent = true;
						allowColorChars = true;
						text = "<just:center>0:00:000";
					};
				};
		
				new GuiBitmapCtrl(directorStepBack)
				{	
					profile = "GuiDefaultProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = "228 32";
					extent = "144 20";
					minExtent = "144 20";
					enabled = true;
					visible = true;
					clipToParent = true;
					bitmap = $Cutscene::Root @ "data/stepsize";
					wrap = false;
					lockAspectRatio = true;
					alignLeft = false;
					alignTop = false;
					overflowImage = false;
					keepCached = false;
					mColor = "255 255 255 255";
					mMultiply = false;
		
					new GuiMLTextCtrl(directorStepSize)
					{
						profile = "DirectorTextProfile";
						horizSizing = "right";
						vertSizing = "bottom";
						position = "0 0";
						extent = "144 18";
						minExtent = "144 18";
						enabled = true;
						visible = true;
						clipToParent = true;
						allowColorChars = true;
						text = "<just:center>Zoom: N/A";
					};
				};
			};

			new GuiMLTextCtrl(directorLabel)
			{
				profile = "DirectorTextProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				position =  "16 4";
				extent = "256 18";
				minExtent = "144 18";
				enabled = true;
				visible = true;
				clipToParent = true;
				allowColorChars = true;
				text = "<just:center><font:impact:18>Name: N/A";
			};
		};
	};

	//Move the rest of the elements of the chat HUD down underneath our stuff.
	%ext = newChatHud.extent;
	%cpos = newChatHud.position;
	newChatHud.resize(getWord(%cpos, 0), getWord(%cpos, 1), getWord(%ext, 0), (getWord(%ext, 1) + 52));
	%ct = newChatHud.getCount();
	for(%i = 0; %i < %ct; %i++)
	{
		%obj =	newChatHud.getObject(%i);
		%pos = %obj.position;
		%ex = %obj.extent;
		%obj.resize(getWord(%pos, 0), (getWord(%pos, 1) + 52), getWord(%ex, 0), getWord(%ex, 1));
	}
	newChatHud.add(directorTimeLineDlg);

	%epos = HUD_EnergyBar.position;
	%eext = HUD_EnergyBar.extent;
	HUD_EnergyBar.resize(getWord(%epos, 0), (getWord(%epos, 1) + 52), getWord(%eext, 0), getWord(%eext, 1));

	%epos = HUD_ToolBox.position;
	%eext = HUD_ToolBox.extent;
	HUD_ToolBox.resize(getWord(%epos, 0), (getWord(%epos, 1) + 52), getWord(%eext, 0), getWord(%eext, 1));

	%epos = HUD_ToolNameBG.position;
	%eext = HUD_ToolNameBG.extent;
	HUD_ToolNameBG.resize(getWord(%epos, 0), (getWord(%epos, 1) + 52), getWord(%eext, 0), getWord(%eext, 1));

	return directorTimeLineDlg;
}

function directorEventSwatch::clearEvents(%this)
{
	if((%ct = %this.getCount()) <= 0)
		return;
	for(%i = %ct - 1; %i >= 0; %i--)
	{
		%bitmap = %this.getObject(%i);
		%event = %bitmap.director_event;
		%bitmap.delete();
		if(isObject(%event))
			Director_EventKeeper.eventbitmap[%event.sid] = "";
	}
}

function directorTimeLineDlg::updateEvents(%this)
{
	if(!$Director::Active)
		return;
	directorEventSwatch.clearEvents();
	%list = Director_EventKeeper.GetRange($Director::StartTime, $Director::EndTime);
	if((%ct = getWordCount(%list)) <= 0)
		return;

	%range = $Director::EndTime - $Director::StartTime;
	%poss = "";
	for(%i = 0; %i < %ct; %i++)
	{
		%event = getWord(%list, %i);
		if(!isObject(%event))
			continue;
		%pos = %event.position;
		%perc = (%pos - $Director::StartTime) / %range;
		%epos = mFloor((%perc * 480) + 0.5);
		%events[%epos] = trim(%events[%epos] SPC %event);
		if(searchFields(%poss, %epos) == -1)
			%poss = trim(%poss TAB %epos);
	}

	if(%poss $= "")
		return;

	%poss = bubbleSort(%poss);
	%ct = getFieldCount(%poss);
	for(%i = 0; %i < %ct; %i++)
	{
		%epos = getField(%poss, %i);
		%es = %events[%epos];
		%ect = getWordCount(%es);
		for(%a = 0; %a < %ect; %a++)
		{
			%event = getWord(%es, %a);
			if(!isObject(%event))
				continue;
			%bitmap = new GuiBitmapCtrl("timelineDot_" @ %event.sid)
						{
							profile = "GuiDefaultProfile";
							horizSizing = "right";
							vertSizing = "bottom";
							position = (%epos - 5) SPC "11";
							extent = "10 10";
							minExtent = "10 10";
							enabled = true;
							visible = true;
							clipToParent = true;
							bitmap = $Cutscene::Root @ getField($DMIMG, %event.good + 1) @ "_n";
							wrap = false;
							lockAspectRatio = true;
							alignLeft = false;
							alignTop = false;
							overflowImage = false;
							keepCached = false;
							mColor = "255 255 255 255";
							mMultiply = false;
							director_event = %event;
							//command = "echo(" @ %event @ ");";
							text = " ";
						};
			%bitmap.setColor($DMCOLOR_[%event.type]);
			directorEventSwatch.add(%bitmap);
			Director_EventKeeper.eventbitmap[%event.sid] = %bitmap;
			%event =  new GuiMouseEventCtrl("timelineDotEvent_" @ %event.sid)
						{
							profile = "GuiDefaultProfile";
							extent = "10 10";
							profile = "0 0";
							clipToParent = true;
							enabled = true;
							visible = true;
							minExtent = "10 10";
							horizSizing = "right";
							vertSizing = "bottom";
							lockMouse = false;
						};
			%bitmap.add(%event);
		}
	}
	if(isObject($Director::ActiveEvent))
	{
		//echo("bongo");
		%bm = Director_EventKeeper.eventBitmap[$Director::ActiveEvent.sid];
		if(!isObject(%bm))
			return;
		%bpos = %bm.position;
		%bm.resize(getWord(%bpos, 0), 1, 10, 10);
		// %selbm = new GuiBitmapButtonCtrl("timelineSelection")
		// 		{
		// 					profile = "GuiButtonProfile";
		// 					horizSizing = "right";
		// 					vertSizing = "bottom";
		// 					position = "0 0";
		// 					extent = "10 10";
		// 					minExtent = "10 10";
		// 					enabled = true;
		// 					visible = true;
		// 					clipToParent = true;
		// 					bitmap = $Cutscene::Root @ "data/activeevent";
		// 					wrap = false;
		// 					lockAspectRatio = true;
		// 					alignLeft = false;
		// 					alignTop = false;
		// 					overflowImage = false;
		// 					keepCached = false;
		// 					mColor = "255 255 255 255";
		// 					mMultiply = false;
		// 					command = "timelineSelection.getGroup().performClick();";
		// 					text = " ";
		// 		};
		// %selbm.setColor($DMCOLOR_[$Director::ActiveEvent.type]);
		// %bm.add(%selbm);
		// echo("bonanza" SPC %bm SPC %selbm);
	}
}

function directorTimeLineDlg::updatePlayhead(%this)
{
	if(!$Director::Active)
		return;
	if(isObject(directorPlayhead))
		directorPlayhead.delete();
	if(isObject(directorPlayheadLine))
		directorPlayheadLine.delete();
	if($Director::Playhead < $Director::StartTime)
		$Director::Playhead = $Director::StartTime;
	if($Director::Playhead > $Director::EndTime)
		$Director::Playhead = $Director::EndTime;

	%range = $Director::EndTime - $Director::StartTime;
	%perc = ($Director::Playhead - $Director::StartTime) / %range;
	%pos = mFloor((%perc * 480) + 0.5);
	%x = %pos - 4;
	%playhead = new GuiBitmapCtrl(directorPlayhead)
				{
					profile = "GuiDefaultProfile";
					horizSizing = "right";
					vertSizing = "bottom";
					position = (%x + 60) SPC "32";
					extent = "9 16";
					minExtent = "9 16";
					enabled = true;
					visible = true;
					clipToParent = true;
					bitmap = $Cutscene::Root @ "data/playhead";
					wrap = false;
					lockAspectRatio = true;
					alignLeft = false;
					alignTop = false;
					overflowImage = false;
					keepCached = false;
					mColor = "255 255 255 255";
					mMultiply = false;
				};
	%line = new GuiSwatchCtrl(directorPlayheadLine)
			{
				profile = "GuiDefaultProfile";
				horizSizing = "right";
				vertSizing = "bottom";
				extent = "1 32";
				minExtent = "1 32";
				enabled = true;
				visible = true;
				clipToParent = true;
				color = "255 255 255 255";
				position = (%x + 64) SPC "0";
			};
	directorTimeLineSwatch.add(%playhead, %line);
	commandToServer('Dir_SetTimelinePosition', $Director::Playhead);
}

function directorTimeLineDlg::updateElements(%this)
{
	if(!$Director::Active)
		return;
	directorStartTime.setText("<just:center>" @ formatMSTime($Director::StartTime));
	directorEndTime.setText("<just:center>" @ formatMSTime($Director::EndTime));
	if($Director::StepSize < $Cutscene::Director::ZoomSpeed)
		$Director::StepSize = $Cutscene::Director::ZoomSpeed;
	if($Director::StepSize > $Cutscene::Director::MaxZoomLevel)
		$Director::StepSize = $Cutscene::Director::MaxZoomLevel;
	directorStepSize.setText("<just:center>Zoom:" SPC mFloatLength($Director::StepSize / 1000, 2) @ "x");
	directorLabel.setText("<just:left><font:impact:18>Name:" SPC Director_EventKeeper.label);
	%this.updateEvents();
	%this.updatePlayhead();
	newChatHud.add(%this);
}

function Director_ModifyTimeline(%start, %step, %maintain)
{
	if(!$Director::Active)
		return;

	if(%start <= 0)
	{
		$Director::Playhead = 0;
		%start = 0;
		%maintain = 0;
	}
	if(%maintain)
	{
		%range = $Director::EndTime - $Director::StartTime;
		%perc = ($Director::Playhead - $Director::StartTime) / %range;
	}
	$Director::StartTime = %start;
	$Director::StepSize = (%step > 1 ? %step : 1000);
	$Director::EndTime = $Director::StartTime + ($Director::StepSize * 15);

	if(%maintain)
	{	
		%range = $Director::EndTime - $Director::StartTime;
		%add = mFloor((%range * %perc) + 0.5);
		$Director::Playhead = $Director::StartTime + %add;
	}

	directorTimeLineDlg.updateElements();
}

function Director_Activate()
{
	if(Canvas.getContent().getID() != PlayGUI.getID() || $Director::Active)
		return;

	$Director::Active = true;
	Director_CreateEventKeeper();
	PlayGUI.add(Director_BuildTimeLineGUI());
	if($Director::StepSize $= "")
		$Director::StepSize = $Cutscene::Director::DefaultStepSize;
	if($Director::StartTime $= "")
		$Director::StartTime = 0;
	$Director::EndTime = $Director::StartTime + ($Director::StepSize * 15);

	$Director::Playhead = $Director::StartTime;

	directorTimeLineDlg.updateElements();
	commandToServer('Dir_ReqEvents');
}

function Director_Exit()
{
	if(!$Director::Active)
		return;

	if(isObject(directorTimeLineDlg))
		directorTimeLineDlg.delete();
	if(isObject(Director_EventKeeper))
		Director_EventKeeper.delete();

	deleteVariables("$Director::*");

	//Move the chat HUD elements back up
	%ext = newChatHud.extent;
	%cpos = newChatHud.position;
	newChatHud.resize(getWord(%cpos, 0), getWord(%cpos, 1), getWord(%ext, 0), (getWord(%ext, 1) - 52));
	%ct = newChatHud.getCount();
	for(%i = 0; %i < %ct; %i++)
	{
		%obj =	newChatHud.getObject(%i);
		%pos = %obj.position;
		%ex = %obj.extent;
		%obj.resize(getWord(%pos, 0), (getWord(%pos, 1) - 52), getWord(%ex, 0), getWord(%ex, 1));
	}

	%epos = HUD_EnergyBar.position;
	%eext = HUD_EnergyBar.extent;
	HUD_EnergyBar.resize(getWord(%epos, 0), (getWord(%epos, 1) - 52), getWord(%eext, 0), getWord(%eext, 1));

	%epos = HUD_ToolBox.position;
	%eext = HUD_ToolBox.extent;
	HUD_ToolBox.resize(getWord(%epos, 0), (getWord(%epos, 1) - 52), getWord(%eext, 0), getWord(%eext, 1));

	%epos = HUD_ToolNameBG.position;
	%eext = HUD_ToolNameBG.extent;
	HUD_ToolNameBG.resize(getWord(%epos, 0), (getWord(%epos, 1) - 52), getWord(%eext, 0), getWord(%eext, 1));
}

function Director_Scrub(%amt, %restrict)
{
	if(!$Director::Active)
		return "";
	$Director::Playhead += %amt;
	if($Director::Playhead > $Director::EndTime)
	{
		if(%restrict)
			$Director::Playhead = $Director::EndTime;
		else
			Director_ModifyTimeline($Director::EndTime, $Director::StepSize);
	}
	else if($Director::Playhead < $Director::StartTime)
	{
		if(%restrict)
			$Director::Playhead = $Director::StartTime;
		else if((%t = ($Director::StartTime - ($Director::StepSize * 15))) >= 0)
			Director_ModifyTimeline(%t, $Director::StepSize);
		else
			$Director::Playhead = $Director::StartTime;
	}
	directorTimeLineDlg.updateElements();
	return $Director::Playhead;
}

function Director_EasyScrub(%type, %mult)
{
	if(!$Director::Active)
		return;

	if(%mult $= "")
		%mult = 1;

	if(%type $= "normal")
		%val = $Director::StepSize * $Cutscene::Director::ScrubPerc;
	else if(%type $= "fine")
		%val = $Director::StepSize * $Cutscene::Director::FineScrubPerc;
	else if(%type $= "fast")
		%val = $Director::StepSize;
	else if(%type $= "scroll")
	{
		%start = $Director::StartTime;
		%add = ($Director::StepSize * 15) * %mult;
		Director_ModifyTimeline(%start + %add, $Director::StepSize, 1);
	}
	else if(%type $= "jump")
	{
		if(%mult > $Director::EndTime || %mult < $Director::StartTime)
		{
			%len = $Director::StepSize * 15;
			%scroll = mFloor((%mult / %len) + 0.5);
			Director_ModifyTimeline(%len * %scroll, $Director::StepSize);
		}
		$Director::Playhead = %mult;
	}
	else if(%type $= "zoom")
	{
		%add = $Cutscene::Director::ZoomSpeed * %mult;
		$Director::StepSize += %add;
		if($Director::StepSize < $Cutscene::Director::ZoomSpeed)
			$Director::StepSize = $Cutscene::Director::ZoomSpeed;
		if($Director::StepSize > $Cutscene::Director::MaxZoomLevel)
			$Director::StepSize = $Cutscene::Director::MaxZoomLevel;
		Director_ModifyTimeline($Director::StartTime, $Director::StepSize);
	}
	else if(%type $= "zoomreset")
		$Director::StepSize = 1000;
	else
		%val = $Director::StepSize;

	%val *= %mult;
	Director_Scrub(%val);
}

package DirectorClient
{
	function ToggleCursor(%val) //Workaround for an issue where you couldn't use the PlayGUI cursor in singleplayer.
	{
		if($Pref::Net::ServerType $= "SinglePlayer" && (Canvas.getContent() == nameToID(PlayGUI) || Canvas.getContent() == nameToID(NoHudGUI)) && %val)
		{
			$SinglePlayerCursor = !$SinglePlayerCursor;
			if($SinglePlayerCursor)
				cursorOn();
			else
				cursorOff();
			return;
		}
		parent::ToggleCursor(%val);
	}

	function GuiMouseEventCtrl::onMouseDown(%this, %mod, %pos, %click)
	{
		%parent = %this.getGroup();
		if(isObject(%parent.director_event))
		{
			%parent.setBitmap($Cutscene::Root @ getField($DMIMG, %parent.director_event.good + 1) @ "_d");
			if(%click == 1)
				schedule(150, 0, commandToServer, 'DirectorSelectEvent', %parent.director_event.sid);
		}
		else
			parent::onMouseDown(%this, %mod, %pos, %click);
	}

	function GuiMouseEventCtrl::onMouseUp(%this, %mod, %pos, %click)
	{
		%parent = %this.getGroup();
		if(isObject(%parent.director_event))
		{
			%parent.setBitmap($Cutscene::Root @ getField($DMIMG, %parent.director_event.good + 1) @ "_h");
			if(%click >= 2)
			{
				echo("LEFT DOUBLE" SPC %this SPC %parent);
				//blegh
			}
		}
		else
			parent::onMouseUp(%this, %mod, %pos, %click);
	}

	function GuiMouseEventCtrl::onRightMouseDown(%this, %mod, %pos, %click)
	{
		%parent = %this.getGroup();
		if(isObject(%parent.director_event))
		{
			%parent.setBitmap($Cutscene::Root @ getField($DMIMG, %parent.director_event.good + 1) @ "_d");
		}
		else
			parent::onRightMouseDown(%this, %mod, %pos, %click);
	}

	function GuiMouseEventCtrl::onRightMouseUp(%this, %mod, %pos, %click)
	{
		%parent = %this.getGroup();
		if(isObject(%parent.director_event))
		{
			%parent.setBitmap($Cutscene::Root @ getField($DMIMG, %parent.director_event.good + 1) @ "_h");
			echo("RIGHT SINGLE" SPC %this SPC %parent);
		}
		else
			parent::onRightMouseUp(%this, %mod, %pos, %click);
	}

	function GuiMouseEventCtrl::onMouseEnter(%this, %mod, %pos, %click)
	{
		%parent = %this.getGroup();
		if(isObject(%parent.director_event))
		{
			%parent.setBitmap($Cutscene::Root @ getField($DMIMG, %parent.director_event.good + 1) @ "_h");
		}
		else
			parent::onMouseEnter(%this, %mod, %pos, %click);
	}

	function GuiMouseEventCtrl::onMouseLeave(%this, %mod, %pos, %click)
	{
		%parent = %this.getGroup();
		if(isObject(%parent.director_event))
		{
			%parent.setBitmap($Cutscene::Root @ getField($DMIMG, %parent.director_event.good + 1) @ "_n");
		}
		else
			parent::onMouseLeave(%this, %mod, %pos, %click);
	}

	function disconnectedCleanup()
	{
		%r = parent::disconnectedCleanup();
		if($Director::Active)
			Director_Exit();
		return %r;
	}
};
activatePackage(DirectorClient);