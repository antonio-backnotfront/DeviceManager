// using DeviceProject.DeviceProject.Models;
// using src.DeviceProject.Models.devices;
using src.DeviceManager.Models;

namespace src.DeviceProject.Repository;

public interface IDeviceRepository
{
    public IEnumerable<DeviceDTO> GetAllDevices();
    public Device? GetDeviceById(string id);
    public void AddPersonalComputer(PersonalComputer personalComputer);
    public void AddSmartWatch(SmartWatch smartWatch);
    public void UpdateSmartWatch(SmartWatch smartWatch);
    public void AddEmbeddedDevice(EmbeddedDevice embeddedDevice);
    public void UpdateEmbeddedDevice(EmbeddedDevice embeddedDevice);
    public void UpdatePersonalComputer(PersonalComputer personalComputer);
    public void DeleteComputer(string id);
    public void DeleteWatch(string id);
    public void DeleteEmbeddedDevice(string id);

}