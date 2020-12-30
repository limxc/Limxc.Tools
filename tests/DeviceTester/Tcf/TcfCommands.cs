namespace DeviceTester.Tcf
{
    public static class TcfCommands
    {
        /// <summary>
        /// 公司名+仪器型号+序列号；确认连接
        /// dhy,dba310,xxxxxxxx\n
        /// </summary>
        public static string QueryId() => "*idn?\n";

        /// <summary>
        /// 查询重量
        /// wei=xxx.x\n
        /// </summary>
        public static string QueryWeight() => "*wei?\n";

        /// <summary>
        /// 查询当前步骤 1、待机测量体重 2、用户信息输入  3、测量中  4、测量完成
        /// stp=x\n
        /// </summary>
        public static string QueryStep() => "*stp?\n";

        /// <summary>
        /// 公司名+仪器型号+序列号；确认连接
        /// ok\n
        /// </summary>
        /// <param name="id"></param>
        public static string SendId(string id) => $"*id={id}\n";

        /// <summary>
        /// 发送身高
        /// ok\n
        /// </summary>
        /// <param name="value"></param>
        public static string SendHeight(double value) => $"*hei={value:000.0}\n";

        /// <summary>
        /// 发送年龄
        /// ok\n
        /// </summary>
        /// <param name="value"></param>
        public static string SendAge(int value) => $"*age={value:00}\n";

        /// <summary>
        /// 发送性别 0女 1男
        /// ok\n
        /// </summary>
        /// <param name="gender"></param>
        public static string SendGender(string gender) => $"*gen={(gender == "男" ? 1 : 0)}\n";

        /// <summary>
        /// 开始测量
        /// ok\n
        /// </summary>
        public static string Measure() => "*mea\n";

        /// <summary>
        /// 查询最后一次错误 1.采集板通信错误 2.输入信息错误 3.接触阻抗异常
        /// *err=x\n  ok\n
        /// </summary>
        public static string QueryLastError() => "*err?\n";

        /// <summary>
        /// 返回上一步
        /// ok\n
        /// </summary>
        public static string Return() => "ok\n";

        /// <summary>
        /// 返回首页
        /// ok\n
        /// </summary>
        public static string ReturnHome() => "*ret\n";

        /// <summary>
        /// 读取测量结果
        /// measing\n 或 结果
        /// </summary>
        /*
            id=xxx;
            date=xxx;
            gen=x;
            age=xx;
            hei=xxx;
            wei=xxx.x;
            tbw=xxx.x; 总体水
            pro=xxx.x; 蛋白质
            mus=xxx.x; 肌肉量
            min=xxx.x; 矿物质
            ffm=xxx.x; 去脂体重
            fat=xxx.x; 脂肪
            bmi=xxx.x; 身体质量指数
            pbf=xxx.x; 身体脂肪率
            vfa=xxx.x; 内脏脂肪面积
            whr=xxx.x; 腰臀比
            smm=xxx.x; 骨骼肌
            larmm=xxx.x; 左手肌肉量
            rarmm=xxx.x; 右手肌肉量
            trm=xxx.x; 躯干肌肉量
            llegm=xxx.x; 左腿肌肉量
            rlegm=xxx.x; 右腿肌肉量
            score=xxx.x; 总评分 1、低脂肪低体重 2、低脂肪肌肉型 3、运动员型 4、低体重 5、标准体型 6、超重肌肉型 7、隐形肥胖 8、脂肪过量 9、肥胖

            shape=x; 体型
            tarwei=xxx.x; 目标体重
            ctrwei=xxx.x; 体重控制
            ctrmus=xxx.x; 肌肉控制
            ctrfat=xxx.x; 脂肪控制
            bmr=xxx.x; 基础代谢量
            cday=xxxx.x; 每日所需热量
            evalpro=x; 蛋白质评估 1、正常；2、不足
            evalmin=x; 无机盐评估 1、正常；2、不足
            evalfat=x; 脂肪评估 1、正常；2、不足；3、超标
            evalarme=x; 上肢均衡 1、均衡；2、不均衡
            evallege=x; 下肢均衡 1、均衡；2、不均衡
            evalarmm=x; 上肢发达 1、正常；2、不足；3、发达
            evallegm=x; 下肢发达 1、正常；2、不足；3、发达
            maxwei=xxx.x; 体重范围
            minwei=xxx.x;
            stdwei=xxx.x;
            maxfat=xxx.x; 脂肪范围
            minfat=xxx.x;
            stdfat=xxx.x;
            maxffm=xxx.x; 去脂体重范围
            minffm=xxx.x;
            stdffm=xxx.x;
            maxtbw=xxx.x; 总体水范围
            mintbw=xxx.x;
            stdtbw=xxx.x;
            maxmin=xxx.x; 矿物质范围
            minmin=xxx.x;
            stdmin=xxx.x;
            maxbmi=xxx.x; 身体质量指数范围
            minbmi=xxx.x;
            stdbmi=xxx.x;
            maxvfa=xxx.x; 内脏脂肪面积范围
            minvfa=xxx.x;
            stdvfa=xxx.x;
            maxwhr=xxx.x; 腰臀比范围
            minwhr=xxx.x;
            stdwhr=xxx.x;
            maxpro=xxx.x; 蛋白质范围
            minpro=xxx.x;
            stdpro=xxx.x;
            maxarmm=xxx.x; 上肢肌肉范围
            minarmm=xxx.x;
            stdarmm=xxx.x;
            maxlegm=xxx.x; 下肢肌肉范围
            minlegm=xxx.x;
            stdlegm=xxx.x;
            maxtrm=xxx.x; 躯干肌肉范围
            mintrm=xxx.x;
            stdtrm=xxx.x;
            maxpbf=xxx.x; 身体脂肪率范围
            minpbf=xxx.x;
            stdpbf=xxx.x;
            maxsmm=xxx.x; 骨骼肌范围
            minsmm=xxx.x;
            stdsmm=xxx.x;
            ra5k = xxx.x; 阻抗数据
            la5k = xxx.x;
            tr5k = xxx.x;
            rl5k = xxx.x;
            ll5k = xxx.x;
            ra50k = xxx.x;
            la50k = xxx.x;
            tr50k = xxx.x;
            rl50k = xxx.x;
            ll50k = xxx.x;
            ra250k =xxx.x;
            la250k =xxx.x;
            tr250k =xxx.x;
            rl250k =xxx.x;
            ll250k =xxx.x;
            over\n 结束
         */

        public static string Read() => "*read?\n";
    }
}