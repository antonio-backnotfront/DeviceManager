EXEC AddPersonalComputer @DeviceId = 'P-1', @Name = 'Microsoft Ex 1', @IsOn = 0, @OperatingSystem = 'Windows 10';
EXEC AddPersonalComputer @DeviceId = 'P-2', @Name = 'Macbook M1 2021', @IsOn = 0, @OperatingSystem = 'MacOS Safari';

EXEC AddSmartWatch @DeviceId = 'sw-1', @Name = 'Apple Watch Series 7', @IsOn = 0, @BatteryCharge = 40;
EXEC AddSmartWatch @DeviceId = 'sw-2', @Name = 'Samsung Galaxy Watch 2', @IsOn = 1, @BatteryCharge = 15;
EXEC AddSmartWatch @DeviceId = 'sw-3', @Name = 'Samsung Galaxy Watch 3', @IsOn = 1, @BatteryCharge = 92;
EXEC AddEmbeddedDevice @DeviceId = 'ED-1', @Name = 'Efafa 33', @IsOn = 1, @IpAddress = '190.169.0.0', @NetworkName = 'UniversityNetwork', @IsConnected = 0;
EXEC AddEmbeddedDevice @DeviceId = 'ED-2', @Name = 'Ofapfa 11', @IsOn = 1, @IpAddress = '192.169.0.15', @NetworkName = 'UniversityNetwork', @IsConnected = 1;
EXEC AddEmbeddedDevice @DeviceId = 'ED-3', @Name = 'Ofapfa 12', @IsOn = 1, @IpAddress = '192.169.0.15', @NetworkName = 'UniversityNetwork', @IsConnected = 1;
