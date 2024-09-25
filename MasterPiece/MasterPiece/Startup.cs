using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(MasterPiece.Startup))]

namespace MasterPiece
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Map the SignalR service
            app.MapSignalR();
        }
    }
}