using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace FTD2xxNet
{
    /// <summary>
    /// Wrapper class for FTD2XX.DLL
    /// </summary>
    public partial class FTDI
    {
        #region Constants

        // Flags for FT_OpenEx
        private const uint FT_OPEN_BY_SERIAL_NUMBER = 0x01;
        private const uint FT_OPEN_BY_DESCRIPTION = 0x02;
        private const uint FT_OPEN_BY_LOCATION = 0x04;

        // Default values.
        private const uint FT_DEFAULT_BAUD_RATE = 9600;
        private const uint FT_DEFAULT_DEADMAN_TIMEOUT = 5000;
        private const int FT_COM_PORT_NOT_ASSIGNED = -1;
        private const uint FT_DEFAULT_IN_TRANSFER_SIZE = 0x1000;
        private const uint FT_DEFAULT_OUT_TRANSFER_SIZE = 0x1000;
        private const byte FT_DEFAULT_LATENCY = 16;
        private const uint FT_DEFAULT_DEVICE_ID = 0x04036001;

        #endregion

        #region Private Fields

        private IntPtr handle = IntPtr.Zero;

        #endregion

        #region Constructor
        /// <summary>
        /// Creates a new instance of the FTDI class.
        /// </summary>
        public FTDI()
        { }

        #endregion

        #region Public Methods

        /// <summary>
        /// Gets the number of FTDI devices available.  
        /// </summary>
        /// <returns>FT_STATUS value from FT_CreateDeviceInfoList in FTD2XX.DLL</returns>
        /// <param name="deviceCount">The number of FTDI devices available.</param>
        /// <returns>FT_STATUS value from FT_CreateDeviceInfoList in FTD2XX.DLL</returns>
        public FT_STATUS GetNumberOfDevices(ref uint deviceCount)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_CreateDeviceInfoList(ref deviceCount);
            return status;
        }

        /// <summary>
        /// Gets information on all of the FTDI devices available.  
        /// </summary>
        /// <param name="deviceList">An array of type FT_DEVICE_INFO_NODE to contain the device information for all available devices.</param>
        /// /// <param name="deviceCount">The number of FTDI devices available.</param>
        /// <returns>FT_STATUS value from FT_GetDeviceInfoList in FTD2XX.DLL</returns>
        public FT_STATUS GetDeviceInfoList(ref FT_DEVICE_INFO_NODE[] deviceList, ref uint deviceCount)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FTDI_API.FT_DEVICE_LIST_INFO_NODE deviceInfoNode;

            status = FTDI_API.FT_CreateDeviceInfoList(ref deviceCount);
            if (deviceCount > 0)
            {
                deviceList = new FT_DEVICE_INFO_NODE[deviceCount];

                // Get the size of the device info structure and allocate memory on the heap.
                int size = Marshal.SizeOf<FTDI_API.FT_DEVICE_LIST_INFO_NODE>();
                IntPtr listPointer = Marshal.AllocHGlobal(size * (int)deviceCount);
                IntPtr currentPointer = listPointer;    // Tracking pointer.

                status = FTDI_API.FT_GetDeviceInfoList(listPointer, ref deviceCount);
                if (status == FT_STATUS.FT_OK)
                {
                    // Copy the native data to the managed class type.
                    for (uint index = 0; index < deviceCount; index++, currentPointer += size)
                    {
                        deviceInfoNode = Marshal.PtrToStructure<FTDI_API.FT_DEVICE_LIST_INFO_NODE>(currentPointer);
                        deviceList[index] = new FT_DEVICE_INFO_NODE();
                        deviceList[index].Flags = deviceInfoNode.Flags;
                        deviceList[index].Type = (FT_DEVICE)deviceInfoNode.Type;
                        deviceList[index].ID = deviceInfoNode.ID;
                        deviceList[index].LocId = deviceInfoNode.LocId;
                        deviceList[index].SerialNumber = GetString(deviceInfoNode.SerialNumber);
                        deviceList[index].Description = GetString(deviceInfoNode.Description);
                        deviceList[index].Handle = deviceInfoNode.Handle;
                    }
                }

                // Free the unmanaged buffer.
                Marshal.FreeHGlobal(listPointer);
            }

            return status;
        }

        /// <summary>
        /// Gets information on the FTDI device referenced by index..
        /// </summary>
        /// <param name="index">index of the entry in the device info list.</param>
        /// <param name="deviceInfo">struct to return the device information.</param>
        /// <returns>FT_STATUS value from FT_GetDeviceInfoDetail in FTD2XX.DLL</returns>
        public FT_STATUS GetDeviceInfoDetail(uint index, FT_DEVICE_INFO_NODE deviceInfo)
        {
            byte[] serialNumber = new byte[16];
            byte[] description = new byte[64];
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_GetDeviceInfoDetail(index, 
                                                     ref deviceInfo.Flags,
                                                     ref deviceInfo.Type, ref deviceInfo.ID,
                                                     ref deviceInfo.LocId, serialNumber, description,
                                                     ref deviceInfo.Handle);

            deviceInfo.SerialNumber = GetString(serialNumber);
            deviceInfo.Description = GetString(description);

            return status;
        }


        /// <summary>
        /// Opens the FTDI device with the specified index.  
        /// </summary>
        /// <param name="deviceIndex">Index of the device to open.
        /// Note that this cannot be guaranteed to open a specific device.</param>
        /// <returns>FT_STATUS value from FT_Open in FTD2XX.DLL</returns>
        /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenByIndex(int deviceIndex)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_Open(deviceIndex, ref handle);
            if (status == FT_STATUS.FT_OK && handle != IntPtr.Zero)
            {
                status = InitialiseDevice();
            }

            else
            {
                handle = IntPtr.Zero;
            }

            return status;
        }

        /// <summary>
        /// Opens the FTDI device with the specified serial number.  
        /// </summary>
        /// <param name="serialNumber">Serial number of the device to open.</param>
        /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
        /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenBySerialNumber(string serialNumber)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_OpenEx(serialNumber, FT_OPEN_BY_SERIAL_NUMBER, ref handle);
            if (status == FT_STATUS.FT_OK && handle != IntPtr.Zero)
            {
                status = InitialiseDevice();
            }

            else
            {
                handle = IntPtr.Zero;
            }

            return status;
        }

        /// <summary>
        /// Opens the FTDI device with the specified description.  
        /// </summary>
        /// <param name="description">Description of the device to open.</param>
        /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
        /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenByDescription(string description)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_OpenEx(description, FT_OPEN_BY_DESCRIPTION, ref handle);
            if (status == FT_STATUS.FT_OK && handle != IntPtr.Zero)
            {
                status = InitialiseDevice();
            }

            else
            {
                handle = IntPtr.Zero;
            }

            return status;
        }

        /// <summary>
        /// Opens the FTDI device at the specified physical location.  
        /// </summary>
        /// <param name="location">Location of the device to open.</param>
        /// <returns>FT_STATUS value from FT_OpenEx in FTD2XX.DLL</returns>
        /// <remarks>Initialises the device to 8 data bits, 1 stop bit, no parity, no flow control and 9600 Baud.</remarks>
        public FT_STATUS OpenByLocation(uint location)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_OpenEx(location, FT_OPEN_BY_LOCATION, ref handle);
            if (status == FT_STATUS.FT_OK && handle != IntPtr.Zero)
            {
                status = InitialiseDevice();
            }

            else
            {
                handle = IntPtr.Zero;
            }

            return status;
        }

        /// <summary>
        /// Closes the handle to an open FTDI device.  
        /// </summary>
        /// <returns>FT_STATUS value from FT_Close in FTD2XX.DLL</returns>
        public FT_STATUS Close()
        {
            FT_STATUS status = FT_STATUS.FT_DEVICE_NOT_OPENED;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_Close(handle);
            }

            handle = IntPtr.Zero;
            return status;
        }

        /// <summary>
        /// Read data from an open FTDI device.
        /// </summary>
        /// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device.</param>
        /// <param name="bytesToRead">The number of bytes requested from the device.</param>
        /// <param name="bytesRead">The number of bytes actually read.</param>
        /// <returns>FT_STATUS value from FT_Read in FTD2XX.DLL</returns>
        public FT_STATUS Read(byte[] dataBuffer, uint bytesToRead, ref uint bytesRead)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // If the buffer is not big enough to receive the amount of data requested, adjust the number of bytes to read.
            if (dataBuffer.Length < bytesToRead)
            {
                bytesToRead = (uint)dataBuffer.Length;
            }

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_Read(handle, dataBuffer, bytesToRead, ref bytesRead);
            }

            return status;
        }

        /// <summary>
        /// Read data from an open FTDI device.
        /// </summary>
        /// <param name="stringData">A string containing the data read</param>
        /// <param name="bytesToRead">The number of bytes requested from the device.</param>
        /// <param name="bytesRead">The number of bytes actually read.</param>
        /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
        public FT_STATUS Read(out string stringData, uint bytesToRead, ref uint bytesRead)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            stringData = string.Empty;
            byte[] dataBuffer = new byte[bytesToRead];

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_Read(handle, dataBuffer, bytesToRead, ref bytesRead);
                if (status == FT_STATUS.FT_OK)
                {
                    stringData = Encoding.ASCII.GetString(dataBuffer);
                }
            }

            return status;
        }

        /// <summary>
        /// Write data to an open FTDI device.
        /// </summary>
        /// <param name="dataBuffer">An array of bytes which contains the data to be written to the device.</param>
        /// <param name="bytesToWrite">The number of bytes to be written to the device.</param>
        /// <param name="bytesWritten">The number of bytes actually written to the device.</param>
        /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
        public FT_STATUS Write(byte[] dataBuffer, uint bytesToWrite, ref uint bytesWritten)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_Write(handle, dataBuffer, bytesToWrite, ref bytesWritten);
            }

            return status;
        }

        /// <summary>
        /// Write data to an open FTDI device.
        /// </summary>
        /// <param name="stringData">A  string which contains the data to be written to the device.</param>
        /// <param name="bytesToWrite">The number of bytes to be written to the device.</param>
        /// <param name="bytesWritten">The number of bytes actually written to the device.</param>
        /// <returns>FT_STATUS value from FT_Write in FTD2XX.DLL</returns>
        public FT_STATUS Write(string stringData, UInt32 bytesToWrite, ref UInt32 bytesWritten)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                byte[] dataBuffer = Encoding.ASCII.GetBytes(stringData);
                status = FTDI_API.FT_Write(handle, dataBuffer, bytesToWrite, ref bytesWritten);
            }

            return status;
        }

        /// <summary>
        /// Reset an open FTDI device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_ResetDevice in FTD2XX.DLL</returns>
        public FT_STATUS ResetDevice()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_ResetDevice(handle);
            }

            return status;
        }

        /// <summary>
        /// Purges data from the devices transmit and/or receive buffers.
        /// </summary>
        /// <param name="purgeMask">Specifies which buffer(s) to be purged.</param>
        /// <remarks>
        /// Valid values are any combination of the following flags: FT_PURGE_RX, FT_PURGE_TX.</remarks>
        /// <returns>FT_STATUS value from FT_Purge in FTD2XX.DLL</returns>
        public FT_STATUS Purge(uint purgeMask)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_Purge(handle, purgeMask);
            }

            return status;
        }

        /// <summary>
        /// Register for event notification.
        /// </summary>
        /// <remarks>After setting event notification, the event can be caught by executing the WaitOne() method of the EventWaitHandle.  If multiple event types are being monitored, the event that fired can be determined from the GetEventType method.</remarks>
        /// <param name="eventMask">The type of events to signal.  Can be any combination of the following: FT_EVENT_RXCHAR, FT_EVENT_MODEM_STATUS, FT_EVENT_LINE_STATUS</param>
        /// <param name="eventHandle">Handle to the event that will receive the notification</param>
        /// <returns>FT_STATUS value from FT_SetEventNotification in FTD2XX.DLL</returns>
        public FT_STATUS SetEventNotification(uint eventMask, EventWaitHandle eventHandle)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetEventNotification(handle, eventMask, eventHandle.SafeWaitHandle);
            }

            return status;
        }

        /// <summary>
        /// Stops the driver issuing USB in requests.
        /// </summary>
        /// <returns>FT_STATUS value from FT_StopInTask in FTD2XX.DLL</returns>
        public FT_STATUS StopInTask()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_StopInTask(handle);
            }

            return status;
        }

        /// <summary>
        /// Resumes the driver issuing USB in requests.
        /// </summary>
        /// <returns>FT_STATUS value from FT_RestartInTask in FTD2XX.DLL</returns>
        public FT_STATUS RestartInTask()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_RestartInTask(handle);
            }

            return status;
        }

        /// <summary>
        /// Resets the device port.
        /// </summary>
        /// <returns>FT_STATUS value from FT_ResetPort in FTD2XX.DLL</returns>
        public FT_STATUS ResetPort()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_ResetPort(handle);
            }

            return status;
        }

        /// <summary>
        /// Causes the device to be re-enumerated on the USB bus. This is equivalent to unplugging and replugging the device.
        /// </summary>
        /// <returns>FT_STATUS value from FT_CyclePort in FTD2XX.DLL</returns>
        /// <remarks>Also calls FT_Close if FT_CyclePort if successful, so no need to call this separately in the application.</remarks>
        public FT_STATUS CyclePort()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_CyclePort(handle);
                if (status == FT_STATUS.FT_OK)
                {
                    // If successful, call FT_Close.
                    status = FTDI_API.FT_Close(handle);
                    if (status == FT_STATUS.FT_OK)
                    {
                        handle = IntPtr.Zero;
                    }
                }
            }

            return status;
        }

        /// <summary>
        /// Causes the system to check for USB hardware changes.
        /// </summary>
        /// <returns>FT_STATUS value from FT_Rescan in FTD2XX.DLL</returns>
        /// <remarks>This is equivalent to clicking on the "Scan for hardware changes" button in the Device Manager.</remarks>
        public FT_STATUS Rescan()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_Rescan();
            return status;
        }

        /// <summary>
        /// Forces a reload of the driver for devices with a specific VID and PID combination.
        /// </summary>
        /// <param name="vendorId">Vendor ID of the devices to have the driver reloaded</param>
        /// <param name="productId">Product ID of the devices to have the driver reloaded</param>
        /// <returns>FT_STATUS value from FT_Reload in FTD2XX.DLL</returns>
        /// <remarks>If the VID and PID parameters are 0, the drivers for USB root hubs will be reloaded, causing all USB devices connected to reload their drivers</remarks>
        public FT_STATUS Reload(ushort vendorId, ushort productId)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_Reload(vendorId, productId);
            return status;
        }

        /// <summary>
        /// puts the device in a mode other than the default UART or FIFO mode.
        /// </summary>
        /// <param name="mask">Value for mask.</param>
        /// <param name="bitMode">Mode value.</param>
        /// <returns>FT_STATUS value from FT_SetBitMode in FTD2XX.DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not support the requested bit mode.</exception>
        public FT_STATUS SetBitMode(byte mask, byte bitMode)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;

                // Set Bit Mode does not apply to FT8U232AM, FT8U245AM or FT8U100AX devices.
                GetDeviceType(ref deviceType);
                if (deviceType == FT_DEVICE.FT_DEVICE_AM)
                {
                    // Throw an exception.
                    errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(status, errorCondition);
                }

                else if (deviceType == FT_DEVICE.FT_DEVICE_100AX)
                {
                    // Throw an exception.
                    errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                    ErrorHandler(status, errorCondition);
                }

                else if ((deviceType == FT_DEVICE.FT_DEVICE_BM) && (bitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((bitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG)) == 0)
                    {
                        // Throw an exception
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                }

                else if ((deviceType == FT_DEVICE.FT_DEVICE_2232) && (bitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((bitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG |
                                    FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL)) == 0)
                    {
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                    if ((bitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & (InterfaceIdentifier != "A"))
                    {
                        // MPSSE mode is only available on channel A.
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                }
                else if ((deviceType == FT_DEVICE.FT_DEVICE_232R) && (bitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((bitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_CBUS_BITBANG)) == 0)
                    {
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                }
                else if ((deviceType == FT_DEVICE.FT_DEVICE_2232H) && (bitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((bitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG |
                                    FT_BIT_MODES.FT_BIT_MODE_MCU_HOST | FT_BIT_MODES.FT_BIT_MODE_FAST_SERIAL | FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) == 0)
                    {
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                    if (((bitMode == FT_BIT_MODES.FT_BIT_MODE_MCU_HOST) | (bitMode == FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)) & (InterfaceIdentifier != "A"))
                    {
                        // MCU Host Emulation and Single channel synchronous 245 FIFO mode is only available on channel A.
                        // Throw an exception
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                }

                else if ((deviceType == FT_DEVICE.FT_DEVICE_4232H) && (bitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    if ((bitMode & (FT_BIT_MODES.FT_BIT_MODE_ASYNC_BITBANG | FT_BIT_MODES.FT_BIT_MODE_MPSSE | FT_BIT_MODES.FT_BIT_MODE_SYNC_BITBANG)) == 0)
                    {
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                    if ((bitMode == FT_BIT_MODES.FT_BIT_MODE_MPSSE) & ((InterfaceIdentifier != "A") & (InterfaceIdentifier != "B")))
                    {
                        // MPSSE mode is only available on channel A and B.
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                }

                else if ((deviceType == FT_DEVICE.FT_DEVICE_232H) && (bitMode != FT_BIT_MODES.FT_BIT_MODE_RESET))
                {
                    // FT232H supports all current bit modes!
                    if (bitMode > FT_BIT_MODES.FT_BIT_MODE_SYNC_FIFO)
                    {
                        // Throw an exception.
                        errorCondition = FT_ERROR.FT_INVALID_BITMODE;
                        ErrorHandler(status, errorCondition);
                    }
                }

                // Requested bit mode is supported.
                // Note FT_BIT_MODES.FT_BIT_MODE_RESET falls through to here - no bits set so cannot check for AND.
                status = FTDI_API.FT_SetBitMode(handle, mask, bitMode);
            }

            return status;
        }

        /// <summary>
        /// Gets the instantaneous state of the device IO pins.
        /// </summary>
        /// <param name="bitMode">A bitmap value containing the instantaneous state of the device IO pins</param>
        /// <returns>FT_STATUS value from FT_GetBitMode in FTD2XX.DLL</returns>
        public FT_STATUS GetPinStates(ref byte bitMode)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetBitMode(handle, ref bitMode);
            }

            return status;
        }

        /// <summary>
        /// Gets the chip type of the current device.
        /// </summary>
        /// <param name="deviceType">The FTDI chip type of the current device.</param>
        /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
        public FT_STATUS GetDeviceType(ref FT_DEVICE deviceType)
        {
            uint deviceID = 0;
            byte[] serialNumber = new byte[16];
            byte[] description = new byte[64];

            deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetDeviceInfo(handle, ref deviceType, ref deviceID, serialNumber, description, IntPtr.Zero);
            }

            return status;
        }

        /// <summary>
        /// Gets the Vendor ID and Product ID of the current device.
        /// </summary>
        /// <param name="deviceId">The device ID (Vendor ID and Product ID) of the current device.</param>
        /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
        public FT_STATUS GetDeviceId(ref uint deviceId)
        {
            byte[] serialNumber = new byte[16];
            byte[] description = new byte[64];

            FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetDeviceInfo(handle, ref deviceType, ref deviceId, serialNumber, description, IntPtr.Zero);
            }

            return status;
        }

        /// <summary>
        /// Gets the description of the current device.
        /// </summary>
        /// <param name="deviceDescription">The description of the current device.</param>
        /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
        public FT_STATUS GetDescription(out string deviceDescription)
        {
            uint deviceID = 0;
            deviceDescription = string.Empty;
            byte[] serialNum = new byte[16];
            byte[] description = new byte[64];

            FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetDeviceInfo(handle, ref deviceType, ref deviceID, serialNum, description, IntPtr.Zero);
                deviceDescription = Encoding.ASCII.GetString(description);
                deviceDescription = deviceDescription.Substring(0, deviceDescription.IndexOf("\0"));
            }

            return status;
        }

        /// <summary>
        /// Gets the serial number of the current device.
        /// </summary>
        /// <param name="deviceSerialNumber">The serial number of the current device.</param>
        /// <returns>FT_STATUS value from FT_GetDeviceInfo in FTD2XX.DLL</returns>
        public FT_STATUS GetSerialNumber(out string deviceSerialNumber)
        {
            uint deviceID = 0;
            deviceSerialNumber = string.Empty;
            byte[] serialNumber = new byte[16];
            byte[] description = new byte[64];

            FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetDeviceInfo(handle, ref deviceType, ref deviceID, serialNumber, description, IntPtr.Zero);
                deviceSerialNumber = Encoding.ASCII.GetString(serialNumber);
                deviceSerialNumber = deviceSerialNumber.Substring(0, deviceSerialNumber.IndexOf("\0"));
            }

            return status;
        }

        /// <summary>
        /// Gets the number of bytes available in the receive buffer.
        /// </summary>
        /// <param name="queueCountRx">The number of bytes available to be read.</param>
        /// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
        public FT_STATUS GetQueueStatus(ref uint queueCountRx)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetQueueStatus(handle, ref queueCountRx);
            }

            return status;
        }

        /// <summary>
        /// Gets the number of bytes available in the receive buffer.
        /// </summary>
        /// <param name="queueCountRx">The number of bytes available to be read.</param>
        /// /// <returns>FT_STATUS value from FT_GetQueueStatus in FTD2XX.DLL</returns>
        public FT_STATUS GetRxBytesAvailable(ref uint queueCountRx)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetQueueStatus(handle, ref queueCountRx);
            }

            return status;
        }

        /// <summary>
        /// Gets the number of bytes waiting in the transmit buffer.
        /// </summary>
        /// <param name="queueCountTx">The number of bytes waiting to be sent.</param>
        /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
        public FT_STATUS GetTxBytesWaiting(ref uint queueCountTx)
        {
            uint queueCountRx = 0;
            uint eventStatus = 0;

            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetStatus(handle, ref queueCountRx, ref queueCountTx, ref eventStatus);
            }

            return status;
        }

        /// <summary>
        /// Gets the event type after an event has fired.
        /// Can be used to distinguish which event has been triggered when waiting on multiple event types.
        /// </summary>
        /// <param name="eventStatus">The type of event that has occurred</param>
        /// <returns>FT_STATUS value from FT_GetStatus in FTD2XX.DLL</returns>
        public FT_STATUS GetEventType(ref uint eventStatus)
        {
            uint queueCountRx = 0;
            uint queueCountTx = 0;

            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetStatus(handle, ref queueCountRx, ref queueCountTx, ref eventStatus);
            }

            return status;
        }

        /// <summary>
        /// Gets the current modem and line status.
        /// </summary>
        /// <param name="modemStatus">A bit map representaion of the current modem status.</param>
        /// <param name="lineStatus">A bit map representaion of the current line status.</param>
        /// <returns>FT_STATUS value from FT_GetModemStatus in FTD2XX.DLL</returns>
        public FT_STATUS GetModemStatus(ref byte modemStatus, ref byte lineStatus)
        {
            uint modemLineStatus = 0;

            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetModemStatus(handle, ref modemLineStatus);
            }

            modemStatus = Convert.ToByte(modemLineStatus & 0x000000ff);
            lineStatus = Convert.ToByte((modemLineStatus >> 8) & 0x000000ff);

            return status;
        }

        /// <summary>
        /// Sets the current baud rate.
        /// </summary>
        /// <param name="baudRate">The desired Baud rate for the device.</param>
        /// <returns>FT_STATUS value from FT_SetBaudRate in FTD2XX.DLL</returns>
        public FT_STATUS SetBaudRate(uint baudRate)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetBaudRate(handle, baudRate);
            }

            return status;
        }

        /// <summary>
        /// Sets the data bits, stop bits and parity for the device.
        /// </summary>
        /// <param name="dataBits">The number of data bits for UART data.</param>
        /// <param name="stopBits">The number of stop bits for UART data.</param>
        /// <param name="parity">The parity of the UART data.</param>
        /// <returns>FT_STATUS value from FT_SetDataCharacteristics in FTD2XX.DLL</returns>
        public FT_STATUS SetDataCharacteristics(byte dataBits, byte stopBits, byte parity)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetDataCharacteristics(handle, dataBits, stopBits, parity);
            }

            return status;
        }

        /// <summary>
        /// Sets the flow control type.
        /// </summary>
        /// <param name="flowControl">The type of flow control for the UART.</param>
        /// <param name="xOn">The type of flow control for the UART.</param>
        /// <param name="xOff">The Xoff character for Xon/Xoff flow control.</param>
        /// <returns>FT_STATUS value from FT_SetFlowControl in FTD2XX.DLL</returns>
        public FT_STATUS SetFlowControl(ushort flowControl, byte xOn, byte xOff)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetFlowControl(handle, flowControl, xOn, xOff);
            }

            return status;
        }

        /// <summary>
        /// Sets the state of the Request To Send (RTS) line.
        /// </summary>
        /// <param name="enable">If true sets RTS high, if false sets RTS low.</param>
        /// <returns>FT_STATUS value from FT_SetRts or FT_ClrRts in FTD2XX.DLL</returns>
        public FT_STATUS SetRTS(bool enable)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = enable ? FTDI_API.FT_SetRts(handle) : FTDI_API.FT_ClrRts(handle);
            }

            return status;
        }

        /// <summary>
        /// Sets the state of the Data Terminal Ready (DTR) line.
        /// </summary>
        /// <param name="enable">If true sets DTR high, if false sets DRT low.</param>
        /// <returns>FT_STATUS value from FT_SetDtr or FT_ClrDtr in FTD2XX.DLL</returns>
        public FT_STATUS SetDTR(bool enable)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = enable ? FTDI_API.FT_SetDtr(handle) : FTDI_API.FT_ClrDtr(handle);
            }

            return status;
        }

        /// <summary>
        /// Sets the read and write timeout values.
        /// </summary>
        /// <param name="readTimeout">Read timeout value in ms. A value of 0 indicates an infinite timeout.</param>
        /// <param name="writeTimeout">Write timeout value in ms. A value of 0 indicates an infinite timeout.</param>
        /// <returns>FT_STATUS value from FT_SetTimeouts in FTD2XX.DLL</returns>
        public FT_STATUS SetTimeouts(uint readTimeout, uint writeTimeout)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetTimeouts(handle, readTimeout, writeTimeout);
            }

            return status;
        }

        /// <summary>
        /// Sets or clears the break state.
        /// </summary>
        /// <param name="enable">If true, sets break on. If false, sets break off.</param>
        /// <returns>FT_STATUS value from FT_SetBreakOn or FT_SetBreakOff in FTD2XX.DLL</returns>
        public FT_STATUS SetBreakState(bool enable)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = enable ? FTDI_API.FT_SetBreakOn(handle) : FTDI_API.FT_SetBreakOff(handle);
            }

            return status;
        }

        /// <summary>
        /// Sets the reset pipe retry count..
        /// </summary>
        /// <param name="resetPipeRetryCount">The reset pipe retry count. Default value is 50.</param>
        /// <returns>FT_STATUS value from FT_SetResetPipeRetryCount in FTD2XX.DLL</returns>
        /// <remarks>Electrically noisy environments may benefit from a larger value.</remarks>
        public FT_STATUS SetResetPipeRetryCount(uint resetPipeRetryCount)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetResetPipeRetryCount(handle, resetPipeRetryCount);
            }

            return status;
        }

        /// <summary>
        /// Gets the current FTDIBUS.SYS driver version number.
        /// </summary>
        /// <param name="driverVersion">The current driver version number.</param>
        /// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
        public FT_STATUS GetDriverVersion(ref uint driverVersion)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetDriverVersion(handle, ref driverVersion);
            }

            return status;
        }

        /// <summary>
        /// Gets the current FTDIBUS.SYS driver version number.
        /// </summary>
        /// <param name="version">The current driver version number as a string.</param>
        /// <returns>FT_STATUS value from FT_GetDriverVersion in FTD2XX.DLL</returns>
        public FT_STATUS GetDriverVersion(out string version)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            uint driverVersion = 0;
            version = string.Empty;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetDriverVersion(handle, ref driverVersion);
                byte major = Convert.ToByte((driverVersion >> 16) & 0xff);
                byte minor = Convert.ToByte((driverVersion >> 8) & 0xff);
                byte build = Convert.ToByte(driverVersion & 0xff);
                version = string.Format("{0}.{1, 0:D2}.{2, 0:D2}", major, minor, build);
            }

            return status;
        }

        /// <summary>
        /// Gets the current FTDIBUS.SYS library version number.
        /// </summary>
        /// <param name="libraryVersion">The current library version number.</param>
        /// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
        public FT_STATUS GetLibraryVersion(ref uint libraryVersion)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            status = FTDI_API.FT_GetLibraryVersion(ref libraryVersion);
            return status;
        }

        /// <summary>
        /// Gets the current FTDIBUS.SYS library version number.
        /// </summary>
        /// <param name="version">The current library version number as a string.</param>
        /// <returns>FT_STATUS value from FT_GetLibraryVersion in FTD2XX.DLL</returns>
        public FT_STATUS GetLibraryVersion(out string version)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            uint libraryVersion = 0;
            version = string.Empty;


            status = FTDI_API.FT_GetLibraryVersion(ref libraryVersion);
            if (status == FT_STATUS.FT_OK)
            {
                byte major = Convert.ToByte((libraryVersion >> 16) & 0xff);
                byte minor = Convert.ToByte((libraryVersion >> 8) & 0xff);
                byte build = Convert.ToByte(libraryVersion & 0xff);
                version = string.Format("{0}.{1, 0:D2}.{2, 0:D2}", major, minor, build);
            }

            return status;
        }

        /// <summary>
        /// Sets the USB deadman timeout value.
        /// </summary>
        /// <param name="deadmanTimeout">The deadman timeout value in ms. Default is 5000ms.</param>
        /// <returns>FT_STATUS value from FT_SetDeadmanTimeout in FTD2XX.DLL</returns>
        public FT_STATUS SetDeadmanTimeout(uint deadmanTimeout)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetDeadmanTimeout(handle, deadmanTimeout);
            }

            return status;
        }

        /// <summary>
        /// Sets the value of the latency timer. Default value is 16ms.
        /// </summary>
        /// <param name="latency">The latency timer value in ms.</param>
        /// <returns>FT_STATUS value from FT_SetLatencyTimer in FTD2XX.DLL</returns>
        /// <remarks>
        /// Valid values are 2ms - 255ms for FT232BM, FT245BM and FT2232 devices.
        /// Valid values are 0ms - 255ms for other devices.
        /// </remarks>
        public FT_STATUS SetLatency(byte latency)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                if ((deviceType == FT_DEVICE.FT_DEVICE_BM) || (deviceType == FT_DEVICE.FT_DEVICE_2232))
                {
                    // Do not allow latency of 1ms or 0ms for older devices, since this can cause problems/lock ups due to buffering mechanism.
                    if (latency < 2)
                    {
                        latency = 2;
                    }
                }

                status = FTDI_API.FT_SetLatencyTimer(handle, latency);
            }

            return status;
        }

        /// <summary>
        /// Gets the value of the latency timer.
        /// </summary>
        /// <param name="latency">The latency timer value in ms.</param>
        /// <returns>The latency timer value in ms.</returns>
        public FT_STATUS GetLatency(ref byte latency)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetLatencyTimer(handle, ref latency);
            }

            return status;
        }

        /// <summary>
        /// Sets the USB In and Out transfer sizes.
        /// </summary>
        /// <param name="inTransferSize">InTransferSize">The USB IN transfer size in bytes.</param>
        /// <returns>FT_STATUS value from FT_SetUSBParameters in FTD2XX.DLL</returns>
        public FT_STATUS InTransferSize(uint inTransferSize)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            uint outTransferSize = inTransferSize;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetUSBParameters(handle, inTransferSize, outTransferSize);
            }

            return status;
        }

        /// <summary>
        ///  Sets an event character, an error character and enables or disables them.
        /// </summary>
        /// <param name="eventChar">A character that will be tigger an IN to the host when this character is received.</param>
        /// <param name="eventCharEnable">Determines if the 'eventChar' is enabled or disabled.</param>
        /// <param name="errorChar">A character that will be inserted into the data stream to indicate that an error has occurred.</param>
        /// <param name="errorCharEnable">Determines if the 'errorChar' is enabled or disabled.</param>
        /// <returns>FT_STATUS value from FT_SetChars in FTD2XX.DLL</returns>
        public FT_STATUS SetCharacters(byte eventChar, bool eventCharEnable, byte errorChar, bool errorCharEnable)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_SetChars(handle, eventChar, eventCharEnable, errorChar, errorCharEnable);
            }

            return status;
        }

        /// <summary>
        /// Gets the corresponding COM port number for the current device. If no COM port is exposed, an empty string is returned.
        /// </summary>
        /// <param name="commPortName">The COM port name corresponding to the current device.  If no COM port is installed, an empty string is passed back.</param>
        /// <returns>FT_STATUS value from FT_GetComPortNumber in FTD2XX.DLL</returns>
        public FT_STATUS GetCommPort(out string commPortName)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            commPortName = string.Empty;
            int commPortNumber = FT_COM_PORT_NOT_ASSIGNED;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_GetComPortNumber(handle, ref commPortNumber);
                if (commPortNumber == FT_COM_PORT_NOT_ASSIGNED)
                {
                    commPortName = string.Empty;
                }

                else
                {
                    // If installed, return full COM string.
                    // This can then be passed to an instance of the SerialPort class to assign the port number.
                    commPortName = string.Format("COM{0}", commPortNumber);
                }
            }

            return status;
        }

        /// <summary>
        /// Reads an individual value from a specified location in the device's EEPROM.
        /// </summary>
        /// <param name="address">The EEPROM location to read data from.</param>
        /// <param name="value">The value read from the EEPROM location specified in the 'address' paramter.</param>
        /// <returns>FT_STATUS value from FT_ReadEE in FTD2XX.DLL</returns>
        public FT_STATUS ReadEEPROMLocation(uint address, ushort value)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_ReadEE(handle, address, ref value);
            }

            return status;
        }

        /// <summary>
        /// Writes an individual value to a specified location in the device's EEPROM.
        /// </summary>
        /// <param name="address">The EEPROM location to write data to.</param>
        /// <param name="value">The value to write to the EEPROM location specified by the 'address' parameter.</param>
        /// <returns>FT_STATUS value from FT_WriteEE in FTD2XX.DLL</returns>
        public FT_STATUS WriteEEPROMLocation(uint address, ushort value)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_WriteEE(handle, address, value);
            }

            return status;
        }

        /// <summary>
        /// Erases the device EEPROM.
        /// </summary>
        /// <returns>FT_STATUS value from FT_EraseEE in FTD2XX.DLL</returns>
        public FT_STATUS EraseEEPROM()
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType == FT_DEVICE.FT_DEVICE_232R)
                {
                    // If it is a device with an internal EEPROM, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                status = FTDI_API.FT_EraseEE(handle);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an FT232B or FT245B device.
        /// </summary>
        /// <param name="ee232b">An FT232B_EEPROM_STRUCTURE which contains only the relevant information for an FT232B and FT245B device.</param>
        /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadFT232BEEPROM(FT232B_EEPROM_STRUCTURE ee232b)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_BM)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 2;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                status = FTDI_API.FT_EE_Read(handle, eedata);
                if (status == FT_STATUS.FT_OK)
                {
                    // Retrieve string values.
                    ee232b.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                    ee232b.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                    ee232b.Description = Marshal.PtrToStringAnsi(eedata.Description);
                    ee232b.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    ee232b.VendorID = eedata.VendorID;
                    ee232b.ProductID = eedata.ProductID;
                    ee232b.MaxPower = eedata.MaxPower;
                    ee232b.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                    ee232b.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);

                    // B specific fields.
                    ee232b.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable);
                    ee232b.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable);
                    ee232b.USBVersionEnable = Convert.ToBoolean(eedata.USBVersionEnable);
                    ee232b.USBVersion = eedata.USBVersion;
                }

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an FT2232 device.
        /// </summary>
        /// <param name="ee2232">An FT2232_EEPROM_STRUCTURE which contains only the relevant information for an FT2232 device.</param>
        /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadFT2232EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_2232)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 2;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                status = FTDI_API.FT_EE_Read(handle, eedata);
                if (status == FT_STATUS.FT_OK)
                {
                    // Retrieve string values.
                    ee2232.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                    ee2232.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                    ee2232.Description = Marshal.PtrToStringAnsi(eedata.Description);
                    ee2232.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    ee2232.VendorID = eedata.VendorID;
                    ee2232.ProductID = eedata.ProductID;
                    ee2232.MaxPower = eedata.MaxPower;
                    ee2232.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                    ee2232.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);

                    // 2232 specific fields.
                    ee2232.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable5);
                    ee2232.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable5);
                    ee2232.USBVersionEnable = Convert.ToBoolean(eedata.USBVersionEnable5);
                    ee2232.USBVersion = eedata.USBVersion5;
                    ee2232.AIsHighCurrent = Convert.ToBoolean(eedata.AIsHighCurrent);
                    ee2232.BIsHighCurrent = Convert.ToBoolean(eedata.BIsHighCurrent);
                    ee2232.IFAIsFifo = Convert.ToBoolean(eedata.IFAIsFifo);
                    ee2232.IFAIsFifoTar = Convert.ToBoolean(eedata.IFAIsFifoTar);
                    ee2232.IFAIsFastSer = Convert.ToBoolean(eedata.IFAIsFastSer);
                    ee2232.AIsVCP = Convert.ToBoolean(eedata.AIsVCP);
                    ee2232.IFBIsFifo = Convert.ToBoolean(eedata.IFBIsFifo);
                    ee2232.IFBIsFifoTar = Convert.ToBoolean(eedata.IFBIsFifoTar);
                    ee2232.IFBIsFastSer = Convert.ToBoolean(eedata.IFBIsFastSer);
                    ee2232.BIsVCP = Convert.ToBoolean(eedata.BIsVCP);
                }

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an FT232R or FT245R device.
        /// </summary>
        /// <param name="ee232r">An FT232R_EEPROM_STRUCTURE which contains only the relevant information for an FT232R and FT245R device.</param>
        /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadFT232REEPROM(FT232R_EEPROM_STRUCTURE ee232r)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_232R)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 2;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                status = FTDI_API.FT_EE_Read(handle, eedata);
                if (status == FT_STATUS.FT_OK)
                {
                    // Retrieve string values.
                    ee232r.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                    ee232r.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                    ee232r.Description = Marshal.PtrToStringAnsi(eedata.Description);
                    ee232r.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    ee232r.VendorID = eedata.VendorID;
                    ee232r.ProductID = eedata.ProductID;
                    ee232r.MaxPower = eedata.MaxPower;
                    ee232r.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                    ee232r.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);

                    // 232R specific fields.
                    ee232r.UseExtOsc = Convert.ToBoolean(eedata.UseExtOsc);
                    ee232r.HighDriveIOs = Convert.ToBoolean(eedata.HighDriveIOs);
                    ee232r.EndpointSize = eedata.EndpointSize;
                    ee232r.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnableR);
                    ee232r.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnableR);
                    ee232r.InvertTXD = Convert.ToBoolean(eedata.InvertTXD);
                    ee232r.InvertRXD = Convert.ToBoolean(eedata.InvertRXD);
                    ee232r.InvertRTS = Convert.ToBoolean(eedata.InvertRTS);
                    ee232r.InvertCTS = Convert.ToBoolean(eedata.InvertCTS);
                    ee232r.InvertDTR = Convert.ToBoolean(eedata.InvertDTR);
                    ee232r.InvertDSR = Convert.ToBoolean(eedata.InvertDSR);
                    ee232r.InvertDCD = Convert.ToBoolean(eedata.InvertDCD);
                    ee232r.InvertRI = Convert.ToBoolean(eedata.InvertRI);
                    ee232r.Cbus0 = eedata.Cbus0;
                    ee232r.Cbus1 = eedata.Cbus1;
                    ee232r.Cbus2 = eedata.Cbus2;
                    ee232r.Cbus3 = eedata.Cbus3;
                    ee232r.Cbus4 = eedata.Cbus4;
                    ee232r.RIsD2XX = Convert.ToBoolean(eedata.RIsD2XX);
                }

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an FT2232H device.
        /// </summary>
        /// <param name="ee2232h">An FT2232H_EEPROM_STRUCTURE which contains only the relevant information for an FT2232H device.</param>
        /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadFT2232HEEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_2232H)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 3;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                status = FTDI_API.FT_EE_Read(handle, eedata);
                if (status == FT_STATUS.FT_OK)
                {
                    // Retrieve string values.
                    ee2232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                    ee2232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                    ee2232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
                    ee2232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    ee2232h.VendorID = eedata.VendorID;
                    ee2232h.ProductID = eedata.ProductID;
                    ee2232h.MaxPower = eedata.MaxPower;
                    ee2232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                    ee2232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);

                    // 2232H specific fields.
                    ee2232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable7);
                    ee2232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable7);
                    ee2232h.ALSlowSlew = Convert.ToBoolean(eedata.ALSlowSlew);
                    ee2232h.ALSchmittInput = Convert.ToBoolean(eedata.ALSchmittInput);
                    ee2232h.ALDriveCurrent = eedata.ALDriveCurrent;
                    ee2232h.AHSlowSlew = Convert.ToBoolean(eedata.AHSlowSlew);
                    ee2232h.AHSchmittInput = Convert.ToBoolean(eedata.AHSchmittInput);
                    ee2232h.AHDriveCurrent = eedata.AHDriveCurrent;
                    ee2232h.BLSlowSlew = Convert.ToBoolean(eedata.BLSlowSlew);
                    ee2232h.BLSchmittInput = Convert.ToBoolean(eedata.BLSchmittInput);
                    ee2232h.BLDriveCurrent = eedata.BLDriveCurrent;
                    ee2232h.BHSlowSlew = Convert.ToBoolean(eedata.BHSlowSlew);
                    ee2232h.BHSchmittInput = Convert.ToBoolean(eedata.BHSchmittInput);
                    ee2232h.BHDriveCurrent = eedata.BHDriveCurrent;
                    ee2232h.IFAIsFifo = Convert.ToBoolean(eedata.IFAIsFifo7);
                    ee2232h.IFAIsFifoTar = Convert.ToBoolean(eedata.IFAIsFifoTar7);
                    ee2232h.IFAIsFastSer = Convert.ToBoolean(eedata.IFAIsFastSer7);
                    ee2232h.AIsVCP = Convert.ToBoolean(eedata.AIsVCP7);
                    ee2232h.IFBIsFifo = Convert.ToBoolean(eedata.IFBIsFifo7);
                    ee2232h.IFBIsFifoTar = Convert.ToBoolean(eedata.IFBIsFifoTar7);
                    ee2232h.IFBIsFastSer = Convert.ToBoolean(eedata.IFBIsFastSer7);
                    ee2232h.BIsVCP = Convert.ToBoolean(eedata.BIsVCP7);
                    ee2232h.PowerSaveEnable = Convert.ToBoolean(eedata.PowerSaveEnable);
                }

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an FT4232H device.
        /// </summary>
        /// <param name="ee4232h">An FT4232H_EEPROM_STRUCTURE which contains only the relevant information for an FT4232H device.</param>
        /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadFT4232HEEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_4232H)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 4;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                status = FTDI_API.FT_EE_Read(handle, eedata);
                if (status == FT_STATUS.FT_OK)
                {
                    // Retrieve string values.
                    ee4232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                    ee4232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                    ee4232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
                    ee4232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    ee4232h.VendorID = eedata.VendorID;
                    ee4232h.ProductID = eedata.ProductID;
                    ee4232h.MaxPower = eedata.MaxPower;
                    ee4232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                    ee4232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);

                    // 4232H specific fields.
                    ee4232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnable8);
                    ee4232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnable8);
                    ee4232h.ASlowSlew = Convert.ToBoolean(eedata.ASlowSlew);
                    ee4232h.ASchmittInput = Convert.ToBoolean(eedata.ASchmittInput);
                    ee4232h.ADriveCurrent = eedata.ADriveCurrent;
                    ee4232h.BSlowSlew = Convert.ToBoolean(eedata.BSlowSlew);
                    ee4232h.BSchmittInput = Convert.ToBoolean(eedata.BSchmittInput);
                    ee4232h.BDriveCurrent = eedata.BDriveCurrent;
                    ee4232h.CSlowSlew = Convert.ToBoolean(eedata.CSlowSlew);
                    ee4232h.CSchmittInput = Convert.ToBoolean(eedata.CSchmittInput);
                    ee4232h.CDriveCurrent = eedata.CDriveCurrent;
                    ee4232h.DSlowSlew = Convert.ToBoolean(eedata.DSlowSlew);
                    ee4232h.DSchmittInput = Convert.ToBoolean(eedata.DSchmittInput);
                    ee4232h.DDriveCurrent = eedata.DDriveCurrent;
                    ee4232h.ARIIsTXDEN = Convert.ToBoolean(eedata.ARIIsTXDEN);
                    ee4232h.BRIIsTXDEN = Convert.ToBoolean(eedata.BRIIsTXDEN);
                    ee4232h.CRIIsTXDEN = Convert.ToBoolean(eedata.CRIIsTXDEN);
                    ee4232h.DRIIsTXDEN = Convert.ToBoolean(eedata.DRIIsTXDEN);
                    ee4232h.AIsVCP = Convert.ToBoolean(eedata.AIsVCP8);
                    ee4232h.BIsVCP = Convert.ToBoolean(eedata.BIsVCP8);
                    ee4232h.CIsVCP = Convert.ToBoolean(eedata.CIsVCP8);
                    ee4232h.DIsVCP = Convert.ToBoolean(eedata.DIsVCP8);
                }

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an FT232H device.
        /// </summary>
        /// <param name="ee232h">An FT232H_EEPROM_STRUCTURE which contains only the relevant information for an FT232H device.</param>
        /// <returns>FT_STATUS value from FT_EE_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadFT232HEEPROM(FT232H_EEPROM_STRUCTURE ee232h)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_232H)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 5;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                status = FTDI_API.FT_EE_Read(handle, eedata);
                if (status == FT_STATUS.FT_OK)
                {
                    // Retrieve string values.
                    ee232h.Manufacturer = Marshal.PtrToStringAnsi(eedata.Manufacturer);
                    ee232h.ManufacturerID = Marshal.PtrToStringAnsi(eedata.ManufacturerID);
                    ee232h.Description = Marshal.PtrToStringAnsi(eedata.Description);
                    ee232h.SerialNumber = Marshal.PtrToStringAnsi(eedata.SerialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    ee232h.VendorID = eedata.VendorID;
                    ee232h.ProductID = eedata.ProductID;
                    ee232h.MaxPower = eedata.MaxPower;
                    ee232h.SelfPowered = Convert.ToBoolean(eedata.SelfPowered);
                    ee232h.RemoteWakeup = Convert.ToBoolean(eedata.RemoteWakeup);

                    // 232H specific fields.
                    ee232h.PullDownEnable = Convert.ToBoolean(eedata.PullDownEnableH);
                    ee232h.SerNumEnable = Convert.ToBoolean(eedata.SerNumEnableH);
                    ee232h.ACSlowSlew = Convert.ToBoolean(eedata.ACSlowSlewH);
                    ee232h.ACSchmittInput = Convert.ToBoolean(eedata.ACSchmittInputH);
                    ee232h.ACDriveCurrent = eedata.ACDriveCurrentH;
                    ee232h.ADSlowSlew = Convert.ToBoolean(eedata.ADSlowSlewH);
                    ee232h.ADSchmittInput = Convert.ToBoolean(eedata.ADSchmittInputH);
                    ee232h.ADDriveCurrent = eedata.ADDriveCurrentH;
                    ee232h.Cbus0 = eedata.Cbus0H;
                    ee232h.Cbus1 = eedata.Cbus1H;
                    ee232h.Cbus2 = eedata.Cbus2H;
                    ee232h.Cbus3 = eedata.Cbus3H;
                    ee232h.Cbus4 = eedata.Cbus4H;
                    ee232h.Cbus5 = eedata.Cbus5H;
                    ee232h.Cbus6 = eedata.Cbus6H;
                    ee232h.Cbus7 = eedata.Cbus7H;
                    ee232h.Cbus8 = eedata.Cbus8H;
                    ee232h.Cbus9 = eedata.Cbus9H;
                    ee232h.IsFifo = Convert.ToBoolean(eedata.IsFifoH);
                    ee232h.IsFifoTar = Convert.ToBoolean(eedata.IsFifoTarH);
                    ee232h.IsFastSer = Convert.ToBoolean(eedata.IsFastSerH);
                    ee232h.IsFT1248 = Convert.ToBoolean(eedata.IsFT1248H);
                    ee232h.FT1248Cpol = Convert.ToBoolean(eedata.FT1248CpolH);
                    ee232h.FT1248Lsb = Convert.ToBoolean(eedata.FT1248LsbH);
                    ee232h.FT1248FlowControl = Convert.ToBoolean(eedata.FT1248FlowControlH);
                    ee232h.IsVCP = Convert.ToBoolean(eedata.IsVCPH);
                    ee232h.PowerSaveEnable = Convert.ToBoolean(eedata.PowerSaveEnableH);
                }

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Reads the EEPROM contents of an X-Series device.
        /// </summary>
        /// <param name="eeXSeries">An FT_XSERIES_EEPROM_STRUCTURE which contains only the relevant information for an X-Series device.</param>
        /// <returns>FT_STATUS value from FT_EEPROM_Read in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS ReadXSeriesEEPROM(FT_XSERIES_EEPROM_STRUCTURE eeXSeries)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_X_SERIES)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                FT_XSERIES_DATA eeData = new FT_XSERIES_DATA();
                FT_EEPROM_HEADER eeHeader = new FT_EEPROM_HEADER();

                byte[] manufacturer = new byte[32];
                byte[] manufacturerID = new byte[16];
                byte[] description = new byte[64];
                byte[] serialNumber = new byte[16];

                eeHeader.deviceType = (uint)FT_DEVICE.FT_DEVICE_X_SERIES;
                eeData.common = eeHeader;

                // Calculate the size of our data structure.
                int size = Marshal.SizeOf(eeData);

                // Allocate space for our pointer.
                IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(eeData, eeDataMarshal, false);

                status = FTDI_API.FT_EEPROM_Read(handle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);
                if (status == FT_STATUS.FT_OK)
                {
                    // Get the data back from the pointer.
                    eeData = (FT_XSERIES_DATA)Marshal.PtrToStructure(eeDataMarshal, typeof(FT_XSERIES_DATA));

                    // Retrieve string values.
                    UTF8Encoding encoder = new UTF8Encoding();
                    eeXSeries.Manufacturer = encoder.GetString(manufacturer);
                    eeXSeries.ManufacturerID = encoder.GetString(manufacturerID);
                    eeXSeries.Description = encoder.GetString(description);
                    eeXSeries.SerialNumber = encoder.GetString(serialNumber);

                    // Map non-string elements to structure to be returned.
                    // Standard elements.
                    eeXSeries.VendorID = eeData.common.VendorId;
                    eeXSeries.ProductID = eeData.common.ProductId;
                    eeXSeries.MaxPower = eeData.common.MaxPower;
                    eeXSeries.SelfPowered = Convert.ToBoolean(eeData.common.SelfPowered);
                    eeXSeries.RemoteWakeup = Convert.ToBoolean(eeData.common.RemoteWakeup);
                    eeXSeries.SerNumEnable = Convert.ToBoolean(eeData.common.SerNumEnable);
                    eeXSeries.PullDownEnable = Convert.ToBoolean(eeData.common.PullDownEnable);

                    // X-Series specific fields.
                    // CBUS.
                    eeXSeries.Cbus0 = eeData.Cbus0;
                    eeXSeries.Cbus1 = eeData.Cbus1;
                    eeXSeries.Cbus2 = eeData.Cbus2;
                    eeXSeries.Cbus3 = eeData.Cbus3;
                    eeXSeries.Cbus4 = eeData.Cbus4;
                    eeXSeries.Cbus5 = eeData.Cbus5;
                    eeXSeries.Cbus6 = eeData.Cbus6;

                    // Drive options.
                    eeXSeries.ACDriveCurrent = eeData.ACDriveCurrent;
                    eeXSeries.ACSchmittInput = eeData.ACSchmittInput;
                    eeXSeries.ACSlowSlew = eeData.ACSlowSlew;
                    eeXSeries.ADDriveCurrent = eeData.ADDriveCurrent;
                    eeXSeries.ADSchmittInput = eeData.ADSchmittInput;
                    eeXSeries.ADSlowSlew = eeData.ADSlowSlew;

                    // BCD.
                    eeXSeries.BCDDisableSleep = eeData.BCDDisableSleep;
                    eeXSeries.BCDEnable = eeData.BCDEnable;
                    eeXSeries.BCDForceCbusPWREN = eeData.BCDForceCbusPWREN;

                    // FT1248.
                    eeXSeries.FT1248Cpol = eeData.FT1248Cpol;
                    eeXSeries.FT1248FlowControl = eeData.FT1248FlowControl;
                    eeXSeries.FT1248Lsb = eeData.FT1248Lsb;

                    // I2C.
                    eeXSeries.I2CDeviceId = eeData.I2CDeviceId;
                    eeXSeries.I2CDisableSchmitt = eeData.I2CDisableSchmitt;
                    eeXSeries.I2CSlaveAddress = eeData.I2CSlaveAddress;

                    // RS232 signals.
                    eeXSeries.InvertCTS = eeData.InvertCTS;
                    eeXSeries.InvertDCD = eeData.InvertDCD;
                    eeXSeries.InvertDSR = eeData.InvertDSR;
                    eeXSeries.InvertDTR = eeData.InvertDTR;
                    eeXSeries.InvertRI = eeData.InvertRI;
                    eeXSeries.InvertRTS = eeData.InvertRTS;
                    eeXSeries.InvertRXD = eeData.InvertRXD;
                    eeXSeries.InvertTXD = eeData.InvertTXD;

                    // Hardware options.
                    eeXSeries.PowerSaveEnable = eeData.PowerSaveEnable;
                    eeXSeries.RS485EchoSuppress = eeData.RS485EchoSuppress;

                    // Driver option.
                    eeXSeries.IsVCP = eeData.DriverType;
                }

                // Free unmanaged buffer. 
                Marshal.FreeHGlobal(eeDataMarshal);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an FT232B or FT245B device.
        /// </summary>
        /// <param name="ee232b">The EEPROM settings to be written to the device.</param>
        /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS WriteFT232BEEPROM(FT232B_EEPROM_STRUCTURE ee232b)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232B or FT245B that we are trying to write.
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_BM)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee232b.VendorID == 0x0000) | (ee232b.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000.
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee232b.Manufacturer.Length > 32)
                {
                    ee232b.Manufacturer = ee232b.Manufacturer.Substring(0, 32);
                }

                if (ee232b.ManufacturerID.Length > 16)
                {
                    ee232b.ManufacturerID = ee232b.ManufacturerID.Substring(0, 16);
                }

                if (ee232b.Description.Length > 64)
                {
                    ee232b.Description = ee232b.Description.Substring(0, 64);
                }

                if (ee232b.SerialNumber.Length > 16)
                {
                    ee232b.SerialNumber = ee232b.SerialNumber.Substring(0, 16);
                }

                // Set string values.
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232b.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232b.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee232b.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232b.SerialNumber);

                // Map non-string elements to structure.
                // Standard elements.
                eedata.VendorID = ee232b.VendorID;
                eedata.ProductID = ee232b.ProductID;
                eedata.MaxPower = ee232b.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee232b.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee232b.RemoteWakeup);

                // B specific fields.
                eedata.Rev4 = Convert.ToByte(true);
                eedata.PullDownEnable = Convert.ToByte(ee232b.PullDownEnable);
                eedata.SerNumEnable = Convert.ToByte(ee232b.SerNumEnable);
                eedata.USBVersionEnable = Convert.ToByte(ee232b.USBVersionEnable);
                eedata.USBVersion = ee232b.USBVersion;

                status = FTDI_API.FT_EE_Program(handle, eedata);

                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an FT2232 device.
        /// </summary>
        /// <param name="ee2232">The EEPROM settings to be written to the device.</param>
        /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS WriteFT2232EEPROM(FT2232_EEPROM_STRUCTURE ee2232)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT2232 that we are trying to write.
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_BM)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee2232.VendorID == 0x0000) | (ee2232.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000.
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 2;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits
                // If not, trim them to make them our maximum length
                if (ee2232.Manufacturer.Length > 32)
                {
                    ee2232.Manufacturer = ee2232.Manufacturer.Substring(0, 32);
                }

                if (ee2232.ManufacturerID.Length > 16)
                {
                    ee2232.ManufacturerID = ee2232.ManufacturerID.Substring(0, 16);
                }

                if (ee2232.Description.Length > 64)
                {
                    ee2232.Description = ee2232.Description.Substring(0, 64);
                }

                if (ee2232.SerialNumber.Length > 16)
                {
                    ee2232.SerialNumber = ee2232.SerialNumber.Substring(0, 16);
                }

                // Set string values.
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee2232.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232.SerialNumber);

                // Map non-string elements to structure.
                // Standard elements.
                eedata.VendorID = ee2232.VendorID;
                eedata.ProductID = ee2232.ProductID;
                eedata.MaxPower = ee2232.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee2232.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee2232.RemoteWakeup);

                // 2232 specific fields.
                eedata.Rev5 = Convert.ToByte(true);
                eedata.PullDownEnable5 = Convert.ToByte(ee2232.PullDownEnable);
                eedata.SerNumEnable5 = Convert.ToByte(ee2232.SerNumEnable);
                eedata.USBVersionEnable5 = Convert.ToByte(ee2232.USBVersionEnable);
                eedata.USBVersion5 = ee2232.USBVersion;
                eedata.AIsHighCurrent = Convert.ToByte(ee2232.AIsHighCurrent);
                eedata.BIsHighCurrent = Convert.ToByte(ee2232.BIsHighCurrent);
                eedata.IFAIsFifo = Convert.ToByte(ee2232.IFAIsFifo);
                eedata.IFAIsFifoTar = Convert.ToByte(ee2232.IFAIsFifoTar);
                eedata.IFAIsFastSer = Convert.ToByte(ee2232.IFAIsFastSer);
                eedata.AIsVCP = Convert.ToByte(ee2232.AIsVCP);
                eedata.IFBIsFifo = Convert.ToByte(ee2232.IFBIsFifo);
                eedata.IFBIsFifoTar = Convert.ToByte(ee2232.IFBIsFifoTar);
                eedata.IFBIsFastSer = Convert.ToByte(ee2232.IFBIsFastSer);
                eedata.BIsVCP = Convert.ToByte(ee2232.BIsVCP);

                status = FTDI_API.FT_EE_Program(handle, eedata);

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an FT232R or FT245R device.
        /// </summary>
        /// <param name="ee232r">The EEPROM settings to be written to the device.</param>
        /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        public FT_STATUS WriteFT232REEPROM(FT232R_EEPROM_STRUCTURE ee232r)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232R or FT245R that we are trying to write
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_232R)
                {
                    // If it is not, throw an exception
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee232r.VendorID == 0x0000) | (ee232r.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 2;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits.
                // If not, trim them to make them our maximum length.
                if (ee232r.Manufacturer.Length > 32)
                {
                    ee232r.Manufacturer = ee232r.Manufacturer.Substring(0, 32);
                }

                if (ee232r.ManufacturerID.Length > 16)
                {
                    ee232r.ManufacturerID = ee232r.ManufacturerID.Substring(0, 16);
                }

                if (ee232r.Description.Length > 64)
                {
                    ee232r.Description = ee232r.Description.Substring(0, 64);
                }

                if (ee232r.SerialNumber.Length > 16)
                {
                    ee232r.SerialNumber = ee232r.SerialNumber.Substring(0, 16);
                }

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232r.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232r.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee232r.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232r.SerialNumber);

                // Map non-string elements to structure.
                // Standard elements.
                eedata.VendorID = ee232r.VendorID;
                eedata.ProductID = ee232r.ProductID;
                eedata.MaxPower = ee232r.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee232r.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee232r.RemoteWakeup);

                // 232R specific fields.
                eedata.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
                eedata.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
                eedata.UseExtOsc = Convert.ToByte(ee232r.UseExtOsc);
                eedata.HighDriveIOs = Convert.ToByte(ee232r.HighDriveIOs);

                // Override any endpoint size the user has selected and force 64 bytes.
                // Some users have been known to wreck devices by setting 0 here.
                eedata.EndpointSize = 64;
                eedata.PullDownEnableR = Convert.ToByte(ee232r.PullDownEnable);
                eedata.SerNumEnableR = Convert.ToByte(ee232r.SerNumEnable);
                eedata.InvertTXD = Convert.ToByte(ee232r.InvertTXD);
                eedata.InvertRXD = Convert.ToByte(ee232r.InvertRXD);
                eedata.InvertRTS = Convert.ToByte(ee232r.InvertRTS);
                eedata.InvertCTS = Convert.ToByte(ee232r.InvertCTS);
                eedata.InvertDTR = Convert.ToByte(ee232r.InvertDTR);
                eedata.InvertDSR = Convert.ToByte(ee232r.InvertDSR);
                eedata.InvertDCD = Convert.ToByte(ee232r.InvertDCD);
                eedata.InvertRI = Convert.ToByte(ee232r.InvertRI);
                eedata.Cbus0 = ee232r.Cbus0;
                eedata.Cbus1 = ee232r.Cbus1;
                eedata.Cbus2 = ee232r.Cbus2;
                eedata.Cbus3 = ee232r.Cbus3;
                eedata.Cbus4 = ee232r.Cbus4;
                eedata.RIsD2XX = Convert.ToByte(ee232r.RIsD2XX);

                status = FTDI_API.FT_EE_Program(handle, eedata);

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an FT2232H device.
        /// </summary>
        /// <param name="ee2232h">The EEPROM settings to be written to the device.</param>
        /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        /// <remarks>Calls FT_EE_Program in FTD2XX DLL</remarks>
        public FT_STATUS WriteFT2232HEEPROM(FT2232H_EEPROM_STRUCTURE ee2232h)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT2232H that we are trying to write.
                GetDeviceType(ref deviceType);
                if (deviceType != FT_DEVICE.FT_DEVICE_2232H)
                {
                    // If it is not, throw an exception
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((ee2232h.VendorID == 0x0000) | (ee2232h.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000.
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 3;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits.
                // If not, trim them to make them our maximum length.
                if (ee2232h.Manufacturer.Length > 32)
                {
                    ee2232h.Manufacturer = ee2232h.Manufacturer.Substring(0, 32);
                }

                if (ee2232h.ManufacturerID.Length > 16)
                {
                    ee2232h.ManufacturerID = ee2232h.ManufacturerID.Substring(0, 16);
                }

                if (ee2232h.Description.Length > 64)
                {
                    ee2232h.Description = ee2232h.Description.Substring(0, 64);
                }

                if (ee2232h.SerialNumber.Length > 16)
                {
                    ee2232h.SerialNumber = ee2232h.SerialNumber.Substring(0, 16);
                }

                // Set string values.
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee2232h.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee2232h.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee2232h.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee2232h.SerialNumber);

                // Map non-string elements to structure.
                // Standard elements.
                eedata.VendorID = ee2232h.VendorID;
                eedata.ProductID = ee2232h.ProductID;
                eedata.MaxPower = ee2232h.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee2232h.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee2232h.RemoteWakeup);

                // 2232H specific fields.
                eedata.PullDownEnable7 = Convert.ToByte(ee2232h.PullDownEnable);
                eedata.SerNumEnable7 = Convert.ToByte(ee2232h.SerNumEnable);
                eedata.ALSlowSlew = Convert.ToByte(ee2232h.ALSlowSlew);
                eedata.ALSchmittInput = Convert.ToByte(ee2232h.ALSchmittInput);
                eedata.ALDriveCurrent = ee2232h.ALDriveCurrent;
                eedata.AHSlowSlew = Convert.ToByte(ee2232h.AHSlowSlew);
                eedata.AHSchmittInput = Convert.ToByte(ee2232h.AHSchmittInput);
                eedata.AHDriveCurrent = ee2232h.AHDriveCurrent;
                eedata.BLSlowSlew = Convert.ToByte(ee2232h.BLSlowSlew);
                eedata.BLSchmittInput = Convert.ToByte(ee2232h.BLSchmittInput);
                eedata.BLDriveCurrent = ee2232h.BLDriveCurrent;
                eedata.BHSlowSlew = Convert.ToByte(ee2232h.BHSlowSlew);
                eedata.BHSchmittInput = Convert.ToByte(ee2232h.BHSchmittInput);
                eedata.BHDriveCurrent = ee2232h.BHDriveCurrent;
                eedata.IFAIsFifo7 = Convert.ToByte(ee2232h.IFAIsFifo);
                eedata.IFAIsFifoTar7 = Convert.ToByte(ee2232h.IFAIsFifoTar);
                eedata.IFAIsFastSer7 = Convert.ToByte(ee2232h.IFAIsFastSer);
                eedata.AIsVCP7 = Convert.ToByte(ee2232h.AIsVCP);
                eedata.IFBIsFifo7 = Convert.ToByte(ee2232h.IFBIsFifo);
                eedata.IFBIsFifoTar7 = Convert.ToByte(ee2232h.IFBIsFifoTar);
                eedata.IFBIsFastSer7 = Convert.ToByte(ee2232h.IFBIsFastSer);
                eedata.BIsVCP7 = Convert.ToByte(ee2232h.BIsVCP);
                eedata.PowerSaveEnable = Convert.ToByte(ee2232h.PowerSaveEnable);

                status = FTDI_API.FT_EE_Program(handle, eedata);

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an FT4232H device.
        /// </summary>
        /// <param name="ee4232h">Writes the specified values to the EEPROM of an FT4232H device..</param>
        /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        /// <remarks>Calls FT_EE_Program in FTD2XX DLL</remarks>
        public FT_STATUS WriteFT4232HEEPROM(FT4232H_EEPROM_STRUCTURE ee4232h)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT4232H that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_4232H)
                {
                    // If it is not, throw an exception
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000.
                if ((ee4232h.VendorID == 0x0000) | (ee4232h.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000.
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers.
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xffffffff;
                eedata.Version = 4;

                // Allocate space from unmanaged heap.
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits.
                // If not, trim them to make them our maximum length.
                if (ee4232h.Manufacturer.Length > 32)
                {
                    ee4232h.Manufacturer = ee4232h.Manufacturer.Substring(0, 32);
                }

                if (ee4232h.ManufacturerID.Length > 16)
                {
                    ee4232h.ManufacturerID = ee4232h.ManufacturerID.Substring(0, 16);
                }

                if (ee4232h.Description.Length > 64)
                {
                    ee4232h.Description = ee4232h.Description.Substring(0, 64);
                }

                if (ee4232h.SerialNumber.Length > 16)
                {
                    ee4232h.SerialNumber = ee4232h.SerialNumber.Substring(0, 16);
                }

                // Set string values
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee4232h.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee4232h.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee4232h.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee4232h.SerialNumber);

                // Map non-string elements to structure.
                // Standard elements.
                eedata.VendorID = ee4232h.VendorID;
                eedata.ProductID = ee4232h.ProductID;
                eedata.MaxPower = ee4232h.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee4232h.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee4232h.RemoteWakeup);

                // 4232H specific fields.
                eedata.PullDownEnable8 = Convert.ToByte(ee4232h.PullDownEnable);
                eedata.SerNumEnable8 = Convert.ToByte(ee4232h.SerNumEnable);
                eedata.ASlowSlew = Convert.ToByte(ee4232h.ASlowSlew);
                eedata.ASchmittInput = Convert.ToByte(ee4232h.ASchmittInput);
                eedata.ADriveCurrent = ee4232h.ADriveCurrent;
                eedata.BSlowSlew = Convert.ToByte(ee4232h.BSlowSlew);
                eedata.BSchmittInput = Convert.ToByte(ee4232h.BSchmittInput);
                eedata.BDriveCurrent = ee4232h.BDriveCurrent;
                eedata.CSlowSlew = Convert.ToByte(ee4232h.CSlowSlew);
                eedata.CSchmittInput = Convert.ToByte(ee4232h.CSchmittInput);
                eedata.CDriveCurrent = ee4232h.CDriveCurrent;
                eedata.DSlowSlew = Convert.ToByte(ee4232h.DSlowSlew);
                eedata.DSchmittInput = Convert.ToByte(ee4232h.DSchmittInput);
                eedata.DDriveCurrent = ee4232h.DDriveCurrent;
                eedata.ARIIsTXDEN = Convert.ToByte(ee4232h.ARIIsTXDEN);
                eedata.BRIIsTXDEN = Convert.ToByte(ee4232h.BRIIsTXDEN);
                eedata.CRIIsTXDEN = Convert.ToByte(ee4232h.CRIIsTXDEN);
                eedata.DRIIsTXDEN = Convert.ToByte(ee4232h.DRIIsTXDEN);
                eedata.AIsVCP8 = Convert.ToByte(ee4232h.AIsVCP);
                eedata.BIsVCP8 = Convert.ToByte(ee4232h.BIsVCP);
                eedata.CIsVCP8 = Convert.ToByte(ee4232h.CIsVCP);
                eedata.DIsVCP8 = Convert.ToByte(ee4232h.DIsVCP);

                status = FTDI_API.FT_EE_Program(handle, eedata);

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an FT232H device.
        /// </summary>
        /// <param name="ee232h">The EEPROM settings to be written to the device.</param>
        /// <returns>FT_STATUS value from FT_EE_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        /// <remarks>Calls FT_EE_Program in FTD2XX DLL</remarks>
        public FT_STATUS WriteFT232HEEPROM(FT232H_EEPROM_STRUCTURE ee232h)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232H that we are trying to write.
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_232H)
                {
                    // If it is not, throw an exception.
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000.
                if ((ee232h.VendorID == 0x0000) | (ee232h.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000.
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_PROGRAM_DATA eedata = new FT_PROGRAM_DATA();

                // Set up structure headers
                eedata.Signature1 = 0x00000000;
                eedata.Signature2 = 0xFFFFFFFF;
                eedata.Version = 5;

                // Allocate space from unmanaged heap
                eedata.Manufacturer = Marshal.AllocHGlobal(32);
                eedata.ManufacturerID = Marshal.AllocHGlobal(16);
                eedata.Description = Marshal.AllocHGlobal(64);
                eedata.SerialNumber = Marshal.AllocHGlobal(16);

                // Check lengths of strings to make sure that they are within our limits.
                // If not, trim them to make them our maximum length.
                if (ee232h.Manufacturer.Length > 32)
                    ee232h.Manufacturer = ee232h.Manufacturer.Substring(0, 32);
                if (ee232h.ManufacturerID.Length > 16)
                    ee232h.ManufacturerID = ee232h.ManufacturerID.Substring(0, 16);
                if (ee232h.Description.Length > 64)
                    ee232h.Description = ee232h.Description.Substring(0, 64);
                if (ee232h.SerialNumber.Length > 16)
                    ee232h.SerialNumber = ee232h.SerialNumber.Substring(0, 16);

                // Set string values.
                eedata.Manufacturer = Marshal.StringToHGlobalAnsi(ee232h.Manufacturer);
                eedata.ManufacturerID = Marshal.StringToHGlobalAnsi(ee232h.ManufacturerID);
                eedata.Description = Marshal.StringToHGlobalAnsi(ee232h.Description);
                eedata.SerialNumber = Marshal.StringToHGlobalAnsi(ee232h.SerialNumber);

                // Map non-string elements to structure.
                // Standard elements.
                eedata.VendorID = ee232h.VendorID;
                eedata.ProductID = ee232h.ProductID;
                eedata.MaxPower = ee232h.MaxPower;
                eedata.SelfPowered = Convert.ToUInt16(ee232h.SelfPowered);
                eedata.RemoteWakeup = Convert.ToUInt16(ee232h.RemoteWakeup);

                // 232H specific fields.
                eedata.PullDownEnableH = Convert.ToByte(ee232h.PullDownEnable);
                eedata.SerNumEnableH = Convert.ToByte(ee232h.SerNumEnable);
                eedata.ACSlowSlewH = Convert.ToByte(ee232h.ACSlowSlew);
                eedata.ACSchmittInputH = Convert.ToByte(ee232h.ACSchmittInput);
                eedata.ACDriveCurrentH = Convert.ToByte(ee232h.ACDriveCurrent);
                eedata.ADSlowSlewH = Convert.ToByte(ee232h.ADSlowSlew);
                eedata.ADSchmittInputH = Convert.ToByte(ee232h.ADSchmittInput);
                eedata.ADDriveCurrentH = Convert.ToByte(ee232h.ADDriveCurrent);
                eedata.Cbus0H = Convert.ToByte(ee232h.Cbus0);
                eedata.Cbus1H = Convert.ToByte(ee232h.Cbus1);
                eedata.Cbus2H = Convert.ToByte(ee232h.Cbus2);
                eedata.Cbus3H = Convert.ToByte(ee232h.Cbus3);
                eedata.Cbus4H = Convert.ToByte(ee232h.Cbus4);
                eedata.Cbus5H = Convert.ToByte(ee232h.Cbus5);
                eedata.Cbus6H = Convert.ToByte(ee232h.Cbus6);
                eedata.Cbus7H = Convert.ToByte(ee232h.Cbus7);
                eedata.Cbus8H = Convert.ToByte(ee232h.Cbus8);
                eedata.Cbus9H = Convert.ToByte(ee232h.Cbus9);
                eedata.IsFifoH = Convert.ToByte(ee232h.IsFifo);
                eedata.IsFifoTarH = Convert.ToByte(ee232h.IsFifoTar);
                eedata.IsFastSerH = Convert.ToByte(ee232h.IsFastSer);
                eedata.IsFT1248H = Convert.ToByte(ee232h.IsFT1248);
                eedata.FT1248CpolH = Convert.ToByte(ee232h.FT1248Cpol);
                eedata.FT1248LsbH = Convert.ToByte(ee232h.FT1248Lsb);
                eedata.FT1248FlowControlH = Convert.ToByte(ee232h.FT1248FlowControl);
                eedata.IsVCPH = Convert.ToByte(ee232h.IsVCP);
                eedata.PowerSaveEnableH = Convert.ToByte(ee232h.PowerSaveEnable);

                status = FTDI_API.FT_EE_Program(handle, eedata);

                // Free unmanaged buffers.
                Marshal.FreeHGlobal(eedata.Manufacturer);
                Marshal.FreeHGlobal(eedata.ManufacturerID);
                Marshal.FreeHGlobal(eedata.Description);
                Marshal.FreeHGlobal(eedata.SerialNumber);
            }

            return status;
        }

        /// <summary>
        /// Writes the specified values to the EEPROM of an X-Series device.
        /// </summary>
        /// <param name="eeXSeries">The EEPROM settings to be written to the device.</param>
        /// <returns>FT_STATUS value from FT_EEPROM_Program in FTD2XX DLL</returns>
        /// <exception cref="FT_Exception">Thrown when the current device does not match the type required by this method.</exception>
        /// <remarks>Calls FT_EEPROM_Program in FTD2XX DLL</remarks>
        public FT_STATUS WriteXSeriesEEPROM(FT_XSERIES_EEPROM_STRUCTURE eeXSeries)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;
            FT_ERROR errorCondition = FT_ERROR.FT_NO_ERROR;
            byte[] manufacturer, manufacturerID, description, serialNumber;

            if (handle != IntPtr.Zero)
            {
                FT_DEVICE DeviceType = FT_DEVICE.FT_DEVICE_UNKNOWN;
                // Check that it is an FT232H that we are trying to write
                GetDeviceType(ref DeviceType);
                if (DeviceType != FT_DEVICE.FT_DEVICE_X_SERIES)
                {
                    // If it is not, throw an exception
                    errorCondition = FT_ERROR.FT_INCORRECT_DEVICE;
                    ErrorHandler(status, errorCondition);
                }

                // Check for VID and PID of 0x0000
                if ((eeXSeries.VendorID == 0x0000) | (eeXSeries.ProductID == 0x0000))
                {
                    // Do not allow users to program the device with VID or PID of 0x0000
                    return FT_STATUS.FT_INVALID_PARAMETER;
                }

                FT_XSERIES_DATA eeData = new FT_XSERIES_DATA();

                // String manipulation.
                // Allocate space from unmanaged heap
                manufacturer = new byte[32];
                manufacturerID = new byte[16];
                description = new byte[64];
                serialNumber = new byte[16];

                // Check lengths of strings to make sure that they are within our limits.
                // If not, trim them to make them our maximum length.
                if (eeXSeries.Manufacturer.Length > 32)
                {
                    eeXSeries.Manufacturer = eeXSeries.Manufacturer.Substring(0, 32);
                }

                if (eeXSeries.ManufacturerID.Length > 16)
                {
                    eeXSeries.ManufacturerID = eeXSeries.ManufacturerID.Substring(0, 16);
                }

                if (eeXSeries.Description.Length > 64)
                {
                    eeXSeries.Description = eeXSeries.Description.Substring(0, 64);
                }

                if (eeXSeries.SerialNumber.Length > 16)
                {
                    eeXSeries.SerialNumber = eeXSeries.SerialNumber.Substring(0, 16);
                }

                // Set string values
                UTF8Encoding encoding = new UTF8Encoding();
                manufacturer = encoding.GetBytes(eeXSeries.Manufacturer);
                manufacturerID = encoding.GetBytes(eeXSeries.ManufacturerID);
                description = encoding.GetBytes(eeXSeries.Description);
                serialNumber = encoding.GetBytes(eeXSeries.SerialNumber);

                // Map non-string elements to structure to be returned.
                // Standard elements.
                eeData.common.deviceType = (uint)FT_DEVICE.FT_DEVICE_X_SERIES;
                eeData.common.VendorId = eeXSeries.VendorID;
                eeData.common.ProductId = eeXSeries.ProductID;
                eeData.common.MaxPower = eeXSeries.MaxPower;
                eeData.common.SelfPowered = Convert.ToByte(eeXSeries.SelfPowered);
                eeData.common.RemoteWakeup = Convert.ToByte(eeXSeries.RemoteWakeup);
                eeData.common.SerNumEnable = Convert.ToByte(eeXSeries.SerNumEnable);
                eeData.common.PullDownEnable = Convert.ToByte(eeXSeries.PullDownEnable);

                // X-Series specific fields.
                // CBUS.
                eeData.Cbus0 = eeXSeries.Cbus0;
                eeData.Cbus1 = eeXSeries.Cbus1;
                eeData.Cbus2 = eeXSeries.Cbus2;
                eeData.Cbus3 = eeXSeries.Cbus3;
                eeData.Cbus4 = eeXSeries.Cbus4;
                eeData.Cbus5 = eeXSeries.Cbus5;
                eeData.Cbus6 = eeXSeries.Cbus6;

                // Drive options.
                eeData.ACDriveCurrent = eeXSeries.ACDriveCurrent;
                eeData.ACSchmittInput = eeXSeries.ACSchmittInput;
                eeData.ACSlowSlew = eeXSeries.ACSlowSlew;
                eeData.ADDriveCurrent = eeXSeries.ADDriveCurrent;
                eeData.ADSchmittInput = eeXSeries.ADSchmittInput;
                eeData.ADSlowSlew = eeXSeries.ADSlowSlew;

                // BCD.
                eeData.BCDDisableSleep = eeXSeries.BCDDisableSleep;
                eeData.BCDEnable = eeXSeries.BCDEnable;
                eeData.BCDForceCbusPWREN = eeXSeries.BCDForceCbusPWREN;

                // FT1248.
                eeData.FT1248Cpol = eeXSeries.FT1248Cpol;
                eeData.FT1248FlowControl = eeXSeries.FT1248FlowControl;
                eeData.FT1248Lsb = eeXSeries.FT1248Lsb;

                // I2C.
                eeData.I2CDeviceId = eeXSeries.I2CDeviceId;
                eeData.I2CDisableSchmitt = eeXSeries.I2CDisableSchmitt;
                eeData.I2CSlaveAddress = eeXSeries.I2CSlaveAddress;

                // RS232 signals.
                eeData.InvertCTS = eeXSeries.InvertCTS;
                eeData.InvertDCD = eeXSeries.InvertDCD;
                eeData.InvertDSR = eeXSeries.InvertDSR;
                eeData.InvertDTR = eeXSeries.InvertDTR;
                eeData.InvertRI = eeXSeries.InvertRI;
                eeData.InvertRTS = eeXSeries.InvertRTS;
                eeData.InvertRXD = eeXSeries.InvertRXD;
                eeData.InvertTXD = eeXSeries.InvertTXD;

                // Hardware options.
                eeData.PowerSaveEnable = eeXSeries.PowerSaveEnable;
                eeData.RS485EchoSuppress = eeXSeries.RS485EchoSuppress;

                // Driver option.
                eeData.DriverType = eeXSeries.IsVCP;

                // Check the size of the structure.
                int size = Marshal.SizeOf(eeData);
                // Allocate space for our pointer.
                IntPtr eeDataMarshal = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(eeData, eeDataMarshal, false);

                status = FTDI_API.FT_EEPROM_Program(handle, eeDataMarshal, (uint)size, manufacturer, manufacturerID, description, serialNumber);

                // Free unmanaged buffer.
                Marshal.FreeHGlobal(eeDataMarshal);
            }

            return status;
        }

        /// <summary>
        /// Gets the size of the EEPROM user area.
        /// </summary>
        /// <param name="userAreaSize">The EEPROM user area size in bytes.</param>
        /// <returns>FT_STATUS value from FT_EE_UASize in FTD2XX.DLL</returns>
        public FT_STATUS EEUserAreaSize(ref uint userAreaSize)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                status = FTDI_API.FT_EE_UASize(handle, ref userAreaSize);
            }

            return status;
        }

        /// <summary>
        /// Reads data from the user area of the device EEPROM.
        /// </summary>
        /// <param name="dataBuffer">An array of bytes which will be populated with the data read from the device EEPROM user area.</param>
        /// <param name="bytesRead">The number of bytes actually read from the EEPROM user area.</param>
        /// <returns>FT_STATUS from FT_UARead in FTD2XX.DLL</returns>
        public FT_STATUS EEReadUserArea(byte[] dataBuffer, ref uint bytesRead)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                uint userAreaSize = 0;
                status = FTDI_API.FT_EE_UASize(handle, ref userAreaSize);
                if (dataBuffer.Length >= userAreaSize)
                {
                    status = FTDI_API.FT_EE_UARead(handle, dataBuffer, (uint)dataBuffer.Length, ref bytesRead);
                }
            }

            return status;
        }

        /// <summary>
        /// Writes data to the user area of the device EEPROM.
        /// </summary>
        /// <param name="dataBuffer">An array of bytes which will be written to the device EEPROM user area.</param>
        /// <returns>FT_STATUS value from FT_UAWrite in FTD2XX.DLL</returns>
        public FT_STATUS EEWriteUserArea(byte[] dataBuffer)
        {
            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            if (handle != IntPtr.Zero)
            {
                uint userAreaSize = 0;
                status = FTDI_API.FT_EE_UASize(handle, ref userAreaSize);
                if (dataBuffer.Length >= userAreaSize)
                {
                    status = FTDI_API.FT_EE_UAWrite(handle, dataBuffer, (uint)dataBuffer.Length);
                }
            }

            return status;
        }

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets the open status of the device.
        /// </summary>
        public bool IsOpen
        {
            get { return (handle != IntPtr.Zero); }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Initialise the device's UART to some default values.
        /// </summary>
        /// <returns>FT_STATUS value from FT_SetDataCharacteristics in FTD2XX.DLL</returns>
        private FT_STATUS InitialiseDevice()
        {
            // Initialise port data characteristics.
            byte wordLength = FT_DATA_BITS.FT_BITS_8;
            byte stopBits = FT_STOP_BITS.FT_STOP_BITS_1;
            byte parity = FT_PARITY.FT_PARITY_NONE;

            FT_STATUS status = FT_STATUS.FT_OTHER_ERROR;

            // Initialise port data characteristics.
            status = FTDI_API.FT_SetDataCharacteristics(handle, wordLength, stopBits, parity);
            if (status != FT_STATUS.FT_OK)
            {
                return status;
            }

            // Initialise to no flow control.
            ushort flowControl = FT_FLOW_CONTROL.FT_FLOW_NONE;
            byte xOn = 0x11;
            byte xOff = 0x13;
            status = FTDI_API.FT_SetFlowControl(handle, flowControl, xOn, xOff);
            if (status != FT_STATUS.FT_OK)
            {
                return status;
            }

            // Initialise Baud rate.
            uint baudRate = FT_DEFAULT_BAUD_RATE;
            status = FTDI_API.FT_SetBaudRate(handle, baudRate);

            return status;
        }

        /// <summary>
        /// Decodes all the byte in the specified array to null terminate ASCII string.
        /// </summary>
        /// <param name="byteArray">byte array to decoded.</param>
        /// <returns>An ASCII encoded string.</returns>
        private string GetString(byte[] byteArray)
        {
            string encodedString = Encoding.ASCII.GetString(byteArray);
            // Trim strings to first occurrence of a null terminator character.
            int nullIndex = encodedString.IndexOf("\0");
            if (nullIndex != -1)
            {
                encodedString = encodedString.Substring(0, nullIndex);
            }

            return encodedString;
        }

        /// <summary>
        /// Method to check status and errorCondition values for error conditions and throw exceptions accordingly.
        /// </summary>
        private void ErrorHandler(FT_STATUS status, FT_ERROR errorCondition)
        {
            if (status != FT_STATUS.FT_OK)
            {
                // Check FT_STATUS values returned from FTD2XX DLL calls
                switch (status)
                {
                    case FT_STATUS.FT_DEVICE_NOT_FOUND:
                        {
                            throw new FT_Exception("FTDI device not found.");
                        }

                    case FT_STATUS.FT_DEVICE_NOT_OPENED:
                        {
                            throw new FT_Exception("FTDI device not opened.");
                        }

                    case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_ERASE:
                        {
                            throw new FT_Exception("FTDI device not opened for erase.");
                        }

                    case FT_STATUS.FT_DEVICE_NOT_OPENED_FOR_WRITE:
                        {
                            throw new FT_Exception("FTDI device not opened for write.");
                        }

                    case FT_STATUS.FT_EEPROM_ERASE_FAILED:
                        {
                            throw new FT_Exception("Failed to erase FTDI device EEPROM.");
                        }

                    case FT_STATUS.FT_EEPROM_NOT_PRESENT:
                        {
                            throw new FT_Exception("No EEPROM fitted to FTDI device.");
                        }

                    case FT_STATUS.FT_EEPROM_NOT_PROGRAMMED:
                        {
                            throw new FT_Exception("FTDI device EEPROM not programmed.");
                        }

                    case FT_STATUS.FT_EEPROM_READ_FAILED:
                        {
                            throw new FT_Exception("Failed to read FTDI device EEPROM.");
                        }

                    case FT_STATUS.FT_EEPROM_WRITE_FAILED:
                        {
                            throw new FT_Exception("Failed to write FTDI device EEPROM.");
                        }

                    case FT_STATUS.FT_FAILED_TO_WRITE_DEVICE:
                        {
                            throw new FT_Exception("Failed to write to FTDI device.");
                        }

                    case FT_STATUS.FT_INSUFFICIENT_RESOURCES:
                        {
                            throw new FT_Exception("Insufficient resources.");
                        }

                    case FT_STATUS.FT_INVALID_ARGS:
                        {
                            throw new FT_Exception("Invalid arguments for FTD2XX function call.");
                        }

                    case FT_STATUS.FT_INVALID_BAUD_RATE:
                        {
                            throw new FT_Exception("Invalid Baud rate for FTDI device.");
                        }

                    case FT_STATUS.FT_INVALID_HANDLE:
                        {
                            throw new FT_Exception("Invalid handle for FTDI device.");
                        }

                    case FT_STATUS.FT_INVALID_PARAMETER:
                        {
                            throw new FT_Exception("Invalid parameter for FTD2XX function call.");
                        }

                    case FT_STATUS.FT_IO_ERROR:
                        {
                            throw new FT_Exception("FTDI device IO error.");
                        }

                    case FT_STATUS.FT_OTHER_ERROR:
                        {
                            throw new FT_Exception("An unexpected error has occurred when trying to communicate with the FTDI device.");
                        }

                    default:
                        break;
                }
            }

            if (errorCondition != FT_ERROR.FT_NO_ERROR)
            {
                // Check for other error conditions not handled by FTD2XX DLL
                switch (errorCondition)
                {
                    case FT_ERROR.FT_INCORRECT_DEVICE:
                        {
                            throw new FT_Exception("The current device type does not match the EEPROM structure.");
                        }

                    case FT_ERROR.FT_INVALID_BITMODE:
                        {
                            throw new FT_Exception("The requested bit mode is not valid for the current device.");
                        }

                    case FT_ERROR.FT_BUFFER_SIZE:
                        {
                            throw new FT_Exception("The supplied buffer is not big enough.");
                        }

                    default:
                        break;
                }
            }

            return;
        }

        #endregion

        #region Private Properties

        /// <summary>
        /// Gets the interface identifier.
        /// </summary>
        private string InterfaceIdentifier
        {
            get
            {
                string identifier = string.Empty;
                if (IsOpen)
                {
                    FT_DEVICE deviceType = FT_DEVICE.FT_DEVICE_BM;
                    GetDeviceType(ref deviceType);
                    if ((deviceType == FT_DEVICE.FT_DEVICE_2232) | (deviceType == FT_DEVICE.FT_DEVICE_2232H) | (deviceType == FT_DEVICE.FT_DEVICE_4232H))
                    {
                        GetDescription(out string description);
                        identifier = description.Substring((description.Length - 1));
                    }
                }

                return identifier;
            }
        }

        #endregion

    }
}
