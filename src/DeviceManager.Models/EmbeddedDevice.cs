using System.Text.RegularExpressions;
using src.DeviceManager.Exceptions;

namespace src.DeviceManager.Models;


public class EmbeddedDevice : Device
{
    
    public int Id { get; set; }
 
    public string IpAddress { get; set; }
    
    public string NetworkName { get; set;  }


    public bool IsConnected { get; set; }
    
    public string Device_Id { get; set; }
    
  
    public byte[] RowVersion { get; set; }

    
    public EmbeddedDevice(int id, string name, bool isOn, string ipAddress, string networkName, string deviceId) : base(deviceId, name, isOn)
    {
        SetIpAddress(ipAddress);
        NetworkName = networkName;
        IpAddress = ipAddress;
        IsConnected = false;
        Id = id;
        Device_Id = deviceId;
    }
    
    public EmbeddedDevice() : base("", "", false) { }

   
    public void SetIpAddress(string ip)
    {
        if (!Regex.IsMatch(ip, @"^\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$"))
            throw new IpAddressException();
        
        IpAddress = ip;
    }


    public void Connect()
    {
        if (!NetworkName.Contains("MD Ltd."))
            throw new ConnectionException();
        
        IsConnected = true;
        Console.WriteLine($"{Name} connected successfully.");
    }

    public void Disconnect()
    {
        if (!IsConnected)
        {
            Console.WriteLine($"{Name} is already disconnected.");
            return;
        } 
        IsConnected = false;
        Console.WriteLine($"{Name} was disconnected.");
    }
    

    public override void TurnOn()
    {
        Connect();
        base.TurnOn();
    }

    public override void TurnOff()
    {
        Disconnect();
        base.TurnOff();
    }


    public override string ToString()
    {
        return $"{base.ToString()}, ip address: {IpAddress}, network: {NetworkName}";
    }
}