using System;
using System.IO;
using System.Linq;

namespace FreeRaider.Loader
{
    public partial class Constants
    {
        public const int TR_AUDIO_MAP_SIZE_TR2 = 370;
    }

    public partial class Level
    {
        private void Load_TR2()
        {
            var version = reader.ReadUInt32();

            if(version != 0x0000002d)
                throw new ArgumentException("Load_TR2: Wrong level version");

            Palette = Palette.Read(reader, Engine.TR1);

            Palette16 = Palette.Read(reader, Engine.TR2);

            var numTextiles = reader.ReadUInt32();
            Texture8 = reader.ReadArray(numTextiles, () => ByteTexture.Read(reader));
            Texture16 = reader.ReadArray(numTextiles, () => WordTexture.Read(reader));

            var unused = reader.ReadUInt32();
            if(unused != 0)
                Cerr.Write("Load_TR2: unused: Expected 0, Found " + unused.ToString("X8"));

            var numRooms = reader.ReadUInt16();
            Rooms = reader.ReadArray(numRooms, () => Room.Read(reader, Engine.TR2));

            var numFloorData = reader.ReadUInt32();
            FloorData = reader.ReadUInt16Array(numFloorData);

            ReadMeshData();

            var numAnimations = reader.ReadUInt32();
            Animations = reader.ReadArray(numAnimations, () => Animation.Read(reader, Engine.TR2));

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

            var numObjectTextures = reader.ReadUInt32();
            ObjectTextures = reader.ReadArray(numObjectTextures, () => ObjectTexture.Read(reader, Engine.TR2));

            var numSpriteTextures = reader.ReadUInt32();
            SpriteTextures = reader.ReadArray(numSpriteTextures, () => SpriteTexture.Read(reader, Engine.TR2));

            var numSpriteSequences = reader.ReadUInt32();
            SpriteSequences = reader.ReadArray(numSpriteSequences, () => SpriteSequence.Read(reader));

            if(IsDemoOrUb)
                LightMap = LightMap.Read(reader);

            var numCameras = reader.ReadUInt32();
            Cameras = reader.ReadArray(numCameras, () => Camera.Read(reader));

            var numSoundSources = reader.ReadUInt32();
            SoundSources = reader.ReadArray(numSoundSources, () => SoundSource.Read(reader));

            var numBoxes = reader.ReadUInt32();
            Boxes = reader.ReadArray(numBoxes, () => Box.Read(reader, Engine.TR2));

            var numOverlaps = reader.ReadUInt32();
            Overlaps = reader.ReadUInt16Array(numOverlaps);

            Zones = reader.ReadArray(numBoxes, () => Zone.Read(reader, Engine.TR2));

            var numAnimatedTextures = reader.ReadUInt32();
            AnimatedTextures = reader.ReadUInt16Array(numAnimatedTextures);

            var numItems = reader.ReadUInt32();
            Items = reader.ReadArray(numItems, () => Item.Read(reader, Engine.TR2));

            if (!IsDemoOrUb)
                LightMap = LightMap.Read(reader);

            var numCinematicFrames = reader.ReadUInt16();
            CinematicFrames = reader.ReadArray(numCinematicFrames, () => CinematicFrame.Read(reader));

            var numDemoData = reader.ReadUInt16();
            DemoData = reader.ReadBytes(numDemoData);

            SoundMap = reader.ReadInt16Array(Constants.TR_AUDIO_MAP_SIZE_TR2);

            var numSoundDetails = reader.ReadUInt32();
            SoundDetails = reader.ReadArray(numSoundDetails, () => Loader.SoundDetails.Read(reader, Engine.TR2));

            var numSampleIndices = reader.ReadUInt32();
            SampleIndices = reader.ReadUInt32Array(numSampleIndices);

            for (uint i = 0; i < numSoundDetails; i++)
            {
                if (SoundDetails[i].Sample < numSampleIndices)
                    SoundDetails[i].Sample = (ushort)SampleIndices[SoundDetails[i].Sample];
            }

            if(!File.Exists(SfxPath))
                Cerr.Write("Load_TR2: '" + SfxPath + "' not found, no samples loaded");
            else
            {
                using (var fs = new FileStream(SfxPath, FileMode.Open))
                {
                    using (var br = new BinaryReader(fs))
                    {
                        SamplesData = new byte[fs.Length];

                        for(long i = 0; i < SamplesData.Length; i++)
                        {
                            SamplesData[i] = br.ReadByte();

                            if(i >= 4)
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
            for(uint i = 0; i < numTextiles; i++)
            {
                Textures[i] = ConvertTexture(Texture16[i]);
            }
        }

        private void Write_TR2()
        {
            writer.Write((uint)0x0000002d);

            Palette.Write(writer, Engine.TR1);

            Palette16.Write(writer, Engine.TR2);

            writer.Write((uint)Textures.Length);
            writer.WriteArray(Texture8, x => x.Write(writer));
            writer.WriteArray(Texture16, x => x.Write(writer));

            writer.Write((uint)0); // unused

            writer.Write((ushort)Rooms.Length);
            writer.WriteArray(Rooms, x => x.Write(writer, Engine.TR2));

            writer.Write((uint)FloorData.Length);
            writer.WriteUInt16Array(FloorData);

            WriteMeshData();

            writer.Write((uint)Animations.Length);
            writer.WriteArray(Animations, x => x.Write(writer, Engine.TR2));

            writer.Write((uint)StateChanges.Length);
            writer.WriteArray(StateChanges, x => x.Write(writer));

            writer.Write((uint)AnimDispatches.Length);
            writer.WriteArray(AnimDispatches, x => x.Write(writer));

            writer.Write((uint)AnimCommands.Length);
            writer.WriteInt16Array(AnimCommands);

            writer.Write((uint)MeshTreeData.Length);
            writer.WriteUInt32Array(MeshTreeData);

            WriteFrameMoveableData();

            writer.Write((uint)StaticMeshes.Length);
            writer.WriteArray(StaticMeshes, x => x.Write(writer));

            writer.Write((uint)ObjectTextures.Length);
            writer.WriteArray(ObjectTextures, x => x.Write(writer, Engine.TR2));

            writer.Write((uint)SpriteTextures.Length);
            writer.WriteArray(SpriteTextures, x => x.Write(writer, Engine.TR2));

            writer.Write((uint)SpriteSequences.Length);
            writer.WriteArray(SpriteSequences, x => x.Write(writer));

            if (WriteIsDemoOrUb)
                LightMap.Write(writer);

            writer.Write((uint)Cameras.Length);
            writer.WriteArray(Cameras, x => x.Write(writer));

            writer.Write((uint)SoundSources.Length);
            writer.WriteArray(SoundSources, x => x.Write(writer));

            writer.Write((uint)Boxes.Length);
            writer.WriteArray(Boxes, x => x.Write(writer, Engine.TR2));


            writer.Write((uint)Overlaps.Length);
            writer.WriteUInt16Array(Overlaps);

            writer.WriteArray(Zones, x => x.Write(writer, Engine.TR2));

            writer.Write((uint)AnimatedTextures.Length);
            writer.WriteUInt16Array(AnimatedTextures);

            writer.Write((uint)Items.Length);
            writer.WriteArray(Items, x => x.Write(writer, Engine.TR2));

            if (!WriteIsDemoOrUb)
                LightMap.Write(writer);

            writer.Write((ushort)CinematicFrames.Length);
            writer.WriteArray(CinematicFrames, x => x.Write(writer));

            writer.Write((ushort)DemoData.Length);
            writer.Write(DemoData);

            writer.WriteInt16Array(SoundMap.Resize(Constants.TR_AUDIO_MAP_SIZE_TR2));

            writer.Write((uint)SoundDetails.Length);

            var origSndDetails = SoundDetails.ToArray();

            for (uint i = 0; i < origSndDetails.Length; i++)
            {
                var id = Array.IndexOf(SampleIndices, origSndDetails[i].Sample);
                if (id != -1)
                    origSndDetails[i].Sample = (ushort)id;
            }

            writer.WriteArray(origSndDetails, x => x.Write(writer, Engine.TR2));

            writer.Write((uint)SampleIndices.Length);
            writer.WriteUInt32Array(SampleIndices);
        }
    }
}
