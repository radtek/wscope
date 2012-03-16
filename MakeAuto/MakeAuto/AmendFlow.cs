using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Windows.Forms;

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
            _aflow.Add(new PackerReadMe());
            _aflow.Add(new PackerProcess());
            _aflow.Add(new PackerCheck());
            _aflow.Add(new PackerVSSCode());
            _aflow.Add(new PackerCompile());
            _aflow.Add(new PackerDiffer());
            _aflow.Add(new PackerSO());
            _aflow.Add(new PackerRePack());
            _aflow.Add(new PackerUpload());
            _aflow.Add(new PackCleanUp());

            _state = (State)_aflow[0];  // 起始步骤
        }

        public bool Work()
        {

            int i;
            for (i = _aflow.IndexOf(_state); i < _aflow.Count; ++i)
            {
                _state = (State)_aflow[i];

                string message = "下一步 " + _state.StateName + " 继续 ？";
                string caption = "测试";

                DialogResult result = MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel);
                if (result == System.Windows.Forms.DialogResult.Cancel)
                {
                    continue;
                }
                else if (result == System.Windows.Forms.DialogResult.No)
                {
                    break;
                }

                log.WriteLog(_state.StateName + " 开始");
                if (!_state.DoWork(_amend))
                    return false;

                log.WriteLog(_state.StateName + " 完成");
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
