using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public interface IAzmiCommand
    {
        public string Name();
        public string Description();

        public AzmiOption[] AzmiOptions();

        //public object Options();

        public string Execute(object options);
    }

}
