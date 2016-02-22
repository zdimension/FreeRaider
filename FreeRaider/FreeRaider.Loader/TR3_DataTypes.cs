using System;
using System.IO;
using System.Runtime.InteropServices;

namespace FreeRaider.Loader
{
    public struct TR3Level : ILevel
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] private tr2_colour[] _palette;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)] private tr2_colour4[] _palette16;

        public TR2LevelVersion GameVersion { get; set; }

        public tr2_colour[] Palette
        {
            get { return _palette; }
            set { _palette = value; }
        }

        public tr2_colour4[] Palette16
        {
            get { return _palette16; }
            set { _palette16 = value; }
        }

        public tr2_textile8[] Textile8 { get; set; }
        public tr2_textile16[] Textile16 { get; set; }
        public uint Unused { get; set; }
        public tr3_room[] Rooms { get; set; }
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
        public tr2_sprite_texture[] SpriteTextures { get; set; }
        public tr2_sprite_sequence[] SpriteSequences { get; set; }
        public tr2_camera[] Cameras { get; set; }
        public tr2_sound_source[] SoundSources { get; set; }
        public tr2_box[] Boxes { get; set; }
        public ushort[] Overlaps { get; set; }
        public tr2_overlap_zone[] Zones { get; set; }
        public ushort[] AnimatedTextures { get; set; }
        public tr2_object_texture[] ObjectTextures { get; set; }
        public tr2_item[] Items { get; set; }
        public byte[] LightMap { get; set; }
        public tr2_cinematic_frame[] CinematicFrames { get; set; }
        public byte[] DemoData { get; set; }
        public short[] SoundMap { get; set; }
        public tr2_sound_details[] SoundDetails { get; set; }
        public uint[] SampleIndices { get; set; }

        public static unsafe TR3Level Parse(BinaryReader br)
        {
            var lvl = new TR3Level();

            #region Palette

            lvl.Palette = br.ReadArray<tr2_colour>(256);
            lvl.Palette16 = br.ReadArray<tr2_colour4>(256);

            #endregion

            #region Textures

            var numTextiles = br.ReadUInt32();
            lvl.Textile8 = br.ReadArray<tr2_textile8>(numTextiles);
            lvl.Textile16 = br.ReadArray<tr2_textile16>(numTextiles);

            #endregion

            lvl.Unused = br.ReadUInt32(); // 32-bit unused value (4 bytes)

            #region Rooms

            var numRooms = br.ReadUInt16();
            lvl.Rooms = br.ReadArray<tr3_room>(numRooms);

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
                if (numMeshData < meshPointer / 2)
                {
                    throw new Exception("Invalid mesh: " + meshPointer / 2 + " > " + numMeshData);
                }
                fixed (ushort* ptr = &buffer[meshPointer / 2])
                {
                    char* tmpPtr = (char*) ptr;
                    var mem =
                        new BinaryReader(new UnmanagedMemoryStream((byte*) tmpPtr,
                            numMeshData * 2 - meshPointer));
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
            lvl.Boxes = br.ReadArray<tr2_box>(numBoxes);

            var numOverlaps = br.ReadUInt32();
            lvl.Overlaps = br.ReadUInt16Array(numOverlaps);

            lvl.Zones = br.ReadArray<tr2_overlap_zone>(numBoxes);

            #endregion

            #region Animated textures

            var numAnimatedTextures = br.ReadUInt32();
            lvl.AnimatedTextures = br.ReadUInt16Array(numAnimatedTextures);

            #endregion

            #region Textiles

            var numObjectTextures = br.ReadUInt32();
            lvl.ObjectTextures = br.ReadArray<tr2_object_texture>(numObjectTextures);

            #endregion

            #region Items

            var numItems = br.ReadUInt32();
            lvl.Items = br.ReadArray<tr2_item>(numItems);

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

            lvl.SoundMap = br.ReadInt16Array(370);

            #endregion

            #region Sound details

            var numSoundDetails = br.ReadUInt32();
            lvl.SoundDetails = br.ReadArray<tr2_sound_details>(numSoundDetails);

            #endregion

            #region Sample indices

            var numSampleIndices = br.ReadUInt32();
            lvl.SampleIndices = br.ReadUInt32Array(numSampleIndices);

            #endregion

            return lvl;
        }
    }


    /// <summary>
    ///     Here's where all the room data come together. As it's stored in the file, the <see cref="tr2_room_info" />
    ///     structure comes first, followed by a <see cref="uint" /> NumDataWords, which specifies the number of 16-bit words
    ///     to follow.  Those data words must be parsed in order to interpret and construct the variable-length arrays of
    ///     vertices, meshes, doors, and sectors.
    /// </summary>
    public struct tr3_room
    {
        public tr2_room_info info; // where the room exists, in world coordinates 
        public tr2_room_data RoomData; // the room mesh
        public tr2_room_portal[] Portals; // list of visibility portals
        public ushort NumZsectors; // "width" of sector list
        public ushort NumXsectors; // "height" of sector list
        public tr2_room_sector[] SectorList; // list of sectors in this room
        public short AmbientIntensity1; // this and the next one only affect externally-lit objects

        public short AmbientIntensity2;
        // almost always the same value as AmbientIntensity1 [absent from TR1 data files]
        public tr2_room_light[] Lights; // list of point lights
        public tr2_room_staticmesh[] StaticMeshes; // list of static meshes
        public short AlternateRoom; // number of the room that this room can alternate with
        public tr2_room_Flags Flags;
        public tr2_colour RoomLightColour; // Present in TR3 only; absent from TR1/TR2

        public static tr3_room Parse(BinaryReader br)
        {
            var ret = new tr3_room();
            ret.info = tr2_room_info.Parse(br);
            var numData = br.ReadUInt32();
            ret.RoomData = tr2_room_data.Parse(br);
            var numPortals = br.ReadUInt16();
            ret.Portals = br.ReadArray<tr2_room_portal>(numPortals);
            ret.NumZsectors = br.ReadUInt16();
            ret.NumXsectors = br.ReadUInt16();
            ret.SectorList = br.ReadArray<tr2_room_sector>(ret.NumZsectors * ret.NumXsectors);
            ret.AmbientIntensity1 = br.ReadInt16();
            ret.AmbientIntensity2 = br.ReadInt16();
            var numLights = br.ReadUInt16();
            ret.Lights = br.ReadArray<tr2_room_light>(numLights);
            var numStaticMeshes = br.ReadUInt16();
            ret.StaticMeshes = br.ReadArray<tr2_room_staticmesh>(numStaticMeshes);
            ret.AlternateRoom = br.ReadInt16();
            ret.Flags = (tr2_room_Flags) br.ReadInt16();
            ret.RoomLightColour = tr2_colour.Parse(br);
            return ret;
        }
    }
}