using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Server
{
    class Logger
    {
        private string logName;
        private StreamWriter logWriter;

        public Logger(string logName)
        {
            this.logName = logName;

            try
            {
                this.logWriter = File.AppendText(this.logName);
                this.logWriter.WriteLine("INICIANDO LOG - " + DateTime.Now.ToString());
                //File.AppendAllText(logName, "INICIO DO LOG - " + DateTime.Now.ToString() + Environment.NewLine);
            }
            catch
            {
                Console.WriteLine("Erro ao criar arquivo de log");
            }
        }

        public void Log(string logMessage)
        {
            try
            {
                this.logWriter.WriteLine("[" + DateTime.Now.ToString() + "]" + logMessage + Environment.NewLine);
                //File.AppendAllText(logName, "[" + DateTime.Now.ToString() + "]" + logMessage + Environment.NewLine);
            }
            catch
            {
                Console.WriteLine("hello1");
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
