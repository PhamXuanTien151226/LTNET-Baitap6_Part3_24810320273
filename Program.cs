using System;
using System.Windows.Forms;

namespace Lab6Part3App
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();
            Application.Run(new GUI.frmMainApp());
        }
    }
}