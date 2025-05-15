using src.DeviceManager.Exceptions;

namespace src.DeviceManager.Models;

public class SmartWatch : Device, IPowerNotifier
{
    public int Id { get; set; }
    
    private int _batteryCharge;

    public int BatteryCharge
    {
        get => _batteryCharge;
        set
        {
            if (value < 0 || value > 100)
                throw new ArgumentOutOfRangeException(nameof(BatteryCharge), 
                    "The battery range is from 0 to 100");

            _batteryCharge = value;

            if (_batteryCharge < 20) NotifyLowPower();
        }
    }
    
    public string Device_Id { get; set; }
    
    public byte[] RowVersion { get; set; }
    
   
    public SmartWatch(int id, string name, bool isOn, int batteryCharge, string deviceId) : base(deviceId, name, isOn)
    {
        Id = id;
        _batteryCharge = batteryCharge;
        Device_Id = deviceId;
    }
    
    public SmartWatch() : base("", "", false) { }
    
   
    public override void TurnOn()
    {
        if (_batteryCharge < 11)
            throw new LowBatteryException();
        
        base.TurnOn();
        _batteryCharge -= 10;
    }
    
    public void NotifyLowPower()
    {
        Console.WriteLine("Watch has low power");
    }
    
    public override string ToString()
    {
        return $"{base.ToString()}, battery: {_batteryCharge}";
    }
}
