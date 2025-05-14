-- Created by Vertabelo (http://vertabelo.com)
-- Last modification date: 2025-04-25 09:21:17.158

-- tables
-- Table: Device


CREATE TABLE Device (
    Id                 varchar(250) NOT NULL,
    Name               varchar(250) NOT NULL,
    IsOn                        bit NOT NULL,
    DeviceRowVersion     ROWVERSION NOT NULL,
    CONSTRAINT Device_pk PRIMARY KEY  (Id)
);

-- Table: EmbeddedDevice
CREATE TABLE EmbeddedDevice (
    Id                      int IDENTITY(1, 1) NOT NULL,
    IpAddress      varchar(250) NOT NULL,
    NetworkName    varchar(250) NOT NULL,
    IsConnected             bit NOT NULL,
    Device_id      varchar(250) NOT NULL,
    RowVersion       ROWVERSION NOT NULL,
    CONSTRAINT EmbeddedDevice_pk PRIMARY KEY  (Id,Device_id)
);

-- Table: PersonalComputer
CREATE TABLE PersonalComputer (
    Id                          int IDENTITY(1, 1) NOT NULL,
    OperatingSystem    varchar(250) NOT NULL,
    Device_id          varchar(250) NOT NULL,
    RowVersion           ROWVERSION NOT NULL,
    CONSTRAINT PersonalComputer_pk PRIMARY KEY  (Id,Device_id)
);

-- Table: SmartWatch
CREATE TABLE SmartWatch (
    Id                      int IDENTITY(1, 1) NOT NULL,
    BatteryCharge           int NOT NULL,
    Device_id      varchar(250) NOT NULL,
    RowVersion       ROWVERSION NOT NULL,
    CONSTRAINT SmartWatch_pk PRIMARY KEY  (Id,Device_id)
);

-- foreign keys
-- Reference: EmbeddedDevice_Device (table: EmbeddedDevice)
ALTER TABLE EmbeddedDevice ADD CONSTRAINT EmbeddedDevice_Device
    FOREIGN KEY (Device_id)
        REFERENCES Device (Id);

-- Reference: PersonalComputer_Device (table: PersonalComputer)
ALTER TABLE PersonalComputer ADD CONSTRAINT PersonalComputer_Device
    FOREIGN KEY (Device_id)
        REFERENCES Device (Id);

-- Reference: SmartWatch_Device (table: SmartWatch)
ALTER TABLE SmartWatch ADD CONSTRAINT SmartWatch_Device
    FOREIGN KEY (Device_id)
        REFERENCES Device (Id);

-- End of file.

