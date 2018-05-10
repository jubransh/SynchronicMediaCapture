using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronicMediaCapture;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using Windows.Media.Capture.Frames;
using System.Linq;
using Windows.Media.Capture;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void DebugTest()
        {
            //var d = BitConverter.ToDouble(new byte[] { 0x20, 0x0, 0x0, 0x0, 0x20, 0x0, 0x0, 0x0 }, 0);
        }

        [TestMethod]
        public void IR_TEST_ONLY()
        {
            Device d1 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            if (devices.Count == 0)
                return;

            if (devices.Count > 0)
            {
                d1 = devices[0]; //get first device            
                d1.InitDevice(); // init device
                d1.InitSensors(); // init device sensors

                d1.ResetCamera();
                Thread.Sleep(5000);

                //resacan devices *************************
                d1.Close();
                d1 = null;
                dm = new DeviceManager();
                devices = dm.GetListOfConnectedDevices();
                d1 = devices[0]; //get first device            
                d1.InitDevice(); // init device
                d1.InitSensors(); // init device sensors
                //******************************************

                StreamConfiguration scIR = new StreamConfiguration(
                    Types.Sensor.IR,
                    Types.Formats.UYVY,
                    424, 240, 15);


                //Get List Of Device Sensors
                var sensors = d1.Sensors;

                //test for AWG/ PWG/ASR/PSR
                if (sensors.Count == 3)
                {
                    //Init And configure the IR sensor
                    sensors[1].Init();
                    sensors[1].Configure(scIR);
                    sensors[1].FrameReader.FrameArrived += IR_FrameReader_FrameArrived;

                    //sensors[0].SensorType = Types.Sensor.IR
                    sensors[1].Start();

                    Thread.Sleep(3000);

                    sensors[1].Stop();
                }
            }
        }
        [TestMethod]
        public void Depth_TEST_ONLY()
        {
            Device d1 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            if (devices.Count == 0)
                return;

            if (devices.Count > 0)
            {
                d1 = devices[0]; //get first device 
                d1.InitDevice(); // init device
                d1.InitSensors(); // init device sensors

                StreamConfiguration scIR = new StreamConfiguration(
                    Types.Sensor.Depth,
                    Types.Formats.Z,
                    640, 480, 90);

                //Get List Of Device Sensors
                var sensors = d1.Sensors;

                //test for AWG/ PWG/ASR/PSR
                if (sensors.Count == 3)
                {
                    //Init And configure the IR sensor
                    sensors[0].Init();
                    sensors[0].Configure(scIR);
                    sensors[0].FrameReader.FrameArrived += Depth_FrameReader_FrameArrived;

                    //sensors[0].SensorType = Types.Sensor.IR
                    sensors[0].Start();

                    Thread.Sleep(10000);

                    sensors[0].Stop();
                }
            }
        }
        [TestMethod]
        public void TestStreamPerSensor_ColorAndDepth()
        {
            Device d1 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            if (devices.Count == 0)
                return;

            if (devices.Count > 0)
            {
                d1 = devices[0]; //get first device 
                d1.InitDevice(); // init device
                d1.InitSensors(); // init device sensors

                StreamConfiguration scDepth = new StreamConfiguration(
                    Types.Sensor.Depth,
                    Types.Formats.Z,
                    640, 480, 30);

                //StreamConfiguration scIR = new StreamConfiguration(
                //    Types.Sensor.IR,
                //    Types.Formats.Y,
                //    640, 480, 30);

                StreamConfiguration scColor = new StreamConfiguration(
                    Types.Sensor.Color,
                    Types.Formats.YUY2,
                    640, 480, 30);

                //Get List Of Device Sensors
                var sensors = d1.Sensors;

                //test for AWG/ PWG/ASR/PSR
                if (sensors.Count == 3)
                {
                    //Init And configure the depth sensor
                    sensors[0].Init();
                    sensors[0].Configure(scDepth);
                    sensors[0].FrameReader.FrameArrived += Depth_FrameReader_FrameArrived;

                    ////Init And configure the IR sensor
                    //sensors[1].Init();
                    //sensors[1].Configure(scIR);
                    //sensors[1].FrameReader.FrameArrived += IR_FrameReader_FrameArrived;

                    //Init And configure the color sensor
                    sensors[2].Init();
                    sensors[2].Configure(scColor);
                    sensors[2].FrameReader.FrameArrived += Color_FrameReader_FrameArrived;

         
                    //sensors[0].SensorType = Types.Sensor.IR
                    sensors[0].Start();
                    //sensors[1].Start();
                    sensors[2].Start();

                    Thread.Sleep(10000);

                    sensors[0].Stop();
                    //sensors[1].Stop();
                    sensors[2].Stop();
                }
            }
        }

        [TestMethod]
        public void TestStreamPerSensor()
        {
            Device d1 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            if (devices.Count == 0)
                return;

            if (devices.Count > 0)
            {
                d1 = devices[0]; //get first device 
                d1.InitDevice(); // init device
                d1.InitSensors(); // init device sensors

                var ssss = d1.GetSerial();

                StreamConfiguration scIR = new StreamConfiguration(
                    Types.Sensor.IR,
                    Types.Formats.Y,
                    640, 480, 30);

                StreamConfiguration scDepth = new StreamConfiguration(
                    Types.Sensor.Depth,
                    Types.Formats.Z,
                    640, 480, 30);

                StreamConfiguration scColor = new StreamConfiguration(
                    Types.Sensor.Color,
                    Types.Formats.YUY2,
                    640, 480, 30);

                //Get List Of Device Sensors
                var sensors = d1.Sensors;
                sensors[0].Init();

                //Get Supported Controls
                var supportedControls = sensors[0].GetSupportedControls();

                //test for AWG/ PWG/ASR/PSR
                if (sensors.Count == 3)
                {
                    //Init And configure the depth sensor
                    sensors[0].Init();

                    sensors[0].Configure(scDepth);
                    sensors[0].FrameReader.FrameArrived += Depth_FrameReader_FrameArrived;                    

                    //Init And configure the IR sensor
                    sensors[1].Init();
                    sensors[1].Configure(scIR);
                    sensors[1].FrameReader.FrameArrived += IR_FrameReader_FrameArrived;

                    //Init And configure the color sensor
                    sensors[2].Init();
                    sensors[2].Configure(scColor);
                    sensors[2].FrameReader.FrameArrived += Color_FrameReader_FrameArrived;

                    ////set color controls
                    //var res = sensors[2].SetControl(Types.GenericControl.HUE, 5);
                    //var val = sensors[2].GetControl(Types.GenericControl.HUE);

                    //res = sensors[2].SetControl(Types.GenericControl.AUTO_EXPOSURE_PRIORITY, 1);
                    var val = sensors[2].GetControl(Types.GenericControl.AUTO_EXPOSURE_PRIORITY);


                    ////set depth control example
                    //val = sensors[0].GetControl(Types.GenericControl.AUTO_EXPOSURE);
                    //res = sensors[0].SetControl(Types.GenericControl.AUTO_EXPOSURE, 1);
                    //val = sensors[0].GetControl(Types.GenericControl.AUTO_EXPOSURE);

                    //val = sensors[0].GetControl(Types.GenericControl.EXPOSURE);
                    //res = sensors[0].SetControl(Types.GenericControl.EXPOSURE, 20);
                    //val = sensors[0].GetControl(Types.GenericControl.EXPOSURE);

                    ////val = sensors[0].GetControl(Types.GenericControl.LASER_POWER_MODE);
                    ////res = sensors[0].SetControl(Types.GenericControl.LASER_POWER_MODE, 1);
                    ////val = sensors[0].GetControl(Types.GenericControl.LASER_POWER_MODE);

                    ////val = sensors[0].GetControl(Types.GenericControl.MANUAL_LASER_POWER);
                    ////res = sensors[0].SetControl(Types.GenericControl.MANUAL_LASER_POWER, 30);
                    ////val = sensors[0].GetControl(Types.GenericControl.MANUAL_LASER_POWER);

                    //res = sensors[0].SetControl(Types.GenericControl.GAIN, 17);
                    //val = sensors[0].GetControl(Types.GenericControl.GAIN);
                    ////set Get Example - depth controls

                    //sensors[0].SensorType = Types.Sensor.IR
                    sensors[0].Start();
                    sensors[1].Start();
                    sensors[2].Start();

                    Thread.Sleep(5000);

                    sensors[0].Stop();
                    sensors[1].Stop();
                    sensors[2].Stop();
                }
            }
        }

        [TestMethod]
        public void TestStartStop()
        {
            int iterations_number = 2500;

            Device d1 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            if (devices.Count == 0)
                return;

            if (devices.Count > 0)
            {
                d1 = devices[0]; //get first device 
                d1.InitDevice(); // init device
                d1.InitSensors(); // init device sensors

                var ssss = d1.GetSerial();

                StreamConfiguration scIR = new StreamConfiguration(
                    Types.Sensor.IR,
                    Types.Formats.Y,
                    480, 270, 6);

                StreamConfiguration scDepth = new StreamConfiguration(
                    Types.Sensor.Depth,
                    Types.Formats.Z,
                    480, 270, 6);

                StreamConfiguration scColor = new StreamConfiguration(
                    Types.Sensor.Color,
                    Types.Formats.YUY2,
                    424, 240, 6);

                //Get List Of Device Sensors
                var sensors = d1.Sensors;
                for (int i = 1; i <= iterations_number; i++)
                {
                    Thread.Sleep(2000);
                    Logger.Debug(String.Format("Start Iteration: {0}", i.ToString()));
                    //test for AWG/ PWG/ASR/PSR
                    if (sensors.Count == 3)
                    {
                        //Init And configure the depth sensor
                        sensors[0].Init();
                        sensors[0].Configure(scDepth);
                        sensors[0].FrameReader.FrameArrived += Depth_FrameReader_FrameArrived;

                        //Init And configure the IR sensor
                        sensors[1].Init();
                        sensors[1].Configure(scIR);
                        sensors[1].FrameReader.FrameArrived += IR_FrameReader_FrameArrived;

                        //Init And configure the color sensor
                        sensors[2].Init();
                        sensors[2].Configure(scColor);
                        sensors[2].FrameReader.FrameArrived += Color_FrameReader_FrameArrived;

                        //sensors[0].SensorType = Types.Sensor.IR
                        sensors[0].Start();
                        sensors[1].Start();
                        sensors[2].Start();

                        Thread.Sleep(5000);

                        sensors[0].Stop();
                        sensors[1].Stop();
                        sensors[2].Stop();
                    }
                    else
                    {
                        Logger.Error(String.Format("sensors.Count == {0}", 3));
                    }
                    Logger.Debug(String.Format("End Iteration: {0}", i.ToString()));
                }
            }
        }

        [TestMethod]
        public void GeneralDeviceTest()
        {
            Device device = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            if (devices.Count == 0)
                return;

            device = devices[0];
            device.InitDevice();

            StreamConfigurations sCs = new StreamConfigurations();

            StreamConfiguration sC = new StreamConfiguration(
                Types.Sensor.Color,
                Types.Formats.YUY2,
                1280, 720, 30);

            sCs.Append(sC);

            device.ConfigureStream(sCs.GetListOfStreamConfigurations());
            device.OnDataReceived += Frame_Arrived;
            device.RegisterNonIntelCameraFramesCallback();
            device.StartStreaming();
            Thread.Sleep(10000);
            device.StopStreaming();
            device.Close();
        }

        private void Frame_Arrived(object source, Events.TestEventArgs e)
        {
            Logger.Debug("############### " + e.GetInfo().format);
            var ts = e.GetInfo().sw_timeStamp;
            Logger.Debug(ts);
        }

        [TestMethod]
        public void DepthControlTest()
        {
            DeviceManager dm = new DeviceManager();
            var device = dm.GetListOfConnectedDevices()[0];
            device.InitDevice();
            device.InitSensors();
            var controlValue = device.GetControl(Types.GenericControl.LASER_POWER_MODE, Types.Sensor.Depth);

        }
        [TestMethod]
        public void TestStreamPerDevice()
        {
            Device d1 = null, d2 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

            DeviceManager dm2 = new DeviceManager();
            var devices2 = dm2.GetListOfConnectedDevices();


            if (devices.Count == 0)
                return;

            if (devices.Count > 0)
            {
                d1 = devices[0];                
                //d1.InitSensors();
               
                //================ Example about how to run by sensor and not by device =======================
                //=============================================================================================

                d1.InitDevice();
                string fV = d1.GetFwVersion();
                string sN = d1.GetSerial();

                ////Reset
                //var res1 = d1.SendXuCommand("14 00 AB CD 20 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");

                //Get Depth Calibration
                var res1 = d1.SendXuCommand("14 00 AB CD 15 00 00 00 1f 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00");
                var dd = res1.BytesResult;
                //Reset Camera 
                d1.SetU3Mode(true);
                //var result = d1.SendCommand(Environment.CurrentDirectory, "gvd");               
                //d1.ResetCamera();

                var ver = d1.GetFwVersion();
                var serial = d1.GetSerial();


                //Set Color Controls
                bool res;
                res = d1.SetControl(Types.GenericControl.EXPOSURE, Types.Sensor.Color, 0);
                res = d1.SetControl(Types.GenericControl.GAIN, Types.Sensor.Color, 0);
                res = d1.SetControl(Types.GenericControl.BACKLIGHT_COMPENSATION, Types.Sensor.Color, 1);
                res = d1.SetControl(Types.GenericControl.BRIGHTNESS, Types.Sensor.Color, 64);
                res = d1.SetControl(Types.GenericControl.CONTRAST, Types.Sensor.Color, 100);
                res = d1.SetControl(Types.GenericControl.GAMMA, Types.Sensor.Color, 500);
                res = d1.SetControl(Types.GenericControl.HUE, Types.Sensor.Color, 180);
                res = d1.SetControl(Types.GenericControl.SATURATION, Types.Sensor.Color, 100);
                res = d1.SetControl(Types.GenericControl.SHARPNESS, Types.Sensor.Color, 100);
                res = d1.SetControl(Types.GenericControl.WHITEBALANCE, Types.Sensor.Color, 4610);

                //Get Color Controls
                double val;
                val = d1.GetControl(Types.GenericControl.EXPOSURE, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.GAIN, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.BACKLIGHT_COMPENSATION, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.BRIGHTNESS, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.CONTRAST, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.GAMMA, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.HUE, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.SATURATION, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.SHARPNESS, Types.Sensor.Color);
                val = d1.GetControl(Types.GenericControl.WHITEBALANCE, Types.Sensor.Color);

                var supportedControls = d1.GetSupportedControls();
            }

            if (devices.Count > 1)
            {
                d2 = devices[1];
                d2.InitDevice();
                var ver = d2.GetFwVersion();
                var serial = d2.GetSerial();
            }

            StreamConfiguration sc = new StreamConfiguration(
                Types.Sensor.Depth,
                Types.Formats.Z,
                640, 480, 30);

            StreamConfiguration sc2 = new StreamConfiguration(
                Types.Sensor.Color,
                Types.Formats.YUY2,
                640, 480, 30);

            StreamConfiguration sc3 = new StreamConfiguration(
                Types.Sensor.IR,
                Types.Formats.Y,
                640, 480, 30);

            StreamConfigurations sCs = new StreamConfigurations();
            sCs.Append(sc);
            sCs.Append(sc2);
            sCs.Append(sc3);

            //Create list of stream Configurations
            //List<StreamConfiguration> ListOfConfigurations = new List<StreamConfiguration>();
            //ListOfConfigurations.Add(sc);
            //ListOfConfigurations.Add(sc2);

            d1.ConfigureStream(sCs.GetListOfStreamConfigurations()/*ListOfConfigurations*/);

            //bool success = d.SetControl("Exposure", -8);
            //var valAfter = d.GetControl("Exposure");

            d1.OnDataReceived += D_OnDataReceived;
            d1.RegisterFramesCallback();            
            d1.StartStreaming();
            Thread.Sleep(10000);
            d1.StopStreaming();

            d1.Close();
        }


        const string Ds5_XU_GUID = "{C9606CCB-594C-4D25-AF47-CCC496435995}";
        /* =============== List Of All XU Controls ===============
        const uint8_t DS5_HWMONITOR                       = 1;
        const uint8_t DS5_DEPTH_EMITTER_ENABLED           = 2;
        const uint8_t DS5_EXPOSURE                        = 3;
        const uint8_t DS5_LASER_POWER                     = 4;
        const uint8_t DS5_ERROR_REPORTING                 = 7;
        const uint8_t DS5_EXT_TRIGGER                     = 8;
        const uint8_t DS5_ASIC_AND_PROJECTOR_TEMPERATURES = 9;
        const uint8_t DS5_ENABLE_AUTO_WHITE_BALANCE       = 0xA;
        const uint8_t DS5_ENABLE_AUTO_EXPOSURE            = 0xB; 
        =========================================================*/

        public enum XU_Controls
        {
            DS5_HWMONITOR = 1, //For Terminal
            DepthAE = 11,
            ManualLaserPower = 4,
            LaserPowerMode = 2,     //On or Off
            DepthExposure = 3,
        };

        [TestMethod]
        public void SendXU()
        {
            MediaCapture depthMC = null;
            MediaFrameSourceGroup depthSourceGroup;

            //looking for Media Frame Source Groups"
            IReadOnlyList<MediaFrameSourceGroup> allGroups = null;
            Task.Run(async () =>
            {
                //looking for Media Frame Source Groups"
                allGroups = await MediaFrameSourceGroup.FindAllAsync();
            }).Wait();


            //if no devices found
            if (allGroups == null || allGroups.ToList().Count == 0)            
                return;
            

            //Get Second Source group For Example 
            depthSourceGroup = allGroups[2];
            
            var depthMediaCaptureSettings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = depthSourceGroup,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };


            //init Media Capture object   
            Task.Run(async () =>
            {
                depthMC = new MediaCapture();
                await depthMC?.InitializeAsync(depthMediaCaptureSettings);
            }).Wait();

            var controller = depthMC.VideoDeviceController;
            var controlValue = 0;

            //Set XU Data/ Property (For Examople Laser Power)
            var ControlId = (int)XU_Controls.ManualLaserPower;
            controller.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId), BitConverter.GetBytes(controlValue));

            Console.WriteLine("done");
        }


        private void Color_FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            Logger.Debug("Color");
        }

        private void IR_FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var frame = sender.TryAcquireLatestFrame();
            var fmt = frame.Format;
            var intelCaptureTiming = "2BF10C23-BF48-4C54-B1F9-9BB19E70DB05";
            Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME intelCaptureTimingMD = new Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME();
            var properties = frame.Properties;
            var intelCaptureTimingMDBytes = properties.Where(x => x.Key.ToString().ToUpper() == intelCaptureTiming).First().Value;
            intelCaptureTimingMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME>((byte[])intelCaptureTimingMDBytes);
            int id = (int)intelCaptureTimingMD.frameCounter;
            Logger.Debug("IR Frame Arrived = " + id);
        }
        private void Depth_FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var frame = sender.TryAcquireLatestFrame();
            var fmt = frame.Format;
            var intelCaptureTiming = "2BF10C23-BF48-4C54-B1F9-9BB19E70DB05";
            Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME intelCaptureTimingMD = new Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME();
            var properties = frame.Properties;
            var intelCaptureTimingMDBytes = properties.Where(x => x.Key.ToString().ToUpper() == intelCaptureTiming).First().Value;
            intelCaptureTimingMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME>((byte[])intelCaptureTimingMDBytes);
            int id = (int)intelCaptureTimingMD.frameCounter;
            Logger.Debug("Depth Frame Arrived = " + id);
        }

        private void FrameReader_FrameArrived(Windows.Media.Capture.Frames.MediaFrameReader sender, Windows.Media.Capture.Frames.MediaFrameArrivedEventArgs args)
        {
            var frame = sender.TryAcquireLatestFrame();
            var fmt = frame.Format;

            if (fmt.Subtype.ToLower().Equals("d16"))
                Logger.Debug("Depth");

            if (fmt.Subtype.ToLower().Equals("l8"))
                Logger.Debug("IR");

            if (fmt.Subtype.ToLower().Equals("yuy2"))
                Logger.Debug("Color");

        }

        private void D_OnDataReceived(object source, Events.TestEventArgs e)
        {
            Logger.Debug("############### " + e.GetInfo().format);            
        }
    }
}
 