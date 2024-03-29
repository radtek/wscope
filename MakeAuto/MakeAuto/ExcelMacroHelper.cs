using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace MakeAuto
{
    /// <summary>
    /// 定义宏调用类型
    /// </summary>
    public enum MacroType
    {
        Nothing = 0,
        ProC = 1,
        SQL,
        FuncXml,
        Hyper,
    }

    public enum CState
    {
        Nothing = 0,
        Start = 1,
        Process = 2,
        End = 3,
        Fail = 4,
        Success = 5,
        FailEx = 6,
        Other = 7,
        All = 8,
    }

    public class ExArg
    {
        public ExArg(MacroType MacroType, int No)
        {
            MType = MacroType;
            ModuleNo = No;
        }

        public MacroType MType { get; set; }
        public int ModuleNo { get; set; }
    }

    public class VBAState
    {
        // 活动的详细设计说明书
        // 总计编译个数
        public int count { get; set; }
        // 当前个数
        public int index { get; set; }
        // 进度
        public int percent { get; set; }
        // 当前文件编译状态
        public CState cstate { get; set; }
        //
        public string info { get; set; }
    }


    /// <summary>
    /// 执行Excel VBA宏帮助类
    /// </summary>
    class ExcelMacroHelper
    {
        public ExcelMacroHelper(string ExcelFile)
        {
            SrcDir = @"C:\src";
            ExcelFilePath = ExcelFile;
            ShowExcel = true;
            state = new VBAState();
        }

        /// <summary>
        /// 执行 Excel 宏
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        public void RunExcelMacro(System.ComponentModel.BackgroundWorker worker,
            System.ComponentModel.DoWorkEventArgs e)
        {
            int count = 0, num = 0, percent = 0;

            // 根据不同功能调用对应的宏
            ExArg ExA = (ExArg)e.Argument;
            VBAState state = new VBAState();
            state.cstate = CState.Start;
            state.info = "启动";
            worker.ReportProgress(0, state);
            try
            {
                if (RunExcelMacro((int)(ExA.MType), SrcDir, ExA.ModuleNo, ExA.ModuleNo) == 0)
                {
                    ++num;
                    percent = (int)(num * 100.0 / count);
                    state.cstate = CState.Success;
                    state.info = Enum.GetName(typeof(CState), state.cstate);
                    worker.ReportProgress(percent, state);
                }
                else
                {
                    ++num;
                    percent = (int)(num * 100.0 / count);
                    state.cstate = CState.Fail;
                    state.info = Enum.GetName(typeof(CState), state.cstate);
                    worker.ReportProgress(percent, state);
                }
            }
            catch (Exception ex)
            {
                ++num;
                percent = (int)(num * 100.0 / count);
                state.cstate = CState.FailEx;
                state.info = ex.Message;
                worker.ReportProgress(percent, state);
            }

            // 标记所有完成
            state.cstate = CState.End;
            state.info = "编译完成";
            worker.ReportProgress(100, state);
        }

        public bool ScmRunExcelMacro(MacroType m, int FileNo, string outdir)
        {
            int OperType = (int)m;

            try
            {
                if (RunExcelMacro(OperType, outdir, FileNo, FileNo) != 0)
                {
                    log.WriteLog("编译excel失败");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MAConf.instance.WriteLog("编译excel异常，" + ex.Message);
                return false;
            }

            return true;
        }

        private int RunExcelMacro(int OperType, string SrcDir, int BeginNo, int EndNo)
        {
            object r = null;
            RunExcelMacro(ExcelFilePath, MacroName, new object[] {OperType, SrcDir, BeginNo, EndNo }, out r, ShowExcel);
            
            if (r != null)
            {
                return int.Parse(r.ToString());
            }
            else 
            {
                return -1;
            }
        }

        /// <summary>
        /// 执行Excel中的宏
        /// </summary>
        /// <param name="excelFilePath">Excel文件路径</param>
        /// <param name="macroName">宏名称</param>
        /// <param name="parameters">宏参数组</param>
        /// <param name="rtnValue">宏返回值</param>
        /// <param name="isShowExcel">执行时是否显示Excel</param>
        private void RunExcelMacro(string excelFilePath, string macroName, object[] parameters,
            out object rtnValue, bool isShowExcel)
        {
            try
            {
                #region 检查入参

                // 检查文件是否存在
                if (!File.Exists(excelFilePath))
                {
                    throw new System.Exception(excelFilePath + " 文件不存在");
                }

                // 检查是否输入宏名称
                if (string.IsNullOrEmpty(macroName))
                {
                    throw new System.Exception("请输入宏的名称");
                }

                #endregion

                #region 调用宏处理

                // 准备打开Excel文件时的缺省参数对象
                object oMissing = System.Reflection.Missing.Value;

                // 根据参数组是否为空，准备参数组对象
                object[] paraObjects;

                if (parameters == null)
                {
                    paraObjects = new object[] { macroName };
                }
                else
                {
                    // 宏参数组长度
                    int paraLength = parameters.Length;

                    paraObjects = new object[paraLength + 1];

                    paraObjects[0] = macroName;
                    for (int i = 0; i < paraLength; i++)
                    {
                        paraObjects[i + 1] = parameters[i];
                    }
                }

                // 创建Excel对象示例
                Excel.Application oExcel = new Excel.Application();

                // 判断是否要求执行时Excel可见
                if (isShowExcel)
                {
                    // 使创建的对象可见
                    oExcel.Visible = true;
                }

                // 创建Workbooks对象
                Excel.Workbooks oBooks = oExcel.Workbooks;

                // 创建Workbook对象
                Excel._Workbook oBook = null;

                // 打开指定的Excel文件
                oBook = oBooks.Open(excelFilePath, oMissing, oMissing, oMissing, oMissing, oMissing,
                    oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing,
                    oMissing);

                // 执行Excel中的宏
                rtnValue = this.RunMacro(oExcel, paraObjects);

                // 保存更改
                //oBook.Save();

                // 退出Workbook
                oBook.Close(false, oMissing, oMissing);

                #endregion

                #region 释放对象

                // 释放Workbook对象
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBook);
                oBook = null;

                // 释放Workbooks对象
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBooks);
                oBooks = null;

                // 关闭Excel
                oExcel.Quit();

                // 释放Excel对象
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcel);
                oExcel = null;

                // 调用垃圾回收
                GC.Collect();

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// 执行宏
        /// </summary>
        /// <param name="oApp">Excel对象</param>
        /// <param name="oRunArgs">参数（第一个参数为指定宏名称，后面为指定宏的参数值）</param>
        /// <returns>宏返回值</returns>
        private object RunMacro(object oApp, object[] oRunArgs)
        {
            try
            {
                // 声明一个返回对象
                object objRtn;
                
                // 反射方式执行宏
                objRtn = oApp.GetType().InvokeMember("Run", 
                    System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                    null, oApp, oRunArgs);

                // 返回值
                return objRtn;

            }
            catch (Exception ex)
            {
                // 如果有底层异常，抛出底层异常
                if (ex.InnerException.Message.ToString().Length > 0)
                {
                    throw ex.InnerException;
                }
                else
                {
                    throw ex;
                }
            }
        }

        public string ExcelFilePath { get; private set; }

        public bool ShowExcel { get; private set; }

        // 这三个属性对应于通过菜单功能说明书时操作的选择
        private string SrcDir { get; set; }
        private int BeginNo { get; set; }
        private int EndNo { get; set; }

        public VBAState state;

        private OperLog log = OperLog.instance;

        // 外调宏名称，现在写成 ExtPub
        private static readonly string MacroName = "ScmExtPub";
    }
}
