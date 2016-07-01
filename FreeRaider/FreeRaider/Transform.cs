using System;
using System.Linq;
using OpenTK;

namespace FreeRaider
{
    public class Transform
    {
        public Matrix3 Basis;

        public Vector3 Origin;

        public Quaternion Rotation
        {
            get { return Basis.GetRotation(); }
            set { Helper.SetRotation(ref Basis, value); }
        }

        public Transform Inverse
        {
            get
            {
                var inv = Basis;
                inv.Transpose();
                return new Transform(inv, inv.MultiplyByVector(-Origin));
            }
        }

        public Transform()
        {
        }

        public Transform(Matrix3 m, Vector3 c)
        {
            Basis = m;
            Origin = c;
        }

        public Transform Clone()
        {
            return new Transform(Basis, Origin);
        }

        public static Vector3 operator * (Transform t, Vector3 x)
        {
            return x.Dot3(t.Basis.Row0, t.Basis.Row1, t.Basis.Row2) + t.Origin;
        }

        public static Vector3 operator *(Vector3 x, Transform t)
        {
            return t * x;
        }

        public static Transform operator *(Transform a, Matrix4 b)
        {
            return a * (Transform) b;
        }

        public static Matrix4 operator *(Matrix4 a, Transform b)
        {
            return a * (Matrix4) b;
        }

        public static Transform operator *(Transform a, Transform b)
        {
            return new Transform {Origin = a.Origin + a.Basis.MultiplyByVector(b.Origin), Basis = a.Basis * b.Basis};
        }

        public static explicit operator Matrix4(Transform t)
        {
            return Helper.CreateMat4(t.GetOpenGLMatrix());
        }

        public static explicit operator Transform(Matrix4 t)
        {
            var r = new Transform();
            r.SetFromOpenGLMatrix(t.ToArray());
            return r;
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

        public void SetFromOpenGLMatrix(float[] m)
        {
            Helper.SetFromOpenGLSubMatrix(ref Basis, m);
            Origin = new Vector3(m[12], m[13], m[14]);
        }

        public void SetIdentity()
        {
            Basis = Matrix3.Identity;
            Origin = Vector3.Zero;
        }
    }
}
