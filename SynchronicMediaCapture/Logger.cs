using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace SynchronicMediaCapture
{
    public static class Logger
    {
        //Log Level Strings
        public static string ERROR =    "[Error]  ";
        public static string WARNING =  "[Warning]";
        public static string DEBUG =    "[Debug]  ";

        public static string pathString;
        public static string NameOfMethod { get; set; }
        static Boolean title = false;
        private static object lockObject = new object();

        #region Constructor
        public static void StartLogger(string path)
        {
            var logDir = Path.GetDirectoryName(path);
            if (Directory.Exists(logDir) == false)
                Directory.CreateDirectory(logDir);
            pathString = path;
        }
        #endregion

        #region Public Methods
        public static void WriteTimeStamp()
        {
            Write(string.Format("[{0}]\t", DateTime.Now.ToString("HH:mm:ss.ff")));
        }
        public static void Write(string message)
        {
            lock (lockObject)
            {
                /*The writing to the file is in Append mode*/
                using (FileStream fs = new FileStream(pathString, FileMode.Append))
                {
                    /*New StreamWriter to write to the folder*/
                    StreamWriter writer = new StreamWriter(fs);

                    /*checks if the file excist*/
                    if (!File.Exists(pathString))
                    {
                        /*Create new file*/
                        File.Create(pathString);

                        ///*Checks if the title of the method writes to the file*/
                        //if (title == false)
                        //{
                        //    /*Writes a line to the file*/
                        //    writer.WriteLine(NameOfMethod);
                        //    title = true;
                        //}

                        /*Writhe the data to the file*/
                        writer.Write(message);
                    }

                    else if (File.Exists(pathString))
                    {
                        //if (title == false)
                        //{
                        //    writer.WriteLine(NameOfMethod);
                        //    title = true;
                        //}
                        writer.Write(message);
                    }
                    /*Close the streamWriter*/
                    writer.Close();
                    /*Release all the resorces*/
                    writer.Dispose();
                }
            }
        }
        public static void PrintTitle(string titleString = "")
        {
            int maxChars = 170;
            var eachSide = (maxChars - titleString.Length) / 2;

            //print left side
            for (int i = 0; i < eachSide; i++)
                Write("=");

            //print title
            Write(string.Format("{0}", titleString));

            //print left side
            for (int i = 0; i < eachSide; i++)
                Write("=");

            Write("\r\n");
        }
        public static void Debug(string message)
        {
            WriteLine(message, false, Types.LogLevel.DEBUG);
        }
        public static void Warning(string message)
        {
            WriteLine(message, false, Types.LogLevel.WARNING);
        }
        public static void Error(string message)
        {
            WriteLine(message, false, Types.LogLevel.ERROR);
        }

        public static void WriteLine(string message, bool removeTimeStamp = false, Types.LogLevel errorLevel = Types.LogLevel.DEBUG)
        {
            lock (lockObject)
            {
                string levelStr = errorLevel == Types.LogLevel.ERROR ? ERROR : errorLevel == Types.LogLevel.WARNING ? WARNING : DEBUG;
                if (removeTimeStamp == false)
                    message = string.Format("[{0}]\t{1}\t{2}", DateTime.Now.ToString("HH:mm:ss.fff"), levelStr, message);

                /*The writing to the file is in Append mode*/
                using (FileStream fs = new FileStream(pathString, FileMode.Append))
                {
                    /*New StreamWriter to write to the folder*/
                    StreamWriter writer = new StreamWriter(fs);

                    /*checks if the file excist*/
                    if (!File.Exists(pathString))
                    {
                        /*Create new file*/
                        File.Create(pathString);

                        /*Checks if the title of the method writes to the file*/
                        if (title == false)
                        {
                            /*Writes a line to the file*/
                            writer.WriteLine(NameOfMethod);
                            title = true;
                        }

                        /*Writhe the data to the file*/
                        writer.WriteLine(message);
                    }

                    else if (File.Exists(pathString))
                    {
                        if (title == false)
                        {
                            writer.WriteLine(NameOfMethod);
                            title = true;
                        }
                        writer.WriteLine(message);
                    }
                    /*Close the streamWriter*/
                    writer.Close();
                    /*Release all the resorces*/
                    writer.Dispose();
                }
            }
        }
        /*The function stops The writing to the file*/
        public static void Delete()
        {
            if (File.Exists(pathString))
                File.Delete(pathString);
        }

        #endregion

        #region Private Methods
        private static void StopWrite()
        {
            using (FileStream fs = new FileStream(pathString, FileMode.Append))
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.WriteLine();
                title = true;
                writer.Close();
                writer.Dispose();
            }
        }
        /*The function stops The writing to the file include message to logfile*/
        private static void StopWrite(string message)
        {
            using (FileStream fs = new FileStream(pathString, FileMode.Append))
            {
                StreamWriter writer = new StreamWriter(fs);
                writer.WriteLine(message);
                title = true;
                writer.Close();
                writer.Dispose();
            }
        }
        #endregion
    }
}




