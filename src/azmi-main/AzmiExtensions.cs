using System.Collections.Generic;

namespace azmi_main
{
    public static class AzmiExtensions
    {
        public static List<string> ToStringList(this string str)
        {
            return new List<string> { str };
        }
    }
}
