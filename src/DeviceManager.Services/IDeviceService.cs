using System.Text.Json.Nodes;
using src.DeviceManager.Models;

namespace src.DeviceManager.Services;

public interface IDeviceService
{
    public IEnumerable<DeviceDTO> GetAllDevices();
    public Device? GetDeviceById(string id);
    public Task<bool> AddDeviceByRawText(string text);
    public Task<bool> AddDeviceByJson(JsonNode? json);
    public Task<bool> DeleteDevice(string id);
    public Task<bool> UpdateDevice(JsonNode? json);
}