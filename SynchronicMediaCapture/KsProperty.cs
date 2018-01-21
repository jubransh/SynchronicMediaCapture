using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace SynchronicMediaCapture
{

    public enum VideoProcAmp_ControlValues
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
        POWERLINE_FREQUENCY
    };

    public enum CameraControl_ControlValues
    {
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
        AUTO_EXPOSURE_PRIORITY
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KSIDENTIFIER
    {
        public Guid set;
        public UInt32 id;
        public UInt32 flags;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct PROPSETID_VIDCAP
    {
        public KSIDENTIFIER ksproperty;
        public Int32 value;
        public UInt32 flags;
        public UInt32 capabilities;
        public UInt32 padding;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KSPROPERTY_MEMBERSHEADER
    {
        public UInt32 MembersFlags;
        public UInt32 MembersSize;
        public UInt32 MembersCount;
        public UInt32 Flags;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KSPROPERTY_BOUNDS_LONG
    {
        public UInt32 Minimum;
        public UInt32 Maximum;
    };

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct KSPROPERTY_STEPPING_LONG
    {
        public UInt32 SteppingDelta;
        public UInt32 Reserved;
        public KSPROPERTY_BOUNDS_LONG Bounds;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct KSPROPERTY_DESCRIPTION
    {
        public UInt32 AccessFlags;
        public UInt32 DescriptionSize;
        public KSIDENTIFIER PropTypeSet;
        public UInt32 MembersListCount;
        public UInt32 Reserved;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct CapSourceKsMemberList
    {
        public KSPROPERTY_DESCRIPTION desc;
        public KSPROPERTY_MEMBERSHEADER hdr;
        public KSPROPERTY_STEPPING_LONG step;
    }

    public struct KSPROPERTY_CAMERACONTROL_S
    {
        public KSIDENTIFIER Property;
        public Int32 Value;                   // value to get or set
        public UInt32 Flags;                   // KSPROPERTY_CAMERACONTROL_FLAGS_*
        public UInt32 Capabilities;            // KSPROPERTY_CAMERACONTROL_FLAGS_*
        public UInt32 padding;
    };

    public class DeviceProperty
    {
        protected string str;
        protected uint id;
        protected Windows.Media.Devices.VideoDeviceController controller;

        protected DeviceProperty(string theSet, uint theId, Windows.Media.Devices.VideoDeviceController deviceController)
        {
            id = theId;
            str = "{" + theSet + "} " + id;
            controller = deviceController;
        }

        protected byte[] PdoToBuffer<T>(T obj)
        {
            int len = Marshal.SizeOf<T>();
            byte[] arr = new byte[len];
            IntPtr ptr = Marshal.AllocHGlobal(len);
            Marshal.StructureToPtr<T>(obj, ptr, true);
            Marshal.Copy(ptr, arr, 0, len);
            Marshal.FreeHGlobal(ptr);
            return arr;
        }

        public static T ByteArrayToStructure<T>(byte[] arr)
        {
            T str = default(T);
            int size = Marshal.SizeOf<T>();
            IntPtr ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(arr, 0, ptr, size);
            str = Marshal.PtrToStructure<T>(ptr);
            Marshal.FreeHGlobal(ptr);
            return str;
        }
    }

    public class PropsetidVidcapControl : DeviceProperty
    {
        public PropsetidVidcapControl(string theSet, uint theId, Windows.Media.Devices.VideoDeviceController deviceController)
            : base(theSet, theId, deviceController)
        {
            set = new Guid(theSet);
        }

        public bool Set(int value)
        {
            try
            {
                PROPSETID_VIDCAP vidCap = new PROPSETID_VIDCAP();
                vidCap.ksproperty.set = set;
                vidCap.ksproperty.id = id;
                vidCap.ksproperty.flags = 2;
                vidCap.flags = 0x2;
                vidCap.capabilities = 0;
                vidCap.value = value;

                byte[] data = PdoToBuffer<PROPSETID_VIDCAP>(vidCap);
                var status = controller.SetDevicePropertyByExtendedId(data, data);
                Console.WriteLine("Status: " + status.ToString());
                if ((int)status == 0)
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set with exception " + ex.ToString());
                return false;
            }
            return false;
        }
        public int Get()
        {
            try
            {
                PROPSETID_VIDCAP vidCap = new PROPSETID_VIDCAP();
                vidCap.ksproperty.set = set;
                vidCap.ksproperty.id = id;
                vidCap.ksproperty.flags = 1;
                vidCap.flags = 0;
                vidCap.capabilities = 0;
                vidCap.value = 0;

                byte[] data = PdoToBuffer<PROPSETID_VIDCAP>(vidCap);
                int size = Marshal.SizeOf<KSPROPERTY_CAMERACONTROL_S>();
                var result = controller.GetDevicePropertyByExtendedId(data, (uint)size);

                Debug.WriteLine(result.Status);
                byte[] propertyData = result.Value as byte[];
                //Debug.Assert(propertyData != null && propertyData?.Length > 0);
                KSPROPERTY_CAMERACONTROL_S resultValue = new KSPROPERTY_CAMERACONTROL_S();
                resultValue = ByteArrayToStructure<KSPROPERTY_CAMERACONTROL_S>(propertyData);
                return resultValue.Value;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to set with exception " + ex.ToString());
                return 0;
            }
        }

        Guid set;
    }

    public class VideoProcAmp : PropsetidVidcapControl
    {
        public VideoProcAmp(uint id, Windows.Media.Devices.VideoDeviceController deviceController)
            : base("C6E13360-30AC-11d0-A18C-00A0C9118956", id, deviceController)
        {
        }
    }

    public class CameraControl : PropsetidVidcapControl
    {
        public CameraControl(uint id, Windows.Media.Devices.VideoDeviceController deviceController)
            : base("C6E13370-30AC-11d0-A18C-00A0C9118956", id, deviceController)
        {
        }
    }
}
