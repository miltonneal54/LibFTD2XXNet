using System;
using System.Runtime.InteropServices;

namespace FTD2xxNet
{
    /// <summary>
    /// Internal structure for reading and writing EEPROM contents.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal class FT_PROGRAM_DATA
    {
        public uint Signature1;
        public uint Signature2;
        public uint Version;
        public ushort VendorID;
        public ushort ProductID;

        public IntPtr Manufacturer;
        public IntPtr ManufacturerID;
        public IntPtr Description;
        public IntPtr SerialNumber;

        public ushort MaxPower;
        public ushort PnP;
        public ushort SelfPowered;
        public ushort RemoteWakeup;

        // FT232B extensions.
        public byte Rev4;
        public byte IsoIn;
        public byte IsoOut;
        public byte PullDownEnable;
        public byte SerNumEnable;
        public byte USBVersionEnable;
        public ushort USBVersion;

        // FT2232D extensions.
        public byte Rev5;
        public byte IsoInA;
        public byte IsoInB;
        public byte IsoOutA;
        public byte IsoOutB;
        public byte PullDownEnable5;
        public byte SerNumEnable5;
        public byte USBVersionEnable5;
        public ushort USBVersion5;
        public byte AIsHighCurrent;
        public byte BIsHighCurrent;
        public byte IFAIsFifo;
        public byte IFAIsFifoTar;
        public byte IFAIsFastSer;
        public byte AIsVCP;
        public byte IFBIsFifo;
        public byte IFBIsFifoTar;
        public byte IFBIsFastSer;
        public byte BIsVCP;

        // FT232R extensions.
        public byte UseExtOsc;
        public byte HighDriveIOs;
        public byte EndpointSize;
        public byte PullDownEnableR;
        public byte SerNumEnableR;
        public byte InvertTXD;          // Non-zero if invert TXD.
        public byte InvertRXD;          // Non-zero if invert RXD.
        public byte InvertRTS;          // Non-zero if invert RTS.
        public byte InvertCTS;          // Non-zero if invert CTS.
        public byte InvertDTR;          // Non-zero if invert DTR.
        public byte InvertDSR;          // Non-zero if invert DSR.
        public byte InvertDCD;          // Non-zero if invert DCD.
        public byte InvertRI;           // Non-zero if invert RI.
        public byte Cbus0;              // Cbus Mux control - Ignored for FT245R.
        public byte Cbus1;              // Cbus Mux control - Ignored for FT245R.
        public byte Cbus2;              // Cbus Mux control - Ignored for FT245R.
        public byte Cbus3;              // Cbus Mux control - Ignored for FT245R.
        public byte Cbus4;              // Cbus Mux control - Ignored for FT245R.
        public byte RIsD2XX;            // Default to loading VCP.

        // FT2232H extensions.
        public byte PullDownEnable7;
        public byte SerNumEnable7;
        public byte ALSlowSlew;         // Non-zero if AL pins have slow slew.
        public byte ALSchmittInput;     // Non-zero if AL pins are Schmitt input.
        public byte ALDriveCurrent;     // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte AHSlowSlew;         // Non-zero if AH pins have slow slew.
        public byte AHSchmittInput;     // Non-zero if AH pins are Schmitt input.
        public byte AHDriveCurrent;     // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte BLSlowSlew;         // Non-zero if BL pins have slow slew.
        public byte BLSchmittInput;     // Non-zero if BL pins are Schmitt input.
        public byte BLDriveCurrent;		// Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte BHSlowSlew;         // Non-zero if BH pins have slow slew.
        public byte BHSchmittInput;     // Non-zero if BH pins are Schmitt input.
        public byte BHDriveCurrent;     // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte IFAIsFifo7;         // Non-zero if interface is 245 FIFO.
        public byte IFAIsFifoTar7;      // Non-zero if interface is 245 FIFO CPU target.
        public byte IFAIsFastSer7;      // Non-zero if interface is Fast serial.
        public byte AIsVCP7;            // Non-zero if interface is to use VCP drivers.
        public byte IFBIsFifo7;         // Non-zero if interface is 245 FIFO.
        public byte IFBIsFifoTar7;      // Non-zero if interface is 245 FIFO CPU target.
        public byte IFBIsFastSer7;      // Non-zero if interface is Fast serial.
        public byte BIsVCP7;            // Non-zero if interface is to use VCP drivers.
        public byte PowerSaveEnable;    // Non-zero if using BCBUS7 to save power for self-powered designs.

        // FT4232H extensions.
        public byte PullDownEnable8;
        public byte SerNumEnable8;
        public byte ASlowSlew;          // Non-zero if AL pins have slow slew.
        public byte ASchmittInput;      // Non-zero if AL pins are Schmitt input.
        public byte ADriveCurrent;      // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte BSlowSlew;          // Non-zero if AH pins have slow slew.
        public byte BSchmittInput;      // Non-zero if AH pins are Schmitt input.
        public byte BDriveCurrent;      // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte CSlowSlew;          // Non-zero if BL pins have slow slew.
        public byte CSchmittInput;      // Non-zero if BL pins are Schmitt input.
        public byte CDriveCurrent;      // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte DSlowSlew;          // Non-zero if BH pins have slow slew.
        public byte DSchmittInput;      // Non-zero if BH pins are Schmitt input.
        public byte DDriveCurrent;      // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte ARIIsTXDEN;
        public byte BRIIsTXDEN;
        public byte CRIIsTXDEN;
        public byte DRIIsTXDEN;
        public byte AIsVCP8;            // Non-zero if interface is to use VCP drivers.
        public byte BIsVCP8;            // Non-zero if interface is to use VCP drivers.
        public byte CIsVCP8;            // Non-zero if interface is to use VCP drivers.
        public byte DIsVCP8;            // Non-zero if interface is to use VCP drivers.

        // FT232H extensions.
        public byte PullDownEnableH;    // Non-zero if pull down enabled.
        public byte SerNumEnableH;      // Non-zero if serial number to be used.
        public byte ACSlowSlewH;        // Non-zero if AC pins have slow slew.
        public byte ACSchmittInputH;	// Non-zero if AC pins are Schmitt input.
        public byte ACDriveCurrentH;    // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte ADSlowSlewH;        // Non-zero if AD pins have slow slew.
        public byte ADSchmittInputH;    // Non-zero if AD pins are Schmitt input.
        public byte ADDriveCurrentH;    // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte Cbus0H;             // Cbus Mux control.
        public byte Cbus1H;             // Cbus Mux control.
        public byte Cbus2H;             // Cbus Mux control.
        public byte Cbus3H;             // Cbus Mux control.
        public byte Cbus4H;             // Cbus Mux control.
        public byte Cbus5H;             // Cbus Mux control.
        public byte Cbus6H;             // Cbus Mux control.
        public byte Cbus7H;             // Cbus Mux control.
        public byte Cbus8H;             // Cbus Mux control.
        public byte Cbus9H;             // Cbus Mux control.
        public byte IsFifoH;            // Non-zero if interface is 245 FIFO.
        public byte IsFifoTarH;         // Non-zero if interface is 245 FIFO CPU target.
        public byte IsFastSerH;         // Non-zero if interface is Fast serial.
        public byte IsFT1248H;          // Non-zero if interface is FT1248.
        public byte FT1248CpolH;        // FT1248 clock polarity.
        public byte FT1248LsbH;         // FT1248 data is LSB (1) or MSB (0).
        public byte FT1248FlowControlH; // FT1248 flow control enable.
        public byte IsVCPH;             // Non-zero if interface is to use VCP drivers.
        public byte PowerSaveEnableH;   // Non-zero if using ACBUS7 to save power for self-powered designs.
    }

    /// <summary>
    /// Internal structure for reading and writing the EEPROM header.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct FT_EEPROM_HEADER
    {
        public uint deviceType;             // FTxxxx device type to be programmed.

        // Device descriptor options.
        public ushort VendorId;             // 0x0403.
        public ushort ProductId;            // 0x6001.
        public byte SerNumEnable;           // Non-zero if serial number to be used.

        // Config descriptor options.
        public ushort MaxPower;             // 0 < MaxPower <= 500.
        public byte SelfPowered;            // 0 = bus powered, 1 = self powered.
        public byte RemoteWakeup;           // 0 = not capable, 1 = capable.

        // Hardware options.
        public byte PullDownEnable;         // Non-zero if pull down in suspend enabled.
    }

    /// <summary>
    /// Internal structure for reading and writing XSERIES EEPROM data.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct FT_XSERIES_DATA
    {
        public FT_EEPROM_HEADER common;
        public byte ACSlowSlew;         // Non-zero if AC bus pins have slow slew.
        public byte ACSchmittInput;     // Non-zero if AC bus pins are Schmitt input.
        public byte ACDriveCurrent;     // Valid values are 4mA, 8mA, 12mA, 16mA.
        public byte ADSlowSlew;         // Non-zero if AD bus pins have slow slew.
        public byte ADSchmittInput;     // Non-zero if AD bus pins are Schmitt input.
        public byte ADDriveCurrent;     // Valid values are 4mA, 8mA, 12mA, 16mA.

        // CBUS options.
        public byte Cbus0;              // Cbus Mux control.
        public byte Cbus1;              // Cbus Mux control.
        public byte Cbus2;              // Cbus Mux control.
        public byte Cbus3;              // Cbus Mux control.
        public byte Cbus4;              // Cbus Mux control.
        public byte Cbus5;              // Cbus Mux control.
        public byte Cbus6;              // Cbus Mux control.

        // UART signal options.
        public byte InvertTXD;          // Non-zero if invert TXD.
        public byte InvertRXD;          // Non-zero if invert RXD.
        public byte InvertRTS;          // Non-zero if invert RTS.
        public byte InvertCTS;          // Non-zero if invert CTS.
        public byte InvertDTR;          // Non-zero if invert DTR.
        public byte InvertDSR;          // Non-zero if invert DSR.
        public byte InvertDCD;          // Non-zero if invert DCD.
        public byte InvertRI;           // Non-zero if invert RI.

        // Battery Charge Detect options.
        public byte BCDEnable;          // Enable Battery Charger Detection.
        public byte BCDForceCbusPWREN;  // Asserts the power enable signal on CBUS when charging port detected.
        public byte BCDDisableSleep;    // Forces the device never to go into sleep mode.

        // I2C options.
        public ushort I2CSlaveAddress;  // I2C slave device address.
        public uint I2CDeviceId;        // I2C device ID.
        public byte I2CDisableSchmitt;  // Disable I2C Schmitt trigger.

        // FT1248 options.
        public byte FT1248Cpol;         // FT1248 clock polarity - clock idle high (1) or clock idle low (0).
        public byte FT1248Lsb;          // FT1248 data is LSB (1) or MSB (0).
        public byte FT1248FlowControl;  // FT1248 flow control enable.

        // Hardware options.
        public byte RS485EchoSuppress;
        public byte PowerSaveEnable;

        // Driver option.
        public byte DriverType;
    }
}
