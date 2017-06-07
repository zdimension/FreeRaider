using System;
using System.IO;
using System.Linq;

namespace FreeRaider.Loader
{
    public partial class Constants
    {
        public const int TR_AUDIO_MAP_SIZE_TR5 = 450;
    }

    public partial class Level
    {
        private void Load_TR5()
        {
            var version = reader.ReadUInt32();

            if (version != 0x00345254)
                throw new ArgumentException("Load_TR5: Wrong level version");

            var numRoomTextiles = reader.ReadUInt16();
            var numObjTextiles = reader.ReadUInt16();
            var numBumpTextiles = reader.ReadUInt16();
            var numMiscTextiles = 3;
            var numTextiles = numRoomTextiles + numObjTextiles + numBumpTextiles + numMiscTextiles;

            var uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR5: Textiles32 uncompSize == 0", nameof(uncompSize));

            var compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                var compBuffer = reader.ReadBytes((int)compSize);

                var newsrc = Helper.Decompress(compBuffer);
                Textures = newsrc.ReadArray(numTextiles - numMiscTextiles, () => DWordTexture.Read(newsrc));
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR5: Textiles16 uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            Texture16 = new WordTexture[0];
            if (compSize > 0)
            {
                if (Textures.Length == 0)
                {
                    var compBuffer = reader.ReadBytes((int)compSize);

                    var newsrc = Helper.Decompress(compBuffer);
                    Texture16 = newsrc.ReadArray(numTextiles - numMiscTextiles, () => WordTexture.Read(newsrc));
                }
                else
                {
                    reader.BaseStream.Position += compSize;
                }
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR5: Textiles32d uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                // 262144 = Width * Height * Depth
                //        =  256  *  256   *  4
                if (uncompSize / 262144 > 3)
                    Cerr.Write("Load_TR5: NumMiscTextiles > 3");

                var compBuffer = reader.ReadBytes((int) compSize);

                var newsrc = Helper.Decompress(compBuffer);
                var t = newsrc.ReadArray(numMiscTextiles, () => DWordTexture.Read(newsrc));

                Textures = Textures.AddArray(t);
            }

            LaraType = (LaraType) reader.ReadUInt16();
            WeatherType = (WeatherType) reader.ReadUInt16();

            var flags = reader.ReadUInt32Array(7); // 28 bytes zero padding
            for (var i = 0; i < flags.Length; i++)
            {
                if (flags[i] != 0)
                    Cerr.Write("Load_TR5: flags[" + (i + 1) + "]: Expected 0, Found 0x" + flags[i].ToString("X8"));
            }

            var levelUncompSize = reader.ReadUInt32();
            var levelCompSize = reader.ReadUInt32();

            var unused = reader.ReadUInt32();
            if(unused != 0)
                Cerr.Write("Load_TR5: unused: Expected 0, Found " + unused.ToString("X8"));

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
                throw new ArgumentException("Load_TR5: Expected 'SPR', Found '" + spr + "'", nameof(spr));

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
                throw new ArgumentException("Load_TR5: Expected 'TEX', Found '" + tex + "'", nameof(tex));

            var numObjectTextures = reader.ReadUInt32();
            ObjectTextures = reader.ReadArray(numObjectTextures, () => ObjectTexture.Read(reader, Engine.TR5));

            var numItems = reader.ReadUInt32();
            Entities = reader.ReadArray(numItems, () => Entity.Read(reader, Engine.TR4));

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
                Textures = new DWordTexture[Texture16.Length];
                for (uint i = 0; i < Texture16.Length; i++)
                {
                    Textures[i] = ConvertTexture(Texture16[i]);
                }
            }
        }

        private void Write_TR5()
        {
            if (Format.Engine != Engine.TR5)
            {
                ConvertRoomsToTR5();
            }

            writer.Write((uint) 0x00345254);

            writer.Write((ushort) Textures.Length);
            writer.Write((ushort) 0);
            writer.Write((ushort) 0);

            byte[] buf;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    bw.WriteArray(Textures, x => x.Write(bw));
                buf = ms.ToArray();
            }
            writer.Write((uint) buf.Length); // uncompSize
            var bufComp = Helper.Encompress(buf);
            writer.Write((uint) bufComp.Length); // compSize
            writer.Write(bufComp);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    bw.WriteArray(Texture16, x => x.Write(bw));
                buf = ms.ToArray();
            }
            writer.Write((uint) buf.Length); // uncompSize
            bufComp = Helper.Encompress(buf);
            writer.Write((uint) bufComp.Length); // compSize
            writer.Write(bufComp);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    bw.WriteArray(Textures.Skip(Textures.Length - 3).Take(3), x => x.Write(bw));
                buf = ms.ToArray();
            }
            writer.Write((uint) buf.Length); // uncompSize
            bufComp = Helper.Encompress(buf);
            writer.Write((uint) bufComp.Length); // compSize
            writer.Write(bufComp);


            writer.Write((ushort) LaraType);
            writer.Write((ushort) WeatherType);

            writer.Write(new byte[28]);

            var levelStartPos = writer.BaseStream.Position;

            writer.Write((uint)0); // uncompSize
            writer.Write((uint)0); // compSize

            writer.Write((uint) 0); // unused

            writer.Write((ushort) Rooms.Length);
            writer.WriteArray(Rooms, x => x.Write(writer, Engine.TR5));

            writer.Write((uint) FloorData.Length);
            writer.WriteUInt16Array(FloorData);

            WriteMeshData();

            writer.Write((uint) Animations.Length);
            writer.WriteArray(Animations, x => x.Write(writer, Engine.TR4));

            writer.Write((uint) StateChanges.Length);
            writer.WriteArray(StateChanges, x => x.Write(writer));

            writer.Write((uint) AnimDispatches.Length);
            writer.WriteArray(AnimDispatches, x => x.Write(writer));

            writer.Write((uint) AnimCommands.Length);
            writer.WriteInt16Array(AnimCommands);

            writer.Write((uint) MeshTreeData.Length);
            writer.WriteUInt32Array(MeshTreeData);

            WriteFrameMoveableData();

            writer.Write((uint) StaticMeshes.Length);
            writer.WriteArray(StaticMeshes, x => x.Write(writer));

            writer.Write((sbyte) 'S');
            writer.Write((sbyte) 'P');
            writer.Write((sbyte) 'R');
            writer.Write((sbyte) '\0');

            writer.Write((uint) SpriteTextures.Length);
            writer.WriteArray(SpriteTextures, x => x.Write(writer, Engine.TR4));

            writer.Write((uint) SpriteSequences.Length);
            writer.WriteArray(SpriteSequences, x => x.Write(writer));

            writer.Write((uint) Cameras.Length);
            writer.WriteArray(Cameras, x => x.Write(writer));

            writer.Write((uint) FlybyCameras.Length);
            writer.WriteArray(FlybyCameras, x => x.Write(writer));

            writer.Write((uint) SoundSources.Length);
            writer.WriteArray(SoundSources, x => x.Write(writer));

            writer.Write((uint) Boxes.Length);
            writer.WriteArray(Boxes, x => x.Write(writer, Engine.TR2));

            writer.Write((uint) Overlaps.Length);
            writer.WriteUInt16Array(Overlaps);

            writer.WriteArray(Zones, x => x.Write(writer, Engine.TR2));

            writer.Write((uint) AnimatedTextures.Length);
            writer.WriteUInt16Array(AnimatedTextures);

            writer.Write((byte) AnimatedTexturesUVCount);

            writer.Write((sbyte) 'T');
            writer.Write((sbyte) 'E');
            writer.Write((sbyte) 'X');
            writer.Write((sbyte) '\0');

            writer.Write((uint) ObjectTextures.Length);
            writer.WriteArray(ObjectTextures, x => x.Write(writer, Engine.TR5));

            var newEnt = ConvertEntityArray(Format.Engine, Engine.TR4, Entities);

            writer.Write((uint)newEnt.Length);
            writer.WriteArray(newEnt, x => x.Write(writer, Engine.TR4));

            writer.Write((uint) AIObjects.Length);
            writer.WriteArray(AIObjects, x => x.Write(writer));

            writer.Write((ushort) DemoData.Length);
            writer.Write(DemoData);

            writer.WriteInt16Array(SoundMap.Resize(Constants.TR_AUDIO_MAP_SIZE_TR5));

            writer.Write((uint) SoundDetails.Length);
            writer.WriteArray(SoundDetails, x => x.Write(writer, Engine.TR3));

            writer.Write((uint) SampleIndices.Length);
            writer.WriteUInt32Array(SampleIndices);

            var levelEndPos = writer.BaseStream.Position;
            writer.BaseStream.Position = levelStartPos;
            var theSize = levelEndPos - levelStartPos;
            writer.Write((uint)theSize); // uncompSize
            writer.Write((uint)theSize); // compSize

            writer.BaseStream.Position = levelEndPos;

            writer.Write(new byte[6]);

            writer.Write((uint) SamplesCount);
            writer.Write(SamplesData);
        }

        private void ConvertRoomsToTR5()
        {
            
        }
    }
}
