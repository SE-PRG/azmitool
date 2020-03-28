using System;

namespace azmi_main
{
    public static class AzmiException
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
