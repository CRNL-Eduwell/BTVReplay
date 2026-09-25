namespace BTV.Data
{
    /// <summary>
    /// Number of EEG file slots per experiment (the database editor has one widget per slot, and
    /// each montage holds one BtvProgram per slot). Each slot is a full managed copy of a
    /// recording, so this is also the memory ceiling. It used to be a literal 6 in a dozen places.
    /// </summary>
    public static class EegSlots
    {
        public const int Count = 6;
    }
}
