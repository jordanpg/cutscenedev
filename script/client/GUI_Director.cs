//GLOBALS
$Cutscene::Director::DefaultStepSize = 1000;

function addLeadingZeros(%src, %num)
{
	%len = strLen(%src);
	%zeros = %num - %len;
	if(%zeros < 0)
		return %src;
	for(%i = 1; %i <= %zeros; %i++)
		%add = %add @ "0";
	return %add @ %src;
}

function formatMSTime(%ms)
{
	%ms = mAbs(%ms);

	%mil = %ms % 1000;
	%sc = mFloor(%ms / 1000);
	%sec = %sc % 60;
	%min = mFloor(%sc / 60);

	return %min @ ":" @ addLeadingZeros(%sec, 2) @ ":" @ addLeadingZeros(%mil, 3);
}

function Director_BuildTimeLineGUI()
{
	if(isObject(directorTimeLineDlg))
		directorTimeLineDlg.delete();

	new GuiSwatchCtrl(directorTimeLineDlg)
	{
		profile = "GuiDefaultProfile";
		horizSizing = "right";
		vertSizing = "bottom";
		extent = "600 50";
		minExtent = "512 32";
		enabled = true;
		visible = true;
		clipToParent = true;
		color = "0 0 0 0";
		position = ((getWord(getRes(), 0) / 2) - 300) SPC "0";

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
		};

		new GuiMLTextCtrl(directorStartTime)
		{
			profile = "GuiTextProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "0 0";
			extent = "60 18";
			minExtent = "60 18";
			enabled = true;
			visible = true;
			clipToParent = true;
			allowColorChars = true;
			text = "<just:left>0:00:000";
		};

		new GuiMLTextCtrl(directorEndTime)
		{
			profile = "GuiTextProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "540 0";
			extent = "60 18";
			minExtent = "60 18";
			enabled = true;
			visible = true;
			clipToParent = true;
			allowColorChars = true;
			text = "<just:right>0:00:000";
		};

		new GuiMLTextCtrl(directorStepSize)
		{
			profile = "GuiTextProfile";
			horizSizing = "right";
			vertSizing = "bottom";
			position = "180 32";
			extent = "240 18";
			minExtent = "120 18";
			enabled = true;
			visible = true;
			clipToParent = true;
			allowColorChars = true;
			text = "<just:center>Step: N/A";
		};
	};

	return directorTimeLineDlg;
}

function directorTimeLineDlg::refreshElements(%this)
{
	directorStartTime.setText("<just:left>" @ formatMSTime($Director::StartTime));
	directorEndTime.setText("<just:right>" @ formatMSTime($Director::EndTime));
	directorStepSize.setText("<just:center>Step:" SPC $Director::StepSize @ "ms");
	//Update event dots
}

function Director_Activate()
{
	if(Canvas.getContent().getID() != PlayGUI.getID() || $Director::Active)
		return;

	PlayGUI.add(Director_BuildTimeLineGUI());
	if($Director::StepSize $= "")
		$Director::StepSize = $Cutscene::Director::DefaultStepSize;
	if($Director::StartTime $= "")
		$Director::StartTime = 0;
	$Director::EndTime = $Director::StartTime + ($Director::StepSize * 15);

	directorTimeLineDlg.refreshElements();
}