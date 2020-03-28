using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public class SubCommandDefinition
    {
        public string name;
        public string description;
        public AzmiOption[] arguments;
    }

    public interface IAzmiCommand
    {
        public SubCommandDefinition Definition();

        public List<string> Execute(object options);        

        // public class Options { };
        // TODO: Like this it is not visible inside extensions class

    }
}
