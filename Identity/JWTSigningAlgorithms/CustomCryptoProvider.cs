using Microsoft.IdentityModel.Tokens;

namespace JWTSigningAlgorithms
{
    internal class CustomCryptoProvider : ICryptoProvider
    {
        public bool IsSupportedAlgorithm(string algorithm, params object[] args)
           => algorithm == "ES256K";

        public object Create(string algorithm, params object[] args)
        {
            if (algorithm == "ES256K"
                && args[0] is BouncyCastleEcdsaSecurityKey key)
            {
                return new CustomSignatureProvider(key, algorithm);
            }

            throw new NotSupportedException();
        }

        public void Release(object cryptoInstance)
        {
            if (cryptoInstance is IDisposable disposableObject)
                disposableObject.Dispose();
        }
    }
}