using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.IO.Ports;
using ReadyBE.Serial;

namespace ReadyBE
{
    class SerialCommunication
    {
        static bool _continue;
        static SerialPort _serialPort;
        static public List<SerialJSON> JSONlist = new List<SerialJSON>();
        public static List<string> failedJSON = new List<string>();
        public static bool Debug { get; set; }

        public static string[] AvailablePorts()
        {
            string[] ports = SerialPort.GetPortNames();
            return ports;
        }
        public static void Connect(string port, bool debug = true)
        {
            Debug = debug;
            //StringComparer stringComparer = StringComparer.OrdinalIgnoreCase;
            Thread readThread = new Thread(Read);

            // Create a new SerialPort object with default settings.  
            _serialPort = new SerialPort();

            // Allow the user to set the appropriate properties.
            _serialPort.PortName = port;
            _serialPort.BaudRate = 500000;
            _serialPort.Parity = (Parity)Enum.Parse(typeof(Parity), "None");
            _serialPort.DataBits = 8;
            _serialPort.StopBits = (StopBits)Enum.Parse(typeof(StopBits), "One");
            _serialPort.Handshake = (Handshake)Enum.Parse(typeof(Handshake), "None");
            // Set the read/write timeouts  
            _serialPort.ReadTimeout = 500;
            _serialPort.WriteTimeout = 500;

            _serialPort.Open();

            readThread.Start();

            _continue = true;
        }
        public static void Write(string message)
        {
            _serialPort.Write(message + "\n");
            DebugMessage.Write("*******************************************************************************", Debug);
            DebugMessage.Write("Raspberry - Arduino : " + message, Debug);
            JSONlist.Clear();
        }
        public static void Disconnect()
        {
            _continue = false;
            _serialPort.Close();
        }
        public static string LastReceived { get; set; }
        public static void Read()
        {
            while (_continue)
            {
                try
                {
                    string message = _serialPort.ReadLine();
                    LastReceived = message;
                    DebugMessage.Write("Arduino - Raspberry : " + message, Debug);
                    JSONlist.Add(new SerialJSON(message));
                    DebugMessage.Write($"JSON hozzáadva a listához, {JSONlist.Count} db elem van a listában.", Debug);
                }
                catch (TimeoutException) { }
            }
        }
    }
}
