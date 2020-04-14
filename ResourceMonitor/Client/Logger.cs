using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Client
{
    class Logger
    {
        private string logName;

        public Logger(string logName)
        {
            this.logName = logName;
            try
            {
                File.AppendAllText(logName, "INICIO DO LOG - " + DateTime.Now.ToString() + Environment.NewLine);
            }
            catch
            {

            }
        }

        public void Log(string logMessage)
        {
            try
            {
                File.AppendAllText(logName, "[" + DateTime.Now.ToString() + "]" + logMessage + Environment.NewLine);
            }
            catch
            {

            }
        }
    }
}

/*
    class Logger
    {
        private string logName;
        private StreamWriter logWriter;

        public Logger(string logName)
        {
            this.logName = logName;
            this.logWriter = File.AppendText(this.logName);

            this.logWriter.WriteLine("INICIANDO LOG - " + DateTime.Now.ToString());
        }

        public void Log(string logMessage)
        {
            this.logWriter.WriteLine("[" + DateTime.Now.ToString() + "]" + logMessage + Environment.NewLine);
        }
    }
 
 */
