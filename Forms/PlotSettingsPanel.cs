using Comparser.Forms.Controls;
using Comparser.Forms.Core;
using static Comparser.Forms.Core.IPanel;

namespace Comparser.Forms;
public partial class PlotSettingsPanel : UserControl, IPanel {
	#region IPanel
	private readonly ControlVar _var;
	public ControlVar GetVar() => _var;
	public Size GetSize() => new(320, 64);
	public void SetDark(bool dark) => BaseSetDark(this);
	public void PerformClose() { }
	public void CoreLayout() { } // TODO hide fixedY when XY mode
	#endregion

	#region Inits
	public PlotSettingsPanel() { 
		InitializeComponent(); 
		Controls.Add(S = new() { Dock = DockStyle.Fill, Location = new Point(0, 0), Size = Size });
	}
	public PlotSettingsPanel(MenuPanel root, ParentForm parent) : this() => InitVar(ref _var, this, root, parent, "Comparser - Plot Settings");
	#endregion

	public readonly PlotSettingsControl S;
}