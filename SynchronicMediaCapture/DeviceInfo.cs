using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronicMediaCapture
{
    public class DeviceInfo
    {
        public string ColorId { get; private set; }
        public string DepthId { get; private set; }
        public string FisheyeId { get; private set; }

        public DeviceInfo()
        {
            ColorId = "N/A";
            DepthId = "N/A";
            FisheyeId = "N/A";
        }
        public void AddDeviceInfo(string name, string id)
        {
            switch(name.ToLower())
            {
                case "depth":
                    {
                        DepthId = id;
                        break;
                    }
                default:
                    return;
            }

        }

    }
}
