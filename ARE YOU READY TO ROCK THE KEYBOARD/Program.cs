namespace ARE_YOU_READY_TO_ROCK_THE_KEYBOARD
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Initialize the application configuration
            ApplicationConfiguration.Initialize();

            // Run Form1 correctly
            Application.Run(new Form1());
        }
    }
}
