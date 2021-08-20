namespace Extensions
{
    public static class IntExtensions
    {
        public static int ToCircleIndex(this int a, int arrayLength) => (a + 1 + (arrayLength - 1)) % arrayLength;
    }
}