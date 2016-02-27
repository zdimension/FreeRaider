using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BulletSharp;
using OpenTK;
using OpenTK.Graphics;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int DEBUG_DRAWER_DEFAULT_BUFFER_SIZE = 128 * 1024;

        public const int INIT_FRAME_VERTEX_BUFFER_SIZE = 1024 * 1024;

        public const int STENCIL_FRUSTUM = 1;
    }

    public class RenderDebugDrawer : IDebugDraw
    {


        public void Draw3dText(ref Vector3 location, string textString)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawAabb(ref Vector3 @from, ref Vector3 to, Color4 color)
        {
        }


        public virtual void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle,
            float maxAngle, Color4 color, bool drawSect)
        {
        }

        public virtual void DrawArc(ref Vector3 center, ref Vector3 normal, ref Vector3 axis, float radiusA, float radiusB, float minAngle,
            float maxAngle, Color4 color, bool drawSect, float stepDegrees)
        {
        }

        public virtual void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, ref Matrix4 trans, Color4 color)
        {
        }

        public virtual void DrawBox(ref Vector3 bbMin, ref Vector3 bbMax, Color4 color)
        {
        }

        public virtual void DrawCapsule(float radius, float halfHeight, int upAxis, ref Matrix4 transform, Color4 color)
        {
        }

        public virtual void DrawCone(float radius, float height, int upAxis, ref Matrix4 transform, Color4 color)
        {
        }

        public virtual void DrawContactPoint(ref Vector3 pointOnB, ref Vector3 normalOnB, float distance, int lifeTime, Color4 color)
        {
        }

        public virtual void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix4 transform, Color4 color)
        {
        }

        public void DrawLine(ref Vector3 @from, ref Vector3 to, Color4 color)
        {
            throw new NotImplementedException();
        }

        public virtual void DrawLine(ref Vector3 @from, ref Vector3 to, Color4 fromColor, Color4 toColor)
        {
        }

        public virtual void DrawPlane(ref Vector3 planeNormal, float planeConst, ref Matrix4 transform, Color4 color)
        {
        }

        public virtual void DrawSphere(float radius, ref Matrix4 transform, Color4 color)
        {
        }

        public virtual void DrawSphere(ref Vector3 p, float radius, Color4 color)
        {
        }

        public virtual void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh,
            float minPs, float maxPs, Color4 color)
        {
        }

        public virtual void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh,
            float minPs, float maxPs, Color4 color, float stepDegrees)
        {
        }

        public virtual void DrawSpherePatch(ref Vector3 center, ref Vector3 up, ref Vector3 axis, float radius, float minTh, float maxTh,
            float minPs, float maxPs, Color4 color, float stepDegrees, bool drawCenter)
        {
        }

        public virtual void DrawTransform(ref Matrix4 transform, float orthoLen)
        {
        }

        public virtual void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, Color4 color, float alpha)
        {
        }

        public virtual void DrawTriangle(ref Vector3 v0, ref Vector3 v1, ref Vector3 v2, ref Vector3 __unnamed3, ref Vector3 __unnamed4,
            ref Vector3 __unnamed5, Color4 color, float alpha)
        {
        }

        public virtual void FlushLines()
        {
        }

        public void ReportErrorWarning(string warningString)
        {
            throw new NotImplementedException();
        }

        public virtual DebugDrawModes DebugMode { get; set; }
    }

    public enum TR_ANIMTEXTURE
    {
        Forward = 0,
        Backward = 1,
        Reverse = 2
    }

    public class RenderSettings
    {
        public float LodBias = 0;
        public uint MipmapMode = 3;
        public uint Mipmaps = 3;
        public uint Anisotropy = 0;

        public bool Antialias = false;
        public int AntialiasSamples = 0;

        public int TextureBorder = 8;
        public bool SaveTextureMemory = false;

        public int Zdepth = 16;

        public bool FogEnabled = true;
        public float[] FogColor = new float[]{0, 0, 0, 1};
        public float FogStart_depth = 10000;
        public float FogEnd_depth = 16000;

        public bool UseGL3 = false;
    }
}
