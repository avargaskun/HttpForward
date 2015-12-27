using System.ServiceProcess;

namespace HttpForward
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            var options = new Options();
            CommandLine.Parser.Default.ParseArgumentsStrict(args, options);

            if (options.Install)
            {
                Service.Install();
                return;
            }
            
            if (options.Uninstall)
            {
                Service.Uninstall();
                return;
            }

            if (options.Local)
            {
                new Service().RunLocal(args);
                return;
            }

            ServiceBase.Run(new Service());
        }
    }
}
