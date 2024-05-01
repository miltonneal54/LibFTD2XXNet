using System;
using FTD2xxNet;

namespace FTDI_Test
{
    class Program
    {
        public static FTDI ftdi = new FTDI();
        public static uint deviceCount = 0;
        public static uint versionNum = 0;
        public static FTDI.FT_DEVICE_INFO_NODE[] deviceList = null;
        public static FTDI.FT_DEVICE_INFO_NODE deviceInfo = null;
        public static FTDI.FT_STATUS status;

        static void Main(string[] args)
        {
            try
            {
                status = ftdi.GetLibraryVersion(out string version);
                if (status == FTDI.FT_STATUS.FT_OK)
                {
                    status = ftdi.GetLibraryVersion(ref versionNum);
                    Console.WriteLine(string.Format("DLL Library Version: {0} [{1}]\n", version, versionNum));
                }
            }

            catch (DllNotFoundException e)
            {
                Console.WriteLine(e.Message);
                _ = Console.ReadKey();
                Environment.Exit(1);
            }

            Console.WriteLine("Looking for FTDI devices.");
            status = ftdi.GetNumberOfDevices(ref deviceCount);
            if (status == FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine(string.Format("Number of FTDI devices found: {0}\n", deviceCount));
                ftdi.SetBitMode(0x0, 0x2);
            }

           /*uint index = 1;

            deviceInfo = new FTDI.FT_DEVICE_INFO_NODE();
            status = ftdi.GetDeviceInfoDetail(index, deviceInfo);
            if (status == FTDI.FT_STATUS.FT_OK)
            {
                Console.WriteLine("Displaying device information.");
                Console.WriteLine("==============================");
                Console.WriteLine("Device Index: {0}", index);
                Console.WriteLine("Type:          " + deviceInfo.Type.ToString());
                Console.WriteLine("Description:   " + deviceInfo.Description);
                Console.WriteLine("Serial Number: " + deviceInfo.SerialNumber);
                Console.WriteLine("ID:            " + deviceInfo.ID.ToString() + "\n");
            }

            else
            {
                Console.WriteLine("Device {0} not found.", index);
                Console.ReadKey();
                Environment.Exit(1);
            }*/

            status = ftdi.GetDeviceInfoList(ref deviceList, ref deviceCount);
            if (status == FTDI.FT_STATUS.FT_OK)
            {
                int index = 0;
                Console.WriteLine("Displaying device information.");
                Console.WriteLine("==============================");
                foreach (FTDI.FT_DEVICE_INFO_NODE device in deviceList)
                {
                    Console.WriteLine("Device Index: {0}", index);
                    Console.WriteLine("Type:          " + device.Type.ToString());
                    Console.WriteLine("Description:   " + device.Description);
                    Console.WriteLine("Serial Number: " + device.SerialNumber);
                    Console.WriteLine("ID:            " + device.ID.ToString() + "\n");
                    index++;
                }
            }

            for (int i = 0; i < deviceCount; i++)
            {
                ftdi.OpenByIndex(i);

                status = ftdi.GetCommPort(out string commPortName);
                if (status == FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Device {0} is assigned to serial port: " + commPortName, i);
                }

                status = ftdi.GetDriverVersion(out string version);
                if (status == FTDI.FT_STATUS.FT_OK)
                {
                    Console.WriteLine("Driver Version: " + version + '\n');
                }

                FTDI.FT_DEVICE device = FTDI.FT_DEVICE.FT_DEVICE_UNKNOWN;
                ftdi.GetDeviceType(ref device);
                {

                    if (device == FTDI.FT_DEVICE.FT_DEVICE_BM)
                    {
                        FTDI.FT232B_EEPROM_STRUCTURE ee232b = new FTDI.FT232B_EEPROM_STRUCTURE();
                        status = ftdi.ReadFT232BEEPROM(ee232b);
                    }

                    if (device == FTDI.FT_DEVICE.FT_DEVICE_232R)
                    {
                        FTDI.FT232R_EEPROM_STRUCTURE ee232r = new FTDI.FT232R_EEPROM_STRUCTURE();
                        status = ftdi.ReadFT232REEPROM(ee232r);
                    }
                }

                ftdi.Close();
            }

            bool open = ftdi.IsOpen;
            Console.ReadKey();
        }
    }
}
