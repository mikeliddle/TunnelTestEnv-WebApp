using System.Security.Cryptography.X509Certificates;

namespace CertificateApi.Common
{
    public static class CertificateUtils
    {
        public static bool ValidateCertificate(X509Certificate2 clientCertificate)
        {
            var cert = new X509Certificate2(Path.Combine("cacert.pem"), "");

            if (clientCertificate.Issuer == cert.SubjectName.Name)
            {
                var chain = new X509Chain();
                chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                chain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
                chain.ChainPolicy.CustomTrustStore.Add(cert);
                
                try
                {
                    return chain.Build(clientCertificate);
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    return false;
                }
            }
            return false;
        }
    }
}