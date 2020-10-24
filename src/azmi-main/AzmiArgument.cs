using System.Diagnostics.CodeAnalysis;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("azmi-main-tests")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("azmi-commandline-tests")]

namespace azmi_main
{

    public enum ArgType { str, flag, url };
    // what are supported input argument types
    // flag is boolean type
    // str is string type
    // url fails back to string, its just different in description


    public class AzmiArgument
    {
        public string name { get; set; } // "blob"
        public char? alias { get; set; } // "b" or null
        public string description { get; set; }
        public bool required { get; set; }
        public ArgType type { get; set; } // string, url or bool
        public bool multiValued { get; set; }

        private const ArgType defaultType = ArgType.str;
        private const bool defaultRequired = false;
        private const bool defaultMultiValued = false;

        internal AzmiArgument(string name, char? alias, string description, bool required, ArgType type, bool multiValued)
        {
            this.name = name;
            this.alias = alias;
            this.description = description;
            this.required = required;
            this.type = type;
            this.multiValued = multiValued;
        }

        // constructor NAME with one string
        internal AzmiArgument(string name,
            ArgType type = defaultType, bool required = defaultRequired, bool multiValued = defaultMultiValued)
        : this(
            name,
            name[0],
            $"Description for {name}",
            required,
            type,
            multiValued)
        { }

        // constructor NAME + ALIAS? + DESCRIPTION
        internal AzmiArgument(string name, char? alias, string description,
            ArgType type = defaultType, bool required = defaultRequired, bool multiValued = defaultMultiValued)
        : this(
            name,
            alias,
            description,
            required,
            type,
            multiValued)
        { }

        // constructor NAME + DESCRIPTION
        internal AzmiArgument(string name, [DisallowNull] string description,
            ArgType type = defaultType, bool required = defaultRequired, bool multiValued = defaultMultiValued)
        : this(
            name,
            name[0],
            description,
            required,
            type,
            multiValued)
        { }
    }
}
