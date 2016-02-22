using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace FreeRaider
{
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
        public Vector3 RightDirection = new Vector3(1.0f, 0.0f, 0.0f);

        public Vector3 previousPosition = new Vector3(0.0f, 0.0f, 0.0f);
        public Vector3 angle;

        public Matrix4 glViewMat = new Matrix4();
        public Matrix4 glProjMat = new Matrix4();
        public Matrix4 glViewProjMat = new Matrix4();

        public Plane[] clipPlanes = new Plane[4]; // frustum side clip planes
        public Frustum frustum; // camera frustum structure

        public float distNear = 1;
        public float distFar = 65536;

        public float fov = 75;
        public float aspect = 1;
        public float f;
        public float height;
        public float width;

        public float shakeValue = 0;
        public float shakeTime = 0;

        public TR_CAM_TARG targetDir = TR_CAM_TARG.Front;
        public Room currentRoom = null;

        public void Apply()
        {
            glProjMat = new Matrix4(
                f / aspect, 0, 0, 0,
                0, f, 0, 0,
                0, 0, (distNear + distFar) / (distNear - distFar), -1,
                0, 0, 2 * distNear * distFar / (distNear - distFar), 0
                );

            glViewMat = new Matrix4();
            glViewMat[0, 0] = RightDirection[0];
            glViewMat[1, 0] = RightDirection[1];
            glViewMat[2, 0] = RightDirection[2];

            glViewMat[0, 1] = UpDirection[0];
            glViewMat[1, 1] = UpDirection[1];
            glViewMat[2, 1] = UpDirection[2];

            glViewMat[0, 2] = -ViewDirection[0];
            glViewMat[1, 2] = -ViewDirection[1];
            glViewMat[2, 2] = -ViewDirection[2];

            glViewMat[3, 0] =
                -(glViewMat[0, 0] * Position[0] + glViewMat[1, 0] * Position[1] + glViewMat[2, 0] * Position[2]);
            glViewMat[3, 1] =
                -(glViewMat[0, 1] * Position[0] + glViewMat[1, 1] * Position[1] + glViewMat[2, 1] * Position[2]);
            glViewMat[3, 2] =
                -(glViewMat[0, 2] * Position[0] + glViewMat[1, 2] * Position[1] + glViewMat[2, 2] * Position[2]);

            glViewMat[0, 3] = 0;
            glViewMat[1, 3] = 0;
            glViewMat[2, 3] = 0;
            glViewMat[3, 3] = 1;

            glViewProjMat = glProjMat * glViewMat;
        }

        public void SetFovAspect(float fov, float aspect)
        {
            this.fov = fov;
            this.aspect = aspect;
            f = (float)Math.Tan(fov * Constants.RadPerDeg / 2);
            height = 2.0f * distNear * f;
            width = height * aspect;
            f = 1.0f / f;
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
            shakeValue = power;
            shakeTime = time;
        }

        public void DeltaRotation(Vector3 angles)
        {
            angle += angles;

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
            angle = angles;

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
            var nearViewPoint = ViewDirection * distNear;

            frustum.Normal.Assign(ViewDirection, Position); // Main clipping plane (we don't draw things beyond us).

            // Lower clipping plane vector
            var LU = nearViewPoint - height / 2.0f * UpDirection;
            clipPlanes[2].Assign(RightDirection, LU, Position);

            // Upper clipping plane vector
            LU = nearViewPoint + height / 2.0f * UpDirection;
            clipPlanes[2].Assign(RightDirection, LU, Position);

            // Left clipping plane vector
            LU = nearViewPoint - width / 2.0f * RightDirection;
            clipPlanes[2].Assign(UpDirection, LU, Position);

            // Right clipping plane vector
            LU = nearViewPoint + width / 2.0f * RightDirection;
            clipPlanes[2].Assign(UpDirection, LU, Position);

            var worldNearViewPoint = Position + ViewDirection * distNear;

            // Ensure that normals point outside
            for (var i = 0; i < 4; i++)
            {
                if (clipPlanes[i].Distance(worldNearViewPoint) < 0.0)
                    clipPlanes[i].MirrorNormal();
            }

            // TODO: Assert !frustum.vertices.empty()
            frustum.Vertices[0] = Position + ViewDirection;
        }

        public Camera()
        {
            f = 1.0f / (float) Math.Tan(fov * Constants.RadPerDeg / 2);
            height = 2.0f * distNear / f;
            width = height * aspect;

            frustum.Vertices = new List<Vector3>();
            frustum.Planes = clipPlanes.Take(4).ToList();
        }
    }
}
