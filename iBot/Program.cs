using System;
using System.Diagnostics;

namespace IBot
{
    public class Program
    {
        private static App _app;

        public static void Main()
        {
            try
            {
                _app = new App();
                _app.StartApp();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }

        public static void Shutdown()
        {
            Console.WriteLine(@"Bye");
            _app.StopApp();
        }
    }
}