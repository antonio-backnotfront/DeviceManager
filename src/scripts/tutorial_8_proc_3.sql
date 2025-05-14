CREATE PROCEDURE AddPersonalComputer
    @DeviceId VARCHAR(50),
    @Name NVARCHAR(100),
    @IsOn BIT,
    @OperatingSystem VARCHAR(50)
AS
BEGIN
    SET NOCOUNT ON;

    BEGIN TRY
        BEGIN TRANSACTION;

        -- Insert into Device table
        INSERT INTO Device (Id, Name, IsOn)
        VALUES (@DeviceId, @Name, @IsOn);

        -- Insert into PersonalComputer table
        INSERT INTO PersonalComputer (OperatingSystem, Device_Id)
        VALUES (@OperatingSystem, @DeviceId);

        COMMIT TRANSACTION;
    END TRY
    BEGIN CATCH
        ROLLBACK TRANSACTION;
        THROW;
    END CATCH
END
