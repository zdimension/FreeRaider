using System.Dynamic;

namespace FreeRaider.LoaderTests
{
    public partial class Loader
    {
        public static dynamic TR2Format = new ExpandoObject();

        private static void InitTR2Format()
        {
            //TR1Format = new dynamic();
        }
    }
}
