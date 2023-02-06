using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CardReaderLib
{
    /// <summary>
    /// 將讀卡機的鍵盤輸入事件攔截並轉換成卡片號碼
    /// </summary>
    public class CardReader : IDisposable
    {
        private readonly CardReaderNativeWindow _nativeWindow;

        /// <summary>
        /// 將讀卡機的鍵盤輸入事件攔截並轉換成卡片號碼
        /// </summary>
        public CardReader()
        {
            _nativeWindow = new CardReaderNativeWindow();
            _nativeWindow.Register();

            _nativeWindow.Input += InputEvent;
            _nativeWindow.ReadCardNumber += ReadCardNumberEvent;
        }

        /// <summary>
        /// 鍵盤輸入時觸發的事件
        /// </summary>
        public event Action<InputDevice, Keys> Input;

        /// <summary>
        /// 讀取到卡號的事件
        /// <para>只有在 <see cref="AllowedDevices"/> 的裝置才會被觸發</para>
        /// </summary>
        public event Action<InputDevice, string> ReadCardNumber;

        /// <summary>
        /// 允許的裝置
        /// </summary>
        public IEnumerable<InputDevice> AllowedDevices => _nativeWindow.AllowedDevices;

        public bool IsEnter => _nativeWindow.IsEnter;

        /// <summary>
        /// 判斷目前Reader是不是正在感應的狀態，是的話為 true
        /// <para>目的在於外部調用時可參考此參數來攔截鍵盤</para>
        /// </summary>
        public bool IsReading => _nativeWindow.IsReading;

        /// <summary>
        /// 目前的裝置
        /// </summary>
        public InputDevice CurrentDevice => _nativeWindow.CurrentDevice;

        /// <summary>
        /// 新增裝置
        /// </summary>
        /// <param name="device"></param>
        public void AddInputDevice(InputDevice device)
        {
            if(device == null)
            {
                return;
            }

            if(_nativeWindow.AllowedDevices.Find(c=>c.DevicePath == device.DevicePath) != null)
            {
                return;
            }

            _nativeWindow.AllowedDevices.Add(device);
        }

        /// <summary>
        /// 釋放資源
        /// </summary>
        public void Dispose()
        {
            _nativeWindow.UnRegister();
            _nativeWindow.Input -= InputEvent;
            _nativeWindow.ReadCardNumber -= ReadCardNumberEvent;
        }

        /// <summary>
        /// 取得所有裝置
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InputDevice> GetInputDevices()
        {
            return _nativeWindow.GetInputDevices();
        }

        /// <summary>
        /// 移除裝置
        /// </summary>
        /// <param name="device"></param>
        public void RemoveInputDevice(InputDevice device)
        {
            if (device == null)
            {
                return;
            }

            if (_nativeWindow.AllowedDevices.Find(c => c.DevicePath == device.DevicePath) == null)
            {
                return;
            }

            _nativeWindow.AllowedDevices.Remove(device);
        }

        private void InputEvent(InputDevice device, Keys key)
        {
            Input?.Invoke(device, key);
        }

        private void ReadCardNumberEvent(InputDevice device, string cardNumber)
        {
            ReadCardNumber?.Invoke(device, cardNumber);
        }
    }
}
