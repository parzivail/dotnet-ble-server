using DotnetBleServer.Core;
using DotnetBleServer.Gatt.BlueZModel;
using DotnetBleServer.Gatt.Description;

using Tmds.DBus;

namespace DotnetBleServer.Gatt
{
    public class GattApplicationManager
    {
        private readonly ServerContext _context;

        public GattApplicationManager(ServerContext context)
        {
            _context = context;
        }

        public async Task RegisterGattApplication(ObjectPath path, IEnumerable<GattServiceDescription> gattServiceDescriptions)
        {
            await BuildApplicationTree(path, gattServiceDescriptions);
            await RegisterApplicationInBluez(path);
        }

        private async Task BuildApplicationTree(ObjectPath applicationObjectPath, IEnumerable<GattServiceDescription> gattServiceDescriptions)
        {
            var application = await BuildGattApplication(applicationObjectPath);

            foreach (var serviceDescription in gattServiceDescriptions)
            {
                var service = await AddNewService(application, serviceDescription);

                foreach (var characteristicDescription in serviceDescription.GattCharacteristicDescriptions)
                {
                    var characteristic = await AddNewCharacteristic(service, characteristicDescription);

                    foreach (var descriptorDescription in characteristicDescription.Descriptors)
                        await AddNewDescriptor(characteristic, descriptorDescription);
                }
            }
        }

        private async Task RegisterApplicationInBluez(ObjectPath applicationObjectPath)
        {
            var gattManager = _context.Connection.CreateProxy<IGattManager1>("org.bluez", _context.Adapter);
            await gattManager.RegisterApplicationAsync(applicationObjectPath, new Dictionary<string, object>());
        }

        private async Task<GattApplication> BuildGattApplication(ObjectPath applicationObjectPath)
        {
            var application = new GattApplication(applicationObjectPath);
            await _context.Connection.RegisterObjectAsync(application);
            return application;
        }

        private async Task<GattService> AddNewService(GattApplication application, GattServiceDescription serviceDescription)
        {
            var gattService1Properties = GattPropertiesFactory.CreateGattService(serviceDescription);
            var gattService = application.AddService(gattService1Properties);
            await _context.Connection.RegisterObjectAsync(gattService);
            return gattService;
        }

        private async Task<GattCharacteristic> AddNewCharacteristic(GattService gattService, GattCharacteristicDescription characteristic)
        {
            var gattCharacteristic1Properties = GattPropertiesFactory.CreateGattCharacteristic(characteristic);
            var gattCharacteristic = gattService.AddCharacteristic(gattCharacteristic1Properties, characteristic.CharacteristicSource);
            await _context.Connection.RegisterObjectAsync(gattCharacteristic);
            return gattCharacteristic;
        }

        private async Task AddNewDescriptor(GattCharacteristic gattCharacteristic, GattDescriptorDescription descriptor)
        {
            var gattDescriptor1Properties = GattPropertiesFactory.CreateGattDescriptor(descriptor);
            var gattDescriptor = gattCharacteristic.AddDescriptor(gattDescriptor1Properties);
            await _context.Connection.RegisterObjectAsync(gattDescriptor);
        }
    }
}