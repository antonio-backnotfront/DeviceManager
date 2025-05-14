using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using src.DeviceManager.Models;
using src.DeviceManager.Repositories;
using src.DeviceManager.Services;
using src.DeviceProject.Repository;

namespace src.DeviceManager.Services;

public class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;

    public DeviceService(IDeviceRepository deviceRepository)
    {
        _deviceRepository = deviceRepository;
    }

    public IEnumerable<DeviceDTO> GetAllDevices() => _deviceRepository.GetAllDevices();

    public Device? GetDeviceById(string id) => _deviceRepository.GetDeviceById(id);

    public bool AddDeviceByJson(JsonNode? json)
    {
        var deviceType = json?["deviceType"]?.ToString()?.ToLower();
        if (string.IsNullOrEmpty(deviceType))
            throw new ArgumentException("Invalid JSON format. deviceType is not specified.");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        return deviceType switch
        {
            "sw" => DeserializeAndAddDevice<SmartWatch>(json, options, ValidateSmartWatch, _deviceRepository.AddSmartWatch),
            "pc" => DeserializeAndAddDevice<PersonalComputer>(json, options, ValidatePC, _deviceRepository.AddPersonalComputer),
            "ed" => DeserializeAndAddDevice<EmbeddedDevice>(json, options, ValidateEmbeddedDevice, _deviceRepository.AddEmbeddedDevice),
            _ => throw new ApplicationException("Unknown device type.")
        };
    }

    public bool AddDeviceByRawText(string text)
    {
        var parts = text.Split(',');
        var idPrefix = parts[0].Split('-')[0].ToLower();

        return idPrefix switch
        {
            "sw" => AddSmartWatchFromText(parts),
            "p" => AddPCFromText(parts),
            "ed" => AddEmbeddedDeviceFromText(parts),
            _ => throw new ArgumentException("Unknown device.")
        };
    }

    public bool UpdateDevice(JsonNode? json)
    {
        var id = json?["device_id"]?.ToString();
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Invalid or not specified id.");

        if (GetDeviceById(id) == null)
            throw new FileNotFoundException("Device not found.");

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

        if (id.Contains("SW"))
            return DeserializeAndUpdateDevice<SmartWatch>(json, options, ValidateSmartWatch, _deviceRepository.UpdateSmartWatch);
        else if (id.Contains("P"))
            return DeserializeAndUpdateDevice<PersonalComputer>(json, options, ValidatePC, _deviceRepository.UpdatePersonalComputer);
        else if (id.Contains("ED"))
            return DeserializeAndUpdateDevice<EmbeddedDevice>(json, options, ValidateEmbeddedDevice, _deviceRepository.UpdateEmbeddedDevice);

        throw new ApplicationException("Unknown device type.");
    }

    public bool DeleteDevice(string id)
    {
        if (GetDeviceById(id) == null)
            throw new FileNotFoundException("Device not found.");

        if (id.Contains("SW")) _deviceRepository.DeleteWatch(id);
        else if (id.Contains("P")) _deviceRepository.DeleteComputer(id);
        else if (id.Contains("ED")) _deviceRepository.DeleteEmbeddedDevice(id);
        else throw new ApplicationException("Unknown device type.");

        return true;
    }

    private static bool DeserializeAndAddDevice<T>(JsonNode? json, JsonSerializerOptions options, Action<T> validator, Action<T> repositoryAdd) where T : class
    {
        T? device;
        try { device = JsonSerializer.Deserialize<T>(json, options); }
        catch { throw new ArgumentException("JSON deserialization failed. Seek help."); }

        if (device == null)
            throw new ArgumentException("JSON deserialization failed. Seek help.");

        validator(device);
        repositoryAdd(device);
        return true;
    }

    private static bool DeserializeAndUpdateDevice<T>(JsonNode? json, JsonSerializerOptions options, Action<T> validator, Action<T> repositoryUpdate) where T : class
    {
        T? device;
        try { device = JsonSerializer.Deserialize<T>(json, options); }
        catch { throw new ArgumentException("JSON deserialization failed. Seek help."); }

        if (device == null)
            throw new ArgumentException("JSON deserialization failed. Seek help.");

        validator(device);
        repositoryUpdate(device);
        return true;
    }

    private static void ValidateSmartWatch(SmartWatch watch)
    {
        if (watch.BatteryCharge is < 0 or > 100)
            throw new ArgumentException("Battery charge is out of range [0 - 100].");
    }

    private static void ValidatePC(PersonalComputer pc)
    {
        if (pc.IsOn && pc.OperatingSystem.IsNullOrEmpty())
            throw new ArgumentException("PC cannot be turned on without operating system.");
    }

    private static void ValidateEmbeddedDevice(EmbeddedDevice ed)
    {
        if (!Regex.IsMatch(ed.IpAddress, @"^((25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)\.){3}(25[0-5]|2[0-4]\d|1\d{2}|[1-9]?\d)$"))
            throw new ArgumentException("IP address is not a valid IP address.");

        if (!ed.IsOn && ed.IsConnected)
            throw new ArgumentException("Device cannot be connected if it is turned off.");

        if (ed.IsConnected && !ed.NetworkName.Contains("MD Ltd."))
            throw new ArgumentException("The network name should contain \"MD Ltd.\" for the device to be able to be connected.");
    }

    private bool AddSmartWatchFromText(string[] parts)
    {
        var watch = new SmartWatch
        {
            Device_Id = parts[0],
            Name = parts[1],
            IsOn = bool.TryParse(parts[2], out var isOn) ? isOn : throw new ArgumentException("Invalid boolean value for IsOn parameter."),
            BatteryCharge = int.TryParse(parts[3].Replace("%", ""), out var battery) ? battery : throw new ArgumentException("Invalid int value for BatteryCharge parameter.")
        };

        ValidateSmartWatch(watch);
        _deviceRepository.AddSmartWatch(watch);
        return true;
    }

    private bool AddPCFromText(string[] parts)
    {
        var pc = new PersonalComputer
        {
            Device_Id = parts[0],
            Name = parts[1],
            IsOn = bool.TryParse(parts[2], out var isOn) ? isOn : throw new ArgumentException("Invalid boolean value for IsOn parameter."),
            OperatingSystem = parts.Length > 3 ? parts[3] : ""
        };

        ValidatePC(pc);
        _deviceRepository.AddPersonalComputer(pc);
        return true;
    }

    private bool AddEmbeddedDeviceFromText(string[] parts)
    {
        var ed = new EmbeddedDevice
        {
            Device_Id = parts[0],
            Name = parts[1],
            IpAddress = parts[2],
            NetworkName = parts[3],
            IsOn = false,
            IsConnected = false
        };

        ValidateEmbeddedDevice(ed);
        _deviceRepository.AddEmbeddedDevice(ed);
        return true;
    }
}
