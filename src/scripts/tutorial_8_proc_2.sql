CREATE PROCEDURE AddSmartWatch
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsOn BIT,
    @BatteryCharge INT
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsOn)
        VALUES (@DeviceId, @Name, @IsOn);

        -- Insert into Smartwatch table
        INSERT INTO SmartWatch (BatteryCharge, Device_id)
        VALUES (@BatteryCharge, @DeviceId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
