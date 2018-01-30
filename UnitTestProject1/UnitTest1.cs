﻿using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SynchronicMediaCapture;
using System.Diagnostics;
using System.Collections.Generic;
using System.Threading;
using Windows.Media.Capture.Frames;

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

                StreamConfiguration scIR = new StreamConfiguration(
                    Types.Sensor.IR,
                    Types.Formats.Y,
                    640, 480, 30);


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
                    424, 240, 6);


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

                    Thread.Sleep(7000);

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

                StreamConfiguration scDepth = new StreamConfiguration(
                    Types.Sensor.Depth,
                    Types.Formats.Z,
                    640, 480, 30);

                StreamConfiguration scIR = new StreamConfiguration(
                    Types.Sensor.IR,
                    Types.Formats.Y,
                    640, 480, 30);

                StreamConfiguration scColor = new StreamConfiguration(
                    Types.Sensor.Color,
                    Types.Formats.YUY2,
                    640, 480, 30);

                //Get List Of Device Sensors
                var sensors = d1.Sensors;

                //test for AWG/ PWG/ASR/PSR
                if(sensors.Count == 3)
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

                    //set color controls
                    var res = sensors[2].SetControl(Types.GenericControl.HUE, 5);
                    var val = sensors[2].GetControl(Types.GenericControl.HUE);

                    res = sensors[2].SetControl(Types.GenericControl.AUTO_EXPOSURE_PRIORITY, 1);
                    val = sensors[2].GetControl(Types.GenericControl.AUTO_EXPOSURE_PRIORITY);


                    //set depth control example
                    val = sensors[0].GetControl(Types.GenericControl.AUTO_EXPOSURE);
                    res = sensors[0].SetControl(Types.GenericControl.AUTO_EXPOSURE, 1);
                    val = sensors[0].GetControl(Types.GenericControl.AUTO_EXPOSURE);

                    val = sensors[0].GetControl(Types.GenericControl.EXPOSURE);
                    res = sensors[0].SetControl(Types.GenericControl.EXPOSURE, 20);
                    val = sensors[0].GetControl(Types.GenericControl.EXPOSURE);

                    //val = sensors[0].GetControl(Types.GenericControl.LASER_POWER_MODE);
                    //res = sensors[0].SetControl(Types.GenericControl.LASER_POWER_MODE, 1);
                    //val = sensors[0].GetControl(Types.GenericControl.LASER_POWER_MODE);

                    //val = sensors[0].GetControl(Types.GenericControl.MANUAL_LASER_POWER);
                    //res = sensors[0].SetControl(Types.GenericControl.MANUAL_LASER_POWER, 30);
                    //val = sensors[0].GetControl(Types.GenericControl.MANUAL_LASER_POWER);

                    res = sensors[0].SetControl(Types.GenericControl.GAIN, 17);
                    val = sensors[0].GetControl(Types.GenericControl.GAIN);
                    //set Get Example - depth controls

                    //sensors[0].SensorType = Types.Sensor.IR
                    sensors[0].Start();
                    sensors[1].Start();
                    sensors[2].Start();

                    Thread.Sleep(2000);

                    sensors[0].Stop();
                    sensors[1].Stop();
                    sensors[2].Stop();
                }
            }
        }

        [TestMethod]
        public void TestStreamPerDevice()
        {
            Device d1 = null, d2 = null;
            DeviceManager dm = new DeviceManager();
            var devices = dm.GetListOfConnectedDevices();

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

        private void Color_FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            Logger.Debug("Color");
        }

        private void IR_FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            Logger.Debug("IR");
        }

        private void Depth_FrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            Logger.Debug("Depth Frame Arrived");
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
 