using Autofac;
using Autofac.Features.ResolveAnything;
using BetterStepsRecorder.WPF.Services;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Windows;
using IContainer = Autofac.IContainer;

namespace BetterStepsRecorder.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var builder = new ContainerBuilder();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            builder.RegisterType<ExportService>().As<IExportService>().SingleInstance();
            IContainer container = builder.Build();
            DependecyInjectionExtension.Resolver = container.Resolve;
        }   
    }
}
