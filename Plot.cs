
using Comparser;

namespace Expressions;
public partial class Plot : Form {
	private readonly ComparserForm m;

	private Comparser.Numbers.Complex TL, TR, BL, BR, E;

	public Plot(ComparserForm main) {
		InitializeComponent();
		m = main;
	}

	private void expBox_TextChanged(object sender, EventArgs e) {

	}

	private void tlBox_TextChanged(object sender, EventArgs e) {

	}

	private void trBox_TextChanged(object sender, EventArgs e) {

	}

	private void blBox_TextChanged(object sender, EventArgs e) {

	}

	private void brBox_TextChanged(object sender, EventArgs e) {

	}
}
