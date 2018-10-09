using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace sampleApp
{
    class Program
    {
        const string Ds5_XU_GUID = "{C9606CCB-594C-4D25-AF47-CCC496435995}";

        static void Main(string[] args)
        {
            try
            {
                Flow_1();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Execution Failed on: " + ex?.Message);
            }
        }
        public static MediaCapture InitMediaFrameSourceGroup(MediaFrameSourceGroup sensorGroup, string groupName)
        {
            var mediaCaptureSettings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = sensorGroup,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };

            MediaCapture mc = null;
            //init Media Capture object   
            Task.Run(async () =>
            {
                mc = new MediaCapture();
                await mc?.InitializeAsync(mediaCaptureSettings);
            }).Wait();

            return mc;
        }
        public static void Flow_1()
        {
            Console.WriteLine("Press Enter To Start Flow");
            Console.ReadKey();

            //find all source groups of all connected cameras
            IReadOnlyList<MediaFrameSourceGroup> allGroups = null;
            Task.Run(async () =>
            {
                // looking for Media Frame Source Groups
                allGroups = await MediaFrameSourceGroup.FindAllAsync();
            }).Wait();

            //MediaFrameSourceGroup sensorGroup = allGroups[0];

            var depthMC = InitMediaFrameSourceGroup(allGroups[0], "Depth");
            var colorMC = InitMediaFrameSourceGroup(allGroups[0], "Color");
            var sharedMC = InitMediaFrameSourceGroup(allGroups[0], "Shared");


            Console.WriteLine(" Media Capture Inited");
            Console.WriteLine("Press Enter To Set Controls");
            Console.ReadKey();

            //XU 
            var DepthAE = 11;
            var DepthExposure = 3;
            var ManualLaserPower = 4;
            var value = 30;
            depthMC.VideoDeviceController.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, DepthAE), BitConverter.GetBytes((1)));
            depthMC.VideoDeviceController.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, DepthAE), BitConverter.GetBytes((0)));
            depthMC.VideoDeviceController.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, DepthExposure), BitConverter.GetBytes((value)));
            depthMC.VideoDeviceController.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ManualLaserPower), BitConverter.GetBytes((150)));

            Console.WriteLine("Setting Controls Done");
            Console.WriteLine("Press Enter To Dispose Device");
            Console.ReadKey();

            depthMC?.Dispose();
            colorMC?.Dispose();
            sharedMC?.Dispose();

            Console.WriteLine("Test Done");
            Thread.Sleep(10000);

            Console.ReadKey();
        }

    }
}
