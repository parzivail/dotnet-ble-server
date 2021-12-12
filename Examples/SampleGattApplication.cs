using System;
using System.Text;
using System.Threading.Tasks;
using DotnetBleServer.Core;
using DotnetBleServer.Gatt;
using DotnetBleServer.Gatt.Description;

namespace Examples
{
    internal class SampleGattApplication
    {
        public static async Task RegisterGattApplication(ServerContext serverContext)
        {
            var gattServiceDescription = new GattServiceDescription
            {
                UUID = "12345678-1234-5678-1234-56789abcdef0",
                Primary = true
            };

            var gattCharacteristicDescription = new GattCharacteristicDescription
            {
                CharacteristicSource = new ExampleCharacteristicSource(),
                UUID = "12345678-1234-5678-1234-56789abcdef1",
                Flags = CharacteristicFlags.Read | CharacteristicFlags.Write | CharacteristicFlags.WritableAuxiliaries
            };
            var gattDescriptorDescription = new GattDescriptorDescription
            {
                Value = new[] {(byte) 't'},
                UUID = "12345678-1234-5678-1234-56789abcdef2",
                Flags = new[] {"read", "write"}
            };
            var gab = new GattApplicationBuilder();
            gab
                .AddService(gattServiceDescription)
                .WithCharacteristic(gattCharacteristicDescription, new[] {gattDescriptorDescription});

            await new GattApplicationManager(serverContext).RegisterGattApplication(gab.BuildServiceDescriptions());
        }

        internal class ExampleCharacteristicSource : ICharacteristicSource
        {
            const bool NotificationAvoiding = false;
            public override Task WriteValueAsync(byte[] value, bool response)
            {
                // The client sent us some data!

                Console.WriteLine(Encoding.ASCII.GetChars(value));

                if (NotificationAvoiding)
                {
                    // We get our properties, so that we can directly write the value
                    var props = Properties.GetAllAsync().Result;

                    Console.WriteLine("Writing value");
                    // Assign the value directly, since we don't want to notify the client of what it sent us - it should probably know that!
                    props.Value = value;
                    return Task.CompletedTask;
                }

                Console.WriteLine("Writing value");
                // Let's write it to our characteristic's value! (This will trigger a notification, if they're turned on)
                return Properties.SetAsync("Value", value);
            }

            public override Task<byte[]> ReadValueAsync()
            {
                // The client asked for our value
                Console.WriteLine("Reading value");

                // Get our value, in the form of a byte array task
                var props = Properties.GetAsync<byte[]>("Value");

                // Return our task, mission accomplished!
                return props;
            }

            public override Task StartNotifyAsync()
            {
                // Our client has requested notifications!
                Console.WriteLine("Starting to Notify");

                // Get our properties class
                var props = Properties.GetAllAsync().Result;
                // Set notifying to true, so we'll start sending replies on Properties.SetAsync("Value", [value])
                props.Notifying = true;
                // Return something, since we have to.
                return Task.CompletedTask;
            }

            public override Task StopNotifyAsync()
            {
                // Our client has asked us to stop talking to it :(
                Console.WriteLine("Stopping notifications..");

                // Get our properties class again
                var props = Properties.GetAllAsync().Result;
                // Set notifying to false. Now Properties.SetAsync("Value", [value]) won't send notifications anymore.
                props.Notifying = false;
                // Return a completed task so we can leave.
                return Task.CompletedTask;
            }

            public override Task ConfirmAsync()
            {
                // The client told us that it got the packet. That's very polite!
                // Indicate characteristics will have their notifications confirmed by the client, which will trigger this function.
                return Task.CompletedTask;
            }
        }


        internal class ReplyCharacteristicSource : ICharacteristicSource
        {
            const bool Reply = true;
            public override Task WriteValueAsync(byte[] value, bool response)
            {
                // The client sent us some data!

                // If you have a client that actually writes "request" in the optional data, you can use this, but my experience is that you either have a "write" (write with reply) or
                // a "write-without-response" characteristic, so it seems you have just a default course of action. I've also not had a client use the options (Gatt.BlueZModel.GattCharacteristic.cs#30)
                // to say "request" ( https://git.kernel.org/pub/scm/bluetooth/bluez.git/tree/doc/gatt-api.txt#n99 ), so I'm not sure they exist?
                if (response || Reply)
                {
                    // We set our reply as our value, so that the notification will trigger
                    // (assuming notifications are turned on, but we could also move this down below the assignment of props to check if necessary)
                    Properties.SetAsync("Value", Encoding.ASCII.GetBytes("Our Reply!")).Wait();
                }

                // We get our properties, so that we can directly write the value
                var props = Properties.GetAllAsync().Result;

                Console.WriteLine("Writing value");
                // Assign the value directly, since we don't want to notify the client of what it sent us - it should probably know that!
                props.Value = value;
                Console.WriteLine(Encoding.ASCII.GetChars(value));

                // Gotta return a task, and we already awaited the other task, so completed task it is!
                return Task.CompletedTask;
            }

            public override Task<byte[]> ReadValueAsync()
            {
                // The client asked for our value
                Console.WriteLine("Reading value");

                // Get our value, in the form of a byte array task
                var props = Properties.GetAsync<byte[]>("Value");

                // Return our task, mission accomplished!
                return props;
            }

            public override Task StartNotifyAsync()
            {
                // Our client has requested notifications!
                Console.WriteLine("Starting to Notify");

                // Get our properties class
                var props = Properties.GetAllAsync().Result;
                // Set notifying to true, so we'll start sending replies on Properties.SetAsync("Value", [value])
                props.Notifying = true;
                // Return something, since we have to.
                return Task.CompletedTask;
            }

            public override Task StopNotifyAsync()
            {
                // Our client has asked us to stop talking to it :(
                Console.WriteLine("Stopping notifications..");

                // Get our properties class again
                var props = Properties.GetAllAsync().Result;
                // Set notifying to false. Now Properties.SetAsync("Value", [value]) won't send notifications anymore.
                props.Notifying = false;
                // Return a completed task so we can leave.
                return Task.CompletedTask;
            }

            public override Task ConfirmAsync()
            {
                // The client told us that it got the packet. That's very polite!
                // Indicate characteristics will have their notifications confirmed by the client, which will trigger this function.
                return Task.CompletedTask;
            }
        }
    }
}