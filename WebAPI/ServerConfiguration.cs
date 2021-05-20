using Microsoft.Extensions.Configuration;

namespace WebAPI
{
    public static class ServerConfiguration
    {
        public static IConfiguration Configuration { get; set; }
        public static string AddCode
        {
            get { return Configuration.GetValue<string>($"ServerConfiguration:AddCode"); }
            set { Configuration["ServerConfiguration:AddCode"] = value; }
        }
        public static string ConnectionString
        {
            get { return Configuration.GetValue<string>($"ServerConfiguration:ConnectionString"); }
            set { Configuration["ServerConfiguration:ConnectionString"] = value; }
        }
    }
}
