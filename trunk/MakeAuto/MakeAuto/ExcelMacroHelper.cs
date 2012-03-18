using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace MakeAuto
{
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

    public class VBAState
    {
        // �����ϸ���˵����
        public Detail dl { get; set; }
        // �ܼƱ������
        public int count { get; set; }
        // ��ǰ����
        public int index { get; set; }
        // ����
        public int percent { get; set; }
        // ��ǰ�ļ�����״̬
        public CState cstate { get; set; }
        //
        public string info { get; set; }
    }


    /// <summary>
    /// ִ��Excel VBA�������
    /// </summary>
    class ExcelMacroHelper
    {
        private ExcelMacroHelper()
        {
            SrcDir = MAConf.instance.SrcDir;
            ExcelFilePath = MAConf.instance.DetailFile;
            ShowExcel = true;
            state = new VBAState();
        }

        // ������ ExcelMacroHelper
        public static readonly ExcelMacroHelper instance = new ExcelMacroHelper();

        /// <summary>
        /// ִ�� Excel ��
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        public void RunExcelMacro(System.ComponentModel.BackgroundWorker worker,
            System.ComponentModel.DoWorkEventArgs e)
        {
            int count = 0, num = 0, percent = 0;

            // ���ݲ�ͬ���ܵ��ö�Ӧ�ĺ�
            MacroType m = (MacroType)e.Argument;
            
            // ������ѡ�е�ģ��ִ�б���
            
            // ͳ����Ҫ�����ģ�����������浽 count ������
            foreach (Detail dl in MAConf.instance.Dls)
            {
                // ����Ҫ���������
                if (!dl.Compile)
                {
                    continue;
                }

                // ���ѡ������pas�ļ��ֶ�Ϊ�գ���ô����Ҫ����SO
                if (dl.Pas.Trim() == String.Empty)
                {
                    state.cstate = CState.Fail;
                    state.info = "���Ǻ���ģ�飬����ҪΪ�����Proc�ļ��� \r\n";
                    worker.ReportProgress(0, state);
                    continue;
                }

                ++count;
            }

            // �ܼƵı�����Ŀ
            state.count = count;

            // ִ�б�������
            foreach (Detail dl in MAConf.instance.Dls)
            {
                // ����Ҫ��������������ѡ������pas�ļ��ֶ�Ϊ�գ���ô����Ҫ����SO
                if (!dl.Compile || dl.Pas.Trim() == String.Empty)
                {
                    continue;
                }

                // ��������ɸ���
                percent = (int) (num * 100.0 / count);

                // ��status״ֵ̬
                state.dl = dl;
                state.index = num;
                state.percent = percent;
                state.info = "";
                state.cstate = CState.Nothing;
                

                // һ��һ�����룬������ɣ����� BeginNo �� EndNo ��ͬ
                BeginNo = MAConf.instance.Dls.IndexOf(dl) + 1;
                EndNo = BeginNo;

                // ���ȡ���� index Ϊ -1�� beginno == 0
                if (BeginNo == 0)
                {
                    state.cstate = CState.Fail;
                    state.info = Enum.GetName(typeof(CState), state.cstate);
                    worker.ReportProgress(percent, state);
                    continue;
                }
                
                // ���ó�ʼ��·��
                state.cstate = CState.Start;
                state.info = Enum.GetName(typeof(CState), state.cstate);
                worker.ReportProgress(percent, state);
                try
                {
                    if (RunExcelMacro((int)m, SrcDir, BeginNo, EndNo) == 0)
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
            }

            // ����������
            percent = 100;
            state.cstate = CState.End;
            state.info = "�������";
            worker.ReportProgress(percent, state);
        }

        public bool ScmRunExcelMacro(MacroType m, int FileNo, string outdir)
        {
            int OperType = (int)m;

            try
            {
                if (RunExcelMacro(OperType, outdir, FileNo, FileNo) != 0)
                {
                    log.WriteLog("����excelʧ��");
                    return false;
                }
            }
            catch (Exception ex)
            {
                MAConf.instance.WriteLog("����excel�쳣��" + ex.Message);
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
        /// ִ��Excel�еĺ�
        /// </summary>
        /// <param name="excelFilePath">Excel�ļ�·��</param>
        /// <param name="macroName">������</param>
        /// <param name="parameters">�������</param>
        /// <param name="rtnValue">�귵��ֵ</param>
        /// <param name="isShowExcel">ִ��ʱ�Ƿ���ʾExcel</param>
        private void RunExcelMacro(string excelFilePath, string macroName, object[] parameters,
            out object rtnValue, bool isShowExcel)
        {
            try
            {
                #region ������

                // ����ļ��Ƿ����
                if (!File.Exists(excelFilePath))
                {
                    throw new System.Exception(excelFilePath + " �ļ�������");
                }

                // ����Ƿ����������
                if (string.IsNullOrEmpty(macroName))
                {
                    throw new System.Exception("������������");
                }

                #endregion

                #region ���ú괦��

                // ׼����Excel�ļ�ʱ��ȱʡ��������
                object oMissing = System.Reflection.Missing.Value;

                // ���ݲ������Ƿ�Ϊ�գ�׼�����������
                object[] paraObjects;

                if (parameters == null)
                {
                    paraObjects = new object[] { macroName };
                }
                else
                {
                    // ������鳤��
                    int paraLength = parameters.Length;

                    paraObjects = new object[paraLength + 1];

                    paraObjects[0] = macroName;
                    for (int i = 0; i < paraLength; i++)
                    {
                        paraObjects[i + 1] = parameters[i];
                    }
                }

                // ����Excel����ʾ��
                Excel.Application oExcel = new Excel.Application();

                // �ж��Ƿ�Ҫ��ִ��ʱExcel�ɼ�
                if (isShowExcel)
                {
                    // ʹ�����Ķ���ɼ�
                    oExcel.Visible = true;
                }

                // ����Workbooks����
                Excel.Workbooks oBooks = oExcel.Workbooks;

                // ����Workbook����
                Excel._Workbook oBook = null;

                // ��ָ����Excel�ļ�
                oBook = oBooks.Open(excelFilePath, oMissing, oMissing, oMissing, oMissing, oMissing,
                    oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing, oMissing,
                    oMissing);

                // ִ��Excel�еĺ�
                rtnValue = this.RunMacro(oExcel, paraObjects);

                // �������
                //oBook.Save();

                // �˳�Workbook
                oBook.Close(false, oMissing, oMissing);

                #endregion

                #region �ͷŶ���

                // �ͷ�Workbook����
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBook);
                oBook = null;

                // �ͷ�Workbooks����
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oBooks);
                oBooks = null;

                // �ر�Excel
                oExcel.Quit();

                // �ͷ�Excel����
                System.Runtime.InteropServices.Marshal.ReleaseComObject(oExcel);
                oExcel = null;

                // ������������
                GC.Collect();

                #endregion
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// ִ�к�
        /// </summary>
        /// <param name="oApp">Excel����</param>
        /// <param name="oRunArgs">��������һ������Ϊָ�������ƣ�����Ϊָ����Ĳ���ֵ��</param>
        /// <returns>�귵��ֵ</returns>
        private object RunMacro(object oApp, object[] oRunArgs)
        {
            try
            {
                // ����һ�����ض���
                object objRtn;
                
                // ���䷽ʽִ�к�
                objRtn = oApp.GetType().InvokeMember("Run", 
                    System.Reflection.BindingFlags.Default | System.Reflection.BindingFlags.InvokeMethod,
                    null, oApp, oRunArgs);

                // ����ֵ
                return objRtn;

            }
            catch (Exception ex)
            {
                // ����еײ��쳣���׳��ײ��쳣
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

        private Detail currDetail { get; set; }

        // ���������Զ�Ӧ��ͨ���˵�����˵����ʱ������ѡ��
        private string SrcDir { get; set; }
        private int BeginNo { get; set; }
        private int EndNo { get; set; }

        public VBAState state;

        private OperLog log = OperLog.instance;

        // ��������ƣ�����д�� ExtPub
        private static readonly string MacroName = "ScmExtPub";
    }
}
