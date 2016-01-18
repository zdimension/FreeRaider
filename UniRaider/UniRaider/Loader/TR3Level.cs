using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniRaider.Loader
{
    public class TR3Level : Level
    {
        public TR3Level(BinaryReader br, TRVersion ver) : base(br, ver)
        {
        }
    }
}
