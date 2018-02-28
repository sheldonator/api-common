namespace ApiExtensions
{
    public static class ByteExtensions
    {
        public static string ToBase64String(this byte[] array)
        {
            return System.Convert.ToBase64String(array);
        }
    }
}
