using System.Buffers;

using Sulakore.Cryptography.Ciphers;

using Xunit;

namespace Sulakore.Tests.Cryptography;

public class CipherTests
{
    // RFC 7539 - ChaCha20 Test vectors
    // 2.3.2. - "Test Vector for the ChaCha20 Block Function"
    [Theory]
    [InlineData("000102030405060708090a0b0c0d0e0f101112131415161718191a1b1c1d1e1f", "000000090000004a00000000", 1, "10f1e7e4d13b5915500fdd1fa32071c4c7d1f4c733c068030422aa9ac3d46c4ed2826446079faa0914c2d705d98b02a2b5129cd1de164eb9cbd083e8a2503c4e")]
    // Appendix A. - "Additional Test Vectors"
    [InlineData("0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000", 0, "76b8e0ada0f13d90405d6ae55386bd28bdd219b8a08ded1aa836efcc8b770dc7da41597c5157488d7724e03fb8d84a376a43b8f41518a11cc387b669b2ee6586")]
    [InlineData("0000000000000000000000000000000000000000000000000000000000000000", "0000000000000000", 1, "9f07e7be5551387a98ba977c732d080dcb0f29a048e3656912c6533e32ee7aed29b721769ce64e43d57133b074d839d531ed1f28510afb45ace10a1f4b794d6f")]
    [InlineData("0000000000000000000000000000000000000000000000000000000000000001", "0000000000000000", 1, "3aeb5224ecf849929b9d828db1ced4dd832025e8018b8160b82284f3c949aa5a8eca00bbb4a73bdad192b5c42f73f2fd4e273644c8b36125a64addeb006c13a0")]
    // TODO: #4 fails. Seems to be issue with the initial block count
    // TODO: [InlineData("00ff000000000000000000000000000000000000000000000000000000000000", "0000000000000000", 2, "72d54dfbf12ec44b362692df94137f328fea8da73990265ec1bbbea1ae9af0ca13b25aa26cb4a648cb9b9d1be65b2c0924a66c54d545ec1b7374f4872e99f096")]
    [InlineData("0000000000000000000000000000000000000000000000000000000000000000", "0000000000000002", 0, "c2c64d378cd536374ae204b9ef933fcd1a8b2288b3dfa49672ab765b54ee27c78a970e0e955c14f3a88e741b97c286f75f8fc299e8148362fa198a39531bed6d")]
    public void ChaCha20_TestVectors_RFC7539(string key, string nonce, uint blockCount, string expectedKeystream)
    {
        byte[] keyBytes = Convert.FromHexString(key);
        byte[] nonceBytes = Convert.FromHexString(nonce);
        Span<byte> actualKeystreamBytes = stackalloc byte[64];
        
        var chacha = new ChaCha20(keyBytes, nonceBytes, blockCount);
        chacha.Process(actualKeystreamBytes);

        string actualKeystream = Convert.ToHexString(actualKeystreamBytes);
        Assert.Equal(expectedKeystream, actualKeystream, ignoreCase: true);
    }

    // RFC 6229 - RC4 Test Vectors
    [Fact]
    public void RC4_TestVector_0x0102030405()
    {
        byte[] keyStream = ArrayPool<byte>.Shared.Rent(8192);
        Array.Clear(keyStream); // ArrayPool buffers are not zeroed

        var rc4 = new RC4(new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05 });
        rc4.Process(keyStream);

        AssertKeyStreamEqual(keyStream, 0, "b2396305f03dc027ccc3524a0a1118a8");
        AssertKeyStreamEqual(keyStream, 16, "6982944f18fc82d589c403a47a0d0919");
        AssertKeyStreamEqual(keyStream, 240, "28cb1132c96ce286421dcaadb8b69eae");
        AssertKeyStreamEqual(keyStream, 256, "1cfcf62b03eddb641d77dfcf7f8d8c93");
        AssertKeyStreamEqual(keyStream, 496, "42b7d0cdd918a8a33dd51781c81f4041");
        AssertKeyStreamEqual(keyStream, 512, "6459844432a7da923cfb3eb4980661f6");
        AssertKeyStreamEqual(keyStream, 752, "ec10327bde2beefd18f9277680457e22");
        AssertKeyStreamEqual(keyStream, 768, "eb62638d4f0ba1fe9fca20e05bf8ff2b");
        AssertKeyStreamEqual(keyStream, 1008, "45129048e6a0ed0b56b490338f078da5");
        AssertKeyStreamEqual(keyStream, 1024, "30abbcc7c20b01609f23ee2d5f6bb7df");
        AssertKeyStreamEqual(keyStream, 1520, "3294f744d8f9790507e70f62e5bbceea");
        AssertKeyStreamEqual(keyStream, 1536, "d8729db41882259bee4f825325f5a130");
        AssertKeyStreamEqual(keyStream, 2032, "1eb14a0c13b3bf47fa2a0ba93ad45b8b");
        AssertKeyStreamEqual(keyStream, 2048, "cc582f8ba9f265e2b1be9112e975d2d7");
        AssertKeyStreamEqual(keyStream, 3056, "f2e30f9bd102ecbf75aaade9bc35c43c");
        AssertKeyStreamEqual(keyStream, 3072, "ec0e11c479dc329dc8da7968fe965681");
        AssertKeyStreamEqual(keyStream, 4080, "068326a2118416d21f9d04b2cd1ca050");
        AssertKeyStreamEqual(keyStream, 4096, "ff25b58995996707e51fbdf08b34d875");

        ArrayPool<byte>.Shared.Return(keyStream);
    }

    [Fact]
    public void RC4_TestVector_IETF()
    {
        byte[] keyStream = ArrayPool<byte>.Shared.Rent(8192);
        Array.Clear(keyStream); // ArrayPool buffers are not zeroed

        // echo -n "Internet Engineering Task Force" | sha256sum
        var rc4 = new RC4(Convert.FromHexString("1ada31d5cf688221c109163908ebe51debb46227c6cc8b37641910833222772a"));
        rc4.Process(keyStream);

        AssertKeyStreamEqual(keyStream, 0, "dd5bcb0018e922d494759d7c395d02d3");
        AssertKeyStreamEqual(keyStream, 16, "c8446f8f77abf737685353eb89a1c9eb");
        AssertKeyStreamEqual(keyStream, 240, "af3e30f9c095045938151575c3fb9098");
        AssertKeyStreamEqual(keyStream, 256, "f8cb6274db99b80b1d2012a98ed48f0e");
        AssertKeyStreamEqual(keyStream, 496, "25c3005a1cb85de076259839ab7198ab");
        AssertKeyStreamEqual(keyStream, 512, "9dcbc183e8cb994b727b75be3180769c");
        AssertKeyStreamEqual(keyStream, 752, "a1d3078dfa9169503ed9d4491dee4eb2");
        AssertKeyStreamEqual(keyStream, 768, "8514a5495858096f596e4bcd66b10665");
        AssertKeyStreamEqual(keyStream, 1008, "5f40d59ec1b03b33738efa60b2255d31");
        AssertKeyStreamEqual(keyStream, 1024, "3477c7f764a41baceff90bf14f92b7cc");
        AssertKeyStreamEqual(keyStream, 1520, "ac4e95368d99b9eb78b8da8f81ffa795");
        AssertKeyStreamEqual(keyStream, 1536, "8c3c13f8c2388bb73f38576e65b7c446");
        AssertKeyStreamEqual(keyStream, 2032, "13c4b9c1dfb66579eddd8a280b9f7316");
        AssertKeyStreamEqual(keyStream, 2048, "ddd27820550126698efaadc64b64f66e");
        AssertKeyStreamEqual(keyStream, 3056, "f08f2e66d28ed143f3a237cf9de73559");
        AssertKeyStreamEqual(keyStream, 3072, "9ea36c525531b880ba124334f57b0b70");
        AssertKeyStreamEqual(keyStream, 4080, "d5a39e3dfcc50280bac4a6b5aa0dca7d");
        AssertKeyStreamEqual(keyStream, 4096, "370b1c1fe655916d97fd0d47ca1d72b8");

        ArrayPool<byte>.Shared.Return(keyStream);
    }

    private static void AssertKeyStreamEqual(Span<byte> keyStream, int offset, string expectedKeyStream)
        => Assert.Equal(expectedKeyStream, Convert.ToHexString(keyStream.Slice(offset, 16)), ignoreCase: true);
}
