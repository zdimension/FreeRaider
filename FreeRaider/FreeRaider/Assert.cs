using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;

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
