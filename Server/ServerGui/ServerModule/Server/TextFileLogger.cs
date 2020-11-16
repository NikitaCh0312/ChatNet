using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ServerModule.Logger
{
    /// <summary>
    /// класс является синглтоном
    /// пришлось так сделать чтобы был доступ к классу из разных модулей
    /// </summary>
    public class TextFileLogger : ILogger
    {
        private TextFileLogger()
        {
            //получить директорию
            logFileDirectory = Directory.GetCurrentDirectory() + "\\log.txt";

        }
        private static TextFileLogger _instance;
        public static TextFileLogger GetInstance()
        {
            if (_instance == null)
                _instance = new TextFileLogger();
            return _instance;
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        private string logFileDirectory;
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public void writeLineLogMessage(Message msg)
        {
            writeLogMessage(msg);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public async void writeLineLogMessageAsync(Message msg)
        {
            await Task.Run(() => writeLogMessage(msg));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        private void writeLogMessage(Message message)
        {
            //записать лог
            using (StreamWriter sw = new StreamWriter(logFileDirectory, true))
            {
                string log = DateTime.Now.ToString() + $" From: {message.SourceLogin} Message: {message.MessageData}";
                sw.WriteLine(log);
            }
        }
    }
}
