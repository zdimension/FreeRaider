﻿using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using FreeRaider.Loader;
using FreeRaider.Script;
using NLua;
using NLua.Exceptions;
using OpenTK;
using OpenTK.Audio.OpenAL;
using OpenTK.Graphics.OpenGL;
using static FreeRaider.Global;
using static FreeRaider.Constants;
using static FreeRaider.Loader.Constants;

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

        public const bool LOG_ANIM_DISPATCHES = false;
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

        public static void Res_GenRoomSpritesBuffer(Room room)
        {
            // Find the number of different texture pages used and the number of non-null sprites
            var highestTexturePageFound = 0;
            var actualSpritesFound = 0;
            foreach (var sp in room.Sprites)
            {
                if (sp.Sprite != null)
                {
                    actualSpritesFound++;
                    highestTexturePageFound = Math.Max(highestTexturePageFound, (int) sp.Sprite.Texture);
                }
            }
            if (actualSpritesFound == 0)
            {
                room.SpriteBuffer = null;
                return;
            }

            room.SpriteBuffer = new SpriteBuffer();
            room.SpriteBuffer.NumTexturePages = (uint) highestTexturePageFound + 1;
            room.SpriteBuffer.ElementCountPerTexture.Resize((int) room.SpriteBuffer.NumTexturePages);

            // First collect indices on a per-texture basis
            var elementsForTexture = new ushort[highestTexturePageFound + 1][];

            var spriteData = new float[actualSpritesFound * 4 * 7];

            var writeIndex = 0;
            foreach (var roomSprite in room.Sprites)
            {
                if (roomSprite.Sprite != null)
                {
                    var vertexStart = writeIndex;
                    // top right
                    Array.Copy(roomSprite.Position.ToArray(), 0, spriteData, writeIndex * 7 + 0, 3);
                    Array.Copy(roomSprite.Sprite.TexCoord, 0, spriteData, writeIndex * 7 + 3, 2);
                    spriteData[writeIndex * 7 + 5] = roomSprite.Sprite.Right;
                    spriteData[writeIndex * 7 + 6] = roomSprite.Sprite.Top;

                    writeIndex++;

                    // top left
                    Array.Copy(roomSprite.Position.ToArray(), 0, spriteData, writeIndex * 7 + 0, 3);
                    Array.Copy(roomSprite.Sprite.TexCoord, 2, spriteData, writeIndex * 7 + 3, 2);
                    spriteData[writeIndex * 7 + 5] = roomSprite.Sprite.Left;
                    spriteData[writeIndex * 7 + 6] = roomSprite.Sprite.Top;

                    writeIndex++;

                    // bottom left
                    Array.Copy(roomSprite.Position.ToArray(), 0, spriteData, writeIndex * 7 + 0, 3);
                    Array.Copy(roomSprite.Sprite.TexCoord, 4, spriteData, writeIndex * 7 + 3, 2);
                    spriteData[writeIndex * 7 + 5] = roomSprite.Sprite.Left;
                    spriteData[writeIndex * 7 + 6] = roomSprite.Sprite.Bottom;

                    writeIndex++;

                    // bottom right
                    Array.Copy(roomSprite.Position.ToArray(), 0, spriteData, writeIndex * 7 + 0, 3);
                    Array.Copy(roomSprite.Sprite.TexCoord, 4, spriteData, writeIndex * 7 + 3, 2);
                    spriteData[writeIndex * 7 + 5] = roomSprite.Sprite.Right;
                    spriteData[writeIndex * 7 + 6] = roomSprite.Sprite.Bottom;

                    writeIndex++;

                    // Assign indices
                    var texture = (int) roomSprite.Sprite.Texture;
                    var start = room.SpriteBuffer.ElementCountPerTexture[texture];
                    var s = (int) start;
                    var newElementCount = start + 6;
                    room.SpriteBuffer.ElementCountPerTexture[texture] = newElementCount;
                    elementsForTexture[texture] = new ushort[(int) newElementCount];

                    elementsForTexture[texture][s + 0] = (ushort) (vertexStart + 0);
                    elementsForTexture[texture][s + 1] = (ushort) (vertexStart + 1);
                    elementsForTexture[texture][s + 2] = (ushort) (vertexStart + 2);
                    elementsForTexture[texture][s + 3] = (ushort) (vertexStart + 2);
                    elementsForTexture[texture][s + 4] = (ushort) (vertexStart + 3);
                    elementsForTexture[texture][s + 5] = (ushort) (vertexStart + 0);
                }
            }

            var elements = elementsForTexture.SelectMany(x => x).ToArray(); // TODO: Maybe won't work as expected
            elementsForTexture.Clear();

            // Now load into OpenGL
            var arrayBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, arrayBuffer);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr) (7 * sizeof (float) * 4 * actualSpritesFound),
                spriteData, BufferUsageHint.StaticDraw);
            spriteData.Clear();

            var elementBuffer = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, elementBuffer);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr) elements.GetSize(),
                elements, BufferUsageHint.StaticDraw);
            elements.Clear();

            var attribs = new[]
            {
                new VertexArrayAttribute((int) SpriteShaderDescription.VertexAttribs.Position, 3,
                    VertexAttribPointerType.Float, false, (uint) arrayBuffer, 7 * sizeof (float), 0),
                new VertexArrayAttribute((int) SpriteShaderDescription.VertexAttribs.TexCoord, 2,
                    VertexAttribPointerType.Float, false, (uint) arrayBuffer, 7 * sizeof (float), 3 * sizeof (float)),
                new VertexArrayAttribute((int) SpriteShaderDescription.VertexAttribs.CornerOffset, 2,
                    VertexAttribPointerType.Float, false, (uint) arrayBuffer, 7 * sizeof (float), 5 * sizeof (float))
            };

            room.SpriteBuffer.Data = new VertexArray((uint)elementBuffer, 3, attribs);
        }

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
                    var motionState = new DefaultMotionState(((Matrix4) room.Transform).ToBullet());
                    room.BtBody = new RigidBody(new RigidBodyConstructionInfo(0.0f, motionState, cshape, localInertia.ToBullet()));
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
            world.FlipData.Resize(FLIPMAX_MAX_NUMBER, () => new FlipInfo());
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

        public static uint Res_Sector_BiggestCorner(uint v1, uint v2, uint v3, uint v4)
        {
            return Helper.Max(v1, v2, v3, v4);
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

        public static bool Res_Sector_IsWall(RoomSector ws, RoomSector ns)
        {
            if(ws.PortalToRoom < 0 && ns.PortalToRoom < 0 && ws.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Wall)
            {
                return true;
            }

            if(ns.PortalToRoom < 0 && ns.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall && ws.PortalToRoom >= 0)
            {
                ws = ws.CheckPortalPointer();
                if(ws.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Wall || !ns.IsTwoSidePortal(ws))
                {
                    return true;
                }
            }

            return false;
        }

        public static List<SectorTween> Res_Sector_GenTweens(Room room)
        {
            var result = new List<SectorTween>();
            for (var h = 0; h < room.SectorsY - 1; h++)
            {
                for (var w = 0; w < room.SectorsX - 1; w++)
                {
                    var room_tween = new SectorTween();

                    // Init X-plane tween [ | ]

                    var current_heightmap = room.Sectors[w * room.SectorsY + h];
                    var next_heightmap = room.Sectors[w * room.SectorsY + h + 1];
                    byte joinedFloors = 0;
                    byte joinedCeilings = 0;

                    // XY corners coordinates must be calculated from native room sector 
                    room_tween.FloorCorners[0][1] = current_heightmap.FloorCorners[0][1];
                    room_tween.FloorCorners[1][1] = room_tween.FloorCorners[0][1];
                    room_tween.FloorCorners[2][1] = room_tween.FloorCorners[0][1];
                    room_tween.FloorCorners[3][1] = room_tween.FloorCorners[0][1];
                    room_tween.FloorCorners[0][0] = current_heightmap.FloorCorners[0][0];
                    room_tween.FloorCorners[1][0] = room_tween.FloorCorners[0][0];
                    room_tween.FloorCorners[2][0] = current_heightmap.FloorCorners[1][0];
                    room_tween.FloorCorners[3][0] = room_tween.FloorCorners[2][0];
                    room_tween.CeilingCorners[0][1] = current_heightmap.CeilingCorners[0][1];
                    room_tween.CeilingCorners[1][1] = room_tween.CeilingCorners[0][1];
                    room_tween.CeilingCorners[2][1] = room_tween.CeilingCorners[0][1];
                    room_tween.CeilingCorners[3][1] = room_tween.CeilingCorners[0][1];
                    room_tween.CeilingCorners[0][0] = current_heightmap.CeilingCorners[0][0];
                    room_tween.CeilingCorners[1][0] = room_tween.CeilingCorners[0][0];
                    room_tween.CeilingCorners[2][0] = current_heightmap.CeilingCorners[1][0];
                    room_tween.CeilingCorners[3][0] = room_tween.CeilingCorners[2][0];

                    if (w > 0)
                    {
                        if (next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall ||
                            current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                        {
                            if (Res_Sector_IsWall(next_heightmap, current_heightmap))
                            {
                                room_tween.FloorCorners[0][2] = current_heightmap.FloorCorners[0][2];
                                room_tween.FloorCorners[1][2] = current_heightmap.CeilingCorners[0][2];
                                room_tween.FloorCorners[2][2] = current_heightmap.CeilingCorners[1][2];
                                room_tween.FloorCorners[3][2] = current_heightmap.FloorCorners[1][2];
                                Res_Sector_SetTweenFloorConfig(room_tween);
                                room_tween.CeilingTweenType = SectorTweenType.None;
                                joinedFloors = 1;
                                joinedCeilings = 1;
                            }
                            else if (Res_Sector_IsWall(current_heightmap, next_heightmap))
                            {
                                room_tween.FloorCorners[0][2] = next_heightmap.FloorCorners[3][2];
                                room_tween.FloorCorners[1][2] = next_heightmap.CeilingCorners[3][2];
                                room_tween.FloorCorners[2][2] = next_heightmap.CeilingCorners[2][2];
                                room_tween.FloorCorners[3][2] = next_heightmap.FloorCorners[2][2];
                                Res_Sector_SetTweenFloorConfig(room_tween);
                                room_tween.CeilingTweenType = SectorTweenType.None;
                                joinedFloors = 1;
                                joinedCeilings = 1;
                            }
                            else
                            {
                                /************************** SECTION WITH DROPS CALCULATIONS **********************/
                                if (current_heightmap.PortalToRoom < 0 && next_heightmap.PortalToRoom < 0 ||
                                    current_heightmap.IsTwoSidePortal(next_heightmap))
                                {
                                    current_heightmap = current_heightmap.CheckPortalPointer();
                                    next_heightmap = next_heightmap.CheckPortalPointer();
                                    if (current_heightmap.PortalToRoom < 0 && next_heightmap.PortalToRoom < 0 &&
                                        current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall &&
                                        next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                                    {
                                        if (current_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid ||
                                            next_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                                        {
                                            room_tween.FloorCorners[0][2] = current_heightmap.FloorCorners[0][2];
                                            room_tween.FloorCorners[1][2] = next_heightmap.FloorCorners[3][2];
                                            room_tween.FloorCorners[2][2] = next_heightmap.FloorCorners[2][2];
                                            room_tween.FloorCorners[3][2] = current_heightmap.FloorCorners[1][2];
                                            Res_Sector_SetTweenFloorConfig(room_tween);
                                            joinedFloors = 1;
                                        }
                                        if (current_heightmap.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Solid ||
                                            next_heightmap.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                                        {
                                            room_tween.CeilingCorners[0][2] = current_heightmap.CeilingCorners[0][2];
                                            room_tween.CeilingCorners[1][2] = next_heightmap.CeilingCorners[3][2];
                                            room_tween.CeilingCorners[2][2] = next_heightmap.CeilingCorners[2][2];
                                            room_tween.CeilingCorners[3][2] = current_heightmap.CeilingCorners[1][2];
                                            Res_Sector_SetTweenCeilingConfig(room_tween);
                                            joinedCeilings = 1;
                                        }
                                    }
                                }
                            }
                        }

                        current_heightmap = room.Sectors[w * room.SectorsY + h];
                        next_heightmap = room.Sectors[w * room.SectorsY + h + 1];
                        if (joinedFloors == 0 && (current_heightmap.PortalToRoom < 0 || next_heightmap.PortalToRoom < 0))
                        {
                            byte valid = 0;
                            if (next_heightmap.PortalToRoom >= 0 && current_heightmap.SectorAbove != null &&
                                current_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                next_heightmap = next_heightmap.CheckPortalPointer();
                                if (next_heightmap.OwnerRoom.ID == current_heightmap.SectorAbove.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        current_heightmap.SectorAbove.OwnerRoom.GetSectorRaw(next_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == next_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (current_heightmap.PortalToRoom >= 0 && next_heightmap.SectorAbove != null &&
                                next_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                current_heightmap = current_heightmap.CheckPortalPointer();
                                if (current_heightmap.OwnerRoom.ID == next_heightmap.SectorAbove.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        next_heightmap.SectorAbove.OwnerRoom.GetSectorRaw(current_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == current_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (valid == 1 && current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall &&
                                next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                            {
                                room_tween.FloorCorners[0][2] = current_heightmap.FloorCorners[0][2];
                                room_tween.FloorCorners[1][2] = next_heightmap.FloorCorners[3][2];
                                room_tween.FloorCorners[2][2] = next_heightmap.FloorCorners[2][2];
                                room_tween.FloorCorners[3][2] = current_heightmap.FloorCorners[1][2];
                                Res_Sector_SetTweenFloorConfig(room_tween);
                            }
                        }

                        current_heightmap = room.Sectors[w * room.SectorsY + h];
                        next_heightmap = room.Sectors[w * room.SectorsY + h + 1];
                        if (joinedCeilings == 0 &&
                            (current_heightmap.PortalToRoom < 0 || next_heightmap.PortalToRoom < 0))
                        {
                            byte valid = 0;
                            if (next_heightmap.PortalToRoom >= 0 && current_heightmap.SectorBelow != null &&
                                current_heightmap.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                next_heightmap = next_heightmap.CheckPortalPointer();
                                if (next_heightmap.OwnerRoom.ID == current_heightmap.SectorBelow.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        current_heightmap.SectorBelow.OwnerRoom.GetSectorRaw(next_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == next_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (current_heightmap.PortalToRoom >= 0 && next_heightmap.SectorBelow != null &&
                                next_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                current_heightmap = current_heightmap.CheckPortalPointer();
                                if (current_heightmap.OwnerRoom.ID == next_heightmap.SectorBelow.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        next_heightmap.SectorBelow.OwnerRoom.GetSectorRaw(current_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == current_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (valid == 1 && current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall &&
                                next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                            {
                                room_tween.CeilingCorners[0][2] = current_heightmap.CeilingCorners[0][2];
                                room_tween.CeilingCorners[1][2] = next_heightmap.CeilingCorners[3][2];
                                room_tween.CeilingCorners[2][2] = next_heightmap.CeilingCorners[2][2];
                                room_tween.CeilingCorners[3][2] = current_heightmap.CeilingCorners[1][2];
                                Res_Sector_SetTweenCeilingConfig(room_tween);
                            }
                        }
                    }

                    result.Add(room_tween);

                    /*****************************************************************************************************
                     ********************************   CENTRE  OF  THE  ALGORITHM   *************************************
                     *****************************************************************************************************/

                    room_tween = new SectorTween();

                    current_heightmap = room.Sectors[w * room.SectorsY + h];
                    next_heightmap = room.Sectors[(w + 1) * room.SectorsY + h];
                    room_tween.FloorCorners[0][0] = current_heightmap.FloorCorners[1][0];
                    room_tween.FloorCorners[1][0] = room_tween.FloorCorners[0][0];
                    room_tween.FloorCorners[2][0] = room_tween.FloorCorners[0][0];
                    room_tween.FloorCorners[3][0] = room_tween.FloorCorners[0][0];
                    room_tween.FloorCorners[0][1] = current_heightmap.FloorCorners[1][1];
                    room_tween.FloorCorners[1][1] = room_tween.FloorCorners[0][1];
                    room_tween.FloorCorners[2][1] = current_heightmap.FloorCorners[2][1];
                    room_tween.FloorCorners[3][1] = room_tween.FloorCorners[2][1];

                    room_tween.CeilingCorners[0][0] = current_heightmap.CeilingCorners[1][0];
                    room_tween.CeilingCorners[1][0] = room_tween.CeilingCorners[0][0];
                    room_tween.CeilingCorners[2][0] = room_tween.CeilingCorners[0][0];
                    room_tween.CeilingCorners[3][0] = room_tween.CeilingCorners[0][0];
                    room_tween.CeilingCorners[0][1] = current_heightmap.CeilingCorners[1][1];
                    room_tween.CeilingCorners[1][1] = room_tween.CeilingCorners[0][1];
                    room_tween.CeilingCorners[2][1] = current_heightmap.CeilingCorners[2][1];
                    room_tween.CeilingCorners[3][1] = room_tween.CeilingCorners[2][1];

                    joinedFloors = 0;
                    joinedCeilings = 0;

                    if (h > 0)
                    {
                        if (next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall ||
                            current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                        {
                            // Init Y-plane tween  [ - ]
                            if (Res_Sector_IsWall(next_heightmap, current_heightmap))
                            {
                                room_tween.FloorCorners[0][2] = current_heightmap.FloorCorners[1][2];
                                room_tween.FloorCorners[1][2] = current_heightmap.CeilingCorners[1][2];
                                room_tween.FloorCorners[2][2] = current_heightmap.CeilingCorners[2][2];
                                room_tween.FloorCorners[3][2] = current_heightmap.FloorCorners[2][2];
                                Res_Sector_SetTweenFloorConfig(room_tween);
                                room_tween.CeilingTweenType = SectorTweenType.None;
                                joinedFloors = 1;
                                joinedCeilings = 1;
                            }
                            else if (Res_Sector_IsWall(current_heightmap, next_heightmap))
                            {
                                room_tween.FloorCorners[0][2] = next_heightmap.FloorCorners[0][2];
                                room_tween.FloorCorners[1][2] = next_heightmap.CeilingCorners[0][2];
                                room_tween.FloorCorners[2][2] = next_heightmap.CeilingCorners[3][2];
                                room_tween.FloorCorners[3][2] = next_heightmap.FloorCorners[3][2];
                                Res_Sector_SetTweenFloorConfig(room_tween);
                                room_tween.CeilingTweenType = SectorTweenType.None;
                                joinedFloors = 1;
                                joinedCeilings = 1;
                            }
                            else
                            {
                                /************************** BIG SECTION WITH DROPS CALCULATIONS **********************/
                                if (current_heightmap.PortalToRoom < 0 && next_heightmap.PortalToRoom < 0 ||
                                    current_heightmap.IsTwoSidePortal(next_heightmap))
                                {
                                    current_heightmap = current_heightmap.CheckPortalPointer();
                                    next_heightmap = next_heightmap.CheckPortalPointer();
                                    if (current_heightmap.PortalToRoom < 0 && next_heightmap.PortalToRoom < 0 &&
                                        current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall &&
                                        next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                                    {
                                        if (current_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid ||
                                            next_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                                        {
                                            room_tween.FloorCorners[0][2] = current_heightmap.FloorCorners[1][2];
                                            room_tween.FloorCorners[1][2] = next_heightmap.FloorCorners[0][2];
                                            room_tween.FloorCorners[2][2] = next_heightmap.FloorCorners[3][2];
                                            room_tween.FloorCorners[3][2] = current_heightmap.FloorCorners[2][2];
                                            Res_Sector_SetTweenFloorConfig(room_tween);
                                            joinedFloors = 1;
                                        }
                                        if (current_heightmap.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Solid ||
                                            next_heightmap.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                                        {
                                            room_tween.CeilingCorners[0][2] = current_heightmap.CeilingCorners[1][2];
                                            room_tween.CeilingCorners[1][2] = next_heightmap.CeilingCorners[0][2];
                                            room_tween.CeilingCorners[2][2] = next_heightmap.CeilingCorners[3][2];
                                            room_tween.CeilingCorners[3][2] = current_heightmap.CeilingCorners[2][2];
                                            Res_Sector_SetTweenCeilingConfig(room_tween);
                                            joinedCeilings = 1;
                                        }
                                    }
                                }
                            }
                        }

                        current_heightmap = room.Sectors[w * room.SectorsY + h];

                        next_heightmap = room.Sectors[(w + 1) * room.SectorsY + h];
                        if (joinedFloors == 0 && (current_heightmap.PortalToRoom < 0 || next_heightmap.PortalToRoom < 0))
                        {
                            byte valid = 0;
                            if (next_heightmap.PortalToRoom >= 0 && current_heightmap.SectorAbove != null &&
                                current_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                next_heightmap = next_heightmap.CheckPortalPointer();
                                if (next_heightmap.OwnerRoom.ID == current_heightmap.SectorAbove.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        current_heightmap.SectorAbove.OwnerRoom.GetSectorRaw(next_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == next_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (current_heightmap.PortalToRoom >= 0 && next_heightmap.SectorAbove != null &&
                                next_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                current_heightmap = current_heightmap.CheckPortalPointer();
                                if (current_heightmap.OwnerRoom.ID == next_heightmap.SectorAbove.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        next_heightmap.SectorAbove.OwnerRoom.GetSectorRaw(current_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == current_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (valid == 1 && current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall &&
                                next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                            {
                                room_tween.FloorCorners[0][2] = current_heightmap.FloorCorners[1][2];
                                room_tween.FloorCorners[1][2] = next_heightmap.FloorCorners[0][2];
                                room_tween.FloorCorners[2][2] = next_heightmap.FloorCorners[3][2];
                                room_tween.FloorCorners[3][2] = current_heightmap.FloorCorners[2][2];
                                Res_Sector_SetTweenFloorConfig(room_tween);
                            }
                        }

                        current_heightmap = room.Sectors[w * room.SectorsY + h];

                        next_heightmap = room.Sectors[(w + 1) * room.SectorsY + h];
                        if (joinedCeilings == 0 &&
                            (current_heightmap.PortalToRoom < 0 || next_heightmap.PortalToRoom < 0))
                        {
                            byte valid = 0;
                            if (next_heightmap.PortalToRoom >= 0 && current_heightmap.SectorBelow != null &&
                                current_heightmap.CeilingPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                next_heightmap = next_heightmap.CheckPortalPointer();
                                if (next_heightmap.OwnerRoom.ID == current_heightmap.SectorBelow.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        current_heightmap.SectorBelow.OwnerRoom.GetSectorRaw(next_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == next_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (current_heightmap.PortalToRoom >= 0 && next_heightmap.SectorBelow != null &&
                                next_heightmap.FloorPenetrationConfig == TR_PENETRATION_CONFIG.Solid)
                            {
                                current_heightmap = current_heightmap.CheckPortalPointer();
                                if (current_heightmap.OwnerRoom.ID == next_heightmap.SectorBelow.OwnerRoom.ID)
                                {
                                    valid = 1;
                                }
                                if (valid == 0)
                                {
                                    var rs =
                                        next_heightmap.SectorBelow.OwnerRoom.GetSectorRaw(current_heightmap.Position);
                                    if (rs != null && rs.PortalToRoom == current_heightmap.OwnerRoom.ID)
                                    {
                                        valid = 1;
                                    }
                                }
                            }

                            if (valid == 1 && current_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall &&
                                next_heightmap.FloorPenetrationConfig != TR_PENETRATION_CONFIG.Wall)
                            {
                                room_tween.CeilingCorners[0][2] = current_heightmap.CeilingCorners[1][2];
                                room_tween.CeilingCorners[1][2] = next_heightmap.CeilingCorners[0][2];
                                room_tween.CeilingCorners[2][2] = next_heightmap.CeilingCorners[3][2];
                                room_tween.CeilingCorners[3][2] = current_heightmap.CeilingCorners[2][2];
                                Res_Sector_SetTweenCeilingConfig(room_tween);
                            }
                        }
                    }

                    result.Add(room_tween);
                }
            }
            return result;
        }

        public static void Res_Sector_FixHeights(RoomSector sector)
        {
            if(sector.Floor == TR_METERING_WALLHEIGHT)
            {
                sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Wall;
            }
            if(sector.Ceiling == TR_METERING_WALLHEIGHT)
            {
                sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Wall;
            }

            // Fix non-material crevices

            for(var i = 0; i < 4; i++)
            {
                if (sector.CeilingCorners[i].Z == sector.FloorCorners[i].Z)
                    sector.CeilingCorners[i].Z += LARA_HANG_VERTICAL_EPSILON;
            }
        }

        public static void GenerateAnimCommandsTransform(SkeletalModel model)
        {
            if(EngineWorld.AnimCommands.Length == 0)
            {
                return;
            }
            //Sys.DebugLog("anim_transform.txt", "MODEL[{0}]", model.ID);
            for (var anim = 0; anim < model.Animations.Count; anim++)
            {
                if(model.Animations[anim].NumAnimCommands > 255)
                {
                    continue; // If no anim commands or current anim has more than 255 (according to TRosettaStone).
                }

                var af = model.Animations[anim];
                if (af.NumAnimCommands == 0)
                    continue;

                Assert(af.AnimCommand < EngineWorld.AnimCommands.Length);
                var ac = (int) af.AnimCommand;

                for (var i = 0; i < af.NumAnimCommands; i++)
                {
                    var command = EngineWorld.AnimCommands[ac];
                    ac++;
                    switch((TR_ANIMCOMMAND)command)
                    {
                        case TR_ANIMCOMMAND.SetPosition:
                            // This command executes ONLY at the end of animation.
                            af.Frames.Last().Move.X = EngineWorld.AnimCommands[ac + 0]; // x = x
                            af.Frames.Last().Move.Z = -EngineWorld.AnimCommands[ac + 1]; // z = -y
                            af.Frames.Last().Move.Y = EngineWorld.AnimCommands[ac + 2]; // y = z
                            af.Frames.Last().Command |= (ushort)ANIM_CMD.Move;
                            ac += 3;
                            break;

                        case TR_ANIMCOMMAND.JumpDistance:
                            af.Frames.Last().V_Vertical= EngineWorld.AnimCommands[ac + 0];
                            af.Frames.Last().V_Horizontal = -EngineWorld.AnimCommands[ac + 1];
                            af.Frames.Last().Command |= (ushort) ANIM_CMD.Jump;
                            ac += 2;
                            break;

                        case TR_ANIMCOMMAND.EmptyHands:
                            break;

                        case TR_ANIMCOMMAND.Kill:
                            break;

                        case TR_ANIMCOMMAND.PlaySound:
                            ac += 2;
                            break;

                        case TR_ANIMCOMMAND.PlayEffect:
                            switch(EngineWorld.AnimCommands[ac + 1] & 0x3FFF)
                            {
                                case (int)TR_EFFECT.ChangeDirection:
                                    af.Frames.Last().Command |= (ushort) ANIM_CMD.ChangeDirection;
                                    ConsoleInfo.Instance.Printf("ROTATE: anim = {0}, frame = {1} of {2}", anim, EngineWorld.AnimCommands[ac + 0], af.Frames.Count);
                                    break;
                            }
                            ac += 2;
                            break;
                    }
                }
            }
        }

        public static bool TR_IsSectorsIn2SideOfPortal(RoomSector s1, RoomSector s2, Portal p)
        {
            if(s1.Position.X == s2.Position.X && s1.Position.Y != s2.Position.Y && Math.Abs(p.Normal.Normal.Y) > 0.99f)
            {
                var minX = p.Vertices.Min(x => x.X);
                var maxX = p.Vertices.Max(x => x.X);
                var minY = Math.Min(s1.Position.Y, s2.Position.Y);
                var maxY = Math.Max(s1.Position.Y, s2.Position.Y);

                if(s1.Position.X.IsBetween(minX, maxX, IB.aEbE) && p.Centre.Y.IsBetween(minY, maxY, IB.aEbE))
                {
                    return true;
                }
            }
            else if (s1.Position.X != s2.Position.X && s1.Position.Y == s2.Position.Y && Math.Abs(p.Normal.Normal.X) > 0.99f)
            {
                var minY = p.Vertices.Min(x => x.Y);
                var maxY = p.Vertices.Max(x => x.Y);
                var minX = Math.Min(s1.Position.X, s2.Position.X);
                var maxX = Math.Max(s1.Position.X, s2.Position.X);

                if (p.Centre.X.IsBetween(minX, maxX, IB.aEbE) && s1.Position.Y.IsBetween(minY, maxY, IB.aEbE))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Assign animated texture to a polygon.
        /// </summary>
        public static bool Res_Poly_SetAnimTexture(Polygon polygon, uint texIndex, World world)
        {
            polygon.AnimID = 0; // Reset to 0 by default

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
        public static void Res_SetEntityFunction(Entity ent)
        {
            if(ent.Bf.Animations.Model != null)
            {
                var funcName = EngineLua.Call("getEntityFunction", (int) EngineWorld.EngineVersion,
                    ent.Bf.Animations.Model.ID)[0] as string;
                if(funcName != null)
                    Res_CreateEntityFunc(EngineLua, funcName, (int)ent.ID);
            }
        }

        public static void Res_CreateEntityFunc(ScriptEngine lua, string func_name, int entity_id)
        {
            if(((LuaTable)lua["entity_funcs"])[entity_id] == null)
                lua.DoString("entity_funcs[" + entity_id + "] = {}");
            ((LuaFunction) lua[func_name + "_init"]).Call(entity_id);
        }

        public static void Res_GenEntityFunctions(Dictionary<uint, Entity> entities)
        {
            foreach(var pair in entities)
                Res_SetEntityFunction(pair.Value);
        }

        // Assign pickup functions to previously created base items.

        public static void Res_EntityToItem(Dictionary<uint, BaseItem> map)
        {
            foreach (var item in map.Values)
            {
                foreach (var ent in from room in EngineWorld.Rooms
                                    from cont in room.Containers
                                    where cont.ObjectType == OBJECT_TYPE.Entity
                                    select (Entity) cont.Object
                                    into ent
                                    where ent.Bf.Animations.Model.ID == item.WorldModelId
                                    select ent)
                {
                    if (((LuaTable) EngineLua["entity_funcs"])[ent.ID] == null)
                        EngineLua.DoString("entity_funcs[" + ent.ID + "] = {}");
                        //EngineLua.state.NewTable("entity_funcs." + ent.ID);

                    ((LuaFunction) EngineLua["pickup_init"]).Call(ent.ID, item.ID);

                    ent.DisableCollision();
                }
            }
        }

        // Functions setting parameters from configuration scripts.

        public static void Res_SetEntityProperties(Entity ent)
        {
            if(ent.Bf.Animations.Model != null && EngineLua["getEntityModelProperties"] is LuaFunction)
            {
                var tmp = EngineLua.Call("getEntityModelProperties", (int) EngineWorld.EngineVersion,
                    ent.Bf.Animations.Model.ID);
                ent.Self.CollisionType = (COLLISION_TYPE) (int)(tmp[0] ?? 0);
                ent.Self.CollisionShape = (COLLISION_SHAPE) (int)(tmp[1] ?? 0);
                var flg = (ENTITY_TYPE) Convert.ToInt32(tmp[2] ?? 0);

                ent.Visible = !ent.Visible;
                ent.TypeFlags |= flg;
            }
        }

        public static void Res_SetStaticMeshProperties(StaticMesh r_static)
        {
            var tmp = EngineLua.Call("getStaticMeshProperties", r_static.ObjectID);
            var collisionType = (COLLISION_TYPE) tmp[0];    

            if(collisionType > 0)
            {
                var collisionShape = (COLLISION_SHAPE)tmp[1];
                var hide = (bool)tmp[2];

                r_static.Self.CollisionType = collisionType;
                r_static.Self.CollisionShape = collisionShape;
                r_static.Hide = hide;
            }
        }

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

        public static void Res_AutoexecOpen(TRGame engine_version)
        {
            var tempScriptName = Engine.GetAutoexecName(engine_version);

            if(Engine.FileFound(tempScriptName))
            {
                try
                {
                    EngineLua.DoFile(tempScriptName);
                }
                catch(LuaException e)
                {
                    Sys.DebugLog(LUA_LOG_FILENAME, "{0}", e.Message);
                }
            }
        }

        // Functions generating native OpenTomb structs from legacy TR structs.

        public static void TR_GenWorld(World world, Level tr)
        {
            world.EngineVersion = Loader.Helper.GameToEngine(tr.GameVersion);

            Res_AutoexecOpen(tr.GameVersion); // Open and do preload autoexec.
            EngineLua.Call("autoexec_PreLoad");
            Gui.DrawLoadScreen(150);

            Res_GenRBTrees(world);
            Gui.DrawLoadScreen(200);

            TR_GenTextures(world, tr); // a bit slow (1-2sec)
            Gui.DrawLoadScreen(300);

            TR_GenAnimCommands(world, tr);
            Gui.DrawLoadScreen(310);

            TR_GenAnimTextures(world, tr);
            Gui.DrawLoadScreen(320);

            TR_GenMeshes(world, tr); // a bit slow (1sec)
            Gui.DrawLoadScreen(400);

            TR_GenSprites(world, tr);
            Gui.DrawLoadScreen(420);

            TR_GenBoxes(world, tr);
            Gui.DrawLoadScreen(440);

            TR_GenCameras(world, tr);
            Gui.DrawLoadScreen(460);

            TR_GenRooms(world, tr);
            Gui.DrawLoadScreen(500);

            Res_GenRoomFlipMap(world);
            Gui.DrawLoadScreen(520);

            TR_GenSkeletalModels(world, tr); // very slow (10sec)
            Gui.DrawLoadScreen(600);

            TR_GenEntities(world, tr); // very slow (5sec)
            Gui.DrawLoadScreen(650);

            Res_GenBaseItems(world);
            Gui.DrawLoadScreen(680);

            Res_GenSpritesBuffer(world); // Should be done ONLY after TR_GenEntities.
            Gui.DrawLoadScreen(700);

            TR_GenRoomProperties(world, tr);
            Gui.DrawLoadScreen(750);

            Res_GenRoomCollision(world);
            Gui.DrawLoadScreen(800);

#if !NO_AUDIO
            TR_GenSamples(world, tr);
#endif
            Gui.DrawLoadScreen(850);

            world.SkyBox = Res_GetSkybox(world, world.EngineVersion);
            Gui.DrawLoadScreen(860);

            Res_GenEntityFunctions(world.EntityTree); // a bit very slow (2-3sec)
            Gui.DrawLoadScreen(910);

            Res_GenVBOs(world);
            Gui.DrawLoadScreen(950);

            EngineLua.DoFile("scripts/autoexec.lua"); // Postload autoexec.
            EngineLua.Call("autoexec_PostLoad");
            Gui.DrawLoadScreen(960);

            Res_FixRooms(world); // Fix initial room states
            Gui.DrawLoadScreen(970);
        }

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
            world.Meshes.Resize(tr.Meshes.Length, () => new BaseMesh());
            for (var i = 0; i < world.Meshes.Count; i++)
            {
                TR_GenMesh(world, i, world.Meshes[i], tr);
            }
        }

        public static void TR_GenMesh(World world, int mesh_index, BaseMesh mesh, Level tr)
        {
            var texMask = world.EngineVersion == Loader.Engine.TR4 ? TextureIndexMaskTr4 : TextureIndexMask;

            /* TR WAD FORMAT DOCUMENTATION!
             * tr4_face[3,4]_t:
             * flipped texture & 0x8000 (1 bit  ) - horizontal flipping.
             * shape texture   & 0x7000 (3 bits ) - texture sample shape.
             * index texture   & $0FFF  (12 bits) - texture sample index.
             *
             * if bit [15] is set, as in ( texture and $8000 ), it indicates that the texture
             * sample must be flipped horizontally prior to be used.
             * Bits [14..12] as in ( texture and $7000 ), are used to store the texture
             * shape, given by: ( texture and $7000 ) shr 12.
             * The valid values are: 0, 2, 4, 6, 7, as assigned to a square starting from
             * the top-left corner and going clockwise: 0, 2, 4, 6 represent the positions
             * of the square angle of the triangles, 7 represents a quad.
             */

            var trMesh = tr.Meshes[mesh_index];
            mesh.ID = (uint)mesh_index;
            mesh.Center.X = trMesh.Centre.X;
            mesh.Center.Y = trMesh.Centre.Z;
            mesh.Center.Z = trMesh.Centre.Y;
            mesh.Radius = trMesh.CollisionSize;
            mesh.TexturePageCount = world.TextureAtlas.NumAtlasPages + 1;

            mesh.Vertices.Resize(trMesh.Vertices.Length, () => new Vertex());
            for (var i = 0; i < mesh.Vertices.Count; i++)
            {
                mesh.Vertices[i].Position = trMesh.Vertices[i].ToVector3();
                mesh.Vertices[i].Normal = Vector3.Zero; // paranoid
            }
            
            mesh.FindBB();

            mesh.Polygons.Clear();

            // textured triangles
            foreach (var face3 in trMesh.TexturedTriangles)
            {
                var p = new Polygon();

                var tex = tr.ObjectTextures[face3.Texture & texMask];

                p.DoubleSide = face3.Texture >> 15 != 0; // CORRECT, BUT WRONG IN TR3-5

                SetAnimTexture(p, (uint)face3.Texture & texMask, world);

                p.BlendMode = face3.Lighting.HasFlagUns(0x01) ? BlendingMode.Multiply : tex.TransparencyFlags;

                tr_accumulateNormals(trMesh, mesh, 3, face3.Vertices, p);
                tr_setupTexturedFace(trMesh, mesh, face3.Vertices, p);

                world.TextureAtlas.GetCoordinates((uint) face3.Texture & texMask, false, p);

                mesh.Polygons.Add(p);
            }

            // coloured triangles
            foreach (var face3 in trMesh.ColouredTriangles)
            {
                var p = new Polygon();

                var col = face3.Texture & 0xff;
                p.TexIndex = (ushort)world.TextureAtlas.NumAtlasPages;
                p.BlendMode = BlendingMode.Opaque;
                p.AnimID = 0;

                tr_accumulateNormals(trMesh, mesh, 3, face3.Vertices, p);
                tr_setupColoredFace(trMesh, tr, mesh, face3.Vertices, col, p);

                mesh.Polygons.Add(p);
            }

            // textured rectangles
            foreach (var face4 in trMesh.TexturedRectangles)
            {
                var p = new Polygon();

                var tex = tr.ObjectTextures[face4.Texture & texMask];

                p.DoubleSide = face4.Texture >> 15 != 0; // CORRECT, BUT WRONG IN TR3-5

                SetAnimTexture(p, (uint)face4.Texture & texMask, world);

                p.BlendMode = face4.Lighting.HasFlagUns(0x01) ? BlendingMode.Multiply : tex.TransparencyFlags;

                tr_accumulateNormals(trMesh, mesh, 4, face4.Vertices, p);
                tr_setupTexturedFace(trMesh, mesh, face4.Vertices, p);

                world.TextureAtlas.GetCoordinates((uint)face4.Texture & texMask, false, p);

                mesh.Polygons.Add(p);
            }

            // coloured rectangles
            foreach (var face4 in trMesh.ColouredRectangles)
            {
                var p = new Polygon();

                var col = face4.Texture & 0xff;
                p.TexIndex = (ushort)world.TextureAtlas.NumAtlasPages;
                p.BlendMode = BlendingMode.Opaque;
                p.AnimID = 0;

                tr_accumulateNormals(trMesh, mesh, 4, face4.Vertices, p);
                tr_setupColoredFace(trMesh, tr, mesh, face4.Vertices, col, p);

                mesh.Polygons.Add(p);
            }

            // let us normalise normals %)
            foreach (var v in mesh.Vertices)
            {
                v.Normal = v.Normal.SafeNormalize();
            }

            // triangles
            var j = 0;
            for (var i = 0; i < trMesh.TexturedTriangles.Length; i++, j++)
            {
                tr_copyNormals(mesh.Polygons[j], mesh, trMesh.TexturedTriangles[i].Vertices);
            }

            for (var i = 0; i < trMesh.ColouredTriangles.Length; i++, j++)
            {
                tr_copyNormals(mesh.Polygons[j], mesh, trMesh.ColouredTriangles[i].Vertices);
            }

            // triangles
            for (var i = 0; i < trMesh.TexturedRectangles.Length; i++, j++)
            {
                tr_copyNormals(mesh.Polygons[j], mesh, trMesh.TexturedRectangles[i].Vertices);
            }

            for (var i = 0; i < trMesh.ColouredRectangles.Length; i++, j++)
            {
                tr_copyNormals(mesh.Polygons[j], mesh, trMesh.ColouredRectangles[i].Vertices);
            }

            mesh.Vertices.Clear();
            mesh.GenFaces();
            mesh.PolySortInMesh();
        }

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

        public static void TR_GenSkeletalModel(World world, int model_num, SkeletalModel model, Level tr)
        {
            var trMoveable = tr.Moveables[model_num]; // original tr structure

            model.CollisionMap.Resize(model.MeshCount);
            for (ushort i = 0; i < model.MeshCount; i++)
            {
                model.CollisionMap[i] = i;
            }

            model.MeshTree.Resize(model.MeshCount, () => new MeshTreeTag());
            var treeTag = model.MeshTree[0];

            var meshIndex = tr.MeshIndices.SkipEx(trMoveable.StartingMesh);

            for (var k = 0; k < model.MeshCount; k++)
            {
                treeTag = model.MeshTree[k];
                treeTag.MeshBase = world.Meshes[(int) meshIndex[k]];
                treeTag.MeshSkin = null; // PARANOID: I use calloc for tree_tag's
                treeTag.ReplaceAnim = 0x00;
                treeTag.ReplaceMesh = 0x00;
                treeTag.BodyPart = 0x00;
                treeTag.Offset = Vector3.Zero;
                if (k == 0)
                {
                    treeTag.Flag = 0x02;
                }
                else
                {
                    var tr_mesh_tree = tr.MeshTreeData.SkipEx((int) trMoveable.MeshTreeIndex + (k - 1) * 4);
                    treeTag.Flag = (ushort) (tr_mesh_tree[0] & 0xFF);
                    treeTag.Offset.X = tr_mesh_tree[1];
                    treeTag.Offset.Y = tr_mesh_tree[3];
                    treeTag.Offset.Z = -tr_mesh_tree[2];
                }
            }

            /*
             * =================    now, animation loading    ========================
             */

            if (trMoveable.AnimationIndex >= tr.Animations.Length)
            {
                // model has no start offset and any animation
                model.Animations.Resize(1, () => new AnimationFrame());
                model.Animations[0].Frames.Resize(1, () => new BoneFrame());
                var boneFrame = model.Animations[0].Frames[0];

                model.Animations[0].ID = TR_ANIMATION.LaraRun;
                model.Animations[0].NextAnim = null;
                model.Animations[0].NextFrame = 0;
                model.Animations[0].StateChange.Clear();
                model.Animations[0].OriginalFrameRate = 1;

                boneFrame.BoneTags.Resize(model.MeshCount, () => new BoneTag());

                boneFrame.Position = Vector3.Zero;
                boneFrame.Move = Vector3.Zero;
                boneFrame.V_Horizontal = 0.0f;
                boneFrame.V_Vertical = 0.0f;
                boneFrame.Command = 0x00;
                for (var k = 0; k < boneFrame.BoneTags.Count; k++)
                {
                    treeTag = model.MeshTree[k];
                    var boneTag = boneFrame.BoneTags[k];

                    VMath.Vec4_SetTRRotations(ref boneTag.QRotate, Vector3.Zero);
                    boneTag.Offset = treeTag.Offset;
                }
                return;
            }
            //Sys.DebugLog(LOG_FILENAME, "model = {0}, anims = {1}", trMoveable.ObjectID, TR_GetNumAnimationsForMoveable(tr, model_num));
            model.Animations.Resize(Math.Max(1, TR_GetNumAnimationsForMoveable(tr, model_num)),
                () => new AnimationFrame()); // the animation count must be >= 1

            /*
             *   Ok, let us calculate animations;
             *   there is no difficult:
             * - first 9 words are bounding box and frame offset coordinates.
             * - 10's word is a rotations count, must be equal to number of meshes in model.
             *   BUT! only in TR1. In TR2 - TR5 after first 9 words begins next section.
             * - in the next follows rotation's data. one word - one rotation, if rotation is one-axis (one angle).
             *   two words in 3-axis rotations (3 angles). angles are calculated with bit mask.
             */
            for (var i = 0; i < model.Animations.Count; i++)
            {
                var anim = model.Animations[i];
                var trAnimation = tr.Animations[trMoveable.AnimationIndex + i];

                var frameOffset = trAnimation.FrameOffset / 2;
                var l_start = 0x09;

                if (tr.GameVersion.IsAnyOf(TRGame.TR1, TRGame.TR1Demo, TRGame.TR1UnfinishedBusiness))
                {
                    l_start = 0x0A;
                }

                var frameStep = trAnimation.FrameSize;

                anim.ID = (TR_ANIMATION) i;
                anim.OriginalFrameRate = trAnimation.FrameRate;

                anim.SpeedX = trAnimation.Speed;
                anim.AccelX = trAnimation.Acceleration;
                anim.SpeedY = trAnimation.AccelerationLateral;
                anim.AccelY = trAnimation.SpeedLateral; // TODO: Inverted?

                anim.AnimCommand = trAnimation.AnimCommand;
                anim.NumAnimCommands = trAnimation.NumAnimCommands;
                anim.StateID = (TR_STATE) trAnimation.StateID;

                anim.Frames.Resize(TR_GetNumFramesForAnimation(tr, trMoveable.AnimationIndex + i), () => new BoneFrame());

                //Sys.DebugLog(LOG_FILENAME, "Anim[{0}], {1}", trMoveable.AnimationIndex, TR_GetNumFramesForAnimation(tr, trMoveable.AnimationIndex));

                // Parse AnimCommands
                // Max. amount of AnimCommands is 255, larger numbers are considered as 0.
                // See http://evpopov.com/dl/TR4format.html#Animations for details.

                if (anim.NumAnimCommands.IsBetween(0, 255, IB.aEbI))
                {
                    // Calculate current animation anim command block offset.
                    Assert(anim.AnimCommand < world.AnimCommands.Length);
                    unsafe
                    {
                        fixed (short* tmp = &world.AnimCommands[(int) anim.AnimCommand])
                        {
                            var pointer = tmp;
                            for (uint count = 0; count < anim.NumAnimCommands; count++)
                            {
                                var command = *pointer;
                                ++pointer;
                                switch ((TR_ANIMCOMMAND) command)
                                {
                                    case TR_ANIMCOMMAND.PlayEffect:
                                    case TR_ANIMCOMMAND.PlaySound:
                                        // Recalculate absolute frame number to relative.
                                        pointer[0] -= (short) trAnimation.FrameStart;
                                        pointer += 2;
                                        break;

                                    case TR_ANIMCOMMAND.SetPosition:
                                        // Parse through 3 operands.
                                        pointer += 3;
                                        break;

                                    case TR_ANIMCOMMAND.JumpDistance:
                                        // Parse through 2 operands.
                                        pointer += 2;
                                        break;

                                    default:
                                        // All other commands have no operands.
                                        break;
                                }
                            }
                        }
                    }
                }

                if (anim.Frames.Count == 0)
                {
                    // number of animations must be >= 1, because frame contains base model offset
                    anim.Frames.Resize(1, () => new BoneFrame());
                }

                // let us begin to load animations
                foreach (var boneFrame in anim.Frames)
                {
                    boneFrame.BoneTags.Resize(model.MeshCount, () => new BoneTag());
                    boneFrame.Position = Vector3.Zero;
                    boneFrame.Move = Vector3.Zero;
                    TR_GetBFrameBB_Pos(tr, (int) frameOffset, boneFrame);

                    if (frameOffset >= tr.FrameData.Length)
                    {
                        for (var k = 0; k < boneFrame.BoneTags.Count; k++)
                        {
                            treeTag = model.MeshTree[k];
                            var boneTag = boneFrame.BoneTags[k];
                            VMath.Vec4_SetTRRotations(ref boneTag.QRotate, Vector3.Zero);
                            boneTag.Offset = treeTag.Offset;
                        }
                    }
                    else
                    {
                        var l = l_start;
                        ushort temp1, temp2;
                        float ang;

                        for (var k = 0; k < boneFrame.BoneTags.Count; k++)
                        {
                            treeTag = model.MeshTree[k];
                            var boneTag = boneFrame.BoneTags[k];
                            VMath.Vec4_SetTRRotations(ref boneTag.QRotate, Vector3.Zero);
                            boneTag.Offset = treeTag.Offset;

                            switch (tr.GameVersion)
                            {
                                case TRGame.TR1:
                                case TRGame.TR1UnfinishedBusiness:
                                case TRGame.TR1Demo:
                                    temp2 = tr.FrameData[frameOffset + l];
                                    l++;
                                    temp1 = tr.FrameData[frameOffset + l];
                                    l++;
                                    VMath.Vec4_SetTRRotations(ref boneTag.QRotate, new Vector3(
                                        (temp1 & 0x3ff0) >> 4,
                                        -(((temp1 & 0x000f) << 6) | ((temp2 & 0xfc00) >> 10)),
                                        temp2 & 0x03ff) * (360.0f / 1024.0f));
                                    break;

                                default:
                                    temp1 = tr.FrameData[frameOffset + l];
                                    l++;
                                    if (tr.GameVersion >= TRGame.TR4)
                                    {
                                        ang = (temp1 & 0x0fff) * (360.0f / 4096.0f);
                                    }
                                    else
                                    {
                                        ang = (temp1 & 0x03ff) * (360.0f / 1024.0f);
                                    }

                                    switch (temp1 & 0xc000)
                                    {
                                        case 0x4000: // x only
                                            VMath.Vec4_SetTRRotations(ref boneTag.QRotate, new Vector3(ang, 0, 0));
                                            break;

                                        case 0x8000: // y only
                                            VMath.Vec4_SetTRRotations(ref boneTag.QRotate, new Vector3(0, 0, -ang));
                                            break;

                                        case 0xc000: // z only
                                            VMath.Vec4_SetTRRotations(ref boneTag.QRotate, new Vector3(0, ang, 0));
                                            break;

                                        default: // all three
                                            temp2 = tr.FrameData[frameOffset + l];
                                            VMath.Vec4_SetTRRotations(ref boneTag.QRotate, new Vector3(
                                                (temp1 & 0x3ff0) >> 4,
                                                -(((temp1 & 0x000f) << 6) | ((temp2 & 0xfc00) >> 10)),
                                                temp2 & 0x03ff) * (360.0f / 1024.0f));
                                            l++;
                                            break;
                                    }
                                    break;
                            }
                        }
                    }

                    frameOffset += frameStep;
                }
            }

            // Animations interpolation to 1/30 sec like in original. Needed for correct state change works.
            model.InterpolateFrames();
            // state change's loading

            if (LOG_ANIM_DISPATCHES)
            {
                if (model.Animations.Count > 1)
                {
                    Sys.DebugLog(LOG_FILENAME, "MODEL[{0}], anims = {1}", model_num, model.Animations.Count);
                }
            }
            for (var i = 0; i < model.Animations.Count; i++)
            {
                var anim = model.Animations[i];
                anim.StateChange.Clear();

                var trAnimation = tr.Animations[trMoveable.AnimationIndex + i];
                var animId = (trAnimation.NextAnimation - trMoveable.AnimationIndex) & 0x7fff;
                    // this masks out the sign bit
                Assert(animId >= 0);
                if (animId < model.Animations.Count)
                {
                    anim.NextAnim = model.Animations[animId];
                    anim.NextFrame = Math.Max(0,
                        (trAnimation.NextFrame - tr.Animations[trAnimation.NextAnimation].FrameStart) %
                        anim.NextAnim.Frames.Count);

                    if (LOG_ANIM_DISPATCHES)
                    {
                        Sys.DebugLog(LOG_FILENAME, "ANIM[{0}], next_anim = {1}, next_frame = {2}", i,
                            (int) anim.NextAnim.ID,
                            anim.NextFrame);
                    }
                }
                else
                {
                    anim.NextAnim = null;
                    anim.NextFrame = 0;
                }

                anim.StateChange.Clear(); // TODO: Needed?

                if (trAnimation.NumStateChanges > 0 && model.Animations.Count > 1)
                {
                    if (LOG_ANIM_DISPATCHES)
                    {
                        Sys.DebugLog(LOG_FILENAME, "ANIM[{0}], next_anim = {1}, next_frame = {2}", i,
                            anim.NextAnim == null ? -1 : (int) anim.NextAnim.ID, anim.NextFrame);
                    }
                    anim.StateChange.Resize(trAnimation.NumStateChanges, () => new StateChange());

                    for (var j = 0; j < trAnimation.NumStateChanges; j++)
                    {
                        var schP = anim.StateChange[j];
                        var trSch = tr.StateChanges[j + trAnimation.StateChangeOffset];
                        schP.ID = (TR_STATE) trSch.StateID;
                        schP.AnimDispatch.Clear();
                        for (var l = 0; l < trSch.NumAnimDispatches; l++)
                        {
                            var trAdisp = tr.AnimDispatches[trSch.AnimDispatch + l];
                            var nextAnim = trAdisp.NextAnimation & 0x7fff;
                            var nextAnimInd = nextAnim - (trMoveable.AnimationIndex & 0x7fff);
                            if (nextAnimInd < model.Animations.Count)
                            {
                                var adsp = new AnimDispatch();
                                var nextFramesCount =
                                    model.Animations[nextAnim - trMoveable.AnimationIndex].Frames.Count;
                                var nextFrame = trAdisp.NextFrame - tr.Animations[nextAnim].FrameStart;

                                var low = trAdisp.Low - trAnimation.FrameStart;
                                var high = trAdisp.High - trAnimation.FrameStart;

                                adsp.FrameLow = (ushort) (low % anim.Frames.Count);
                                adsp.FrameHigh = (ushort) ((high - 1) % anim.Frames.Count);
                                adsp.NextAnim = (TR_ANIMATION) (nextAnim - trMoveable.AnimationIndex);
                                adsp.NextFrame = (ushort) (nextFrame % nextFramesCount);

                                schP.AnimDispatch.Add(adsp);

                                if (LOG_ANIM_DISPATCHES)
                                {
                                    Sys.DebugLog(LOG_FILENAME,
                                        "anim_disp[{0}], frames.size() = {1}: interval[{2}.. {3}], next_anim = {4}, next_frame = {5}",
                                        l, anim.Frames.Count, adsp.FrameLow, adsp.FrameHigh, (int) adsp.NextAnim,
                                        adsp.NextFrame);
                                }
                            }
                        }
                    }
                }
            }
            GenerateAnimCommandsTransform(model);
        }

        public static void TR_GenEntities(World world, Level tr)
        {
            for (var i = 0; i < tr.Items.Length; i++)
            {
                var trItem = tr.Items[i];
                var entity = trItem.ObjectID == 0 ? new Character((uint) i) : new Entity((uint) i);
                entity.Transform.Origin.X = trItem.Position.X;
                entity.Transform.Origin.Y = -trItem.Position.Z;
                entity.Transform.Origin.Z = trItem.Position.Y;
                entity.Angles.X = trItem.Rotation;
                entity.Angles.Y = 0;
                entity.Angles.Z = 0;
                entity.UpdateTransform();
                entity.Self.Room = trItem.Room.IsBetween(0, world.Rooms.Count - 1) ? world.Rooms[trItem.Room] : null;

                entity.TriggerLayout = (ENTITY_TLAYOUT)trItem.ActivationMash; // FIXME: Ignore INVISIBLE and CLEAR BODY flags for a moment.
                entity.OCB = trItem.ObjectCodeBit;
                entity.Timer = 0.0f;

                entity.Self.CollisionType = COLLISION_TYPE.Kinematic;
                entity.Self.CollisionShape = COLLISION_SHAPE.TrimeshConvex;
                entity.MoveType = MoveType.StaticPos;
                entity.InertiaLinear = 0.0f;
                entity.InertiaAngular = Vector2.Zero;

                entity.Bf.Animations.Model = world.GetModelByID((uint)trItem.ObjectID);

                if(entity.Bf.Animations.Model == null)
                {
                    var id = EngineLua.Call("getOverridedID", Loader.Helper.GameToEngine(tr.GameVersion), trItem.ObjectID)[0];
                    entity.Bf.Animations.Model = world.GetModelByID((uint) id);
                }

                var replaceAnimId = (int)EngineLua.Call("getOverridedAnim", Loader.Helper.GameToEngine(tr.GameVersion), trItem.ObjectID)[0];
                if(replaceAnimId > 0)
                {
                    var replaceAnimModel = world.GetModelByID((uint)replaceAnimId);
                    var tmp = entity.Bf.Animations.Model.Animations;
                    entity.Bf.Animations.Model.Animations = replaceAnimModel.Animations;
                    replaceAnimModel.Animations = tmp;
                }

                if(entity.Bf.Animations.Model == null)
                {
                    // SPRITE LOADING
                    var sp = world.GetSpriteByID((uint)trItem.ObjectID);
                    if(sp != null && entity.Self.Room != null)
                    {
                        var rsp = new RoomSprite();
                        rsp.Sprite = sp;
                        rsp.Position = entity.Transform.Origin;
                        rsp.WasRendered = false;
                        entity.Self.Room.Sprites.Add(rsp);
                    }

                    continue; // that entity has no model. may be it is a some trigger or look at object
                }

                if(tr.GameVersion < TRGame.TR2 && trItem.ObjectID == 83) // FIXME: brutal magick hardcode! ;-)
                {
                    // skip PSX save model
                    continue;
                }

                entity.Bf.FromModel(entity.Bf.Animations.Model);

                if(trItem.ObjectID == 0) // Lara is unical model
                {
                    var lara = (Character) entity;
                    Assert(lara != null);

                    lara.MoveType = MoveType.OnFloor;
                    world.Character = lara;
                    lara.Self.CollisionType = COLLISION_TYPE.Actor;
                    lara.Self.CollisionShape = COLLISION_SHAPE.TrimeshConvex;
                    lara.TypeFlags |= ENTITY_TYPE.TriggerActivator;
                    SkeletalModel LM;

                    EngineLua.Set("player", lara.ID);

                    switch (Loader.Helper.GameToEngine(tr.GameVersion))
                    {
                        case Loader.Engine.TR1:
                            if (GameflowManager.LevelID == 0)
                            {
                                LM = world.GetModelByID((uint) TR_ITEM_LARA.AlternateTR1);
                                if (LM != null)
                                {
                                    // In TR1, Lara has unified head mesh for all her alternate skins.
                                    // Hence, we copy all meshes except head, to prevent Potato Raider bug.
                                    SkeletonCopyMeshes(world.SkeletalModels[0].MeshTree, LM.MeshTree,
                                        world.SkeletalModels[0].MeshCount - 1);
                                }
                            }
                            break;

                        case Loader.Engine.TR3:
                            LM = world.GetModelByID((uint) TR_ITEM_LARA.TR3);
                            if (LM != null)
                            {
                                SkeletonCopyMeshes(world.SkeletalModels[0].MeshTree, LM.MeshTree,
                                    world.SkeletalModels[0].MeshCount);
                                var tmp = world.GetModelByID(11); // moto / quadro cycle animations
                                if (tmp != null)
                                {
                                    SkeletonCopyMeshes(tmp.MeshTree, LM.MeshTree, world.SkeletalModels[0].MeshCount);
                                }
                            }
                            break;

                        case Loader.Engine.TR4:
                        case Loader.Engine.TR5:
                            LM = world.GetModelByID((uint)TR_ITEM_LARA.TR4_5); // base skeleton meshes
                            if (LM != null)
                            {
                                SkeletonCopyMeshes(world.SkeletalModels[0].MeshTree, LM.MeshTree,
                                    world.SkeletalModels[0].MeshCount);
                            }
                            LM = world.GetModelByID((uint)TR_ITEM_LARA.Joints_TR4_5); // skin skeleton meshes
                            if (LM != null)
                            {
                                SkeletonCopyMeshes2(world.SkeletalModels[0].MeshTree, LM.MeshTree,
                                    world.SkeletalModels[0].MeshCount);
                            }
                            world.SkeletalModels[0].FillSkinnedMeshMap();
                            break;

                        case Loader.Engine.Unknown:
                            break;
                    }

                    for (var j = 0; j < lara.Bf.BoneTags.Count; j++)
                    {
                        lara.Bf.BoneTags[j].MeshBase = lara.Bf.Animations.Model.MeshTree[j].MeshBase;
                        lara.Bf.BoneTags[j].MeshSkin = lara.Bf.Animations.Model.MeshTree[j].MeshSkin;
                        lara.Bf.BoneTags[i].MeshSlot = null;
                    }

                    world.Character.SetAnimation(TR_ANIMATION.LaraStayIdle, 0);
                    lara.GenRigidBody();
                    lara.CreateGhosts();
                    lara.Height = 768.0f;
                    lara.StateFunc = AnimStateControl.StateControlLara;

                    continue;
                }

                entity.SetAnimation(TR_ANIMATION.LaraRun, 0); // Set zero animation and zero frame

                Res_SetEntityProperties(entity);
                entity.RebuildBV();
                entity.GenRigidBody();

                entity.Self.Room.AddEntity(entity);
                world.AddEntity(entity);
            }
        }

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

        public static void TR_GenTextures(World world, Level tr)
        {
            var borderSize = Renderer.Settings.TextureBorder.Clamp(0, 64);

            world.TextureAtlas = new BorderedTextureAtlas(
                borderSize, 
                Renderer.Settings.SaveTextureMemory, 
                tr.Textures,
                tr.ObjectTextures, 
                tr.SpriteTextures);

            world.Textures.Resize((int)world.TextureAtlas.NumAtlasPages + 1);

            GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
            GL.PixelZoom(1, 1);
            unsafe
            {
                var tmp = world.Textures.ToArray();
                fixed(uint* ptr = tmp)
                    world.TextureAtlas.CreateTextures(ptr, 1);
            }

            // white texture data for coloured polygons and debug lines.
            var whtx = new[]
            {
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff,
                0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff, 0xff
            };

            // Select mipmap mode
            switch(Renderer.Settings.MipmapMode)
            {
                case 0:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapNearest);
                    break;

                case 1:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapNearest);
                    break;

                case 2:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.NearestMipmapLinear);
                    break;

                case 3:
                default:
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
                    GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear);
                    break;
            }

            // Set mipmaps number
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMaxLevel, Renderer.Settings.Mipmaps);

            // Set anisotropy degree
            GL.TexParameter(TextureTarget.Texture2D,
                (TextureParameterName) ExtTextureFilterAnisotropic.TextureMaxAnisotropyExt, Renderer.Settings.Anisotropy);

            // Read lod bias
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureLodBias, Renderer.Settings.LodBias);

            GL.BindTexture(TextureTarget.Texture2D, world.Textures.Last()); // solid color =)
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter,
                (int) TextureMagFilter.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, 4, 4, 0, PixelFormat.Rgba,
                PixelType.UnsignedByte, whtx);
            GL.TexImage2D(TextureTarget.Texture2D, 1, PixelInternalFormat.Rgba, 2, 2, 0, PixelFormat.Rgba,
             PixelType.UnsignedByte, whtx);
            GL.TexImage2D(TextureTarget.Texture2D, 2, PixelInternalFormat.Rgba, 1, 1, 0, PixelFormat.Rgba,
             PixelType.UnsignedByte, whtx);
            //GL.Disable(EnableCap.Texture2D); // Why it is here? It is blocking loading screen.
        }

        public static void TR_GenAnimCommands(World world, Level tr)
        {
            world.AnimCommands = tr.AnimCommands.ToArray();
        }

        /**   Animated textures loading.
          *   Natively, animated textures stored as a stream of bitu16s, which
          *   is then parsed on the fly. What we do is parse this stream to the
          *   proper structures to be used later within renderer.
          */
        public static unsafe void TR_GenAnimTextures(World world, Level tr)
        {
            var p0 = new Polygon();
            var p = new Polygon();

            p0.Vertices.Resize(3, () => new Vertex());
            p.Vertices.Resize(3, () => new Vertex());

            fixed(ushort* tmp = tr.AnimatedTextures)
            {
                var pointer = tmp;
                var numUvrotates = tr.AnimatedTexturesUVCount;

                var numSequences = *pointer++; // First word in a stream is sequence count.

                world.AnimSequences.Resize(numSequences, () => new AnimSeq());

                for(var i = 0; i < numSequences; i++)
                {
                    var seq = world.AnimSequences[i];

                    seq.Frames.Resize(*pointer++ + 1, () => new TexFrame());
                    seq.FrameList.Resize(seq.Frames.Count);

                    // Fill up new sequence with frame list
                    seq.AnimType = TR_ANIMTEXTURE.Forward;
                    seq.FrameLock = false; // by default anim is playing
                    seq.UVRotate = false; // by default uvrotate
                    seq.ReverseDirection = false; // Needed for proper reverse-type start-up.
                    seq.FrameRate = 0.05f; // Should be passed as 1 / FPS.
                    seq.FrameTime = 0.0f; // Reset frame time to initial state.
                    seq.CurrentFrame = 0; // Reset current frame to zero.

                    for(var j = 0; j < seq.Frames.Count; j++)
                    {
                        seq.FrameList[j] = *pointer++; // Add one frame.
                    }

                    // UVRotate textures case.
                    // In TR4-5, it is possible to define special UVRotate animation mode.
                    // It is specified by num_uvrotates variable. If sequence belongs to
                    // UVRotate range, each frame will be divided in half and continously
                    // scrolled from one part to another by shifting UV coordinates.
                    // In OpenTomb, we can have BOTH UVRotate and classic frames mode
                    // applied to the same sequence, but there we specify compatibility
                    // method for TR4-5.

                    var uvrotateScript = 0;
                    var tmp1 = EngineLua["UVRotate"];
                    if(tmp1 != null)
                    {
                        uvrotateScript = (int) tmp1;
                    }

                    if(i < numUvrotates)
                    {
                        seq.FrameLock = false; // by default anim is playing

                        seq.UVRotate = true;
                        // Get texture height and divide it in half.
                        // This way, we get a reference value which is used to identify
                        // if scrolling is completed or not.
                        seq.Frames.Resize(8, () => new TexFrame());
                        seq.UVRotateMax = world.TextureAtlas.GetTextureHeight(seq.FrameList[0]) / 2f;
                        seq.UVRotateSpeed = seq.UVRotateMax / seq.Frames.Count;
                        seq.FrameList.Resize(8);

                        if(uvrotateScript > 0)
                        {
                            seq.AnimType = TR_ANIMTEXTURE.Forward;
                        }
                        else if(uvrotateScript < 0)
                        {
                            seq.AnimType = TR_ANIMTEXTURE.Backward;
                        }

                        EngineWorld.TextureAtlas.GetCoordinates(seq.FrameList[0], false, p, 0, true);
                        for(var j = 0; j < seq.Frames.Count; j++)
                        {
                            EngineWorld.TextureAtlas.GetCoordinates(seq.FrameList[0], false, p, (int)(j * seq.UVRotateSpeed), true);
                            seq.Frames[j].TextureIndex = p.TexIndex;

                            var A0 = new[]
                            {
                                p0.Vertices[1].TexCoord[0] - p0.Vertices[0].TexCoord[0], // TODO: p0 hasn't been modified??
                                p0.Vertices[1].TexCoord[1] - p0.Vertices[0].TexCoord[1]
                            };
                            var B0 = new[]
                            {
                                p0.Vertices[2].TexCoord[0] - p0.Vertices[0].TexCoord[0],
                                p0.Vertices[2].TexCoord[1] - p0.Vertices[0].TexCoord[1]
                            };

                            var A = new[]
                            {
                                p.Vertices[1].TexCoord[0] - p.Vertices[0].TexCoord[0],
                                p.Vertices[1].TexCoord[1] - p.Vertices[0].TexCoord[1]
                            };
                            var B = new[]
                            {
                                p.Vertices[2].TexCoord[0] - p.Vertices[0].TexCoord[0],
                                p.Vertices[2].TexCoord[1] - p.Vertices[0].TexCoord[1]
                            };

                            var d = A0[0] * B0[1] - A0[1] * B0[0];
                            seq.Frames[j].Mat[0 + 0 * 2] = (A[0] * B0[1] - A0[1] * B[0]) / d;
                            seq.Frames[j].Mat[1 + 0 * 2] = -(A[1] * B0[1] - A0[1] * B[1]) / d;
                            seq.Frames[j].Mat[0 + 1 * 2] = -(A0[0] * B[0] - A[0] * B0[0]) / d;
                            seq.Frames[j].Mat[1 + 1 * 2] = (A0[0] * B[1] - A[1] * B0[0]) / d;

                            seq.Frames[j].Move[0] = p.Vertices[0].TexCoord[0] -
                                                    (p0.Vertices[0].TexCoord[0] * seq.Frames[j].Mat[0 + 0 * 2] +
                                                     p0.Vertices[0].TexCoord[1] * seq.Frames[j].Mat[0 + 1 * 2]);
                            seq.Frames[j].Move[1] = p.Vertices[0].TexCoord[1] -
                                                    (p0.Vertices[0].TexCoord[0] * seq.Frames[j].Mat[1 + 0 * 2] +
                                                     p0.Vertices[0].TexCoord[1] * seq.Frames[j].Mat[1 + 1 * 2]);
                        }
                    }
                    else
                    {
                        EngineWorld.TextureAtlas.GetCoordinates(seq.FrameList[0], false, p0);
                        for (var j = 0; j < seq.Frames.Count; j++)
                        {
                            EngineWorld.TextureAtlas.GetCoordinates(seq.FrameList[j], false, p);
                            seq.Frames[j].TextureIndex = p.TexIndex;

                            var A0 = new[]
                            {
                                p0.Vertices[1].TexCoord[0] - p0.Vertices[0].TexCoord[0],
                                p0.Vertices[1].TexCoord[1] - p0.Vertices[0].TexCoord[1]
                            };
                            var B0 = new[]
                            {
                                p0.Vertices[2].TexCoord[0] - p0.Vertices[0].TexCoord[0],
                                p0.Vertices[2].TexCoord[1] - p0.Vertices[0].TexCoord[1]
                            };

                            var A = new[]
                            {
                                p.Vertices[1].TexCoord[0] - p.Vertices[0].TexCoord[0],
                                p.Vertices[1].TexCoord[1] - p.Vertices[0].TexCoord[1]
                            };
                            var B = new[]
                            {
                                p.Vertices[2].TexCoord[0] - p.Vertices[0].TexCoord[0],
                                p.Vertices[2].TexCoord[1] - p.Vertices[0].TexCoord[1]
                            };

                            var d = A0[0] * B0[1] - A0[1] * B0[0];
                            seq.Frames[j].Mat[0 + 0 * 2] = (A[0] * B0[1] - A0[1] * B[0]) / d;
                            seq.Frames[j].Mat[1 + 0 * 2] = -(A[1] * B0[1] - A0[1] * B[1]) / d;
                            seq.Frames[j].Mat[0 + 1 * 2] = -(A0[0] * B[0] - A[0] * B0[0]) / d;
                            seq.Frames[j].Mat[1 + 1 * 2] = (A0[0] * B[1] - A[1] * B0[0]) / d;

                            seq.Frames[j].Move[0] = p.Vertices[0].TexCoord[0] -
                                                    (p0.Vertices[0].TexCoord[0] * seq.Frames[j].Mat[0 + 0 * 2] +
                                                     p0.Vertices[0].TexCoord[1] * seq.Frames[j].Mat[0 + 1 * 2]);
                            seq.Frames[j].Move[1] = p.Vertices[0].TexCoord[1] -
                                                    (p0.Vertices[0].TexCoord[0] * seq.Frames[j].Mat[1 + 0 * 2] +
                                                     p0.Vertices[0].TexCoord[1] * seq.Frames[j].Mat[1 + 1 * 2]);
                        }
                    }
                }
            }
        }

        public static void TR_GenRooms(World world, Level tr)
        {
            world.Rooms.Resize(tr.Rooms.Length, () => new Room());
            for (var i = 0; i < world.Rooms.Count; i++)
            {
                TR_GenRoom(i, world.Rooms[i], world, tr);
            }
        }

        public static void TR_GenRoom(int roomIndex, Room room, World world, Level tr)
        {
            var trRoom = tr.Rooms[roomIndex];

#region Room properties

            room.ID = (uint) roomIndex;
            room.Active = true;
            room.Frustum = new List<Frustum>();
            room.Flags = trRoom.Flags;
            room.LightMode = trRoom.LightMode;
            room.ReverbInfo = (byte) trRoom.ReverbInfo;
            room.WaterScheme = trRoom.WaterScheme;
            room.AlternateGroup = (byte) trRoom.AlternateGroup;

            room.Transform = new Transform();
            room.Transform.SetIdentity();
            room.Transform.Origin = trRoom.Offset.ToVector3();
            room.AmbientLighting = new float[3];
            room.AmbientLighting[0] = trRoom.LightColor.R * 2;
            room.AmbientLighting[1] = trRoom.LightColor.G * 2;
            room.AmbientLighting[2] = trRoom.LightColor.B * 2;
            room.Self = new EngineContainer();
            room.Self.Room = room;
            room.Self.Object = room;
            room.Self.ObjectType = OBJECT_TYPE.RoomBase;
            room.NearRoomList = new List<Room>();
            room.OverlappedRoomList = new List<Room>();

            room.GenMesh(world, (uint) roomIndex, tr);

            room.BtBody = null;
            // let's load static room meshes
            room.StaticMesh = new List<StaticMesh>();

#endregion

#region Static meshes

            Loader.StaticMesh trStatic;

            for (var i = 0; i < trRoom.StaticMeshes.Length; i++)
            {
                var trsm = trRoom.StaticMeshes[i];
                trStatic = tr.FindStaticMeshById(trsm.ObjectID);
                if (trStatic.Equals(default(Loader.StaticMesh)))
                {
                    continue;
                }
                var rStatic = new StaticMesh();
                rStatic.Self = new EngineContainer();
                rStatic.Self.Room = room;
                rStatic.Self.Object = rStatic;
                rStatic.Self.ObjectType = OBJECT_TYPE.StaticMesh;
                rStatic.ObjectID = trsm.ObjectID;
                rStatic.Mesh = world.Meshes[(int) tr.MeshIndices[trStatic.Mesh]];
                rStatic.Position = trsm.Position.ToVector3();
                rStatic.Rotation = new Vector3(trsm.Rotation, 0.0f, 0.0f);
                rStatic.Tint[0] = trsm.Tint.R * 2;
                rStatic.Tint[1] = trsm.Tint.G * 2;
                rStatic.Tint[2] = trsm.Tint.B * 2;
                rStatic.Tint[3] = trsm.Tint.A * 2;

                rStatic.CBBMin.X = trStatic.CollisionBox[0].X;
                rStatic.CBBMin.Y = -trStatic.CollisionBox[0].Z;
                rStatic.CBBMin.Z = trStatic.CollisionBox[1].Y;
                rStatic.CBBMax.X = trStatic.CollisionBox[1].X;
                rStatic.CBBMax.Y = -trStatic.CollisionBox[1].Z;
                rStatic.CBBMax.Z = trStatic.CollisionBox[0].Y;

                rStatic.VBBMin.X = trStatic.VisibilityBox[0].X;
                rStatic.VBBMin.Y = -trStatic.VisibilityBox[0].Z;
                rStatic.VBBMin.Z = trStatic.VisibilityBox[1].Y;

                rStatic.VBBMax.X = trStatic.VisibilityBox[1].X;
                rStatic.VBBMax.Y = -trStatic.VisibilityBox[1].Z;
                rStatic.VBBMax.Z = trStatic.VisibilityBox[0].Y;

                rStatic.OBB.Transform = rStatic.Transform;
                rStatic.OBB.Radius = rStatic.Mesh.Radius;
                rStatic.Transform.SetIdentity();
                VMath.Mat4_Translate(rStatic.Transform, rStatic.Position);
                VMath.Mat4_RotateZ(rStatic.Transform, rStatic.Rotation.X);
                rStatic.WasRendered = 0;
                rStatic.OBB.Rebuild(rStatic.VBBMin, rStatic.VBBMax);
                rStatic.OBB.DoTransform();

                rStatic.BtBody = null;
                rStatic.Hide = false;

                // Disable static mesh collision, if flag value is 3 (TR1) or all bounding box
                // coordinates are equal (TR2-5).

                if (trStatic.Flags == 3 ||
                    trStatic.CollisionBox[0].X == -trStatic.CollisionBox[0].Y &&
                    trStatic.CollisionBox[0].Y == trStatic.CollisionBox[0].Z &&
                    trStatic.CollisionBox[1].X == -trStatic.CollisionBox[1].Y &&
                    trStatic.CollisionBox[1].Y == trStatic.CollisionBox[1].Z)
                {
                    rStatic.Self.CollisionType = COLLISION_TYPE.None;
                }
                else
                {
                    rStatic.Self.CollisionType = COLLISION_TYPE.Static;
                    rStatic.Self.CollisionShape = COLLISION_SHAPE.Box;
                }

                // Set additional static mesh properties from level script override.

                Res_SetStaticMeshProperties(rStatic);

                // Set static mesh collision.

                if (rStatic.Self.CollisionType != COLLISION_TYPE.None)
                {
                    CollisionShape cshape;
                    switch (rStatic.Self.CollisionShape)
                    {
                        case COLLISION_SHAPE.Box:
                            cshape = BT_CSfromBBox(rStatic.CBBMin, rStatic.CBBMax, true, true);
                            break;

                        case COLLISION_SHAPE.BoxBase:
                            cshape = BT_CSfromBBox(rStatic.Mesh.BBMin, rStatic.Mesh.BBMax, true, true);
                            break;

                        case COLLISION_SHAPE.Trimesh:
                            cshape = BT_CSfromMesh(rStatic.Mesh, true, true, true);
                            break;

                        case COLLISION_SHAPE.TrimeshConvex:
                            cshape = BT_CSfromMesh(rStatic.Mesh, true, true, true);
                            break;

                        default:
                            cshape = null;
                            break;
                    }

                    if (cshape != null)
                    {
                        var startTransform = rStatic.Transform;
                        var motionState = new DefaultMotionState(((Matrix4) startTransform).ToBullet());
                        var localInertia = Vector3.Zero;
                        rStatic.BtBody =
                            new RigidBody(new RigidBodyConstructionInfo(0.0f, motionState, cshape, localInertia.ToBullet()));
                        BtEngineDynamicsWorld.AddRigidBody(rStatic.BtBody, CollisionFilterGroups.AllFilter,
                            CollisionFilterGroups.AllFilter);
                        rStatic.BtBody.UserObject = rStatic.Self;
                    }
                }

                room.StaticMesh.Add(rStatic);
            }

#endregion

#region Sprites

            foreach (var trs in trRoom.Sprites)
            {
                var rs = new RoomSprite();
                if (trs.Texture.IsBetween(0, world.Sprites.Count, IB.aIbE))
                {
                    rs.Sprite = world.Sprites[trs.Texture];
                    rs.Position = trRoom.Vertices[trs.Vertex].Vertex.ToVector3() + room.Transform.Origin;
                }
                room.Sprites.Add(rs);
            }

#endregion

#region Sectors

            room.SectorsX = trRoom.Num_X_Sectors;
            room.SectorsY = trRoom.Num_Z_Sectors;
            room.Sectors = new List<RoomSector>();
            room.Sectors.Resize(room.SectorsX * room.SectorsY, () => new RoomSector());

            // base sectors information loading and collisional mesh creation

            // To avoid manipulating with unnecessary information, we declare simple
            // heightmap here, which will be operated with sector and floordata parsing,
            // then vertical inbetween polys will be constructed, and Bullet collisional
            // object will be created. Afterwards, this heightmap also can be used to
            // quickly detect slopes for pushable blocks and other entities that rely on
            // floor level.

            for (var i = 0; i < room.Sectors.Count; i++)
            {
                var sector = room.Sectors[i];

                // Filling base sectors information.

                sector.IndexX = (short) (i / room.SectorsY);
                sector.IndexY = (short) (i % room.SectorsY);

                sector.Position.X = room.Transform.Origin.X + (sector.IndexX + 0.5f) * TR_METERING_SECTORSIZE;
                sector.Position.Y = room.Transform.Origin.Y + (sector.IndexY + 0.5f) * TR_METERING_SECTORSIZE;
                sector.Position.Z = 0.5f * (trRoom.Y_Bottom + trRoom.Y_Top);

                sector.OwnerRoom = room;

                if (tr.GameVersion < TRGame.TR3)
                {
                    sector.BoxIndex = trRoom.Sectors[i].Box_Index;
                    sector.Material = SectorMaterial.Stone;
                }
                else
                {
                    sector.BoxIndex = (trRoom.Sectors[i].Box_Index & 0xFFF0) >> 4;
                    sector.Material = (SectorMaterial)((uint) trRoom.Sectors[i].Box_Index & 0x000F);
                }

                if (sector.BoxIndex == 0xFFFF) sector.BoxIndex = -1;

                sector.Flags = 0; // Clear sector flags

                sector.Floor = (int) -TR_METERING_STEP * trRoom.Sectors[i].Floor;
                sector.Ceiling = (int) -TR_METERING_STEP * trRoom.Sectors[i].Ceiling;
                sector.TrigIndex = trRoom.Sectors[i].FD_Index;

                // BUILDING CEILING HEIGHTMAP.

                // Penetration config is used later to build inbetween vertical collision polys.
                // If sector's penetration config is a wall, we simply build a vertical plane to
                // isolate this sector from top to bottom. Also, this allows to trick out wall
                // sectors inside another wall sectors to be ignored completely when building
                // collisional mesh.
                // Door penetration config means that we should either ignore sector collision
                // completely (classic door) or ignore one of the triangular sector parts (TR3+).

                if (sector.Ceiling == TR_METERING_WALLHEIGHT)
                {
                    sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Wall;
                }
                else if (trRoom.Sectors[i].RoomAbove != 0xFF)
                {
                    sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Ghost;
                }
                else
                {
                    sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Solid;
                }

                // Reset some sector parameters to avoid garbaged memory issues.

                sector.PortalToRoom = -1;
                sector.CeilingDiagonalType = TR_SECTOR_DIAGONAL_TYPE.None;
                sector.FloorDiagonalType = TR_SECTOR_DIAGONAL_TYPE.None;

                // Now, we define heightmap cells position and draft (flat) height.
                // Draft height is derived from sector's floor and ceiling values, which are
                // copied into heightmap cells Y coordinates. As result, we receive flat
                // heightmap cell, which will be operated later with floordata.

                sector.CeilingCorners[0][0] = sector.IndexX * TR_METERING_SECTORSIZE;
                sector.CeilingCorners[0][1] = sector.IndexY * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.CeilingCorners[0][2] = sector.Ceiling;

                sector.CeilingCorners[1][0] = sector.IndexX * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.CeilingCorners[1][1] = sector.IndexY * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.CeilingCorners[1][2] = sector.Ceiling;

                sector.CeilingCorners[2][0] = sector.IndexX * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.CeilingCorners[2][1] = sector.IndexY * TR_METERING_SECTORSIZE;
                sector.CeilingCorners[2][2] = sector.Ceiling;

                sector.CeilingCorners[3][0] = sector.IndexX * TR_METERING_SECTORSIZE;
                sector.CeilingCorners[3][1] = sector.IndexY * TR_METERING_SECTORSIZE;
                sector.CeilingCorners[3][2] = sector.Ceiling;


                // BUILDING FLOOR HEIGHTMAP.

                // Features same steps as for the ceiling.

                if (sector.Floor == TR_METERING_WALLHEIGHT)
                {
                    sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Wall;
                }
                else if (trRoom.Sectors[i].RoomBelow != 0xFF)
                {
                    sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Ghost;
                }
                else
                {
                    sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Solid;
                }

                sector.FloorCorners[0][0] = sector.IndexX * TR_METERING_SECTORSIZE;
                sector.FloorCorners[0][1] = sector.IndexY * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.FloorCorners[0][2] = sector.Floor;

                sector.FloorCorners[1][0] = sector.IndexX * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.FloorCorners[1][1] = sector.IndexY * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.FloorCorners[1][2] = sector.Floor;

                sector.FloorCorners[2][0] = sector.IndexX * TR_METERING_SECTORSIZE + TR_METERING_SECTORSIZE;
                sector.FloorCorners[2][1] = sector.IndexY * TR_METERING_SECTORSIZE;
                sector.FloorCorners[2][2] = sector.Floor;

                sector.FloorCorners[3][0] = sector.IndexX * TR_METERING_SECTORSIZE;
                sector.FloorCorners[3][1] = sector.IndexY * TR_METERING_SECTORSIZE;
                sector.FloorCorners[3][2] = sector.Floor;
            }

#endregion

#region Lights

            room.Lights.Resize(trRoom.Lights.Length, () => new Light());

            for (var i = 0; i < trRoom.Lights.Length; i++)
            {
                var l = room.Lights[i];
                var tl = trRoom.Lights[i];

                l.LightType = tl.LightType;

                l.Position = tl.Position.ToVector3();

                if (l.LightType == LightType.Shadow)
                {
                    l.Colour[0] = -(tl.Color.R / 255.0f) * tl.Intensity;
                    l.Colour[1] = -(tl.Color.G / 255.0f) * tl.Intensity;
                    l.Colour[2] = -(tl.Color.B / 255.0f) * tl.Intensity;
                    l.Colour[3] = 1.0f;
                }
                else
                {
                    l.Colour[0] = tl.Color.R / 255.0f * tl.Intensity;
                    l.Colour[1] = tl.Color.G / 255.0f * tl.Intensity;
                    l.Colour[2] = tl.Color.B / 255.0f * tl.Intensity;
                    l.Colour[3] = 1.0f;
                }

                l.Inner = tl.R_Inner;
                l.Outer = tl.R_Outer;
                l.Length = tl.Length;
                l.Cutoff = tl.Cutoff;

                l.Falloff = 0.001f / l.Outer;
            }

#endregion

#region Portals

            room.Portals.Resize(trRoom.Portals.Length, () => new Portal());
            for (var i = 0; i < room.Portals.Count; i++)
            {
                var trp = trRoom.Portals[i];
                var p = room.Portals[i];
                var rDest = world.Rooms[trp.AdjoiningRoom];
                p.Vertices.Resize(4); // in original TR all portals are axis aligned rectangles
                p.DestRoom = rDest;
                p.CurrentRoom = room;
                p.Vertices = trp.Vertices.Reverse().Select(x => x.ToVector3() + room.Transform.Origin).ToList();
                p.Centre = p.Vertices.Sum() / p.Vertices.Count;
                p.GenNormal();

                // Portal position fix
                // X_MIN
                if(p.Normal.Normal.X > 0.999f && (int)p.Centre.X % 2 != 0)
                {
                    p.Move(Vector3.UnitX);
                }

                // Y_MIN
                if (p.Normal.Normal.Y > 0.999f && (int)p.Centre.Y % 2 != 0)
                {
                    p.Move(Vector3.UnitY);
                }

                // Z_MAX
                if (p.Normal.Normal.Z < -0.999f && (int)p.Centre.Z % 2 != 0)
                {
                    p.Move(-Vector3.UnitZ);
                }
            }

#endregion

#region Room borders

            room.BBMin.Z = trRoom.Y_Bottom;
            room.BBMax.Z = trRoom.Y_Top;

            room.BBMin.X = room.Transform.Origin.X + TR_METERING_SECTORSIZE;
            room.BBMin.Y = room.Transform.Origin.Y + TR_METERING_SECTORSIZE;
            room.BBMax.X = room.Transform.Origin.X + TR_METERING_SECTORSIZE * room.SectorsX - TR_METERING_SECTORSIZE;
            room.BBMax.Y = room.Transform.Origin.Y + TR_METERING_SECTORSIZE * room.SectorsY - TR_METERING_SECTORSIZE;

#endregion

#region Alternate room

            // alternate room pointer calculation if one exists.
            room.AlternateRoom = null;
            room.BaseRoom = null;

            if(trRoom.AlternateRoom.IsBetween(0, tr.Rooms.Length, IB.aIbE))
            {
                room.AlternateRoom = world.Rooms[trRoom.AlternateRoom];
            }

#endregion
        }

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

#if !NO_AUDIO
        public static unsafe void TR_GenSamples(World world, Level tr)
        {
            world.AudioBuffers = new uint[tr.SamplesCount];
            fixed (uint* ptr = world.AudioBuffers)
                AL.GenBuffers(world.AudioBuffers.Length, ptr);

            // Generate stream track map array.
            // We use scripted amount of tracks to define map bounds.
            // If script had no such parameter, we define map bounds by default.
            var n = EngineLua.GetNumTracks();
            world.StreamTrackMap.Resize(n == 0 ? TR_AUDIO_STREAM_MAP_SIZE : n);

            // Generate new audio effects array.
            world.AudioEffects.Resize(tr.SoundDetails.Length, () => new AudioEffect());

            // Generate new audio emitters array.
            world.AudioEmitters.Resize(tr.SoundSources.Length, () => new AudioEmitter());

            // Cycle through raw samples block and parse them to OpenAL buffers.

            // Different TR versions have different ways of storing samples.
            // TR1:     sample block size, sample block, num samples, sample offsets.
            // TR2/TR3: num samples, sample offsets. (Sample block is in MAIN.SFX.)
            // TR4/TR5: num samples, (uncomp_size-comp_size-sample_data) chain.
            //
            // Hence, we specify certain parse method for each game version.

            if (tr.SamplesData.Length > 0)
            {
                fixed (byte* tmp = tr.SamplesData)
                {
                    var pointer = tmp;
                    switch (tr.GameVersion)
                    {
                        case TRGame.TR1:
                        case TRGame.TR1Demo:
                        case TRGame.TR1UnfinishedBusiness:
                        {
                            world.AudioMap = tr.SoundMap.ToList();

                            for (var i = 0; i < tr.SampleIndices.Length - 1; i++)
                            {
                                pointer = tmp + tr.SampleIndices[i];
                                var size = tr.SampleIndices[i + 1] - tr.SampleIndices[i];
                                Audio.LoadALBufferFromMem(world.AudioBuffers[i], pointer, size);
                            }
                        }
                            break;

                        case TRGame.TR2:
                        case TRGame.TR2Demo:
                        case TRGame.TR3:
                        {
                            // TODO: TR3 gold ??
                            world.AudioMap = tr.SoundMap.ToList();
                            uint ind1 = 0;
                            uint ind2 = 0;
                            var flag = false;
                            var i = 0;
                            while (pointer < tmp + tr.SamplesData.Length - 4)
                            {
                                pointer = tmp + ind2;
                                if (*(int*) pointer == 0x46464952)
                                {
                                    // RIFF
                                    if (!flag)
                                    {
                                        ind1 = ind2;
                                        flag = true;
                                    }
                                    else
                                    {
                                        var uncompSize = ind2 - ind1;
                                        var srcData = tmp + ind1;
                                        Audio.LoadALBufferFromMem(world.AudioBuffers[i], srcData, uncompSize);
                                        i++;
                                        if (i >= world.AudioBuffers.Length)
                                        {
                                            break;
                                        }
                                        ind1 = ind2;
                                    }
                                }
                                ind2++;
                            }
                            var uncomp_Size = (uint) tr.SamplesData.Length - ind1;
                            pointer = tmp + ind1;
                            if (i < world.AudioBuffers.Length)
                            {
                                Audio.LoadALBufferFromMem(world.AudioBuffers[i], pointer, uncomp_Size);
                            }
                            break;
                        }

                        case TRGame.TR4:
                        case TRGame.TR4Demo:
                        case TRGame.TR5:
                            world.AudioMap = tr.SoundMap.ToList();

                            for (var i = 0; i < tr.SamplesCount; i++)
                            {
                                // Parse sample sizes.
                                // Always use comp_size as block length, as uncomp_size is used to cut raw sample data.
                                var uncompSize = *(uint*) pointer;
                                pointer += 4;
                                var compSize = *(uint*) pointer;
                                pointer += 4;

                                // Load WAV sample into OpenAL buffer.
                                Audio.LoadALBufferFromMem(world.AudioBuffers[i], pointer, compSize, uncompSize);

                                // Now we can safely move pointer through current sample data.
                                pointer += compSize;
                            }
                            break;

                        default:
                            world.AudioMap.Clear();
                            tr.SamplesData.Clear();
                            return;
                    }
                }
            }

            // Cycle through SoundDetails and parse them into native OpenTomb
            // audio effects structure.
            for (var i = 0; i < world.AudioEffects.Count; i++)
            {
                if (tr.GameVersion < TRGame.TR3)
                {
                    world.AudioEffects[i].Gain = tr.SoundDetails[i].Volume / 32767.0f;
                        // Max. volume in TR1/TR2 is 32767.
                    world.AudioEffects[i].Chance = tr.SoundDetails[i].Chance;
                }
                else if (tr.GameVersion > TRGame.TR3)
                {
                    world.AudioEffects[i].Gain = tr.SoundDetails[i].Volume / 255.0f; // Max. volume in TR3 is 255.
                    world.AudioEffects[i].Chance = tr.SoundDetails[i].Chance * 255;
                }
                else
                {
                    world.AudioEffects[i].Gain = tr.SoundDetails[i].Volume / 255.0f; // Max. volume in TR3 is 255.
                    world.AudioEffects[i].Chance = tr.SoundDetails[i].Chance * 127;
                }

                world.AudioEffects[i].RandomizeGainVar = 50;
                world.AudioEffects[i].RandomizePitchVar = 50;

                world.AudioEffects[i].Pitch = tr.SoundDetails[i].Pitch / 127.0f + 1.0f;
                world.AudioEffects[i].Range = tr.SoundDetails[i].SoundRange * 1024.0f;

                world.AudioEffects[i].RandomizePitch = tr.SoundDetails[i].UseRandomPitch;
                world.AudioEffects[i].RandomizeGain = tr.SoundDetails[i].UseRandomVolume;

                world.AudioEffects[i].Loop = tr.SoundDetails[i].GetLoopType(Loader.Helper.GameToEngine(tr.GameVersion));

                world.AudioEffects[i].SampleIndex = tr.SoundDetails[i].Sample;
                world.AudioEffects[i].SampleCount = tr.SoundDetails[i].SampleCount;
            }

            // Try to override samples via script.
            // If there is no script entry exist, we just leave default samples.
            // NB! We need to override samples AFTER audio effects array is inited, as override
            //     routine refers to existence of certain audio effect in level.

            Audio.LoadOverridedSamples(world);

            // Hardcoded version-specific fixes!

            switch (world.EngineVersion)
            {
                case Loader.Engine.TR1:
                    // Fix for underwater looped sound.
                    if (world.AudioMap[(int) TR_AUDIO_SOUND.Underwater] >= 0)
                    {
                        world.AudioEffects[world.AudioMap[(int) TR_AUDIO_SOUND.Underwater]].Loop = LoopType.Forward;
                    }
                    break;
                case Loader.Engine.TR2:
                    // Fix for helicopter sound range.
                    if (world.AudioMap[297] >= 0)
                    {
                        world.AudioEffects[world.AudioMap[297]].Range *= 10.0f;
                    }
                    break;
                default:
                    break;
            }

            // Cycle through sound emitters and
            // parse them to native OpenTomb sound emitters structure.

            for (var i = 0; i < world.AudioEmitters.Count; i++)
            {
                world.AudioEmitters[i].EmitterIndex = (uint) i;
                world.AudioEmitters[i].SoundIndex = tr.SoundSources[i].SoundID;
                world.AudioEmitters[i].Position = new Vector3(tr.SoundSources[i].X, tr.SoundSources[i].Z,
                    -tr.SoundSources[i].Y);
                world.AudioEmitters[i].Flags = tr.SoundSources[i].Flags;
            }
        }
#endif

        // Helper functions to convert legacy TR structs to native OpenTomb structs.

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

        public static unsafe int TR_Sector_TranslateFloorData(RoomSector sector, Level tr)
        {
            var ret = 0;

#region Depth 1

            if (sector == null || !sector.TrigIndex.IsBetween(0, tr.FloorData.Length, IB.aEbE))
            {
                return 0;
            }

            sector.Flags = 0; // Clear sector flags before parsing.

            // PARSE FUNCTIONS

            fixed(ushort* tmp = tr.FloorData)
            {
                var end_p = tmp + tr.FloorData.Length - 1;
                var entry = tmp + sector.TrigIndex;
                
                int endBit;

                do
                {
#region Depth 2

                    // TR1 - TR2
                    //function = (*entry) & 0x00FF;                   // 0b00000000 11111111
                    //sub_function = ((*entry) & 0x7F00) >> 8;        // 0b01111111 00000000

                    //TR3+, but works with TR1 - TR2
                    var function = *entry & 0x001F; // 0b00000000 00011111
                    // var functionValue = (*entry & 0x00E0) >> 5; // 0b00000000 11100000  TR3+
                    var subFunction = (*entry & 0x7F00) >> 8; // 0b01111111 00000000

                    endBit = (*entry & 0x8000) >> 15; // 0b10000000 00000000

                    entry++;

                    switch ((FD_FUNC) function)
                    {
                        case FD_FUNC.PortalSector: // PORTAL DATA
                            if (subFunction == 0x00)
                            {
                                if (*entry < EngineWorld.Rooms.Count)
                                {
                                    sector.PortalToRoom = *entry;
                                    sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Ghost;
                                    sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Ghost;
                                }
                                entry++;
                            }
                            break;

                        case FD_FUNC.FloorSlant: // FLOOR SLANT
                            if (subFunction == 0x00)
                            {
                                var rawYslant = *entry & 0x00FF;
                                var rawXslant = (*entry & 0xFF00) >> 8;

                                sector.FloorDiagonalType = TR_SECTOR_DIAGONAL_TYPE.None;
                                sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Solid;

                                if (rawXslant > 0)
                                {
                                    sector.FloorCorners[2][2] -= rawXslant * TR_METERING_STEP;
                                    sector.FloorCorners[3][2] -= rawXslant * TR_METERING_STEP;
                                }
                                else if (rawXslant < 0)
                                {
                                    sector.FloorCorners[0][2] -= Math.Abs(rawXslant) * TR_METERING_STEP;
                                    sector.FloorCorners[1][2] -= Math.Abs(rawXslant) * TR_METERING_STEP;
                                }

                                if (rawYslant > 0)
                                {
                                    sector.FloorCorners[0][2] -= rawYslant * TR_METERING_STEP;
                                    sector.FloorCorners[3][2] -= rawYslant * TR_METERING_STEP;
                                }
                                else if (rawYslant < 0)
                                {
                                    sector.FloorCorners[1][2] -= Math.Abs(rawYslant) * TR_METERING_STEP;
                                    sector.FloorCorners[2][2] -= Math.Abs(rawYslant) * TR_METERING_STEP;
                                }

                                entry++;
                            }
                            break;

                        case FD_FUNC.CeilingSlant: // CEILING SLANT
                            if (subFunction == 0x00)
                            {
                                var rawYslant = *entry & 0x00FF;
                                var rawXslant = (*entry & 0xFF00) >> 8;

                                sector.CeilingDiagonalType = TR_SECTOR_DIAGONAL_TYPE.None;
                                sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Solid;

                                if (rawXslant > 0)
                                {
                                    sector.CeilingCorners[3][2] += rawXslant * TR_METERING_STEP;
                                    sector.CeilingCorners[2][2] += rawXslant * TR_METERING_STEP;
                                }
                                else if (rawXslant < 0)
                                {
                                    sector.CeilingCorners[1][2] += Math.Abs(rawXslant) * TR_METERING_STEP;
                                    sector.CeilingCorners[0][2] += Math.Abs(rawXslant) * TR_METERING_STEP;
                                }

                                if (rawYslant > 0)
                                {
                                    sector.CeilingCorners[1][2] += rawYslant * TR_METERING_STEP;
                                    sector.CeilingCorners[2][2] += rawYslant * TR_METERING_STEP;
                                }
                                else if (rawYslant < 0)
                                {
                                    sector.CeilingCorners[0][2] += Math.Abs(rawYslant) * TR_METERING_STEP;
                                    sector.CeilingCorners[3][2] += Math.Abs(rawYslant) * TR_METERING_STEP;
                                }

                                entry++;
                            }
                            break;

                        case FD_FUNC.Trigger: // TRIGGERS
                        {
#region Trigger

                            var header = ""; // Header condition
                            var once_condition = ""; // One-shot condition
                            var cont_events = ""; // Continous trigger events
                            var single_events = ""; // One-shot trigger events
                            var item_events = ""; // Item activation events
                            var anti_events = ""; // Item deactivation events, if needed

                            var script = ""; // Final script compile

                            var buf = "\0"; // Stream buffer
                            var buf2 = "\0"; // Conditional pre-buffer for SWITCH triggers

                            var activator = Activator.Normal; // Activator is normal by default.
                            var actionType = ActionType.Normal; // Action type is normal by default.
                            var condition = 0; // No condition by default.
                            var maskMode = AMASK_OP_OR; // Activation mask by default.

                            var timerField = *entry & 0x00FF; // Used as common parameter for some commands.
                            var triggerMask = (*entry & 0x3E00) >> 9;
                            var onlyOnce = (*entry & 0x0100) >> 8; // Lock out triggered items after activation.

                            // Processed entities lookup array initialization.

                            var entLookupTable = Helper.FillArray(0xFF, 64);

                            // Activator type is LARA for all triggers except HEAVY ones, which are triggered by
                            // some specific entity classes.

                            var activatorType = ((TriggerTypes) subFunction).IsAnyOf(TriggerTypes.Heavy,
                                TriggerTypes.HeavyAntiTrigger, TriggerTypes.HeavySwitch)
                                ? ActivatorType.Misc
                                : ActivatorType.Lara;

                            // Table cell header

                            buf =
                                Helper.Format(
                                    "trigger_list[{0}] = {{activator_type = {1}, func = function(entity_index) \n",
                                    sector.TrigIndex, (int) activatorType);

                            script += buf;
                            buf = ""; // Zero out buffer to prevent further trashing.

                            switch ((TriggerTypes) subFunction)
                            {
                                case TriggerTypes.Trigger:
                                case TriggerTypes.Heavy:
                                    activator = Activator.Normal;
                                    break;

                                case TriggerTypes.Pad:
                                case TriggerTypes.AntiPad:
                                    // Check move type for triggering entity.
                                    buf = Helper.Format("if(getEntityMoveType(entity_index) == {0}) then \n",
                                        (int) MoveType.OnFloor);
                                    if (subFunction == (int) TriggerTypes.AntiPad) actionType = ActionType.Anti;
                                    condition = 1; // Set additional condition.
                                    break;

                                case TriggerTypes.Switch:
                                    // Set activator and action type for now; conditions are linked with first item in operand chain.
                                    activator = Activator.Switch;
                                    actionType = ActionType.Switch;
                                    maskMode = AMASK_OP_XOR;
                                    break;

                                case TriggerTypes.HeavySwitch:
                                    // Action type remains normal, as HEAVYSWITCH acts as "heavy trigger" with activator mask filter.
                                    activator = Activator.Switch;
                                    maskMode = AMASK_OP_XOR;
                                    break;

                                case TriggerTypes.Key:
                                    // Action type remains normal, as key acts one-way (no need in switch routines).
                                    activator = Activator.Key;
                                    break;

                                case TriggerTypes.Pickup:
                                    // Action type remains normal, as pick-up acts one-way (no need in switch routines).
                                    activator = Activator.Pickup;
                                    break;

                                case TriggerTypes.Combat:
                                    // Check weapon status for triggering entity.
                                    buf = " if(getCharacterCombatMode(entity_index) > 0) then \n";
                                    condition = 1; // Set additional condition.
                                    break;

                                case TriggerTypes.Dummy:
                                case TriggerTypes.Skeleton: // TODO: Find the meaning later!!!
                                    // These triggers are being parsed, but not added to trigger script!
                                    actionType = ActionType.Bypass;
                                    break;

                                case TriggerTypes.AntiTrigger:
                                case TriggerTypes.HeavyAntiTrigger:
                                    actionType = ActionType.Anti;
                                    break;

                                case TriggerTypes.Monkey:
                                case TriggerTypes.Climb:
                                    // Check move type for triggering entity.
                                    buf = Helper.Format(" if(getEntityMoveType(entity_index) == {0}) then \n",
                                        (int)
                                            (subFunction == (int) TriggerTypes.Monkey
                                                ? MoveType.Monkeyswing
                                                : MoveType.Climbing));
                                    condition = 1; // Set additional condition.
                                    break;

                                case TriggerTypes.Tightrope:
                                    // Check state range for triggering entity.
                                    buf =
                                        Helper.Format(
                                            " local state = getEntityState(entity_index) \n" +
                                            " if((state >= {0}) and (state <= {1})) then \n",
                                            (int) TR_STATE.LaraTightropeIdle, (int) TR_STATE.LaraTightropeExit);
                                    condition = 1; // Set additional condition.
                                    break;

                                case TriggerTypes.CrawlDuck:
                                    // Check state range for triggering entity.
                                    buf =
                                        Helper.Format(
                                            " local state = getEntityState(entity_index) \n" +
                                            " if((state >= {0}) and (state <= {1})) then \n",
                                            (int) TR_ANIMATION.LaraCrouchRollForwardBegin,
                                            (int) TR_ANIMATION.LaraCrawlSmashLeft);
                                    // TODO: Inverted STATE and ANIMATION?
                                    condition = 1; // Set additional condition.
                                    break;
                            }

                            header += buf; // Add condition to header.

                            int contBit;
                            var argn = 0;

                            // Now parse operand chain for trigger function!

                            do
                            {
                                entry++;

                                var triggerFunction = (*entry & 0x7C00) >> 10; // 0b01111100 00000000
                                var operands = *entry & 0x03FF; // 0b00000011 11111111
                                contBit = (*entry & 0x8000) >> 15; // 0b10000000 00000000

                                switch ((FD_TRIGFUNC) triggerFunction)
                                {
                                    case FD_TRIGFUNC.Object: // ACTIVATE / DEACTIVATE object
                                        // If activator is specified, first item operand counts as activator index (except
                                        // heavy switch case, which is ordinary heavy trigger case with certain differences).
                                        if (argn == 0 && activator != Activator.Normal)
                                        {
                                            switch (activator)
                                            {
                                                case Activator.Switch:
                                                    if (actionType == ActionType.Switch)
                                                    {
                                                        // Switch action type case.
                                                        script +=
                                                            Helper.Format(
                                                                " local switch_state = getEntityState({0}); \n" +
                                                                " local switch_sectorstatus = getEntitySectorStatus({0}); \n" +
                                                                " local switch_mask = getEntityMask({0}); \n\n",
                                                                operands);
                                                    }
                                                    else
                                                    {
                                                        // Ordinary type case (e.g. heavy switch).
                                                        script +=
                                                            Helper.Format(
                                                                " local switch_sectorstatus = getEntitySectorStatus(entity_index); \n" +
                                                                " local switch_mask = getEntityMask(entity_index); \n\n");
                                                    }

                                                    script +=
                                                        Helper.Format(
                                                            " if(switch_mask == 0) then switch_mask = 0x1F end; \n" +
                                                            " switch_mask = bit32.band(switch_mask, 0x{0:X2}); \n\n",
                                                            triggerMask);
                                                    if (actionType == ActionType.Switch)
                                                    {
                                                        // Switch action type case.
                                                        buf =
                                                            Helper.Format(
                                                                " if((switch_state == 0) and switch_sectorstatus) then \n" +
                                                                "   setEntitySectorStatus({0}, false); \n" +
                                                                "   setEntityTimer({0}, {1}); \n", operands,
                                                                timerField);
                                                        if (EngineWorld.EngineVersion >= Loader.Engine.TR3 &&
                                                            onlyOnce != 0)
                                                        {
                                                            // Just lock out activator, no anti-action needed.
                                                            buf2 = Helper.Format(" setEntityLock({0}, true) \n",
                                                                operands);
                                                        }
                                                        else
                                                        {
                                                            // Create statement for antitriggering a switch.
                                                            buf2 =
                                                                Helper.Format(
                                                                    " elseif((switch_state == 1) and switch_sectorstatus) then\n" +
                                                                    "   setEntitySectorStatus({0}, false); \n   setEntityTimer({0}, 0); \n",
                                                                    operands);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        // Ordinary type case (e.g. heavy switch).
                                                        item_events +=
                                                            Helper.Format(
                                                                "   activateEntity({0}, entity_index, switch_mask, {1}, {2}, {3}); \n",
                                                                operands, maskMode, onlyOnce != 0 ? "true" : "false",
                                                                timerField);
                                                        buf =
                                                            " if(not switch_sectorstatus) then \n" +
                                                            "   setEntitySectorStatus(entity_index, true) \n";
                                                    }
                                                    break;

                                                case Activator.Key:
                                                    buf =
                                                        Helper.Format(
                                                            " if((getEntityLock({0})) and (not getEntitySectorStatus({0}))) then \n" +
                                                            "   setEntitySectorStatus({0}, true); \n",
                                                            operands);
                                                    break;

                                                case Activator.Pickup:
                                                    buf =
                                                        Helper.Format(
                                                            " if((not getEntityEnability({0})) and (not getEntitySectorStatus({0}))) then \n" +
                                                            "   setEntitySectorStatus({0}, true); \n",
                                                            operands);
                                                    break;
                                            }

                                            script += buf;
                                        }
                                        else
                                        {
                                            // In many original Core Design levels, level designers left dublicated entity activation operands.
                                            // This results in setting same activation mask twice, effectively blocking entity from activation.
                                            // To prevent this, a lookup table was implemented to know if entity already had its activation
                                            // command added.
                                            if (!Res_IsEntityProcessed(entLookupTable, (ushort) operands))
                                            {
                                                // Other item operands are simply parsed as activation functions. Switch case is special, because
                                                // function is fed with activation mask argument derived from activator mask filter (switch_mask),
                                                // and also we need to process deactivation in a same way as activation, excluding resetting timer
                                                // field. This is needed for two-way switch combinations (e.g. Palace Midas).
                                                if (activator == Activator.Switch)
                                                {
                                                    item_events +=
                                                        Helper.Format(
                                                            "   activateEntity({0}, entity_index, switch_mask, {1}, {2}, {3}); \n",
                                                            operands, maskMode, onlyOnce != 0 ? "true" : "false",
                                                            timerField);
                                                    anti_events +=
                                                        Helper.Format(
                                                            "   activateEntity({0}, entity_index, switch_mask, {1}, {2}, 0); \n",
                                                            operands, maskMode, onlyOnce != 0 ? "true" : "false");
                                                }
                                                else
                                                {
                                                    item_events +=
                                                        Helper.Format(
                                                            "   activateEntity({0}, entity_index, 0x{1:X2}, {2}, {3}, {4}); \n",
                                                            operands, triggerMask, maskMode,
                                                            onlyOnce != 0 ? "true" : "false",
                                                            timerField);
                                                    anti_events +=
                                                        Helper.Format(
                                                            "   deactivateEntity({0}, entity_index, {1}); \n",
                                                            operands, onlyOnce != 0 ? "true" : "false");
                                                }
                                            }
                                        }
                                        argn++;
                                        break;

                                    case FD_TRIGFUNC.CameraTarget:
                                    {
                                        var camIndex = *entry & 0x007F;
                                        entry++;
                                        var camTimer = *entry & 0x00FF;
                                        var camOnce = (*entry & 0x0100) >> 8;
                                        var camZoom = EngineWorld.EngineVersion < Loader.Engine.TR2
                                            ? (*entry & 0x0400) >> 10
                                            : (*entry & 0x1000) >> 12;
                                        contBit = (*entry & 0x8000) >> 15; // 0b10000000 00000000

                                        single_events += Helper.Format("   setCamera({0}, {1}, {2}, {3}); \n",
                                            camIndex, camTimer, camOnce, camZoom);
                                    }
                                        break;

                                    case FD_TRIGFUNC.UwCurrent:
                                        cont_events += Helper.Format("   moveToSink(entity_index, {0}); \n",
                                            operands);
                                        break;

                                    case FD_TRIGFUNC.FlipMap:
                                        // FLIPMAP trigger acts two-way for switch cases, so we add FLIPMAP off event to
                                        // anti-events array.
                                        if (activator == Activator.Switch)
                                        {
                                            single_events +=
                                                Helper.Format(
                                                    "   setFlipMap({0}, switch_mask, 1); \n" +
                                                    "   setFlipState({0}, true); \n",
                                                    operands);
                                        }
                                        else
                                        {
                                            single_events +=
                                                Helper.Format(
                                                    "   setFlipMap({0}, 0x{1:X2}, 0); \n" +
                                                    "   setFlipState({0}, true); \n",
                                                    operands, triggerMask);
                                        }
                                        break;

                                    case FD_TRIGFUNC.FlipOn:
                                        // FLIP_ON trigger acts one-way even in switch cases, i.e. if you un-pull
                                        // the switch with FLIP_ON trigger, room will remain flipped.
                                        single_events += Helper.Format("   setFlipState({0}, true); \n", operands);
                                        break;

                                    case FD_TRIGFUNC.FlipOff:
                                        // FLIP_OFF trigger acts one-way even in switch cases, i.e. if you un-pull
                                        // the switch with FLIP_OFF trigger, room will remain unflipped.
                                        single_events += Helper.Format("   setFlipState({0}, false); \n", operands);
                                        break;

                                    case FD_TRIGFUNC.LookAt:
                                        single_events += Helper.Format("   setCamTarget({0}, {1}); \n", operands,
                                            timerField);
                                        break;

                                    case FD_TRIGFUNC.EndLevel:
                                        single_events += Helper.Format("   setLevel({0}); \n", operands);
                                        break;

                                    case FD_TRIGFUNC.PlayTrack:
#if !NO_AUDIO
                                            // Override for looped BGM tracks in TR1: if there are any sectors
                                            // triggering looped tracks, ignore it, as BGM is always set in script.
                                            if (EngineWorld.EngineVersion < Loader.Engine.TR2)
                                        {
                                            TR_AUDIO_STREAM_TYPE looped;
                                            string tmp1;
                                            TR_AUDIO_STREAM_METHOD tmp2;
                                            EngineLua.GetSoundtrack(operands, out tmp1, out tmp2, out looped);
                                            if (looped == TR_AUDIO_STREAM_TYPE.Background) break;
                                        }
#endif
                                        single_events += Helper.Format("   playStream({0}, 0x{1:X2}); \n", operands,
                                            (triggerMask << 1) + onlyOnce);
                                        break;

                                    case FD_TRIGFUNC.FlipEffect:
                                        cont_events += Helper.Format("   doEffect({0}, entity_index, {1}); \n",
                                            operands, timerField);
                                        break;

                                    case FD_TRIGFUNC.Secret:
                                        single_events += Helper.Format("   findSecret({0}); \n", operands);
                                        break;

                                    case FD_TRIGFUNC.ClearBodies:
                                        single_events += "   clearBodies(); \n";
                                        break;

                                    case FD_TRIGFUNC.FlyBy:
                                        entry++;
                                        var flybyOnce = (*entry & 0x0100) >> 8;
                                        contBit = (*entry & 0x8000) >> 15;

                                        cont_events += Helper.Format("   playFlyby({0}, {1}); \n", operands,
                                            flybyOnce);
                                        break;

                                    case FD_TRIGFUNC.Cutscene:
                                        single_events += Helper.Format("   playCutscene({0}); \n", operands);
                                        break;

                                    default: // UNKNOWN!
                                        break;
                                }
                            } while (contBit == 0 && entry < end_p);

                            if (!string.IsNullOrWhiteSpace(script))
                            {
                                script += header;

                                // Heavy trigger and antitrigger item events are engaged ONLY
                                // once, when triggering item is approaching sector. Hence, we
                                // copy item events to single events and nullify original item
                                // events sequence to prevent it to be merged into continous
                                // events.

                                if (((TriggerTypes) subFunction).IsAnyOf(TriggerTypes.Heavy,
                                    TriggerTypes.HeavyAntiTrigger))
                                {
                                    if (actionType == ActionType.Anti)
                                    {
                                        single_events += anti_events;
                                    }
                                    else
                                    {
                                        single_events += item_events;
                                    }

                                    anti_events = "";
                                    item_events = "";
                                }

                                if (activator == Activator.Normal) // Ordinary trigger cases.
                                {
                                    if (!string.IsNullOrWhiteSpace(single_events))
                                    {
                                        if (condition != 0)
                                            once_condition += " ";
                                        once_condition += " if(not getEntitySectorStatus(entity_index)) then \n";
                                        script += once_condition;
                                        script += single_events;
                                        script += "   setEntitySectorStatus(entity_index, true); \n";

                                        if (condition != 0)
                                        {
                                            script += "  end;\n"; // First ENDIF is tabbed for extra condition.
                                        }
                                        else
                                        {
                                            script += " end;\n";
                                        }
                                    }

                                    // Item commands kind depends on action type. If type is ANTI, then item
                                    // antitriggering is engaged. If type is normal, ordinary triggering happens
                                    // in cycle with other continous commands. It is needed to prevent timer dispatch
                                    // before activator leaves trigger sector.

                                    if (actionType == ActionType.Anti) // TODO: Duplicate of L3543?
                                    {
                                        script += anti_events;
                                    }
                                    else
                                    {
                                        script += item_events;
                                    }

                                    script += cont_events;
                                    if (condition != 0)
                                        script += " end;\n"; // Additional ENDIF for extra condition.
                                }
                                else // SWITCH, KEY and ITEM cases.
                                {
                                    script += single_events;
                                    script += item_events;
                                    script += cont_events;
                                    if (actionType == ActionType.Switch && activator == Activator.Switch)
                                    {
                                        script += buf2;
                                        if (EngineWorld.EngineVersion < Loader.Engine.TR3 || onlyOnce == 0)
                                        {
                                            script += single_events;
                                            script += anti_events; // Single/continous events are engaged along with
                                            script += cont_events; // antitriggered items, as described above.
                                        }
                                    }
                                    script += " end;\n";
                                }

                                script += "return 1;\n" +
                                          "end }\n"; // Finalize the entry.
                            }

                            if (actionType != ActionType.Bypass)
                            {
                                //Sys.DebugLog("triggers.lua", script);    // Debug!
                                EngineLua.DoString(script);
                            }

#endregion
                        }
                            break;

                        case FD_FUNC.Death:
                            sector.Flags |= SectorFlag.Death;
                            break;

                        case FD_FUNC.Climb:
                            // First 4 sector flags are similar to subfunction layout.
                            sector.Flags |= (SectorFlag) subFunction;
                            break;

                        case FD_FUNC.Monkey:
                            sector.Flags |= SectorFlag.ClimbCeiling;
                            break;

                        case FD_FUNC.MinecartLeft:
                            // Minecart left (TR3) and trigger triggerer mark (TR4-5) has the same flag value.
                            // We re-parse them properly here.
                            if (tr.GameVersion < TRGame.TR4)
                            {
                                sector.Flags |= SectorFlag.MinecartLeft;
                            }
                            else
                            {
                                sector.Flags |= SectorFlag.TriggererMark;
                            }
                            break;

                        case FD_FUNC.MinecartRight:
                            // Minecart right (TR3) and beetle mark (TR4-5) has the same flag value.
                            // We re-parse them properly here.
                            if (tr.GameVersion < TRGame.TR4)
                            {
                                sector.Flags |= SectorFlag.MinecartRight;
                            }
                            else
                            {
                                sector.Flags |= SectorFlag.BeetleMark;
                            }
                            break;

                        default:
                            // Other functions are TR3+ collisional triangle functions.
                            if (((FD_FUNC) function).IsBetween(FD_FUNC.FloorTriangleNW,
                                FD_FUNC.CeilingTriangleNEPortalSE))
                            {
                                entry--; // Go back, since these functions are parsed differently.

                                endBit = (*entry & 0x8000) >> 15; // 0b10000000 00000000

#if NOPE
                    int16_t  slope_t01 = ((*entry) & 0x7C00) >> 10;      // 0b01111100 00000000
                    int16_t  slope_t00 = ((*entry) & 0x03E0) >> 5;       // 0b00000011 11100000
                    // uint16_t slope_func = ((*entry) & 0x001F);            // 0b00000000 00011111
                    // t01/t02 are 5-bit values, where sign is specified by 0x10 mask.
                    if(slope_t01 & 0x10) slope_t01 |= 0xFFF0;
                    if(slope_t00 & 0x10) slope_t00 |= 0xFFF0;
#endif

                                entry++;

                                var slope_t13 = (uint) (*entry & 0xF000) >> 12; // 0b11110000 00000000
                                var slope_t12 = (uint) (*entry & 0x0F00) >> 8; // 0b00001111 00000000
                                var slope_t11 = (uint) (*entry & 0x00F0) >> 4; // 0b00000000 11110000
                                var slope_t10 = (uint) (*entry & 0x000F); // 0b00000000 00001111

                                entry++;

                                var overallAdjustment =
                                    Res_Sector_BiggestCorner(slope_t10, slope_t11, slope_t12, slope_t13) *
                                    TR_METERING_STEP;

                                if (((FD_FUNC) function).IsAnyOf(FD_FUNC.FloorTriangleNW,
                                    FD_FUNC.FloorTriangleNWPortalSW, FD_FUNC.FloorTriangleNWPortalNE))
                                {
                                    sector.FloorDiagonalType = TR_SECTOR_DIAGONAL_TYPE.NorthWest;

                                    sector.FloorCorners[0][2] -= overallAdjustment - slope_t12 * TR_METERING_STEP;
                                    sector.FloorCorners[1][2] -= overallAdjustment - slope_t13 * TR_METERING_STEP;
                                    sector.FloorCorners[2][2] -= overallAdjustment - slope_t10 * TR_METERING_STEP;
                                    sector.FloorCorners[3][2] -= overallAdjustment - slope_t11 * TR_METERING_STEP;

                                    if (function == (int) FD_FUNC.FloorTriangleNWPortalSW)
                                    {
                                        sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalA;
                                    }
                                    else if (function == (int) FD_FUNC.FloorTriangleNWPortalSW)
                                    {
                                        sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalB;
                                    }
                                    else
                                    {
                                        sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Solid;
                                    }
                                }
                                else if (((FD_FUNC) function).IsAnyOf(FD_FUNC.FloorTriangleNE,
                                    FD_FUNC.FloorTriangleNEPortalNW, FD_FUNC.FloorTriangleNEPortalSE))
                                {
                                    sector.FloorDiagonalType = TR_SECTOR_DIAGONAL_TYPE.NorthEast;

                                    sector.FloorCorners[0][2] -= overallAdjustment - slope_t12 * TR_METERING_STEP;
                                    sector.FloorCorners[1][2] -= overallAdjustment - slope_t13 * TR_METERING_STEP;
                                    sector.FloorCorners[2][2] -= overallAdjustment - slope_t10 * TR_METERING_STEP;
                                    sector.FloorCorners[3][2] -= overallAdjustment - slope_t11 * TR_METERING_STEP;

                                    if (function == (int) FD_FUNC.FloorTriangleNEPortalNW)
                                    {
                                        sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalA;
                                    }
                                    else if (function == (int) FD_FUNC.FloorTriangleNEPortalSE)
                                    {
                                        sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalB;
                                    }
                                    else
                                    {
                                        sector.FloorPenetrationConfig = TR_PENETRATION_CONFIG.Solid;
                                    }
                                }
                                else if (((FD_FUNC) function).IsAnyOf(FD_FUNC.CeilingTriangleNW,
                                    FD_FUNC.CeilingTriangleNWPortalSW, FD_FUNC.CeilingTriangleNWPortalNE))
                                {
                                    sector.CeilingDiagonalType = TR_SECTOR_DIAGONAL_TYPE.NorthWest;

                                    sector.CeilingCorners[0][2] += overallAdjustment - slope_t11 * TR_METERING_STEP;
                                    sector.CeilingCorners[1][2] += overallAdjustment - slope_t10 * TR_METERING_STEP;
                                    sector.CeilingCorners[2][2] += overallAdjustment - slope_t13 * TR_METERING_STEP;
                                    sector.CeilingCorners[3][2] += overallAdjustment - slope_t12 * TR_METERING_STEP;

                                    if (function == (int) FD_FUNC.CeilingTriangleNWPortalSW)
                                    {
                                        sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalA;
                                    }
                                    else if (function == (int) FD_FUNC.CeilingTriangleNWPortalSW)
                                    {
                                        sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalB;
                                    }
                                    else
                                    {
                                        sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Solid;
                                    }
                                }
                                else if (((FD_FUNC) function).IsAnyOf(FD_FUNC.CeilingTriangleNE,
                                    FD_FUNC.CeilingTriangleNEPortalNW, FD_FUNC.CeilingTriangleNEPortalSE))
                                {
                                    sector.CeilingDiagonalType = TR_SECTOR_DIAGONAL_TYPE.NorthEast;

                                    sector.CeilingCorners[0][2] += overallAdjustment - slope_t11 * TR_METERING_STEP;
                                    sector.CeilingCorners[1][2] += overallAdjustment - slope_t10 * TR_METERING_STEP;
                                    sector.CeilingCorners[2][2] += overallAdjustment - slope_t13 * TR_METERING_STEP;
                                    sector.CeilingCorners[3][2] += overallAdjustment - slope_t12 * TR_METERING_STEP;

                                    if (function == (int) FD_FUNC.CeilingTriangleNEPortalNW)
                                    {
                                        sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalA;
                                    }
                                    else if (function == (int) FD_FUNC.CeilingTriangleNEPortalSE)
                                    {
                                        sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.DoorVerticalB;
                                    }
                                    else
                                    {
                                        sector.CeilingPenetrationConfig = TR_PENETRATION_CONFIG.Solid;
                                    }
                                }
                            }
                            else
                            {
                                // Unknown floordata function!
                            }
                            break;
                    }

#endregion
                    
                    ret++;
                } while (endBit == 0 && entry < end_p);
            }
#endregion

            return ret;
        }

        public static void TR_Sector_Calculate(World world, Level tr, int room_index)
        {
            var room = world.Rooms[room_index];
            var trRoom = tr.Rooms[room_index];

            // Sectors loading

            for (var i = 0; i < room.Sectors.Count; i++)
            {
                var sector = room.Sectors[i];

                // Let us fill pointers to sectors above and sectors below

                var rp = trRoom.Sectors[i].RoomBelow;
                sector.SectorBelow = null;
                if (rp < world.Rooms.Count && rp != 255)
                {
                    sector.SectorBelow = world.Rooms[rp].GetSectorRaw(sector.Position);
                }
                rp = trRoom.Sectors[i].RoomAbove;
                sector.SectorBelow = null;
                if (rp < world.Rooms.Count && rp != 255)
                {
                    sector.SectorAbove = world.Rooms[rp].GetSectorRaw(sector.Position);
                }

                RoomSector nearSector = null;

                // OX
                if (sector.IndexY.IsBetween(0, room.SectorsY - 1, IB.aEbE) && sector.IndexX == 0)
                {
                    nearSector = room.Sectors[i + room.SectorsY];
                }
                if (sector.IndexY.IsBetween(0, room.SectorsY - 1, IB.aEbE) && sector.IndexX == room.SectorsX - 1)
                {
                    nearSector = room.Sectors[i - room.SectorsY];
                }
                // OY
                if (sector.IndexX.IsBetween(0, room.SectorsX - 1, IB.aEbE) && sector.IndexY == 0)
                {
                    nearSector = room.Sectors[i + 1];
                }
                if (sector.IndexX.IsBetween(0, room.SectorsX - 1, IB.aEbE) && sector.IndexY == room.SectorsY - 1)
                {
                    nearSector = room.Sectors[i - 1];
                }

                if (nearSector != null && sector.PortalToRoom >= 0)
                {
                    foreach (var p in room.Portals)
                    {
                        if(p.Normal.Normal.Z.IsBetween(-0.01f, 0.01f, IB.aEbE))
                        {
                            var dst = p.DestRoom?.GetSectorRaw(sector.Position);
                            var origDst = EngineWorld.Rooms[sector.PortalToRoom].GetSectorRaw(sector.Position);

                            if (dst != null && dst.PortalToRoom < 0 && dst.Floor != TR_METERING_WALLHEIGHT &&
                                dst.Ceiling != TR_METERING_WALLHEIGHT && sector.PortalToRoom != p.DestRoom.ID &&
                                dst.Floor < origDst.Floor && TR_IsSectorsIn2SideOfPortal(nearSector, dst, p))
                            {
                                sector.PortalToRoom = (int)p.DestRoom.ID;
                                origDst = dst; // TODO: What's the point of that?
                            }
                        }
                    }
                }
            }
        }

        public static void tr_setupRoomVertices(World world, Level tr, Loader.Room tr_room, BaseMesh mesh,
            int numCorners, ushort[] vertices, ushort masked_texture, Polygon p)
        {
            p.Vertices.Resize(numCorners, () => new Vertex());

            for (var i = 0; i < numCorners; i++)
            {
                p.Vertices[i].Position = tr_room.Vertices[vertices[i]].Vertex.ToVector3();
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
            p.Vertices.Resize(numCorners, () => new Vertex());

            for (var i = 0; i < numCorners; i++)
            {
                p.Vertices[i].Position = trMesh.Vertices[vertexIndices[i]].ToVector3();
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

        public static void Assert(bool condition, string message = "Incorrect value")
        {
            if (!condition)
                throw new Exception("Assert: " + message);
        }
    }
}

