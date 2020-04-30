using System;
using System.Collections.Generic;
using System.IO;

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
                description = "Fetches latest or specific version of a secret from key vault.",

                arguments = new AzmiArgument[] {
                    new AzmiArgument("secret", required: true, type: ArgType.url,
                        description: "URL of a secret inside of key vault. Examples: https://my-key-vault.vault.azure.net/secrets/mySecret.pwd or https://my-key-vault.vault.azure.net/secrets/mySecret.pwd/67d1f6c499824607b81d5fa852f9865c ."),
                    SharedAzmiArguments.identity,
                    new AzmiArgument("file",
                        description: "Path to local file to which secret will be saved to. Examples: /tmp/mySecret.pwd, ./mySecret.pwd"),
                    SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string secret { get; set; }
            public string file { get; set; }
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

            return Execute(opt.secret, opt.file, opt.identity).ToStringList();
        }

        //
        // execute GetSecret
        //

        public string Execute(string secretIdentifierUrl, string filePath = null, string identity = null)
        {
            (Uri keyVaultUri, string secretName, string secretVersion) = ValidateAndParseSecretURL(secretIdentifierUrl);

            var MIcredential = new ManagedIdentityCredential(identity);
            var secretClient = new SecretClient(keyVaultUri, MIcredential);

            // Retrieve a secret
            try
            {
                KeyVaultSecret secret = secretClient.GetSecret(secretName, secretVersion);
                string secretValue = secret.Value;

                if (String.IsNullOrEmpty(filePath))
                {   // print to stdout
                    return secretValue;
                }
                else
                {   // creates or overwrites file and saves secret into it
                    File.WriteAllText(filePath, secretValue);
                    return "Saved";
                }
            } catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

        //
        // private methods
        //

        private enum SecretURLsegmentsScheme
        {
            // https://my-key-vault.vault.azure.net/secrets/mySecret.pwd/67d1f6c499824607b81d5fa852f9865c
            NoSlash = 0,       //
            FirstSlash = 1,    // /
            SecretFolder = 2,  // secrets/
            SecretName = 3,    // mySecret.pwd/
            SecretVersion = 4  // 67d1f6c499824607b81d5fa852f9865c
        }

        private (Uri, string, string) ValidateAndParseSecretURL(string secretIdentifierUrl)
        {
        // Example of expected URLs: https://my-key-vault.vault.azure.net/secrets/mySecret.pwd (latest version)
        // or https://my-key-vault.vault.azure.net/secrets/mySecret.pwd/67d1f6c499824607b81d5fa852f9865c (specific version)
        Uri secretIdentifierUri = new Uri(secretIdentifierUrl);

            if (secretIdentifierUri.Scheme != Uri.UriSchemeHttps)
                throw new UriFormatException($"Only '{Uri.UriSchemeHttps}' protocol is supported.");

            // e.g. https://my-key-vault.vault.azure.net
            Uri keyVaultUri = new Uri(secretIdentifierUri.GetLeftPart(UriPartial.Authority));

            // Segments = /, secrets/, mySecret.pwd/, 67d1f6c499824607b81d5fa852f9865c
            SecretURLsegmentsScheme segmentsCount = (SecretURLsegmentsScheme)secretIdentifierUri.Segments.Count();
            string secretName = null;
            string secretVersion = null;

            switch (segmentsCount)
            {
                case SecretURLsegmentsScheme.NoSlash:
                case SecretURLsegmentsScheme.FirstSlash:
                case SecretURLsegmentsScheme.SecretFolder:
                    throw new UriFormatException($"URL '{secretIdentifierUrl}' is missing a path to Azure secret.");
                // secret name only (no specific version)
                case SecretURLsegmentsScheme.SecretName:
                    secretName = secretIdentifierUri.Segments.Last();
                    secretVersion = null;
                    break;
                // secret including specific version
                case SecretURLsegmentsScheme.SecretVersion:
                    int lastButOne = secretIdentifierUri.Segments.Length - 2;
                    secretName = secretIdentifierUri.Segments[lastButOne];
                    secretVersion = secretIdentifierUri.Segments.Last();
                    break;
                default:
                    throw new InvalidOperationException("URL seems too long and does not seem to be a valid URL to Azure secret.");
            }

            return (keyVaultUri, secretName, secretVersion);
        }
    }
}
