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
        List<DeviceDTO> devices = [];
        const string query = "SELECT * FROM Device";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            SqlCommand command = new SqlCommand(query, connection);
            
            connection.Open();
            SqlDataReader reader = command.ExecuteReader();
            try
            {
                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        var deviceRow = new DeviceDTO
                        {
                            Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsOn = reader.GetBoolean(2)
                        };
                        devices.Add(deviceRow);
                    }
                }
            }
            finally
            {
                reader.Close();
            }
            return devices;
        }
    }
    
    public Device? GetDeviceById(string id)
    {
        var query = "SELECT * FROM Device";

        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            if (id.Contains("SW"))
            {
                query += " JOIN SmartWatch ON Device.Id = SmartWatch.Device_id WHERE SmartWatch.Device_Id = @id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        return new SmartWatch
                        {
                            Device_Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsOn = reader.GetBoolean(2),
                            Id = reader.GetInt32(4),
                            BatteryCharge = reader.GetInt32(5),
                            DeviceRowVersion = reader.GetSqlBinary(reader.GetOrdinal("DeviceRowVersion")).Value,
                            RowVersion = reader.GetSqlBinary(reader.GetOrdinal("RowVersion")).Value
                        };
                    }
                }
                finally
                {
                    reader.Close();
                }
            } else if (id.Contains("P"))
            {
                query += " JOIN PersonalComputer ON Device.Id = PersonalComputer.Device_id WHERE PersonalComputer.Device_Id = @id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        return new PersonalComputer
                        {
                            Device_Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsOn = reader.GetBoolean(2),
                            Id = reader.GetInt32(4),
                            OperatingSystem = reader.GetString(5),
                            DeviceRowVersion = reader.GetSqlBinary(reader.GetOrdinal("DeviceRowVersion")).Value,
                            RowVersion = reader.GetSqlBinary(reader.GetOrdinal("RowVersion")).Value
                        };
                    }
                }
                finally
                {
                    reader.Close();
                }
            } else if (id.Contains("ED"))
            {
                query += " JOIN EmbeddedDevice on Device.Id = EmbeddedDevice.Device_id WHERE EmbeddedDevice.Device_id = @id";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@id", id);
                connection.Open();
                
                SqlDataReader reader = command.ExecuteReader();
                try
                {
                    if (reader.Read())
                    {
                        return new EmbeddedDevice
                        {
                            Device_Id = reader.GetString(0),
                            Name = reader.GetString(1),
                            IsOn = reader.GetBoolean(2),
                            Id = reader.GetInt32(4),
                            IpAddress = reader.GetString(5),
                            NetworkName = reader.GetString(6),
                            DeviceRowVersion = reader.GetSqlBinary(reader.GetOrdinal("DeviceRowVersion")).Value,
                            RowVersion = reader.GetSqlBinary(reader.GetOrdinal("RowVersion")).Value
                        };
                    }
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        return null;
    }
    
    public void AddSmartWatch(SmartWatch smartWatch)
    {
        // adding id and inserting into the db
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var countSwQuery = "SELECT MAX(id) FROM SmartWatch";
            var count = -1;
            SqlCommand countCommand = new SqlCommand(countSwQuery, connection);
            SqlDataReader reader = countCommand.ExecuteReader();
            try
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
            }
            finally
            {
                reader.Close();
            }

            // set the device id only if it was not set
            if (smartWatch.Device_Id.IsNullOrEmpty())
            {
                smartWatch.Device_Id = $"SW-{count + 1}";
            }

            SqlCommand command = new SqlCommand("AddSmartWatch", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@DeviceId", smartWatch.Device_Id);
            command.Parameters.AddWithValue("@Name", smartWatch.Name);
            command.Parameters.AddWithValue("@IsOn", smartWatch.IsOn);
            command.Parameters.AddWithValue("@BatteryCharge", smartWatch.BatteryCharge);
            
            command.ExecuteNonQuery();
        }
    }

    public void AddPersonalComputer(PersonalComputer personalComputer)
    {
        // adding id and inserting into the db
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var countPcQuery = "SELECT MAX(id) FROM PersonalComputer";
            var count = -1;
            SqlCommand countCommand = new SqlCommand(countPcQuery, connection);
            SqlDataReader reader = countCommand.ExecuteReader();
            try
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
            }
            finally
            {
                reader.Close();
            }
                
            // set the device id only if it was not set
            if (personalComputer.Device_Id.IsNullOrEmpty())
            {
                personalComputer.Device_Id = $"P-{count + 1}";
            }

            SqlCommand command = new SqlCommand("AddPersonalComputer", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@DeviceId", personalComputer.Device_Id);
            command.Parameters.AddWithValue("@Name", personalComputer.Name);
            command.Parameters.AddWithValue("@IsOn", personalComputer.IsOn);
            command.Parameters.AddWithValue("@OperatingSystem", personalComputer.OperatingSystem);
            
            command.ExecuteNonQuery();
        }
    }
    
    public void AddEmbeddedDevice(EmbeddedDevice embeddedDevice)
    {
        // adding id and inserting into the db
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            var countEdQuery = "SELECT MAX(id) FROM EmbeddedDevice";
            var count = -1;
            SqlCommand countCommand = new SqlCommand(countEdQuery, connection);
            SqlDataReader reader = countCommand.ExecuteReader();
            try
            {
                if (reader.Read())
                {
                    count = reader.GetInt32(0);
                }
            }
            finally
            {
                reader.Close();
            }
                
            // set the device id only if it was not set
            if (embeddedDevice.Device_Id.IsNullOrEmpty())
            {
                embeddedDevice.Device_Id = $"ED-{count + 1}";
            }

            SqlCommand command = new SqlCommand("AddEmbeddedDevice", connection);
            command.CommandType = CommandType.StoredProcedure;
            command.Parameters.AddWithValue("@DeviceId", embeddedDevice.Device_Id);
            command.Parameters.AddWithValue("@Name", embeddedDevice.Name);
            command.Parameters.AddWithValue("@IsOn", embeddedDevice.IsOn);
            command.Parameters.AddWithValue("@IpAddress", embeddedDevice.IpAddress);
            command.Parameters.AddWithValue("@NetworkName", embeddedDevice.NetworkName);
            command.Parameters.AddWithValue("@IsConnected", embeddedDevice.IsConnected);
            
            command.ExecuteNonQuery();
        }
    }
    
    public void UpdateSmartWatch(SmartWatch smartWatch)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
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
                    using (var reader = rowVersionCmd.ExecuteReader())
                    {
                        if (reader.Read())
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

                    if (updateDeviceCommand.ExecuteNonQuery() == 0)
                        throw new DBConcurrencyException("Device update failed due to concurrent modification.");
                }

                using (SqlCommand updateWatchCommand = new SqlCommand(updateWatchQuery, connection, transaction))
                {
                    updateWatchCommand.Parameters.AddWithValue("@Id", smartWatch.Device_Id);
                    updateWatchCommand.Parameters.AddWithValue("@BatteryCharge", smartWatch.BatteryCharge);
                    updateWatchCommand.Parameters.Add("@WatchRowVersion", SqlDbType.Timestamp).Value = watchRowVersion;

                    if (updateWatchCommand.ExecuteNonQuery() == 0)
                        throw new DBConcurrencyException("SmartWatch update failed due to concurrent modification.");
                }

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public void UpdatePersonalComputer(PersonalComputer personalComputer)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
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
                    using (var reader = rowVersionCmd.ExecuteReader())
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
        
                if (updateDeviceCommand.ExecuteNonQuery() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
    
                SqlCommand updateComputerCommand = new SqlCommand(updateComputerQuery, connection, transaction);
                updateComputerCommand.Parameters.AddWithValue("@Id", personalComputer.Device_Id);
                updateComputerCommand.Parameters.AddWithValue("@OperatingSystem", personalComputer.OperatingSystem);
                updateComputerCommand.Parameters.Add("@ComputerRowVersion", SqlDbType.Timestamp).Value = computerRowVersion;
        
                if (updateComputerCommand.ExecuteNonQuery() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
            
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public void UpdateEmbeddedDevice(EmbeddedDevice embeddedDevice)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
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
                    using (var reader = rowVersionCmd.ExecuteReader())
                    {
                        if (reader.Read())
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
        
                if (updateDeviceCommand.ExecuteNonQuery() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
    
                SqlCommand updateEmbeddeCommand = new SqlCommand(updateEmbeddedQuery, connection, transaction);
                updateEmbeddeCommand.Parameters.AddWithValue("@Id", embeddedDevice.Device_Id);
                updateEmbeddeCommand.Parameters.AddWithValue("@IpAddress", embeddedDevice.IpAddress);
                updateEmbeddeCommand.Parameters.AddWithValue("@NetworkName", embeddedDevice.NetworkName);
                updateEmbeddeCommand.Parameters.AddWithValue("@IsConnected", embeddedDevice.IsConnected);
                updateEmbeddeCommand.Parameters.Add("@EmbeddedRowVersion", SqlDbType.Timestamp).Value = embeddedRowVersion;
        
                if (updateEmbeddeCommand.ExecuteNonQuery() == 0)
                    throw new DBConcurrencyException("Device update failed due to concurrent modification.");
            
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public void DeleteWatch(string id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
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
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public void DeleteComputer(string id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var deleteComputerResult = -1;
                var deleteDeviceResult = -1;

                var deleteComputerQuery = "DELETE FROM PersonalComputer WHERE Device_Id = @Device_Id";
                var deleteDeviceQuery = "DELETE FROM Device WHERE Id = @Id";

                SqlCommand deleteComputerCommand = new SqlCommand(deleteComputerQuery, connection, transaction);
                deleteComputerCommand.Parameters.AddWithValue("@Device_Id", id);
                deleteComputerResult = deleteComputerCommand.ExecuteNonQuery();

                if (deleteComputerResult == -1)
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
                transaction.Rollback();
                throw;
            }
        }
    }
    
    public void DeleteEmbeddedDevice(string id)
    {
        using (SqlConnection connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            SqlTransaction transaction = connection.BeginTransaction();

            try
            {
                var deleteEmbeddedResult = -1;
                var deleteDeviceResult = -1;

                var deleteEmbeddedQuery = "DELETE FROM EmbeddedDevice WHERE Device_Id = @Device_Id";
                var deleteDeviceQuery = "DELETE FROM Device WHERE Id = @Id";
                
                SqlCommand deleteEmbeddedCommand = new SqlCommand(deleteEmbeddedQuery, connection, transaction);
                deleteEmbeddedCommand.Parameters.AddWithValue("@Device_Id", id);
                deleteEmbeddedResult = deleteEmbeddedCommand.ExecuteNonQuery();

                if (deleteEmbeddedResult == -1)
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
                transaction.Rollback();
                throw;
            }
        }
    }
}