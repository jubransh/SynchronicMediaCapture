using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SynchronicMediaCapture
{
    public class Control
    {        
        public string ControlName { get; private set; }
        public Types.Controls Property { get; private set; }
        public Types.GenericControl GProperty { get; private set; }

        public Types.ControlType Type { get; private set; }
        public Types.SourceGroupType BelongTo { get; private set; }
        public double Max { get; private set; }
        public double Min { get; private set; }
        public double Default { get; private set; }
        public double Step { get; private set; }
        public bool AutoCapable { get; private set; }
        public bool Value { get; private set; }

        public Control(string name, Types.Controls property, Types.ControlType type, Types.SourceGroupType Sensor, double max, double min, double def, double step, bool autoCap)
        {
            ControlName = name;
            Property = property;
            Type = type;
            BelongTo = Sensor;
            Max = max;
            Min = min;
            Default = def;
            Step = step;
            AutoCapable = autoCap;
        }
        public Control(string name, Types.GenericControl property, Types.ControlType type, Types.SourceGroupType Sensor, double max, double min, double def, double step, bool autoCap)
        {
            ControlName = name;
            GProperty = property;
            Type = type;
            BelongTo = Sensor;
            Max = max;
            Min = min;
            Default = def;
            Step = step;
            AutoCapable = autoCap;
        }

    }
}
