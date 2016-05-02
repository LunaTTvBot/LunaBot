using System;
using System.Diagnostics;

namespace IBot
{
    public class Program
    {
        public static void Main()
        {
            try
            {
                var app = new App();
                app.StartApp();
            }
            catch (Exception e)
            {
                Trace.WriteLine(e.Message);
            }
        }
    }
}