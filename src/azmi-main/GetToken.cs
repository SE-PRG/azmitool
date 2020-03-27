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

        public SubCommandDefinition Definition() {
            return new SubCommandDefinition {
                
                name = "gettoken",
                description = "test for classified gettoken subcommand",
                
                arguments = new AzmiOption[] {
                    new AzmiOption("endpoint","Endpoint against which to authenticate. Examples: management, storage. Default 'management'"),
                    SharedAzmiOptions.identity,
                    new AzmiOption("jwt-format", "Print token in JSON Web Token (JWT) format.", ArgType.flag),
                    SharedAzmiOptions.verbose
                }
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

            //return $"id: {identity}, endpoint: {endpoint}, jwt: {jwt_format}";
            
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
