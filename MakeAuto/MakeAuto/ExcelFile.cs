using System.Collections;

namespace MakeAuto
{
    /// <summary>
    /// 详细设计说明书信息
    /// </summary>
    public class Detail
    {
        /// <summary>
        /// 详细设计说明书类构造函数，保存模块的相关信息
        /// </summary>
        /// <param name="Name">模块名称</param>
        /// <param name="File">Excel文件名</param>
        /// <param name="Sql">SQL文件名</param>
        /// <param name="Pas">PAS文件名</param>
        public Detail(string Name, string File, string Sql, string Pas)
        {
            this.Name= Name;
            this.File = File;
            this.Pas = Pas;
            this.Sql = Sql;
            
            // 保存Proc文件列表
            ProcFiles = new ArrayList();
            if (Pas != string.Empty)
            {
                ProcFiles.Add(Gcc);
                ProcFiles.Add(Pc);
                ProcFiles.Add(Header);
                ProcFiles.Add(Cpp);
            }

            Compile = false;
            Show = true;
        }

        // Pro*C中间件文件
        public ArrayList ProcFiles;

        // 是否勾选了编译，在勾选时变为true；否则变为 false
        public bool Compile { get; set; }

        // 是否显示
        public bool Show { get; set; }

        #region 一些属性，用来保存模块编号及相关的文件名
        public string Name { get; set; }
        public string File {get; set;}
        public string Pas {get; set;}
        public string Sql {get; set;}

        public string Gcc
        {
            get { return "s_" + Pas + "flow.gcc"; }
        }

        public string Pc
        {
            get { return "s_" + Pas + "func.pc"; }
        }

        public string Cpp
        {
            get { return "s_" + Pas + "flow.cpp"; }
        }

        public string Header
        {
            get { return "s_" + Pas + "func.h"; }
        }

        public string OFlow
        {
            get { return "s_" + Pas + "flow.o"; }
        }

        public string OFunc
        {
            get { return "s_" + Pas + "func.o"; }
        }

        public string SO
        {
            get { return Pas.Trim() == string.Empty ? string.Empty : "libs_" + Pas + "flow.10.so"; }
        }
        #endregion
        
    }

    class Details : ArrayList
    {
        public Detail this[string name]
        {
            get
            {
                foreach (Detail a in this)
                {
                    if (a.Name == name)
                    {
                        return a;
                    }
                }

                return null;
            }
        }
    }
}
