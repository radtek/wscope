using System;
using Excel = Microsoft.Office.Interop.Excel;
using System.IO;

namespace MakeAuto
{
    /// <summary>
    /// ִ��Excel VBA�������
    /// </summary>
    class ExcelMacroHelper
    {
        public string ExcelFilePath { get; set; }
        public string MacroName { get; set;}
        public bool IsShowExcel { get; set; }
        
        public string SrcDir{ get; set;}
        public int BeginNo { get; set;}
        public int EndNo { get; set;}
        
        /// <summary>
        /// ִ�� Excel ��
        /// </summary>
        /// <param name="worker"></param>
        /// <param name="e"></param>
        public void RunExcelMacro(System.ComponentModel.BackgroundWorker worker,
            System.ComponentModel.DoWorkEventArgs e)
        {
            worker.ReportProgress(0, "Start");
            try
            {
                if (RunExcelMacro(SrcDir, BeginNo, EndNo) == 0)
                {
                    worker.ReportProgress(100, "End");
                    worker.ReportProgress(100, "Succed");
                }
                else
                {
                    worker.ReportProgress(0, "Failed");
                }
            }
            catch (Exception ex)
            {
                worker.ReportProgress(0, ex.Message);
            }
        }


        public int RunExcelMacro(string SrcDir, int BeginNo, int EndNo)
        {
            object r = null;
            RunExcelMacro(ExcelFilePath, MacroName, new object[] { SrcDir, BeginNo, EndNo }, out r, IsShowExcel);
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
        public void RunExcelMacro(string excelFilePath, string macroName, object[] parameters,
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
    }
}
