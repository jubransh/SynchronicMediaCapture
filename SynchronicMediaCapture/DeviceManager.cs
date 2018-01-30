using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//Frame Server Libraries 
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;

namespace SynchronicMediaCapture
{
    public class DeviceManager
    {
        int _numOfConnectedDevices;
        List<Device> _connectedDevices;
        MediaCapture  mediaCapture;
        MediaFrameSourceGroup intelSourceGroup;

        public DeviceManager()
        {
            //Start Logger
            var logPath = string.Format(@"c:\temp\SynchronicMC_{0}.log", DateTime.Now.ToString().Replace('/', '.').Replace(" ", "_").Replace(':', '-'));
            Logger.StartLogger(logPath);

            Logger.PrintTitle("Device Manager Constructor");

            //init count of connected devices and the list of the connected devices
            _numOfConnectedDevices = 0;
            _connectedDevices = new List<Device>();
            Logger.Debug("Preparing List of Connected devices");

            //init media capture object
            mediaCapture = new MediaCapture();
            Logger.Debug("Media Capture Object created");

            //find all source groups of all connected cameras
            IReadOnlyList<MediaFrameSourceGroup> allGroups = null;
            Task.Run(async () => 
            {
                Logger.Debug("looking for Media Frame Source Groups");
                allGroups = await MediaFrameSourceGroup.FindAllAsync();
            }).Wait();

            //if no devices found
            if (allGroups == null || allGroups.ToList().Count == 0)
            {
                Logger.Debug("Media Frame Source Groups is Null or empty");
                return;
            }
            Logger.Debug(string.Format("{0} Media Frame Source Groups were Found", allGroups.ToList().Count));

            //scan all sensor groups to export the List/ count of the connected devices
            List<string> deviceIdsList = new List<string>();
            DeviceInfo dI = new DeviceInfo();
            Logger.Debug("DeviceInfo Object created");

            Logger.Debug("Quiring Media Frame Source Groups were found");
            foreach (var sG in allGroups)
            {                
                //check what is the type of the source group
                Types.SourceGroupType sGT = GetTypeOfSourceGroup(sG);
                Logger.Debug("GetTypeOfSourceGroup() returned " + sGT);

                var currentId = sG.SourceInfos[0].Id;
                Logger.Debug("Getting ID of source group - result: " + currentId);

                var uniqDeviceId = currentId.Split('#')[3].Split('&')[1];

                //check if new device 
                Logger.Debug("checking if new device");
                if (deviceIdsList.IndexOf(uniqDeviceId) == -1)
                {
                    Logger.Debug("New device found - adding device to list of devices");

                    deviceIdsList.Add(uniqDeviceId);
                    //lastUniqDeviceId = uniqDeviceId;

                    _numOfConnectedDevices++;
                    var newDevice = new Device(currentId/*mediaCapture/*, sG*/);
                    _connectedDevices.Add(newDevice);
                    Logger.Debug("new device added to the connected devices list");
                    _connectedDevices[_connectedDevices.Count - 1].AddSourceGroup(sGT, sG);
                }
                else
                {
                    _connectedDevices[deviceIdsList.IndexOf(uniqDeviceId)].AddSourceGroup(sGT, sG);
                }
            }
            Logger.PrintTitle();
        }
        public List<Device> GetListOfConnectedDevices()
        {
            Logger.Debug("GetListOfConnectedDevices() Method Called");
            return _connectedDevices;
        }
        public int GetNumberOfConnectedDevices()
        {
            Logger.Debug("GetNumberOfConnectedDevices() Method Called");
            return _numOfConnectedDevices;
        }

        private Types.SourceGroupType GetTypeOfSourceGroup(MediaFrameSourceGroup sG)
        {
            Logger.Debug(string.Format("Running  GetTypeOfSourceGroup() method on {0}", sG.DisplayName));

            //check what is the type of the source group
            switch (sG.DisplayName)
            {
                case "Intel RS400 Cameras":
                    return Types.SourceGroupType.SHARED;

                case "Intel(R) RealSense(TM) 410 Depth":
                case "Intel(R) RealSense(TM) 415 Depth":
                case "Intel(R) RealSense(TM) 415 with RGB Module Depth":
                case "Intel(R) RealSense(TM) 430 Depth":
                case "Intel(R) RealSense(TM) 430 with RGB Module Depth":
                case "Intel(R) RealSense(TM) 430 with Tracking Module Depth":
                    return Types.SourceGroupType.DEPTH;

                case "Intel(R) RealSense(TM) 415 RGB":
                case "Intel(R) RealSense(TM) 415 with RGB Module RGB":
                case "Intel(R) RealSense(TM) 430 with RGB Module RGB":
                    return Types.SourceGroupType.COLOR;

                case "Intel(R) RealSense(TM) 430 with Tracking Module FishEye":
                    return Types.SourceGroupType.FISHEYE;

                default:
                    Logger.Debug("MediaFrameSourceGroup is: " + sG.DisplayName);
                    return Types.SourceGroupType.UNKNOWN;
            }
        }
        
    }
}
