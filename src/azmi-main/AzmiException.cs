using System;

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

        internal static Exception IDCheck(string identity, Exception ex)
        {
            if (string.IsNullOrEmpty(identity) && GetTokenFails())
            {
                return new AzmiException("Missing identity argument", ex);
            } else if (ex.Message.Contains("See inner exception for details.") && (ex.InnerException != null))
            {
                if (ex.InnerException.Message.Contains("Identity not found"))
                {
                    return new AzmiException("Managed identity not found", ex);
                } else
                {
                    return ex.InnerException;
                }
            } else
            {
                return ex;
            }
        }

        internal static Exception WrongObject(Exception ex)
        {
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