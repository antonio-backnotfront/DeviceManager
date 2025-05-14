CREATE PROCEDURE AddEmbeddedDevice
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsOn BIT,
    @IpAddress VARCHAR(50),
    @NetworkName VARCHAR(100),
    @IsConnected BIT
AS
BEGIN
--     SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsOn)
        VALUES (@DeviceId, @Name, @IsOn);

        -- Insert into Embedded table
        INSERT INTO EmbeddedDevice (IpAddress, NetworkName, IsConnected, Device_id)
        VALUES (@IpAddress, @NetworkName, @IsConnected, @DeviceId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
