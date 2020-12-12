using ReadyBE.AutomaticLight;
using ReadyBE.Mobo;
using ReadyBE.Serial;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace ReadyBE
{
    class Program
    {
        static void Main(string[] args) //static async Task Main(string[] args)
        {
            /*
            var hostBuilder = new HostBuilder()
            .ConfigureAppConfiguration((hostContext, configBuilder) =>
            {
            })
            .ConfigureLogging((hostContext, configLogging) =>
            {
            })
            .ConfigureServices((hostContext, services) =>
            {
                Ready.ApplicationRun();
            });

            await hostBuilder.RunConsoleAsync();
            */

            Ready.ApplicationRun();
        }
        
    }
}
