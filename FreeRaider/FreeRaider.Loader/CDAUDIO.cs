using System;
using System.IO;
using System.Linq;

namespace FreeRaider.Loader
{
	public class CDAUDIO
	{
		public Tuple<string, byte[]>[] Entries;

		public static CDAUDIO Read(string fname)
		{
			using (var fs = File.OpenRead(fname))
			{
				return Read(fs);
			}
		}

		public static CDAUDIO Read(Stream s)
		{
			using (var br = new BinaryReader(s))
			{
				return Read(br);
			}
		}

		public static CDAUDIO Read(BinaryReader br)
		{
			var ret = new CDAUDIO();

			ret.Entries = new Tuple<string, byte[]>[NUM_ENTRIES];

			for (var i = 0; i < ret.Entries.Length; i++)
			{
				var name = br.ReadString(260);
				var wavLength = br.ReadUInt32();
				var wavOffset = br.ReadUInt32();

				var pos = br.BaseStream.Position;
				br.BaseStream.Position = wavOffset;
				var data = br.ReadBytes((int)wavLength);
				br.BaseStream.Position = pos;

				ret.Entries[i] = Tuple.Create(name, data);
			}

			return ret;
		}

		public void Write(string fname)
		{
			if (File.Exists(fname))
			{
				File.Delete(fname);
			}
			using (var fs = File.OpenWrite(fname))
			{
				Write(fs);
			}
		}

		public void Write(Stream s)
		{
			using (var bw = new BinaryWriter(s))
			{
				Write(bw);
			}
		}

		public const int NUM_ENTRIES = 130;
		public const int ENTRY_SIZE = 260 * sizeof(byte) + sizeof(uint) + sizeof(uint); // 268 bytes
		public const int DATA_START = ENTRY_SIZE * NUM_ENTRIES;

		public void Write(BinaryWriter bw)
		{
			var tally = DATA_START;
			for (var i = 0; i < Entries.Length; i++)
			{
				bw.WriteString(Entries[i].Item1, length: 260);
				bw.Write(Entries[i].Item2.Length);
				bw.Write((uint)tally);
				tally += Entries[i].Item2.Length;
			}

			for (var i = 0; i < Entries.Length; i++)
			{
				bw.Write(Entries[i].Item2);
			}
		}

		public int GetOffset(int entryID)
		{
			return DATA_START + Entries.Take(entryID).Sum(x => x.Item2.Length);
		}

		public int GetOffset(Tuple<string, byte[]> entry) => GetOffset(Array.IndexOf(Entries, entry));

		public double GetWavLength(int entry)
		{
			return GetWavLength(Entries[entry].Item2);
		}

		public static double GetWavLength(byte[] bs)
		{
			if (bs.Length < 32) return 0;
			//return (bs.Length - 8) * 8.0 / (BitConverter.ToInt32(new[] { bs[28], bs[29], bs[30], bs[31] }, 0) * 8);
			return (bs.Length - 8.0) / BitConverter.ToInt32(new[] { bs[28], bs[29], bs[30], bs[31] }, 0);
		}
	}
}
