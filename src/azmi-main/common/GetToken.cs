﻿using System;
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
            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            } catch
            {
                throw AzmiException.WrongObject();                    
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
