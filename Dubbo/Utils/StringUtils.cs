namespace Dubbo.Utils
{
    public static class StringUtils
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }
}
