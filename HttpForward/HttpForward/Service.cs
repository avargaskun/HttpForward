using System;
using System.Configuration;
using System.Configuration.Install;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;

namespace HttpForward
{
    public partial class Service : ServiceBase
    {
        private Listener listener;

        public Service()
        {
            this.InitializeComponent();
        }

        public static void Install()
        {
            ManagedInstallerClass.InstallHelper(new[] { Assembly.GetExecutingAssembly().Location });
        }

        public static void Uninstall()
        {
            ManagedInstallerClass.InstallHelper(new[] { "/u", Assembly.GetExecutingAssembly().Location });
        }

        public void RunLocal(string[] args)
        {
            this.OnStart(args);
            Console.WriteLine("Press a key to exit ...");
            Console.ReadKey();
            this.OnStop();
        }

        protected override void OnStart(string[] args)
        {
            Log.EventLog = this.eventLog;
            Log.Info("HttpForward service starting ...");

            try
            {
                var prefix = ConfigurationManager.AppSettings["listeningPrefix"];
                var address = ConfigurationManager.AppSettings["forwardingAddress"];
                var ignoreSslErrors = bool.Parse(ConfigurationManager.AppSettings["ignoreSslErrors"] ?? "false");
                this.listener = new Listener(prefix, address, ignoreSslErrors);
                this.listener.Authorization = ConfigurationManager.AppSettings["authorization"];
                this.listener.StartListening();
            }
            catch (Exception e)
            {
                Log.Error($"Could not start service with {e.GetType().Name}: {e.Message}");
                throw;
            }
        }

        protected override void OnStop()
        {
            this.listener.StopListening();
            this.listener.Dispose();
            Log.Info("HttpForward service stopping ...");
        }
    }
}
