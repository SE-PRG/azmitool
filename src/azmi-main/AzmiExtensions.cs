﻿using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("azmi-main-tests")]

namespace azmi_main
{
    internal static class AzmiExtensions
    {
        internal static List<string> ToStringList(this string str)
        {
            return new List<string> { str };
        }

    }
}
