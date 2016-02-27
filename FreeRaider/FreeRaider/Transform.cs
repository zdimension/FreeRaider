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
        public Matrix3 Basis;

        public Vector3 Origin;

        public Quaternion Rotation => Basis.ExtractRotation();

        public static Vector3 operator * (Transform t, Vector3 x)
        {
            return x.Dot3(t.Basis.Row0, t.Basis.Row1, t.Basis.Row2) + t.Origin;
        }

        public static Vector3 operator *(Vector3 x, Transform t)
        {
            return t * x;
        }

        public static Transform operator *(Transform a, Transform b)
        {
            return new Transform {Origin = a.Origin + a.Basis.MultiplyByVector(b.Origin), Basis = a.Basis * b.Basis};
        }

        public static explicit operator Matrix4(Transform t)
        {
            return Helper.CreateInstance<Matrix4>(t.GetOpenGLMatrix());
        }

        public float[] GetOpenGLMatrix()
        {
            var ret = new float[16];
            var sm = Basis.GetOpenGLSubMatrix();
            Array.Copy(sm, ret, 12);
            ret[12] = Origin.X;
            ret[13] = Origin.Y;
            ret[14] = Origin.Z;
            ret[15] = 1.0f;
            return ret;
        }

        public void SetIdentity()
        {
            Basis = Matrix3.Identity;
            Origin = Vector3.Zero;
        }
    }
}
