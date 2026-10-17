using System;
using System.Numerics;
using System.Security.Cryptography;
using UnityEngine;

public class RSACrypt
{
    private RSACryptoServiceProvider _rsaProvider;
    private RSA _rsa;
    private RSAParameters _rsaParams;
    private byte[] _modulus;
    // hardcoded modulus
    private byte[] _exponent = new byte[] { 1, 0, 1 };


    public RSACrypt(byte[] exponent, bool needUnscramble) {
        if(needUnscramble) {
            UnscrambledRSAKey(exponent);
        }

        _modulus = (byte[])exponent.Clone();
        InitRSACrypt(exponent);
    }

    private void InitRSACrypt(byte[] modulus) {
        _rsaParams = new RSAParameters {
            Modulus = modulus,
            Exponent = _exponent
        };

        _rsaProvider = new RSACryptoServiceProvider();
        _rsaProvider.ImportParameters(_rsaParams);
        _rsa = RSA.Create();
        _rsa.ImportParameters(_rsaParams);
    }

    public byte[] DecryptRSABlock(byte[] encryptedData) {
        try {
            // Encrypt without padding
            return _rsaProvider.Decrypt(encryptedData, false);
        } catch (Exception ex) {
            // Handle other unexpected errors
            Debug.LogError($"Unexpected error during RSA encryption: {ex.Message}");
            return null;
        }
    }

    public byte[] EncryptRSANoPadding(byte[] block) {
        try {
            byte[] mLe = new byte[block.Length + 1];
            for (int i = 0; i < block.Length; i++) mLe[i] = block[block.Length - 1 - i];
            byte[] nLe = new byte[_modulus.Length + 1];
            for (int i = 0; i < _modulus.Length; i++) nLe[i] = _modulus[_modulus.Length - 1 - i];
            BigInteger m = new BigInteger(mLe);
            BigInteger n = new BigInteger(nLe);
            BigInteger c = BigInteger.ModPow(m, 65537, n);
            byte[] cLe = c.ToByteArray();
            byte[] res = new byte[128];
            for (int i = 0; i < Math.Min(128, cLe.Length); i++) res[127 - i] = cLe[i];
            return res;
        } catch (Exception ex) {
            Debug.LogError($"Unexpected error during raw RSA encryption: {ex.Message}");
            return null;
        }
    }

    public byte[] EncryptRSAPskc1(byte[] block) {
        try {
            byte[] encryptedBlock = _rsa.Encrypt(block, RSAEncryptionPadding.Pkcs1);
            return encryptedBlock;        
        } catch (CryptographicException ex) {
            // Handle cryptographic errors
            Debug.LogError($"RSA encryption error: {ex.Message}");
        } catch (Exception ex) {
            // Handle other unexpected errors
            Debug.LogError($"Unexpected error during RSA encryption: {ex.Message}");
        }

        return null;
    }

    public void UnscrambledRSAKey(byte[] rsaKey) {
        Debug.Log($"Scrambled RSA: {StringUtils.ByteArrayToString(rsaKey)}");

        // step 4 : xor last 0x40 bytes with  first 0x40 bytes
        for (int i = 0; i < 0x40; i++) {
            rsaKey[0x40 + i] = (byte)(rsaKey[0x40 + i] ^ rsaKey[i]);
        }
        // step 3 : xor bytes 0x0d-0x10 with bytes 0x34-0x38
        for (int i = 0; i < 4; i++) {
            rsaKey[0x0d + i] = (byte)(rsaKey[0x0d + i] ^ rsaKey[0x34 + i]);
        }
        // step 2 : xor first 0x40 bytes with  last 0x40 bytes 
        for (int i = 0; i < 0x40; i++) {
            rsaKey[i] = (byte)(rsaKey[i] ^ rsaKey[0x40 + i]);
        }
        // step 1 : 0x4d-0x50 <-> 0x00-0x04
        for (int i = 0; i < 4; i++) {
            byte temp = rsaKey[0x00 + i];
            rsaKey[0x00 + i] = rsaKey[0x4d + i];
            rsaKey[0x4d + i] = temp;
        }

        Debug.Log($"Unscrambled RSA {rsaKey.Length} : {StringUtils.ByteArrayToString(rsaKey)}");
    }
}
