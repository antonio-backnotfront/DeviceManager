namespace src.DeviceManager.Models;


public abstract class Device
{
    
    public string Id {get; set;}
    

    public string Name {get; set;}
    
    public bool IsOn {get; set;}
    
    public byte[] DeviceRowVersion { get; set; }
    
    protected Device(string id, string name, bool isOn)
    {
        Id = id;
        Name = name;
        IsOn = isOn;
    }
    
    public virtual void TurnOn() => IsOn = true;
    
    public virtual void TurnOff() => IsOn = false;
    
    public override string ToString()
    {
        return $"Device {Id}: {Name}, Is turned on: {IsOn}";
    }
}