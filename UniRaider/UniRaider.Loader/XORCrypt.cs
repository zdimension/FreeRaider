using System.Text;

namespace UniRaider.Loader
{
    public static class XORCrypt
    {
        public static string XOR(this string s, int key)
        {
            var sb = new StringBuilder();
            foreach (var t in s)
                sb.Append((char) (t ^ key));
            return sb.ToString();
        }
    }
}
