namespace MusicInterface
{
    public class PlayingParams
    {
        public int KeyAdjustmentInSemitones { get; set; }
        public int Instrument { get; set; }
        public int Velocity { get; set; }

        public static PlayingParams Default()
        {
            return new PlayingParams
            {
                KeyAdjustmentInSemitones = 0,
                Instrument = 1,
                Velocity = 127
            };
        }

    }
}
