using ReactiveUI;
using Splat;
using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Windows.Forms;

namespace DeviceTester
{
    public class RxUILoader
    {
        private static readonly Lazy<RxUILoader> lazy = new Lazy<RxUILoader>(() => new RxUILoader());

        public static RxUILoader Instance => lazy.Value;

        private RxUILoader()
        {
            ConfigureServices();
            RxApp.DefaultExceptionHandler = new GlobalErrorHandler();
        }

        private void ConfigureServices()
        {
            Locator.CurrentMutable.RegisterViewsForViewModels(Assembly.GetCallingAssembly());
        }

        /// <summary>
        /// 使用前注册  Locator.CurrentMutable.RegisterViewsForViewModels 及服务
        /// </summary>
        /// <param name="viewmodel"></param>
        public void ShowForm(Type viewmodel)
        {
            // 创建ViewModel
            var viewModel = Activator.CreateInstance(viewmodel) as ReactiveObject;
            // 解析ViewModel的视图
            var view = ViewLocator.Current.ResolveView(viewModel);
            view.ViewModel = viewModel;
            // 运行应用程序
            var f = (Form)view;
            f.ShowDialog();
        }
    }

    public class GlobalErrorHandler : IObserver<Exception>
    {
#if DEBUG
        private bool IsDebug = true;
#else
        private bool IsDebug = false;
#endif

        public void OnError(Exception error)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
            else if (IsDebug)
                Debugger.Launch();

            RxApp.MainThreadScheduler.Schedule(() =>
            {
                //throw error;
                MessageBox.Show(error.Message);
            });
        }

        public void OnNext(Exception error)
        {
            if (Debugger.IsAttached)
                Debugger.Break();

            RxApp.MainThreadScheduler.Schedule(() =>
            {
                //throw error;
                MessageBox.Show(error.Message);
            });
        }

        public void OnCompleted()
        {
        }
    }
}