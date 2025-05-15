

using src.DeviceManager.Exceptions;

namespace src.DeviceManager.Models;

public class PersonalComputer : Device
{
    public int Id { get; set; }
    public string OperatingSystem { get; set; }
    
    public string Device_Id { get; set; }
    
    public byte[] RowVersion { get; set; }


    public PersonalComputer(int id, string name, bool isOn, string operatingSystem, string deviceId) : base(deviceId, name, isOn)
    {
        Id = id;
        OperatingSystem = operatingSystem;
        Device_Id = deviceId;
    }
    
    public PersonalComputer() : base("", "", false) { }


    public override void TurnOn()
    {
        if (string.IsNullOrEmpty(OperatingSystem))
            throw new EmptySystemException();
        
        base.TurnOn();
    }
    

    public override string ToString()
    {
        return $"{base.ToString()}, operating system: {OperatingSystem}";
    }
}