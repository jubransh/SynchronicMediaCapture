using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.Devices;
using Winusb.Cli;

namespace SynchronicMediaCapture
{
    public class Device
    {
        bool _hasDepth = false;
        bool _hasColor = false;
        bool _hasFisheye = false;

        int colorIndex;
        int depthIndex;
        int iRIndex;
        int fisheyeIndex;
        XUCommandRes gvdData;
        string xmlLocation;


        public List<Sensor> Sensors { get; private set; } 

        List<Types.FrameData> _storedDepthFrames;
        List<Types.FrameData> _storedIRFrames;
        List<Types.FrameData> _storedFisheyeFrames;
        List<Types.FrameData> _storedColorFrames;

        CameraProperties _CameraProperty;
        MediaFrameSourceGroup _selectedGroup;
        MediaFrameSourceGroup _sharedSourceGroup;
        MediaFrameSourceGroup _depthSourceGroup;
        MediaFrameSourceGroup _colorSourceGroup;
        MediaFrameSourceGroup _fisheyeSourceGroup;

        MediaCapture _mC;
        MediaCapture _colorMC;
        MediaCapture _depthMC;
        MediaCapture _fisheyeMC;

        public string VID { get; }
        public string PID { get; }

        List<MediaFrameSource> listOfMediaFrameSource = null;
        List<MediaFrameReader> listOfMediaFrameReader = null;
        public event Events.DataRecieved OnDataReceived;
        Delegate pythonFunc;

        public Device(string id/*MediaCapture mC, /*MediaFrameSourceGroup selectedGroup*/)
        {
            Sensors = new List<Sensor>();

            VID = ExtractVid(id);
            PID = ExtractPid(id);
            Logger.Debug(string.Format("New Device with [VID : {0}] [PID : {1}] Was created", VID, PID));

            listOfMediaFrameSource = new List<MediaFrameSource>();
            listOfMediaFrameReader = new List<MediaFrameReader>();
        }
        public void InitSensors()
        {
            //Create Sensors
            if (_depthSourceGroup == null)
                return;

            if (_depthSourceGroup != null && _sharedSourceGroup == null) //only depth device is connected
            {
                Logger.Debug("_depthSourceGroup != null && _sharedSourceGroup == null");
                Sensor sDepth = new Sensor(_mC, Types.Sensor.Depth, _depthSourceGroup, _depthSourceGroup);
                Logger.Debug("Depth Sensor Created");
                Sensor sIR = new Sensor(_mC, Types.Sensor.IR, _depthSourceGroup, _depthSourceGroup);
                Logger.Debug("IR Sensor Created");
                Sensors.Add(sDepth);
                Logger.Debug("Depth Sensor Added to Sensors List");
                Sensors.Add(sIR);
                Logger.Debug("IR Sensor Added to Sensors List");
            }
            else // not only depth 
            {
                Logger.Debug("not only depth");
                //Add depth sensors           
                Sensor sDepth = new Sensor(_mC, Types.Sensor.Depth, _sharedSourceGroup, _depthSourceGroup);
                Sensor sIR = new Sensor(_mC, Types.Sensor.IR, _sharedSourceGroup, _depthSourceGroup);
                Sensors.Add(sDepth);
                Sensors.Add(sIR);

                if (_colorSourceGroup != null)
                {
                    Sensor sColor = new Sensor(_mC, Types.Sensor.Color, _sharedSourceGroup, _colorSourceGroup);
                    Sensors.Add(sColor);
                }

                if (_fisheyeSourceGroup != null)
                {
                    Sensor sFisheye = new Sensor(_mC, Types.Sensor.Fisheye, _sharedSourceGroup, _fisheyeSourceGroup);
                    Sensors.Add(sFisheye);
                }
            }
        }
        private MediaFrameSourceInfo GetSourceInfo(MediaCapture mC, MediaFrameSourceGroup mFSG, Types.Sensor sensor)
        {
            Logger.Debug(string.Format("GetSourceInfo() Method : Looking for {0} into the folowing ource Group: {1}", sensor, mFSG.DisplayName));
            int currentIndex = 0;
            foreach (var sI in mFSG.SourceInfos)
            {
                var mFS = mC?.FrameSources[sI.Id];
                listOfMediaFrameSource.Add(mFS);
                var debugList = mFS.SupportedFormats.ToList();


                foreach (var item in mFS.SupportedFormats)
                {
                    if (sensor == Types.Sensor.Depth)
                    {
                        if (item.Subtype.ToLower().Contains("d16"))
                        {
                            depthIndex = currentIndex++;
                            _hasDepth = true;
                            Logger.Debug(string.Format("{0} Sensor was Found", sensor));
                            return sI;
                        }
                    }

                    if (sensor == Types.Sensor.IR)
                    {
                        if (item.Subtype.ToLower().Contains("l8"))
                        {
                            iRIndex = currentIndex++;
                            _hasDepth = true;
                            Logger.Debug(string.Format("{0} Sensor was Found", sensor));
                            return sI;
                        }
                    }

                    if (sensor == Types.Sensor.Color)
                    {
                        if (item.Subtype.ToLower().Contains("yuy2"))
                        {
                            colorIndex = currentIndex++;
                            _hasColor = true;
                            Logger.Debug(string.Format("{0} Sensor was Found", sensor));
                            return sI;
                        }
                    }

                    if (sensor == Types.Sensor.Fisheye)
                    {
                        if (item.Subtype.Contains("{38574152-1A66-A242-9065-D01814A8EF8A}"))
                        {
                            fisheyeIndex = currentIndex++;
                            _hasFisheye = true;
                            Logger.Debug(string.Format("{0} Sensor was Found", sensor));
                            return sI;
                        }
                    }
                }
            }
            Logger.Error(string.Format("Error: {0} Sensor was Not Found !!!"));
            return null;
        }        
        public void InitDevice(string xmlLocation = "default")
        {
            this.xmlLocation = xmlLocation.ToLower().Equals("default") ? Environment.CurrentDirectory : xmlLocation;
            Logger.PrintTitle(" Init Device ");
            //validate that list of source info is not null
            //if (listOfSourceInfo == null)
            // return;

            MediaCaptureInitializationSettings mediaCaptureSettings = null;
            MediaCaptureInitializationSettings depthMediaCaptureSettings = null;
            MediaCaptureInitializationSettings colorMediaCaptureSettings = null;
            MediaCaptureInitializationSettings fisheyeMediaCaptureSettings = null;

            // ================================ Create Shared Media Capture ================================
            if (_sharedSourceGroup != null)
            {
                Logger.Debug("Creating shared MediaCapture setting object");
                mediaCaptureSettings = new MediaCaptureInitializationSettings()
                {
                    SourceGroup = _sharedSourceGroup,
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                //init Media Capture object   
                Task.Run(async () =>
                {
                    Logger.Debug("Trying to init Shared MediaCapture object");
                    _mC = new MediaCapture();
                    await _mC?.InitializeAsync(mediaCaptureSettings);
                }).Wait();
                Logger.Debug("Initializing shared MediaCapture object passed");
            }
            // ================================================================================================

            // ================================== Create Depth Media Capture ==================================
            if (_depthSourceGroup != null)
            {
                Logger.Debug("Creating Depth MediaCapture setting object");
                depthMediaCaptureSettings = new MediaCaptureInitializationSettings()
                {
                    SourceGroup = _depthSourceGroup,
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                //init Media Capture object   
                Task.Run(async () =>
                {
                    Logger.Debug("Trying to init Depth MediaCapture object");
                    _depthMC = new MediaCapture();
                    await _depthMC?.InitializeAsync(depthMediaCaptureSettings);
                }).Wait();

                Logger.Debug("Initializing Depth MediaCapture object passed");

                //var sI = GetSourceInfo(_depthMC, _depthSourceGroup, Types.Sensor.Depth);
                //var mFS = _depthMC?.FrameSources[sI.Id];
                //listOfMediaFrameSource.Add(mFS);
                _hasDepth = true;
            }

            // ================================================================================================

            // ================================ Create Color Media Capture ================================
            if (_colorSourceGroup != null)
            {
                Logger.Debug("Creating Color MediaCapture setting object");
                colorMediaCaptureSettings = new MediaCaptureInitializationSettings()
                {
                    SourceGroup = _colorSourceGroup,
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                //init Media Capture object   
                Task.Run(async () =>
                {
                    Logger.Debug("Trying to init Color MediaCapture object");
                    _colorMC = new MediaCapture();
                    await _colorMC?.InitializeAsync(colorMediaCaptureSettings);
                }).Wait();

                Logger.Debug("Initializing Color MediaCapture object passed");
                //var sI = GetSourceInfo(_colorMC, _colorSourceGroup, Types.Sensor.Color);
                //var mFS = _colorMC?.FrameSources[sI.Id];
                //listOfMediaFrameSource.Add(mFS);
                _hasColor = true;
            }

            // ================================================================================================

            // ================================ Create Fisheye Media Capture ================================
            if (_fisheyeSourceGroup != null)
            {
                Logger.Debug("Creating Fisheye MediaCapture setting object");
                fisheyeMediaCaptureSettings = new MediaCaptureInitializationSettings()
                {
                    SourceGroup = _fisheyeSourceGroup,
                    SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                    MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                    StreamingCaptureMode = StreamingCaptureMode.Video
                };

                //init Media Capture object   
                Task.Run(async () =>
                {
                    Logger.Debug("Trying to init Fisheye MediaCapture object");
                    _fisheyeMC = new MediaCapture();
                    await _fisheyeMC?.InitializeAsync(fisheyeMediaCaptureSettings);
                }).Wait();

                Logger.Debug("Initializing Fisheye MediaCapture object passed");
                //var sI = GetSourceInfo(_fisheyeMC, _fisheyeSourceGroup, Types.Sensor.Fisheye);
                //var mFS = _fisheyeMC?.FrameSources[sI.Id];
                //listOfMediaFrameSource.Add(mFS);
                _hasFisheye = true;
            }
            // ================================================================================================

            //create new camera property class
            if (_colorMC != null && _depthMC != null)
            {
                Logger.Debug("_colorMC != null && _depthMC != null");
                _CameraProperty = new CameraProperties(_depthMC.VideoDeviceController, _colorMC.VideoDeviceController, null);
            }
            else if (_depthMC != null)
            {
                Logger.Debug("_depthMC is not null");
                _CameraProperty = new CameraProperties(_depthMC.VideoDeviceController);
            }
            else if (_colorMC != null)
            {
                Logger.Debug("_colorMC is not null");
                _CameraProperty = new CameraProperties(_colorMC.VideoDeviceController);
            }

            //set the selected Group and media capture
            _selectedGroup = _sharedSourceGroup != null ? _sharedSourceGroup : _depthSourceGroup;
            _mC = _sharedSourceGroup != null ? _mC : _depthMC;


            //print to log
            Logger.Debug("Selected Group is: " + _selectedGroup?.DisplayName);

            //Getting Device Version Data GVD
            if (_selectedGroup.DisplayName.ToLower().Contains("intel"))
            {
                Logger.Debug("Getting Device Version Data GVD");
                gvdData = _CameraProperty.SendCommand("gvd", null);
                //gvdData = SendCommand("gvd");
            }
            Logger.PrintTitle("");
        }
        public List<Control> GetSupportedControls()
        {
            List<Control> supportedControls = new List<Control>();

            //==============================================[ Color Controls ]==============================================
            //Color Exposure Control
            var controlCap = _colorMC.VideoDeviceController.Exposure.Capabilities;
            if (controlCap.Supported)
                supportedControls.Add(new Control("Color Exposure", Types.Controls.Exposure, Types.ControlType.STANDARD, Types.SourceGroupType.COLOR, controlCap.Max, controlCap.Min, controlCap.Default, controlCap.Step, controlCap.AutoModeSupported));

            //color white balance
            controlCap = _colorMC.VideoDeviceController.WhiteBalance.Capabilities;
            if (controlCap.Supported)
                supportedControls.Add(new Control("White Balance", Types.Controls.WhiteBalance, Types.ControlType.STANDARD, Types.SourceGroupType.COLOR, controlCap.Max, controlCap.Min, controlCap.Default, controlCap.Step, controlCap.AutoModeSupported));

            //Color Hue Control
            controlCap = _colorMC.VideoDeviceController.Hue.Capabilities;
            if (controlCap.Supported)
                supportedControls.Add(new Control("Hue", Types.Controls.Hue, Types.ControlType.STANDARD, Types.SourceGroupType.COLOR, controlCap.Max, controlCap.Min, controlCap.Default, controlCap.Step, controlCap.AutoModeSupported));

            //Color Contrast Control
            controlCap = _colorMC.VideoDeviceController.Contrast.Capabilities;
            if (controlCap.Supported)
                supportedControls.Add(new Control("Contrast", Types.Controls.Contrast, Types.ControlType.STANDARD, Types.SourceGroupType.COLOR, controlCap.Max, controlCap.Min, controlCap.Default, controlCap.Step, controlCap.AutoModeSupported));

            //Color Brightness Control
            controlCap = _colorMC.VideoDeviceController.Brightness.Capabilities;
            if (controlCap.Supported)
                supportedControls.Add(new Control("Brightness", Types.Controls.Brightness, Types.ControlType.STANDARD, Types.SourceGroupType.COLOR, controlCap.Max, controlCap.Min, controlCap.Default, controlCap.Step, controlCap.AutoModeSupported));

            //Color BacklightCompensation Control
            controlCap = _colorMC.VideoDeviceController.BacklightCompensation.Capabilities;
            if (controlCap.Supported)
                supportedControls.Add(new Control("Brightness", Types.Controls.BacklightCompensation, Types.ControlType.STANDARD, Types.SourceGroupType.COLOR, controlCap.Max, controlCap.Min, controlCap.Default, controlCap.Step, controlCap.AutoModeSupported));

            //==============================================[ Depth XU Controls ]==============================================
            CameraProperties cp = new CameraProperties(_depthMC.VideoDeviceController);

            //try to get control, if exception is thrown then the control is not supported
            try
            {
                //cp.GetControl(Types.ControlName.DEPTH_AE);
                supportedControls.Add(new Control("Depth Auto Exposure", Types.Controls.DepthAE, Types.ControlType.XU, Types.SourceGroupType.DEPTH, 1, 0, 1, 1, true));
            }
            catch { /*do nothing*/}

            try
            {
                //cp.GetManualLaserPower();
               // supportedControls.Add(new Control("Manual Laser Power", Types.Controls.LaserPower, Types.ControlType.XU, Types.SourceGroupType.DEPTH, 1, 0, 1, 1, true));
               // supportedControls.Add(new Control("Laser Power", Types.Controls.LaserPowerOnOff, Types.ControlType.XU, Types.SourceGroupType.DEPTH, 2, 0, 1, 1, true));
            }
            catch { /*do nothing*/}

            try
            {
                //cp.GetDepthExposure();
                //supportedControls.Add(new Control("Depth Exposure", Types.Controls.DepthExposure, Types.ControlType.XU, Types.SourceGroupType.DEPTH, 166000, 20, 80, 10, true));
            }
            catch { /*do nothing*/}

            return supportedControls;
        }
        public void ConfigureStream(List<StreamConfiguration> streamList)
        {
            var errorMessage = "";
            Logger.PrintTitle(" Configure Stream ");
            MediaFrameSource mFS;

            int sCC = 1; //Stream Configuration Count
            foreach (var sC in streamList)
            {
                Logger.Debug(string.Format("stream number {0} is {1} / {2} / {3}X{4} / {5}", sCC++, sC.Sensor, sC.Format, sC.Width, sC.Height, sC.FrameRate));
                uint numerator, denominator;
                if (ParseFrameRate(sC.FrameRate, sC.Format, (uint)sC.Width, (uint)sC.Height, out numerator, out denominator) == false)
                {
                    errorMessage = "Failed to parse some "+ sC.FrameRate  + " Rate";
                    Logger.Error(errorMessage);
                    throw new Exception(errorMessage);
                }

                Task.Run(async () =>
                {
                    Logger.Debug(string.Format("Configuring {0} Sensor", sC.Sensor));
                    switch (sC.Sensor)
                    {
                        case Types.Sensor.Color:
                            {
                                //valide that sensor is supported in the source group
                                if (!_hasColor)
                                {
                                    errorMessage = "Color Sensor Is Not Supported";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                var sourceKind = GetMediaFrameSourceKind(Types.Sensors.COLOR);
                                GetMediaSourceInfoFromGroup(_mC, _selectedGroup, sourceKind, out mFS);

                                //Get Media Frame Format 
                                MediaFrameFormat mFF = GetFormatFromStreamConfiguration(mFS, sC.Format);
                                if (mFF == null)
                                {
                                    Logger.Error("GetFormatFromStreamConfiguration() of Color returned Null");
                                    break;
                                }

                                if (await setFrameFormat(mFS, mFF, (uint)sC.Width, (uint)sC.Height, sC.FrameRate, numerator, denominator) == false)
                                {
                                    errorMessage = "Failed To Set Format";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                Logger.Debug("Creating Color FrameReader");
                                var mFR = await _mC.CreateFrameReaderAsync(mFS);
                                listOfMediaFrameReader.Add(mFR);
                                Logger.Debug("Color FrameReader Added to the ListOfMediaFrameReaders");
                                break;
                            }
                        case Types.Sensor.Depth:
                            {
                                //valide that sensor is supported in the source group
                                if (!_hasDepth)
                                {
                                    errorMessage = "Depth Sensor Is Not Supported";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                var sourceKind = GetMediaFrameSourceKind(Types.Sensors.DEPTH);
                                GetMediaSourceInfoFromGroup(_mC, _selectedGroup, sourceKind, out mFS);

                                //Get Media Frame Format 
                                MediaFrameFormat mFF = GetFormatFromStreamConfiguration(mFS, sC.Format);
                                if (mFF == null)
                                {
                                    Logger.Error("GetFormatFromStreamConfiguration() of Depth returned Null");
                                    break;
                                }

                                if (await setFrameFormat(mFS, mFF, (uint)sC.Width, (uint)sC.Height, sC.FrameRate, numerator, denominator) == false)
                                {
                                    errorMessage = "Failed To Set Format";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                Logger.Debug("Creating Depth FrameReader");
                                var mFR = await _mC.CreateFrameReaderAsync(mFS);
                                listOfMediaFrameReader.Add(mFR);
                                Logger.Debug("Depth FrameReader Added to the ListOfMediaFrameReaders");
                                break;
                            }

                        case Types.Sensor.IR:
                            {
                                //valide that sensor is supported in the source group
                                if (!_hasDepth)
                                {
                                    errorMessage = "IR Sensor Is Not Supported";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                var sourceKind = GetMediaFrameSourceKind(Types.Sensors.IR);
                                GetMediaSourceInfoFromGroup(_mC, _selectedGroup, sourceKind, out mFS);

                                //Get Media Frame Format 
                                MediaFrameFormat mFF = GetFormatFromStreamConfiguration(mFS, sC.Format);
                                if (mFF == null)
                                {
                                    Logger.Error("GetFormatFromStreamConfiguration() of IR returned Null");
                                    break;
                                }

                                if (await setFrameFormat(mFS, mFF, (uint)sC.Width, (uint)sC.Height, sC.FrameRate, numerator, denominator) == false)
                                {
                                    errorMessage = "Failed To Set Format";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                Logger.Debug("Creating IR FrameReader");
                                var mFR = await _mC.CreateFrameReaderAsync(/*listOfMediaFrameSource[iRIndex]*/mFS);
                                listOfMediaFrameReader.Add(mFR);
                                Logger.Debug("IR FrameReader Added to the ListOfMediaFrameReaders");
                                break;
                            }

                        case Types.Sensor.Fisheye:
                            {
                                //valide that sensor is supported in the source group
                                if (!_hasFisheye)
                                {
                                    errorMessage = "FishEye Sensor Is Not Supported";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                var sourceKind = GetMediaFrameSourceKind(Types.Sensors.IR);
                                GetMediaSourceInfoFromGroup(_mC, _selectedGroup, sourceKind, out mFS);

                                //Get Media Frame Format 
                                MediaFrameFormat mFF = GetFormatFromStreamConfiguration(listOfMediaFrameSource[fisheyeIndex], sC.Format);
                                if(mFF == null)
                                {
                                    Logger.Error("GetFormatFromStreamConfiguration() of Fisheye returned Null");
                                    break;
                                }

                                if (await setFrameFormat(listOfMediaFrameSource[fisheyeIndex], mFF, (uint)sC.Width, (uint)sC.Height, sC.FrameRate, numerator, denominator) == false)
                                {
                                    errorMessage = "Failed To Set Format";
                                    Logger.Error(errorMessage);
                                    throw new Exception(errorMessage);
                                }

                                Logger.Debug("Creating Fisheye FrameReader");
                                var mFR = await _mC.CreateFrameReaderAsync(mFS);
                                listOfMediaFrameReader.Add(mFR);
                                Logger.Debug("Fisheye FrameReader Added to the ListOfMediaFrameReaders");
                                break;
                            }
                    }
                }).Wait();
            }
            Logger.PrintTitle();
        }
        public void RegisterPythonListener(Delegate frameArriveHandler)
        {
            Logger.Debug("Registering Python callback");
            pythonFunc = frameArriveHandler;
        }
        int framesCnt = 0;
        public void RegisterNonIntelCameraFramesCallback()
        {
            Logger.Debug("Registering  Non-Intel Camera Frames callback");
            int cnt = 1;
            foreach (var mFR in listOfMediaFrameReader)
            {
                mFR.FrameArrived += MFR_FrameArrived_NonIntel;
                Logger.Debug("MediaFrameReader number " + cnt++ + " was registered");
            }
        }

        public void RegisterFramesCallback()
        {
            Logger.Debug("Registering Frames callback");
            int cnt = 1;
            foreach (var mFR in listOfMediaFrameReader)
            {
                mFR.FrameArrived += MFR_FrameArrived;
                Logger.Debug("MediaFrameReader number " + cnt++ + " was registered");
            }
        }
        public void StartStreaming()
        {
            Logger.Debug("Starting Streaming");
            foreach (var mFR in listOfMediaFrameReader)
            {                
                Task.Run(async () =>
                {
                    await mFR.StartAsync();
                }).Wait();
            }
            Logger.Debug("Stream Started successfully");
        }
        public void StopStreaming()
        {
            Logger.Debug("Stopping Streaming");
            Task.Run(async () =>
            {
                foreach (var mFR in listOfMediaFrameReader)
                {
                    mFR.FrameArrived -= MFR_FrameArrived;
                    await mFR.StopAsync();
                    mFR.Dispose();
                }
            }).Wait();
            Logger.Debug("Stream Stopped successfully");

        }
        public string GetFwVersion()
        {
            return GetDataFromGVD("FunctionalPayloadVersion:");
            //try
            //{
            //    Logger.PrintTitle("Getting FW Version");

            //    var res = _CameraProperty.SendCommand("gvd", null);
            //    if (res?.IsCompletedOk == false)
            //    {
            //        Logger.Error("GVD command was not completed ok");
            //        return "N/A";
            //    }
            //    Logger.Debug("GVD Command was Completed Ok");

            //    var rows = res.StringResult.Split('\n');
            //    var fwVersionKey = "FW Version: ";
            //    foreach (var item in rows)
            //    {
            //        if (item.ToLower().StartsWith(fwVersionKey.ToLower()))
            //        {
            //            Logger.Debug("Returned FW Version is: " + item);
            //            Logger.PrintTitle();
            //            return item.Replace(fwVersionKey, "").Replace('-', '.');
            //        }
            //    }

            //}
            //catch (Exception exp)
            //{
            //    Logger.Debug("_CameraProperty.SendCommand Failed On: " + exp.Message);
            //}
            //Logger.Debug("FW Version key not found into the returned data from GVD command");
            //Logger.PrintTitle();
            //return "N/A";            
        }
        private string GetDataFromGVD(string key)
        {
            ////workingDir = workingDir.Equals("") ? Environment.CurrentDirectory : workingDir;
            //string workingDir = xmlLocation;
            //Logger.PrintTitle("Getting ASIC Serial Number");
            //Logger.Error("Current Dir is: " + workingDir);
            ////var res = _CameraProperty.SendCommand("gvd", null);
            //var res = SendCommand(/*workingDir,*/ "gvd");
            //if (res.ByteArray.Length == 0)
            //{
            //    Logger.Error("GVD command was not completed ok");
            //    return "000000000";
            //}
            //var rows = res.FormatedString.Split('\n');
            if (gvdData == null)
                return "N/A";


            var rows = gvdData.StringResult.Split('\n');
            var serialKey = key;
            foreach (var item in rows)
            {
                if (item.ToLower().StartsWith(serialKey.ToLower()))
                {
                    Logger.Debug(string.Format("Returned {0} is: {1}", key, item));
                    Logger.PrintTitle();
                    return item.Replace(serialKey, "").Replace("-", "").Replace(" ", "");
                }
            }
            Logger.Debug("key not found into the returned data from GVD command");
            Logger.PrintTitle();

            //var rows = gvdData.FormatedString.Split('\n');

            ////if (res.IsCompletedOk == false)
            ////{
            ////    Logger.Error("GVD command was not completed ok");
            ////    return "N/A";
            ////}
            ////var rows = res.StringResult.Split('\n');
            //var serialKey = key;
            //foreach (var item in rows)
            //{
            //    if (item.ToLower().StartsWith(serialKey.ToLower()))
            //    {
            //        Logger.Debug(string.Format("Returned {0} is: {1}",key, item));
            //        Logger.PrintTitle();
            //        return item.Replace(serialKey, "").Replace("-", "").Replace(" ", "");
            //    }
            //}
            //Logger.Debug("key not found into the returned data from GVD command");
            //Logger.PrintTitle();
            return "N/A";

        }

        public string GetSerial(/*string workingDir = ""*/)
        {
            return GetDataFromGVD("OpticModuleSerial:");
        }
        public bool ResetCamera()
        {
            return _CameraProperty.SendCommand("rst", null).IsCompletedOk; 
        }

        public bool SetU3Mode(bool mode)
        {
            int parameter = mode ? 1 : 0;
            return _CameraProperty.SendCommand("pfd", new int[] { parameter, 0, 0, 0 }).IsCompletedOk;
        }
        public bool SetControl(Types.GenericControl control, Types.Sensor sT, int val)
        {          
            return _CameraProperty.SetControl(control, sT, val);   
        }
        public double GetControl(Types.GenericControl control, Types.Sensor sT)
        {
            return _CameraProperty.GetControl(control, sT);
        }
        public void AddSourceGroup(Types.SourceGroupType sGT, MediaFrameSourceGroup mFSG)
        {
            switch (sGT)
            {
                case Types.SourceGroupType.SHARED: { _sharedSourceGroup = mFSG; Logger.Debug("Shared device was added to the device"); break; }
                case Types.SourceGroupType.DEPTH: { _depthSourceGroup = mFSG; Logger.Debug("Depth device was added to the device"); break; }
                case Types.SourceGroupType.COLOR: { _colorSourceGroup = mFSG; Logger.Debug("Color device was added to the device"); break; }
                case Types.SourceGroupType.FISHEYE: {_fisheyeSourceGroup = mFSG; Logger.Debug("Fisheye device was added to the device"); break;}
                default: break;
            }
        }
        public void Close()
        {
            Logger.Debug("Closing Device");
            _mC?.Dispose();            
            _depthMC?.Dispose();
            _colorMC?.Dispose();
            _fisheyeMC?.Dispose();
            foreach(var sensor in Sensors)
            {
                sensor?.Dispose();
            }
        }
        public void Dispose()
        {
            Logger.Debug("Restarting Frame Server Service");
            KillFrameServer();
            //RestartFrameServer();
        }
        public List<Types.SensorInfo> GetSensors()
        {
            Types.SensorInfo sI = new Types.SensorInfo();
            var listOfSensors = new List<Types.SensorInfo>();

            if (_depthSourceGroup != null)
            {
                sI.DisplayName = _depthSourceGroup.DisplayName;
                sI.FullID = _depthSourceGroup.Id;
                sI.PID = ExtractPid(_depthSourceGroup.Id);
                sI.VID = ExtractVid(_depthSourceGroup.Id);
                sI.SensorType = GetTypeFromDisplayName(_depthSourceGroup.DisplayName);
                sI.ID = Types.ExtractSensorEndPointFromID(_depthSourceGroup.Id);
                listOfSensors.Add(sI);
            }
            if (_colorSourceGroup != null)
            {
                sI.DisplayName =    _colorSourceGroup.DisplayName;
                sI.FullID =         _colorSourceGroup.Id;
                sI.PID =            ExtractPid(_colorSourceGroup.Id);
                sI.VID =            ExtractVid(_colorSourceGroup.Id);
                sI.SensorType =     GetTypeFromDisplayName(_colorSourceGroup.DisplayName);
                sI.ID =             Types.ExtractSensorEndPointFromID(_colorSourceGroup.Id);
                listOfSensors.Add(sI);
            }
            if (_fisheyeSourceGroup != null)
            {
                sI.DisplayName =    _fisheyeSourceGroup.DisplayName;
                sI.FullID =         _fisheyeSourceGroup.Id;
                sI.PID =            ExtractPid(_fisheyeSourceGroup.Id);
                sI.VID =            ExtractVid(_fisheyeSourceGroup.Id);
                sI.SensorType =     GetTypeFromDisplayName(_fisheyeSourceGroup.DisplayName);
                sI.ID =             Types.ExtractSensorEndPointFromID(_fisheyeSourceGroup.Id);
                listOfSensors.Add(sI);
            }

            return listOfSensors;
        }
        public XUCommandRes SendXuCommand(string command)
        {
            return _CameraProperty.SendCommand(command);
        }
        public CommandResult SendCommand(/*string xmlLocation,*/ string command)
        {
            var DS5GUID = "08090549CE7841DCA0FB1BD66694BB0C";
            var DS5_XU_GUID = new Guid("C9606CCB-594C-4D25-AF47-CCC496435995");
            Guid guid = new Guid(DS5GUID);
            HWMonitorDevice hwMonitorDevice = null;
            //Try Via WinUSB first (if camera is locked will create HWMonitor via XU
            try
            {
                Logger.Debug("Trying to create HWMonitorDevice via WinUSB pipe");
                hwMonitorDevice = new HWMonitorDevice(VID, PID, guid, 1); //setting of the command..
                Logger.Debug("Creating HWMonitorDevice via WinUSB WinUsb Completed");
            }
            catch
            {
                Logger.Warning("Creating HWMonitorDevice via WinUSB pipe Failed [Device may be locked]");
                Logger.Debug("Trying to create HWMonitorDevice via UVC XU");
                hwMonitorDevice = new HWMonitorDevice(VID, PID, "0", DS5_XU_GUID, 1, 1);
                Logger.Debug("Creating HWMonitorDevice via WinUSB UVC XU Completed");
            }
            var xml_path = Path.Combine(xmlLocation, "CommandsDS5.xml");
            var parser = new CommandsXmlParser(xml_path); //the xml file where you find all commands availble
            try
            {
                return parser.SendCommand(hwMonitorDevice, command); //insert the HW command, the same as typing in the Terminal
            }
            catch(Exception ex)
            {
                var errorMsg = string.Format("Sending {0} Command was Failed on {1}", command, ex.Message);
                Logger.Error(errorMsg);
                throw new Exception(errorMsg);
            }
        }

        //==========================================================================================================================================
        //                                                      Private Methods
        //==========================================================================================================================================
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIME
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
        private MediaFrameSourceInfo GetMediaSourceInfoFromGroup(MediaCapture mC, MediaFrameSourceGroup group, MediaFrameSourceKind sK, out MediaFrameSource mFS)
        {
            mFS = null;
            //QaLogger.WriteLine("Getting MediaFrameSourceInfo from (MediaFrameSourceGroup and MediaFrameSourceKind)");
            var list = group?.SourceInfos.ToList();
            foreach (var sI in group?.SourceInfos)
            {
                mFS = mC?.FrameSources[sI.Id];

                if (sI.SourceKind == MediaFrameSourceKind.Color)
                {
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
                }
                else if (sI.SourceKind == MediaFrameSourceKind.Depth)
                {
                    if (sK == MediaFrameSourceKind.Depth)
                    {
                        foreach (var item in mFS.SupportedFormats)
                        {
                            if (item.Subtype.ToLower().Contains("d16"))
                                return sI;
                        }
                    }
                }

                //var mst = sI.MediaStreamType;
                //var iD = sI.Id;
                //var dI = sI.DeviceInformation;
                //Console.WriteLine(string.Format("Source Kind = {0}\tDevice ID = {1}\tDevice Info = {2}\tMediaType = {3}", sI.SourceKind, sI.Id, sI.DeviceInformation, sI.MediaStreamType));

                //if (sI?.SourceKind == sK)
                //    return sI;
            }
            return null;
        }
        private void MFR_FrameArrived_NonIntel(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            Logger.Debug("Frame Recieved");
            Types.FrameData _tempData = new Types.FrameData();
            var frame = sender.TryAcquireLatestFrame();

            var systemTimeStamp = frame?.SystemRelativeTime.Value.TotalMilliseconds;
            var fmt = Types.GetFormatFromMediaFrameFormat(frame.Format);
            var sensor = Types.GetSensorTypeFromFormat(fmt);
            var reso = string.Format("{0}X{1}", frame.Format.VideoFormat.Width, frame.Format.VideoFormat.Height);
            var fps = (int)(frame.Format.FrameRate.Numerator / Convert.ToDouble(frame.Format.FrameRate.Denominator));
            //var frameCnt = IncFrameCounter(fmt);

            _tempData.FrameId = framesCnt++;
            _tempData.sensorSource = sensor;
            _tempData.format = fmt;
            _tempData.resolution = reso;
            _tempData.frameRate = fps;
            _tempData.sw_timeStamp = string.Format("{0}", systemTimeStamp);
            _tempData.hw_timeStamp = "000000000";
            _tempData.ActualData = frame.BufferMediaFrame.Buffer.ToArray();


            //Raise Done Event       
            Logger.Debug("Trying To Raise Event ");
            RaiseDataReceivedEvent(this, new Events.TestEventArgs(_tempData));
        }

        private void MFR_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            Logger.Debug("Frame Recieved");
            Types.FrameData _tempData = new Types.FrameData();
            var frame = sender.TryAcquireLatestFrame();

            var intelCaptureTiming = "2BF10C23-BF48-4C54-B1F9-9BB19E70DB05";
            Guid HW_TimeStampGuid = new Guid("D3C6ABAC-291A-4C75-9F47-D7B284A52619");
            Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING intelCaptureTimingMD = new Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING();
            UInt32 HwTimeStamp = 0;
            Object temp;
            var properties = frame.Properties;

            // *********************************************     Try getting Fame ID     ********************************************* 
            try
            {
                var intelCaptureTimingMDBytes = properties.Where(x => x.Key.ToString().ToUpper() == intelCaptureTiming).First().Value;
                intelCaptureTimingMD = Types.ByteArrayToStructure<Types.REAL_SENSE_RS400_DEPTH_METADATA_INTEL_CAPTURE_TIMING>((byte[])intelCaptureTimingMDBytes);
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

            //Raise Done Event       
            Logger.Debug("Trying To Raise Event ");                     
            RaiseDataReceivedEvent(this, new Events.TestEventArgs(_tempData));
        }
        private void RaiseDataReceivedEvent(object source, Events.TestEventArgs e)
        {
            Logger.Debug("RaiseDataReceivedEvent() Started ");
            var origData = e.GetInfo();

            //Pass parameters to Python
            Logger.Debug("Building FrameData Object");

            var newData = new Types.FrameData();
            newData.format = origData.format;
            newData.FrameId = origData.FrameId;
            newData.hw_timeStamp = origData.hw_timeStamp;
            newData.sw_timeStamp = origData.sw_timeStamp;
            newData.mmCounter = origData.mmCounter;
            newData.usbCounter = origData.usbCounter;
            newData.x = origData.x;
            newData.y = origData.y;
            newData.z = origData.z;
            newData.sensorSource = origData.sensorSource;
            newData.resolution = origData.resolution;
            newData.frameRate = origData.frameRate;
            newData.ActualData = origData.ActualData;

            object[] paramsToPass = new object[1];
            paramsToPass[0] = newData;

            Logger.Debug("FrameData Object was built");

            try
            {
                if(pythonFunc == null)
                {
                    Logger.Error("No Python Func CallBack Was Received - it's Null");
                }
                else
                {
                    var ts = DateTime.Now.Millisecond;
                    Logger.Debug("Trying Invoke = " + ts);
                    pythonFunc?.DynamicInvoke(paramsToPass);
                    Logger.Debug("Invoke Done = " + ts);
                }

                Logger.Debug("Frame Number = " + newData.FrameId);
            }
            catch (Exception ex)
            {
                Logger.Error("pythonFunc.DynamicInvoke Was Failed on the folowing error: " + ex.Message);                
            }

            OnDataReceived(source, e);
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
        private bool ParseFrameRate(int fps, Types.Formats fmt, uint width, uint height, out uint numerator, out uint denominator)
        {
            numerator = 0;
            denominator = 0;

            switch (fps)
            {
                case 6:     { numerator = 2000000; denominator = 333333; return true; }
                case 15:    { numerator = 15; denominator = 1; return true; }
                case 30:    { numerator = 30; denominator = 1; return true; }
                case 60:    { numerator = 60; denominator = 1; return true; }
                case 90:    { numerator = 10000000; denominator = 111111; return true; }
                default:    return false;
            }
        }
        private static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();
            return stuff;
        }
        enum KSCAMERA_MetadataId : long
        {
            MetadataId_Standard_Start = 1,
            MetadataId_PhotoConfirmation = MetadataId_Standard_Start,
            MetadataId_UsbVideoHeader,
            MetadataId_CaptureStats,
            MetadataId_CameraExtrinsics,
            MetadataId_CameraIntrinsics,
            MetadataId_FrameIllumination,
            MetadataId_Standard_End = MetadataId_FrameIllumination,
            MetadataId_Custom_Start = 0x80000000,
        }
        struct KSCAMERA_METADATA_ITEMHEADER
        {
            long MetadataId;
            long Size;         // Size of this header + metadata payload following
        }
        private Types.Formats GetFormatFromMediaFrameFormat(MediaFrameFormat mFF)
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
        private Types.Sensors GetSensorTypeFromFormat(Types.Formats fmt)
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
        private string ExtractVid(string fullId)
        {
            var parts = fullId.Split('_');
            if (parts == null || parts.Length < 4)
                return "Failed to get VID";
            return parts[1].Replace("&PID", "");
        }
        private string ExtractPid(string fullId)
        {
            var parts = fullId.Split('_');
            if (parts == null || parts.Length < 4)
                return "Failed to get PID";
            return parts[2].Replace("&MI", "");
        }
        private string ExtractSensorEndPointFromID(string fullId)
        {
            var parts = fullId.Split('&');
            if (parts == null || parts.Length < 6)
                return "Failed to get Sensor Endpoint";
            return parts[5].Split('#')[0];
        }
        private Types.SourceGroupType GetTypeFromDisplayName(string str)
        {
            switch(str)
            {
                case "Intel RS400 Cameras":
                    return Types.SourceGroupType.SHARED;

                case "Intel(R) RealSense(TM) 430 Depth":
                case "Intel(R) RealSense(TM) 430 with RGB Module Depth":
                case "Intel(R) RealSense(TM) 430 with Tracking Module Depth":
                    return Types.SourceGroupType.DEPTH;

                case "Intel(R) RealSense(TM) 430 with RGB Module RGB":
                    return Types.SourceGroupType.COLOR;

                case "Intel(R) RealSense(TM) 430 with Tracking Module FishEye":
                    return Types.SourceGroupType.FISHEYE;

                default:
                    return Types.SourceGroupType.UNKNOWN;
            }
        }
        private void RestartFrameServer()
        { 
            string serviceName = "Windows Camera Frame Server";
            Logger.Debug("Restarting Frame Server Service");
            ServiceController serviceController = new ServiceController(serviceName);
            try
            {
                if ((serviceController.Status.Equals(ServiceControllerStatus.Running)) || (serviceController.Status.Equals(ServiceControllerStatus.StartPending)))
                {
                    serviceController.Stop();
                }
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                serviceController.Start();
                serviceController.WaitForStatus(ServiceControllerStatus.Running);
                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Logger.Error("Error when trying restarting windows service: " + e.Message);
            }
        }
        private void KillFrameServer()
        {
            string serviceName = "Windows Camera Frame Server";
            Logger.Debug("Killing Frame Server Service");
            ServiceController serviceController = new ServiceController(serviceName);
            try
            {
                if ((serviceController.Status.Equals(ServiceControllerStatus.Running)) || (serviceController.Status.Equals(ServiceControllerStatus.StartPending)))
                {
                    serviceController.Stop();
                }
                serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                Thread.Sleep(500);
            }
            catch (Exception e)
            {
                Logger.Error("Error when trying Killing windows service: " + e.Message);
            }
        }

    }
}




    

