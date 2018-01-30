using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronicMediaCapture
{
    public class XUCommandRes
    {//
        public bool IsCompletedOk { get; private set; }
        public string StringResult { get; private set; }
        public byte[] BytesResult { get; private set; }

        public XUCommandRes(bool isCompletedOk, string result, byte[] resultsBytes)
        {
            BytesResult = resultsBytes;
            StringResult = result;
            IsCompletedOk = isCompletedOk;
        }
    }
}
