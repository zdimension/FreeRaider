using System;

namespace FreeRaider
{
    public class Assert
    {
        public static void That(bool condition, string message = "Incorrect value")
        {
            throw new Exception("Assert: " + message);
        }
    }
}
