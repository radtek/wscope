using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MakeAuto
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (args.Length == 0)
            {
                MessageBox.Show("无递交路径，退出程序！");
                Application.Exit();
                //Application.Run(new FormCommit());
                
            }
            else
                Application.Run(new FormCommit(args));
        }
    }
}
