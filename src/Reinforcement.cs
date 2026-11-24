namespace NightsOfTerror
{
    // Lives for one dawn checkpoint. Loading the checkpoint creates a fresh schedule.
    internal sealed class Reinforcement
    {
        internal const int GroupSize = 10;
        internal int Remaining { get; private set; } = GroupSize;
        private float nextArrival;

        internal bool IsDue(float scaledTime) => Remaining > 0 && scaledTime >= nextArrival;

        internal void Arrived(float scaledTime, bool rushed)
        {
            Remaining--;
            nextArrival = scaledTime + (rushed ? 0.075f : 1f);
        }

        internal int ForClearance(int living) => Remaining > 0 && living == 0 ? 1 : living;

        internal void Cancel() => Remaining = 0;
    }
}
