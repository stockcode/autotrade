using System;
using System.Reflection;
using System.Windows.Forms;
using Autofac;
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
    }
}