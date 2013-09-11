using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MakeAuto
{
    public enum LogLevel
    {
        Nothing = 0,
        FileLog,
        FormLog,
        SqlExe,
        Info,
        Warning,
        Error,
    }

    public class LogInfoArgs : EventArgs
    {
        public string info {get; set;}
        public string title { get; private set; }
        public LogLevel level { get; set; }

        public LogInfoArgs(string info, LogLevel level = LogLevel.Info)
        {
            if (level == LogLevel.Error)
                this.title = "[ERROR]";
            else if (level == LogLevel.Warning)
                this.title = "[Warning]";
            else this.title = "";

            this.info = info;
            this.level = level;
        }
    }

    public delegate void LogInfoEventHandler(object sender, LogInfoArgs e);    //定义信息输出委托

    class OperLog
    {
        public event LogInfoEventHandler OnLogInfo;    //基本信息实现事件

        // 单例化 MAConf
        public static readonly OperLog instance = new OperLog();

        private OperLog()
        {
            LogDir = "Log";
            LogFile = Path.Combine(LogDir, "MA" + DateTime.Now.ToString("yyyyMMdd") + ".Log");
            if (!System.IO.Directory.Exists(LogDir))
            {
                try
                {
                    System.IO.Directory.CreateDirectory(LogDir);
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message);
                }
            }

            try
            {
                filestream = new FileStream(LogFile, FileMode.OpenOrCreate, FileAccess.Write);
            }
            catch (ArgumentException e)
            {
                System.Windows.Forms.MessageBox.Show("ArgumentException\r\n" + e.Message);
            }
            catch (IOException e)
            {
                System.Windows.Forms.MessageBox.Show("IOException\r\n" + e.Message);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Exception\r\n" + e.Message);
            }

            try
            {
                writer = new StreamWriter(filestream, System.Text.Encoding.Default);
                writer.BaseStream.Seek(0, SeekOrigin.End);
            }
            catch (ArgumentNullException e)
            {
                System.Windows.Forms.MessageBox.Show("ArgumentNullException\r\n" + e.Message);
            }
            catch (ArgumentException e)
            {
                System.Windows.Forms.MessageBox.Show("ArgumentException\r\n" + e.Message);
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show("Exception\r\n" + e.Message);
            }

            //System.Windows.Forms.MessageBox.Show("seek");
        }

        public void WriteFileLog(string info)
        {
            WriteLog(info, LogLevel.FileLog);
        }

        public void WriteInfoLog(string info)
        {
            WriteLog(info, LogLevel.Info);
        }

        public void WriteErrorLog(string info)
        {
            WriteLog(info, LogLevel.Error);
        }

        // 写文本日志
        public void WriteLog(string info, LogLevel level = LogLevel.Info)
        {
            LogInfoArgs e = new LogInfoArgs(info, level);

            try
            {
                writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "  " + e.title + e.info);
                writer.Flush(); // 及时写入
            }
            catch (ObjectDisposedException ex)
            {
                System.Windows.Forms.MessageBox.Show("w ObjectDisposedException\r\n" + ex.Message);
            }
            catch (IOException ex)
            {
                System.Windows.Forms.MessageBox.Show("w IOException\r\n" + ex.Message);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("w Exception\r\n" + ex.Message);
            }

            // 通知日志的订户
            if (OnLogInfo != null)
            {
                OnLogInfo(this, e);
            }
        }

        public void Flush()
        {
            writer.Flush();
        }

        private string LogDir;
        private string LogFile;
        private FileStream filestream;
        private StreamWriter writer;

    }
}
