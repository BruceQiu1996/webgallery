namespace WebGallery.Extensions
{
    public static class TypeExtensions
    {
        public static int ToInt(this string str)
        {
            int retVal = 0;
            int.TryParse(str, out retVal);

            return retVal;
        }

        public static bool ToBool(this string str)
        {
            bool retVal = false;
            bool.TryParse(str, out retVal);

            return retVal;
        }
    }
}