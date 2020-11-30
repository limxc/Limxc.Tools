using Limxc.Tools.DeviceComm.Entities;
using Limxc.Tools.DeviceComm.TaskManager;
using Limxc.Tools.DeviceComm.Utils;
using System;
using System.Windows.Forms;

namespace Limxc.Tools.DeviceComm.TestControls
{
    public partial class TaskManagerDebugUC : UserControl
    {
        public TaskManagerDebugUC()
        {
            InitializeComponent();
        }

        private JobScheduler sch = JobScheduler.Instance;

        private void CPSEDebugUC_Load(object sender, EventArgs e)
        {
            sch.OnJobFinished += (s) =>
            {
            };

            sch.OnExecStateChanged += s =>
            {
                this.Invoke(new Action(() =>
                {
                    switch (s)
                    {
                        case JobSchedulerState.执行中:

                            btnStart.Enabled = false;
                            btnStop.Enabled = btnReset.Enabled = true;

                            break;

                        case JobSchedulerState.停止中:
                            btnStart.Enabled = btnStop.Enabled = btnReset.Enabled = btnAppend.Enabled = false;
                            break;

                        case JobSchedulerState.就绪:
                        default:
                            btnStart.Enabled = btnReset.Enabled = btnAppend.Enabled = true;
                            btnStop.Enabled = false;
                            break;
                    }
                }));
            };

            sch.OnLog += (msg, level) =>
            {
                try
                {
                    this.Invoke(new Action(() =>
                    {
                        rtbJDs.AppendText($"{msg}{Environment.NewLine}");
                    }));
                }
                catch (Exception ex) { }
            };
        }

        private async void btnStart_Click(object sender, EventArgs e) => await sch.Start();

        private async void btnStop_Click(object sender, EventArgs e) => await sch.Stop();

        private async void btnReset_Click(object sender, EventArgs e) => await sch.ClearJobs().ContinueWith(_ => sch.ClearFinishedJobs());

        private void btnClearLogs_Click(object sender, EventArgs e) => rtbJDs.Clear();

        private void btnDemo_Click(object sender, EventArgs e)
        {
            Func<CPCmdTaskManager, string> ExecHandler = (cmd) =>
            {
                //Thread.Sleep(10);

                var r = CPTool.SendAndParse("Com15", 115200, cmd);
                return string.Join(",", r);
            };

            Func<CPResp, JobState> RespHandler = (rst) =>
           {
               return JobState.完成;
           };

            /*无参数命令*/
            var jdforever = new JobDetail(new CPCmdTaskManager("AA 52 02 0000 00 00 0000 BB", "BB 02 $4 0000 00 AA", 10, 0), false, 3, 100);//一直执行 每次重试1次  执行间隔800ms

            var jd1 = new JobDetail(new CPCmdTaskManager("AA1101000409000101", "AA110100[]0900{}", 2000, 1));

            var jd2 = new JobDetail(new CPCmdTaskManager("AB2201000409000101", "xxxxxxxxxxxxxxxxxx", 2000, 2), false);//执行1次 每次重试2次  不中断后续

            var jd3 = new JobDetail(new CPCmdTaskManager("AC3301000409000101", "xxxxxxxxxxxxxxxxxx", 2000, 3), true, 2);//执行1+2次 每次重试3次 ; 中断不影响重复执行,但会中断后续执行

            var jd4 = new JobDetail(new CPCmdTaskManager("AD4401000409000101", "AD440100[]0900{}", 2000, 1));

            sch.AddButch((jd) =>
            {
                jd.UseExecHandler(ExecHandler).UseCallback(RespHandler);
            }
            //, jd1
            //, jd2
            //, jd3
            //, jd4

            , jdforever
            );
        }

        public void SetJobs(params JobDetail[] jobDetails)
        {
            for (int i = 0; i < jobDetails.Length; i++)
            {
                sch.Jobs.Enqueue(jobDetails[i]);
            }
        }
    }
}