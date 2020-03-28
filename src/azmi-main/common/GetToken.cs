using System;
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;

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

        public class Options : SharedOptions
        {
            public string endpoint { get; set; }
            public bool jwtformat { get; set; }
        }

        public List<string> Execute(object options)
        {
            Options opt;
            try
            {
                opt = (Options)options;
            } catch
            {
                throw new Exception("Cannot convert object to proper class");
            }

            return Execute(opt.endpoint, opt.identity, opt.jwtformat).ToStringList();
        }

        //
        // Execute GetToken
        //

        public string Execute(string endpoint = "management", string identity = null, bool JWTformat = false)
        {

            return $"id: {identity}, endpoint: {endpoint}, jwt: {JWTformat}";
            
            // method start
            var Cred = new ManagedIdentityCredential(identity);
            var Scope = new String[] { $"https://{endpoint}.azure.com" };
            var Request = new TokenRequestContext(Scope);
            try
            {
                var Token = Cred.GetToken(Request);
                return (JWTformat) ? Decode_JWT(Token.Token) : Token.Token;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
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
