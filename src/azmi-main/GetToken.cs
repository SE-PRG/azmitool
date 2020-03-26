using System;
using System.IdentityModel.Tokens.Jwt;
using Azure.Core;
using Azure.Identity;

namespace azmi_main
{    
    public class GetToken : IAzmiCommand
    {

        //
        // Declare command elements
        //

        public string Name() { return "gettoken2"; }
        public string Description() { return "test for classified gettoken subcommand"; }

        public AzmiOption[] AzmiOptions() { 
            return new AzmiOption[] {
                SharedAzmiOptions.identity,
                SharedAzmiOptions.verbose,
                new AzmiOption("endpoint"),
                new AzmiOption("jwt-format", AcceptedTypes.boolType)
            };
        }
        
        public class Options : SharedOptions {
            public string endpoint { get; set; }
            public bool jwtformat { get; set; }
        }

        //
        // Execute GetToken
        //

        public string Execute(object options) {return Execute((Options)options);}

        public string Execute(Options options)
        {
            // parse arguments
            string identity = options.identity;
            string endpoint = options.endpoint ?? "management";
            bool jwt_format = options.jwtformat;

            return $"id: {identity}, endpoint: {endpoint}, jwt: {jwt_format}";
            
            // method start
            var Cred = new ManagedIdentityCredential(identity);
            var Scope = new String[] { $"https://{endpoint}.azure.com" };
            var Request = new TokenRequestContext(Scope);
            var Token = Cred.GetToken(Request);
            return (jwt_format) ? Decode_JWT(Token.Token) : Token.Token;
        }

        //
        // Private Methods
        //

        private string Decode_JWT(string tokenEncoded)
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenDecoded = handler.ReadJwtToken(tokenEncoded);
            return tokenDecoded.ToString(); // decoded JSON Web Token

        }
    }    
}
