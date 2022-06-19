using System.Numerics;
using System.Security.Cryptography;
using System.Runtime.CompilerServices;

namespace Sulakore.Cryptography;

/// <summary>
/// Provides functionality for two parties establish a shared secret using RSA and Diffie-hellman key exchange.
/// </summary>
/// <remarks>
/// NOTE: This class is not considered cryptographically secure; it is a managed implementation, does not do constant-time operations,
/// constant memory access patterns or maintain any other best practices required to be resilient against side-channel attacks!
/// </remarks>
public sealed class HKeyExchange
{
    private readonly static RandomNumberGenerator _rng = RandomNumberGenerator.Create();

    public int BlockSize { get; }

    public BigInteger Modulus { get; }
    public BigInteger Exponent { get; }
    public BigInteger PrivateExponent { get; }

    public BigInteger DHPublic { get; private set; }
    public BigInteger DHPrivate { get; private set; }

    public BigInteger DHPrime { get; private set; }
    public BigInteger DHGenerator { get; private set; }

    public bool CanDecrypt => PrivateExponent != BigInteger.Zero;

    public HKeyExchange(BigInteger exponent, BigInteger modulus)
    {
        Exponent = exponent;
        Modulus = modulus;
        BlockSize = modulus.GetByteCount();
    }
    public HKeyExchange(BigInteger exponent, BigInteger modulus, BigInteger privateExponent, int dhBitSize = 256)
        : this(exponent, modulus)
    {
        PrivateExponent = privateExponent;

        if (privateExponent != BigInteger.Zero)
        {
            GenerateDHPrimes(dhBitSize);
            GenerateDHKeys(DHPrime, DHGenerator);
        }
    }

    public BigInteger CalculatePublic(BigInteger value) => BigInteger.ModPow(value, Exponent, Modulus);
    public BigInteger CalculatePrivate(BigInteger value) => BigInteger.ModPow(value, PrivateExponent, Modulus);

    public BigInteger SignPrime() => Sign(DHPrime);
    public BigInteger SignGenerator() => Sign(DHGenerator);

    public BigInteger GetPublicKey() => CanDecrypt ? Sign(DHPublic) : Encrypt(DHPublic);

    public BigInteger GetSharedKey(BigInteger publicKey)
    {
        BigInteger bigInteger = CanDecrypt ? Decrypt(publicKey) : Verify(publicKey);
        return BigInteger.ModPow(bigInteger, DHPrivate, DHPrime);
    }
    public void VerifyDHPrimes(BigInteger p, BigInteger g)
    {
        DHPrime = Verify(p);
        if (DHPrime <= 2)
            throw new ArgumentException("P cannot be less than, or equal to 2.", nameof(p));

        DHGenerator = Verify(g);
        if (DHGenerator >= DHPrime)
            throw new ArgumentException("G cannot be greater than, or equal to P.", nameof(g));

        GenerateDHKeys(DHPrime, DHGenerator);
    }

    // TODO: Consider trying to just use the OS RSA impl over interop. It's really painful to deal with though.
    public BigInteger Sign(BigInteger value) => PKCSPad(CalculatePrivate(value));
    public BigInteger Verify(BigInteger value) => CalculatePublic(PKCSUnpad(value));

    public BigInteger Encrypt(BigInteger value) => CalculatePublic(PKCSPad(value));
    public BigInteger Decrypt(BigInteger value) => PKCSUnpad(CalculatePrivate(value));

    // TODO: Would using the MODP DH groups from RFC 3526 be easier?
    private static BigInteger CreateRandomProbablePrime(int bitSize)
    {
        Span<byte> integerData = stackalloc byte[(bitSize + 7) / 8];
        BigInteger probablePrime;

        do
        {
            RandomNumberGenerator.Fill(integerData);

            integerData[^1] &= 0x7f;
            probablePrime = new BigInteger(integerData);
        }
        while (IsRandomProbablePrime(probablePrime, 16, integerData));

        return probablePrime;
    }
    private static bool IsRandomProbablePrime(BigInteger candidate, int certainty, Span<byte> integerBuffer)
    {
        if (candidate == 2 || candidate == 3) return true;
        if (candidate < 2 || candidate % 2 == 0) return false;

        // Miller-rabin rewrite
        BigInteger d = candidate - 1;
        uint s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s++;
        }

        BigInteger a;
        for (int i = 0; i < certainty; i++)
        {
            do
            {
                RandomNumberGenerator.Fill(integerBuffer);
                a = new BigInteger(integerBuffer);
            }
            while (a < 2 || a >= candidate - 2);

            BigInteger x = BigInteger.ModPow(a, d, candidate);
            if (x == 1 || x == candidate - 1)
                continue;

            for (int r = 0; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, candidate);
                if (x == 1)
                    return false;
                if (x == candidate - 1)
                    break;
            }

            if (x != candidate - 1)
                return false;
        }
        return true;
    }

    private void GenerateDHPrimes(int bitSize)
    {
        DHPrime = CreateRandomProbablePrime(bitSize);
        DHGenerator = CreateRandomProbablePrime(bitSize);

        if (DHGenerator > DHPrime)
            (DHGenerator, DHPrime) = (DHPrime, DHGenerator);
    }
    private void GenerateDHKeys(BigInteger p, BigInteger g, int bitSize = 256)
    {
        DHPrivate = CreateRandomProbablePrime(bitSize);
        DHPublic = BigInteger.ModPow(g, DHPrivate, p);
    }

    /// <summary>
    /// Pads an integer using RSA PKCS#1 v1.5 mode 0x02. The value must be no longer than the length of <see cref="Modulus"/> minus 11 bytes.
    /// </summary>
    /// <param name="value">The integer to be padded.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="value"/> was too large.</exception>
    // SAFETY: Only overwritten slice of the uninitialized span is returned.
    [SkipLocalsInit]
    private BigInteger PKCSPad(BigInteger value)
    {
        // Value can be at most BlockSize - 11 bytes long
        Span<byte> data = stackalloc byte[BlockSize];
        int valueLength = value.GetByteCount();

        if (valueLength > BlockSize - 11 ||
            !value.TryWriteBytes(data.Slice(data.Length - valueLength), out int written))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        // padded_value = 0x00 || 0x02 || padding || 0x00 || value
        data[0] = 0;
        data[1] = 2;

        Span<byte> padding = data.Slice(2, data.Length - written - 3);

        // FUTURE: This method will likely be obsoleted in near future. See https://github.com/dotnet/runtime/issues/42763
        _rng.GetNonZeroBytes(padding);

        padding[^1] = 0;
        return new BigInteger(padding);
    }

    /// <summary>
    /// Unpads an integers from RSA PKCS#1 1.5 padded message.
    /// </summary>
    /// <param name="value">The padded value.</param>
    /// <exception cref="ArgumentOutOfRangeException">If the <paramref name="value"/> was too large.</exception>
    // SAFETY: Only overwritten slice of the uninitialized span is returned.
    [SkipLocalsInit]
    private BigInteger PKCSUnpad(BigInteger value)
    {
        Span<byte> data = stackalloc byte[BlockSize];

        if (!value.TryWriteBytes(data, out int bytesWritten))
        {
            throw new ArgumentOutOfRangeException(nameof(value));
        }

        // TODO: Do some length checks
        int dataOffset = data.Slice(2, bytesWritten).IndexOf((byte)0) + 1;
        return new BigInteger(data.Slice(dataOffset));
    }

    public static HKeyExchange Create(int keySizeInBits) => Create(RSA.Create(keySizeInBits));
    public static HKeyExchange Create(RSA rsa)
    {
        RSAParameters keys = rsa.ExportParameters(true);
        return new HKeyExchange(new BigInteger(keys.Modulus), new BigInteger(keys.Exponent), new BigInteger(keys.D));
    }
}