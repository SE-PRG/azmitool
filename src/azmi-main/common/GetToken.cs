using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using Azure.Core;
using Azure.Identity;
using NLog;

namespace azmi_main
{
    public class GetToken : IAzmiCommand
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string className = nameof(GetToken);

        //
        // Declare command elements
        //

        public SubCommandDefinition Definition()
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            return new SubCommandDefinition
            {
                name = "gettoken",
                description = "Obtains Azure authorization token for usage in other command line tools.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("endpoint","Endpoint against which to authenticate. Examples: management, storage. Default 'management'"),
                    SharedAzmiArguments.identity,
                    new AzmiArgument("jwt-format", "Print token in JSON Web Token (JWT) format.", ArgType.flag),
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string endpoint { get; set; }
            public bool jwtformat { get; set; }
        }

        public List<string> Execute(object options)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            }
            catch (Exception ex)
            {
                logger.Error(ex, "WrongObject");
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.endpoint, opt.identity, opt.jwtformat).ToStringList();
        }

        //
        // Execute GetToken
        //

        public string Execute(string endpoint = "management", string identity = null, bool JWTformat = false)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            // method start
            var Cred = new ManagedIdentityCredential(identity);
            if (String.IsNullOrEmpty(endpoint)) { endpoint = "management"; }
            var Scope = new String[] { $"https://{endpoint}.azure.com" };
            var Request = new TokenRequestContext(Scope);
            try
            {
                var Token = Cred.GetToken(Request);
                return (JWTformat) ? Decode_JWT(Token.Token) : Token.Token;
            }
            catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex, false);
            }
        }

        //
        // Private Methods
        //

        private string Decode_JWT(string tokenEncoded)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            var handler = new JwtSecurityTokenHandler();
            var tokenDecoded = handler.ReadJwtToken(tokenEncoded);
            return tokenDecoded.ToString(); // decoded JSON Web Token

        }
    }
}
