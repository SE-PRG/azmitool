using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public abstract class BaseCommand : IAzmiCommand
    {
        public string name = "basecommand";

        public string Name() { return name; }
    }
}
