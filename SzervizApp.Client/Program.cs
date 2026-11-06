using System.Drawing;
using System.Windows.Forms;

namespace SzervizApp.Client;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        using var login = new Forms.LoginForm();
        var result = login.ShowDialog();
        var client = login.Client;
        var user = login.User;
        if (result != DialogResult.OK || client is null || user is null)
            return;

        Application.Run(new Forms.MainForm(client, user));
    }
}
