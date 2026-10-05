using System;
using System.Windows.Forms;
using ComputerClubWinForms.Data;
using ComputerClubWinForms.Forms;

namespace ComputerClubWinForms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();

        var database = new DatabaseHelper();
        database.Initialize();

        Application.Run(new AuthPage(database));
    }
}
