using System;
using System.IO;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int TR_AUDIO_MAP_SIZE_TR4 = 370;
    }
}
namespace FreeRaider.Loader
{
    public class TR4Level : Level
    {
        public TR4Level(BinaryReader br, TRGame ver) : base(br, ver)
        {
        }

        public override void Load()
        {
            var version = reader.ReadUInt32();

            if (version != 0x00345254)
                throw new ArgumentException("TR4Level.Load: Wrong level version");

            var texture16 = new WordTexture[0];

            var numRoomTextiles = reader.ReadUInt16();
            var numObjTextiles = reader.ReadUInt16();
            var numBumpTextiles = reader.ReadUInt16();
            var numMiscTextiles = 2;
            var numTextiles = numRoomTextiles + numObjTextiles + numBumpTextiles + numMiscTextiles;

            var uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("TR4Level.Load: Textiles32 uncompSize == 0", nameof(uncompSize));

            var compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                var compBuffer = reader.ReadBytes((int) compSize);

                var newsrc = Helper.Decompress(compBuffer);
                Textures = newsrc.ReadArray(numTextiles - numMiscTextiles, () => DWordTexture.Read(newsrc));
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("TR4Level.Load: Textiles16 uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                if (Textures.Length == 0)
                {
                    var compBuffer = reader.ReadBytes((int) compSize);

                    var newsrc = Helper.Decompress(compBuffer);
                    texture16 = newsrc.ReadArray(numTextiles - numMiscTextiles, () => WordTexture.Read(newsrc));
                }
                else
                {
                    reader.BaseStream.Position += compSize;
                }
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("TR4Level.Load: Textiles32d uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                if (Textures.Length == 0)
                {
                    // 262144 = 256*256*4
                    if (uncompSize / 262144 > 2)
                        Cerr.Write("TR4Level.Load: NumMiscTextiles > 2");

                    Array.Resize(ref Textures, numTextiles);

                    var compBuffer = reader.ReadBytes((int) compSize);

                    var newsrc = Helper.Decompress(compBuffer);
                    Textures = newsrc.ReadArray(numMiscTextiles, () => DWordTexture.Read(newsrc));
                }
                else
                {
                    reader.BaseStream.Position += compSize;
                }
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("TR4Level.Load: Packed geometry uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize == 0)
                throw new ArgumentException("TR4Level.Load: Packed geometry", nameof(compSize));

            var compBuffer_ = reader.ReadBytes((int) compSize);

            reader = Helper.Decompress(compBuffer_);

            var unused = reader.ReadUInt32();
            if (unused != 0)
                Cerr.Write("TR4Level.Load: unused: Expected 0, Found " + unused.ToString("X8"));

            var numRooms = reader.ReadUInt16();
            Rooms = reader.ReadArray(numRooms, () => Room.Read(reader, Engine.TR4));

            var numFloorData = reader.ReadUInt32();
            FloorData = reader.ReadUInt16Array(numFloorData);

            ReadMeshData();

            var numAnimations = reader.ReadUInt32();
            Animations = reader.ReadArray(numAnimations, () => Animation.Read(reader, Engine.TR4));

            var numStateChanges = reader.ReadUInt32();
            StateChanges = reader.ReadArray(numStateChanges, () => StateChange.Read(reader));

            var numAnimDispatches = reader.ReadUInt32();
            AnimDispatches = reader.ReadArray(numAnimDispatches, () => AnimDispatch.Read(reader));

            var numAnimCommands = reader.ReadUInt32();
            AnimCommands = reader.ReadInt16Array(numAnimCommands);

            var numMeshTreeData = reader.ReadUInt32();
            MeshTreeData = reader.ReadUInt32Array(numMeshTreeData);

            ReadFrameMoveableData();

            var numStaticMeshes = reader.ReadUInt32();
            StaticMeshes = reader.ReadArray(numStaticMeshes, () => StaticMesh.Read(reader));

            var spr1 = (char)reader.ReadSByte();
            var spr2 = (char)reader.ReadSByte();
            var spr3 = (char)reader.ReadSByte();
            var spr = "" + spr1 + spr2 + spr3;
            if(spr != "SPR")
                throw new ArgumentException("TR4Level.Load: Expected 'SPR', Found '" + spr + "'", nameof(spr));

            var numSpriteTextures = reader.ReadUInt32();
            SpriteTextures = reader.ReadArray(numSpriteTextures, () => SpriteTexture.Read(reader, Engine.TR4));

            var numSpriteSequences = reader.ReadUInt32();
            SpriteSequences = reader.ReadArray(numSpriteSequences, () => SpriteSequence.Read(reader));

            var numCameras = reader.ReadUInt32();
            Cameras = reader.ReadArray(numCameras, () => Camera.Read(reader));

            var numFlybyCameras = reader.ReadUInt32();
            FlybyCameras = reader.ReadArray(numFlybyCameras, () => FlybyCamera.Read(reader));

            var numSoundSources = reader.ReadUInt32();
            SoundSources = reader.ReadArray(numSoundSources, () => SoundSource.Read(reader));

            var numBoxes = reader.ReadUInt32();
            Boxes = reader.ReadArray(numBoxes, () => Box.Read(reader, Engine.TR2));

            var numOverlaps = reader.ReadUInt32();
            Overlaps = reader.ReadUInt16Array(numOverlaps);

            Zones = reader.ReadArray(numBoxes, () => Zone.Read(reader, Engine.TR2));

            var numAnimatedTextures = reader.ReadUInt32();
            AnimatedTextures = reader.ReadUInt16Array(numAnimatedTextures);

            AnimatedTexturesUVCount = reader.ReadByte();

            var tex1 = (char)reader.ReadSByte();
            var tex2 = (char)reader.ReadSByte();
            var tex3 = (char)reader.ReadSByte();
            var tex = "" + tex1 + tex2 + tex3;
            if (tex != "TEX")
                throw new ArgumentException("TR4Level.Load: Expected 'TEX', Found '" + tex + "'", nameof(tex));

            var numObjectTextures = reader.ReadUInt32();
            ObjectTextures = reader.ReadArray(numObjectTextures, () => ObjectTexture.Read(reader, Engine.TR4));

            var numItems = reader.ReadUInt32();
            Items = reader.ReadArray(numItems, () => Item.Read(reader, Engine.TR4));

            var numAiObjects = reader.ReadUInt32();
            AIObjects = reader.ReadArray(numAiObjects, () => AIObject.Read(reader));

            var numDemoData = reader.ReadUInt16();
            DemoData = reader.ReadBytes(numDemoData);

            SoundMap = reader.ReadInt16Array(Constants.TR_AUDIO_MAP_SIZE_TR4);

            var numSoundDetails = reader.ReadUInt32();
            SoundDetails = reader.ReadArray(numSoundDetails, () => Loader.SoundDetails.Read(reader, Engine.TR3));

            var numSampleIndices = reader.ReadUInt32();
            SampleIndices = reader.ReadUInt32Array(numSampleIndices);

            var numSamples = reader.ReadUInt32();
            if(numSamples > 0)
            {
                SamplesCount = (int) numSamples;
                SamplesData = reader.ReadBytes((int)(reader.BaseStream.Length - reader.BaseStream.Position));
            }

            if (Textures.Length == 0)
            {
                Textures = new DWordTexture[texture16.Length];
                for (uint i = 0; i < texture16.Length; i++)
                {
                    Textures[i] = ConvertTexture(texture16[i]);
                }
            }

            var unused2 = reader.ReadUInt16(); // I don't know what it is used for, but there's always 2 bytes of data at the end of TR4 levels

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Cerr.Write("Error: " + (reader.BaseStream.Length - reader.BaseStream.Position) +
                                    " bytes of data after end of level");
            }
        }
    }
}
