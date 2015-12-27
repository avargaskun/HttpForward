using CommandLine;

namespace HttpForward
{
    public class Options
    {
        [Option('i', "install", MutuallyExclusiveSet = "installer", HelpText = "Installs SendVideo as a Windows Service.")]
        public bool Install { get; set; }

        [Option('u', "uninstall", MutuallyExclusiveSet = "installer", HelpText = "Uninstalls the SendVideo Windows Service.")]
        public bool Uninstall { get; set; }

        [Option('l', "local", HelpText = "Run locally (not as a service).")]
        public bool Local { get; set; }
    }
}
