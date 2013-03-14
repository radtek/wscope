using System;
using System.Collections;
using System.Diagnostics;


namespace MakeAuto
{
    enum FileStatus
    {
        NoChange = 0,
        Old = 1,
        New = 2,
        Unkown = 3,
    }

    enum SAWType
    {
        Nothing = 0,
        Project = 1,
        File = 2,
    }

    // 这里保存需要从 SAW 刷代码的文件列表
    class SAWFile
    {
        public SAWFile(string path, FileStatus status = FileStatus.NoChange)
        {
            Path = path;
            fstatus = status;
            Version = " ";

            // 根据最后带不带后缀分析
            if (System.IO.Path.GetExtension(Path) == string.Empty)
            {
                Type = SAWType.Project;
            }
            else Type = SAWType.File;
        }

        public string Path;  // ReadMe 中的路径，可能是文件，也可能是目录，这个类的主键
        public SAWType Type;   // Project or File ?? 1 - Project 2-File
        public string UriPath;
        public string LocalPath;
        public string Version;
        //public string LocalVersion;
        public FileStatus fstatus;
        public DateTime LastModTime;
    }

    class SAWFileList : ArrayList
    {
        public SAWFile this[string path]
        {
            get
            {
                foreach (SAWFile s in this)
                {
                    if (s.Path == path)
                        return s;
                }
                return null;
            }
        }
    }
}