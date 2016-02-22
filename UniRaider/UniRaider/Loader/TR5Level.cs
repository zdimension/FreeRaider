using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniRaider.Loader
{
    public class TR5Level : Level
    {
        public TR5Level(BinaryReader br, Engine ver) : base(br, ver)
        {
        }

        public TR5Level(BinaryReader br, Game ver) : base(br, ver)
        {
        }
    }
}
