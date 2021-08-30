using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JWTSigningAlgorithms
{
    public class BouncyCastleEcdsaSecurityKey : AsymmetricSecurityKey
    {
        public BouncyCastleEcdsaSecurityKey(ECKeyParameters keyParameters)
        {
            KeyParameters = keyParameters;
            CryptoProviderFactory.CustomCryptoProvider = new CustomCryptoProvider();
        }

        public ECKeyParameters KeyParameters { get; }
        public override int KeySize => throw new NotImplementedException();

        [Obsolete("HasPrivateKey method is deprecated, please use PrivateKeyStatus.")]
        public override bool HasPrivateKey => KeyParameters.IsPrivate;

        public override PrivateKeyStatus PrivateKeyStatus
            => KeyParameters.IsPrivate ? PrivateKeyStatus.Exists : PrivateKeyStatus.DoesNotExist;
    }
}
