namespace Dubbo
{
    public static class StringUtils
    {
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }
}
