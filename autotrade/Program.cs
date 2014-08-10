using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using Autofac;
using CrashReporterDotNET;
using QuantBox.CSharp2CTP.Event;

namespace autotrade
{
    internal static class Program
    {
        /// <summary>
        ///     应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.ThreadException += ApplicationThreadException;

            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var builder = new ContainerBuilder();

            var assembly = Assembly.GetExecutingAssembly();

            builder.RegisterType(typeof (TraderApiWrapper)).SingleInstance();
            builder.RegisterType(typeof(MdApiWrapper)).SingleInstance();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Manager") || t.Name.EndsWith("Repository") ).PropertiesAutowired().SingleInstance();


            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("Strategy")).PropertiesAutowired();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("StopLoss")).PropertiesAutowired();

            builder.RegisterAssemblyTypes(Assembly.GetExecutingAssembly())
                .Where(t => t.Name.EndsWith("StopProfit")).PropertiesAutowired();

            builder.RegisterAssemblyTypes(assembly).AssignableTo<Form>().PropertiesAutowired();
            
            IContainer container = builder.Build();

            MainForm mainForm = (MainForm) container.Resolve(typeof(MainForm));

            mainForm.Container = container;

            Application.Run(mainForm);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs unhandledExceptionEventArgs)
        {
            ReportCrash((Exception)unhandledExceptionEventArgs.ExceptionObject);
            Environment.Exit(0);
        }

        private static void ApplicationThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ReportCrash(e.Exception);
        }

        private static void ReportCrash(Exception exception)
        {
            var reportCrash = new ReportCrash
            {
                FromEmail = "13613803575@139.com",
                ToEmail = "13613803575@139.com",
                SmtpHost = "smtp.139.com",
                Port = 25,
                UserName = "13613803575",
                Password = "gk790624",
                EnableSSL = false,
            };

            reportCrash.Send(exception);
        }
    }
}