using System.Collections.Generic;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("azmi-main-tests")]

namespace azmi_main
{
    public class SubCommandDefinition
    {
        public string name { get; set; }
        public string description { get; set; }
        public AzmiArgument[] arguments { get; set; }
    }

    public interface IAzmiCommand
    {
        public SubCommandDefinition Definition();

        public List<string> Execute(object options);

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass { };

    }
}
