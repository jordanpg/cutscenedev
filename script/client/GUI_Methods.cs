$CutsceneModuleClient::GUI_Methods = true;

function NoHudGUI::Widescreen(%this, %height, %len, %speed, %color)
{
	if(isObject(WidescreenTop))
		WidescreenTop.delete();
	if(isObject(WidescreenBottom))
		WidescreenBottom.delete();
	if(%height <= 0 || %len <= 0)
		return;

	%speed = mFloor(%speed + 0.5);
	%res = getRes();
	%xRes = getWord(%res, 0);
	%yRes = getWord(%res, 1);
	%top = new GuiSwatchCtrl(WidescreenTop)
			{
				color = (%color $= "" ? "0 0 0 255" : %color);
				position = "0" SPC -%height;
				extent = %xRes SPC %height;
			};
	%bottom = new GuiSwatchCtrl(WidescreenBottom)
			{
				color = (%color $= "" ? "0 0 0 255" : %color);
				position = "0" SPC %yRes;
				extent = %xRes SPC %height;
			};
	NoHudGUI.add(%top, %bottom);
	if(%speed <= 0)
	{
		%top.position = "0 0";
		%bottom.position = "0" SPC %yRes - %height;
		return;
	}
	%top.Slide(0, -%height, 0, 0, %len, %speed);
	%bottom.Slide(0, %yRes, 0, %yRes - %height, %len, %speed);
}

function GuiSwatchCtrl::Slide(%this, %x, %y, %nX, %nY, %len, %speed, %i)
{
	if(isEventPending(%this.slide))
		cancel(%this.slide);
	%steps = %speed / %len;
	if(%steps > 0.25)
		%steps = 0.25;
	%sched = mFloor((%len / %speed) + 0.5);
	if(%i > mFloor(%len / %sched))
	{
		%this.position = %nX SPC %nY;
		return;
	}
	%diffX = %nX - %x;
	%diffY = %nY - %y;
	%pX = getWord(%this.position, 0);
	%pY = getWord(%this.position, 1);
	%this.position = %pX + (%diffX * %steps) SPC %pY + (%diffY * %steps);
	echo(%this SPC %this.position TAB %diffX SPC %diffY TAB %steps TAB %i);

	if(%this.position !$= (%nX SPC %nY))
	{
		%this.slide = %this.schedule(%sched, Slide, %x, %y, %nX, %nY, %len, %speed, %i + 1);
		echo(%this SPC %this.position TAB %nX SPC %nY TAB (%this.position !$= (%nX SPC %nY)));
	}
}