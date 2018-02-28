namespace ApiMiddleware
{
    public class SerilogConfig
    {
        public string Hostname { get; set; }
        public int Port { get; set; }
        public string LogFile { get; set; }
        public string Facility { get; set; }
        public string ApplicationName { get; set; }
        public string ModuleName { get; set; }
        public string AppApplicationName { get; set; }
    }
}