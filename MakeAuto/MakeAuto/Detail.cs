using System.Collections;

namespace MakeAuto
{
    /// <summary>
    /// 详细设计说明书信息
    /// </summary>
    class Detail
    {
        public Detail(CommitCom c)
        {
            this.Name = c.cname;
            Pas = c.cname.Replace("libs_", "").Replace("flow.10.so", "");
            ProcFiles = new ArrayList();
            MiddFiles = new ArrayList();
            Gcc = "s_" + Pas + "flow.gcc";
            SO = "libs_" + Pas + "flow.10.so"; 
            ProcFiles.Add(Gcc);
            
            ProcFiles.Add("s_" + Pas + "func.h");

            if (c.cname.IndexOf("s_ls_") >= 0)
            {
                ProcFiles.Add("s_" + Pas + "flow.cpp");
                ProcFiles.Add("s_" + Pas + "func.cpp"); // 与原子与逻辑不同
            }
            else if (c.cname.IndexOf("s_as_") >= 0)
            {
                ProcFiles.Add("s_" + Pas + "flow.pc");
                ProcFiles.Add("s_" + Pas + "func.pc");
                
                MiddFiles.Add("s_" + Pas + "func.cpp");
            }

            MiddFiles.Add("s_" + Pas + "flow.o");
            MiddFiles.Add("s_" + Pas + "func.o");

            Compile = false;
            Show = true;
        }

        /// <summary>
        /// 详细设计说明书类构造函数，保存模块的相关信息
        /// </summary>
        /// <param name="Name">模块名称</param>
        /// <param name="File">Excel文件名</param>
        /// <param name="Sql">SQL文件名</param>
        /// <param name="Pas">PAS文件名</param>
        public Detail(string Name, string File, string Sql, string Pas)
        {
            this.Name = Name;
            this.File = File;
            this.Pas = Pas;
            this.XmlFile = Pas + ".xml";
 
            // 保存Proc文件列表
            ProcFiles = new ArrayList();
            MiddFiles = new ArrayList();
            if (Pas != string.Empty)
            {
                Gcc = "s_" + Pas + "flow.gcc";
                ProcFiles.Add(Gcc);
                ProcFiles.Add("s_" + Pas + "flow.cpp");
                ProcFiles.Add("s_" + Pas + "func.h");
                ProcFiles.Add("s_" + Pas + "func.pc");
                MiddFiles.Add("s_" + Pas + "func.cpp");
                MiddFiles.Add("s_" + Pas + "flow.o");
                MiddFiles.Add("s_" + Pas + "func.o");

                if (Pas.Trim() == "public")
                {
                    SO = "libs_" + Pas + "func.10.so";
                }
                else
                {
                    SO = "libs_" + Pas + "flow.10.so";
                }
            }

            if (Sql != string.Empty)
            {
                SqlFile = Sql + "_or.sql";
            }


            Compile = false;
            Show = true;
        }

        public string GetProcStr(bool quotes = true)
        {
            string t = string.Empty;
            foreach(string s in ProcFiles)
            {
                if (quotes)
                    t += "\"";
                t += s;
                if (quotes)
                    t += "\"";
                t += " ";
            }

            return t;
        }

        public string GetMiddStr(bool quotes = true)
        {
            string t = string.Empty;
            foreach (string s in MiddFiles)
            {
                if (quotes)
                    t += "\"";
                t += s;
                if (quotes)
                    t += "\"";
                t += " ";
            }
            return t;
        }

        // Pro*C中间件文件
        public ArrayList ProcFiles;
        public ArrayList MiddFiles;

        // 是否勾选了编译，在勾选时变为true；否则变为 false
        public bool Compile { get; set; }

        // 是否显示
        public bool Show { get; set; }

        public string Pas { get; private set; }
        public string Name { get; private set; }
        public string File { get; private set; }
        public string SqlFile { get; private set; }
        public string XmlFile { get; private set; }
        public string Gcc { get; private set; }
        public string SO { get; private set; }
    }

    class Details : ArrayList
    {
        public Detail this[string name]
        {
            get
            {
                foreach (Detail d in this)
                {
                    if (name.Equals(d.Name, System.StringComparison.Ordinal))
                    {
                        return d;
                    }
                }

                return null;
            }
        }

        public Detail FindByName(CommitCom c)
        {
            if (c.ctype == ComType.SO)
            {
                return FindBySo(c.cname);
            }
            else if (c.ctype == ComType.Sql)
            {
                return FindBySql(c.cname);
            }
            else if (c.ctype == ComType.Xml)
            {
                return FindByXml(c.cname);
            }
            else
            {
                return null;
            }
        }

        private Detail FindBySo(string name)
        {
            foreach (Detail d in this)
            {
                if (name.Equals(d.SO, System.StringComparison.Ordinal))
                {
                    return d;
                }
            }
            return null;
        }

        private Detail FindBySql(string name)
        {
            foreach (Detail d in this)
            {
                if (name.Equals(d.SqlFile, System.StringComparison.Ordinal))
                {
                    return d;
                }
            }

            return null;
        }

        private Detail FindByXml(string name)
        {
            foreach (Detail d in this)
            {
                if (name.Equals(d.XmlFile, System.StringComparison.Ordinal))
                {
                    return d;
                }
            }

            return null;
        }
    }
}
