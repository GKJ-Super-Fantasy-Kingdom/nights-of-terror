namespace NightsOfTerror
{
    // Lives for one dawn checkpoint. Loading the checkpoint creates a fresh schedule.
    internal sealed class Reinforcement
    {
        internal Reinforcement(int count)
        {
            Count = Remaining = count;
        }

        internal static int SizeFromQuota(int quota)
            => quota <= 0 ? 0 : quota / 2 + quota % 2;

        internal int Count { get; }
        internal int Remaining { get; private set; }
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
