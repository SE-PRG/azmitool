using System;
using System.Collections.Generic;
using System.Text;

namespace azmi_main
{
    public class AzmiException : Exception
    {
        public static Exception IDCheck(string identity, Exception ex)
        {
            if (string.IsNullOrEmpty(identity))
            {
                return new ArgumentNullException("Missing identity argument", ex);
            } else if (ex.Message.Contains("See inner exception for details.")
                && (ex.InnerException != null)
                && (ex.InnerException.Message.Contains("Identity not found")))
            {
                return new ArgumentException("Managed identity not found", ex);
            } else
            {
                return ex;
            }
        }
    }
}
