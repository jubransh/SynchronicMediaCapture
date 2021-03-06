﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace SynchronicMediaCapture
{
    public static class Types
    {
        public static Guid DS5_HW_TIME_STAMP_GUID = new Guid("D3C6ABAC-291A-4C75-9F47-D7B284A52619");
        public static Guid DS5_INTEL_CAPTURE_TIMING_GUID = new Guid("2BF10C23-BF48-4C54-B1F9-9BB19E70DB05");
        public static Guid DS5_INTEL_DEPTH_CONTROL_GUID = new Guid("482f9b07-3668-43fe-ad28-e3db3463bcb9");
        public static Guid DS5_INTEL_COLOR_CONTROL_GUID = new Guid("8dcfd1f9-859c-4548-8bbe-40629d36d5b3");


        public struct REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING
        {
            public Int32 version;
            public Int32 flag;
            public Int32 frameCounter;
            public Int32 opticalTimestamp;   //In millisecond unit
            public Int32 readoutTime;        //The readout time in millisecond second unit
            public Int32 exposureTime;       //The exposure time in millisecond second unit
            public Int32 frameInterval;     //The frame interval in millisecond second unit
            public Int32 pipeLatency;        //The latency between start of frame to frame ready in USB buffer
        };
        public struct REAL_SENSE_RS400_DEPTH_METADATA_INTEL_DEPTH_CONTROL
        {
            public UInt32 version;
            public UInt32 flag;
            public UInt32 gainLevel;
            public UInt32 manualExposure;
            public UInt32 laserPower;
            public UInt32 autoExposureMode;
            public UInt32 exposurePriority;
            public UInt32 leftExposureROI;
            public UInt32 rightExposureROI;
            public UInt32 topExposureROI;
            public UInt32 bottomExposureROI;
            public UInt32 preset;
        };
        public struct REALSENSE_SAMPLE_RS400_COLOR_CONTROL_METADATA
        {
            public UInt32 version;
            public UInt32 flag;
            public UInt32 brightness;
            public UInt32 contrast;
            public UInt32 saturation;
            public UInt32 sharpness;
            public UInt32 auto_Exp_Mode;
            public UInt32 auto_WB_Temp;
            public UInt32 gain;
            public UInt32 backlight_Comp;
            public UInt32 gamma;
            public UInt32 hue;
            public UInt32 manual_Exp;
            public UInt32 manual_WB;
            public UInt32 powerLineFrequncy;
            public UInt32 low_Light_Comp;
        };


        public enum LogLevel { DEBUG, WARNING, ERROR};
        public enum ControlType { STANDARD, XU, UNKNOWN};
        public enum ControlName { DEPTH_EXPOSURE, COLOR_EXPOSURE, DEPTH_AE, COLOR_AE, COLOR_EXP_PRIORITY, UNKNOWN};
        public enum Controls
        {
            DepthExposure,
            Exposure,
            DepthAE,
            AutoExposure,
            LaserPower,
            LaserPowerOnOff,
            Brightness,
            Contrast,
            WhiteBalance,
            ExposurePriority,
            Hue,
            BacklightCompensation,
            Unknown
        }
        public enum GenericControl
        {
            BRIGHTNESS,
            CONTRAST,
            HUE,
            SATURATION,
            SHARPNESS,
            GAMMA,
            COLORENABLE,
            WHITEBALANCE,
            BACKLIGHT_COMPENSATION,
            GAIN,
            DIGITAL_MULTIPLIER,
            DIGITAL_MULTIPLIER_LIMIT,
            WHITEBALANCE_COMPONENT,
            POWERLINE_FREQUENCY,
            PAN,
            TILT,
            ROLL,
            ZOOM,
            EXPOSURE,
            IRIS,
            FOCUS,
            SCANMODE,
            PRIVACY,
            PANTILT,
            PAN_RELATIVE,
            TILT_RELATIVE,
            ROLL_RELATIVE,
            ZOOM_RELATIVE,
            EXPOSURE_RELATIVE,
            IRIS_RELATIVE,
            FOCUS_RELATIVE,
            PANTILT_RELATIVE,
            FOCAL_LENGTH,
            AUTO_EXPOSURE_PRIORITY,
            AUTO_EXPOSURE,
            MANUAL_LASER_POWER,
            LASER_POWER_MODE,
            UNKNOWN
        }
        public enum Control
        {
            BRIGHTNESS,
            CONTRAST,
            HUE,
            SATURATION,
            SHARPNESS,
            GAMMA,
            COLORENABLE,
            WHITEBALANCE,
            BACKLIGHT_COMPENSATION,
            GAIN,
            DIGITAL_MULTIPLIER,
            DIGITAL_MULTIPLIER_LIMIT,
            WHITEBALANCE_COMPONENT,
            POWERLINE_FREQUENCY,
            
            PAN,
            TILT,
            ROLL,
            ZOOM,
            EXPOSURE,
            IRIS,
            FOCUS,
            SCANMODE,
            PRIVACY,
            PANTILT,
            PAN_RELATIVE,
            TILT_RELATIVE,
            ROLL_RELATIVE,
            ZOOM_RELATIVE,
            EXPOSURE_RELATIVE,
            IRIS_RELATIVE,
            FOCUS_RELATIVE,
            PANTILT_RELATIVE,
            FOCAL_LENGTH,
            AUTO_EXPOSURE_PRIORITY,

            //Depth Control
            DEPTH_EXPOSURE,
            COLOR_EXPOSURE,
            DEPTH_AE,
            COLOR_AE,
            MANUAL_LASER_POWER,
            LASER_POWER_MODE,
            DEPTH_GAIN,
            UNKNOWN
        }
        public enum Sensors { DEPTH, IR, COLOR, FISHEYE, MOTION, MIXED, UNKNOWN };
        public enum Sensor { Color, Depth, IR, Fisheye};
        public enum SourceGroupType { DEPTH, COLOR, FISHEYE, SHARED, UNKNOWN};
        public enum Formats
        {
            UNDEFINED,

            //Depth
            Z,
            Y,
            ZY,            
            L8R8,
            UYVY,
            CALIBRATION,

            //Color
            RGB24,
            YUY2,

            //FishEye
            RAW8,

            //IMU - Motion 
            CUSTOM,
            GYRO,
            ACCELEROMETER,

            //Randomized
            RANDOM
        };
        public struct FrameData
        {
            public bool isTestDone;
            public int testsDoneCnt;
            public Sensors sensorSource;
            public Formats format;
            public string resolution;
            public int frameRate;
            public int FrameId;
            public int mmCounter;
            public int usbCounter;
            public string sw_timeStamp;
            public string hw_timeStamp;
            public double x, y, z;
            public byte[] ActualData;
            public bool isDepthControlsMDAvailable;
            public bool isColorControlsMDAvailable;
            public bool isFEControlsMDAvailable;
            public int exposurePriority;
            public double ActualExposure;
            public int AutoExposure;
            public double GainLevel;

            public double Brightness;
            public double Contrast;
            public double Saturation;
            public double Sharpness;
            public double BacklightComp;
            public double Gamma;
            public double Hue;
            public double WhiteBalance;
            public double PowerLineFrequency;
            public double LowLightComp;
        }
        public struct SensorInfo
        {
            public string DisplayName;
            public string FullID;
            public string ID;
            public string PID;
            public string VID;
            public SourceGroupType SensorType;
        }
        public static string GetControlName(Controls control)
        {
            switch(control)
            {
                case Controls.AutoExposure:     return "AutoExposure";
                case Controls.Brightness:       return "Brightness";
                case Controls.Contrast:         return "Contrast";
                case Controls.DepthAE:          return "DepthAutoExposure";
                case Controls.DepthExposure:    return "DepthExposure";
                case Controls.Exposure:         return "Exposure";
                case Controls.ExposurePriority: return "ExposurePriority";
                case Controls.LaserPower:       return "LaserPower";
                case Controls.LaserPowerOnOff:  return "LaserPowerOnOff";
                case Controls.WhiteBalance:     return "WhiteBalance";
                default:                        return "Unknown";
            }
        }
        public static ControlName GetControlFromName(string controlName)
        {
            switch (controlName)
            {
                case "AutoExposure":        return ControlName.COLOR_AE;
                case "DepthAutoExposure":   return ControlName.DEPTH_AE;
                case "DepthExposure":       return ControlName.DEPTH_EXPOSURE;
                case "Exposure":            return ControlName.COLOR_EXPOSURE;
                case "ExposurePriority":    return ControlName.COLOR_EXP_PRIORITY;
                default:                    return ControlName.UNKNOWN;
            }
        }
        public static ControlType GetControlType(string controlName)
        {
           switch(controlName.ToLower())
            {
                case "exposure":
                case "autoexposure":
                case "brightness":
                case "whitebalance":    return ControlType.STANDARD;

                case "depthexposure":
                case "laserpower":
                case "laserpoweronoff": return ControlType.XU;

                default:                return ControlType.UNKNOWN;
            }
        }
        public static Types.Formats GetFormatFromMediaFrameFormat(MediaFrameFormat mFF)
        {
            if (mFF.Subtype.Equals("{38574152-1A66-A242-9065-D01814A8EF8A}")) return Types.Formats.RAW8;
            if (mFF.Subtype.Equals("{49323159-0000-0010-8000-00AA00389B71}")) return Types.Formats.CALIBRATION;
            //if (mFF.Subtype.Equals("{59565955-0000-0010-8000-00AA00389B71}")) return Types.Formats.UYVY;
            if (mFF.Subtype.Equals("NV12")) return Types.Formats.UYVY;
            if (mFF.Subtype.Equals("{20493859-0000-0010-8000-00AA00389B71}")) return Types.Formats.L8R8;
            if (mFF.Subtype.ToLower().Equals("l8")) return Types.Formats.Y;
            if (mFF.Subtype.ToLower().Equals("d16")) return Types.Formats.Z;
            if (mFF.Subtype.ToLower().Equals("yuy2")) return Types.Formats.YUY2;
            return Types.Formats.UNDEFINED;
        }
        public static Types.Sensors GetSensorTypeFromFormat(Types.Formats fmt)
        {
            switch (fmt)
            {
                case Types.Formats.RAW8: return Types.Sensors.FISHEYE;
                case Types.Formats.Z: return Types.Sensors.DEPTH;

                case Types.Formats.CALIBRATION:
                case Types.Formats.Y:
                case Types.Formats.UYVY: return Types.Sensors.IR;

                case Types.Formats.YUY2:
                case Types.Formats.RGB24: return Types.Sensors.COLOR;

                default: return Types.Sensors.UNKNOWN;
            }
        }
        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }
        public static string ExtractSensorEndPointFromID(string fullId)
        {
            var parts = fullId.Split('&');
            if (parts == null || parts.Length < 6)
                return "Failed to get Sensor Endpoint";
            return parts[5].Split('#')[0];
        }

        public static Types.FrameData ExtractFrameData(MediaFrameReader sender)
        {
            var frame = sender.TryAcquireLatestFrame();
            Types.FrameData tempData = new Types.FrameData();

            Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING  intelCaptureTimingMD =  new Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING();
            Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_DEPTH_CONTROL   intelDepthControlMD =   new Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_DEPTH_CONTROL();
            Types.REALSENSE_SAMPLE_RS400_COLOR_CONTROL_METADATA         intelColorControlMD =   new Types.REALSENSE_SAMPLE_RS400_COLOR_CONTROL_METADATA();
            UInt32 HwTimeStamp = 0;
            Object temp;
            Object intelCaptureTimingMDObject;
            var properties = frame.Properties;
            

            // **************************************     Try getting Intel Capture Timing data  **************************************** 
            try
            {
                //var intelCaptureTimingMDBytes = properties.Where(x => x.Key == Types.DS5_INTEL_CAPTURE_TIMING_GUID).First().Value;
                //intelCaptureTimingMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING>((byte[])intelCaptureTimingMDBytes);

                if (properties.TryGetValue(Types.DS5_INTEL_CAPTURE_TIMING_GUID, out intelCaptureTimingMDObject) == false)
                    throw new Exception("Getting IntelCaptureTiming Failed");
                intelCaptureTimingMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING>((byte[])intelCaptureTimingMDObject);
            }
            catch(Exception ex)
            {
                Logger.Error("Getting intelCaptureTimingMDBytes Failed on: " + ex.Message);
                
                //Filling Fake fields into the IntelCaptureTiming struct
                intelCaptureTimingMD.frameCounter = -1;
            }


            // ********************************************* Try getting Frame HW Timestamp ********************************************* 
            try
            {
                properties.TryGetValue(Types.DS5_HW_TIME_STAMP_GUID, out temp);
                HwTimeStamp = (UInt32)temp;
            }
            catch
            {
                HwTimeStamp = 0;
            }

            // **************************************     Try getting Intel Depth Control data  **************************************** 
            try
            {
                tempData.isDepthControlsMDAvailable = false;
                if (properties.TryGetValue(Types.DS5_INTEL_DEPTH_CONTROL_GUID, out temp))
                {
                    intelDepthControlMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_DEPTH_CONTROL>((byte[])temp);
                    tempData.isDepthControlsMDAvailable = true;
                }
            }
            catch
            {
                Logger.Error("Failed On: Try getting Intel Depth Control data");
            }

            // **************************************     Try getting Intel Color Control data  **************************************** 
            try
            {
                tempData.isColorControlsMDAvailable = false;
                if (properties.TryGetValue(Types.DS5_INTEL_COLOR_CONTROL_GUID, out temp))
                {
                    intelColorControlMD = Types.ByteArrayToStructure<Types.REALSENSE_SAMPLE_RS400_COLOR_CONTROL_METADATA>((byte[])temp);
                    tempData.isColorControlsMDAvailable = true;
                }
            }
            catch
            {
                Logger.Error("Failed On: Try getting Intel Color Control data");
            }

            var systemTimeStamp = frame?.SystemRelativeTime.Value.TotalMilliseconds;
            var fmt = Types.GetFormatFromMediaFrameFormat(frame.Format);
            var sensor = Types.GetSensorTypeFromFormat(fmt);
            var reso = string.Format("{0}X{1}", frame.Format.VideoFormat.Width, frame.Format.VideoFormat.Height);
            var fps = (int)(frame.Format.FrameRate.Numerator / Convert.ToDouble(frame.Format.FrameRate.Denominator));
            //var frameCnt = IncFrameCounter(fmt);

            tempData.FrameId = (int)intelCaptureTimingMD.frameCounter;
            tempData.sensorSource = sensor;
            tempData.format = fmt;
            tempData.resolution = reso;
            tempData.frameRate = fps;
            tempData.sw_timeStamp = string.Format("{0}", systemTimeStamp);
            tempData.hw_timeStamp = string.Format("{0}", HwTimeStamp);        

            if(tempData.isDepthControlsMDAvailable)
            {
                tempData.ActualExposure =       (int)intelDepthControlMD.manualExposure;
                tempData.GainLevel =            (int)intelDepthControlMD.gainLevel;
                tempData.AutoExposure =         (int)intelDepthControlMD.autoExposureMode;
                tempData.exposurePriority =     (int)intelDepthControlMD.exposurePriority;
            }
            else if(tempData.isColorControlsMDAvailable)
            {
                tempData.ActualExposure =       (int)intelColorControlMD.manual_Exp;
                tempData.GainLevel =            (int)intelColorControlMD.gain;
                tempData.AutoExposure =         (int)intelColorControlMD.auto_Exp_Mode == 8 ? 1 : 0;
                tempData.Brightness =           (int)intelColorControlMD.brightness;
                tempData.Contrast =             (int)intelColorControlMD.contrast;
                tempData.Saturation =           (int)intelColorControlMD.saturation;
                tempData.Sharpness =            (int)intelColorControlMD.sharpness;
                tempData.BacklightComp =        (int)intelColorControlMD.backlight_Comp;
                tempData.Gamma =                (int)intelColorControlMD.gamma;
                tempData.Hue =                  (int)intelColorControlMD.hue;
                tempData.PowerLineFrequency =   (int)intelColorControlMD.powerLineFrequncy;
                tempData.LowLightComp =         (int)intelColorControlMD.low_Light_Comp;
            }
            tempData.ActualData = null;//frame.BufferMediaFrame.Buffer.ToArray();

            frame.Dispose();
            return tempData;
        }


    }
}
