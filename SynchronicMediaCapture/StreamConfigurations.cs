using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronicMediaCapture
{
    public class StreamConfigurations
    {
        public List<StreamConfiguration> _configurations;
        public StreamConfigurations()
        {
            _configurations = new List<StreamConfiguration>();
            Logger.Debug("List Of Stream Configuration was Created Successfully");
        }
        public void Append(StreamConfiguration sC)
        {
            if(_configurations == null)
            {
                Logger.Error("StreamConfigurations List Is Null, validate that the StreamConfigurations Class Constructor is called before");
                return;
            }
            _configurations.Add(sC);
            Logger.Debug("Stream Configuration was added to the inner list");
        }
        public List<StreamConfiguration> GetListOfStreamConfigurations()
        {
            if(_configurations == null)
                Logger.Error("Stream Configurations list is Null");

            return _configurations;
        }
    }
}
