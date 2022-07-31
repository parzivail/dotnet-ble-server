﻿using Tmds.DBus;

namespace DotnetBleServer.Core
{
    public class ServerContext : IDisposable
    {
        public ObjectPath Adapter { get; }

        public ServerContext(ObjectPath adapter)
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