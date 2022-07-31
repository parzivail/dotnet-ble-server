namespace DotnetBleServer.Gatt;

public class ValueBackedCharacteristicSource : ICharacteristicSource
{
    public delegate void ValueSetEvent(byte[] value, bool response, bool didNofify);

    public event ValueSetEvent ValueSet;

    public delegate void CallbackEvent();

    public event CallbackEvent ClientSubscribed;
    public event CallbackEvent ClientUnsubscribed;
    public event CallbackEvent Confirmed;

    public async Task SetData(byte[] value, bool response, bool notify = true)
    {
        if (notify)
            await Properties.SetAsync("Value", value);
        else
        {
            var props = await Properties.GetAllAsync();
            props.Value = value;
        }

        ValueSet.Invoke(value, response, notify);
    }

    public override async Task WriteValueAsync(byte[] value, bool response)
    {
        await SetData(value, response);
    }

    public override Task<byte[]> ReadValueAsync()
    {
        return Properties.GetAsync<byte[]>("Value");
    }

    public override async Task StartNotifyAsync()
    {
        var props = await Properties.GetAllAsync();
        props.Notifying = true;
        ClientSubscribed.Invoke();
    }

    public override async Task StopNotifyAsync()
    {
        var props = await Properties.GetAllAsync();
        props.Notifying = false;
        ClientUnsubscribed.Invoke();
    }

    public override async Task ConfirmAsync()
    {
        Confirmed.Invoke();
    }
}