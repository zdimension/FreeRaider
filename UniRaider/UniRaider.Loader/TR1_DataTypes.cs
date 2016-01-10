using System;
using System.IO;

namespace UniRaider.Loader
{
    public struct TR1Level : ILevel
    {
        public TR2LevelVersion GameVersion { get; set; }

        public tr2_textile8[] Textile8 { get; set; }
        public uint Unused { get; set; }
        public tr1_room[] Rooms { get; set; }
        public ushort[] FloorData { get; set; }
        public tr2_mesh[] Meshes { get; set; }
        public uint[] MeshPointers { get; set; }
        public tr2_animation[] Animations { get; set; }
        public tr2_state_change[] StateChanges { get; set; }
        public tr2_anim_dispatch[] AnimDispatches { get; set; }
        public tr2_anim_command[] AnimCommands { get; set; }
        public tr2_meshtree[] MeshTrees { get; set; }
        public ushort[] Frames { get; set; }
        public tr2_moveable[] Moveables { get; set; }
        public tr2_staticmesh[] StaticMeshes { get; set; }
        public tr2_object_texture[] ObjectTextures { get; set; }
        public tr2_sprite_texture[] SpriteTextures { get; set; }
        public tr2_sprite_sequence[] SpriteSequences { get; set; }
        public tr2_camera[] Cameras { get; set; }
        public tr2_sound_source[] SoundSources { get; set; }
        public tr1_box[] Boxes { get; set; }
        public ushort[] Overlaps { get; set; }
        public tr1_overlap_zone[] Zones { get; set; }
        public ushort[] AnimatedTextures { get; set; }
        public tr1_item[] Items { get; set; }
        public byte[] LightMap { get; set; }
        public tr2_colour[] Palette { get; set; }
        public tr2_cinematic_frame[] CinematicFrames { get; set; }
        public byte[] DemoData { get; set; }
        public short[] SoundMap { get; set; }
        public tr2_sound_details[] SoundDetails { get; set; }
        public byte[] Samples { get; set; }
        public uint[] SampleIndices { get; set; }

        public static unsafe TR1Level Parse(BinaryReader br)
        {
            var lvl = new TR1Level();

            #region Textures

            var numTextiles = br.ReadUInt32();
            lvl.Textile8 = br.ReadArray<tr2_textile8>(numTextiles);

            #endregion

            lvl.Unused = br.ReadUInt32(); // 32-bit unused value (4 bytes)

            #region Rooms

            var numRooms = br.ReadUInt16();
            lvl.Rooms = br.ReadArray<tr1_room>(numRooms);

            #endregion

            #region Floor Data

            var numFloorData = br.ReadUInt32();
            lvl.FloorData = br.ReadUInt16Array(numFloorData);

            #endregion

            #region Meshes

            var numMeshData = br.ReadUInt32();
            var buffer = br.ReadUInt16Array(numMeshData);
            var numMeshPointers = br.ReadUInt32();
            lvl.Meshes = new tr2_mesh[numMeshPointers];
            for (uint i = 0; i < numMeshPointers; i++)
            {
                var meshPointer = br.ReadUInt32();
                if (numMeshData < (meshPointer / 2))
                {
                    throw new Exception("Invalid mesh: " + (meshPointer / 2) + " > " + numMeshData);
                }
                fixed (ushort* ptr = (&buffer[meshPointer / 2]))
                {
                    char* tmpPtr = (char*) ptr;
                    var mem =
                        new BinaryReader(new UnmanagedMemoryStream((byte*) tmpPtr,
                            (numMeshData * 2) - meshPointer));
                    lvl.Meshes[i] = tr2_mesh.Parse(mem);
                }
            }

            #endregion

            #region Moveables

            var numAnimations = br.ReadUInt32();
            lvl.Animations = br.ReadArray<tr2_animation>(numAnimations);
            var numStateChanges = br.ReadUInt32();
            lvl.StateChanges = br.ReadArray<tr2_state_change>(numStateChanges);
            var numAnimDispatches = br.ReadUInt32();
            lvl.AnimDispatches = br.ReadArray<tr2_anim_dispatch>(numAnimDispatches);
            var numAnimCommands = br.ReadUInt32();
            lvl.AnimCommands = br.ReadArray<tr2_anim_command>(numAnimCommands);
            var numMeshTrees = br.ReadUInt32();
            lvl.MeshTrees = br.ReadArray<tr2_meshtree>(numMeshTrees);
            var numFrames = br.ReadUInt32();
            lvl.Frames = br.ReadUInt16Array(numFrames);
            var numMoveables = br.ReadUInt32();
            lvl.Moveables = br.ReadArray<tr2_moveable>(numMoveables);

            #endregion

            #region Static meshes

            var numStaticMeshes = br.ReadUInt32();
            lvl.StaticMeshes = br.ReadArray<tr2_staticmesh>(numStaticMeshes);

            #endregion

            #region Textiles

            var numObjectTextures = br.ReadUInt32();
            lvl.ObjectTextures = br.ReadArray<tr2_object_texture>(numObjectTextures);

            #endregion

            #region Sprites

            var numSpriteTextures = br.ReadUInt32();
            lvl.SpriteTextures = br.ReadArray<tr2_sprite_texture>(numSpriteTextures);
            var numSpriteSequences = br.ReadUInt32();
            lvl.SpriteSequences = br.ReadArray<tr2_sprite_sequence>(numSpriteSequences);

            #endregion

            #region Cameras

            var numCameras = br.ReadUInt32();
            lvl.Cameras = br.ReadArray<tr2_camera>(numCameras);

            #endregion

            #region Sound Sources

            var numSoundSources = br.ReadUInt32();
            lvl.SoundSources = br.ReadArray<tr2_sound_source>(numSoundSources);

            #endregion

            #region Boxes & Overlap zones

            var numBoxes = br.ReadUInt32();
            lvl.Boxes = br.ReadArray<tr1_box>(numBoxes);

            var numOverlaps = br.ReadUInt32();
            lvl.Overlaps = br.ReadUInt16Array(numOverlaps);

            lvl.Zones = br.ReadArray<tr1_overlap_zone>(numBoxes);

            #endregion

            #region Animated textures

            var numAnimatedTextures = br.ReadUInt32();
            lvl.AnimatedTextures = br.ReadUInt16Array(numAnimatedTextures);

            #endregion

            #region Items

            var numItems = br.ReadUInt32();
            lvl.Items = br.ReadArray<tr1_item>(numItems);

            #endregion

            #region Light map (only for 8-bit coloring)

            lvl.LightMap = br.ReadByteArray(8192);

            #endregion

            #region Cinematic frames

            var numCinematicFrames = br.ReadUInt16();
            lvl.CinematicFrames = br.ReadArray<tr2_cinematic_frame>(numCinematicFrames);

            #endregion

            #region Demo data

            var numDemoData = br.ReadUInt16();
            lvl.DemoData = br.ReadByteArray(numDemoData);

            #endregion

            #region Sound map

            lvl.SoundMap = br.ReadInt16Array(256);

            #endregion

            #region Sound details

            var numSoundDetails = br.ReadUInt32();
            lvl.SoundDetails = br.ReadArray<tr2_sound_details>(numSoundDetails);

            #endregion

            #region Samples

            var numSamples = br.ReadUInt32();
            lvl.Samples = br.ReadByteArray(numSamples);

            #endregion

            #region Sample indices

            var numSampleIndices = br.ReadUInt32();
            lvl.SampleIndices = br.ReadUInt32Array(numSampleIndices);

            #endregion

            return lvl;
        }
    }


    /// <summary>
    ///     X/Y/Z are in world coordinates.  Intensity1/Intensity2 are almost always equal. This lighting only affects
    ///     externally-lit objects. Tomb Raider 1 has only the first of the paired Intensity and Fade values.
    /// </summary>
    public struct tr1_room_light
    {
        // 24 bytes [TR1: 18 bytes] 
        public int x; // X-position of light, in world coordinates 
        public int y; // Y-position of light, in world coordinates 
        public int z; // Z-position of light, in world coordinates 
        public ushort Intensity1; // Light intensity  
        public uint Fade1; // Falloff value 1 

        public static tr1_room_light Parse(BinaryReader br)
        {
            return new tr1_room_light
            {
                x = br.ReadInt32(),
                y = br.ReadInt32(),
                z = br.ReadInt32(),
                Intensity1 = br.ReadUInt16(),
                Fade1 = br.ReadUInt32()
            };
        }
    }

    /// <summary>
    ///     This defines the vertices within a room. Room lighting is internal vertex lighting, except for necessarily external
    ///     sources like flares; room ambient lights and point sources are ignored. Tomb Raider 1 has only the first of the two
    ///     light values and lacks the rendering attributes.
    /// </summary>
    public struct tr1_vertex_room
    {
        // 12 bytes [TR1: 8 bytes] 
        public tr2_vertex Vertex; // where this vertex lies (relative to tr2_room_info::x/z) 
        public short Lighting1;

        public static tr1_vertex_room Parse(BinaryReader br)
        {
            return new tr1_vertex_room
            {
                Vertex = tr2_vertex.Parse(br),
                Lighting1 = br.ReadInt16()
            };
        }
    }

    /// <summary>
    ///     Positions and IDs of static meshes (e.g. skeletons, spiderwebs, furniture, trees). This is comparable to the
    ///     <see cref="tr2_item" /> structure, except that static meshes typically have no animations and are confined to a
    ///     single room.
    /// </summary>
    public struct tr1_room_staticmesh
    {
        public uint x; // absolute X position in world coordinates 
        public uint y; // absolute Y position in world coordinates 
        public uint z; // absolute Z position in world coordinates 
        public ushort Rotation; // high two bits (0xC000) indicate steps of 90 degrees (e.g. (Rotation >> 14) * 90)
        public ushort Intensity1; // Constant lighting; -1 means use mesh lighting
        public ushort ObjectID; // which StaticMesh item to draw

        public static tr1_room_staticmesh Parse(BinaryReader br)
        {
            return new tr1_room_staticmesh
            {
                x = br.ReadUInt32(),
                y = br.ReadUInt32(),
                z = br.ReadUInt32(),
                Rotation = br.ReadUInt16(),
                Intensity1 = br.ReadUInt16(),
                ObjectID = br.ReadUInt16()
            };
        }
    }

    /// <summary>
    ///     Here's where all the room data come together. As it's stored in the file, the <see cref="tr2_room_info" />
    ///     structure comes first, followed by a <see cref="uint" /> NumDataWords, which specifies the number of 16-bit words
    ///     to follow.  Those data words must be parsed in order to interpret and construct the variable-length arrays of
    ///     vertices, meshes, doors, and sectors.
    /// </summary>
    public struct tr1_room
    {
        public tr2_room_info info; // where the room exists, in world coordinates 
        public tr2_room_data RoomData; // the room mesh
        public tr2_room_portal[] Portals; // list of visibility portals
        public ushort NumZsectors; // "width" of sector list
        public ushort NumXsectors; // "height" of sector list
        public tr2_room_sector[] SectorList; // list of sectors in this room
        public short AmbientIntensity1; // this and the next one only affect externally-lit objects
        public tr1_room_light[] Lights; // list of point lights
        public tr1_room_staticmesh[] StaticMeshes; // list of static meshes
        public short AlternateRoom; // number of the room that this room can alternate with
        public tr2_room_Flags Flags;

        public static tr1_room Parse(BinaryReader br)
        {
            var ret = new tr1_room();
            ret.info = tr2_room_info.Parse(br);
            var numData = br.ReadUInt32();
            ret.RoomData = tr2_room_data.Parse(br);
            var numPortals = br.ReadUInt16();
            ret.Portals = br.ReadArray<tr2_room_portal>(numPortals);
            ret.NumZsectors = br.ReadUInt16();
            ret.NumXsectors = br.ReadUInt16();
            ret.SectorList = br.ReadArray<tr2_room_sector>(ret.NumZsectors * ret.NumXsectors);
            ret.AmbientIntensity1 = br.ReadInt16();
            var numLights = br.ReadUInt16();
            ret.Lights = br.ReadArray<tr1_room_light>(numLights);
            var numStaticMeshes = br.ReadUInt16();
            ret.StaticMeshes = br.ReadArray<tr1_room_staticmesh>(numStaticMeshes);
            ret.AlternateRoom = br.ReadInt16();
            ret.Flags = (tr2_room_Flags) br.ReadInt16();
            return ret;
        }
    }

    /// <summary>
    ///     Items are instances of objects, which can be sprite sequences or movables. For an object to appear in a level, it
    ///     must be referenced in the Items[] array. Multiple instances are possible (e.g. two identical tigers in different
    ///     rooms are represented using two entries in Items[], one for each). The object ID is used to locate the appropriate
    ///     sprite sequence or movable for the item.
    /// </summary>
    public struct tr1_item
    {
        public short ObjectID; // Object Identifier (matched in Moveables[], or SpriteSequences[], as appropriate)
        public short Room; // which room contains this item
        public int x; // item position in world coordinates
        public int y;
        public int z;
        public short Angle; // ((0xc000 >> 14) * 90) degrees
        public short Intensity1; // (constant lighting; -1 means use mesh lighting)
        public tr2_item_Flags Flags;

        public static tr1_item Parse(BinaryReader br)
        {
            return new tr1_item
            {
                ObjectID = br.ReadInt16(),
                Room = br.ReadInt16(),
                x = br.ReadInt32(),
                y = br.ReadInt32(),
                z = br.ReadInt32(),
                Angle = br.ReadInt16(),
                Intensity1 = br.ReadInt16(),
                Flags = (tr2_item_Flags) br.ReadUInt16()
            };
        }
    }

    public struct tr1_box
    {
        public int Zmin;
        public int Zmax;
        public int Xmin;
        public int Xmax;
        public short TrueFloor; // Y value (no scaling) 

        public short OverlapIndex;
            // index into Overlaps[]. The high bit is sometimes set; this occurs in front of swinging doors and the like. 

        public static tr1_box Parse(BinaryReader br)
        {
            return new tr1_box
            {
                Zmin = br.ReadInt32(),
                Zmax = br.ReadInt32(),
                Xmin = br.ReadInt32(),
                Xmax = br.ReadInt32(),
                TrueFloor = br.ReadInt16(),
                OverlapIndex = br.ReadInt16()
            };
        }
    }

    public struct tr1_overlap_zone
    {
        // Normal room state
        public short ground1;
        public short ground2;
        public short fly;
        // Normal room state
        public short ground1alt;
        public short ground2alt;
        public short flyAlt;

        public static tr1_overlap_zone Parse(BinaryReader br)
        {
            return new tr1_overlap_zone
            {
                ground1 = br.ReadInt16(),
                ground2 = br.ReadInt16(),
                fly = br.ReadInt16(),
                ground1alt = br.ReadInt16(),
                ground2alt = br.ReadInt16(),
                flyAlt = br.ReadInt16()
            };
        }
    }
}