using System;
using System.Collections.Generic;
using System.Text;

namespace ReadyBE
{
    class DebugMessage
    {
        /// <summary>
        /// This Method writes the debug message into the console, if debug mode activated
        /// </summary>
        /// <param name="message"></param>
        /// <param name="debug"></param>
        public static void Write(string message, bool debug)
        {
            if (debug)
            {
                Console.WriteLine(message);
            }

        }
    }
}
