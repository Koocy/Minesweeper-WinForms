using System;

namespace Minesweeper
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>

        public static GameForm gameForm;

        [STAThread]
        static void Main()
        {

            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);
            gameForm = new GameForm();
            System.Windows.Forms.Application.Run(gameForm);
        }
    }
}
