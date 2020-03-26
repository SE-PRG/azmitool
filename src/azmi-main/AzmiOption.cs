using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;


namespace azmi_main
{

    public enum AcceptedTypes {stringType, boolType};

    public class AzmiOption
    {
        public string name; // "blob"
        public char? shortName; // "b" or null
        public string description;
        public bool required;
        public AcceptedTypes type; // string or bool

        public AzmiOption(string name, char? shortName, string description, bool required, AcceptedTypes type)
        {
            this.name = name;
            this.shortName = shortName;
            this.description = description;
            this.required = required;
            this.type = type;
        }

        //public AzmiOption(string name)
        //: this(name, name[0], $"Description for {name}", false, AcceptedTypes.stringType ) { }

        public AzmiOption(string name, AcceptedTypes type = AcceptedTypes.stringType, bool required = false)
        : this(name, name[0], $"Description for {name}", required, type) { }

        public AzmiOption( // name and null, or name and char
            string name, 
            char? shortName, 
            AcceptedTypes type = AcceptedTypes.stringType, 
            bool required = false)
        : this(name, shortName, $"Description for {name}", required, type) { }

        //public AzmiOption(string name)
        //: this(name, name[0], $"Description for {name}", false, AcceptedTypes.stringType) { }
    }
}
