namespace Platformer.Core
{
    /// <summary>
    /// Reference-counted gate for whether gameplay (movement/jump/interact) should read
    /// input right now. Modal UI screens that take over input - the key-remap menu,
    /// dialogue, any future pause/inventory screen - push a lock on open and pop it on
    /// close, instead of reaching into player scripts directly.
    /// </summary>
    public static class GameplayInputGate
    {
        private static int lockCount;

        public static bool IsLocked => lockCount > 0;

        public static void Push() => lockCount++;

        public static void Pop()
        {
            if (lockCount > 0) lockCount--;
        }
    }
}
