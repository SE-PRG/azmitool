using System.Diagnostics.CodeAnalysis;

namespace azmi_main
{

    public enum ArgType {str, flag, url};
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

        internal AzmiArgument(string name, char? alias, string description, bool required, ArgType type)
        {
            this.name = name;
            this.alias = alias;
            this.description = description;
            this.required = required;
            this.type = type;
        }

        // constructor NAME with one string
        internal AzmiArgument(string name, 
            ArgType type = ArgType.str, bool required = false)
        : this(
            name, 
            name[0], 
            $"Description for {name}", 
            required, 
            type) { }

        // constructor NAME + ALIAS? + DESCRIPTION 
        internal AzmiArgument(string name, char? alias, string description,
            ArgType type = ArgType.str, bool required = false)
        : this(
            name,
            alias,
            description,
            required,
            type) { }

        // constructor NAME + DESCRIPTION
        internal AzmiArgument(string name, [DisallowNull]string description,
            ArgType type = ArgType.str, bool required = false)
        : this(
            name,
            name[0],
            description,
            required,
            type) { }
    }
}
