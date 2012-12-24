using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SharpSvn;

namespace MakeAuto
{
    /// <summary>
    /// 提供检出代码到本地的功能
    /// </summary>
    interface IVersion
    {
        bool GetAmendCode(string AmendNo, string version)
    }
}
