using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotnetBleServer.Core;
using Tmds.DBus;

namespace DotnetBleServer.Advertisements
{
    public class AdvertisingManager
    {
        private readonly ServerContext _context;

        public AdvertisingManager(ServerContext context)
        {
            _context = context;
        }

        public async Task RegisterAdvertisement(Advertisement advertisement, ObjectPath device)
        {
            await _context.Connection.RegisterObjectAsync(advertisement);
            Console.WriteLine($"advertisement object {advertisement.ObjectPath} created");

            await GetAdvertisingManager(device).RegisterAdvertisementAsync(((IDBusObject) advertisement).ObjectPath,
                new Dictionary<string, object>());

            Console.WriteLine($"advertisement {advertisement.ObjectPath} registered in BlueZ advertising manager");
        }

        private ILEAdvertisingManager1 GetAdvertisingManager(ObjectPath device)
        {
            return _context.Connection.CreateProxy<ILEAdvertisingManager1>("org.bluez", device);
        }

        public async Task CreateAdvertisement(ObjectPath path, AdvertisementProperties advertisementProperties)
        {
            var advertisement = new Advertisement(path, advertisementProperties);
            await RegisterAdvertisement(advertisement, _context.Adapter);
        }
    }
}