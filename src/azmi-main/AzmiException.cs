using System;

namespace azmi_main
{
    internal static class AzmiException
    {
        internal static Exception IDCheck(string identity, Exception ex)
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

        internal static Exception WrongObject()
        {
            return new ArgumentException("Cannot convert object to proper class");
        }
    }
}
