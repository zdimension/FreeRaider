using System;
using System.IO;
using System.Linq;

namespace FreeRaider.Loader
{
    public partial class Constants
    {
        public const int TR_AUDIO_MAP_SIZE_TR4 = 370;
    }

    public partial class Level
    {
        private void Load_TR4()
        {
            var version = reader.ReadUInt32();

            if (!version.IsAnyOf((uint)0x00345254, (uint)0x63345254, 0xFFFFFFF0))
                throw new ArgumentException("Load_TR4: Wrong level version");

            Texture16 = new WordTexture[0];

            var numRoomTextiles = reader.ReadUInt16();
            var numObjTextiles = reader.ReadUInt16();
            var numBumpTextiles = reader.ReadUInt16();
            var numMiscTextiles = 2;
            var numTextiles = numRoomTextiles + numObjTextiles + numBumpTextiles + numMiscTextiles;

            var uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR4: Textiles32 uncompSize == 0", nameof(uncompSize));

            var compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                var compBuffer = reader.ReadBytes((int) compSize);

                var newsrc_ = Helper.Decompress(compBuffer);
                Textures = newsrc_.ReadArray(numTextiles - numMiscTextiles, () => DWordTexture.Read(newsrc_));
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR4: Textiles16 uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                if (Textures.Length == 0)
                {
                    var compBuffer = reader.ReadBytes((int) compSize);

                    var newsrc_ = Helper.Decompress(compBuffer);
                    Texture16 = newsrc_.ReadArray(numTextiles - numMiscTextiles, () => WordTexture.Read(newsrc_));
                }
                else
                {
                    reader.BaseStream.Position += compSize;
                }
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR4: Textiles32d uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize > 0)
            {
                // 262144 = 256*256*4
                if (uncompSize / 262144 > 2)
                    Cerr.Write("Load_TR4: NumMiscTextiles > 2");

                if (Textures.Length == 0)
                {
                    Array.Resize(ref Textures, numTextiles - numMiscTextiles);
                }

                var compBuffer = reader.ReadBytes((int) compSize);

                var newsrc_ = Helper.Decompress(compBuffer);
                Textures = Textures.AddArray(newsrc_.ReadArray(numMiscTextiles, () => DWordTexture.Read(newsrc_)));
            }

            uncompSize = reader.ReadUInt32();
            if (uncompSize == 0)
                throw new ArgumentException("Load_TR4: Packed geometry uncompSize == 0", nameof(uncompSize));

            compSize = reader.ReadUInt32();
            if (compSize == 0)
                throw new ArgumentException("Load_TR4: Packed geometry", nameof(compSize));

            var compBuffer_ = reader.ReadBytes((int) compSize);

            var newsrc = Helper.Decompress(compBuffer_);

            var unused = newsrc.ReadUInt32();
            if (unused != 0)
                Cerr.Write("Load_TR4: unused: Expected 0, Found " + unused.ToString("X8"));

            var numRooms = newsrc.ReadUInt16();
            Rooms = newsrc.ReadArray(numRooms, () => Room.Read(newsrc, Engine.TR4));

            var numFloorData = newsrc.ReadUInt32();
            FloorData = newsrc.ReadUInt16Array(numFloorData);

            var tmpr = reader;
            reader = newsrc;
            ReadMeshData();
            reader = tmpr;

            var numAnimations = newsrc.ReadUInt32();
            Animations = newsrc.ReadArray(numAnimations, () => Animation.Read(newsrc, Engine.TR4));

            var numStateChanges = newsrc.ReadUInt32();
            StateChanges = newsrc.ReadArray(numStateChanges, () => StateChange.Read(newsrc));

            var numAnimDispatches = newsrc.ReadUInt32();
            AnimDispatches = newsrc.ReadArray(numAnimDispatches, () => AnimDispatch.Read(newsrc));

            var numAnimCommands = newsrc.ReadUInt32();
            AnimCommands = newsrc.ReadInt16Array(numAnimCommands);

            var numMeshTreeData = newsrc.ReadUInt32();
            MeshTreeData = newsrc.ReadUInt32Array(numMeshTreeData);

            tmpr = reader;
            reader = newsrc;
            ReadFrameMoveableData();
            reader = tmpr;

            var numStaticMeshes = newsrc.ReadUInt32();
            StaticMeshes = newsrc.ReadArray(numStaticMeshes, () => StaticMesh.Read(newsrc));

            var spr1 = (char)newsrc.ReadSByte();
            var spr2 = (char)newsrc.ReadSByte();
            var spr3 = (char)newsrc.ReadSByte();
            var spr = "" + spr1 + spr2 + spr3;
            if(spr != "SPR")
                throw new ArgumentException("Load_TR4: Expected 'SPR', Found '" + spr + "'", nameof(spr));

            var numSpriteTextures = newsrc.ReadUInt32();
            SpriteTextures = newsrc.ReadArray(numSpriteTextures, () => SpriteTexture.Read(newsrc, Engine.TR4));

            var numSpriteSequences = newsrc.ReadUInt32();
            SpriteSequences = newsrc.ReadArray(numSpriteSequences, () => SpriteSequence.Read(newsrc));

            var numCameras = newsrc.ReadUInt32();
            Cameras = newsrc.ReadArray(numCameras, () => Camera.Read(newsrc));

            var numFlybyCameras = newsrc.ReadUInt32();
            FlybyCameras = newsrc.ReadArray(numFlybyCameras, () => FlybyCamera.Read(newsrc));

            var numSoundSources = newsrc.ReadUInt32();
            SoundSources = newsrc.ReadArray(numSoundSources, () => SoundSource.Read(newsrc));

            var numBoxes = newsrc.ReadUInt32();
            Boxes = newsrc.ReadArray(numBoxes, () => Box.Read(newsrc, Engine.TR2));

            var numOverlaps = newsrc.ReadUInt32();
            Overlaps = newsrc.ReadUInt16Array(numOverlaps);

            Zones = newsrc.ReadArray(numBoxes, () => Zone.Read(newsrc, Engine.TR2));

            var numAnimatedTextures = newsrc.ReadUInt32();
            AnimatedTextures = newsrc.ReadUInt16Array(numAnimatedTextures);

            AnimatedTexturesUVCount = newsrc.ReadByte();

            var tex1 = (char)newsrc.ReadSByte();
            var tex2 = (char)newsrc.ReadSByte();
            var tex3 = (char)newsrc.ReadSByte();
            var tex = "" + tex1 + tex2 + tex3;
            if (tex != "TEX")
                throw new ArgumentException("Load_TR4: Expected 'TEX', Found '" + tex + "'", nameof(tex));

            var numObjectTextures = newsrc.ReadUInt32();
            ObjectTextures = newsrc.ReadArray(numObjectTextures, () => ObjectTexture.Read(newsrc, Engine.TR4));

            var numItems = newsrc.ReadUInt32();
            Entities = newsrc.ReadArray(numItems, () => Entity.Read(newsrc, Engine.TR4));

            var numAiObjects = newsrc.ReadUInt32();
            AIObjects = newsrc.ReadArray(numAiObjects, () => AIObject.Read(newsrc));

            var numDemoData = newsrc.ReadUInt16();
            DemoData = newsrc.ReadBytes(numDemoData);

            SoundMap = newsrc.ReadInt16Array(Constants.TR_AUDIO_MAP_SIZE_TR4);

            var numSoundDetails = newsrc.ReadUInt32();
            SoundDetails = newsrc.ReadArray(numSoundDetails, () => Loader.SoundDetails.Read(newsrc, Engine.TR3));

            var numSampleIndices = newsrc.ReadUInt32();
            SampleIndices = newsrc.ReadUInt32Array(numSampleIndices);

            var numSamples = reader.ReadUInt32();
            if(numSamples > 0)
            {
                SamplesCount = (int) numSamples;
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

        private void Write_TR4()
        {
            writer.Write((uint)(WriteFormat.IsDemoOrVict ? 0x63345254 : 0x00345254));

            writer.Write((ushort)Textures.Length);
            writer.Write((ushort)0);
            writer.Write((ushort)0);

            byte[] buf;
            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    bw.WriteArray(Textures, x => x.Write(bw));
                buf = ms.ToArray();
            }
            writer.Write((uint)buf.Length); // uncompSize
            var bufComp = Helper.Encompress(buf);
            writer.Write((uint)bufComp.Length); // compSize
            writer.Write(bufComp);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    bw.WriteArray(Texture16, x => x.Write(bw));
                buf = ms.ToArray();
            }
            writer.Write((uint)buf.Length); // uncompSize
            bufComp = Helper.Encompress(buf);
            writer.Write((uint)bufComp.Length); // compSize
            writer.Write(bufComp);

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                    bw.WriteArray(Textures.Skip(Textures.Length - 2).Take(2), x => x.Write(bw));
                buf = ms.ToArray();
            }
            writer.Write((uint)buf.Length); // uncompSize
            bufComp = Helper.Encompress(buf);
            writer.Write((uint)bufComp.Length); // compSize
            writer.Write(bufComp);

            // Here be dragons

            using (var ms = new MemoryStream())
            {
                using (var bw = new BinaryWriter(ms))
                {
                    bw.Write((uint)0); // unused

                    bw.Write((ushort)Rooms.Length);
                    bw.WriteArray(Rooms, x => x.Write(bw, Engine.TR4));

                    bw.Write((uint)FloorData.Length);
                    bw.WriteUInt16Array(FloorData);

                    var tmpw = writer;
                    writer = bw;
                    WriteMeshData();
                    writer = tmpw;

                    bw.Write((uint)Animations.Length);
                    bw.WriteArray(Animations, x => x.Write(bw, Engine.TR4));

                    bw.Write((uint)StateChanges.Length);
                    bw.WriteArray(StateChanges, x => x.Write(bw));

                    bw.Write((uint)AnimDispatches.Length);
                    bw.WriteArray(AnimDispatches, x => x.Write(bw));

                    bw.Write((uint)AnimCommands.Length);
                    bw.WriteInt16Array(AnimCommands);

                    bw.Write((uint)MeshTreeData.Length);
                    bw.WriteUInt32Array(MeshTreeData);

                    tmpw = writer;
                    writer = bw;
                    WriteFrameMoveableData();
                    writer = tmpw;

                    bw.Write((uint)StaticMeshes.Length);
                    bw.WriteArray(StaticMeshes, x => x.Write(bw));

                    bw.Write((sbyte)'S');
                    bw.Write((sbyte)'P');
                    bw.Write((sbyte)'R');

                    bw.Write((uint)SpriteTextures.Length);
                    bw.WriteArray(SpriteTextures, x => x.Write(bw, Engine.TR4));

                    bw.Write((uint)SpriteSequences.Length);
                    bw.WriteArray(SpriteSequences, x => x.Write(bw));

                    bw.Write((uint)Cameras.Length);
                    bw.WriteArray(Cameras, x => x.Write(bw));

                    bw.Write((uint)FlybyCameras.Length);
                    bw.WriteArray(FlybyCameras, x => x.Write(bw));

                    bw.Write((uint)SoundSources.Length);
                    bw.WriteArray(SoundSources, x => x.Write(bw));

                    bw.Write((uint)Boxes.Length);
                    bw.WriteArray(Boxes, x => x.Write(bw, Engine.TR2));

                    bw.Write((uint)Overlaps.Length);
                    bw.WriteUInt16Array(Overlaps);

                    bw.WriteArray(Zones, x => x.Write(bw, Engine.TR2));

                    bw.Write((uint)AnimatedTextures.Length);
                    bw.WriteUInt16Array(AnimatedTextures);

                    bw.Write((byte)AnimatedTexturesUVCount);

                    bw.Write((sbyte)'T');
                    bw.Write((sbyte)'E');
                    bw.Write((sbyte)'X');


                    bw.Write((uint)ObjectTextures.Length);
                    bw.WriteArray(ObjectTextures, x => x.Write(bw, Engine.TR4));

                    var newEnt = ConvertEntityArray(Format.Engine, Engine.TR4, Entities);

                    bw.Write((uint)newEnt.Length);
                    bw.WriteArray(newEnt, x => x.Write(bw, Engine.TR4));

                    bw.Write((uint)AIObjects.Length);
                    bw.WriteArray(AIObjects, x => x.Write(bw));

                    bw.Write((ushort)DemoData.Length);
                    bw.Write(DemoData);

                    bw.WriteInt16Array(SoundMap.Resize(Constants.TR_AUDIO_MAP_SIZE_TR4));

                    bw.Write((uint)SoundDetails.Length);
                    bw.WriteArray(SoundDetails, x => x.Write(bw, Engine.TR3));

                    bw.Write((uint)SampleIndices.Length);
                    bw.WriteUInt32Array(SampleIndices);
                }
                buf = ms.ToArray();
            }

            writer.Write((uint)buf.Length); // uncompSize
            bufComp = Helper.Encompress(buf);
            writer.Write((uint)bufComp.Length); // compSize
            writer.Write(bufComp);

            writer.Write((uint)SamplesCount);
            writer.Write(SamplesData);
        }
    }
}
