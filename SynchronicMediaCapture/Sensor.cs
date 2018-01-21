using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;

namespace SynchronicMediaCapture
{
    public class Sensor
    {
        //public members
        public Types.Sensor SensorType { get; private set; }
        public MediaFrameReader FrameReader;//{ get; private set; }
        public string DisplayName { get; private set; }
        public string FullID { get; private set; }
        public string ID { get; private set; }
        //Private members
        private MediaCapture _mC;
        private MediaFrameSourceGroup _sG;
        private MediaFrameSourceGroup _privateSG;
        private MediaCaptureInitializationSettings _mediaCaptureSettings;
        private bool _inited;
        private bool _isConfigured;
        private bool _activeStream;
        private CameraProperties _cP;

        //Python Events
        Delegate pythonFunc;
        object[] paramsToPass;

        //================================================================== Constructor ========================================================================
        public Sensor(MediaCapture mC, Types.Sensor type, MediaFrameSourceGroup sG, MediaFrameSourceGroup privateSG)
        {
            _inited = false;
            _isConfigured = false;
            _activeStream = false;

            SensorType = type;
            _mC = mC;
            _sG = sG;

            _privateSG = privateSG;
            DisplayName = privateSG.DisplayName;
            FullID = privateSG.Id;
            ID = GetSourceInfoID(this.SensorType, _sG);////Types.ExtractSensorEndPointFromID(FullID);
        }
        //=======================================================================================================================================================
        //================================================================= Public methods ======================================================================
        public void Init()
        {
            if (_sG == null)
                throw new Exception("MediaFrameSourceGroup is Null");

            if (_mC == null)
                throw new Exception("MediaCapture is NULL, try to run the method Device.InitSensors() first");

            //create camera property object
            _cP = GetCameraProperty(SensorType);
            
            Logger.Debug(string.Format("Initializing {0} MediaCapture object passed", SensorType));
            _inited = true;
        }
        public void Configure(StreamConfiguration sC)
        {
            string errorMessage;
            MediaFrameSource mFS;
            MediaFrameFormat mFF;
            MediaFrameSourceKind mFSK = MediaFrameSourceKind.Depth;//Default

            if (!_inited)
            {
                errorMessage = "Operation is not allowed - can't Configure Stream, need to init sensor first by running Sensor.Init()";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            Task.Run(async () =>
            {
                uint numerator, denominator;
                if (ParseFrameRate(sC.FrameRate, sC.Format, (uint)sC.Width, (uint)sC.Height, out numerator, out denominator) == false)
                {
                    errorMessage = "Failed to parse some " + sC.FrameRate + " Rate";
                    Logger.Error(errorMessage);
                    throw new Exception(errorMessage);
                }
          
                switch (SensorType)
                {
                    case Types.Sensor.Color: //=========================== Color ===========================
                        {
                            Logger.Debug("Getting Media Frame Source Kind Of Color Sensor");
                            mFSK = GetMediaFrameSourceKind(Types.Sensors.COLOR);
                            break;                     
                        }
                        
                    case Types.Sensor.Depth: //=========================== Depth ===========================
                        {
                            Logger.Debug("Getting Media Frame Source Kind Of Depth Sensor");
                            mFSK = GetMediaFrameSourceKind(Types.Sensors.DEPTH);
                            break;
                        }

                    case Types.Sensor.IR: //=========================== IR ===========================
                        {
                            Logger.Debug("Getting Media Frame Source Kind Of IR Sensor");
                            mFSK = GetMediaFrameSourceKind(Types.Sensors.IR);
                            break;
                        }

                    case Types.Sensor.Fisheye: //=========================== Fisheye ===========================
                        {
                            Logger.Debug("Getting Media Frame Source Kind Of Fisheye Sensor");
                            var sourceKind = GetMediaFrameSourceKind(Types.Sensors.FISHEYE);
                            break;
                        }

                    default: break;
                }

                Logger.Debug("Trying GetMediaSourceInfoFromGroup() when source group is: " + _sG.DisplayName);                
                GetMediaSourceInfoFromGroup(_mC, _sG, mFSK, out mFS);

                //Get Media Frame Format 
                Logger.Debug("Trying to get media frame format");
                mFF = GetFormatFromStreamConfiguration(mFS, sC.Format);
                if (mFF == null)
                {
                    errorMessage = string.Format("GetFormatFromStreamConfiguration() of {0} returned Null", SensorType);
                    Logger.Error(errorMessage);
                    throw new Exception(errorMessage);
                }

                Logger.Debug("Trying to set frame format");
                if (await setFrameFormat(mFS, mFF, (uint)sC.Width, (uint)sC.Height, sC.FrameRate, numerator, denominator) == false)
                {
                    errorMessage = string.Format("Failed To Set {0} Format", SensorType);
                    Logger.Error(errorMessage);
                    throw new Exception(errorMessage);
                }

                Logger.Debug(string.Format("Creating {0} FrameReader", SensorType));
                FrameReader = await _mC.CreateFrameReaderAsync(mFS);
            }).Wait();

            _isConfigured = true;
        }
        public void Close()
        {
            if (!_isConfigured)
            {
                string errorMessage = "Operation is not allowed - run Open() first before trying Close()";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            _mC.Dispose();       
                 
            _isConfigured = false;
        }
        public bool SetControl(Types.GenericControl control, int val)
        {
            return _cP.SetControl(control, SensorType, val);
        }
        public double GetControl(Types.GenericControl control)
        {
            Logger.Debug("Trying To Get " + control.ToString() +" Control");
            return _cP.GetControl(control, SensorType);
        }
        public void Start()
        {
            if (!_isConfigured)
            {
                string errorMessage = "Operation is not allowed - run Open() first before trying Start the stream";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            if (_activeStream)
            {
                string errorMessage = "Operation is not allowed - Stream is already started.";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            if(FrameReader == null)
            {
                string errorMessage = "Cannot starting Streming - Media Frame Reader Is null";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }
            
            Task.Run(async () =>
            {               
                await FrameReader.StartAsync();
            }).Wait();

            _activeStream = true;
        }
        public void Stop()
        {
            if (!_activeStream)
            {
                string errorMessage = "Operation is not allowed - stream does not started yet.";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            if (FrameReader == null)
            {
                string errorMessage = "Cannot stopping Streming - Media Frame Reader Is null";
                Logger.Error(errorMessage);
                throw new Exception(errorMessage);
            }

            Task.Run(async () =>
            {
                await FrameReader.StopAsync();
            }).Wait();

            _activeStream = false;
        }
        public void RegisterPythonListener(Delegate frameArriveHandler)
        {
            Logger.Debug("Registering Python callback");
            pythonFunc = frameArriveHandler;
            paramsToPass = new object[1];
        }
        public void RegisterFramesCallback()
        {
            Logger.Debug(string.Format("Registering Frames callback for {0}", DisplayName));
            FrameReader.FrameArrived += MFR_FrameArrived;
            Logger.Debug( DisplayName +  " MediaFrameReader was registered");            
        }
        public Types.FrameData ExtractFrameData(MediaFrameReader sender)
        {
            Types.FrameData _tempData = new Types.FrameData();
            var frame = sender.TryAcquireLatestFrame();

            var intelCaptureTiming = "2BF10C23-BF48-4C54-B1F9-9BB19E70DB05";
            Guid HW_TimeStampGuid = new Guid("D3C6ABAC-291A-4C75-9F47-D7B284A52619");
            Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME intelCaptureTimingMD = new Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME();
            UInt32 HwTimeStamp = 0;
            Object temp;
            var properties = frame.Properties;

            // *********************************************     Try getting Fame ID     ********************************************* 
            try
            {
                var intelCaptureTimingMDBytes = properties.Where(x => x.Key.ToString().ToUpper() == intelCaptureTiming).First().Value;
                intelCaptureTimingMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME>((byte[])intelCaptureTimingMDBytes);
            }
            catch (Exception ex)
            {
                properties.TryGetValue(HW_TimeStampGuid, out temp);
                HwTimeStamp = (UInt32)temp;
            }


            // ********************************************* Try getting Frame HW Timestamp ********************************************* 
            try
            {
                properties.TryGetValue(HW_TimeStampGuid, out temp);
                HwTimeStamp = (UInt32)temp;
            }
            catch (Exception ex)
            {
            }

            var systemTimeStamp = frame?.SystemRelativeTime.Value.TotalMilliseconds;
            var fmt = Types.GetFormatFromMediaFrameFormat(frame.Format);
            var sensor = Types.GetSensorTypeFromFormat(fmt);
            var reso = string.Format("{0}X{1}", frame.Format.VideoFormat.Width, frame.Format.VideoFormat.Height);
            var fps = (int)(frame.Format.FrameRate.Numerator / Convert.ToDouble(frame.Format.FrameRate.Denominator));
            //var frameCnt = IncFrameCounter(fmt);

            _tempData.FrameId = (int)intelCaptureTimingMD.frameCounter;
            _tempData.sensorSource = sensor;
            _tempData.format = fmt;
            _tempData.resolution = reso;
            _tempData.frameRate = fps;
            _tempData.sw_timeStamp = string.Format("{0}", systemTimeStamp);
            _tempData.hw_timeStamp = string.Format("{0}", HwTimeStamp);
            _tempData.ActualData = frame.BufferMediaFrame.Buffer.ToArray();

            return _tempData;
        }

        //=======================================================================================================================================================
        //================================================================= Private methods ======================================================================
        private void MFR_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            //Console.WriteLine(this.SensorType);
            var frameData = ExtractFrameData(sender);
            paramsToPass[0] = frameData;

            //Raise Event for Python
            try
            {
                if (pythonFunc == null)
                {
                    Logger.Error("No Python Func CallBack Was Received - it's Null");
                }
                else
                {
                    var ts = DateTime.Now.Millisecond;
                    Logger.Debug("Trying Invoke = " + ts);
                    pythonFunc?.DynamicInvoke(frameData);
                    Logger.Debug("Invoke Done = " + ts);
                }

                Logger.Debug("Frame Number = " + frameData.FrameId);
            }
            catch (Exception ex)
            {
                Logger.Error("pythonFunc.DynamicInvoke Was Failed on the folowing error: " + ex.Message);
            }
        }        
        private bool ParseFrameRate(int fps, Types.Formats fmt, uint width, uint height, out uint numerator, out uint denominator)
        {
            numerator = 0;
            denominator = 0;

            switch (fps)
            {
                case 6: { numerator = 2000000; denominator = 333333; return true; }
                case 15: { numerator = 15; denominator = 1; return true; }
                case 30: { numerator = 30; denominator = 1; return true; }
                case 60: { numerator = 60; denominator = 1; return true; }
                case 90: { numerator = 10000000; denominator = 111111; return true; }
                default: return false;
            }
        }
        private CameraProperties GetCameraProperty(Types.Sensor sensor)
        {
            MediaCapture tempMC = null;
            MediaCaptureInitializationSettings mediaCaptureSettings = new MediaCaptureInitializationSettings()
            {                
                SourceGroup = _privateSG,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };

            //init Media Capture object   
            Task.Run(async () =>
            {
                Logger.Debug(string.Format("Trying to init {0} MediaCapture object", SensorType));
                tempMC = new MediaCapture();
                await tempMC?.InitializeAsync(mediaCaptureSettings);
            }).Wait();

            return new CameraProperties(sensor, tempMC?.VideoDeviceController);
        }
        private MediaFrameSourceKind GetMediaFrameSourceKind(Types.Sensors kindStr)
        {
            //QaLogger.WriteLine("Getting MediaFrameSourceKind from (Types.Sensors kindStr)");
            switch (kindStr)
            {
                case Types.Sensors.COLOR: return MediaFrameSourceKind.Color;
                case Types.Sensors.DEPTH: return MediaFrameSourceKind.Depth;
                case Types.Sensors.IR: return MediaFrameSourceKind.Infrared;
                default: return MediaFrameSourceKind.Custom;
            }
        }
        private string GetSourceInfoID(Types.Sensor wantedSensor, MediaFrameSourceGroup group)
        {
            MediaFrameSource mFS = null;
            switch(wantedSensor)
            {
                case Types.Sensor.Depth:
                    {
                        foreach (var sI in group?.SourceInfos)
                        {
                            mFS = _mC?.FrameSources[sI.Id];
                            foreach (var item in mFS.SupportedFormats)
                            {
                                if (item.Subtype.ToLower().Contains("d16"))
                                    return sI.Id;
                            }
                        }
                        break;
                    }
                case Types.Sensor.IR:
                    {
                        foreach (var sI in group?.SourceInfos)
                        {
                            mFS = _mC?.FrameSources[sI.Id];
                            foreach (var item in mFS.SupportedFormats)
                            {
                                if (item.Subtype.ToLower().Contains("l8"))
                                    return sI.Id;
                            }
                        }
                        break;
                    }
                case Types.Sensor.Color:
                    {
                        foreach (var sI in group?.SourceInfos)
                        {
                            mFS = _mC?.FrameSources[sI.Id];
                            foreach (var item in mFS.SupportedFormats)
                            {
                                if (item.Subtype.ToLower().Contains("yuy2"))
                                    return sI.Id;
                            }
                        }
                        break;
                    }
                case Types.Sensor.Fisheye:
                    {
                        foreach (var sI in group?.SourceInfos)
                        {
                            mFS = _mC?.FrameSources[sI.Id];
                            foreach (var item in mFS.SupportedFormats)
                            {
                                if (item.Subtype.ToLower().Contains("raw8"))
                                    return sI.Id;
                            }
                        }
                        break;
                    }

                default: return "";
            }
            return "";
        }
        private MediaFrameSourceInfo GetMediaSourceInfoFromGroup(MediaCapture mC, MediaFrameSourceGroup group, MediaFrameSourceKind sK, out MediaFrameSource mFS)
        {
            mFS = null;
            //QaLogger.WriteLine("Getting MediaFrameSourceInfo from (MediaFrameSourceGroup and MediaFrameSourceKind)");
            var list = group?.SourceInfos.ToList();
            foreach (var sI in group?.SourceInfos)
            {
                try
                {
                    mFS = mC?.FrameSources[sI.Id];
                }
                catch (Exception e)
                {
                    Logger.Debug("errrror: " + e.Message);
                }
                //if (sI.SourceKind == MediaFrameSourceKind.Color)
                //{
                    if (sK == MediaFrameSourceKind.Infrared)
                    {
                    foreach (var item in mFS.SupportedFormats)
                        {
                            if (item.Subtype.ToLower().Contains("l8"))
                                return sI;
                        }
                    }
                    else if (sK == MediaFrameSourceKind.Color)
                    {
                    foreach (var item in mFS.SupportedFormats)
                        {
                            if (item.Subtype.ToLower().Contains("yuy2"))
                                return sI;
                        }
                    }
                //}
                //else if (sI.SourceKind == MediaFrameSourceKind.Depth)
                //{
                    else if (sK == MediaFrameSourceKind.Depth)
                    {
                        foreach (var item in mFS.SupportedFormats)
                        {                                                
                            if (item.Subtype.ToLower().Contains("d16"))
                                return sI;
                        }
                    }
                //}

                //var mst = sI.MediaStreamType;
                //var iD = sI.Id;
                //var dI = sI.DeviceInformation;
                //Console.WriteLine(string.Format("Source Kind = {0}\tDevice ID = {1}\tDevice Info = {2}\tMediaType = {3}", sI.SourceKind, sI.Id, sI.DeviceInformation, sI.MediaStreamType));

                //if (sI?.SourceKind == sK)
                //    return sI;
            }
            return null;
        }
        private MediaFrameFormat GetFormatFromStreamConfiguration(MediaFrameSource frameSource, Types.Formats fmt)
        {
            Logger.Debug("Calling GetFormatFromStreamConfiguration() method with " + fmt);
            switch (fmt)
            {
                case Types.Formats.Z:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.ToLower().Contains("d16"))
                                return sourceFormat;
                        }
                        break;
                    }
                case Types.Formats.Y:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.ToLower().Contains("l8"))
                                return sourceFormat;
                        }
                        break;
                    }
                case Types.Formats.L8R8:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.Contains("{20493859-0000-0010-8000-00AA00389B71}"))
                                return sourceFormat;
                        }
                        break;
                    }
                case Types.Formats.UYVY:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.Contains("{59565955-0000-0010-8000-00AA00389B71}"))
                                return sourceFormat;
                        }
                        break;
                    }
                case Types.Formats.CALIBRATION:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.Contains("{49323159-0000-0010-8000-00AA00389B71}"))
                                return sourceFormat;
                        }
                        break;
                    }

                case Types.Formats.RAW8:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.Contains("{38574152-1A66-A242-9065-D01814A8EF8A}"))
                                return sourceFormat;
                        }
                        break;
                    }

                case Types.Formats.YUY2:
                    {
                        foreach (var sourceFormat in frameSource.SupportedFormats)
                        {
                            if (sourceFormat.Subtype.ToLower().Contains("yuy2"))
                                return sourceFormat;
                        }
                        break;
                    }

                default:
                    {
                        return null;
                    }
            }
            return null;
        }
        private async Task<bool> setFrameFormat(MediaFrameSource mediaSource, MediaFrameFormat mediaFormat,
            uint width, uint height, int fps, uint numerator, uint denominator)
        {
            var list = mediaSource.SupportedFormats.Where(format =>
            {
                return format.VideoFormat.Width == width
                && format.VideoFormat.Height == height
                && format.FrameRate.Numerator == numerator
                && format.FrameRate.Denominator == denominator
                && format.Subtype == mediaFormat.Subtype;
            }).ToList();

            if (list.Count == 0)
            {
                throw new Exception("Format is not supported");
            }

            mediaFormat = list[0];
            await mediaSource.SetFormatAsync(mediaFormat);
            return true;
        }
        //=======================================================================================================================================================
    }
}
