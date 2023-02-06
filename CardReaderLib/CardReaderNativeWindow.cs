using Linearstar.Windows.RawInput;
using Linearstar.Windows.RawInput.Native;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;

namespace CardReaderLib
{
    internal class CardReaderNativeWindow : NativeWindow
    {
        private string _cardNumber;
        private InputDevice _currentDevice;

        public CardReaderNativeWindow()
        {
            //Create一個Handle，目的在於讓WndProc可以抓到
            CreateHandle(new CreateParams
            {
                X = 0,
                Y = 0,
                Width = 0,
                Height = 0,
                Style = 0x800000,
            });

            AllowedDevices = new List<InputDevice>();
            _cardNumber = "";
        }

        /// <summary>
        /// WndProc觸發時的事件
        /// </summary>
        public event Action<InputDevice, Keys> Input;

        /// <summary>
        /// 讀取到卡號的事件
        /// <para>使用之前要確保有呼叫 <see cref="Register"/></para>
        /// </summary>
        public event Action<InputDevice, string> ReadCardNumber;

        /// <summary>
        /// 允許被偵測的裝置
        /// </summary>
        public List<InputDevice> AllowedDevices { get; set; }

        public InputDevice CurrentDevice => _currentDevice;

        public bool IsEnter { get; set; }

        /// <summary>
        /// 判斷目前Reader是不是正在感應的狀態，是的話為 true
        /// <para>目的在於外部調用時可參考此參數來攔截鍵盤</para>
        /// </summary>
        public bool IsReading { get; private set; }

        /// <summary>
        /// 取得所有裝置
        /// </summary>
        /// <returns></returns>
        public IEnumerable<InputDevice> GetInputDevices()
        {
            return RawInputDevice.GetDevices()
                .Where(c => c.DevicePath != null)
                .Select(c => new InputDevice
                {
                    DevicePath = c.DevicePath,
                    ManufacturerName = c.ManufacturerName,
                    ProductId = c.ProductId,
                    VendorId = c.VendorId,
                });
        }

        /// <summary>
        /// 是否為允許的裝置
        /// </summary>
        /// <param name="device">要判斷的裝置</param>
        /// <returns></returns>
        public bool IsAllowedDevice(InputDevice device)
        {
            if (AllowedDevices == null || AllowedDevices.Count() == 0)
            {
                return false;
            }

            return AllowedDevices.FirstOrDefault(d => d?.DevicePath == device.DevicePath) != null;
        }

        /// <summary>
        /// 註冊 ( 讓WndProc可以抓到 WM_INPUT )
        /// </summary>
        public void Register()
        {
            RawInputDevice.RegisterDevice(HidUsageAndPage.Keyboard, RawInputDeviceFlags.InputSink, Handle);
        }

        /// <summary>
        /// 註銷
        /// </summary>
        public void UnRegister()
        {
            RawInputDevice.UnregisterDevice(HidUsageAndPage.Keyboard);
        }

        //https://github.com/mfakane/rawinput-sharp
        //https://github.com/mfakane/rawinput-sharp/blob/master/RawInput.Sharp.SimpleExample/Program.cs
        protected override void WndProc(ref Message m)
        {
            const int WM_INPUT = 0x00FF;
            if (m.Msg == WM_INPUT)
            {
                var data = RawInputData.FromHandle(m.LParam);
                var keyboard = ((RawInputKeyboardData)data).Keyboard;
                var sourceDevice = new InputDevice
                {
                    DevicePath = data.Device.DevicePath,
                    ManufacturerName = data.Device.ManufacturerName,
                    ProductId = data.Device.ProductId,
                    VendorId = data.Device.VendorId,
                };

                IsEnter = keyboard.VirutalKey == (int)Keys.Enter;

                _currentDevice = sourceDevice;
                Input?.Invoke(sourceDevice, (Keys)keyboard.VirutalKey);

                //每10個字就觸發ReadCardNumber
                if (IsAllowedDevice(sourceDevice) && keyboard.Flags == RawKeyboardFlags.Up && keyboard.VirutalKey != (int)Keys.Enter)
                {
                    _cardNumber += (char)keyboard.VirutalKey;
                    IsReading = true;
                    if (_cardNumber.Length >= 10)
                    {
                        ReadCardNumber?.Invoke(sourceDevice, _cardNumber);
                        _cardNumber = "";
                        IsReading = false;
                    }
                }
                else if (IsAllowedDevice(sourceDevice) && keyboard.VirutalKey != (int)Keys.Enter)
                {
                    //第一次按下去的時候要設為true
                    //否則在外部調用時，按第一個按鍵後IsReading會是false
                    IsReading = true;
                }
            }

            base.WndProc(ref m);
        }
    }
}
