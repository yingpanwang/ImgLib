using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ImgLib.UI.Services;
using ImgLib.UI.ViewModels;
using ImgLib.UI.ViewModels.Design;
using Microsoft.Extensions.DependencyInjection;

namespace ImgLib.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow();

                var provider = ConfigureServices(desktop);
                var mvm = provider.GetRequiredService<MainWindowViewModel>();
                mvm.ParentWindow = desktop.MainWindow;

                // 初始化全局 Toast 服务
                ToastService.Initialize(provider.GetRequiredService<ToastViewModel>());

                // mvm.ImgListBoxViewModel.PropertyChanged += (s, e) =>
                // {
                //     if (e.PropertyName == nameof(mvm.ImgListBoxViewModel.SelectedImgItem))
                //     {
                //         if (mvm.ImgListBoxViewModel.SelectedImgItem != null)
                //         {
                //             mvm.WatermarkDesignViewModel.Reset();
                //             mvm.WatermarkDesignViewModel.PreviewFilePath = mvm.ImgListBoxViewModel.SelectedImgItem.FilePath;
                //         }
                //     }
                // };

                desktop.MainWindow.DataContext = mvm;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private static IServiceProvider ConfigureServices(IClassicDesktopStyleApplicationLifetime desktop)
        {
            var services = new ServiceCollection();

            // 注册 IStorageProvider（从主窗口的 TopLevel 获取）
            var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
            services.AddSingleton(topLevel!.StorageProvider);

            // ── 单例 ViewModels ──
            services.AddSingleton<MainWindowViewModel>();
            services.AddSingleton<ImgListBoxViewModel>();

            services.AddSingleton<ToastViewModel>();
            services.AddSingleton<ExportProgressViewModel>();
            services.AddSingleton<WatermarkDesignViewModel>();
            services.AddSingleton<HistogramViewModel>();
            services.AddSingleton<SystemSettingsViewModel>(_ => SystemSettingsService.Current);
            services.AddSingleton<PreviewSettingsViewModel>(sp =>
                sp.GetRequiredService<SystemSettingsViewModel>().PreviewSettings);

            // ── 瞬态 ViewModels ──
            services.AddTransient<WatermarkSettingsViewModel>();
            services.AddTransient<SettingsWindowViewModel>();
            services.AddTransient<ExportDialogViewModel>();
            services.AddTransient<OpenFolderDialogViewModel>();

            return services.BuildServiceProvider();
        }
    }
}