using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

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
        Ghost = 4, // no collision.
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
        public static void Res_GenRBTrees(World world);

        public static void Res_GenSpritesBuffer(World world);

        public static void Res_GenRoomSpritesBuffer(Room room);

        public static void Res_GenRoomCollision(World world);

        public static void Res_GenRoomFlipMap(World world);

        public static void Res_GenBaseItems(World world);

        public static void Res_GenVBOs(World world);

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

        public static void Res_Sector_FixHeights(RoomSector sector);

        public static bool Res_Poly_SetAnimTexture(Polygon polygon, uint texIndex, World world);

        public static void Res_FixRooms(World world); // Fix start-up room states.

        public static SkeletalModel Res_GetSkybox(World world, Loader.Engine engine_version);

        // Create entity function from script, if exists.

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

            if (Global.EngineWorld.GetEntityByID(entity_index) == null) return true;

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

        public static void Res_AutoexecOpen(Loader.Game engine_version);

        // Functions generating native OpenTomb structs from legacy TR structs.

        public static void TR_GenWorld(World world, Loader.Level tr);

        public static void TR_GenMeshes(World world, Loader.Level tr);

        public static void TR_GenMesh(World world, int mesh_index, BaseMesh mesh, Loader.Level tr);

        public static void TR_GenSkeletalModels(World world, Loader.Level tr);

        public static void TR_GenSkeletalModel(int model_id, SkeletalModel model, Loader.Level tr);

        public static void TR_GenEntities(World world, Loader.Level tr);

        public static void TR_GenSprites(World world, Loader.Level tr);

        public static void TR_GenTextures(World world, Loader.Level tr);

        public static void TR_GenAnimCommands(World world, Loader.Level tr);

        public static void TR_GenAnimTextures(World world, Loader.Level tr);

        public static void TR_GenRooms(World world, Loader.Level tr);

        public static void TR_GenRoom(int room_index, Room room, World world, Loader.Level tr);

        public static void TR_GenRoomProperties(World world, Loader.Level tr);

        public static void TR_GenBoxes(World world, Loader.Level tr);

        public static void TR_GenCameras(World world, Loader.Level tr);

        public static void TR_GenSamples(World world, Loader.Level tr);

        // Helper functions to convert legacy TR structs to native OpenTomb structs.

        public static void TR_vertex_to_arr(Vector3 v, Loader.Vertex tr_v);

        /// <summary>
        /// v[] of length 4
        /// </summary>
        public static void TR_color_to_arr(float[] v, Loader.FloatColor tr_c);

        // Functions for getting various parameters from legacy TR structs.

        public static void TR_GetBFrameBB_Pos(Loader.Level tr, int frame_offset, BoneFrame bone_frame);

        public static int TR_GetNumAnimationsForMoveable(Loader.Level tr, int moveable_ind);

        public static int TR_GetNumFramesForAnimation(Loader.Level tr, int animation_ind);

        public static long TR_GetOriginalAnimationFrameOffset(uint offset, uint anim, Loader.Level tr);

        // Main functions which are used to translate legacy TR floor data
        // to native OpenTomb structs.

        public static int TR_Sector_TranslateFloorData(RoomSector sector, Loader.Level tr);

        public static void TR_Sector_Calculate(World world, Loader.Level tr, long room_index);

        public static void tr_setupRoomVertices(World world, Loader.Level tr, Loader.Room tr_room, BaseMesh mesh,
            int numCorners, ushort vertices, ushort masked_texture, Polygon p);

        public static void tr_copyNormals(Polygon polygon, BaseMesh mesh, ushort mesh_vertex_indices);
    }
}
