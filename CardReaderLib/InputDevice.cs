using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CardReaderLib
{
    public class InputDevice
    {
        /// <summary>
        /// Device Path
        /// </summary>
        public string DevicePath { get; set; }

        /// <summary>
        /// 製造商名稱
        /// </summary>
        public string ManufacturerName { get; set; }

        /// <summary>
        ///供應商Id
        /// </summary>
        public int VendorId { get; set; }

        /// <summary>
        /// 產品Id
        /// </summary>
        public int ProductId { get; set; }
    }
}
