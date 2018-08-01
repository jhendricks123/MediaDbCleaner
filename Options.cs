using CommandLine;

namespace MediaDbCleaner
{
    internal class Options
    {
        [Option('p', "path", Required = true, HelpText = "Path to the root of the media database (the folder normally containing cache.xml and archives_cache.xml)")]
        public string BankPath { get; set; }

        [Option('r', "report", HelpText = "Save a report of media database tables identified as being in an invalid state (missing important files/folders) to the specified path")]
        public string ReportPath { get; set; }

        [Option('l', "listonly", HelpText = "Log information about the state of the media database but do not make any modifications to files or folders")]
        public bool ListOnly { get; set; }
    }
}