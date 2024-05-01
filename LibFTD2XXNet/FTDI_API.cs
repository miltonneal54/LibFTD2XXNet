using System;
using System.Runtime.InteropServices;
using Microsoft.Win32.SafeHandles;

namespace FTD2xxNet
{
    /// <summary>
    /// FTDI DLL API
    /// </summary>
    internal static class FTDI_API
    {
        #region Internal Structures

        /// <summary>
        /// Native type that holds information for the device.
        /// </summary>
        /// <remarks>
        /// Used with FT_GetDeviceInfoList in FTD2XX.DLL.
        /// </remarks>
        [StructLayout(LayoutKind.Sequential, Pack = 0)]
        internal struct FT_DEVICE_LIST_INFO_NODE
        {
            /// <summary>
            /// Indicates device state.
            /// </summary>
            /// <remarks>
            /// Can be any combination of the following: FT_FLAGS_OPENED, FT_FLAGS_HISPEED.
            /// </remarks>
            public uint Flags;

            /// <summary>
            /// Indicates the device type.
            /// </summary>
            /// <remarks>
            /// Can be one of the following: FT_DEVICE_232R, FT_DEVICE_2232C, FT_DEVICE_BM, FT_DEVICE_AM, FT_DEVICE_100AX or FT_DEVICE_UNKNOWN.
            /// </remarks>
            public uint Type;

            /// <summary>
            /// The Vendor ID and Product ID of the device.
            /// </summary>
            public uint ID;

            /// <summary>
            /// The physical location identifier of the device.
            /// </summary>
            public uint LocId;

            /// <summary>
            /// The device serial number.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public byte[] SerialNumber;

            /// <summary>
            /// The device description.
            /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] Description;

            /// <summary>
            /// The device handle.
            /// </summary>
            /// <remarks>
            /// If the device is not open, this value is 0.
            /// </remarks>
            public IntPtr Handle;
        }

        #endregion

        #region D2XX API Functions

        /// <summary>
        /// This function builds a device information list and returns the number of D2XX devices connected to the system.
        /// The list contains information about both unopen and open devices.
        /// </summary>
        /// <param name="numberOfDevices">Pointer to unsigned int to stores the number of devices connected.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_CreateDeviceInfoList", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_CreateDeviceInfoList(ref uint numberOfDevices);

        /// <summary>
        /// This function returns a device information list and the number of D2XX devices in the list.
        /// </summary>
        /// <param name="deviceInfoList">Pointer to an array of FT_DEVICE_LIST_INFO_NODE structures.</param>
        /// <param name="numberOfDevices">Pointer to the number of elements in the array.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetDeviceInfoList", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetDeviceInfoList(IntPtr deviceInfoList, ref uint numberOfDevices);

        /// <summary>
        /// This function returns an entry from the device information list.
        /// </summary>
        /// <param name="index">Index of the entry in the device info list.</param>
        /// <param name="flags">Pointer to unsigned int to store the flag value.</param>
        /// <param name="type">Pointer to unsigned int to store device type.</param>
        /// <param name="id">Pointer to unsigned int to store device ID.</param>
        /// <param name="locId">Pointer to unsigned int to store the device location ID.</param>
        /// <param name="serialNumber">Pointer to buffer to store device serial number as a null-terminated string.</param>
        /// <param name="description">Pointer to buffer to store device description as a null-terminated string.</param>
        /// <param name="handle">Pointer to a variable of type FT_HANDLE (IntPtr) where the handle will be stored.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetDeviceInfoDetail", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetDeviceInfoDetail(uint index, ref uint flags, ref FTDI.FT_DEVICE type,
                                                                ref uint id, ref uint locId, byte[] serialNumber,
                                                                byte[] description, ref IntPtr handle);

        /// <summary>
        /// Open the device and return a handle which will be used for subsequent accesses.
        /// </summary>
        /// <param name="deviceIndex">Index of the device to open.</param>
        /// <param name="handle">Pointer to a variable of type FT_HANDLE (IntPtr) where the handle will be stored.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Open", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Open(int deviceIndex, ref IntPtr handle);

        /// <summary>
        /// Open the specified device and return a handle that will be used for subsequent accesses.
        /// The device can be specified by its serial number, device description or location.
        /// </summary>
        /// <param name="arg">Pointer to an argument whose type depends on the value of dwFlags.</param>
        /// <param name="flags">Serial number, description or location flag</param>
        /// <param name="handle">Pointer to a variable of type FT_HANDLE (IntPtr) where the handle will be stored.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_OpenEx", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_OpenEx(string arg, uint flags, ref IntPtr handle);

        [DllImport("ftd2xx.dll", EntryPoint = "FT_OpenEx", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_OpenEx(uint arg, uint flags, ref IntPtr handle);

        /// <summary>
        /// Close an open device.
        /// </summary>
        /// <param name="handle">Handle for the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Close", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Close(IntPtr handle);

        /// <summary>
        /// Read data from the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="buffer">Pointer to the buffer that receives the data from the device.</param>
        /// <param name="bytesToRead">Number of bytes to be read from the device.</param>
        /// <param name="bytesRead">Pointer to a uint which receives the number of bytes read from the device.</param>
        /// <returns>FT_OK if successful, FT_IO_ERROR otherwise.</returns>
        /// <remarks>FT_Read always returns the number of bytes read in bytesRead</remarks>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Read", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Read(IntPtr handle, byte[] buffer, uint bytesToRead, ref uint bytesRead);

        /// <summary>
        /// Write data to the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="buffer">Pointer to the buffer that contains the data to be written to the device.</param>
        /// <param name="bytesToRead">Number of bytes to write to the device.</param>
        /// <param name="bytesRead">Pointer to a uint which receives the number of bytes written to the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Write", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Write(IntPtr handle, byte[] buffer, uint bytesToRead, ref uint bytesRead);

        /// <summary>
        /// Sets the baud rate for the device.
        /// </summary>
        /// <param name="handle">Handle for the device.</param>
        /// <param name="baudRate">Baud rate.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetBaudRate", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetBaudRate(IntPtr handle, uint baudRate);

        /// <summary>
        /// Sets the data characteristics for the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="wordLenght">Number of bits per word - must be FT_BITS_8 or FT_BITS_7.</param>
        /// <param name="stopBits">Number of stop bits - must be FT_STOP_BITS_1 or FT_STOP_BITS_2.</param>
        /// <param name="parity">Parity - must be FT_PARITY_NONE, FT_PARITY_ODD, FT_PARITY_EVEN, FT_PARITY_MARK or FT_PARITY SPACE.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetDataCharacteristics", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetDataCharacteristics(IntPtr handle, byte wordLenght, byte stopBits, byte parity);

        /// <summary>
        /// Sets the read and write timeouts for the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="readTimeout">Read timeout in milliseconds.</param>
        /// <param name="writeTimeout">Write timeout in milliseconds.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetTimeouts", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetTimeouts(IntPtr handle, uint readTimeout, uint writeTimeout);

        /// <summary>
        /// Sets the flow control for the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="flowControl">Must be one of FT_FLOW_NONE, FT_FLOW_RTS_CTS, FT_FLOW_DTR_DSR or FT_FLOW_XON_XOFF.</param>
        /// <param name="xOn">Character used to signal Xon. Only used if flow control is FT_FLOW_XON_XOFF.</param>
        /// <param name="xOff">Character used to signal Xoff. Only used if flow control is FT_FLOW_XON_XOFF.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetFlowControl", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetFlowControl(IntPtr handle, ushort flowControl, byte xOn, byte xOff);

        /// <summary>
        /// Sets the Data Terminal Ready (DTR) control signal.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetDtr", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetDtr(IntPtr handle);

        /// <summary>
        /// Clears the Data Terminal Ready (DTR) control signal.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_ClrDtr", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_ClrDtr(IntPtr handle);

        /// <summary>
        /// Sets the Request To Send (RTS) control signal.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetRts", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetRts(IntPtr handle);

        /// <summary>
        /// Clears the Request To Send (RTS) control signal.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_ClrRts", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_ClrRts(IntPtr handle);

        /// <summary>
        /// Gets the modem status and line status from the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="modemStatus">Pointer to a uint which receives the modem status and line status from the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetModemStatus", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetModemStatus(IntPtr handle, ref uint modemStatus);

        /// <summary>
        /// Gets the number of bytes in the receive queue.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="queueCountRx">Pointer to a uint which receives the number of bytes in the receive queue.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetQueueStatus", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetQueueStatus(IntPtr handle, ref uint queueCountRx);

        /// <summary>
        /// Get device information for an open device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="type">Pointer to FT_DEVICE to store device type.</param>
        /// <param name="id">Pointer to uint to store device ID.</param>
        /// <param name="serialNumber">Pointer to buffer to store device serial number as a null- terminated string.</param>
        /// <param name="description">Pointer to buffer to store device description as a null-terminated string.</param>
        /// <param name="dummy">Reserved for future use - should be set to NULL.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetDeviceInfo", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetDeviceInfo(IntPtr handle, ref FTDI.FT_DEVICE type, ref uint id, byte[] serialNumber, byte[] description, IntPtr dummy);

        /// <summary>
        /// Gets D2XX DLL version number.
        /// </summary>
        /// <param name="libraryVersion">Pointer to a uint to hold the DLL version number.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetLibraryVersion", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetLibraryVersion(ref uint libraryVersion);

        /// <summary>
        /// Gets the D2XX driver version number.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="driverVersion">Pointer to a uint to hold the driver version number.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetDriverVersion", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetDriverVersion(IntPtr handle, ref uint driverVersion);


        /// <summary>
        /// Retrieves the COM port associated with a device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="comPortNumber">Pointer to a uint which receives the COM port number associated with the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetComPortNumber", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetComPortNumber(IntPtr handle, ref int comPortNumber);

        /// <summary>
        /// Gets the device status including number of characters in the receive queue, 
        /// number of characters in the transmit queue, and the current event status.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="queueCountRx">Pointer to a uint which receives the number of characters in the receive queue.</param>
        /// <param name="queueCountTx">ointer to a uint which receives the number of characters in the transmit queue.P</param>
        /// <param name="eventStatus">Pointer to a uint which receives the current state of the event status.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetStatus", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetStatus(IntPtr handle, ref uint queueCountRx, ref uint queueCountTx, ref uint eventStatus);

        /// <summary>
        /// Sets conditions for event notification.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="eventMask">Conditions that cause the event to be set.</param>
        /// <param name="eventHandle">Interpreted as the handle of an event.( Microsoft.Win32.SafeHandles )</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetEventNotification", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetEventNotification(IntPtr handle, uint eventMask, SafeWaitHandle eventHandle);

        /// <summary>
        /// Sets the 'event' and 'error' special characters for the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="eventChar">Event character.</param>
        /// <param name="eventCharEnable">True if event character is enabled, otherwise false.</param>
        /// <param name="errorChar">Error character.</param>
        /// <param name="errorCharEnable">True if error character is enabled, otherwise false.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetChars", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetChars(IntPtr handle, byte eventChar, bool eventCharEnable, byte errorChar, bool errorCharEnable);

        /// <summary>
        /// Sets the BREAK condition for the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetBreakOn", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetBreakOn(IntPtr handle);

        /// <summary>
        /// Resets the BREAK condition for the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetBreakOff", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetBreakOff(IntPtr handle);

        /// <summary>
        /// Purges receive and transmit buffers in the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="purgeMask">Combination of FT_PURGE_RX and FT_PURGE_TX.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Purge", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Purge(IntPtr handle, uint purgeMask);

        /// <summary>
        /// Sends a reset command to the device.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_ResetDevice", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_ResetDevice(IntPtr handle);

        /// <summary>
        /// Send a reset command to the port.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_ResetPort", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_ResetPort(IntPtr handle);

        /// <summary>
        /// Send a cycle command to the USB port.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_CyclePort", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_CyclePort(IntPtr handle);

        /// <summary>
        /// Scan for hardware changes."
        /// </summary>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        /// <remarks>Calling FT_Rescan is equivalent to clicking the "Scan for hardware changes" button in the Device Manager.
        /// Only USB hardware is checked for new devices. All USB devices are scanned, not just FTDI devices.</remarks>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Rescan", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Rescan();

        /// <summary>
        /// This function forces a reload of the driver for devices with a specific VID and PID combination.
        /// </summary>
        /// <param name="vendorId">Vendor ID of the devices to reload the driver for.</param>
        /// <param name="productId">Product ID of the devices to reload the driver for.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        /// <remarks>Calling FT_Reload forces the operating system to unload and reload the driver for the specified device IDs.
        /// Please note that this function will not work correctly on 64-bit Windows when called from a 32-bit application.</remarks>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_Reload", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_Reload(ushort vendorId, ushort productId);

        /// <summary>
        /// Set the ResetPipeRetryCount value.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="count">Required ResetPipeRetryCount.</param>
        /// <returns></returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetResetPipeRetryCount", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetResetPipeRetryCount(IntPtr handle, uint count);

        /// <summary>
        /// Stops the driver's IN task.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_StopInTask", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_StopInTask(IntPtr handle);

        /// <summary>
        /// Restart the driver's IN task.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_RestartInTask", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_RestartInTask(IntPtr handle);

        /// <summary>
        /// Allows the maximum time in milliseconds that a USB request can remain outstanding to be set.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="deadmanTimeout">Deadman timeout value in milliseconds. Default value is 5000.</param>
        /// <returns></returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetDeadmanTimeout", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetDeadmanTimeout(IntPtr handle, uint deadmanTimeout);

        #endregion

        #region Extended API Functions

        /// <summary>
        /// Set the latency timer value.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="timer">Required value, in milliseconds of the latency timer. Valid range is 2 – 255.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetLatencyTimer", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetLatencyTimer(IntPtr handle, byte timer);

        /// <summary>
        /// Get the current value of the latency timer.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="timer">Pointer to a byte to store latency timer value.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetLatencyTimer", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetLatencyTimer(IntPtr handle, ref byte timer);

        /// <summary>
        /// Enables different chip modes.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="mask">Value for bit mode mask</param>
        /// <param name="mode">Mode value.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetBitMode", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetBitMode(IntPtr handle, byte mask, byte mode);

        /// <summary>
        /// Gets the instantaneous value of the data bus.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="bitMode">Pointer to a byte to store the instantaneous data bus value.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_GetBitMode", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_GetBitMode(IntPtr handle, ref byte bitMode);

        /// <summary>
        /// Set the USB In and Out request transfer sizes.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="inTransferSize">Transfer size for USB In request.</param>
        /// <param name="outTransferSize">Transfer size for USB Out request.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_SetUSBParameters", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_SetUSBParameters(IntPtr handle, uint inTransferSize, uint outTransferSize);

        #endregion

        #region EEPROM Programming Functions

        /// <summary>
        /// Read a value from an EEPROM location.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="offset">EEPROM location to read from.</param>
        /// <param name="value">Pointer to the ushort value read from the EEPROM.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_ReadEE", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_ReadEE(IntPtr handle, uint offset, ref ushort value);

        /// <summary>
        /// Write a value to an EEPROM location.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="offset">EEPROM location to write to.</param>
        /// <param name="value">The ushort value write to the EEPROM.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_WriteEE", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_WriteEE(IntPtr handle, uint offset, ushort value);

        /// <summary>
        /// Erases the device EEPROM.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EraseEE", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EraseEE(IntPtr handle);

        /// <summary>
        /// Read the contents of the EEPROM.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="programData">Pointer to structure of type FT_PROGRAM_DATA.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EE_Read", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EE_Read(IntPtr handle, FT_PROGRAM_DATA programData);

        /// <summary>
        /// Program the EEPROM.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="programData">Pointer to structure of type FT_PROGRAM_DATA.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EE_Program", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EE_Program(IntPtr handle, FT_PROGRAM_DATA programData);

        /// <summary>
        /// Get the available size of the EEPROM user area.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="size">Pointer to a uint that receives the available size, in bytes of the EEPROM user area.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EE_UASize", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EE_UASize(IntPtr handle, ref uint size);

        /// <summary>
        /// Read the contents of the EEPROM user area.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="data">Pointer to a buffer that contains storage for data to be read.</param>
        /// <param name="length">Size in bytes, of buffer that contains storage for the data to be read.</param>
        /// <param name="size">Pointer to a uint that receives the number of bytes read.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EE_UARead", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EE_UARead(IntPtr handle, byte[] data, uint length, ref uint size);

        /// <summary>
        /// Write data into the EEPROM user area.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="data">Pointer to a buffer that contains storage for data to be read.</param>
        /// <param name="length">Size in bytes, of buffer that contains storage for the data to be read.</param>
        /// <returns>FT_OK if successful, otherwise the return value is an FT error code.</returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EE_UAWrite", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EE_UAWrite(IntPtr handle, byte[] data, uint length);

        /// <summary>
        /// Read data from the EEPROM, this command will work for all existing FTDI chipset, but must be used for the FT-X series.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="eepromData">Pointer to a buffer that contains the data to be read.</param>
        /// <param name="eepromDataSize">Size of the eepromData buffer that contains storage for the data to be read.</param>
        /// <param name="manufacturer">Pointer to a null-terminated string containing the manufacturer name.</param>
        /// <param name="manufacturerId">Pointer to a null-terminated string containing the manufacturer ID.</param>
        /// <param name="description">Pointer to a null-terminated string containing the device description.</param>
        /// <param name="serialNumber">Pointer to a null-terminated string containing the device serial number.</param>
        /// <returns></returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EEPROM_Read", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EEPROM_Read(IntPtr handle, IntPtr eepromData, uint eepromDataSize, byte[] manufacturer, byte[] manufacturerId, byte[] description, byte[] serialNumber);

        /// <summary>
        /// Read data from the EEPROM, this command will work for all existing FTDI chipset, but must be used for the FT-X series.
        /// </summary>
        /// <param name="handle">Handle of the device.</param>
        /// <param name="eepromData">Pointer to a buffer that contains the data to be written.</param>
        /// <param name="eepromDataSize">Size of the eepromData buffer that contains storage for the data to be written.</param>
        /// <param name="manufacturer">Pointer to a null-terminated string containing the manufacturer name.</param>
        /// <param name="manufacturerId">Pointer to a null-terminated string containing the manufacturer ID.</param>
        /// <param name="description">Pointer to a null-terminated string containing the device description.</param>
        /// <param name="serialNumber">Pointer to a null-terminated string containing the device serial number.</param>
        /// <returns></returns>
        [DllImport("ftd2xx.dll", EntryPoint = "FT_EEPROM_Program", CallingConvention = CallingConvention.StdCall)]
        internal static extern FTDI.FT_STATUS FT_EEPROM_Program(IntPtr handle, IntPtr eepromData, uint eepromDataSize, byte[] manufacturer, byte[] manufacturerId, byte[] description, byte[] serialNumber);
        #endregion
    }
}
