using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using static FreeRaider.Constants;

namespace FreeRaider
{
    public partial class Constants
    {
        public const float TR_CAM_MAX_SHAKE_DISTANCE = 8192.0f;

        public const float TR_CAM_DEFAULT_SHAKE_POWER = 100.0f;
    }

    public enum TR_CAM_TARG : sbyte
    {
        Back = 0,
        Front = 1,
        Left = 2,
        Right = 3
    }

    public class Camera
    {
        /// <summary>
        /// Camera position
        /// </summary>
        public Vector3 Position = new Vector3(0.0f, 0.0f, 0.0f);

        /// <summary>
        /// Camera view direction
        /// </summary>
        public Vector3 ViewDirection = new Vector3(0.0f, 0.0f, 1.0f);

        /// <summary>
        /// Up vector
        /// </summary>
        public Vector3 UpDirection = new Vector3(0.0f, 1.0f, 0.0f);

        /// <summary>
        /// Strafe vector
        /// </summary>
        public Vector3 RightDirection { get; private set; } = new Vector3(1.0f, 0.0f, 0.0f);

        public Vector3 PreviousPosition = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 Angle;

        public Matrix4 GLViewMat;
        public Matrix4 GLProjMat;
        public Matrix4 GLViewProjMat;

        public Plane[] ClipPlanes = new Plane[4]; // frustum side clip planes
        public Frustum Frustum; // camera frustum structure

        public float DistNear = 1;
        public float DistFar = 65536;

        public float Fov = 75;
        public float Aspect = 1;
        public float F;
        public float Height;
        public float Width;

        public float ShakeValue;
        public float ShakeTime;

        public TR_CAM_TARG TargetDir = TR_CAM_TARG.Front;
        public Room CurrentRoom = null;

        public void Apply()
        {
            GLProjMat = new Matrix4(
                F / Aspect, 0, 0, 0,
                0, F, 0, 0,
                0, 0, (DistNear + DistFar) / (DistNear - DistFar), -1,
                0, 0, 2 * DistNear * DistFar / (DistNear - DistFar), 0
                );

            GLViewMat = new Matrix4
            {
                [0, 0] = RightDirection[0],
                [1, 0] = RightDirection[1],
                [2, 0] = RightDirection[2],

                [0, 1] = UpDirection[0],
                [1, 1] = UpDirection[1],
                [2, 1] = UpDirection[2],

                [0, 2] = -ViewDirection[0],
                [1, 2] = -ViewDirection[1],
                [2, 2] = -ViewDirection[2]

            };



            GLViewMat[3, 0] =
                -(GLViewMat[0, 0] * Position[0] + GLViewMat[1, 0] * Position[1] + GLViewMat[2, 0] * Position[2]);
            GLViewMat[3, 1] =
                -(GLViewMat[0, 1] * Position[0] + GLViewMat[1, 1] * Position[1] + GLViewMat[2, 1] * Position[2]);
            GLViewMat[3, 2] =
                -(GLViewMat[0, 2] * Position[0] + GLViewMat[1, 2] * Position[1] + GLViewMat[2, 2] * Position[2]);

            GLViewMat[0, 3] = 0;
            GLViewMat[1, 3] = 0;
            GLViewMat[2, 3] = 0;
            GLViewMat[3, 3] = 1;

            GLViewProjMat = GLProjMat * GLViewMat;
        }

        public void SetFovAspect(float fov, float aspect)
        {
            Fov = fov;
            Aspect = aspect;
            F = (float)Math.Tan(fov * RadPerDeg / 2);
            Height = 2.0f * DistNear * F;
            Width = Height * aspect;
            F = 1.0f / F;
        }

        public void MoveAlong(float dist)
        {
            Position += ViewDirection * dist;
        }

        public void MoveStrafe(float dist)
        {
            Position += RightDirection * dist;
        }

        public void MoveVertical(float dist)
        {
            Position += UpDirection * dist;
        }

        public void Shake(float power, float time)
        {
            ShakeValue = power;
            ShakeTime = time;
        }

        public void DeltaRotation(Vector3 angles)
        {
            Angle += angles;

            // Roll
            UpDirection = UpDirection.Rotate(ViewDirection, angles.Z);

            // Pitch
            ViewDirection = ViewDirection.Rotate(UpDirection, angles.X);

            // Yaw
            ViewDirection = ViewDirection.Rotate(RightDirection, angles.Y);
            UpDirection = ViewDirection;
        }

        public void SetRotation(Vector3 angles)
        {
            Angle = angles;

            UpDirection = new Vector3(0, 0, 1);

            ViewDirection = new Vector3(0, 1, 0).Rotate(UpDirection, angles.X);
            RightDirection = new Vector3(1, 0, 0).Rotate(UpDirection, angles.X);

            UpDirection = UpDirection.Rotate(RightDirection, angles.Y);
            ViewDirection = ViewDirection.Rotate(RightDirection, angles.Y);

            RightDirection = RightDirection.Rotate(ViewDirection, angles.Z);
            UpDirection = UpDirection.Rotate(ViewDirection, angles.Z);
        }

        public void RecalcClipPlanes()
        {
            var nearViewPoint = ViewDirection * DistNear;

            Frustum.Normal.Assign(ViewDirection, Position); // Main clipping plane (we don't draw things beyond us).

            // Lower clipping plane vector
            var LU = nearViewPoint - Height / 2.0f * UpDirection;
            ClipPlanes[2].Assign(RightDirection, LU, Position);

            // Upper clipping plane vector
            LU = nearViewPoint + Height / 2.0f * UpDirection;
            ClipPlanes[2].Assign(RightDirection, LU, Position);

            // Left clipping plane vector
            LU = nearViewPoint - Width / 2.0f * RightDirection;
            ClipPlanes[2].Assign(UpDirection, LU, Position);

            // Right clipping plane vector
            LU = nearViewPoint + Width / 2.0f * RightDirection;
            ClipPlanes[2].Assign(UpDirection, LU, Position);

            var worldNearViewPoint = Position + ViewDirection * DistNear;

            // Ensure that normals point outside
            for (var i = 0; i < 4; i++)
            {
                if (ClipPlanes[i].Distance(worldNearViewPoint) < 0.0)
                    ClipPlanes[i].MirrorNormal();
            }

            StaticFuncs.Assert(Frustum.Vertices.Any());
            Frustum.Vertices[0] = Position + ViewDirection;
        }

        public Camera()
        {
            F = 1.0f / (float) Math.Tan(Fov * RadPerDeg / 2);
            Height = 2.0f * DistNear / F;
            Width = Height * Aspect;

            Frustum.Vertices = new List<Vector3>();
            Frustum.Planes = ClipPlanes.Take(4).ToList();
        }
    }

    /// <summary>
    /// Static camera / sink structure.
    /// </summary>
    public class StatCameraSink
    {
        public float X;

        public float Y;

        public float Z;

        /// <summary>
        /// Room for camera, strength for sink.
        /// </summary>
        public ushort RoomOrStrength;

        /// <summary>
        /// Flag for camera, zone for sink.
        /// </summary>
        public ushort FlagOrZone;
    }

    /// <summary>
    /// Flyby camera structure.
    /// </summary>
    public class FlybyCamera
    {
        /// <summary>
        /// Camera position vector
        /// </summary>
        public Vector3 Cam;

        /// <summary>
        /// Target orientation vector
        /// </summary>
        public Vector3 Target;

        public float Fov;

        public float Roll;

        public float Speed;

        /// <summary>
        /// Sequence number to which camera belongs
        /// </summary>
        public uint Sequence;

        /// <summary>
        /// Index in sequence
        /// </summary>
        public uint Index;

        /// <summary>
        /// How much to sit there
        /// </summary>
        public uint Timer;

        public uint RoomID;

        /// <summary>
        /// See TRLE manual
        /// </summary>
        public ushort Flags;
    }
}
