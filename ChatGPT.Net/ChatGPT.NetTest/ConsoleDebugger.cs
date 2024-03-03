using ChatGPT.Net;

namespace ChatGPT.NetTest
{
    public class ConsoleDebugger : IGptDebug
    {
        public void Debug(string message)
        {
            Console.WriteLine(message);
        }

        public void Debug(string message, byte r, byte g, byte b)
        {
            Console.ForegroundColor = ClosestConsoleColor(r, g, b);
            Console.WriteLine(message);
            Console.ResetColor();
        }


        /// <summary>
        /// Glenn Slayden - Sep 9, 2012 
        /// <para>- <see href="https://stackoverflow.com/questions/1988833/converting-color-to-consolecolor"/></para>
        /// </summary>
        private static ConsoleColor ClosestConsoleColor(byte r, byte g, byte b)
        {
            ConsoleColor ret = 0;
            double rr = r, gg = g, bb = b, delta = double.MaxValue;

            foreach (ConsoleColor cc in Enum.GetValues(typeof(ConsoleColor)))
            {
                var n = Enum.GetName(typeof(ConsoleColor), cc);
                var c = System.Drawing.Color.FromName(n == "DarkYellow" ? "Orange" : n); // bug fix
                var t = Math.Pow(c.R - rr, 2.0) + Math.Pow(c.G - gg, 2.0) + Math.Pow(c.B - bb, 2.0);
                if (t == 0.0)
                    return cc;
                if (t < delta)
                {
                    delta = t;
                    ret = cc;
                }
            }
            return ret;
        }
    }
}
