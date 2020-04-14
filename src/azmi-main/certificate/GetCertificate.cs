﻿using System;
using System.Collections.Generic;
using System.Linq;

using Azure.Identity;
using Azure.Security.KeyVault.Certificates;

namespace azmi_main
{
    public class GetCertificate : IAzmiCommand
    {
        public SubCommandDefinition Definition()
        {
            return new SubCommandDefinition
            {

                name = "getcertificate",
                description = "Fetches latest or specific version of a certificate from key vault.",

                arguments = new AzmiArgument[] {
                new AzmiArgument("certificate", required: true, type: ArgType.url,
                    description: "URL of a certificate inside of key vault. Examples: https://my-key-vault.vault.azure.net/certificates/readThisCertificate or https://my-key-vault.vault.azure.net/certificates/readThisCertificate/103a7355c6094bc78307b2db7b85b3c2 ."),
                SharedAzmiArguments.identity,
                SharedAzmiArguments.verbose
            }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public string certificate { get; set; }
        }

        public List<string> Execute(object options)
        {
            AzmiArgumentsClass opt;
            try
            {
                opt = (AzmiArgumentsClass)options;
            }
            catch (Exception ex)
            {
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.certificate, opt.identity).ToStringList();
        }


        //
        // execute GetCertificate
        //

        public string Execute(string certificateIdentifierUrl, string identity = null)
        {
            (Uri keyVaultUri, string certificateName, string certificateVersion) = ValidateAndParseCertificateURL(certificateIdentifierUrl);

            var MIcredential = new ManagedIdentityCredential(identity);
            var certificateClient = new CertificateClient(keyVaultUri, MIcredential);

            // Retrieve a certificate
            try
            {
                // certificate (and key) is stored as a secret at the end in Azure
                string secretIdentifierUrl;
                // certificate has no specific version:
                // https://my-key-vault.vault.azure.net/certificates/readThisCertificate
                if (String.IsNullOrEmpty(certificateVersion))
                {
                    KeyVaultCertificateWithPolicy certificateWithPolicy = certificateClient.GetCertificate(certificateName);
                    secretIdentifierUrl = certificateWithPolicy.SecretId.ToString();
                }
                // certificate has specific version:
                // https://my-key-vault.vault.azure.net/certificates/readThisCertificate/103a7355c6094bc78307b2db7b85b3c2
                else
                {
                    KeyVaultCertificate certificate = certificateClient.GetCertificateVersion(certificateName, certificateVersion);
                    secretIdentifierUrl = certificate.SecretId.ToString();
                }

                string secret = new GetSecret().Execute(secretIdentifierUrl, identity);
                return secret;
            }
            catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

        private enum CertificateURLpattern
        {
            // https://my-key-vault.vault.azure.net/certificates/readThisCertificate/013a7355c6094bc78307b2db7b85b3c2
            NoSlash = 0,           //
            FirstSlash = 1,        // /
            CertificateFolder = 2, // certificates/
            CertificateName = 3,   // readThisCertificate/
            CertificateVersion = 4 // 013a7355c6094bc78307b2db7b85b3c2
        }

        private (Uri, string, string) ValidateAndParseCertificateURL(string certificateIdentifierUrl)
        {
            // Example of expected URLs: https://my-key-vault.vault.azure.net/certificates/readThisCertificate (latest version)
            // or https://my-key-vault.vault.azure.net/certificates/readThisCertificate/013a7355c6094bc78307b2db7b85b3c2 (specific version)
            Uri certificateIdentifierUri = new Uri(certificateIdentifierUrl);

            if (certificateIdentifierUri.Scheme != Uri.UriSchemeHttps)
                throw new UriFormatException($"Only '{Uri.UriSchemeHttps}' protocol is supported.");

            // e.g. https://my-key-vault.vault.azure.net
            Uri keyVaultUri = new Uri(certificateIdentifierUri.GetLeftPart(UriPartial.Authority));

            // Segments = /, certificates/, readThisCertificate/, 013a7355c6094bc78307b2db7b85b3c2
            CertificateURLpattern segmentsCount = (CertificateURLpattern)certificateIdentifierUri.Segments.Count();
            string certificateName = null;
            string certificateVersion = null;

            switch (segmentsCount)
            {
                case CertificateURLpattern.NoSlash:
                case CertificateURLpattern.FirstSlash:
                case CertificateURLpattern.CertificateFolder:
                    throw new UriFormatException($"URL '{certificateIdentifierUrl}' is missing a path to Azure certificate.");
                // certificate name only (no specific version)
                case CertificateURLpattern.CertificateName:
                    certificateName = certificateIdentifierUri.Segments.Last();
                    certificateVersion = null;
                    break;
                // certificate including specific version
                case CertificateURLpattern.CertificateVersion:
                    int lastButOne = certificateIdentifierUri.Segments.Length - 2;
                    certificateName = certificateIdentifierUri.Segments[lastButOne];
                    certificateVersion = certificateIdentifierUri.Segments.Last();
                    break;
                default:
                    throw new InvalidOperationException("URL seems too long and does not seem to be a valid URL to Azure certificate.");
            }

            return (keyVaultUri, certificateName, certificateVersion);
        }
    }
}
