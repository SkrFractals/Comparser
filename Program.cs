using Comparser.Forms;
using Comparser.Forms.Core;
namespace Comparser;
internal static class Program {
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main() {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
		ParentForm m = new() {
			ActuallyClose = true, StartPosition = FormStartPosition.CenterScreen
		};
		_ = new MenuPanel(m);
        Application.Run(m);
    }
}
