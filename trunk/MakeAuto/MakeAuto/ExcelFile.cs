using System.Collections;

namespace MakeAuto
{
    /// <summary>
    /// Excel 文件信息
    /// </summary>
    class ExcelFile
    {
        /// <summary>
        /// Excel 模块类构造函数，保存模块的相关信息
        /// </summary>
        /// <param name="ModuleNo">模块编号</param>
        /// <param name="ModuleName">模块名称</param>
        /// <param name="FileName">Excel文件名</param>
        /// <param name="SqlFile">SQL文件名</param>
        /// <param name="PasFile">PAS文件名</param>
        public ExcelFile(int ModuleNo, string ModuleName, string FileName, string SqlFile, string PasFile)
        {
            _moduleno = ModuleNo;
            _modulename = ModuleName;
            _filename = FileName;
            _pasfile = PasFile;
            _sqlfile = SqlFile;
            
            // 保存Proc文件列表
            ProcFiles = new ArrayList();
            if (_pasfile != string.Empty)
            {
                ProcFiles.Add(GccFile);
                ProcFiles.Add(PcFile);
                ProcFiles.Add(HeaderFile);
                ProcFiles.Add(CppFile);
            }
        }

        #region 一些属性，用来保存模块编号及相关的文件名
        private int _moduleno;
        private string _modulename;
        private string _filename;
        private string _pasfile;
        private string _sqlfile;

        public ArrayList ProcFiles;

        public int ModuleNo
        {
            get { return _moduleno; }
            set { _moduleno = value; }
        }

        public string ModuleName 
        {
            get { return _modulename; }
            set { _modulename = value; }
        }

        public string FileName
        {
            get { return _filename; }
            set { _filename = value; }
        }
        public string PasFile
        {
            get { return _pasfile; }
            set { _pasfile = value; }
        }

        public string SqlFile
        {
            get { return _sqlfile; }
            set { _sqlfile = value + ".sql"; }
        }

        public string GccFile
        {
            get { return "s_" + _pasfile + "flow.gcc"; }
        }

        public string PcFile
        {
            get { return  "s_" + _pasfile + "func.pc"; }
        }

        public string CppFile
        {
            get { return "s_" + _pasfile + "flow.cpp"; }
        }

        public string HeaderFile
        {
            get { return  "s_" + _pasfile + "func.h"; }
        }

        public string OFlowFile
        {
            get { return  "s_" + _pasfile + "flow.o"; }
        }

        public string OFuncFile
        {
            get { return "s_" + _pasfile + "func.o"; }
        }

        public string SOFile
        {
            get { return _pasfile.Trim() == string.Empty ? string.Empty : "libs_" + _pasfile + "flow.10.so"; }
        }
        #endregion
        
    }
}
