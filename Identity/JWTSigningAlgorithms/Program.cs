// See https://aka.ms/new-console-template for more information

using JWTSigningAlgorithms;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Math;
using System.Security.Cryptography;

Console.WriteLine("Hello, World!");

var privateKey = ECDsa.Create(ECCurve.NamedCurves.nistP256);
var publicKey = ECDsa.Create(privateKey.ExportParameters(true));

var now = DateTime.UtcNow;
var handler = new JsonWebTokenHandler();

string token = handler.CreateToken(new SecurityTokenDescriptor
{
    Issuer = "me",
    Audience = "you",
    NotBefore = now,
    Expires = now.AddMinutes(30),
    IssuedAt = now,
    Claims = new Dictionary<string, object> { { "sub", "123" } },
    SigningCredentials = new SigningCredentials(new ECDsaSecurityKey(privateKey), "ES256")
});

Console.WriteLine(token);

TokenValidationResult result = handler.ValidateToken(token, new TokenValidationParameters
{
    ValidIssuer = "me",
    ValidAudience = "you",
    IssuerSigningKey = new ECDsaSecurityKey(publicKey) // if change publicKey to privateKey ,it will get same result.
});

var isValid = result.IsValid;
Console.WriteLine(isValid);

string customkey = CustomSign();

string CustomSign()
{
    X9ECParameters secp256k1 = ECNamedCurveTable.GetByName("secp256k1");
    ECDomainParameters domainParams = new ECDomainParameters(
        secp256k1.Curve, secp256k1.G, secp256k1.N, secp256k1.H, secp256k1.GetSeed());

    byte[] d = Base64UrlEncoder.DecodeBytes("e8HThqO0wR_Qw4pNIb80Cs0mYuCSqT6BSQj-o-tKTrg");
    byte[] x = Base64UrlEncoder.DecodeBytes("A3hkIubgDggcoHzmVdXIm11gZ7UMaOa71JVf1eCifD8");
    byte[] y = Base64UrlEncoder.DecodeBytes("ejpRwmCvNMdXMOjR2DodOt09OLPgNUrcKA9hBslaFU0");

    var handler = new JsonWebTokenHandler();

    var token = handler.CreateToken(new SecurityTokenDescriptor
    {
        Issuer = "me",
        Audience = "you",
        SigningCredentials = new SigningCredentials(
            new BouncyCastleEcdsaSecurityKey(
                new ECPrivateKeyParameters(new BigInteger(1, d), domainParams))
            { KeyId = "123" },
            "ES256K")
    });

    var point = secp256k1.Curve.CreatePoint(
        new BigInteger(1, x),
        new BigInteger(1, y));

    var result = handler.ValidateToken(
        token,
        new TokenValidationParameters
        {
            ValidIssuer = "me",
            ValidAudience = "you",
            IssuerSigningKey = new BouncyCastleEcdsaSecurityKey(
                new ECPublicKeyParameters(point, domainParams))
            { KeyId = "123" }
        });
    var jwt = result.SecurityToken as JsonWebToken;
    return jwt?.EncodedToken;
}

Console.WriteLine(customkey);