using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Devices;
using Winusb.Cli;

namespace SynchronicMediaCapture
{
    public class CameraProperties
    {        
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
        VideoDeviceController _vC_Depth, _vC_Color, _vC_Fisheye;
        public CameraProperties(VideoDeviceController depth_VC, VideoDeviceController color_VC, VideoDeviceController fisheye_VC)
        {
            Logger.Debug("CameraProperty Created with Color and Depth VideoDeviceController");
            _vC_Depth = depth_VC;
            _vC_Color = color_VC;
            _vC_Fisheye = fisheye_VC;
        }
        public CameraProperties(VideoDeviceController vC)
        {
            Logger.Debug("CameraProperty Created with Color VideoDeviceController Only");
            _vC_Depth = vC;
            _vC_Color = vC;
            _vC_Fisheye = vC;
        }
        public CameraProperties(Types.Sensor sensorType, VideoDeviceController vC)
        {
            Logger.Debug(string.Format("CameraProperty Created with {0} VideoDeviceController Only", sensorType));

            _vC_Depth = null;
            _vC_Color = null;
            _vC_Fisheye = null;

            switch(sensorType)
            {
                case Types.Sensor.IR:
                case Types.Sensor.Depth:    _vC_Depth = vC; break;

                case Types.Sensor.Color:    _vC_Color = vC; break;

                case Types.Sensor.Fisheye:  _vC_Fisheye = vC; break;

                default: throw new Exception("CameraProperties constructor failed on: Unknown Sensor Type");
            }

        }


        //================== Private methods XU Controls ================== 
        private double ObjectToDouble(object obj)
        {
            //convert the object to byte array
            byte[] arr = (byte[])obj;

            //convert the byte array according to it length
            switch (arr.Length)
            {
                case 1: return (int)arr[0];
                case 2: return BitConverter.ToInt16(arr, 0);
                case 4: return BitConverter.ToInt32(arr, 0);
                case 8: return BitConverter.ToInt64(arr, 0);
                default: throw new Exception("Failed Converting byte array to double");
            }
        }
        private bool SetXUProperty(XU_Controls xuControl, int value)
        {
            var ControlId = (int)xuControl;
            try
            {
                //parse double value to Little Endians bytes array
                var dataToSend = INT2LE(value);

                //set data to the xu control
                _vC_Depth.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId), BitConverter.GetBytes((value)));
                Logger.Debug("Setting Control to value " + value + " Succeded");
                return true;
            }
            catch (Exception e)
            {
                var err = "Setting Control Failed On: " + e.Message;
                Logger.Error(err);
                throw new Exception(err);
            }
        }
        private double GetXUProperty(XU_Controls xuControl)
        {
            var ControlId = (int)xuControl;
            double dVal;
            try
            {
                object val = _vC_Depth.GetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId));
                dVal = ObjectToDouble(val);
                //QaLogger.WriteLine("Get Control Returns: " + val);
                return dVal;
            }
            catch (Exception e)
            {
                var err = "Getting Control Failed On: " + e.Message;
                Logger.Error(err);
                throw new Exception(err);
            }
        }

        //================== Private methods UVC Controls ================== 
        private bool SetManualUvcProperty(MediaDeviceControl control, double value, string controlName = "")
        {
            //if control supports auto
            if (control.Capabilities.AutoModeSupported)
            {
                bool isAuto;
                //check current mode
                if (control.TryGetAuto(out isAuto))
                {
                    //QaLogger.WriteLine(string.Format("{0} Property is in Auto Mode currently", controlName));
                    //if current mode is auto, then set it to manual before setting manual value
                    if (isAuto)
                    {
                        if (control.TrySetAuto(false))
                        {
                            return true;
                        }
                        else
                        {
                            //QaLogger.WriteLine(string.Format("Failed To Set {0} AutoProperty to Manual", controlName, Types.LogLevel.ERROR));
                            return false;
                        }
                    }//End Of is Auto
                }
            }//End Of is Auto Capabile

            //set manual value
            if (control.TrySetValue(value))
            {
                //QaLogger.WriteLine(string.Format("{0} Property set to {1} successfully", controlName, value), Types.LogLevel.DEBUG);
                return true;
            }

            return false;
        }
        private double GetManualUvcProperty(MediaDeviceControl control, string controlName = "")
        {
            try
            {
                double value;
                if (control.TryGetValue(out value))
                {
                    return value;
                }
                throw new Exception(string.Format("Getting {1} Property Failed", controlName));
            }
            catch (Exception e)
            {
                //QaLogger.WriteLine(string.Format("Getting {0} Property Failed on: {1}", controlName, e.Message));
                throw new Exception(string.Format("Getting {0} Property Failed on: {1}", controlName, e.Message));
            }
        }
        private bool GetAutoUvcControl(MediaDeviceControl control, string controlName = "")
        {
            bool returnedVal;
            if (control.TryGetAuto(out returnedVal))
            {
                return returnedVal;
            }
            throw new Exception("Getting Auto UVC control Failed");
        }

        private bool SetAutoUvcControl(MediaDeviceControl control, bool isAuto, string controlName = "")
        {            
            if (control.TrySetAuto(isAuto))
            {
                //QaLogger.WriteLine(string.Format("{0} Property set to {1} Mode Successfully", controlName, isAuto ? "Auto" : "Manual"), Types.LogLevel.DEBUG);
                return true;
            }
           // QaLogger.WriteLine(string.Format("setting {0} Property to {1} Mode was Failed", controlName, isAuto ? "Auto" : "Manual"), Types.LogLevel.ERROR);
            return false;
        }

        //================== Private methods - General ================== 
        byte[] INT2LE(int data)
        {
            byte[] b = new byte[4];
            b[0] = (byte)data;
            b[1] = (byte)(((uint)data >> 8) & 0xFF);
            b[2] = (byte)(((uint)data >> 16) & 0xFF);
            b[3] = (byte)(((uint)data >> 24) & 0xFF);
            return b;
        }
        byte[] INT2LE(int data, int numberOfBytes)
        {
            byte[] b = new byte[numberOfBytes];
            b[0] = (byte)data;

            for (int i = 1; i < numberOfBytes; i++)
            {
                b[i] = (byte)(((uint)data >> 8 * i) & 0xFF);

            }
            return b;
        }
        int GetOpCode(string command)
        {
            switch(command.ToLower())
            {
                case "pfd": return 0x3b;
                case "gvd": return 0x10;
                case "rst": return 0x20;
                default: return 0;
            }
        }
        //================== Public Methods - Depth Controls ================== 
        public bool SetDepthAutoExposure(int val)
        {
            return SetXUProperty(XU_Controls.DepthAE, val);
        }
        public double GetDepthAE()
        {
            return GetXUProperty(XU_Controls.DepthAE);
        }
        public bool SetDepthExposure(int value)
        {
            return SetXUProperty(XU_Controls.DepthExposure, value);
        }
        public double GetDepthExposure()
        {
            return GetXUProperty(XU_Controls.DepthExposure);
        }
        public bool SetLaserPowerMode(int val)
        {
            return SetXUProperty(XU_Controls.LaserPowerMode, val);
        }
        public double GetLaserPowerMode()
        {
            return GetXUProperty(XU_Controls.LaserPowerMode);
        }
        public bool SetManualLaserPower(int value)
        {
            return SetXUProperty(XU_Controls.ManualLaserPower, value);
        }
        public double GetManualLaserPower()
        {
            return GetXUProperty(XU_Controls.ManualLaserPower);
        }
        public bool SetDepthGain(int value)
        {
            return SetControl(Types.GenericControl.GAIN, Types.Sensor.Depth, value);
        }

        //================== Public Methods - Color Controls ================== 
        public bool GetColorAutoExposure()
        {
            return GetAutoUvcControl(_vC_Color.Exposure, "AutoExposure");
        }
        public bool SetColorAutoExposure(bool isAuto)
        {
            return SetAutoUvcControl(_vC_Color.Exposure, isAuto, "AutoExposure");
        }
        public XUCommandRes SendCommand(string command, int[] args)
        {
            Logger.Debug(string.Format("Starting SendCommand({0}) Type = {1}",
                command, _vC_Depth == null ? "Null" :  "Not Null"));
            bool isCompletedOk;
            try
            {
                //get and prepare size
                int commandSize = 0x14;//2 + 2 + 4 + (args != null ? args.Length * 4 : 0); // size + magic number + op code + params size
                var sizeAsBytes = INT2LE(commandSize, 2);

                //prepare magic number
                int magicNumber = 0xcdab;
                var magicNumberAsBytes = INT2LE(magicNumber, 2);

                //prepare command op code
                int commandOpCode = GetOpCode(command);
                var opCodeAsBytes = INT2LE(commandOpCode, 4);

                //prepare commands params
                List<byte[]> commandParams = new List<byte[]>();
                commandParams.Add(new byte[] { 0, 0, 0, 0 });
                commandParams.Add(new byte[] { 0, 0, 0, 0 });
                commandParams.Add(new byte[] { 0, 0, 0, 0 });
                commandParams.Add(new byte[] { 0, 0, 0, 0 });

                if (args != null)
                {
                    for (int i = 0; i < args.Length; i++)
                    {
                        commandParams[i] = INT2LE(args[i], 4);
                    }
                }

                //prepare 1024 byte array and order all the above items (size, magicNum, opcode and params) into this array
                byte[] bufferToSend = new byte[1024];

                Array.Copy(sizeAsBytes, 0, bufferToSend, 0, 2);         // copy the size to the bufferToSend
                Array.Copy(magicNumberAsBytes, 0, bufferToSend, 2, 2);  // copy the magic number to the bufferToSend
                Array.Copy(opCodeAsBytes, 0, bufferToSend, 4, 4);       // copy the opcode to the bufferToSend
                Array.Copy(commandParams[0], 0, bufferToSend, 8, 4);    // copy the param1 to the bufferToSend
                Array.Copy(commandParams[1], 0, bufferToSend, 12, 4);   // copy the param2 to the bufferToSend
                Array.Copy(commandParams[2], 0, bufferToSend, 16, 4);   // copy the param3 to the bufferToSend
                Array.Copy(commandParams[3], 0, bufferToSend, 20, 4);   // copy the param4 to the bufferToSend

                //Send xu control 
                var ControlId = (int)XU_Controls.DS5_HWMONITOR;
                _vC_Depth.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId), bufferToSend);

                //get result from sent command
                var res = _vC_Depth.GetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId));

                //temporary
                Array bigEndiansFwVer = new byte[4];
                Array bigEndiansOpticalSerial = new byte[6];
                Array bigEndiansAsicSerial = new byte[6];

                Array.Copy((byte[])res, 16, bigEndiansFwVer, 0, 4);
                Array.Copy((byte[])res, 52, bigEndiansOpticalSerial, 0, 6);
                Array.Copy((byte[])res, 68, bigEndiansAsicSerial, 0, 6);

                //reverse the endianity 
                Array.Reverse(bigEndiansFwVer);
                //Array.Reverse(bigEndiansSerial);

                string resStr = string.Format("FunctionalPayloadVersion: {0}\nAsicModuleSerial: {1}\nOpticModuleSerial: {2}",
                    string.Join(" ", ((byte[])bigEndiansFwVer).Select(b => b.ToString())).Replace(" ","."),
                    BitConverter.ToString((byte[])bigEndiansAsicSerial).Replace("-", ""),
                    BitConverter.ToString((byte[])bigEndiansOpticalSerial).Replace("-","")

                    //BitConverter.ToString((byte[])bigEndiansFwVer),
                    //BitConverter.ToString((byte[])bigEndiansSerial)
                    );

                //take the first 2 byte of the get to know if command passed
                var receivedBuffer = (byte[])res;
                if(receivedBuffer.Length >= 2)
                {
                    int first2Bytes = BitConverter.ToInt16(receivedBuffer, 0);
                    isCompletedOk = first2Bytes == commandOpCode ? true : false;
                }
                else
                {
                    isCompletedOk = false;
                }

                return new XUCommandRes(isCompletedOk, resStr, (byte[])res);
            }
            catch (Exception ex)
            {
                //write to log                 
                return new XUCommandRes(false, "", null);
            }
        }
        public XUCommandRes SendCommand(string commandBytesString)
        {
            try
            {
                byte[] bufferToSend;
                var bytesStrings = commandBytesString.Split(' ');
                bufferToSend = new byte[bytesStrings.Length];
                for(int i=0; i< bufferToSend.Length; i++)                
                {
                    bufferToSend[i] = Convert.ToByte(bytesStrings[i], 16);
                }
               
                byte[] largeArray = new byte[1024];
                Array.Copy(bufferToSend, 0, largeArray, 0, bufferToSend.Length);

                //Send xu control 
                var ControlId = (int)XU_Controls.DS5_HWMONITOR;
                _vC_Depth.SetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId), largeArray);

                //get result from sent command
                var res = _vC_Depth.GetDeviceProperty(string.Format("{0} {1}", Ds5_XU_GUID, ControlId));
                var resArray = (byte[])res;

                //read the buffer size from the end of the large buffer
                var size = new byte[4];
                Array.Copy(resArray, resArray.Length - 1 - 4, size, 0, 4);
                Array.Reverse(size);
                int arraySize = BitConverter.ToInt32(size, 0);

                byte[] data = new byte[arraySize];
                Array.Copy(resArray, 4, data, 0, data.Length);
                return new XUCommandRes(true, "", data);
            }
            catch (Exception ex)
            {
                //write to log                 
                return new XUCommandRes(false, "", null);
            }
        }


        public bool SetControl(Types.GenericControl control, Types.Sensor sensorType, int value)
        {
            Types.Control specControl = ConvertGenericControlToSpec(control, sensorType);
            switch (specControl)
            {
                //Color Control
                case Types.Control.COLOR_EXPOSURE:
                    return SetMFControl(_vC_Color, CameraControl_ControlValues.EXPOSURE, value);

                case Types.Control.GAIN:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.GAIN, value);

                case Types.Control.BACKLIGHT_COMPENSATION:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.BACKLIGHT_COMPENSATION, value);

                case Types.Control.BRIGHTNESS:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.BRIGHTNESS, value);

                case Types.Control.CONTRAST:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.CONTRAST, value);

                case Types.Control.GAMMA:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.GAMMA, value);

                case Types.Control.HUE:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.HUE, value);

                case Types.Control.SATURATION:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.SATURATION, value);

                case Types.Control.SHARPNESS:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.SHARPNESS, value);

                case Types.Control.WHITEBALANCE:
                    return SetMFControl(_vC_Color, VideoProcAmp_ControlValues.WHITEBALANCE, value);

                case Types.Control.COLOR_AE:
                    return SetColorAutoExposure((value == 0) ? false : true);

                case Types.Control.AUTO_EXPOSURE_PRIORITY:
                    return SetMFControl(_vC_Color, CameraControl_ControlValues.AUTO_EXPOSURE_PRIORITY, value);
                
                //Depth Controls
                case Types.Control.DEPTH_AE:
                    return SetDepthAutoExposure(value);

                case Types.Control.DEPTH_EXPOSURE:
                    {
                        if (SetDepthAutoExposure(0))
                            return SetDepthExposure(value);
                        else
                            return false;
                    }
                case Types.Control.LASER_POWER_MODE:
                    return SetXUProperty(XU_Controls.LaserPowerMode, value);

                case Types.Control.MANUAL_LASER_POWER:
                    return SetXUProperty(XU_Controls.ManualLaserPower, value);

                case Types.Control.DEPTH_GAIN:
                    return SetMFControl(_vC_Depth, VideoProcAmp_ControlValues.GAIN, value);

                default:
                    return false;
            }
        }
        public double GetControl(Types.GenericControl control, Types.Sensor sensorType)
        {
            Types.Control specControl = ConvertGenericControlToSpec(control, sensorType);
            switch (specControl)
            {
                //Color Control               
                case Types.Control.COLOR_AE:
                    return GetColorAutoExposure()? 1:0;

                case Types.Control.COLOR_EXPOSURE:
                    return GetMFControlValue(_vC_Color, CameraControl_ControlValues.EXPOSURE);

                case Types.Control.GAIN:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.GAIN);

                case Types.Control.BACKLIGHT_COMPENSATION:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.BACKLIGHT_COMPENSATION);

                case Types.Control.BRIGHTNESS:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.BRIGHTNESS);

                case Types.Control.CONTRAST:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.CONTRAST);

                case Types.Control.GAMMA:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.GAMMA);

                case Types.Control.HUE:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.HUE);

                case Types.Control.SATURATION:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.SATURATION);

                case Types.Control.SHARPNESS:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.SHARPNESS);

                case Types.Control.WHITEBALANCE:
                    return GetMFControlValue(_vC_Color, VideoProcAmp_ControlValues.WHITEBALANCE);

                case Types.Control.AUTO_EXPOSURE_PRIORITY:
                    return GetMFControlValue(_vC_Color, CameraControl_ControlValues.AUTO_EXPOSURE_PRIORITY);

                //Depth Controls
                case Types.Control.DEPTH_AE:
                    return GetDepthAE();

                case Types.Control.DEPTH_EXPOSURE:
                    return GetDepthExposure();

                case Types.Control.LASER_POWER_MODE:
                    return GetXUProperty(XU_Controls.LaserPowerMode);

                case Types.Control.MANUAL_LASER_POWER:
                    return GetXUProperty(XU_Controls.ManualLaserPower);

                case Types.Control.DEPTH_GAIN:
                    return GetMFControlValue(_vC_Depth, VideoProcAmp_ControlValues.GAIN);

                default:
                    throw new Exception("Failed Getting the value of the folowing control: " + control.ToString());
            }
        }

        //==========================================================================

        private bool SetMFControl(VideoDeviceController vC, VideoProcAmp_ControlValues control, int value)
        {
            try
            {
                if (vC == null)
                    throw new Exception("Color VideoDeviceController is null");

                VideoProcAmp prop = new VideoProcAmp((uint)control, vC);
                var propValue = prop.Get();
                var newValue = value;
                //FW will return an error if you set the same value twice 

                bool res = true;
                if (propValue != newValue)
                {
                    res = prop.Set(newValue);
                    Logger.Debug("SetMFControl " + (res ? "Passed" : "Failed"));
                    return res;
                }
                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Setting {0} Control Failed On: {1}", control.ToString(), ex.Message);
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }
        private bool SetMFControl(VideoDeviceController vC, CameraControl_ControlValues control, int newValue)
        {
            try
            {
                if (vC == null)
                    throw new Exception("Color VideoDeviceController is null");

                CameraControl cameraControl = new CameraControl((uint)control, vC);
                var orig = cameraControl.Get();

                //FW will return an error if you set the same value twice 
                if (orig != newValue)
                {
                    var res = cameraControl.Set(newValue);
                    Logger.Debug("SetMFControl " + (res ? "Passed" : "Failed"));
                    return res;
                }
                return true;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Setting {0} Control Failed On: {1}", control.ToString(), ex.Message);
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }
        private double GetMFControlValue(VideoDeviceController vC, CameraControl_ControlValues control)
        {
            try
            {
                if (vC == null)
                    throw new Exception("Color VideoDeviceController is null");

                CameraControl exposurePriority = new CameraControl((uint)control, vC);
                return exposurePriority.Get();
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Getting {0} Control Failed On: {1}", control.ToString(), ex.Message);
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }
        private double GetMFControlValue(VideoDeviceController vC, VideoProcAmp_ControlValues control)
        {
            try
            {
                if (vC == null)
                {
                    Logger.Debug("Color VideoDeviceController is null");
                    throw new Exception("Color VideoDeviceController is null");
                }

                VideoProcAmp mf_control = new VideoProcAmp((uint)control, vC);
                double val = mf_control.Get();
                Logger.Debug("returned control value is: " + val);
                return val;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Getting {0} Control Failed On: {1}", control.ToString(), ex.Message);
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
        }

        private Types.Control ConvertGenericControlToSpec(Types.GenericControl gC, Types.Sensor sensor)
        {
            switch(gC)
            {
                case Types.GenericControl.EXPOSURE:
                    {
                        switch (sensor)
                        {
                            case Types.Sensor.IR:
                            case Types.Sensor.Depth:    return Types.Control.DEPTH_EXPOSURE;
                            case Types.Sensor.Color:    return Types.Control.COLOR_EXPOSURE;
                            default:                    return Types.Control.UNKNOWN;
                        }
                    }
                case Types.GenericControl.GAIN:
                    {
                        switch (sensor)
                        {
                            case Types.Sensor.IR:
                            case Types.Sensor.Depth: return Types.Control.DEPTH_GAIN;
                            case Types.Sensor.Color: return Types.Control.GAIN;
                            default: return Types.Control.UNKNOWN;
                        }
                    }
                case Types.GenericControl.AUTO_EXPOSURE:
                    {
                        switch (sensor)
                        {
                            case Types.Sensor.IR:
                            case Types.Sensor.Depth: return Types.Control.DEPTH_AE;
                            case Types.Sensor.Color: return Types.Control.COLOR_AE;
                            default: return Types.Control.UNKNOWN;
                        }
                    }
                case Types.GenericControl.BRIGHTNESS: return Types.Control.BRIGHTNESS;
                case Types.GenericControl.CONTRAST: return Types.Control.CONTRAST;
                case Types.GenericControl.HUE: return Types.Control.HUE;
                case Types.GenericControl.SATURATION: return Types.Control.SATURATION;
                case Types.GenericControl.SHARPNESS: return Types.Control.SHARPNESS;
                case Types.GenericControl.GAMMA: return Types.Control.GAMMA;
                case Types.GenericControl.COLORENABLE: return Types.Control.COLORENABLE;
                case Types.GenericControl.WHITEBALANCE: return Types.Control.WHITEBALANCE;
                case Types.GenericControl.BACKLIGHT_COMPENSATION: return Types.Control.BACKLIGHT_COMPENSATION;
                case Types.GenericControl.DIGITAL_MULTIPLIER: return Types.Control.DIGITAL_MULTIPLIER;
                case Types.GenericControl.DIGITAL_MULTIPLIER_LIMIT: return Types.Control.DIGITAL_MULTIPLIER_LIMIT;
                case Types.GenericControl.WHITEBALANCE_COMPONENT: return Types.Control.WHITEBALANCE_COMPONENT;
                case Types.GenericControl.POWERLINE_FREQUENCY: return Types.Control.POWERLINE_FREQUENCY;
                case Types.GenericControl.PAN: return Types.Control.PAN;
                case Types.GenericControl.TILT: return Types.Control.TILT;
                case Types.GenericControl.ROLL: return Types.Control.ROLL;
                case Types.GenericControl.ZOOM: return Types.Control.ZOOM;
                case Types.GenericControl.IRIS: return Types.Control.IRIS;
                case Types.GenericControl.FOCUS: return Types.Control.FOCUS;
                case Types.GenericControl.SCANMODE: return Types.Control.SCANMODE;
                case Types.GenericControl.PRIVACY: return Types.Control.PRIVACY;
                case Types.GenericControl.PANTILT: return Types.Control.PANTILT;
                case Types.GenericControl.PAN_RELATIVE: return Types.Control.PAN_RELATIVE;
                case Types.GenericControl.TILT_RELATIVE: return Types.Control.TILT_RELATIVE;
                case Types.GenericControl.ROLL_RELATIVE: return Types.Control.ROLL_RELATIVE;
                case Types.GenericControl.ZOOM_RELATIVE: return Types.Control.ZOOM_RELATIVE;
                case Types.GenericControl.EXPOSURE_RELATIVE: return Types.Control.EXPOSURE_RELATIVE;
                case Types.GenericControl.IRIS_RELATIVE: return Types.Control.IRIS_RELATIVE;
                case Types.GenericControl.FOCUS_RELATIVE: return Types.Control.FOCUS_RELATIVE;
                case Types.GenericControl.PANTILT_RELATIVE: return Types.Control.PANTILT_RELATIVE;
                case Types.GenericControl.FOCAL_LENGTH: return Types.Control.FOCAL_LENGTH;
                case Types.GenericControl.AUTO_EXPOSURE_PRIORITY: return Types.Control.AUTO_EXPOSURE_PRIORITY;
                case Types.GenericControl.MANUAL_LASER_POWER: return Types.Control.MANUAL_LASER_POWER;
                case Types.GenericControl.LASER_POWER_MODE: return Types.Control.LASER_POWER_MODE;
                case Types.GenericControl.UNKNOWN: return Types.Control.UNKNOWN;

                default:
                    return Types.Control.UNKNOWN;
            }
        }
    }
}

