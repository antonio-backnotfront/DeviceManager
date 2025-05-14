using System.Text.Json.Nodes;
using src.DeviceManager.Models;

namespace src.DeviceManager.Services;

public interface IDeviceService
{
    public IEnumerable<DeviceDTO> GetAllDevices();
    public Device? GetDeviceById(string id);
    public bool AddDeviceByRawText(string text);
    public bool AddDeviceByJson(JsonNode? json);
    public bool DeleteDevice(string id);
    public bool UpdateDevice(JsonNode? json);
}