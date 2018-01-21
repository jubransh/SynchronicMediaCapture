﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace SynchronicMediaCapture
{
    public static class Types
    {
        public struct REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME
        {
            public UInt32 version;
            public UInt32 flag;
            public UInt32 frameCounter;
            public UInt32 opticalTimestamp;   //In millisecond unit
            public UInt32 readoutTime;        //The readout time in millisecond second unit
            public UInt32 exposureTime;       //The exposure time in millisecond second unit
            public UInt32 frameInterval;     //The frame interval in millisecond second unit
            public UInt32 pipeLatency;        //The latency between start of frame to frame ready in USB buffer
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
            CALIBRATION,
            L8R8,
            UYVY,

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
            public double ActualExposure;
            public double GainLevel;
            public double WhiteBalance;
            public string sw_timeStamp;
            public string hw_timeStamp;
            public double x, y, z;
            public byte[] ActualData;
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
            if (mFF.Subtype.Equals("{59565955-0000-0010-8000-00AA00389B71}")) return Types.Formats.UYVY;
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

    }
}
