using Comparser.Forms;
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
        ParentForm m = new() { ActuallyClose = true };
        m.StartPosition = FormStartPosition.CenterScreen;
        _ = new MenuControl(null, m);
        Application.Run(m);
    }
}
