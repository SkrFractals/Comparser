namespace Comparser.Forms.Controls;

public partial class PlotSettingsControl : UserControl {
	// TODO find a way so that click Buttons won't take away the focus from RichTextBoxes
	public bool IsVisible = true;
	private int _controlTabIndex;
	public PlotSettingsControl() {
		InitializeComponent();
		
		string resize = "\nYou can also change this visually by drag - resizing the plot preview window.",
			pinS = "Pins the start value.\n", pinC = "Pins the center value.\n", pinE = "Pins the end value.\n",
			ifPin = ", then this values will stay, while the other will stretch: ",
			pinT = "If you change the animation length" + ifPin,
			pinX = "If you resize the window width" + ifPin,
			pinY = "If you resize the window height" + ifPin,
			ce = "center and end",
			se = "start and end",
			sc = "start and center",
			valT = "The value of the input number 't' at the ",
			valX = "The x-axis contribution to the input number 'z' at the ",
			valY = "The y-axis contribution to the input number 'z' at the ",
			valOy = "The y-axis of the output for the 1D plot modes (Area X and Line X) at the ",
			noStretch = ".\n So the image will not stretch, and the",
			pos = " of the image, at the pixel position #",
			resizeH = ".\nWhen you resize the height, then this range will stay the same, stretching the image on this axis.",
			pinYs = pinS + pinY + ce + noStretch + "top side with stay pinned.",
			pinYc = pinC + pinY + ce + noStretch + "center with stay pinned.",
			pinYe = pinE + pinY + ce + noStretch + "bottom side with stay pinned.",
			setF = "Set the fixed Y value (the Y contribution in the 1D mode, aka the slide of the 2D plane at this value) to",
			zt = "z = the input space sample. the same z value the expression below had received for this pixel.\n"
				+ "t = the input time sample. the same t value the expression below had received for this frame.\n";
		SetupControl(loadButton, "Load a plotter configuration file.");
		SetupControl(saveButton, "Save a plotter configuration file, PNG image, PNG series, or MP4 video.\nBased on which type of export you have selected in the combo box on the right.");
        SetupControl(saveSelect, "Pick what kind of file do you want to sve when clicking the SAVE button on the left.");
        SetupControl(itlBox, "How many animation frames will the whole loop have?\nEach frame will supply an input number 't' from the Time range below.\nFrame #0 will have t=TStart, Frame #Length (after the last frame) would have t=TEnd.");
        SetupControl(prevButton, "Go to the previous animation frame.");
		SetupControl(itfBox, "Goto a specific animation frame, or just see which one you are currently displaying.");
		SetupControl(nextButton, "Go to the next animation frame.");
        SetupControl(animatedBox, "Toggle preview animation.\nIt will continuously click the next frame button when the current frame is finished rendering.\nIt will also turn off low resolution previews and render each frame it doesn't have memorized yet at full resolution immediately.");
		SetupControl(buildButton, "'PLOT' - clicking it will start rendering the current frame.\n'OK' - the current frame is up to date, and clicking will do nothing.\n'CANCEL' - frame is being rendered, click to cancel the task.");
		
        SetupControl(lockResButton, "Locks the resolution values, so they will no longer automatically change when you resize the window.");
        SetupControl(widthBox, "The horizontal size (width) of the images." + resize);
        SetupControl(heightBox, "The vertical size (height) of the images." + resize);

        SetupControl(tRangeButton, "Locks the time range. When you change the animation length, then this range will stay the same over the whole animation.");
        SetupControl(itcButton, pinS + pinT + ce);
        SetupControl(itsBox, valT + "beginning of the animation, exactly at the frame #0");
        SetupControl(itsButton, pinC + pinT + se);
        SetupControl(itcBox, valT + "middle of the animation, at the frame #(Length/2)");
        SetupControl(iteButton, pinE + pinT + sc);
        SetupControl(iteBox, valT + "end of the animation, at the frame #Length. If you want a seamless loop, the this value should render the same picture as the TStart value.");

        SetupControl(modeSelect, "Area X - 1D X->Y plot, filling the areas below the curve.\nLine X -  1D X->Y plot, drawing the curve itself as a line.\nRGB XY - a 2D color plot. The input will be a sum of samples from Input X and Input Y ranges corresponding to the pixel. Top left pixel will be XStart+YStart, and the pixel below and to the right of the bottom right would be XEnd+YEnd");

        SetupControl(xRangeButton, "Locks the Input X range (for both 1D and 2D modes)\nWhen you resize the width, then this range will stay the same, stretching the image on this axis.");
        SetupControl(ixcButton, pinS + pinX + ce + noStretch + "left side with stay pinned.");
        SetupControl(ixsBox, valX + "left side" + pos + "0");
        SetupControl(ixsButton, pinC + pinX + se + noStretch + "center with stay pinned.");
        SetupControl(ixcBox, valX + "middle" + pos + "(Width/2)");
        SetupControl(ixeButton, pinE + pinX + sc + noStretch + "right side will stay pinned");
        SetupControl(ixeBox, valX + "right side" + pos + "#Width");

        SetupControl(yRangeButton, "Locks the 2D mode Input Y range (for the XY RGB mode)" + resizeH);
        SetupControl(iycButton, pinYs);
        SetupControl(iysBox, valY + "top side" + pos + "0");
        SetupControl(iysButton, pinYc);
        SetupControl(iycBox, valY + "middle" + pos + "(Height/2)");
        SetupControl(iyeButton, pinYe);
        SetupControl(iyeBox, valY + "bottom side" + pos + "#Height");

        SetupControl(oyRangeButton, "Locks the 1D mode Output Y range (for the Area X and Line X modes)" + resizeH);
        SetupControl(oycButton, pinYs);
        SetupControl(oysBox, valOy + "top side" + pos + "0");
        SetupControl(oysButton, pinYc);
        SetupControl(oycBox, valOy + "middle" + pos + "(Height/2)");
        SetupControl(oyeButton, pinYe);
        SetupControl(oyeBox, valOy + "bottom side" + pos + "#Height");

        SetupControl(fysButton, setF + "the YStart");
        SetupControl(fycButton, setF + "the YCenter");
        SetupControl(fyeButton, setF + "the YEnd");
        SetupControl(fyeButton, setF + "the sample of the Input Y axis at this height pixel slice.\nIf you type the image Height here, it will sample Input Y End.");

        SetupControl(clipSelect, "Choose how to treat out of bounds values from the evaluation expression, before it is sent into the Rgb expression.\nClamp - clamps between 0-1.\nLoop - loops the value to the 0-1 range. 1.2 will become 0.2, and -0.2 will become 0.8.\nOverflow - No treatment at all, will let the Rgb expression handle the original value.");
        SetupControl(delButton, "Remove the currently selected output. ");
        SetupControl(delButton, "First, write your desired output name to the combo box on the right.\nThe click this Add Output button to add a output with that name to the list.");
        SetupControl(outputSelect, "Select which output in the series do you want to edit.\nThis will switch the codeBoxes below and the clipSelect on the left, to that output.");
        SetupControl(rgbBox, "This is the Rgb expression.\nThe 'shader' that takes the result value from the evaluation expression below, and should process it into an RGB color.\nYou also have a lot of input values available here, such as:\n"
            + "v = the output value from the evaluated expression below.\n"
            + "c = the RGB input value of the pixel.\nBegins at 0,0,0 for the first output layer, and then equal the previous layer's RGB output.\n"
            + zt
            + "x = the screen space X pixel position.\n"
            + "x = the screen space Y pixel position.\n"
            + "w = the screen space horizontal size - the width of the image.\n"
            + "h = the screen space vertical size - the height of the image.\n"
            + "f = the animation frame index. Not sampled Time Axis, it is the value in the textbox between the previous and next frame buttons in the animation row.\n"
            + "l = the animation length, the same value as the leftmost textbox in the animation row.\n");
        SetupControl(rgbBox, "This is the Evaluation expression.\nThe complicated math should be here, as this will also use memory to transfer previously evaluated pixels if you move the viewport.\nYou only have the evaluation input values available:\n" + zt);
    }
	public void Block() { SetBlock(true); buildButton.Text = "CANCEL"; }
	public void Unblock() { SetBlock(false); buildButton.Text = "OK"; }
	private void SetBlock(bool b) {
		//animatedBox.Enabled = prevButton.Enabled = nextButton.Enabled = loadButton.Enabled = saveButton.Enabled = !b;
		animatedBox.Visible = 
		itfBox.Visible = itlBox.Visible =
			prevButton.Visible = nextButton.Visible = loadButton.Visible = saveButton.Visible = saveSelect.Visible = IsVisible = !(exportLabel.Visible = b);
		SetBlock(panel, /*itfBox.ReadOnly = itlBox.ReadOnly =*/ b);
	}
	private void SetBlock(object c, bool r) {
		if (c is not Control cr)
			return;
		cr.Visible = !r;//cr.Tag = r;
		foreach (var ch in cr.Controls)
			SetBlock(ch, r);
	}
	
	private void SetupControl(Control control, string tip) {
    	// Add tooltip and set the next tabIndex
    	//myControls.Add(control, tip);
    	toolTips.SetToolTip(control, tip/*L(tip)*/); // TODO add localization support L(key)
    	control.TabIndex = ++_controlTabIndex;
    }
}
