using System;
using System.Collections;
using System.Windows.Forms;
using System.Threading;

namespace MakeAuto
{
    public delegate void FlowInfoEventHandler(object sender, EventArgs e);    //定义信息输出委托
        
    /// <summary>
    /// 修改单处理流程，状态模式的客户端调用者
    /// </summary>
    class AmendFlow
    {
        public event FlowInfoEventHandler OnFlowInfo;    //基本信息实现事件

        public AmendFlow()
        {
            // 准备流程
            // 下载递交包-预处理递交包-检查递交包-检出VSS代码-编译-递交-清理
            // 下载递交包-从ftp 上获取压缩包
            // 预处理递交包，包括解压缩，检查
            _aflow = new ArrayList();
            _aflow.Add(new PackerDownload());
            _aflow.Add(new PackerReadMe());
            _aflow.Add(new PackerCopyCom());
            //_aflow.Add(new PackerCheck());
            //_aflow.Add(new PackerVSSCode());
            _aflow.Add(new PackerSvnCode());
            //_aflow.Add(new PackerCompile());
            _aflow.Add(new PackerDiffer());
            //_aflow.Add(new PackerSO());
            _aflow.Add(new PackerRePack());
            //_aflow.Add(new PackerUpload());
            //_aflow.Add(new PackCleanUp());
        }

        public bool Work()
        {
            //log.WriteLog("启动集成线程....." + Environment.NewLine, LogLevel.Error);
            Thread t = new Thread(new ThreadStart(ThreadProc));
            t.IsBackground = true;  // 界面关闭时退出线程
            t.Start();
            return true;
        }
        private void ThreadProc()
        {
            bool result = true;
            const string SUCC = "流程结束-处理成功";
            const string FAIL = "流程结束-处理失败";
            if (Amend.scmstatus == ScmStatus.Error)
            {
                log.WriteErrorLog("修改单状态不正常，退出流程。");
                return;
            }

            string message, caption;

            foreach (State _state in _aflow)
            {
                if (_state.Tip)  // 如果这步流程比较重要，需要提示，需要调整步骤的提示为 true，对于 true 的要求用户确认
                {
                    message = "下一步 " + _state.StateName + " 继续 ？";
                    message += "\r\n【是】-继续 【否】-结束流程 【取消】-跳过当前步骤，进行下一步";
                    caption = "测试";

                    DialogResult dRes = MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel);
                    if (dRes == System.Windows.Forms.DialogResult.Cancel)
                    {
                        log.WriteInfoLog("跳过流程，当前步骤：" + _state.StateName);
                        continue;
                    }
                    else if (dRes == System.Windows.Forms.DialogResult.No)
                    {
                        log.WriteInfoLog("结束流程，当前步骤：" + _state.StateName);
                        break;
                    }
                }

                log.WriteLog("STEP：" + _state.StateName + "-开始");

                if (!_state.DoWork(Amend))
                {
                    result = false;
                    break;
                }

                log.WriteLog("STEP：" + _state.StateName + "-完成");
            }

            caption = "处理完成";
            EventArgs e = new EventArgs();

            // 通知日志的订户
            if (result)
            { 
                message = SUCC;
                log.WriteLog(message);
               // MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                message = FAIL;
                log.WriteErrorLog(message);
                //MessageBox.Show(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            if (OnFlowInfo != null)
            {
                OnFlowInfo(this, e);
            }
            
            return;
        }

        public AmendPack Amend;
        private ArrayList _aflow;
        private OperLog log = OperLog.instance;
    }
}
