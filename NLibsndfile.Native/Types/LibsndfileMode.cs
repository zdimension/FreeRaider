namespace NLibsndfile.Native
{
    public enum LibsndfileMode
    {
        /* True and false */
        False = 0,
        True = 1,

        /* Modes for opening files. */
        Read = 0x10,
        Write = 0x20,
        Rdwr = 0x30,

        AmbisonicNone = 0x40,
        AmbisonicBFormat = 0x41
    }
}