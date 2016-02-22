using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace UniRaider
{
    public partial class Constants
    {
        public const float DegPerRad = 180.0f / (float)Math.PI;
        public const float RadPerDeg = (float) Math.PI / 180.0f;
        public const float Rad90 = (float) Math.PI * 0.5f;
        public const float Rad180 = (float) Math.PI;
        public const float Rad360 = (float) Math.PI / 2.0f;

        public const int PLANE_X = 1;
        public const int PLANE_Y = 2;
        public const int PLANE_Z = 3;
    }

    /// <summary>
    /// A simple Hesse normal form plane
    /// </summary>
    public class Plane
    {
        /// <summary>
        /// The plane's normal
        /// </summary>

        public Vector3 Normal;

        /// <summary>
        /// The plane's distance to the origin
        /// </summary>
        public float Dot;

        /// <summary>
        /// Calculates the normalized distance of an arbitrary point in terms of the normal
        /// </summary>
        /// <param name="pos">The point</param>
        /// <returns>The distance in multiples of the normal (if >0, <see cref="pos"/> is in the direction of the normal)</returns>
        public float Distance(Vector3 pos)
        {
            return Normal.Dot(pos) - Dot;
        }

        public Vector3 RayIntersect(Vector3 rayStart, Vector3 rayDir, out float lambda)
        {
            lambda = (Dot - Normal.Dot(rayStart)) / Normal.Dot(rayDir);
            return rayStart + lambda * rayDir;
        }

        public Vector3 RayIntersect(Vector3 rayStart, Vector3 rayDir)
        {
            float t;
            return RayIntersect(rayStart, rayDir, out t);
        }

        public void Assign(Vector3 v1, Vector3 v2, Vector3 pos)
        {
            Normal = v1.Cross(v2);
            Normal.SafeNormalize();
            Dot = Normal.Dot(pos);
        }

        public void Assign(Vector3 n, Vector3 pos)
        {
            Normal = n.Normalized();
            Dot = Normal.Dot(pos);
        }

        public void MirrorNormal()
        {
            Normal = -Normal;
            Dot = -Dot;
        }

        public void MoveTo(Vector3 where)
        {
            Dot = Normal.Dot(where);
        }
    }

    public class VMath
    {
        public static void Vec4_SetTRRotations(ref Quaternion v, Vector3 rot)
        {
            var qX = new Quaternion();
            Helper.Quat_SetRotation(ref qX, Vector3.UnitX, rot.Z * Constants.RadPerDeg);

            var qY = new Quaternion();
            Helper.Quat_SetRotation(ref qY, Vector3.UnitY, rot.Z * Constants.RadPerDeg);

            var qZ = new Quaternion();
            Helper.Quat_SetRotation(ref qZ, Vector3.UnitZ, rot.Z * Constants.RadPerDeg);

            v = qZ * qX * qY;
        }

        public static void Mat4_Translate(Transform mat, Vector3 v)
        {
            mat.Origin += mat.Basis.MultiplyByVector(v);
        }

        public static void Mat4_Translate(Transform mat, float x, float y, float z)
        {
            Mat4_Translate(mat, new Vector3(x, y, z));
        }

        public static void Mat4_Scale(Transform mat, float x, float y, float z)
        {
            mat.Basis = mat.Basis.Scaled(new Vector3(x, y, z));
        }

        public static void Mat4_RotateX(Transform mat, float ang)
        {
            var tmp = ang * Constants.RadPerDeg;
            var sina = (float) Math.Sin(tmp);
            var cosa = (float) Math.Cos(tmp);

            var m = Matrix3.Transpose(mat.Basis);
            m.Row1 = mat.Basis.Column1 * cosa + mat.Basis.Column2 * sina;
            m.Row2 = -mat.Basis.Column1 * sina + mat.Basis.Column2 * cosa;

            mat.Basis = Matrix3.Transpose(m);
        }

        public static void Mat4_RotateY(Transform mat, float ang)
        {
            var tmp = ang * Constants.RadPerDeg;
            var sina = (float)Math.Sin(tmp);
            var cosa = (float)Math.Cos(tmp);

            var m = Matrix3.Transpose(mat.Basis);
            m.Row0 = mat.Basis.Column0 * cosa + mat.Basis.Column2 * sina;
            m.Row2 = -mat.Basis.Column0 * sina + mat.Basis.Column2 * cosa;

            mat.Basis = Matrix3.Transpose(m);
        }

        public static void Mat4_RotateZ(Transform mat, float ang)
        {
            var tmp = ang * Constants.RadPerDeg;
            var sina = (float)Math.Sin(tmp);
            var cosa = (float)Math.Cos(tmp);

            var m = Matrix3.Transpose(mat.Basis);
            m.Row0 = mat.Basis.Column0 * cosa + mat.Basis.Column1 * sina;
            m.Row1 = -mat.Basis.Column0 * sina + mat.Basis.Column1 * cosa;

            mat.Basis = Matrix3.Transpose(m);
        }
    }
}
