using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ReadyBE.Serial
{
    class SerialJSON
    {
        public bool Valid { get; set; }
        public int ID { get; set; }
        public IDictionary dictionary;
        public SerialJSON(string jsonString)
        {
            Valid = false;
            try
            {
                dictionary = JsonConvert.DeserializeObject<IDictionary>(jsonString);
                if(dictionary != null)
                {
                    foreach (DictionaryEntry entry in dictionary)
                    {
                        //Console.WriteLine(entry.Key + " : " + entry.Value);
                        if (entry.Key.ToString() == "ID")
                        {
                            try
                            {
                                ID = int.Parse(entry.Value.ToString());
                                Valid = true;
                            }
                            catch
                            {
                                if (!SerialCommunication.failedJSON.Contains(jsonString))
                                {
                                    SerialCommunication.failedJSON.Add(jsonString);
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }
        }

    }
}
