using System;
using System.Collections.Generic;
using System.Text;

namespace FTD2xxNet
{
    public partial class FTDI
    {
        /// <summary>
        /// Available functions for the FT232R CBUS pins. Controlled by FT232R EEPROM settings.
        /// </summary>
        public class FT_CBUS_OPTIONS
        {
            /// <summary>
            /// FT232R CBUS EEPROM options - Tx Data Enable.
            /// </summary>
            public const byte FT_CBUS_TXDEN = 0x00;

            /// <summary>
            /// FT232R CBUS EEPROM options - Power On.
            /// </summary>
            public const byte FT_CBUS_PWRON = 0x01;

            /// <summary>
            /// FT232R CBUS EEPROM options - Rx LED.
            /// </summary>
            public const byte FT_CBUS_RXLED = 0x02;

            /// <summary>
            /// FT232R CBUS EEPROM options - Tx LED.
            /// </summary>
            public const byte FT_CBUS_TXLED = 0x03;

            /// <summary>
            /// FT232R CBUS EEPROM options - Tx and Rx LED.
            /// </summary>
            public const byte FT_CBUS_TXRXLED = 0x04;

            /// <summary>
            /// FT232R CBUS EEPROM options - Sleep.
            /// </summary>
            public const byte FT_CBUS_SLEEP = 0x05;

            /// <summary>
            /// FT232R CBUS EEPROM options - 48MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK48 = 0x06;

            /// <summary>
            /// FT232R CBUS EEPROM options - 24MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK24 = 0x07;

            /// <summary>
            /// FT232R CBUS EEPROM options - 12MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK12 = 0x08;

            /// <summary>
            /// FT232R CBUS EEPROM options - 6MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK6 = 0x09;

            /// <summary>
            /// FT232R CBUS EEPROM options - IO mode.
            /// </summary>
            public const byte FT_CBUS_IOMODE = 0x0A;

            /// <summary>
            /// FT232R CBUS EEPROM options - Bit-bang write strobe.
            /// </summary>
            public const byte FT_CBUS_BITBANG_WR = 0x0B;

            /// <summary>
            /// FT232R CBUS EEPROM options - Bit-bang read strobe.
            /// </summary>
            public const byte FT_CBUS_BITBANG_RD = 0x0C;
        }

        /// <summary>
        /// Available functions for the FT232H CBUS pins.  Controlled by FT232H EEPROM settings.
        /// </summary>
        public class FT_232H_CBUS_OPTIONS
        {
            /// <summary>
            /// FT232H CBUS EEPROM options - Tristate.
            /// </summary>
            public const byte FT_CBUS_TRISTATE = 0x00;

            /// <summary>
            /// FT232H CBUS EEPROM options - Rx LED.
            /// </summary>
            public const byte FT_CBUS_RXLED = 0x01;

            /// <summary>
            /// FT232H CBUS EEPROM options - Tx LED.
            /// </summary>
            public const byte FT_CBUS_TXLED = 0x02;

            /// <summary>
            /// FT232H CBUS EEPROM options - Tx and Rx LED.
            /// </summary>
            public const byte FT_CBUS_TXRXLED = 0x03;

            /// <summary>
            /// FT232H CBUS EEPROM options - Power Enable#.
            /// </summary>
            public const byte FT_CBUS_PWREN = 0x04;

            /// <summary>
            /// FT232H CBUS EEPROM options - Sleep.
            /// </summary>
            public const byte FT_CBUS_SLEEP = 0x05;

            /// <summary>
            /// FT232H CBUS EEPROM options - Drive pin to logic 0.
            /// </summary>
            public const byte FT_CBUS_DRIVE_0 = 0x06;

            /// <summary>
            /// FT232H CBUS EEPROM options - Drive pin to logic 1.
            /// </summary>
            public const byte FT_CBUS_DRIVE_1 = 0x07;

            /// <summary>
            /// FT232H CBUS EEPROM options - IO Mode.
            /// </summary>
            public const byte FT_CBUS_IOMODE = 0x08;

            /// <summary>
            /// FT232H CBUS EEPROM options - Tx Data Enable.
            /// </summary>
            public const byte FT_CBUS_TXDEN = 0x09;

            /// <summary>
            /// FT232H CBUS EEPROM options - 30MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK30 = 0x0A;

            /// <summary>
            /// FT232H CBUS EEPROM options - 15MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK15 = 0x0B;

            /// <summary>
            /// FT232H CBUS EEPROM options - 7.5MHz clock.
            /// </summary>
            public const byte FT_CBUS_CLK7_5 = 0x0C;
        }

        /// <summary>
        /// Available functions for the X-Series CBUS pins.  Controlled by X-Series EEPROM settings.
        /// </summary>
        public class FT_XSERIES_CBUS_OPTIONS
        {
            /// <summary>
            /// FT X-Series CBUS EEPROM options - Tristate.
            /// </summary>
            public const byte FT_CBUS_TRISTATE = 0x00;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - RxLED#.
            /// </summary>
            public const byte FT_CBUS_RXLED = 0x01;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - TxLED#.
            /// </summary>
            public const byte FT_CBUS_TXLED = 0x02;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - TxRxLED#.
            /// </summary>
            public const byte FT_CBUS_TXRXLED = 0x03;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - PwrEn#.
            /// </summary>
            public const byte FT_CBUS_PWREN = 0x04;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Sleep#.
            /// </summary>
            public const byte FT_CBUS_SLEEP = 0x05;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Drive_0.
            /// </summary>
            public const byte FT_CBUS_Drive_0 = 0x06;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Drive_1.
            /// </summary>
            public const byte FT_CBUS_Drive_1 = 0x07;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - GPIO.
            /// </summary>
            public const byte FT_CBUS_GPIO = 0x08;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - TxdEn.
            /// </summary>
            public const byte FT_CBUS_TXDEN = 0x09;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Clk24MHz.
            /// </summary>
            public const byte FT_CBUS_CLK24MHz = 0x0A;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Clk12MHz.
            /// </summary>
            public const byte FT_CBUS_CLK12MHz = 0x0B;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Clk6MHz.
            /// </summary>
            public const byte FT_CBUS_CLK6MHz = 0x0C;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - BCD_Charger.
            /// </summary>
            public const byte FT_CBUS_BCD_Charger = 0x0D;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - BCD_Charger#.
            /// </summary>
            public const byte FT_CBUS_BCD_Charger_N = 0x0E;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - I2C_TXE#.
            /// </summary>
            public const byte FT_CBUS_I2C_TXE = 0x0F;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - I2C_RXF#.
            /// </summary>
            public const byte FT_CBUS_I2C_RXF = 0x10;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - VBUS_Sense.
            /// </summary>
            public const byte FT_CBUS_VBUS_Sense = 0x11;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - BitBang_WR#.
            /// </summary>
            public const byte FT_CBUS_BitBang_WR = 0x12;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - BitBang_RD#.
            /// </summary>
            public const byte FT_CBUS_BitBang_RD = 0x13;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Time_Stampe.
            /// </summary>
            public const byte FT_CBUS_Time_Stamp = 0x14;

            /// <summary>
            /// FT X-Series CBUS EEPROM options - Keep_Awake#
            /// </summary>
            public const byte FT_CBUS_Keep_Awake = 0x15;
        }
    }
}
