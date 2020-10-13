using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using NLog;

namespace azmi_main
{
    public class GetCertificate : IAzmiCommand
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static readonly string className = nameof(GetCertificate);

        public SubCommandDefinition Definition()
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            return new SubCommandDefinition
            {

                name = "getcertificate",
                description = "Fetches latest or specific version of a certificate(s) and private key bundle from key vault.",

                arguments = new AzmiArgument[] {
                new AzmiArgument("certificate", required: true, type: ArgType.url,
                    description: "URL of a certificate inside of key vault. Examples: https://my-kv.vault.azure.net/certificates/cert1 or https://my-kv.vault.azure.net/certificates/cert2/103a7355c6094bc78307b2db7b85b3c2 ."),
                SharedAzmiArguments.identity,
                new AzmiArgument("file",
                    description: "Path to local file to which bundle will be saved to. Examples: /tmp/readThisCertificate.crt, ./readThisCertificatePfxFormat.pfx"),
                SharedAzmiArguments.verbose
                }
            };
        }

        public class AzmiArgumentsClass : SharedAzmiArgumentsClass
        {
            public Uri certificate { get; set; }
            public string file { get; set; }
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
                throw AzmiException.WrongObject(ex);
            }

            return Execute(opt.certificate, opt.file, opt.identity).ToStringList();
        }


        //
        // execute GetCertificate
        //

        public string Execute(Uri certificateIdentifier, string filePath = null, string identity = null)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            (Uri keyVaultUri, string certificateName, string certificateVersion) = ValidateAndParseCertificateURL(certificateIdentifier);

            var MIcredential = new ManagedIdentityCredential(identity);
            var certificateClient = new CertificateClient(keyVaultUri, MIcredential);

            // Retrieve a certificate (certificate = certificate and private key bundle in Azure terminology)
            // PEM (Privacy Enhanced Mail) or PFX (Personal Information Exchange; PKCS#12 archive file format) formats
            // depends on what content type you set in Azure Key Vault at respective certificate.
            // Both formats usually contain a certificate (possibly with its assorted set of CA certificates) and the corresponding private key

            // Download CER format (X.509 certificate)
            // single certificate, alone and without any wrapping (no private key, no password protection, just the certificate)
            // not supported
            try
            {
                // certificate (and key) is stored as a secret at the end in Azure
                Uri secretIdentifier;
                if (String.IsNullOrEmpty(certificateVersion))
                {
                    // certificate has no specific version:
                    // https://my-key-vault.vault.azure.net/certificates/readThisCertificate
                    KeyVaultCertificateWithPolicy certificateWithPolicy = certificateClient.GetCertificate(certificateName);
                    secretIdentifier = new Uri(certificateWithPolicy.SecretId.ToString());
                }
                else
                {
                    // certificate has specific version:
                    // https://my-key-vault.vault.azure.net/certificates/readThisCertificate/103a7355c6094bc78307b2db7b85b3c2
                    KeyVaultCertificate certificate = certificateClient.GetCertificateVersion(certificateName, certificateVersion);
                    secretIdentifier = new Uri(certificate.SecretId.ToString());
                }

                // filePath: null means get secret into variable only
                // otherwise secret may be unintentionally saved to file by GetSecret() method
                string secret = new GetSecret().Execute(secretIdentifier, filePath: null, identity);

                if (String.IsNullOrEmpty(filePath))
                {   // print to stdout
                    return secret;
                }
                else
                {   // creates or overwrites file and saves secret into it
                    File.WriteAllText(filePath, secret);
                    return "Saved";
                }
            }
            catch (Exception ex)
            {
                throw AzmiException.IDCheck(identity, ex);
            }
        }

        //
        // private methods
        //

        private enum CertificateURLsegmentsScheme
        {
            // https://my-key-vault.vault.azure.net/certificates/readThisCertificate/013a7355c6094bc78307b2db7b85b3c2
            NoSlash = 0,           //
            FirstSlash = 1,        // /
            CertificateFolder = 2, // certificates/
            CertificateName = 3,   // readThisCertificate/
            CertificateVersion = 4 // 013a7355c6094bc78307b2db7b85b3c2
        }

        private (Uri, string, string) ValidateAndParseCertificateURL(Uri certificateIdentifier)
        {
            logger.Debug($"Entering {className}::{MethodBase.GetCurrentMethod().Name}()");

            // Example of expected URLs: https://my-key-vault.vault.azure.net/certificates/readThisCertificate (latest version)
            // or https://my-key-vault.vault.azure.net/certificates/readThisCertificate/013a7355c6094bc78307b2db7b85b3c2 (specific version)

            if (certificateIdentifier.Scheme != Uri.UriSchemeHttps)
                throw new UriFormatException($"Only '{Uri.UriSchemeHttps}' protocol is supported.");

            // e.g. https://my-key-vault.vault.azure.net
            Uri keyVault = new Uri(certificateIdentifier.GetLeftPart(UriPartial.Authority));

            // Segments = /, certificates/, readThisCertificate/, 013a7355c6094bc78307b2db7b85b3c2
            CertificateURLsegmentsScheme segmentsCount = (CertificateURLsegmentsScheme)certificateIdentifier.Segments.Count();
            string certificateName = null;
            string certificateVersion = null;

            switch (segmentsCount)
            {
                case CertificateURLsegmentsScheme.NoSlash:
                case CertificateURLsegmentsScheme.FirstSlash:
                case CertificateURLsegmentsScheme.CertificateFolder:
                    throw new UriFormatException($"URL '{certificateIdentifier}' is missing a path to Azure certificate.");
                // certificate name only (no specific version)
                case CertificateURLsegmentsScheme.CertificateName:
                    certificateName = certificateIdentifier.Segments.Last();
                    certificateVersion = null;
                    break;
                // certificate including specific version
                case CertificateURLsegmentsScheme.CertificateVersion:
                    int lastButOne = certificateIdentifier.Segments.Length - 2;
                    certificateName = certificateIdentifier.Segments[lastButOne];
                    certificateVersion = certificateIdentifier.Segments.Last();
                    break;
                default:
                    throw new InvalidOperationException("URL seems too long and does not seem to be a valid URL to Azure certificate.");
            }

            return (keyVault, certificateName, certificateVersion);
        }
    }
}
