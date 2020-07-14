using System;

[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("azmi-main-tests")]

namespace azmi_main
{
    [Serializable]
    public class AzmiException : Exception
    {
        public AzmiException() { }

        public AzmiException(string message) : base(message) { }

        public AzmiException(string message, Exception ex) : base(message, ex) { }

        protected AzmiException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext) { }

        internal static Exception IDCheck(string identity, Exception ex, bool tryGetToken = true)
        {
            if (string.IsNullOrEmpty(identity) && tryGetToken && GetTokenFails())
            {
                // exception type 1-a
                return new AzmiException("Missing identity argument", ex);
            }
            else if (string.IsNullOrEmpty(identity) && !tryGetToken)
            {
                // used only in gettoken without identity
                // exception type 1-b
                return new AzmiException("Missing identity argument", ex);
            }
            else if (ex.Message.Contains("See inner exception for details.") && (ex.InnerException != null))
            {
                if (ex.InnerException.Message.Contains("Identity not found"))
                {
                    // exception type 2
                    return new AzmiException("Managed identity not found", ex);
                } else
                {
                    // exception type 3
                    return ex.InnerException;
                }
            } else
            {
                // exception type 4
                return ex;
            }
        }

        internal static Exception WrongObject(Exception ex)
        {
            // exception type 5
            return new AzmiException("Cannot convert input object to proper class", ex);
        }

        private static bool GetTokenFails()
        {
            try
            {
                (new GetToken()).Execute(identity: null);
                return false;
            } catch
            {
                return true;
            }
        }
    }
}