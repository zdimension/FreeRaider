using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public class Transform
    {
        public Matrix3 Basis { get; set; }

        public Vector3 Origin { get; set; }

        public Quaternion Rotation => Basis.ExtractRotation();
    }
}
