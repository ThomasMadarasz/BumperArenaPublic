using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeviceHelper.Runtime
{
    public static class DeviceHelper
    {
        public static Enum.Runtime.DeviceType GetDeviceType(int deviceID)
        {
            InputDevice device = InputSystem.devices.FirstOrDefault(x => x.deviceId == deviceID);

            if (device == null)
            {
                Debug.LogError("No device found with thid ID");
                return Enum.Runtime.DeviceType.Unknown;
            }

            return GetDeviceType(device);
        }

        public static Enum.Runtime.DeviceType GetDeviceType(InputDevice device)
        {
            if (device is Gamepad)
            {
                if (string.Equals(device.description.manufacturer, "Sony Interactive Entertainment", System.StringComparison.InvariantCultureIgnoreCase))
                    return Enum.Runtime.DeviceType.Playstation;
                else return Enum.Runtime.DeviceType.Xbox;
            }
            else return Enum.Runtime.DeviceType.Desktop;
        }
    }
}