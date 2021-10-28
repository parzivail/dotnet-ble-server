using System.Threading.Tasks;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt.BlueZModel;

namespace DotnetBleServer.Gatt
{
    public abstract class CharacteristicSource
    {
        public PropertiesBase<GattCharacteristic1Properties> Properties;
        public abstract Task WriteValueAsync(byte[] value);
        public abstract Task<byte[]> ReadValueAsync();
        public abstract Task StartNotifyAsync();
        public abstract Task StopNotifyAsync();
    }
}