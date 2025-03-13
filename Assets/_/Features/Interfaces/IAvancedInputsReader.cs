using System;
using System.Collections.Generic;

namespace Interfaces.Runtime
{
    public interface IAvancedInputsReader : IInputsReader
    {
        public event Action<int> m_onAdvancedSubmitPerformed;
        public event Action<int> m_onAdvancedCancelPerformed;

        public int GetMainDeviceID();
        public void SetMainDevice(int device);

        public void SetDevicesInCurrentActionMap(IEnumerable<int> device);

        public void SetAvailableDevice(IEnumerable<int> devices);

        public void UnpairDevicesAndRemoveUser();

        public IEnumerable<int> GetAvailableDevices();

        public void AddNewDevice(int deviceID);
    }
}