using System;
using System.Collections.Generic;
using System.Text;

namespace FTD2xxNet
{
    public partial class FTDI
    {
        /// <summary>
        /// Permitted data bits for FTDI devices.
        /// </summary>
        public class FT_DATA_BITS
        {
            /// <summary>
            /// 8 data bits.
            /// </summary>
            public const byte FT_BITS_8 = 0x08;

            /// <summary>
            /// 7 data bits.
            /// </summary>
            public const byte FT_BITS_7 = 0x07;
        }

        /// <summary>
        /// Permitted stop bits for FTDI devices.
        /// </summary>
        public class FT_STOP_BITS
        {
            /// <summary>
            /// 1 stop bit.
            /// </summary>
            public const byte FT_STOP_BITS_1 = 0x00;

            /// <summary>
            /// 2 stop bits.
            /// </summary>
            public const byte FT_STOP_BITS_2 = 0x02;
        }

        /// <summary>
        /// Permitted parity values for FTDI devices.
        /// </summary>
        public class FT_PARITY
        {
            /// <summary>
            /// No parity.
            /// </summary>
            public const byte FT_PARITY_NONE = 0x00;

            /// <summary>
            /// Odd parity.
            /// </summary>
            public const byte FT_PARITY_ODD = 0x01;

            /// <summary>
            /// Even parity.
            /// </summary>
            public const byte FT_PARITY_EVEN = 0x02;

            /// <summary>
            /// Mark parity.
            /// </summary>
            public const byte FT_PARITY_MARK = 0x03;

            /// <summary>
            /// Space parity.
            /// </summary>
            public const byte FT_PARITY_SPACE = 0x04;
        }

        /// <summary>
        /// Permitted flow control values for FTDI devices
        /// </summary>
        public class FT_FLOW_CONTROL
        {
            /// <summary>
            /// No flow control.
            /// </summary>
            public const ushort FT_FLOW_NONE = 0x0000;

            /// <summary>
            /// RTS/CTS flow control.
            /// </summary>
            public const ushort FT_FLOW_RTS_CTS = 0x0100;

            /// <summary>
            /// DTR/DSR flow control.
            /// </summary>
            public const ushort FT_FLOW_DTR_DSR = 0x0200;

            /// <summary>
            /// Xon/Xoff flow control.
            /// </summary>
            public const ushort FT_FLOW_XON_XOFF = 0x0400;
        }

        /// <summary>
        /// Purge buffer constant definitions.
        /// </summary>
        /// <remarks>Purge Rx and Tx buffers.</remarks>
        public class FT_PURGE
        {
            /// <summary>
            /// Purge Rx buffer.
            /// </summary>
            public const byte FT_PURGE_RX = 0x01;

            /// <summary>
            /// Purge Tx buffer.
            /// </summary>
            public const byte FT_PURGE_TX = 0x02;
        }

        /// <summary>
        /// FTDI device event types that can be monitored.
        /// </summary>
        public class FT_EVENTS
        {
            /// <summary>
            /// Event on receive character.
            /// </summary>
            public const uint FT_EVENT_RXCHAR = 0x00000001;

            /// <summary>
            /// Event on modem status change.
            /// </summary>
            public const uint FT_EVENT_MODEM_STATUS = 0x00000002;

            /// <summary>
            /// Event on line status change.
            /// </summary>
            public const uint FT_EVENT_LINE_STATUS = 0x00000004;
        }

        /// <summary>
        /// Modem status bit definitions.
        /// </summary>
        public class FT_MODEM_STATUS
        {
            /// <summary>
            /// Clear To Send (CTS) modem status.
            /// </summary>
            public const byte FT_CTS = 0x10;

            /// <summary>
            /// Data Set Ready (DSR) modem status.
            /// </summary>
            public const byte FT_DSR = 0x20;

            /// <summary>
            /// Ring Indicator (RI) modem status.
            /// </summary>
            public const byte FT_RI = 0x40;

            /// <summary>
            /// Data Carrier Detect (DCD) modem status.
            /// </summary>
            public const byte FT_DCD = 0x80;
        }

        // Line Status bits
        /// <summary>
        /// Line status bit definitions.
        /// </summary>
        public class FT_LINE_STATUS
        {
            /// <summary>
            /// Overrun Error (OE) line status.
            /// </summary>
            public const byte FT_OE = 0x02;

            /// <summary>
            /// Parity Error (PE) line status.
            /// </summary>
            public const byte FT_PE = 0x04;

            /// <summary>
            /// Framing Error (FE) line status.
            /// </summary>
            public const byte FT_FE = 0x08;

            /// <summary>
            /// Break Interrupt (BI) line status.
            /// </summary>
            public const byte FT_BI = 0x10;
        }

        /// <summary>
        /// Permitted bit mode values for FTDI devices. For use with SetBitMode.
        /// </summary>
        public class FT_BIT_MODES
        {
            /// <summary>
            /// Reset bit mode.
            /// </summary>
            public const byte FT_BIT_MODE_RESET = 0x00;

            /// <summary>
            /// Asynchronous bit-bang mode.
            /// </summary>
            public const byte FT_BIT_MODE_ASYNC_BITBANG = 0x01;

            /// <summary>
            /// MPSSE bit mode - only available on FT2232, FT2232H, FT4232H and FT232H.
            /// </summary>
            public const byte FT_BIT_MODE_MPSSE = 0x02;

            /// <summary>
            /// Synchronous bit-bang mode.
            /// </summary>
            public const byte FT_BIT_MODE_SYNC_BITBANG = 0x04;

            /// <summary>
            /// MCU host bus emulation mode - only available on FT2232, FT2232H, FT4232H and FT232H.
            /// </summary>
            public const byte FT_BIT_MODE_MCU_HOST = 0x08;

            /// <summary>
            /// Fast opto-isolated serial mode - only available on FT2232, FT2232H, FT4232H and FT232H.
            /// </summary>
            public const byte FT_BIT_MODE_FAST_SERIAL = 0x10;

            /// <summary>
            /// CBUS bit-bang mode - only available on FT232R and FT232H.
            /// </summary>
            public const byte FT_BIT_MODE_CBUS_BITBANG = 0x20;

            /// <summary>
            /// Single channel synchronous 245 FIFO mode - only available on FT2232H channel A and FT232H.
            /// </summary>
            public const byte FT_BIT_MODE_SYNC_FIFO = 0x40;
        }

        /// <summary>
        /// Valid values for drive current options on FT2232H, FT4232H and FT232H devices.
        /// </summary>
        public class FT_DRIVE_CURRENT
        {
            /// <summary>
            /// 4mA drive current.
            /// </summary>
            public const byte FT_DRIVE_CURRENT_4MA = 4;

            /// <summary>
            /// 8mA drive current.
            /// </summary>
            public const byte FT_DRIVE_CURRENT_8MA = 8;

            /// <summary>
            /// 12mA drive current.
            /// </summary>
            public const byte FT_DRIVE_CURRENT_12MA = 12;

            /// <summary>
            /// 16mA drive current.
            /// </summary>
            public const byte FT_DRIVE_CURRENT_16MA = 16;
        }
    }
}
