using ReadyBE.AutomaticLight;
using ReadyBE.Mobo;
using ReadyBE.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using Microsoft.Extensions.Hosting;
using ReadyBE.AutoRefillSystem;
using System.Net;

namespace ReadyBE
{
    class Ready
    {

        public static string[] Ports { get; set; }
        public static void ApplicationRun()
        {
            try
            {
                
                string input = "";
                //Console.WriteLine("Available Ports:");
                string[] ports = SerialCommunication.AvailablePorts();
                for (int i = 0; i < ports.Length; i++)
                {
                    Console.WriteLine("[" + i + "] - " + ports[i]);
                }
                Ports = ports;
                /*
                Console.WriteLine("Please choose a port (with number)");
                int index = int.Parse(Console.ReadLine());
                Console.WriteLine("Please choose the serial period(in Millis)");
                int serialPeriod = int.Parse(Console.ReadLine());
                Console.WriteLine("Please set the Wake Time: (for example: 06:15)");
                string wakeTime = Console.ReadLine();
                Console.WriteLine("Please set the Wake Time: (for example: 23:42)");
                string sleepTime = Console.ReadLine();
                */

                
                SerialHandler.devices.Add(new TempDS("TEMP_LIGHT_HOT"));
                SerialHandler.devices.Add(new TempDS("TEMP_LIGHT_COLD"));
                SerialHandler.devices.Add(new TempDS("TEMP_NUTSOL"));
                SerialHandler.devices.Add(new TempDS("TEMP_CALORIFER_MID"));
                SerialHandler.devices.Add(new TempBME("TEMP_R1"));
                SerialHandler.devices.Add(new TempBME("TEMP_R2"));
                SerialHandler.devices.Add(new TempBME("TEMP_F1"));
                SerialHandler.devices.Add(new TempBME("TEMP_F2"));
                
                //SerialHandler.devices.Add(new Pump("PERIPUMP_1"));
                //SerialHandler.devices.Add(new Pump("PERIPUMP_2"));
                //SerialHandler.devices.Add(new Pump("PERIPUMP_3"));
                //SerialHandler.devices.Add(new Pump("PERIPUMP_4"));
                
                SerialHandler.devices.Add(new DevicePWM("PUMP_A", 2, 54));
                SerialHandler.devices.Add(new DevicePWM("PUMP_B", 2, 55));
                SerialHandler.devices.Add(new TankFloat("TANK_FLOAT"));
                SerialHandler.devices.Add(new InletValve("INLET_VALVE", 4, 58));
                SerialHandler.devices.Add(new DeviceDC("LIGHT_AC", 5, 50));
                
                //SerialHandler.devices.Add(new Relay("RELAY_4", 6,51));
                //SerialHandler.devices.Add(new Relay("RELAY_3", 7,52));
                //SerialHandler.devices.Add(new Relay("RELAY_2", 8,53));

                
                SerialHandler.devices.Add(new DevicePWM("LIGHT_COOL_PUMP", 2, 59));
                SerialHandler.devices.Add(new DevicePWM("CALORIFER_FAN", 10, 57));
                SerialHandler.devices.Add(new DeviceDC("RECIRC_FAN", 11, 56));
                
                //SerialHandler._moboDevices.Last().WriteFields["PWM"] = 1;
                
                SerialHandler.WriteField("RECIRC_FAN", "PWM", 1);
                SerialHandler.WriteField("PUMP_A", "PWM", 255);
                SerialHandler.WriteField("PUMP_B", "PWM", 255);
                
                /*
                Console.WriteLine("Serial debug?");
                Console.WriteLine("[0] false");
                Console.WriteLine("[1] true");
                int serDeb = int.Parse(Console.ReadLine());
                bool serDebB = false;
                if(serDeb == 1)
                {
                    serDebB = true;
                }
                Console.WriteLine("Auto refill debug?");
                Console.WriteLine("[0] false");
                Console.WriteLine("[1] true");
                int arDeb = int.Parse(Console.ReadLine());
                bool arDebB = false;
                if (arDeb == 1)
                {
                    arDebB = true;
                }
                Console.WriteLine("Modbus debug?");
                Console.WriteLine("[0] false");
                Console.WriteLine("[1] true");
                int modDeb = int.Parse(Console.ReadLine());
                bool modDebB = false;
                if (modDeb == 1)
                {
                    modDebB = true;
                }
                Console.WriteLine("Light handler debug?");
                Console.WriteLine("[0] false");
                Console.WriteLine("[1] true");
                int liDeb = int.Parse(Console.ReadLine());
                bool liDebB = false;
                if (liDeb == 1)
                {
                    liDebB = true;
                }
                */

                
                //Server.Connect("127.0.0.1", 6500);
                
                Task.Run(() =>
                {
                    SerialHandler serialHandler = new SerialHandler();
                    serialHandler.Start(ports[0], 100, false);
                });
                /*
                Task.Run(() =>
                {
                    List<string> devices = new List<string>();
                    devices.Add("LIGHT_AC");
                    LightHandler.Init(devices, 2000, "16:10", "23:10", false);
                });
                Task.Run(() =>
                {
                    ModbusHandler.Connect(ports[0], 3000, false);
                });

                Task.Run(() =>
                {
                    RefillHandler.Enable(5000, false);
                });
                */
                while (true)
                {
                    try
                    {
                        //Process currentProcess = Process.GetCurrentProcess();
                        int numberOfThreads = Process.GetCurrentProcess().Threads.Count;
                        double MemoryUsed = GetMemoryUsage();
                        double cpu = GetCpuUsageForProcess();
                        Console.WriteLine("The program Runs:" + numberOfThreads + " Threads, and the CPU Usage is: " + cpu.ToString("0.##") + "%, and the total Memory Used: " + MemoryUsed + "MB!");
                        Dictionary<string, object> DiagnosticFields = new Dictionary<string, object>();

                        //ResourcesToInflux(numberOfThreads, cpu, MemoryUsed);

                        Thread.Sleep(3000);
                    }
                    catch
                    {
                        Console.WriteLine("Hearthbeat...");
                        Thread.Sleep(3000);
                    }
                }
                //Console.ReadLine();
            }
            catch (Exception ex)
            {
                File.WriteAllText(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) + "\\error.txt", ex.Message);
            }
        }
        private static void ResourcesToInflux(int threads, double cpu, double MemoryUsed)
        {
            Dictionary<string, object> DiagnosticFields = new Dictionary<string, object>();
            DiagnosticFields.Add("CPU", cpu);
            DiagnosticFields.Add("MEMORY", MemoryUsed);
            DiagnosticFields.Add("THREADS", threads);
            InfluxDB.Write("PC_RESURCE", DiagnosticFields);
        }
        private static double GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            Thread.Sleep(500);

            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }
        private static double GetMemoryUsage()
        {
            Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
            double result = currentProcess.WorkingSet64 / (1024 * 1024);
            return result;
        }
        /*
        public static object ReadField(string nameOfTheDevice, string nameOfTheField)
        {
            object result = null;
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Name == nameOfTheDevice)
                {
                    result = devices[i].Fields[nameOfTheField];
                }
            }
            return result;
        }
        /// <summary>
        /// This method modify the value of the selected device according its name and field 
        /// </summary>
        /// <param name="nameOfTheDevice">The name of the device for example: TANK_FLOAT, INLET_VALVE etc.</param>
        /// <param name="nameOfTheField">The name of the field for example: TEMP, PWM, CURRENT etc.</param>
        /// <param name="value">The value of the selected field</param>
        public static void WriteField(string nameOfTheDevice, string nameOfTheField, object value)
        {
            for (int i = 0; i < devices.Count; i++)
            {
                if (devices[i].Name == nameOfTheDevice)
                {
                    devices[i].WriteFields[nameOfTheField] = value;
                }
            }
        }
        */
    }
}
