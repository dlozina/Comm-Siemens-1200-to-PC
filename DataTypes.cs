using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Koncar_Siemens_WPF
{
    public struct AreaPoint
    {
        public float X;
        public float Y;
    }

    public struct VelocityPoint
    {
        public float X;
        public float Y;
    }

    public struct CameraOutput
    {
        public AreaPoint POINT1;
        public AreaPoint POINT2;
        public float PARAMETER;
        public short TYPE;
    }

}
