using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronicMediaCapture
{
    public class Events
    {
        public delegate void DataRecieved(object source, TestEventArgs e);
        public delegate void QaLoggerStatus(object source, LoggerEvents e);

        //This is a class which describes the event to the class that recieves it.
        //An EventArgs class must always derive from System.EventArgs.
        public class TestEventArgs : EventArgs
        {
            private Types.FrameData EventInfo;
            public TestEventArgs(Types.FrameData data)
            {
                EventInfo = data;
            }
            public Types.FrameData GetInfo()
            {
                return EventInfo;
            }
        }
        public class LoggerEvents : EventArgs
        {
            private bool EventInfo;
            public LoggerEvents(bool isBusy)
            {
                EventInfo = isBusy;
            }
            public bool isBusy()
            {
                return EventInfo;
            }
        }
    }
}

