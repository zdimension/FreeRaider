using System;
using System.IO;
using System.Linq;

namespace FreeRaider
{
    public partial class Constants
    {
        public const int TR_AUDIO_MAP_SIZE_TR3 = 370;
    }
}
namespace FreeRaider.Loader
{
    public class TR3Level : Level
    {
        public TR3Level(BinaryReader br, TRGame ver) : base(br, ver)
        {
        }

        public override void Load()
        {
            var version = reader.ReadUInt32();

            if (version.NotEqual(0xFF080038, 0xFF180038, 0xFF180034))
                throw new ArgumentException("TR3Level.Load: Wrong level version");

            Palette = Palette.Read(reader, Engine.TR1);

            /* Palette16 = TODO: */
            Palette.Read(reader, Engine.TR2);

            var numTextiles = reader.ReadUInt32();
            var texture8 = reader.ReadArray(numTextiles, () => ByteTexture.Read(reader));
            var texture16 = reader.ReadArray(numTextiles, () => WordTexture.Read(reader));

            if (version == 0xFF180034)
                return; // VICT.TR2, only palette and textiles

            var unused = reader.ReadUInt32();
            if (unused != 0)
                Cerr.Write("TR3Level.Load: unused: Expected 0, Found " + unused.ToString("X8"));

            var numRooms = reader.ReadUInt16();
            Rooms = reader.ReadArray(numRooms, () => Room.Read(reader, Engine.TR3));

            var numFloorData = reader.ReadUInt32();
            FloorData = reader.ReadUInt16Array(numFloorData);

            ReadMeshData();

            var numAnimations = reader.ReadUInt32();
            Animations = reader.ReadArray(numAnimations, () => Animation.Read(reader, Engine.TR3));

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

            var numSpriteTextures = reader.ReadUInt32();
            SpriteTextures = reader.ReadArray(numSpriteTextures, () => SpriteTexture.Read(reader, Engine.TR3));

            var numSpriteSequences = reader.ReadUInt32();
            SpriteSequences = reader.ReadArray(numSpriteSequences, () => SpriteSequence.Read(reader));

            var numCameras = reader.ReadUInt32();
            Cameras = reader.ReadArray(numCameras, () => Camera.Read(reader));

            var numSoundSources = reader.ReadUInt32();
            SoundSources = reader.ReadArray(numSoundSources, () => SoundSource.Read(reader));

            var numBoxes = reader.ReadUInt32();
            Boxes = reader.ReadArray(numBoxes, () => Box.Read(reader, Engine.TR3));

            var numOverlaps = reader.ReadUInt32();
            Overlaps = reader.ReadUInt16Array(numOverlaps);

            Zones = reader.ReadArray(numBoxes, () => Zone.Read(reader, Engine.TR3));

            var numAnimatedTextures = reader.ReadUInt32();
            AnimatedTextures = reader.ReadUInt16Array(numAnimatedTextures);

            var numObjectTextures = reader.ReadUInt32();
            ObjectTextures = reader.ReadArray(numObjectTextures, () => ObjectTexture.Read(reader, Engine.TR3));

            var numItems = reader.ReadUInt32();
            Items = reader.ReadArray(numItems, () => Item.Read(reader, Engine.TR3));

            LightMap = LightMap.Read(reader);

            var numCinematicFrames = reader.ReadUInt16();
            reader.ReadArray(numCinematicFrames, () => CinematicFrame.Read(reader));

            var numDemoData = reader.ReadUInt16();
            DemoData = reader.ReadBytes(numDemoData);

            SoundMap = reader.ReadInt16Array(Constants.TR_AUDIO_MAP_SIZE_TR3);

            var numSoundDetails = reader.ReadUInt32();
            SoundDetails = reader.ReadArray(numSoundDetails, () => Loader.SoundDetails.Read(reader, Engine.TR3));

            var numSampleIndices = reader.ReadUInt32();
            SampleIndices = reader.ReadUInt32Array(numSampleIndices);

            for (uint i = 0; i < numSoundDetails; i++)
            {
                if (SoundDetails[i].Sample < numSampleIndices)
                    SoundDetails[i].Sample = (ushort) SampleIndices[SoundDetails[i].Sample];
            }

            if (!File.Exists(SfxPath))
                Cerr.Write("TR3Level.Load: '" + SfxPath + "' not found, no samples loaded");
            else
            {
                using (var fs = new FileStream(SfxPath, FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        SamplesData = new byte[fs.Length];

                        for (long i = 0; i < SamplesData.Length; i++)
                        {
                            SamplesData[i] = br.ReadByte();

                            if (i >= 4)
                            {
                                if (SamplesData[i - 4] == 82
                                    && SamplesData[i - 3] == 73
                                    && SamplesData[i - 2] == 70
                                    && SamplesData[i - 1] == 70)
                                {
                                    SamplesCount++;
                                }
                            }
                        }
                    }
                }
            }

            Textures = new DWordTexture[numTextiles];
            for (uint i = 0; i < numTextiles; i++)
            {
                Textures[i] = ConvertTexture(texture16[i]);
            }

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Cerr.Write("Error: " + (reader.BaseStream.Length - reader.BaseStream.Position) + " bytes of data after end of level");
            }
        }
    }
}
