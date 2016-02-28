using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using BulletSharp;
using FreeRaider.Loader;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int DEBUG_DRAWER_DEFAULT_BUFFER_SIZE = 128 * 1024;

        public const int INIT_FRAME_VERTEX_BUFFER_SIZE = 1024 * 1024;

        public const bool STENCIL_FRUSTUM = true;
    }

    public class RenderDebugDrawer : IDebugDraw
    {


        public void Draw3dText(ref Vector3 location, string textString)
        {
            //glRasterPos3f(location.x(),  location.y(),  location.z());
            //BMF_DrawString(BMF_GetFont(BMF_kHelvetica10),textString);
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
            var to = pointOnB + normalOnB * distance;
            DrawLine(ref pointOnB, ref to, color);
        }

        public virtual void DrawCylinder(float radius, float halfHeight, int upAxis, ref Matrix4 transform, Color4 color)
        {
        }

        public void DrawLine(ref Vector3 @from, ref Vector3 to, Color4 color)
        {
            addLine(from.ToArray(), color.ToArray3(), to.ToArray(), color.ToArray3());
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
            ConsoleInfo.Instance.AddLine(warningString, FontStyle.ConsoleWarning);
        }

        public virtual DebugDrawModes DebugMode { get; set; }

        public RenderDebugDrawer()
        {
        }

        ~RenderDebugDrawer()
        {
        }

        public bool IsEmpty => buffer.Count == 0;

        public void Reset()
        {
            buffer.Clear();
        }

        public void Render()
        {
            if(buffer.Any())
            {
                if (glBuffer == 0)
                {
                    glBuffer = Helper.GenBufferU();
                    var attribs = new[]
                    {
                        new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Position, 3,
                            VertexAttribPointerType.Float, false, glBuffer, Marshal.SizeOf(new float[6]), 0),
                        new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Color, 3,
                            VertexAttribPointerType.Float, false, glBuffer, Marshal.SizeOf(new float[6]), Marshal.SizeOf(new float[3]))
                    };
                    vertexArray = new VertexArray(0, attribs);
                }

                GL.BindBuffer(BufferTarget.ArrayBuffer, glBuffer);
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)Marshal.SizeOf(buffer), IntPtr.Zero, BufferUsageHint.StreamDraw);

                unsafe
                {
                    var data = (float*) GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);
                    var b = buffer.ToArray();
                    fixed (float* buf = b[0])
                        Helper.PointerCopy(buf, data, 3);
                    GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                }

                vertexArray.Bind();
                GL.DrawArrays(PrimitiveType.Lines, 0, buffer.Count / 2);
            }

            Helper.FillArray(0.0f, color);
            buffer.Clear();
        }

        public void SetColor(float r, float g, float b)
        {
            color[0] = r;
            color[1] = g;
            color[2] = b;
        }

        public void DrawAxis(float r, Transform transform)
        {
            var origin = transform.Origin.ToArray();

            var v = transform.Basis.Column0 * r;
            v += transform.Origin;
            buffer.Add(origin);
            buffer.Add(new[] {1.0f, 0.0f, 0.0f});
            buffer.Add(v.ToArray());
            buffer.Add(new[] {1.0f, 0.0f, 0.0f});

            v = transform.Basis.Column1 * r;
            v += transform.Origin;
            buffer.Add(origin);
            buffer.Add(new[] { 0.0f, 1.0f, 0.0f });
            buffer.Add(v.ToArray());
            buffer.Add(new[] { 0.0f, 1.0f, 0.0f });

            v = transform.Basis.Column2 * r;
            v += transform.Origin;
            buffer.Add(origin);
            buffer.Add(new[] { 0.0f, 0.0f, 1.0f });
            buffer.Add(v.ToArray());
            buffer.Add(new[] { 0.0f, 0.0f, 1.0f });
        }

        public void DrawPortal(Portal p)
        {
            for (var i = 0; i < p.Vertices.Count - 1; i++)
            {
                addLine(p.Vertices[i], p.Vertices[i + 1]);
            }

            addLine(p.Vertices.Last(), p.Vertices.First());
        }

        public void DrawFrustum(Frustum f)
        {
            for (var i = 0; i < f.Vertices.Count - 1; i++)
            {
                addLine(f.Vertices[i], f.Vertices[i + 1]);
            }

            addLine(f.Vertices.Last(), f.Vertices.First());
        }

        public void DrawBBox(Vector3 bbMin, Vector3 bbMax, Transform transform)
        {
            obb.Rebuild(bbMin, bbMax);
            obb.Transform = transform;
            obb.DoTransform();
            DrawOBB(obb);
        }

        public void DrawOBB(OBB obb)
        {
            var p = obb.Polygons;
            addLine(p[0].Vertices[0].Position, p[1].Vertices[0].Position);
            addLine(p[0].Vertices[1].Position, p[1].Vertices[3].Position);
            addLine(p[0].Vertices[2].Position, p[1].Vertices[2].Position);
            addLine(p[0].Vertices[3].Position, p[1].Vertices[1].Position);

            foreach (var po in p)
            {
                for (var j = 0; j < po.Vertices.Count - 1; j++)
                {
                    addLine(po.Vertices[j].Position, po.Vertices[j + 1].Position);
                }
                addLine(po.Vertices.Last().Position, po.Vertices.First().Position);
            }
        }

        public void DrawMeshDebugLines(BaseMesh mesh, Transform transform, List<Vector3> overrideVertices,
            List<Vector3> overrideNormals, Render render)
        {
            if(render.drawNormals)
            {
                SetColor(0.8f, 0.0f, 0.9f);
                if(overrideVertices.Any())
                {
                    for (var i = 0; i < mesh.Vertices.Count; i++)
                    {
                        var ov = overrideVertices[i];
                        var on = overrideNormals[i];

                        var v = transform * ov;
                        buffer.Add(v.ToArray());
                        buffer.Add(color);
                        v += transform.Basis.MultiplyByVector(on) * 128;
                        buffer.Add(v.ToArray());
                        buffer.Add(color);
                    }
                }
                else
                {
                    foreach (var mv in mesh.Vertices)
                    {
                        var v = transform * mv.Position;
                        buffer.Add(v.ToArray());
                        buffer.Add(color);
                        v += transform.Basis.MultiplyByVector(mv.Normal) * 128;
                        buffer.Add(v.ToArray());
                        buffer.Add(color);
                    }
                }
            }
        }

        public void DrawSkeletalModelDebugLines(SSBoneFrame bframe, Transform transform, Render render)
        {
            if(render.drawNormals)
            {
                foreach (var btag in bframe.BoneTags)
                {
                    var tr = transform * btag.FullTransform;
                    DrawMeshDebugLines(btag.MeshBase, tr, new List<Vector3>(), new List<Vector3>(), render);
                }
            }
        }

        public void DrawEntityDebugLines(Entity entity, Render render)
        {
            if(entity.WasRenderedLines || !(render.drawAxis || render.drawNormals || render.drawBoxes) || !entity.Visible)
            {
                return;
            }

            if(render.drawBoxes)
            {
                Global.DebugDrawer.SetColor(0.0f, 0.0f, 1.0f);
                Global.DebugDrawer.DrawOBB(entity.OBB);
            }

            if(render.drawAxis)
            {
                // If this happens, the lines after this will get drawn with random colors. I don't care.
                Global.DebugDrawer.DrawAxis(1000.0f, entity.Transform);
            }

            if(entity.Bf.Animations.Model != null && entity.Bf.Animations.Model.Animations.Any())
            {
                Global.DebugDrawer.DrawSkeletalModelDebugLines(entity.Bf, entity.Transform, render);
            }

            entity.WasRenderedLines = true;
        }

        public void DrawSectorDebugLines(RoomSector rs)
        {
            var bbMin = new Vector3(rs.Position.X - Constants.TR_METERING_SECTORSIZE / 2.0f,
                rs.Position.Y - Constants.TR_METERING_SECTORSIZE / 2.0f, rs.Floor);
            var bbMax = new Vector3(rs.Position.X + Constants.TR_METERING_SECTORSIZE / 2.0f,
               rs.Position.Y + Constants.TR_METERING_SECTORSIZE / 2.0f, rs.Ceiling);

            DrawBBox(bbMin, bbMax, null);
        }

        public void DrawRoomDebugLines(Room room, Render render, Camera cam)
        {
            if(render.drawRoomBoxes)
            {
                Global.DebugDrawer.SetColor(0.0f, 0.1f, 0.9f);
                Global.DebugDrawer.DrawBBox(room.BBMin, room.BBMax, null);
            }

            if(render.drawPortals)
            {
                Global.DebugDrawer.SetColor(0.0f, 0.0f, 0.0f);
                foreach (var p in room.Portals)
                {
                    Global.DebugDrawer.DrawPortal(p);
                }
            }

            if(render.drawFrustums)
            {
                Global.DebugDrawer.SetColor(1.0f, 0.0f, 0.0f);
                foreach (var frus in room.Frustum)
                {
                    Global.DebugDrawer.DrawFrustum(frus);
                }
            }

            if(!render.skipRoom && room.Mesh != null)
            {
                Global.DebugDrawer.DrawMeshDebugLines(room.Mesh, room.Transform, new List<Vector3>(), new List<Vector3>(), render);
            }

            foreach (var sm in room.StaticMesh)
            {
                if(sm.WasRenderedLines != 0 || sm.OBB.IsVisibleInRoom(room, cam) || (sm.Hide && !render.drawDummyStatics))
                {
                    continue;
                }

                if(render.drawBoxes)
                {
                    Global.DebugDrawer.SetColor(0.0f, 1.0f, 0.1f);
                    Global.DebugDrawer.DrawOBB(sm.OBB);
                }

                if(render.drawAxis)
                {
                    Global.DebugDrawer.DrawAxis(1000.0f, sm.Transform);
                }

                Global.DebugDrawer.DrawMeshDebugLines(sm.Mesh, sm.Transform, new List<Vector3>(), new List<Vector3>(), render);

                sm.WasRenderedLines = 1;
            }

            foreach (var cont in room.Containers)
            {
                switch(cont.ObjectType)
                {
                    case OBJECT_TYPE.Entity:
                        var ent = (Entity) cont.Object;
                        if(!ent.WasRenderedLines)
                        {
                            if(ent.OBB.IsVisibleInRoom(room, cam))
                            {
                                Global.DebugDrawer.DrawEntityDebugLines(ent, render);
                            }
                            ent.WasRenderedLines = true;
                        }
                        break;
                }
            }
        }


        private float[] color = {0, 0, 0};

        private List<float[]> buffer = new List<float[]>();

        private OBB obb;

        private void addLine(float[] start, float[] end)
        {
            addLine(start, color, end, color);
        }

        private void addLine(Vector3 start, Vector3 end)
        {
            addLine(start.ToArray(), color, end.ToArray(), color);
        }

        private void addLine(float[] start, float[] startColor, float[] end, float[] endColor)
        {
            buffer.Add(start);
            buffer.Add(startColor);
            buffer.Add(end);
            buffer.Add(endColor);
        }

        private VertexArray vertexArray;

        private uint glBuffer = 0;
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
        public float[] FogColor = {0, 0, 0, 1};
        public float FogStart_depth = 10000;
        public float FogEnd_depth = 16000;

        public bool UseGL3 = false;
    }

    public class Render
    {
        private bool blocked = true;

        private List<Room> renderList = new List<Room>();

        public bool drawWire;
        public bool drawRoomBoxes;
        public bool drawBoxes;
        public bool drawPortals;
        public bool drawFrustums;
        public bool drawNormals;
        public bool drawAxis;
        public bool skipRoom;
        public bool skipStatic;
        public bool skipEntities;
        public bool drawAllModels;
        public bool drawDummyStatics;
        public bool drawColl;
        public bool drawSkybox;
        public bool drawPoints;


        public void CleanList()
        {
            if(World.Character != null)
            {
                World.Character.WasRendered = false;
                World.Character.WasRenderedLines = false;
            }

            foreach (var room in renderList)
            {
                room.Frustum.Clear();
            }

            drawSkybox = false;
            renderList.Clear();
        }

        /// <summary>
        /// Renderer list generation by current world and camera
        /// </summary>
        public void GenWorldList()
        {
            if(World == null)
            {
                return;
            }

            CleanList(); // clear old render list
            Global.DebugDrawer = new RenderDebugDrawer();

            // find room that contains camera
            var currRoom = Room.FindPosCogerrence(Camera.Position, Camera.CurrentRoom);

            Camera.CurrentRoom = currRoom; // set camera's cuttent room pointer
            if(currRoom != null) // camera located in some room
            {
                currRoom.Frustum.Clear(); // room with camera inside has no frustums!
                currRoom.MaxPath = 0;
                AddRoom(currRoom); // room with camera inside adds to the render list immediately
                foreach (var p in currRoom.Portals) // go through all start room portals
                {
                    var lastFrus = Frustum.PortalFrustumIntersect(p, Camera.Frustum, this);
                    if(lastFrus != null)
                    {
                        AddRoom(p.DestRoom); // portal destination room
                        lastFrus.ParentsCount = 1; // created by camera
                        ProcessRoom(p, lastFrus);// next start reccursion algorithm
                    }
                }
            }
            else if(Global.ControlStates.NoClip) // camera is out of all rooms AND noclip is on
            {
                foreach (var r in World.Rooms)
                {
                    if(Camera.Frustum.IsAABBVisible(r.BBMin, r.BBMax, Camera))
                    {
                        AddRoom(r);
                    }
                }
            }
        }

        public void DrawList()
        {
            if(World == null)
            {
                return;
            }

            GL.Enable(EnableCap.CullFace);
            GL.Disable(EnableCap.Blend);

            RenderSkyBox(Camera.GLViewProjMat);

            if(World.Character != null)
            {
                RenderEntity(World.Character, Camera.GLViewMat, Camera.GLViewProjMat, Camera.GLProjMat);
                RenderHair(World.Character, Camera.GLViewMat, Camera.GLProjMat);
            }

            // Room rendering
            foreach (var room in renderList)
            {
                RenderRoom(room, Camera.GLViewMat, Camera.GLViewProjMat, Camera.GLProjMat);
            }

            GL.Disable(EnableCap.CullFace);

            foreach (var room in renderList)
            {
                RenderRoomSprites(room, Camera.GLViewMat, Camera.GLProjMat);
            }

            // Now, render transparency polygons
            Global.RenderDynamicBsp = new DynamicBSP();
            // First generate BSP from base room mesh - it has good for start splitter polygons
            foreach (var room in renderList.Where(room => room.Mesh != null && room.Mesh.TransparencyPolygons.Any()))
            {
                Global.RenderDynamicBsp.AddNewPolygonList(room.Mesh.TransparentPolygons, room.Transform,
                    Camera.Frustum, Camera);
            }

            foreach (var room in renderList)
            {
                // Add transparency polygons from static meshes (if they exists)
                foreach (var sm in room.StaticMesh)
                {
                    if(sm.Mesh.TransparentPolygons.Any() && sm.OBB.IsVisibleInRoom(room, Camera))
                    {
                        Global.RenderDynamicBsp.AddNewPolygonList(sm.Mesh.TransparentPolygons, sm.Transform, Camera.Frustum, Camera);
                    }
                }

                // Add transparency polygons from all entities (if they exists)
                foreach (var cont in room.Containers)
                {
                    if(cont.ObjectType == OBJECT_TYPE.Entity)
                    {
                        var ent = (Entity) cont.Object;
                        if (ent.Bf.Animations.Model.TransparencyFlags == Constants.MESH_HAS_TRANSPARENCY && ent.Visible &&
                            ent.OBB.IsVisibleInRoom(room, Camera))
                        {
                            foreach (var bt in ent.Bf.BoneTags)
                            {
                                if (bt.MeshBase.TransparencyPolygons.Any())
                                {
                                    var tr = ent.Transform * bt.FullTransform;
                                    Global.RenderDynamicBsp.AddNewPolygonList(
                                        bt.MeshBase.TransparentPolygons, tr, Camera.Frustum, Camera);
                                }
                            }
                        }
                    }
                }
            }

            if(Global.EngineWorld.Character != null && Global.EngineWorld.Character.Bf.Animations.Model.TransparencyFlags == Constants.MESH_HAS_TRANSPARENCY)
            {
                var ent = Global.EngineWorld.Character;
                foreach (var bt in ent.Bf.BoneTags)
                {
                    if (bt.MeshBase.TransparencyPolygons.Any())
                    {
                        var tr = ent.Transform * bt.FullTransform;
                        Global.RenderDynamicBsp.AddNewPolygonList(
                            bt.MeshBase.TransparentPolygons, tr, Camera.Frustum, Camera);
                    }
                }
            }

            if(Global.RenderDynamicBsp.Root.PolygonsFront.Any())
            {
                var shader = ShaderManager.GetRoomShader(false, false);
                GL.UseProgram(shader.Program);
                GL.Uniform1(shader.Sampler, 0);
                GL.UniformMatrix4(shader.ModelViewProjection, false, ref Camera.GLViewProjMat);
                GL.DepthMask(false);
                GL.Disable(EnableCap.AlphaTest);
                GL.Enable(EnableCap.Blend);
                RenderBSPBackToFront(BlendingMode.Opaque, Global.RenderDynamicBsp.Root, shader);
                GL.DepthMask(true);
                GL.Disable(EnableCap.Blend);
            }
        }

        public void DrawListDebugLines()
        {
            if(World == null || !(drawBoxes || drawRoomBoxes || drawPortals || drawFrustums || drawAxis || drawNormals || drawColl))
            {
                return;
            }

            if(World.Character != null)
            {
                Global.DebugDrawer.DrawEntityDebugLines(World.Character, this);
            }

            // Render world debug information
            if(drawNormals && World.SkyBox != null)
            {
                var tr = new Transform();
                tr.SetIdentity();
                tr.Origin = Camera.Position + World.SkyBox.Animations[0].Frames[0].BoneTags[0].Offset;
                tr.Rotation = World.SkyBox.Animations[0].Frames[0].BoneTags[0].QRotate;
                Global.DebugDrawer.DrawMeshDebugLines(World.SkyBox.MeshTree[0].MeshBase, tr, new List<Vector3>(),
                    new List<Vector3>(), this);
            }

            foreach (var room in renderList)
            {
                Global.DebugDrawer.DrawRoomDebugLines(room, this, Camera);
            }

            if(drawColl)
            {
                Global.BtEngineDynamicsWorld.DebugDrawWorld();
            }

            if(!Global.DebugDrawer.IsEmpty)
            {
                var shader = ShaderManager.GetDebugLineShader();
                GL.UseProgram(shader.Program);
                GL.Uniform1(shader.Sampler, 0);
                GL.UniformMatrix4(shader.ModelViewProjection, false, ref Camera.GLViewProjMat);
                GL.BindTexture(TextureTarget.Texture2D, Global.EngineWorld.Textures.Last());
                GL.PointSize(6.0f);
                GL.LineWidth(3.0f);
                Global.DebugDrawer.Render();
            }
        }

        public void DoShaders()
        {
            ShaderManager = new ShaderManager();
        }

        public void InitGlobals()
        {
            Settings = new RenderSettings();
        }

        public void Init()
        {
            blocked = true;
            Camera = null;

            renderList.Clear();

            World = null;

            drawWire = false;
            drawRoomBoxes = false;
            drawBoxes = false;
            drawPortals = false;
            drawFrustums = false;
            drawNormals = false;
            drawAxis = false;
            skipRoom = false;
            skipStatic = false;
            skipEntities = false;
            drawAllModels = false;
            drawDummyStatics = false;
            drawColl = false;
            drawSkybox = false;
            drawPoints = false;
        }

        public void Empty()
        {
            World = null;

            renderList.Clear();

            ShaderManager = null;
        }

        public bool AddRoom(Room room)
        {
            if(renderList.Contains(room) || !room.Active)
            {
                return false;
            }

            renderList.Add(room);

            if (room.Flags.HasFlagUns(RoomFlag.Skybox))
                drawSkybox = true;

            foreach (var sm in room.StaticMesh)
            {
                sm.WasRendered = 0;
                sm.WasRenderedLines = 0;
            }

            foreach (var cont in room.Containers)
            {
                switch (cont.ObjectType)
                {
                    case OBJECT_TYPE.Entity:
                        ((Entity) cont.Object).WasRendered = false;
                        ((Entity) cont.Object).WasRenderedLines = false;
                        break;
                }
            }

            foreach (var sp in room.Sprites)
            {
                sp.WasRendered = false;
            }

            return true;
        }

        public void SetWorld(World world)
        {
            ResetWorld();
            var listSize = world.Rooms.Count + 128; // magic 128 was added for debug and testing

            /*if(renderList.Count < listSize) // if old list is less than new one requiring
            {
                renderList.Resize(listSize);
            }*/

            World = world;
            drawSkybox = false;
            renderList.Clear();
            renderList.Resize(listSize);

            Camera = Global.EngineCamera;
            Global.EngineCamera.CurrentRoom = null;
        }

        public void ResetWorld()
        {
            World = null;
            renderList.Clear();
        }

        public ShaderManager ShaderManager { get; private set; }

        public Camera Camera { get; set; } = null;

        public World World { get; private set; }

        public RenderSettings Settings { get; private set; }

        public void HideSkyBox()
        {
            drawSkybox = false;
        }

        public void ToggleWireframe()
        {
            drawWire = !drawWire;
        }

        public void ToggleDrawPoints()
        {
            drawPoints = !drawPoints;
        }

        public void ToggleDrawColl()
        {
            drawColl = !drawColl;
        }

        public void ToggleDrawNormals()
        {
            drawNormals = !drawNormals;
        }

        public void ToggleDrawPortals()
        {
            drawPortals = !drawPortals;
        }

        public void ToggleDrawFrustums()
        {
            drawFrustums = !drawFrustums;
        }

        public void ToggleDrawRoomBoxes()
        {
            drawRoomBoxes = !drawRoomBoxes;
        }

        public void ToggleDrawBoxes()
        {
            drawBoxes = !drawBoxes;
        }

        public void ToggleDrawAxis()
        {
            drawAxis = !drawAxis;
        }

        public void ToggleDrawAllModels()
        {
            drawAllModels = !drawAllModels;
        }

        public void ToggleDrawDummyStatics()
        {
            drawDummyStatics = !drawDummyStatics;
        }

        public void ToggleSkipRoom()
        {
            skipRoom = !skipRoom;
        }

        public void RenderEntity(Entity entity, Matrix4 modelViewMatrix, Matrix4 modelViewProjectionMatrix,
            Matrix4 projection)
        {
            if (!drawAllModels && (entity.WasRendered || !entity.Visible)) return;

            // Calculate lighting
            var shader = setupEntityLight(entity, modelViewMatrix, false);

            if(entity.Bf.Animations.Model != null && entity.Bf.Animations.Model.Animations.Any())
            {
                // base frame offset
                if (entity.TypeFlags.HasFlagEx(ENTITY_TYPE.Dynamic))
                {
                    RenderDynamicEntity(shader, entity, modelViewMatrix, modelViewProjectionMatrix);
                    // TODO: where I need to do bf skinning matrices update? this time ragdoll update function calculates these matrices
                    if (entity.Bf.BoneTags[0].MeshSkin != null)
                    {
                        var skinShader = setupEntityLight(entity, modelViewMatrix, true);
                        RenderDynamicEntitySkin(skinShader, entity, modelViewMatrix, projection);
                    }
                }
                else
                {
                    var scaledTransform = (Matrix4) entity.Transform *
                                          Helper.Mat4_Diagonal(entity.Scaling.ToQuat());
                    var subModelView = modelViewMatrix * scaledTransform;
                    var subModelViewProjection = modelViewProjectionMatrix * scaledTransform;
                    RenderSkeletalModel(shader, entity.Bf, subModelView, subModelViewProjection);
                    if(entity.Bf.BoneTags[0].MeshSkin != null)
                    {
                        var skinShader = setupEntityLight(entity, modelViewMatrix, true);
                        RenderDynamicEntitySkin(skinShader, entity, subModelView, projection);
                    }
                }
            }
        }

        public void RenderDynamicEntity(LitShaderDescription shader, Entity entity, Matrix4 modelViewMatrix,
            Matrix4 modelViewProjectionMatrix)
        {
            for (var i = 0; i < entity.Bf.BoneTags.Count; i++)
            {
                var btag = entity.Bf.BoneTags[i];

                var tr = entity.Bt.BtBody[i].GetWorldTransform();
                var mvTransform = modelViewMatrix * tr;

                GL.UniformMatrix4(shader.ModelView, false, ref mvTransform);

                var mvpTransform = modelViewProjectionMatrix * tr;
                GL.UniformMatrix4(shader.ModelViewProjection, false, ref mvpTransform);

                RenderMesh(btag.MeshBase);
                if(btag.MeshSlot != null)
                {
                    RenderMesh(btag.MeshSlot);
                }
            }
        }

        public void RenderDynamicEntitySkin(LitShaderDescription shader, Entity ent, Matrix4 mvMatrix, Matrix4 pMatrix)
        {
            GL.UniformMatrix4(shader.Projection, false, ref pMatrix);

            for (int i = 0; i < ent.Bf.BoneTags.Count; i++)
            {
                var mat0 = mvMatrix * ent.Bt.BtBody[i].GetWorldTransform();
                var tr1 = new Matrix4();
                // Calculate parent transform
                var btag = ent.Bf.BoneTags[i];
                var foundParentTransform = false;
                for (var j = 0; j < ent.Bf.BoneTags.Count; j++)
                {
                    if(ent.Bf.BoneTags[i] == btag.Parent)
                    {
                        tr1 = ent.Bt.BtBody[j].GetWorldTransform();
                        foundParentTransform = true;
                        break;
                    }
                }
                if (!foundParentTransform)
                    tr1 = (Matrix4) ent.Transform;

                var translate = new Transform();
                translate.SetIdentity();
                translate.Origin += btag.Offset;

                var secondTransform = tr1.MultiplyByTransform(translate);
                var mat1 = mvMatrix * secondTransform;

                var transforms = new float[32];
                unsafe
                {
                    Array.Copy(Helper.GetArrayFromPointer(&mat0.Row0.X, 16), transforms, 16);
                    Array.Copy(Helper.GetArrayFromPointer(&mat1.Row0.X, 16), 0, transforms, 16, 16);
                }
                GL.UniformMatrix4(shader.ModelView, 2, false, transforms);

                if (btag.MeshSkin != null)
                {
                    RenderMesh(btag.MeshSkin);
                }
            }
        }

        public void RenderSkeletalModel(LitShaderDescription shader, SSBoneFrame bframe, Matrix4 mvMatrix,
            Matrix4 mvpMatrix)
        {
            foreach (var btag in bframe.BoneTags)
            {
                var mvTransform = mvMatrix.MultiplyByTransform(btag.FullTransform);
                GL.UniformMatrix4(shader.ModelView, false, ref mvTransform);

                var mvpTransform = mvpMatrix.MultiplyByTransform(btag.FullTransform);
                GL.UniformMatrix4(shader.ModelViewProjection, false, ref mvpTransform);

                RenderMesh(btag.MeshBase);
                if (btag.MeshSlot != null)
                {
                    RenderMesh(btag.MeshSlot);
                }
            }
        }

        public void RenderSkeletalModelSkin(LitShaderDescription shader, Entity ent, Matrix4 mvMatrix, Matrix4 pMatrix)
        {
            GL.UniformMatrix4(shader.Projection, false, ref pMatrix);

            foreach (var btag in ent.Bf.BoneTags)
            {
                var transforms = new float[32];
                var mvTransforms = mvMatrix.MultiplyByTransform(btag.FullTransform);
                unsafe
                {
                    Array.Copy(Helper.GetArrayFromPointer(&mvTransforms.Row0.X, 16), transforms, 16);
                }

                // Calculate parent transform
                var parentTransform = btag.Parent == null ? ent.Transform : btag.Parent.FullTransform;

                var translate = new Transform();
                translate.SetIdentity();
                translate.Origin += btag.Offset;

                var secondTransform = parentTransform * translate;

                var mvTransforms2 = mvMatrix.MultiplyByTransform(secondTransform);
                unsafe
                {
                    Array.Copy(Helper.GetArrayFromPointer(&mvTransforms2.Row0.X, 16), 0, transforms, 16, 16);
                }
                GL.UniformMatrix4(shader.ModelView, 2, false, transforms);

                if(btag.MeshSkin != null)
                {
                    RenderMesh(btag.MeshSkin);
                }
            }
        }

        public void RenderHair(Character entity, Matrix4 modelViewMatrix, Matrix4 projection)
        {
            if (entity == null || entity.Hairs.Count == 0) return;

            // Calculate lighting
            var shader = setupEntityLight(entity, modelViewMatrix, true);

            for (var h = 0; h < entity.Hairs.Count; h++)
            {
                // First: Head attachment
                var globalHead = (Matrix4)(entity.Transform * entity.Bf.BoneTags[(int)entity.Hairs[h].OwnerBody].FullTransform);
                var globalAttachment = globalHead.MultiplyByTransform(entity.Hairs[h].OwnerBodyHairRoot);

                var matrixCount = 10;

                var hairModelToGlobalMatrices = Helper.RepeatValue(16, () => new float[matrixCount]);
                unsafe
                {
                    var tmp = modelViewMatrix * globalAttachment;
                    fixed (float* ptr = &hairModelToGlobalMatrices[0][0])
                        Helper.PointerCopy(&tmp.Row0.X, ptr, 16);
                }

                // Then: Individual hair pieces
                for (var i = 0; i < entity.Hairs[h].Elements.Count; i++)
                {
                    Assert.That(i + 1 < matrixCount);
                    /*
                     * Definitions: x_o - as in original file. x_h - as in hair model
                     * (translated)
                     * M_ho - translation matrix. x_g = global position (before modelview)
                     * M_go - global position
                     *
                     * We know:
                     * x_h = M_ho * x_o
                     * x_g = M_go * x_o
                     * We want:
                     * M_hg so that x_g = M_gh * x_m
                     * We have: M_oh, M_g
                     *
                     * x_h = M_ho * x_o => x_o = M_oh^-1 * x_h
                     * x_g = M_go * M_ho^-1 * x_h
                     * (M_ho^-1 = M_oh so x_g = M_go * M_oh * x_h)
                     */

                    var invOriginToHairModel = new Transform();
                    invOriginToHairModel.SetIdentity();
                    // Simplification: Always translation matrix, no invert needed
                    invOriginToHairModel.Origin -= entity.Hairs[h].Elements[i].Position;

                    var globalFromHair =
                        entity.Hairs[h].Elements[i].Body.GetWorldTransform().MultiplyByTransform(invOriginToHairModel);

                    unsafe
                    {
                        var tmp = modelViewMatrix * globalFromHair;
                        fixed (float* ptr = &hairModelToGlobalMatrices[i + 1][0])
                            Helper.PointerCopy(&tmp.Row0.X, ptr, 16);
                    }
                }

                unsafe
                {
                    fixed (float* ptr = &hairModelToGlobalMatrices[0][0])
                        GL.UniformMatrix4(shader.ModelView, entity.Hairs[h].Elements.Count + 1, false, ptr);
                }
                
                GL.UniformMatrix4(shader.Projection, false, ref projection);

                RenderMesh(entity.Hairs[h].Mesh);
            }
        }

        public void RenderSkyBox(Matrix4 modelViewProjectionMatrix)
        {
            if (drawSkybox && World?.SkyBox != null)
            {
                GL.DepthMask(false);
                var tr = new Transform();
                tr.Origin = Camera.Position + World.SkyBox.Animations[0].Frames[0].BoneTags[0].Offset;
                tr.Rotation = World.SkyBox.Animations[0].Frames[0].BoneTags[0].QRotate;
                var fullView = modelViewProjectionMatrix.MultiplyByTransform(tr);

                var shader = ShaderManager.GetStaticMeshShader();
                GL.UseProgram(shader.Program);
                GL.UniformMatrix4(shader.ModelViewProjection, false, ref fullView);
                GL.Uniform1(shader.Sampler, 0);
                var tint = new float[] {1, 1, 1, 1};
                GL.Uniform4(shader.TintMult, 1, tint);

                RenderMesh(World.SkyBox.MeshTree[0].MeshBase);
                GL.DepthMask(true);
            }
        }

        public void RenderMesh(BaseMesh mesh)
        {
            if(mesh.AllAnimatedElements.Any())
            {
                // Respecify the tex coord buffer
                GL.BindBuffer(BufferTarget.ArrayBuffer, mesh.AnimatedVBOTexCoordArray);
                // Tell OpenGL to discard the old values
                GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(mesh.AnimatedVertices.Count * Marshal.SizeOf(new float[2])), IntPtr.Zero, BufferUsageHint.StreamDraw);

                unsafe
                {
                    // Get writable data (to avoid copy)
                    var data = (float*) GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);

                    var offset = 0;
                    foreach (var p in mesh.Polygons)
                    {
                        if(p.AnimID == 0 || p.IsBroken)
                        {
                            continue;
                        }

                        var seq = Global.EngineWorld.AnimSequences[p.AnimID - 1];

                        var frame = (seq.CurrentFrame + p.FrameOffset) % seq.Frames.Count;
                        var tf = seq.Frames[frame];
                        foreach (var vert in p.Vertices)
                        {
                            var v = vert.TexCoord;
                            data[offset + 0] = tf.Mat[0 + 0 * 2] * v[0] + tf.Mat[0 + 1 * 2] * v[1] + tf.Move[0];
                            data[offset + 1] = tf.Mat[1 + 0 * 2] * v[0] + tf.Mat[1 + 1 * 2] * v[1] + tf.Move[1];

                            offset += 2;
                        }
                    }
                    GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                }

                if(mesh.AnimatedElementCount > 0)
                {
                    mesh.AnimatedVertexArray.Bind();

                    GL.BindTexture(TextureTarget.Texture2D, World.Textures[0]);
                    GL.DrawElements(PrimitiveType.Triangles, (int)mesh.AnimatedElementCount, DrawElementsType.UnsignedInt, IntPtr.Zero);
                }
            }

            if(mesh.Vertices.Any())
            {
                mesh.MainVertexArray.Bind();

                var elementsbase = IntPtr.Zero;

                var offset = 0;
                for (var texture = 0; texture < mesh.TexturePageCount; texture++)
                {
                    if(mesh.ElementsPerTexture[texture] == 0)
                    {
                        continue;
                    }

                    GL.BindTexture(TextureTarget.Texture2D, World.Textures[texture]);
                    GL.DrawElements(PrimitiveType.Triangles, (int)mesh.ElementsPerTexture[texture], DrawElementsType.UnsignedInt, elementsbase + offset);
                    offset += (int)mesh.ElementsPerTexture[texture];
                }
            }
        }

        public void RenderPolygonTransparency(BlendingMode currentTransparency, BSPFaceRef bspRef,
            UnlitTintedShaderDescription shader)
        {
            // Blending mode switcher.
            // Note that modes above 2 aren't explicitly used in TR textures, only for
            // internal particle processing. Theoretically it's still possible to use
            // them if you will force type via TRTextur utility.
            var refe = bspRef.Polygon;
            var p = refe.Polygon;
            if(currentTransparency != p.BlendMode)
            {
                currentTransparency = p.BlendMode;
                switch (p.BlendMode)
                {
                    case BlendingMode.Multiply: // Classic PC alpha
                        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.One);
                        break;

                    case BlendingMode.InvertSrc: // Inversion by src (PS darkness) - SAME AS IN TR3-TR5
                        GL.BlendFunc(BlendingFactorSrc.Zero, BlendingFactorDest.OneMinusSrcAlpha);
                        break;

                    case BlendingMode.InvertDst: // Inversion by dest
                        GL.BlendFunc(BlendingFactorSrc.OneMinusSrcColor, BlendingFactorDest.OneMinusSrcAlpha);
                        break;

                    case BlendingMode.Screen: // Screen (smoke, etc.)
                        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.OneMinusSrcAlpha);
                        break;

                    case BlendingMode.AnimatedTexture:
                        GL.BlendFunc(BlendingFactorSrc.One, BlendingFactorDest.Zero);
                        break;
                }
            }

            var mvp = Camera.GLViewProjMat.MultiplyByTransform(bspRef.Transform);

            GL.UniformMatrix4(shader.ModelViewProjection, false, ref mvp);

            refe.UsedVertexArray.Bind();
            GL.BindTexture(TextureTarget.Texture2D, World.Textures[p.TexIndex]);

            GL.DrawElements(PrimitiveType.Triangles, (int)refe.Count, DrawElementsType.UnsignedInt, (int)(sizeof(uint) * refe.FirstIndex));
        }

        public void RenderBSPFrontToBack(BlendingMode currentTransparency, BSPNode root,
            UnlitTintedShaderDescription shader)
        {
            var d = root.Plane.Distance(Global.EngineCamera.Position);

            if(d >= 0)
            {
                if(root.Front != null)
                {
                    RenderBSPFrontToBack(currentTransparency, root.Front, shader);
                }

                foreach (var p in root.PolygonsFront)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }
                foreach (var p in root.PolygonsBack)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }

                if(root.Back != null)
                {
                    RenderBSPFrontToBack(currentTransparency, root.Back, shader);
                }
            }
            else
            {
                if (root.Back != null)
                {
                    RenderBSPFrontToBack(currentTransparency, root.Back, shader);
                }

                foreach (var p in root.PolygonsBack)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }
                foreach (var p in root.PolygonsFront)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }

                if (root.Front != null)
                {
                    RenderBSPFrontToBack(currentTransparency, root.Front, shader);
                }
            }
        }

        public void RenderBSPBackToFront(BlendingMode currentTransparency, BSPNode root,
            UnlitTintedShaderDescription shader)
        {
            var d = root.Plane.Distance(Global.EngineCamera.Position);

            if (d >= 0)
            {
                if (root.Back != null)
                {
                    RenderBSPBackToFront(currentTransparency, root.Back, shader);
                }

                foreach (var p in root.PolygonsBack)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }
                foreach (var p in root.PolygonsFront)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }

                if (root.Front != null)
                {
                    RenderBSPBackToFront(currentTransparency, root.Front, shader);
                }
            }
            else
            {
                if (root.Front != null)
                {
                    RenderBSPBackToFront(currentTransparency, root.Front, shader);
                }

                foreach (var p in root.PolygonsFront)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }
                foreach (var p in root.PolygonsBack)
                {
                    RenderPolygonTransparency(currentTransparency, p, shader);
                }

                if (root.Back != null)
                {
                    RenderBSPBackToFront(currentTransparency, root.Back, shader);
                }
            }
        }

        public void RenderRoom(Room room, Matrix4 modelViewMatrix, Matrix4 modelViewProjectionMatrix, Matrix4 projection)
        {
            var needStencil = false;
            if (Constants.STENCIL_FRUSTUM)
            {
                // start test stencil test code
                if(room.Frustum.Any())
                {
                    needStencil = room.OverlappedRoomList.Any(r => renderList.Contains(r));

                    if(needStencil)
                    {
                        var shader = ShaderManager.GetStencilShader();
                        GL.UseProgram(shader.Program);
                        GL.UniformMatrix4(shader.ModelViewProjection, false, ref Global.EngineCamera.GLViewProjMat);
                        GL.Enable(EnableCap.StencilTest);
                        GL.Clear(ClearBufferMask.StencilBufferBit);
                        GL.StencilFunc(StencilFunction.Never, 1, 0x00);
                        GL.StencilOp(StencilOp.Replace, StencilOp.Keep, StencilOp.Keep);

                        var stencilVBO = Helper.GenBufferU();

                        var attribs = new[]
                        {
                            new VertexArrayAttribute((int) UnlitShaderDescription.VertexAttribs.Position, 3,
                                VertexAttribPointerType.Float, false, stencilVBO, Marshal.SizeOf(new float[3]), 0),
                        };

                        var array = new VertexArray(0, attribs);
                        array.Bind();

                        foreach (var f in room.Frustum)
                        {
                            GL.BindBuffer(BufferTarget.ArrayBuffer, stencilVBO);
                            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)Marshal.SizeOf(f.Vertices.ToArray()), IntPtr.Zero, BufferUsageHint.StreamDraw);

                            unsafe
                            {
                                var v = (float*) GL.MapBuffer(BufferTarget.ArrayBuffer, BufferAccess.WriteOnly);

                                foreach(var it in f.Vertices.AsEnumerable().Reverse())
                                {
                                    *v++ = it.X;
                                    *v++ = it.Y;
                                    *v++ = it.Z;
                                }

                                GL.UnmapBuffer(BufferTarget.ArrayBuffer);
                            }

                            GL.DrawArrays(PrimitiveType.TriangleFan, 0, f.Vertices.Count);
                        }

                        GL.StencilFunc(StencilFunction.Equal, 1, 0xFF);
                        GL.DeleteBuffer(stencilVBO);
                    }
                }
            }

            if(!skipRoom && room.Mesh != null)
            {
                var modelViewProjectionTransform = modelViewProjectionMatrix.MultiplyByTransform(room.Transform);

                var shader = ShaderManager.GetRoomShader(room.LightMode == 1, room.Flags.HasFlagUns(1));

                var tint = new float[4];
                Global.EngineWorld.CalculateWaterTint(tint, true);
                GL.UseProgram(shader.Program);

                GL.Uniform4(shader.TintMult, 1, tint);
                GL.Uniform1(shader.CurrentTick, (DateTime.Now - Process.GetCurrentProcess().StartTime).Ticks);
                GL.Uniform1(shader.Sampler, 0);
                GL.UniformMatrix4(shader.ModelViewProjection, false, ref modelViewProjectionTransform);
                RenderMesh(room.Mesh);
            }

            if(room.StaticMesh.Any())
            {
                GL.UseProgram(ShaderManager.GetStaticMeshShader().Program);
                foreach (var sm in room.StaticMesh)
                {
                    if(sm.WasRendered != 0 || !sm.OBB.IsVisibleInRoom(room, Camera))
                    {
                        continue;
                    }

                    if(sm.Hide && !drawDummyStatics)
                    {
                        continue;
                    }

                    var transform = modelViewProjectionMatrix.MultiplyByTransform(sm.Transform);
                    GL.UniformMatrix4(ShaderManager.GetStaticMeshShader().ModelViewProjection, false, ref transform);

                    var tint = sm.Tint.CloneArr();

                    // If this static mesh is in a water room
                    if(room.Flags.HasFlagUns(RoomFlag.Water))
                    {
                        Global.EngineWorld.CalculateWaterTint(tint, false);
                    }
                    GL.Uniform4(ShaderManager.GetStaticMeshShader().TintMult, 1, tint);
                    RenderMesh(sm.Mesh);
                    sm.WasRendered = 1;
                }
            }

            if(room.Containers.Any())
            {
                foreach (var cont in room.Containers)
                {
                    switch(cont.ObjectType)
                    {
                        case OBJECT_TYPE.Entity:
                            var ent = (Entity) cont.Object;
                            if(!ent.WasRendered)
                            {
                                if(ent.OBB.IsVisibleInRoom(room, Camera))
                                {
                                    RenderEntity(ent, modelViewMatrix, modelViewProjectionMatrix, projection);
                                }
                                ent.WasRendered = true;
                            }
                            break;
                    }
                }
            }

            if(Constants.STENCIL_FRUSTUM)
            {
                if(needStencil)
                {
                    GL.Disable(EnableCap.StencilTest);
                }
            }
        }

        public void RenderRoomSprites(Room room, Matrix4 modelViewMatrix, Matrix4 projectionMatrix)
        {
            if (room.Sprites.Any() && room.SpriteBuffer != null)
            {
                var shader = ShaderManager.GetSpriteShader();
                GL.UseProgram(shader.Program);
                GL.UniformMatrix4(shader.ModelView, false, ref modelViewMatrix);
                GL.UniformMatrix4(shader.Projection, false, ref projectionMatrix);
                GL.Uniform1(shader.Sampler, 0);

                room.SpriteBuffer.Data.Bind();

                var offset = 0;
                for (var texture = 0; texture < room.SpriteBuffer.NumTexturePages; texture++)
                {
                    if (room.SpriteBuffer.ElementCountPerTexture[texture] == 0)
                    {
                        continue;
                    }

                    GL.BindTexture(TextureTarget.Texture2D, World.Textures[texture]);
                    GL.DrawElements(PrimitiveType.Triangles, (int) room.SpriteBuffer.ElementCountPerTexture[texture],
                        DrawElementsType.UnsignedShort, offset * sizeof (short));
                    offset += (int) room.SpriteBuffer.ElementCountPerTexture[texture];
                }
            }
        }

        /// <summary>
        /// The recursion algorithm: go through the rooms with portal-frustum occlusion test.
        /// </summary>
        /// <param name="portal">Portal we entered to the room through</param>
        /// <param name="frus">Frustum that intersects the portal</param>
        /// <returns>Number of added rooms</returns>
        public int ProcessRoom(Portal portal, Frustum frus)
        {
            var destination = portal.DestRoom;
            var current = portal.CurrentRoom;

            if(current == null || !current.Active || destination == null || !destination.Active)
            {
                return 0;
            }

            var ret = 0;
            foreach (var p in destination.Portals)
            {
                if(p.DestRoom != null && p.DestRoom.Active && p.DestRoom != current)
                {
                    // The main function of portal renderer. Here comes the check.
                    var genFrus = Frustum.PortalFrustumIntersect(p, frus, this);
                    if(genFrus != null)
                    {
                        ret++;
                        AddRoom(p.DestRoom);
                        ProcessRoom(p, genFrus);
                    }
                }
            }
            return ret;
        }


        private LitShaderDescription setupEntityLight(Entity entity, Matrix4 modelViewMatrix, bool skin)
        {
            // Calculate lighting
            if(entity.Self?.Room == null)
            {
                var shader = ShaderManager.GetEntityShader(0, skin);
                GL.UseProgram(shader.Program);
                return shader;
            }

            var room = entity.Self.Room;

            var ambientComponent = new[]
            {
                room.AmbientLighting[0],
                room.AmbientLighting[1],
                room.AmbientLighting[2],
                1.0f
            };

            if(room.Flags.HasFlagUns(RoomFlag.Water))
            {
                Global.EngineWorld.CalculateWaterTint(ambientComponent, false);
            }

            var currentLightNumber = 0;

            var positions = new float[Constants.MAX_NUM_LIGHTS * 3];
            var colors = new float[Constants.MAX_NUM_LIGHTS * 4];
            var innerRadiuses = new float[Constants.MAX_NUM_LIGHTS * 1];
            var outerRadiuses = new float[Constants.MAX_NUM_LIGHTS * 1];

            for (var i = 0; i < room.Lights.Count && currentLightNumber < Constants.MAX_NUM_LIGHTS; i++)
            {
                var currentLight = room.Lights[i];

                var xyz = entity.Transform.Origin - currentLight.Position;
                var distance = xyz.Length;

                // Find color
                for (var j = 0; j < 4; j++)
                    colors[currentLightNumber * 4 + i] = Math.Min(Math.Max(currentLight.Colour[i], 0.0f), 1.0f);

                if (room.Flags.HasFlagUns(RoomFlag.Water))
                {
                    unsafe
                    {
                        fixed (float* ptr = colors)
                            Global.EngineWorld.CalculateWaterTint(&ptr[currentLightNumber * 4],
                                false);
                    }
                }

                // Find position
                var tmpPos = modelViewMatrix.MultiplyByQuaternion(currentLight.Position.ToQuat());
                Array.Copy(tmpPos.ToArray(), 0, positions, currentLightNumber * 3, 3);

                // Find fall-off
                if(currentLight.LightType == LightType.Sun)
                {
                    innerRadiuses[currentLightNumber] = 1e20f;
                    outerRadiuses[currentLightNumber] = 1e21f;
                    currentLightNumber++;
                }
                else if (distance <= currentLight.Outer + 1024.0f &&
                         (currentLight.LightType == LightType.Point || currentLight.LightType == LightType.Shadow))
                {
                    innerRadiuses[currentLightNumber] = Math.Abs(currentLight.Inner);
                    outerRadiuses[currentLightNumber] = Math.Abs(currentLight.Outer);
                    currentLightNumber++;
                }
            }

            {
                var shader = ShaderManager.GetEntityShader(currentLightNumber, skin);
                GL.UseProgram(shader.Program);
                GL.Uniform4(shader.LightAmbient, 1, ambientComponent);
                GL.Uniform4(shader.LightPosition, currentLightNumber, colors);
                GL.Uniform3(shader.LightPosition, currentLightNumber, positions);
                GL.Uniform1(shader.LightInnerRadius, currentLightNumber, innerRadiuses);
                GL.Uniform1(shader.LightOuterRadius, currentLightNumber, outerRadiuses);
                return shader;
            }
        }
    }

    public partial class Global
    {
        public static Render Renderer;

        public static DynamicBSP RenderDynamicBsp;

        public static RenderDebugDrawer DebugDrawer;
    }
}
