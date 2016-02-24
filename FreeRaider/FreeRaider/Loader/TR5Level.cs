using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreeRaider.Loader
{
    public partial class Constants
    {
        public const int TR_AUDIO_MAP_SIZE_TR5 = 450;
    }

    public class TR5Level : Level
    {
        public TR5Level(BinaryReader br, Engine ver) : base(br, ver)
        {
            Load();
        }

        public TR5Level(BinaryReader br, Game ver) : base(br, ver)
        {
            Load();
        }

        private void Load()
        {
            var version = reader.ReadUInt32();

            if (version != 0x00345254)
                throw new ArgumentException("TR5Level.Load: Wrong level version");

            var numRoomTextiles = reader.ReadUInt16();
            var numObjTextiles = reader.ReadUInt16();
            var numBumpTextiles = reader.ReadUInt16();
            var numMiscTextiles = 3;
            var numTextiles = numRoomTextiles + numObjTextiles + numBumpTextiles + numMiscTextiles;

            var uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("TR5Level.Load: Textiles32 uncompSize == 0", nameof(uncompSize));

            var compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                var compBuffer = reader.ReadBytes((int)compSize);

                var newsrc = Helper.Decompress(compBuffer);
                Textures = newsrc.ReadArray(numTextiles - numMiscTextiles, () => DWordTexture.Read(newsrc));
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("TR5Level.Load: Textiles16 uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            var texture16 = new WordTexture[0];
            if (compSize > 0)
            {
                if (Textures.Length == 0)
                {
                    var compBuffer = reader.ReadBytes((int)compSize);

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
                throw new ArgumentException("TR5Level.Load: Textiles32d uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                // 262144 = Width * Height * Depth
                //        =  256  *  256   *  4
                if (uncompSize / 262144 > 3)
                    Cerr.Write("TR5Level.Load: NumMiscTextiles > 3");

                var compBuffer = reader.ReadBytes((int) compSize);

                var newsrc = Helper.Decompress(compBuffer);
                var t = newsrc.ReadArray(numMiscTextiles, () => DWordTexture.Read(newsrc));

                Textures = Textures.AddArray(t);
            }

            LaraType = (LaraType) reader.ReadUInt16();
            WeatherType = (WeatherType) reader.ReadUInt16();

            var flags = reader.ReadUInt32Array(7);
            for (var i = 0; i < flags.Length; i++)
            {
                if (flags[i] != 0)
                    Cerr.Write("TR5Level.Load: flags[" + (i + 1) + "]: Expected 0, Found 0x" + flags[i].ToString("X8"));
            }

            var levelDataSize1 = reader.ReadUInt32();
            var levelDataSize2 = reader.ReadUInt32();

            var unused = reader.ReadUInt32();
            if(unused != 0)
                Cerr.Write("TR5Level.Load: unused: Expected 0, Found " + unused.ToString("X8"));

            var numRooms = reader.ReadUInt32();
            Rooms = reader.ReadArray(numRooms, () => Room.Read(reader, Engine.TR5));

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

            var spr1 = (char) reader.ReadSByte();
            var spr2 = (char) reader.ReadSByte();
            var spr3 = (char) reader.ReadSByte();
            var spr4 = (char) reader.ReadSByte();
            var spr = "" + spr1 + spr2 + spr3 + spr4;
            if (spr != "SPR\0")
                throw new ArgumentException("TR5Level.Load: Expected 'SPR', Found '" + spr + "'", nameof(spr));

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
            var tex4 = (char) reader.ReadSByte();
            var tex = "" + tex1 + tex2 + tex3 + tex4;
            if (tex != "TEX\0")
                throw new ArgumentException("TR5Level.Load: Expected 'TEX', Found '" + tex + "'", nameof(tex));

            var numObjectTextures = reader.ReadUInt32();
            ObjectTextures = reader.ReadArray(numObjectTextures, () => ObjectTexture.Read(reader, Engine.TR5));

            var numItems = reader.ReadUInt32();
            Items = reader.ReadArray(numItems, () => Item.Read(reader, Engine.TR4));

            var numAiObjects = reader.ReadUInt32();
            AIObjects = reader.ReadArray(numAiObjects, () => AIObject.Read(reader));

            var numDemoData = reader.ReadUInt16();
            DemoData = reader.ReadBytes(numDemoData);

            SoundMap = reader.ReadInt16Array(Constants.TR_AUDIO_MAP_SIZE_TR5);

            var numSoundDetails = reader.ReadUInt32();
            SoundDetails = reader.ReadArray(numSoundDetails, () => Loader.SoundDetails.Read(reader, Engine.TR3));

            var numSampleIndices = reader.ReadUInt32();
            SampleIndices = reader.ReadUInt32Array(numSampleIndices);

            reader.BaseStream.Position += 6;

            var numSamples = reader.ReadUInt32();
            if (numSamples > 0)
            {
                SamplesCount = (int)numSamples;
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

            if (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                Cerr.Write("Error: " + (reader.BaseStream.Length - reader.BaseStream.Position) +
                                    " bytes of data after end of level");
            }
        }
    }
}
