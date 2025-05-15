// using DeviceProject.DeviceProject.Models;
// using src.DeviceProject.Models.devices;
using src.DeviceManager.Models;

namespace src.DeviceProject.Repository;

public interface IDeviceRepository
{
    public IEnumerable<DeviceDTO> GetAllDevices();
    public Device? GetDeviceById(string id);
    public Task AddPersonalComputer(PersonalComputer personalComputer);
    public Task AddSmartWatch(SmartWatch smartWatch);
    public Task UpdateSmartWatch(SmartWatch smartWatch);
    public Task AddEmbeddedDevice(EmbeddedDevice embeddedDevice);
    public Task UpdateEmbeddedDevice(EmbeddedDevice embeddedDevice);
    public Task UpdatePersonalComputer(PersonalComputer personalComputer);
    public Task DeleteComputer(string id);
    public Task DeleteWatch(string id);
    public Task DeleteEmbeddedDevice(string id);

}