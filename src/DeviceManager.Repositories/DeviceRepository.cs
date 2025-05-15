using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using src.DeviceManager.Models;
using src.DeviceManager.Repositories;
using src.DeviceProject.Repository;


namespace src.DeviceManager.Repositories;

public class DeviceRepository : IDeviceRepository
{
    private string _connectionString;

    public DeviceRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public IEnumerable<DeviceDTO> GetAllDevices()
    {
        const string query = "SELECT * FROM Device";
        List<DeviceDTO> devices = new();

        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(query, connection);
        connection.Open();
        using SqlDataReader reader = command.ExecuteReader();
        while (reader.Read())
        {
            devices.Add(new DeviceDTO
            {
                Id = reader.GetString(0),
                Name = reader.GetString(1),
                IsOn = reader.GetBoolean(2)
            });
        }
        return devices;
    }
    
    public Device? GetDeviceById(string id)
    {
        if (string.IsNullOrWhiteSpace(id)) return null;

        string query = id switch
        {
            var s when s.Contains("SW") => "SELECT * FROM Device JOIN SmartWatch ON Device.Id = SmartWatch.Device_id WHERE SmartWatch.Device_Id = @id",
            var s when s.Contains("P") => "SELECT * FROM Device JOIN PersonalComputer ON Device.Id = PersonalComputer.Device_id WHERE PersonalComputer.Device_Id = @id",
            var s when s.Contains("ED") => "SELECT * FROM Device JOIN EmbeddedDevice ON Device.Id = EmbeddedDevice.Device_id WHERE EmbeddedDevice.Device_id = @id",
            _ => null
        };

        if (query is null) return null;

        using SqlConnection connection = new(_connectionString);
        using SqlCommand command = new(query, connection);
        command.Parameters.AddWithValue("@id", id);
        connection.Open();

        using SqlDataReader reader = command.ExecuteReader();
        if (!reader.Read()) return null;

        return id switch
        {
            var s when s.Contains("SW") => new SmartWatch
            {
                Device_Id = reader.GetString(0),
                Name = reader.GetString(1),
                IsOn = reader.GetBoolean(2),
                Id = reader.GetInt32(4),
                BatteryCharge = reader.GetInt32(5),
                DeviceRowVersion = reader.GetSqlBinary(reader.GetOrdinal("DeviceRowVersion")).Value,
                RowVersion = reader.GetSqlBinary(reader.GetOrdinal("RowVersion")).Value
            },
            var s when s.Contains("P") => new PersonalComputer
            {
                Device_Id = reader.GetString(0),
                Name = reader.GetString(1),
                IsOn = reader.GetBoolean(2),
                Id = reader.GetInt32(4),
                OperatingSystem = reader.GetString(5),
                DeviceRowVersion = reader.GetSqlBinary(reader.GetOrdinal("DeviceRowVersion")).Value,
                RowVersion = reader.GetSqlBinary(reader.GetOrdinal("RowVersion")).Value
            },
            var s when s.Contains("ED") => new EmbeddedDevice
            {
                Device_Id = reader.GetString(0),
                Name = reader.GetString(1),
                IsOn = reader.GetBoolean(2),
                Id = reader.GetInt32(4),
                IpAddress = reader.GetString(5),
                NetworkName = reader.GetString(6),
                DeviceRowVersion = reader.GetSqlBinary(reader.GetOrdinal("DeviceRowVersion")).Value,
                RowVersion = reader.GetSqlBinary(reader.GetOrdinal("RowVersion")).Value
            },
            _ => null
        };
    }
    
    private static void SetSmartWatchId(SmartWatch watch, int count)
    {
        if (watch.Device_Id.IsNullOrEmpty())
            watch.Device_Id = $"SW-{count + 1}";
    }

    private static void SetPersonalComputerId(PersonalComputer pc, int count)
    {
        if (pc.Device_Id.IsNullOrEmpty())
            pc.Device_Id = $"P-{count + 1}";
    }

    private static void SetEmbeddedDeviceId(EmbeddedDevice ed, int count)
    {
        if (ed.Device_Id.IsNullOrEmpty())
            ed.Device_Id = $"ED-{count + 1}";
    }

    private int GetMaxId(SqlConnection connection, string tableName)
    {
        using SqlCommand countCommand = new($"SELECT MAX(id) FROM {tableName}", connection);
        var result = countCommand.ExecuteScalar();
        return result is DBNull ? 0 : (int)result;
    }

    public async Task AddSmartWatch(SmartWatch smartWatch)
    {
        using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        var count = GetMaxId(connection, "SmartWatch");
        SetSmartWatchId(smartWatch, count);

        using SqlCommand command = new("AddSmartWatch", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@DeviceId", smartWatch.Device_Id);
        command.Parameters.AddWithValue("@Name", smartWatch.Name);
        command.Parameters.AddWithValue("@IsOn", smartWatch.IsOn);
        command.Parameters.AddWithValue("@BatteryCharge", smartWatch.BatteryCharge);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddPersonalComputer(PersonalComputer personalComputer)
    {
        using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        var count = GetMaxId(connection, "PersonalComputer");
        SetPersonalComputerId(personalComputer, count);

        using SqlCommand command = new("AddPersonalComputer", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@DeviceId", personalComputer.Device_Id);
        command.Parameters.AddWithValue("@Name", personalComputer.Name);
        command.Parameters.AddWithValue("@IsOn", personalComputer.IsOn);
        command.Parameters.AddWithValue("@OperatingSystem", personalComputer.OperatingSystem);

        await command.ExecuteNonQueryAsync();
    }

    public async Task AddEmbeddedDevice(EmbeddedDevice embeddedDevice)
    {
        using SqlConnection connection = new(_connectionString);
        await connection.OpenAsync();
        var count = GetMaxId(connection, "EmbeddedDevice");
        SetEmbeddedDeviceId(embeddedDevice, count);

        using SqlCommand command = new("AddEmbeddedDevice", connection)
        {
            CommandType = CommandType.StoredProcedure
        };
        command.Parameters.AddWithValue("@DeviceId", embeddedDevice.Device_Id);
        command.Parameters.AddWithValue("@Name", embeddedDevice.Name);
        command.Parameters.AddWithValue("@IsOn", embeddedDevice.IsOn);
        command.Parameters.AddWithValue("@IpAddress", embeddedDevice.IpAddress);
        command.Parameters.AddWithValue("@NetworkName", embeddedDevice.NetworkName);
        command.Parameters.AddWithValue("@IsConnected", embeddedDevice.IsConnected);

        await command.ExecuteNonQueryAsync();
    }
    
    public async Task UpdateSmartWatch(SmartWatch smartWatch)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // getting the timestamps
                byte[] deviceRowVersion = null;
                byte[] watchRowVersion = null;

                var rowVersionQuery = $"SELECT d.DeviceRowVersion AS DeviceRowVersion, sw.RowVersion AS WatchRowVersion FROM Device d INNER JOIN SmartWatch sw ON d.Id = sw.Device_id WHERE d.Id = @Id";

                using (SqlCommand rowVersionCmd = new SqlCommand(rowVersionQuery, connection, transaction))
                {
                    rowVersionCmd.Parameters.AddWithValue("@Id", smartWatch.Device_Id);
                    using (var reader = await rowVersionCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            deviceRowVersion = (byte[])reader["DeviceRowVersion"];
                            watchRowVersion = (byte[])reader["WatchRowVersion"];
                        }
                        else
                        {
                            throw new KeyNotFoundException("SmartWatch with the specified ID was not found.");
                        }
                    }
                }

                // updating the devices
                var updateDeviceQuery = $"UPDATE Device SET IsOn = @IsOn, Name = @Name WHERE Id = @Id AND DeviceRowVersion = @DeviceRowVersion";

                var updateWatchQuery = $"UPDATE SmartWatch SET BatteryCharge = @BatteryCharge WHERE Device_id = @Id AND RowVersion = @WatchRowVersion";

                using (SqlCommand updateDeviceCommand = new SqlCommand(updateDeviceQuery, connection, transaction))
                {
                    updateDeviceCommand.Parameters.AddWithValue("@Id", smartWatch.Device_Id);
                    updateDeviceCommand.Parameters.AddWithValue("@IsOn", smartWatch.IsOn);
                    updateDeviceCommand.Parameters.AddWithValue("@Name", smartWatch.Name);
                    updateDeviceCommand.Parameters.Add("@DeviceRowVersion", SqlDbType.Timestamp).Value = deviceRowVersion;

                    if (await updateDeviceCommand.ExecuteNonQueryAsync() == 0)
                        throw new DBConcurrencyException("Device update failed due to concurrent modification.");
                }

                using (SqlCommand updateWatchCommand = new SqlCommand(updateWatchQuery, connection, transaction))
                {
                    updateWatchCommand.Parameters.AddWithValue("@Id", smartWatch.Device_Id);
                    updateWatchCommand.Parameters.AddWithValue("@BatteryCharge", smartWatch.BatteryCharge);
                    updateWatchCommand.Parameters.Add("@WatchRowVersion", SqlDbType.Timestamp).Value = watchRowVersion;

                    if (await updateWatchCommand.ExecuteNonQueryAsync() == 0)
                        throw new DBConcurrencyException("SmartWatch update failed due to concurrent modification.");
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    public async Task UpdatePersonalComputer(PersonalComputer personalComputer)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // getting the timestamps
                byte[] deviceRowVersion = null;
                byte[] computerRowVersion = null;

                var rowVersionQuery = $"SELECT d.DeviceRowVersion AS DeviceRowVersion, p.RowVersion AS ComputerRowVersion FROM Device d INNER JOIN PersonalComputer p ON d.Id = p.Device_id WHERE d.Id = @Id";

                using (SqlCommand rowVersionCmd = new SqlCommand(rowVersionQuery, connection, transaction))
                {
                    rowVersionCmd.Parameters.AddWithValue("@Id", personalComputer.Device_Id);
                    using (var reader = await rowVersionCmd.ExecuteReaderAsync())
                    {
                        if (reader.Read())
                        {
                            deviceRowVersion = (byte[])reader["DeviceRowVersion"];
                            computerRowVersion = (byte[])reader["ComputerRowVersion"];
                        }
                        else
                        {
                            throw new KeyNotFoundException("PersonalComputer with the specified ID was not found.");
                        }
                    }
                }
                
                // updating the devices
                var updateDeviceQuery = "UPDATE Device SET IsOn = @IsOn, Name = @Name WHERE Id = @Id AND DeviceRowVersion = @DeviceRowVersion";
                var updateComputerQuery = "UPDATE PersonalComputer SET OperatingSystem = @OperatingSystem WHERE Device_id = @Id AND RowVersion = @ComputerRowVersion";
    
                SqlCommand updateDeviceCommand = new SqlCommand(updateDeviceQuery, connection, transaction);
                updateDeviceCommand.Parameters.AddWithValue("@Id", personalComputer.Device_Id);
                updateDeviceCommand.Parameters.AddWithValue("@IsOn", personalComputer.IsOn);
                updateDeviceCommand.Parameters.AddWithValue("@Name", personalComputer.Name);
                updateDeviceCommand.Parameters.Add("@DeviceRowVersion", SqlDbType.Timestamp).Value = deviceRowVersion;
        
                if (await updateDeviceCommand.ExecuteNonQueryAsync() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
    
                SqlCommand updateComputerCommand = new SqlCommand(updateComputerQuery, connection, transaction);
                updateComputerCommand.Parameters.AddWithValue("@Id", personalComputer.Device_Id);
                updateComputerCommand.Parameters.AddWithValue("@OperatingSystem", personalComputer.OperatingSystem);
                updateComputerCommand.Parameters.Add("@ComputerRowVersion", SqlDbType.Timestamp).Value = computerRowVersion;
        
                if (await updateComputerCommand.ExecuteNonQueryAsync() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
            
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    public async Task UpdateEmbeddedDevice(EmbeddedDevice embeddedDevice)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                // getting the timestamps
                byte[] deviceRowVersion = null;
                byte[] embeddedRowVersion = null;

                var rowVersionQuery = $"SELECT d.DeviceRowVersion AS DeviceRowVersion, ed.RowVersion AS EmbeddedRowVersion FROM Device d INNER JOIN EmbeddedDevice ed ON d.Id = ed.Device_id WHERE d.Id = @Id";

                using (SqlCommand rowVersionCmd = new SqlCommand(rowVersionQuery, connection, transaction))
                {
                    rowVersionCmd.Parameters.AddWithValue("@Id", embeddedDevice.Device_Id);
                    using (var reader = await rowVersionCmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            deviceRowVersion = (byte[])reader["DeviceRowVersion"];
                            embeddedRowVersion = (byte[])reader["EmbeddedRowVersion"];
                        }
                        else
                        {
                            throw new KeyNotFoundException("EmbeddedDevice with the specified ID was not found.");
                        }
                    }
                }
                
                // updating the devices
                var updateDeviceQuery = "UPDATE Device SET IsOn = @IsOn, Name = @Name WHERE Id = @Id AND DeviceRowVersion = @DeviceRowVersion";
                var updateEmbeddedQuery = "UPDATE EmbeddedDevice SET IpAddress = @IpAddress, NetworkName = @NetworkName, IsConnected = @IsConnected WHERE Device_id = @Id AND RowVersion = @EmbeddedRowVersion";
    
                SqlCommand updateDeviceCommand = new SqlCommand(updateDeviceQuery, connection, transaction);
                updateDeviceCommand.Parameters.AddWithValue("@Id", embeddedDevice.Device_Id);
                updateDeviceCommand.Parameters.AddWithValue("@IsOn", embeddedDevice.IsOn);
                updateDeviceCommand.Parameters.AddWithValue("@Name", embeddedDevice.Name);
                updateDeviceCommand.Parameters.Add("@DeviceRowVersion", SqlDbType.Timestamp).Value = deviceRowVersion;
        
                if (await updateDeviceCommand.ExecuteNonQueryAsync() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
    
                SqlCommand updateEmbeddeCommand = new SqlCommand(updateEmbeddedQuery, connection, transaction);
                updateEmbeddeCommand.Parameters.AddWithValue("@Id", embeddedDevice.Device_Id);
                updateEmbeddeCommand.Parameters.AddWithValue("@IpAddress", embeddedDevice.IpAddress);
                updateEmbeddeCommand.Parameters.AddWithValue("@NetworkName", embeddedDevice.NetworkName);
                updateEmbeddeCommand.Parameters.AddWithValue("@IsConnected", embeddedDevice.IsConnected);
                updateEmbeddeCommand.Parameters.Add("@EmbeddedRowVersion", SqlDbType.Timestamp).Value = embeddedRowVersion;
        
                if (await updateEmbeddeCommand.ExecuteNonQueryAsync() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
            
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    public async Task DeleteWatch(string id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var deleteWatchResult = -1;
                var deleteDeviceResult = -1;

                var deleteWatchQuery = "DELETE FROM SmartWatch WHERE Device_Id = @Device_Id";
                var deleteDeviceQuery = "DELETE FROM Device WHERE Id = @Id";

                SqlCommand deleteWatchCommand = new SqlCommand(deleteWatchQuery, connection, transaction);
                deleteWatchCommand.Parameters.AddWithValue("@Device_Id", id);
                deleteWatchResult = deleteWatchCommand.ExecuteNonQuery();

                if (deleteWatchResult == -1)
                    throw new ApplicationException("Deleting the device failed.");
                
                SqlCommand deleteDeviceCommand = new SqlCommand(deleteDeviceQuery, connection, transaction);
                deleteDeviceCommand.Parameters.AddWithValue("@Id", id);
                deleteDeviceResult = deleteDeviceCommand.ExecuteNonQuery();
                
                if (deleteDeviceResult == -1)
                    throw new ApplicationException("Deleting the device failed.");
                
                transaction.Commit();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    public async Task DeleteComputer(string id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var deleteComputerResult = -1;
                var deleteDeviceResult = -1;

                var deleteComputerQuery = "DELETE FROM PersonalComputer WHERE Device_Id = @Device_Id";
                var deleteDeviceQuery = "DELETE FROM Device WHERE Id = @Id";

                SqlCommand deleteComputerCommand = new SqlCommand(deleteComputerQuery, connection, transaction);
                deleteComputerCommand.Parameters.AddWithValue("@Device_Id", id);
                deleteComputerResult =await deleteComputerCommand.ExecuteNonQueryAsync();

                if (deleteComputerResult == -1)
                    throw new ApplicationException("Deleting the device failed.");
                
                SqlCommand deleteDeviceCommand = new SqlCommand(deleteDeviceQuery, connection, transaction);
                deleteDeviceCommand.Parameters.AddWithValue("@Id", id);
                deleteDeviceResult = await deleteDeviceCommand.ExecuteNonQueryAsync();
                
                if (deleteDeviceResult == -1)
                    throw new ApplicationException("Deleting the device failed.");
                
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
    
    public async Task DeleteEmbeddedDevice(string id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            await connection.OpenAsync();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var deleteEmbeddedResult = -1;
                var deleteDeviceResult = -1;

                var deleteEmbeddedQuery = "DELETE FROM EmbeddedDevice WHERE Device_Id = @Device_Id";
                var deleteDeviceQuery = "DELETE FROM Device WHERE Id = @Id";
                
                SqlCommand deleteEmbeddedCommand = new SqlCommand(deleteEmbeddedQuery, connection, transaction);
                deleteEmbeddedCommand.Parameters.AddWithValue("@Device_Id", id);
                deleteEmbeddedResult = await deleteEmbeddedCommand.ExecuteNonQueryAsync();

                if (deleteEmbeddedResult == -1)
                    throw new ApplicationException("Deleting the device failed.");
                
                SqlCommand deleteDeviceCommand = new SqlCommand(deleteDeviceQuery, connection, transaction);
                deleteDeviceCommand.Parameters.AddWithValue("@Id", id);
                deleteDeviceResult = await deleteDeviceCommand.ExecuteNonQueryAsync();
                
                
                if (deleteDeviceResult == -1)
                    throw new ApplicationException("Deleting the device failed.");
                
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}