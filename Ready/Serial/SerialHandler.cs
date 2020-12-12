using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Collections;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;
using System.Threading;

namespace ReadyBE
{
    class SerialHandler
    {
        public static bool _run { get; set; }
        /// <summary>
        /// Ez az ID mondja meg, hogy jelenleg a stackben melyik eszközöket szólítjuk
        /// </summary>
        public static int ID { get; set; }
        public static int TimeOut { get; set; } = 1000;
        private static int Counter { get; set; }
        private static int SerialPeriod { get; set; } //How often should i call the serial check
        private static bool waitForAnswer { get; set; } = false;
        public static List<IMoboDevice> devices = new List<IMoboDevice>();
        public static bool Debug { get; set; }

        //private static System.Threading.Timer timer;
        public static bool SerialCommunicationIsReady { get; set; }
        public void Start(string port, int serialPeriod, bool debug = false)
        {
            SerialPeriod = serialPeriod;
            _run = true;
            Debug = debug;
            try
            {
                SerialCommunication.Connect(port, debug); //************************************
                DebugMessage.Write("Soros kommunikáció létrejött!", Debug);
            }
            catch
            {
                DebugMessage.Write("Soros kmmunikációs hiba!", Debug);
            }
            //timer = new System.Threading.Timer(new System.Threading.TimerCallback(Timer_Elapsed), null, SerialPeriod, SerialPeriod);
            
            while (_run)
            {
                for (int i = 0; i < 100; i++)
                {
                    ID = i;
                    waitForAnswer = GetData(i); //await
                    if (waitForAnswer)
                    {
                        DebugMessage.Write("Várakozás válaszra az ID" + ID + "-n!", Debug);
                        //timer.Change(SerialPeriod, SerialPeriod); //Ez indítja el a Timer-t

                        Task.Run(PeridicCheck);

                        DebugMessage.Write("Timer started!", Debug);
                        while (waitForAnswer)
                        {
                            DebugMessage.Write("Wait for answer...", Debug);
                            Thread.Sleep(SerialPeriod);
                        }
                    }
                }
                SerialCommunicationIsReady = true;
            }
        }
        /// <summary>
        /// This method gives you back the value of the selected device according its name and field
        /// </summary>
        /// <param name="nameOfTheDevice">The name of the device for example: TANK_FLOAT, INLET_VALVE etc.</param>
        /// <param name="nameOfTheField">The name of the field for example: TEMP, PWM, CURRENT etc.</param>
        /// <returns>The value of the selected field</returns>
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
                if(devices[i].Name == nameOfTheDevice)
                {
                    devices[i].WriteFields[nameOfTheField] = value;
                }
            }
        }
        private void PeridicCheck()
        {
            while(true)
            {
                DebugMessage.Write("TimerElapsed", Debug);
                if (Counter < TimeOut / SerialPeriod)
                {
                    if (SerialCommunication.JSONlist.Count > 0)
                    {
                        if (SerialCommunication.JSONlist[0].Valid && SerialCommunication.JSONlist[0].ID == ID)
                        {
                            try
                            {
                                DebugMessage.Write("Timer megtalálta a JSON-t.", Debug);
                                //timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); //Ez állítja meg a timert
                                SetData();
                                SerialCommunication.JSONlist.Clear();
                                Counter = 0;
                                waitForAnswer = false;
                                
                                break;
                            }
                            catch
                            {
                                DebugMessage.Write("JSON list hiba!", Debug);
                            }
                        }
                        else
                        {
                            DebugMessage.Write("Nem megfelelő válasz érkezett!", Debug);
                            SerialCommunication.JSONlist.Clear();
                        }
                    }
                }
                else
                {
                    //timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                    waitForAnswer = false;
                    Counter = 0;
                }
                Counter++;
                Thread.Sleep(SerialPeriod);
            }
        }
        private static void Timer_Elapsed(object state) //, ElapsedEventArgs e
        {
            DebugMessage.Write("TimerElapsed", Debug);
            if (Counter < TimeOut / SerialPeriod)
            {
                if(SerialCommunication.JSONlist.Count > 0)
                {
                    if(SerialCommunication.JSONlist[0].Valid && SerialCommunication.JSONlist[0].ID == ID)
                    {
                        try
                        {
                            DebugMessage.Write("Timer megtalálta a JSON-t.", Debug);
                            //timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); //Ez állítja meg a timert
                            SetData();
                            SerialCommunication.JSONlist.Clear();
                            Counter = 0;
                            waitForAnswer = false;
                        }
                        catch
                        {
                            DebugMessage.Write("JSON list hiba!", Debug);
                        }
                    }
                    else
                    {
                        DebugMessage.Write("Nem megfelelő válasz érkezett!", Debug);
                        SerialCommunication.JSONlist.Clear();
                    }
                }
            }
            else
            {
                //timer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
                waitForAnswer = false;
                Counter = 0;
            }
            Counter++;
        }

        public static void Stop()
        {
            _run = false;
        }
        public static bool GetData(int ID) //public static async Task<bool> GetData(int ID)
        {
            switch (ID)
            {
                case 1:
                    SerialCommunication.Write("{\"ID\":1,\"TAG\":\"TEMP_DS\"}");
                    return true;
                case 2:
                    SerialCommunication.Write("{\"ID\":2,\"TAG\":\"PWM\"}");
                    return true;
                case 3:
                    SerialCommunication.Write("{\"ID\": 3, \"DEVICE\": \"TANK_FLOAT\", \"INTERFACE\": \"STATUS\", \"ACTION\": \"GET\"}");
                    return true;
                case 4:
                    SerialCommunication.Write("{\"ID\": 4, \"DEVICE\": \"INLET_VALVE\", \"INTERFACE\": \"STATUS\", \"ACTION\": \"GET\"}");
                    return true;
                case 5:
                    SerialCommunication.Write("{\"ID\": 5, \"DEVICE\": \"LIGHT_AC\", \"INTERFACE\": \"POWER\", \"ACTION\": \"GET\"}");
                    return true;
                    
                case 6:
                    SerialCommunication.Write("{\"ID\":6,\"TAG\":\"TEMP_BME\"}");
                    return true;
                    /*
                case 7:
                    SerialCommunication.Write("{\"ID\": 7, \"DEVICE\": \"RELAY_3\", \"INTERFACE\": \"POWER\", \"ACTION\": \"GET\"}");
                    return true;
                case 8:
                    SerialCommunication.Write("{\"ID\": 8, \"DEVICE\": \"RELAY_2\", \"INTERFACE\": \"POWER\", \"ACTION\": \"GET\"}");
                    return true;
                case 9:
                    SerialCommunication.Write("{\"ID\": 9, \"DEVICE\": \"LIGHT_COOL_PUMP\", \"INTERFACE\": \"POWER\", \"ACTION\": \"GET\"}");
                    return true;*/
                case 10:
                    SerialCommunication.Write("{\"ID\": 10, \"DEVICE\": \"CALORIFER_FAN\", \"INTERFACE\": \"POWER\", \"ACTION\": \"GET\"}");
                    return true;
                case 11:
                    SerialCommunication.Write("{\"ID\": 11, \"DEVICE\": \"RECIRC_FAN\", \"INTERFACE\": \"POWER\", \"ACTION\": \"GET\"}");
                    return true;
                case 50:
                    SerialWriteCommand(50,"LIGHT_AC","POWER","STATE");
                    return true;
                    /*
                case 51:
                    SerialWriteCommand(51, "RELAY_4", "POWER", "State");
                    return true;
                case 52:
                    SerialWriteCommand(52, "RELAY_3", "POWER", "State");
                    return true;
                case 53:
                    SerialWriteCommand(53, "RELAY_2", "POWER", "State");
                    return true;*/
                case 54:
                    SerialWriteCommand(54, "PUMP_A", "DUTY_CYCLE", "PWM");
                    return true;
                case 55:
                    SerialWriteCommand(55, "PUMP_B", "DUTY_CYCLE", "PWM");
                    return true;
                case 56:
                    SerialWriteCommand(56, "RECIRC_FAN", "POWER", "STATE");
                    return true;
                case 57:
                    SerialWriteCommand(57, "CALORIFER_FAN", "DUTY_CYCLE", "PWM");
                    return true;
                case 58:
                    SerialWriteCommand(58, "INLET_VALVE", "DUTY_CYCLE", "STATE"); //EZ STATE LEGYEN
                    return true;
                case 59:
                    SerialWriteCommand(59, "LIGHT_COOL_PUMP", "DUTY_CYCLE", "PWM");
                    return true;
                default:
                    return false;
            }
        }
        private static void SerialWriteCommand(int _ID, string _device, string _interface, string _fieldNameOfValue)
        {
            for (int i = 0; i < SerialHandler.devices.Count; i++)
            {
                if (SerialHandler.devices[i].Name == _device)
                {
                    try
                    {
                        if(SerialHandler.devices[i].Fields[_fieldNameOfValue] != null || SerialHandler.devices[i].WriteFields[_fieldNameOfValue] != null) //Küldje a write-ot ha a nem write null
                        {
                            string value = "";
                            if (SerialHandler.devices[i].WriteFields[_fieldNameOfValue] != null)
                            {
                                value = SerialHandler.devices[i].WriteFields[_fieldNameOfValue].ToString();
                            }
                            else
                            {
                                value = SerialHandler.devices[i].Fields[_fieldNameOfValue].ToString();
                            }
                            double dValue = 0;
                            bool isNumeric = double.TryParse(value, out dValue);
                            if(!isNumeric)
                            {
                                value = "\"" + value + "\"";
                            }
                            SerialCommunication.Write("{\"ID\": " + _ID + ", \"DEVICE\": \"" + _device + "\", \"INTERFACE\": \"" + _interface + "\", \"ACTION\": \"SET\", \"VALUE\": " + value + "}");
                            break;
                        }
                        else
                        {
                            DebugMessage.Write($"Az alábbi Field({_fieldNameOfValue}) értéke null!", Debug);
                        }
                    }
                    catch (Exception e)
                    {
                        DebugMessage.Write(e.Message, Debug);
                    }
                }
            }
        }
        public static void SetData()
        {
            DebugMessage.Write("Adatok objektumba illesztése elkezdődik.", Debug);
            foreach (IMoboDevice device in devices)
            {
                if (device.ID == ID)
                {
                    try
                    {
                        device.SetData();
                    }
                    catch
                    {

                    }
                    DebugMessage.Write($" A {device.Name} nevű eszköz adatai beállítva.", Debug);
                }
                else if(device.RecID == ID)
                {
                    device.WriteResponseCheck();
                }
            }
        }
        public static string ValueOfGivenKeys(string objectName, string property)
        {
            string result = "";
            foreach (DictionaryEntry entry in SerialCommunication.JSONlist[0].dictionary)
            {
                if (entry.Key.ToString() == objectName)
                {
                    var dictionary = JsonConvert.DeserializeObject<IDictionary>(entry.Value.ToString());
                    foreach (DictionaryEntry entryInner in dictionary)
                    {
                        if (entryInner.Key.ToString() == property && entryInner.Value != null)
                        {
                            try
                            {
                                result = entryInner.Value.ToString();
                                DebugMessage.Write($"{property} of {objectName} has changed to - {entryInner.Value.ToString()}", Debug);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
            }
            return result;
        }
        public static bool ValidateWriteResult()
        {
            string response = "";
            if (SerialCommunication.JSONlist[0].dictionary["RESPONSE"] != null)
            {
                response = SerialCommunication.JSONlist[0].dictionary["RESPONSE"].ToString();
            }
            if (SerialCommunication.JSONlist[0].dictionary["RESULT"] != null)
            {
                response = SerialCommunication.JSONlist[0].dictionary["RESULT"].ToString();
            }
            if (response == "OK")
            {
                DebugMessage.Write("Successful serial writing!", Debug);
                return true;
            }
            else
            {
                DebugMessage.Write("Unsuccessful serial writing!", Debug);
                return false;
            }
        }
    }
}
