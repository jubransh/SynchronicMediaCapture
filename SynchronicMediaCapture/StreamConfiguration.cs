using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Capture.Frames;

namespace SynchronicMediaCapture
{
    public class StreamConfiguration
    {
        public Types.Sensor Sensor  { get; private set; }
        public Types.Formats Format { get; private set; }
        public int Width            { get; private set; }
        public int Height           { get; private set; }
        public int FrameRate        { get; private set; }      
        public StreamConfiguration(Types.Sensor sensor, Types.Formats format, int width, int height, int frameRate)
        {
            Logger.Debug("StreaminConfiguration Constructor Called With the following data:\r\n" +
                "                            Sensor = " + sensor + "\r\n" +
                "                            Format = " + format + "\r\n" +
                "                            Width  = " + width + "\r\n" +
                "                            Height = " + height + "\r\n" +
                "                            FPS    = " + frameRate);
                            
            Sensor = sensor;
            Format = format;
            Width = width;
            Height = height;
            FrameRate = frameRate;
        }
    }
}
