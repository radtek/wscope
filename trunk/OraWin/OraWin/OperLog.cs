using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace OraWin
{
    enum LogLevel
    {
        Info,
        Warning,
        Error,
    }

    class OperLog
    {
        // 单例化 MAConf
        public static readonly OperLog instance = new OperLog();

        private OperLog()
        {
            LogDir = "Log";
            LogFile = LogDir + "\\" + "OraWin" + DateTime.Now.ToString("yyyyMMdd") + ".sql";
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
        }

        // 写文本日志
        public void WriteLog(string info, LogLevel level)
        {
            try
            {
                writer.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + LogType(level) + info);
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
        }

        public void WriteInfo(string info)
        {
            try
            {
                writer.WriteLine(info);
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
 
        }

        public string LogType(LogLevel level)
        {
            if (level == LogLevel.Info)
                return "[消息]";
            if (level == LogLevel.Warning)
                return "[警告]";
            if (level == LogLevel.Error)
                return "[错误]";

            return "[**]";
        }

        private string LogDir;
        private string LogFile;
        private FileStream filestream;
        private StreamWriter writer;

    }
}
