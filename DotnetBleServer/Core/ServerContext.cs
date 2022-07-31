using System;
using System.Threading.Tasks;
using HashtagChris.DotNetBlueZ;
using Tmds.DBus;

namespace DotnetBleServer.Core
{
    public class ServerContext : IDisposable
    {
        public Adapter Adapter { get; }

        public ServerContext(Adapter adapter)
        {
            Adapter = adapter;
            Connection = new Connection(Address.System);
        }

        public async Task Connect()
        {
            await Connection.ConnectAsync();
        }

        public Connection Connection { get; }

        public void Dispose()
        {
            Connection.Dispose();
        }
    }
}