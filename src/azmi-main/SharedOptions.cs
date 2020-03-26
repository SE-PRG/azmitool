using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public class SharedOptions
    {
        public string identity { get; set; }
        public bool verbose { get; set; }
    }

    public static class SharedAzmiOptions
    {
        public static AzmiOption identity = new AzmiOption("identity");
        public static AzmiOption verbose = new AzmiOption("verbose",AcceptedTypes.boolType);
    }
}
