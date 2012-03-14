using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace MakeAuto
{
    /// <summary>
    /// 修改单处理流程，状态模式的客户端调用者
    /// </summary>
    class AmendFlow
    {
        public AmendFlow(AmendPack amend)
        {
            // 修改单构造
            _amendno = amend.AmendNo;
            _amend = amend;

            // 准备流程
            // 下载递交包-预处理递交包-检查递交包-检出VSS代码-编译-递交-清理
            _aflow = new ArrayList();
            _aflow.Add(new PackerDownload());
            _aflow.Add(new PackerProcess());
            _aflow.Add(new PackerReadMe());
            _aflow.Add(new PackerCheck());
            _aflow.Add(new PackerVSSCode());
            _aflow.Add(new PackerCompile());
            _aflow.Add(new PackerRePack());
            _aflow.Add(new PackerUpload());
            _aflow.Add(new PackCleanUp());

            _state = (State)_aflow[0];  // 起始步骤
        }

        public bool Work()
        {
            foreach (State s in _aflow)
            {
                log.WriteLog(s.StateName + " 开始");
                if (!s.DoWork(_amend))
                    return false;
                log.WriteLog(s.StateName + " 完成");
            }

            log.WriteLog("[流程结束]");
            return true;
        }

        public string AmendNo
        {
            get { return _amendno; }
        }

        public AmendPack Amend
        {
            get { return _amend; }
        }

        private State _state;
        private string _amendno;
        private AmendPack _amend;
        private ArrayList _aflow;

        private OperLog log = OperLog.instance;
    }
}
