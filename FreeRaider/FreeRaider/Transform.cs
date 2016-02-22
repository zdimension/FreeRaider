using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreeRaider
{
    public class Transform
    {
        public Matrix3 Basis { get; set; }

        public Vector3 Origin { get; set; }

        public Quaternion Rotation => Basis.ExtractRotation();

        public static Vector3 operator * (Transform t, Vector3 x)
        {
            return x.Dot3(t.Basis.Row0, t.Basis.Row1, t.Basis.Row2) + t.Origin;
        }

        public static Vector3 operator *(Vector3 x, Transform t)
        {
            return t * x;
        }

        public void SetIdentity()
        {
            Basis = Matrix3.Identity;
            Origin = Vector3.Zero;
        }
    }
}
