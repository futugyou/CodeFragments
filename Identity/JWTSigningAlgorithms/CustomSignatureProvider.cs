using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;

namespace JWTSigningAlgorithms
{
    public class CustomSignatureProvider : SignatureProvider
    {
        public CustomSignatureProvider(BouncyCastleEcdsaSecurityKey key, string algorithm)
            : base(key, algorithm) { }

        protected override void Dispose(bool disposing) { }

        public override byte[] Sign(byte[] input)
        {
            var ecDsaSigner = new ECDsaSigner();
            BouncyCastleEcdsaSecurityKey key = Key as BouncyCastleEcdsaSecurityKey;
            ecDsaSigner.Init(true, key.KeyParameters);

            byte[] hashedInput;
            using (var hasher = SHA256.Create())
            {
                hashedInput = hasher.ComputeHash(input);
            }

            var output = ecDsaSigner.GenerateSignature(hashedInput);

            var r = output[0].ToByteArrayUnsigned();
            var s = output[1].ToByteArrayUnsigned();

            var signature = new byte[r.Length + s.Length];
            r.CopyTo(signature, 0);
            s.CopyTo(signature, r.Length);

            return signature;
        }

        public override bool Verify(byte[] input, byte[] signature)
        {
            var ecDsaSigner = new ECDsaSigner();
            BouncyCastleEcdsaSecurityKey key = Key as BouncyCastleEcdsaSecurityKey;
            ecDsaSigner.Init(false, key.KeyParameters);

            byte[] hashedInput;
            using (var hasher = SHA256.Create())
            {
                hashedInput = hasher.ComputeHash(input);
            }

            var r = new BigInteger(1, signature.Take(32).ToArray());
            var s = new BigInteger(1, signature.Skip(32).ToArray());

            return ecDsaSigner.VerifySignature(hashedInput, r, s);
        }
    }
}