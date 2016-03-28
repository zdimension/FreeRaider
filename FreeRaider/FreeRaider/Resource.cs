using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using FreeRaider.Loader;
using NLua;
using OpenTK;
using static FreeRaider.Global;
using static FreeRaider.Constants;

namespace FreeRaider
{
    public partial class Constants
    {
        /// <summary>
        /// Here you can specify the way FreeRaider processes room collision -
        /// in a classic TR way (floor data collision) or in a modern way
        /// (derived from actual room mesh).
        /// </summary>
        public const int TR_MESH_ROOM_COLLISION = 0;

        // Metering step and sector size are basic Tomb Raider world metrics.
        // Use these defines at all times, when you're referencing classic TR
        // dimensions and terrain manipulations.

        public const float TR_METERING_STEP = 256.0f;

        public const float TR_METERING_SECTORSIZE = 1024.0f;

        /// <summary>
        /// Wall height is a magical constant which specifies that sector with such
        /// height contains impassable wall.
        /// </summary>
        public const int TR_METERING_WALLHEIGHT = 32512;

        public const int LOG_ANIM_DISPATCHES = 0;
    }

    /// <summary>
    /// Penetration configuration specifies collision type for floor and ceiling
    /// sectors (squares).
    /// </summary>
    public enum TR_PENETRATION_CONFIG : byte
    {
        Solid = 0, // ordinary sector.
        DoorVerticalA = 1, // TR3-5 triangulated door.
        DoorVerticalB = 2, // TR3-5 triangulated door.
        Wall = 3, // wall (0x81 == TR_METERING_WALLHEIGHT)
        Ghost = 4 // no collision.
    }

    /// <summary>
    /// There are two types of diagonal splits - we call them north-east (NE) and
    /// north-west (NW). In case there is no diagonal in sector (TR1-2 classic sector),
    /// then NONE type is used.
    /// </summary>
    public enum TR_SECTOR_DIAGONAL_TYPE
    {
        None = 0,
        NorthEast = 1,
        NorthWest = 2
    }

    public enum TR_ITEM_SKYBOX
    {
        TR2 = 254,
        TR3 = 355,
        TR4 = 459,
        TR5 = 454
    }

    public enum TR_ITEM_LARA
    {
        AlternateTR1 = 5,
        TR3 = 315,
        TR4_5 = 8,
        Joints_TR4_5 = 9
    }

    public partial class StaticFuncs
    {
        public static void Res_GenRBTrees(World world)
        {
            world.EntityTree.Clear();
            world.NextEntityID = 0;
            world.ItemsTree.Clear();
        }

        public static void Res_GenSpritesBuffer(World world)
        {
            foreach (var r in world.Rooms)
                Res_GenRoomSpritesBuffer(r);
        }

        public static void Res_GenRoomSpritesBuffer(Room room);

        public static void Res_GenRoomCollision(World world)
        {
            foreach (var room in world.Rooms)
            {
                // Inbetween polygons array is later filled by loop which scans adjacent
                // sector heightmaps and fills the gaps between them, thus creating inbetween
                // polygon. Inbetweens can be either quad (if all four corner heights are
                // different), triangle (if one corner height is similar to adjacent) or
                // ghost (if corner heights are completely similar). In case of quad inbetween,
                // two triangles are added to collisional trimesh, in case of triangle inbetween,
                // we add only one, and in case of ghost inbetween, we ignore it.

                // Most difficult task with converting floordata collision to trimesh collision is
                // building inbetween polygons which will block out gaps between sector heights.   
                var roomTween = Res_Sector_GenTweens(room);

                // Final step is sending actual sectors to Bullet collision model. We do it here.

                var cshape = BT_CSfromHeightmap(room.Sectors, roomTween, true, true);

                if(cshape != null)
                {
                    var localInertia = Vector3.Zero;
                    var motionState = new DefaultMotionState((Matrix4) room.Transform);
                    room.BtBody = new RigidBody(new RigidBodyConstructionInfo(0.0f, motionState, cshape, localInertia));
                    BtEngineDynamicsWorld.AddRigidBody(room.BtBody, CollisionFilterGroups.AllFilter, CollisionFilterGroups.AllFilter);
                    room.BtBody.UserObject = room.Self;
                    room.BtBody.Restitution = 1.0f;
                    room.BtBody.Friction = 1.0f;
                    room.Self.CollisionType = COLLISION_TYPE.Static; // meshtree
                    room.Self.CollisionShape = COLLISION_SHAPE.Trimesh;
                }
            }
        }

        public static void Res_GenRoomFlipMap(World world)
        {
            // Flipmap count is hardcoded, as no original levels contain such info.
            world.FlipData.Resize(FLIPMAX_MAX_NUMBER);
        }

        public static void Res_GenBaseItems(World world)
        {
            (EngineLua["genBaseItems"] as LuaFunction).Call();

            if(world.ItemsTree.Count > 0)
            {
                Res_EntityToItem(world.ItemsTree);
            }
        }

        public static void Res_GenVBOs(World world)
        {
            foreach (var mesh in world.Meshes)
            {
                if(mesh.Vertices.Count > 0 || mesh.AnimatedVertices.Count > 0)
                {
                    mesh.GenVBO(Renderer);
                }
            }

            foreach (var room in world.Rooms)
            {
                if(room.Mesh != null && (room.Mesh.Vertices.Count > 0 || room.Mesh.AnimatedVertices.Count > 0))
                {
                    room.Mesh.GenVBO(Renderer);
                }
            }
        }

        public static uint Res_Sector_BiggestCorner(ref uint v1, ref uint v2, uint v3, uint v4)
        {
            v1 = v1 > v2 ? v1 : v2;
            v2 = v3 > v4 ? v3 : v4;
            return v1 > v2 ? v1 : v2;
        }

        public static void Res_Sector_SetTweenFloorConfig(SectorTween tween)
        {
            if (tween.FloorCorners[0][2] > tween.FloorCorners[1][2])
            {
                var temp = tween.FloorCorners[0][2];
                tween.FloorCorners[0][2] = tween.FloorCorners[1][2];
                tween.FloorCorners[1][2] = temp;

                temp = tween.FloorCorners[2][2];
                tween.FloorCorners[2][2] = tween.FloorCorners[3][2];
                tween.FloorCorners[3][2] = temp;
            }

            if (tween.FloorCorners[3][2] > tween.FloorCorners[2][2])
            {
                tween.FloorTweenType = SectorTweenType.TwoTriangles;
            }
            else if ((tween.FloorCorners[0][2] != tween.FloorCorners[1][2]) &&
                     (tween.FloorCorners[2][2] != tween.FloorCorners[3][2]))
            {
                tween.FloorTweenType = SectorTweenType.Quad;
            }
            else if (tween.FloorCorners[0][2] != tween.FloorCorners[1][2])
            {
                tween.FloorTweenType = SectorTweenType.TriangleLeft;
            }
            else if (tween.FloorCorners[2][2] != tween.FloorCorners[3][2])
            {
                tween.FloorTweenType = SectorTweenType.TriangleRight;
            }
            else
            {
                tween.FloorTweenType = SectorTweenType.None;
            }
        }

        public static void Res_Sector_SetTweenCeilingConfig(SectorTween tween)
        {
            if (tween.CeilingCorners[0][2] > tween.CeilingCorners[1][2])
            {
                var temp = tween.CeilingCorners[0][2];
                tween.CeilingCorners[0][2] = tween.CeilingCorners[1][2];
                tween.CeilingCorners[1][2] = temp;

                temp = tween.CeilingCorners[2][2];
                tween.CeilingCorners[2][2] = tween.CeilingCorners[3][2];
                tween.CeilingCorners[3][2] = temp;
            }

            if (tween.CeilingCorners[3][2] > tween.CeilingCorners[2][2])
            {
                tween.CeilingTweenType = SectorTweenType.TwoTriangles;
            }
            else if ((tween.CeilingCorners[0][2] != tween.CeilingCorners[1][2]) &&
                     (tween.CeilingCorners[2][2] != tween.CeilingCorners[3][2]))
            {
                tween.CeilingTweenType = SectorTweenType.Quad;
            }
            else if (tween.CeilingCorners[0][2] != tween.CeilingCorners[1][2])
            {
                tween.CeilingTweenType = SectorTweenType.TriangleLeft;
            }
            else if (tween.CeilingCorners[2][2] != tween.CeilingCorners[3][2])
            {
                tween.CeilingTweenType = SectorTweenType.TriangleRight;
            }
            else
            {
                tween.CeilingTweenType = SectorTweenType.None;
            }
        }

        public static int Res_Sector_IsWall(RoomSector ws, RoomSector ns)
        {
            if(ws.PortalToRoom < 0 && ns.PortalToRoom < 0 && ws.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Wall)
            {
                return 1;
            }

            if(ns.PortalToRoom < 0 && ns.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall && ws.PortalToRoom >= 0)
            {
                ws = ws.CheckPortalPointer();
                if(ws.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Wall || !ns.IsTwoSidePortal(ws))
                {
                    return 1;
                }
            }

            return 0;
        }

        public static List<SectorTween> Res_Sector_GenTweens(Room room); 

        public static void Res_Sector_FixHeights(RoomSector sector);

        public static bool Res_Poly_SetAnimTexture(Polygon polygon, uint texIndex, World world);

        /// <summary>
        /// Fix start-up room states.
        /// </summary>
        /// <param name="world"></param>
        public static void Res_FixRooms(World world)
        {
            foreach (var room in world.Rooms)
            {
                if(room.BaseRoom != null)
                {
                    room.Disable(); // Disable current room
                }

                // Isolated rooms may be used for rolling ball trick (for ex., LEVEL4.PHD).
                // Hence, this part is commented.

                /*
                if((r->portal_count == 0) && (world->room_count > 1))
                {
                    Room_Disable(r);
                }
                */
            }
        }

        public static SkeletalModel Res_GetSkybox(World world, Loader.Engine engine_version)
        {
            switch(engine_version)
            {
                case Loader.Engine.TR2:
                    return world.GetModelByID((uint) TR_ITEM_SKYBOX.TR2);

                case Loader.Engine.TR3:
                    return world.GetModelByID((uint)TR_ITEM_SKYBOX.TR3);

                case Loader.Engine.TR4:
                    return world.GetModelByID((uint)TR_ITEM_SKYBOX.TR4);

                case Loader.Engine.TR5:
                    return world.GetModelByID((uint)TR_ITEM_SKYBOX.TR5);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Create entity function from script, if exists.
        /// </summary>
        /// <param name="ent"></param>
        public static void Res_SetEntityFunction(Entity ent);

        public static void Res_CreateEntityFunc(Script.ScriptEngine lua, string func_name, int entity_id);

        public static void Res_GenEntityFunctions(Dictionary<uint, Entity> entities)
        {
            foreach(var pair in entities)
                Res_SetEntityFunction(pair.Value);
        }

        // Assign pickup functions to previously created base items.

        public static void Res_EntityToItem(Dictionary<uint, BaseItem> map);

        // Functions setting parameters from configuration scripts.

        public static void Res_SetEntityProperties(Entity ent);

        public static void Res_SetStaticMeshProperties(StaticMesh r_static);

        // Check if entity index was already processed (needed to remove dublicated activation calls).
        // If entity is not processed, add its index into lookup table.

        public static bool Res_IsEntityProcessed(int[] lookup_table, ushort entity_index)
        {
            // Fool-proof check for entity existence. Fixes LOTS of stray non-existent
            // entity #256 occurences in original games (primarily TR4-5).

            if (EngineWorld.GetEntityByID(entity_index) == null) return true;

            var currTableIndex = 0;

            while(lookup_table[currTableIndex] != -1)
            {
                if(lookup_table[currTableIndex] == entity_index) return true;
                if (currTableIndex + 1 == lookup_table.Length) break;
                currTableIndex++;
            }

            lookup_table[currTableIndex] = entity_index;
            return false;
        }

        // Open autoexec.

        public static void Res_AutoexecOpen(TRGame engine_version);

        // Functions generating native OpenTomb structs from legacy TR structs.

        public static void TR_GenWorld(World world, Level tr);

        public static bool SetAnimTexture(Polygon polygon, uint texIndex, World world)
        {
            polygon.AnimID = 0; // Reset to 0 by default.

            for (var i = 0; i < world.AnimSequences.Count; i++)
            {
                for (var j = 0; j < world.AnimSequences[i].Frames.Count; j++)
                {
                    if(world.AnimSequences[i].FrameList[j] == texIndex)
                    {
                        // If we have found assigned texture ID in animation texture lists,
                        // we assign corresponding animation sequence to this polygon,
                        // additionally specifying frame offset.
                        polygon.AnimID = (ushort)(i + 1); // Animation sequence ID.
                        polygon.FrameOffset = (ushort)j; // Animation frame offset.
                        return true;
                    }
                }
            }

            return false; // No such TexInfo found in animation textures lists.
        }

        public static void TR_GenMeshes(World world, Level tr)
        {
            world.Meshes.Resize(tr.Meshes.Length);
            for (var i = 0; i < world.Meshes.Count; i++)
            {
                world.Meshes[i] = new BaseMesh();
                TR_GenMesh(world, i, world.Meshes[i], tr);
            }
        }

        public static void TR_GenMesh(World world, int mesh_index, BaseMesh mesh, Level tr);

        public static void TR_GenSkeletalModels(World world, Level tr)
        {
            world.SkeletalModels.Resize(tr.Moveables.Length);

            for (var i = 0; i < tr.Moveables.Length; i++)
            {
                var tr_moveable = tr.Moveables[i];
                var smodel = new SkeletalModel();
                smodel.ID = tr_moveable.ObjectID;
                smodel.MeshCount = tr_moveable.NumMeshes;
                TR_GenSkeletalModel(world, i, smodel, tr);
                smodel.FillTransparency();
                world.SkeletalModels[i] = smodel;
            }
        }

        public static void TR_GenSkeletalModel(World world, int model_id, SkeletalModel model, Level tr);

        public static void TR_GenEntities(World world, Level tr);

        public static void TR_GenSprites(World world, Level tr)
        {
            if(tr.SpriteTextures.Length == 0)
            {
                world.Sprites.Clear();
                return;
            }

            for (var i = 0; i < tr.SpriteTextures.Length; i++)
            {
                var s = new Sprite();

                var tr_st = tr.SpriteTextures[i];

                s.Left = tr_st.LeftSide;
                s.Right = tr_st.RightSide;
                s.Top = tr_st.TopSide;
                s.Bottom = tr_st.BottomSide;

                world.Sprites.Add(s);

                world.TextureAtlas.GetSpriteCoordinates((uint)i, out s.Texture, s.TexCoord);
            }

            foreach (var seq in tr.SpriteSequences)
            {
                if(seq.Offset.IsBetween(0, world.Sprites.Count - 1))
                {
                    world.Sprites[seq.Offset].ID = (uint)seq.ObjectID;
                }
            }
        }

        public static void TR_GenTextures(World world, Level tr);

        public static void TR_GenAnimCommands(World world, Level tr)
        {
            world.AnimCommands = tr.AnimCommands.ToList();
        }

        public static void TR_GenAnimTextures(World world, Level tr);

        public static void TR_GenRooms(World world, Level tr);

        public static void TR_GenRoom(int room_index, Room room, World world, Level tr);

        public static void TR_GenRoomProperties(World world, Level tr)
        {
            for (var i = 0; i < world.Rooms.Count; i++)
            {
                var r = world.Rooms[i];
                if(r.AlternateRoom != null)
                {
                    r.AlternateRoom.BaseRoom = r; // Refill base room pointer.
                }

                // Fill heightmap and translate floordata.
                foreach (var sector in r.Sectors)
                {
                    TR_Sector_TranslateFloorData(sector, tr);
                    Res_Sector_FixHeights(sector);
                }

                // Generate links to the near rooms.
                r.BuildNearRoomsList();
                // Generate links to the overlapped rooms.
                r.BuildOverlappedRoomsList();

                // Basic sector calculations.
                TR_Sector_Calculate(world, tr, i);
            }
        }

        public static void TR_GenBoxes(World world, Level tr)
        {
            world.RoomBoxes.Clear();

            foreach (var trbox in tr.Boxes)
            {
                world.RoomBoxes.Add(
                    new RoomBox
                    {
                        OverlapIndex = trbox.OverlapIndex,
                        TrueFloor = -trbox.TrueFloor,
                        Xmin = (int) trbox.Xmin,
                        Xmax = (int) trbox.Xmax,
                        Ymin = -(int) trbox.Zmax,
                        Ymax = -(int) trbox.Zmin
                    });
            }
        }

        public static void TR_GenCameras(World world, Level tr)
        {
            world.CamerasSinks.Clear();

            foreach(var trcam in tr.Cameras)
            {
                world.CamerasSinks.Add(
                    new StatCameraSink
                    {
                        X = trcam.Position.X,
                        Y = trcam.Position.Y,
                        Z = -trcam.Position.Z,
                        RoomOrStrength = (ushort)trcam.Room,
                        FlagOrZone = trcam.Flag
                    });
            }
        }

        public static void TR_GenSamples(World world, Level tr);

        // Helper functions to convert legacy TR structs to native OpenTomb structs.

        public static Vector3 TR_vertex_to_arr(Loader.Vertex tr_v)
        {
            return new Vector3(tr_v.X, -tr_v.Z, tr_v.Y);
        }

        /// <summary>
        /// v[] of length 4
        /// </summary>
        public static float[] TR_color_to_arr(FloatColor tr_c)
        {
            return new[] {tr_c.R * 2, tr_c.G * 2, tr_c.B * 2, tr_c.A * 2};
        }

        // Functions for getting various parameters from legacy TR structs.

        public static void TR_GetBFrameBB_Pos(Level tr, int frame_offset, BoneFrame bone_frame)
        {
            if(frame_offset < tr.FrameData.Length)
            {
                var frame = tr.FrameData.Skip(frame_offset).ToArray();

                bone_frame.BBMin[0] = frame[0];
                bone_frame.BBMin[1] = frame[4];
                bone_frame.BBMin[2] = -frame[3];

                bone_frame.BBMax[0] = frame[1];
                bone_frame.BBMax[1] = frame[5];
                bone_frame.BBMax[2] = -frame[2];

                bone_frame.Position[0] = frame[6];
                bone_frame.Position[1] = frame[8];
                bone_frame.Position[2] = -frame[7];
            }
            else
            {
                bone_frame.BBMin = Vector3.Zero;

                bone_frame.BBMax = Vector3.Zero;

                bone_frame.Position = Vector3.Zero;
            }

            bone_frame.Centre = (bone_frame.BBMin - bone_frame.BBMax) / 2.0f;
        }

        public static int TR_GetNumAnimationsForMoveable(Level tr, int moveable_ind)
        {
            int ret;
            var currMoveable = tr.Moveables[moveable_ind];

            if(currMoveable.AnimationIndex == 0xFFFF)
            {
                return 0;
            }

            if(moveable_ind == tr.Moveables.Length - 1)
            {
                ret = tr.Animations.Length - currMoveable.AnimationIndex;
                return ret < 0 ? 1 : ret;
            }

            var nextMoveable = tr.Moveables[moveable_ind + 1];
            if(nextMoveable.AnimationIndex == 0xFFFF)
            {
                if(moveable_ind + 2 < tr.Moveables.Length) // I hope there is no two neighboard movables with animation_index'es == 0xFFFF
                {
                    nextMoveable = tr.Moveables[moveable_ind + 2];
                }
                else
                {
                    return 1;
                }
            }

            ret = Math.Min(nextMoveable.AnimationIndex, tr.Animations.Length);
            ret -= currMoveable.AnimationIndex;

            return ret;
        }

        /// <summary>
        /// Returns real animation frame count
        /// </summary>
        public static int TR_GetNumFramesForAnimation(Level tr, int animation_ind)
        {
            int ret;

            var currAnim = tr.Animations[animation_ind];
            if(currAnim.FrameSize <= 0) // TODO: WTF? byte? < 0 ?
            {
                return 1; // impossible!
            }

            if(animation_ind == tr.Animations.Length - 1)
            {
                ret = (int)(2 * tr.FrameData.Length - currAnim.FrameOffset);
                ret /= currAnim.FrameSize * 2; // it is fully correct!
                return ret;
            }

            var nextAnim = tr.Animations[animation_ind + 1];
            ret = (int)((long)nextAnim.FrameOffset - currAnim.FrameOffset);
            ret /= currAnim.FrameSize * 2;

            return ret;
        }

        public static long TR_GetOriginalAnimationFrameOffset(uint offset, uint anim, Level tr)
        {
            if(anim >= tr.Animations.Length)
            {
                return -1;
            }

            var trAnim = tr.Animations[(int) anim];
            if(anim + 1 == tr.Animations.Length)
            {
                if(offset < trAnim.FrameOffset)
                {
                    return -2;
                }
            }
            else
            {
                if (offset < trAnim.FrameOffset && offset >= tr.Animations[(int)anim + 1].FrameOffset)
                {
                    return -2;
                }
            }

            return trAnim.FrameOffset;
        }

        // Main functions which are used to translate legacy TR floor data
        // to native OpenTomb structs.

        public static int TR_Sector_TranslateFloorData(RoomSector sector, Level tr);

        public static void TR_Sector_Calculate(World world, Level tr, long room_index);

        public static void tr_setupRoomVertices(World world, Level tr, Loader.Room tr_room, BaseMesh mesh,
            int numCorners, ushort[] vertices, ushort masked_texture, Polygon p)
        {
            p.Vertices.Resize(numCorners);

            for (var i = 0; i < numCorners; i++)
            {
                p.Vertices[i].Position = TR_vertex_to_arr(tr_room.Vertices[vertices[i]].Vertex);
            }
            p.FindNormal();

            for (var i = 0; i < numCorners; i++)
            {
                mesh.Vertices[vertices[i]].Normal += p.Plane.Normal;
                p.Vertices[i].Normal = p.Plane.Normal;
                p.Vertices[i].Color = TR_color_to_arr(tr_room.Vertices[vertices[i]].Color);
            }

            var tex = tr.ObjectTextures[masked_texture];
            SetAnimTexture(p, masked_texture, world);
            p.BlendMode = tex.TransparencyFlags;

            world.TextureAtlas.GetCoordinates(masked_texture, false, p);
        }

        public static void tr_copyNormals(Polygon polygon, BaseMesh mesh, ushort[] mesh_vertex_indices)
        {
            for (var i = 0; i < polygon.Vertices.Count; i++)
            {
                polygon.Vertices[i].Normal = mesh.Vertices[mesh_vertex_indices[i]].Normal;
            }
        }

        public static void tr_accumulateNormals(Mesh trMesh, BaseMesh mesh, int numCorners, ushort[] vertexIndices, Polygon p)
        {
            p.Vertices.Resize(numCorners);

            for (var i = 0; i < numCorners; i++)
            {
                p.Vertices[i].Position = TR_vertex_to_arr(trMesh.Vertices[vertexIndices[i]]);
            }
            p.FindNormal();

            for (var i = 0; i < numCorners; i++)
            {
                mesh.Vertices[vertexIndices[i]].Normal += p.Plane.Normal;
            }
        }

        public static void tr_setupColoredFace(Mesh trMesh, Level tr, BaseMesh mesh, ushort[] vertexIndices, int color,
            Polygon p)
        {
            var tmp = trMesh.Lights.Length == trMesh.Vertices.Length;
            for (var i = 0; i < p.Vertices.Count; i++)
            {
                p.Vertices[i].Color[0] = tr.Palette.Colour[color].R / 255.0f;
                p.Vertices[i].Color[1] = tr.Palette.Colour[color].G / 255.0f;
                p.Vertices[i].Color[2] = tr.Palette.Colour[color].B / 255.0f;
                if(tmp)
                {
                    p.Vertices[i].Color[0] = p.Vertices[i].Color[0] * 1.0f - trMesh.Lights[vertexIndices[i]] / 8192.0f;
                    p.Vertices[i].Color[1] = p.Vertices[i].Color[1] * 1.0f - trMesh.Lights[vertexIndices[i]] / 8192.0f;
                    p.Vertices[i].Color[2] = p.Vertices[i].Color[2] * 1.0f - trMesh.Lights[vertexIndices[i]] / 8192.0f;
                }
                p.Vertices[i].Color[3] = 1.0f;

                p.Vertices[i].TexCoord[0] = (i & 2) == 2 ? 1.0f : 0.0f;
                p.Vertices[i].TexCoord[1] = i >= 2 ? 1.0f : 0.0f;
            }
            mesh.UsesVertexColors = true;
        }

        public static void tr_setupTexturedFace(Mesh trMesh, BaseMesh mesh, ushort[] vertexIndices, Polygon p)
        {
            if (trMesh.Lights.Length == trMesh.Vertices.Length)
            {
                for (var i = 0; i < p.Vertices.Count; i++)
                {
                    p.Vertices[i].Color[0] = 1.0f - trMesh.Lights[vertexIndices[i]] / 8192.0f;
                    p.Vertices[i].Color[1] = 1.0f - trMesh.Lights[vertexIndices[i]] / 8192.0f;
                    p.Vertices[i].Color[2] = 1.0f - trMesh.Lights[vertexIndices[i]] / 8192.0f;
                    p.Vertices[i].Color[3] = 1.0f;
                }
            }
            else
            {
                foreach (var v in p.Vertices)
                {
                    v.Color = new float[] {1, 1, 1, 1};
                }
            }
        }
    }
}
