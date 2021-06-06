using Microsoft.Extensions.Configuration;

namespace WebAPI
{
    public static class ServerConfiguration
    {
        public static IConfiguration Configuration { get; set; }
        public static string ConnectionString
        {
            get { return Configuration.GetValue<string>($"ServerConfiguration:ConnectionString"); }
            set { Configuration["ServerConfiguration:ConnectionString"] = value; }
        }
        public static int HistorySize
        {
            get { return Configuration.GetValue<int>($"ServerConfiguration:HistorySize"); }
            set { Configuration["ServerConfiguration:HistorySize"] = value.ToString(); }
        }
    }
}
