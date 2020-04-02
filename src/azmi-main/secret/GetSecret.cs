using System;
using System.Collections.Generic;

using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using System.Linq;


namespace azmi_main
{
    public class GetSecret : IAzmiCommand
    {
        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "getsecret",
                description = "Fetches latest version of a secret from key vault.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("secret", required: true, type: ArgType.url,
                        description: "URL of a secret inside of key vault. Example: https://my-key-vault.vault.azure.net/secrets/mySecret.pwd"),
                    SharedAzmiArguments.identity,
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string secret { get; set; }
        }

        public List<string> Execute(object options)
        {
            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            } catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.secret, opt.identity).ToStringList();
        }

        //
        // execute GetSecret
        //

        public string Execute(string secretIdentifierUrl, string identity = null)
        {
            (Uri keyVaultUri, string secretName) = ValidateAndParseSecretURL(secretIdentifierUrl);

            var MIcredential = new ManagedIdentityCredential(identity);
            var secretClient = new SecretClient(keyVaultUri, MIcredential);

            // Retrieve a secret
            try
            {
                KeyVaultSecret secret = secretClient.GetSecret(secretName);
                return secret.Value;
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

        //
        // private methods
        //

        private (Uri, string) ValidateAndParseSecretURL(string secretIdentifierUrl)
        {
            // Example of expected URL: https://my-key-vault.vault.azure.net/secrets/mySecret.pwd
            Uri secretIdentifierUri = new Uri(secretIdentifierUrl);

            if (secretIdentifierUri.Scheme != Uri.UriSchemeHttps)
                throw new UriFormatException($"Only '{Uri.UriSchemeHttps}' protocol is supported.");

            // e.g. https://my-key-vault.vault.azure.net
            Uri keyVaultUri = new Uri(secretIdentifierUri.GetLeftPart(UriPartial.Authority));
            // Segments = /, secrets/, mySecret.pwd
            if (secretIdentifierUri.Segments.Count() <= 2)
                throw new UriFormatException($"URL '{secretIdentifierUrl}' is missing a path to secret.");

            string secretName = secretIdentifierUri.Segments.Last();

            return (keyVaultUri, secretName);
        }
    }
}
