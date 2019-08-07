﻿using Bit.App.Migration.Models;
using Bit.Core.Enums;
using PCLCrypto;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Bit.App.Migration
{
    public static class Crypto
    {
        public static CipherString AesCbcEncrypt(byte[] plainBytes, SymmetricCryptoKey key)
        {
            var parts = AesCbcEncryptToParts(plainBytes, key);
            return new CipherString(parts.Item1, Convert.ToBase64String(parts.Item2),
                Convert.ToBase64String(parts.Item4), parts.Item3 != null ? Convert.ToBase64String(parts.Item3) : null);
        }

        public static byte[] AesCbcEncryptToBytes(byte[] plainBytes, SymmetricCryptoKey key)
        {
            var parts = AesCbcEncryptToParts(plainBytes, key);
            var macLength = parts.Item3?.Length ?? 0;

            var encBytes = new byte[1 + parts.Item2.Length + macLength + parts.Item4.Length];
            encBytes[0] = (byte)parts.Item1;
            parts.Item2.CopyTo(encBytes, 1);
            if(parts.Item3 != null)
            {
                parts.Item3.CopyTo(encBytes, 1 + parts.Item2.Length);
            }
            parts.Item4.CopyTo(encBytes, 1 + parts.Item2.Length + macLength);
            return encBytes;
        }

        private static Tuple<EncryptionType, byte[], byte[], byte[]> AesCbcEncryptToParts(byte[] plainBytes,
            SymmetricCryptoKey key)
        {
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if(plainBytes == null)
            {
                throw new ArgumentNullException(nameof(plainBytes));
            }

            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var cryptoKey = provider.CreateSymmetricKey(key.EncKey);
            var iv = RandomBytes(provider.BlockLength);
            var ct = WinRTCrypto.CryptographicEngine.Encrypt(cryptoKey, plainBytes, iv);
            var mac = key.MacKey != null ? ComputeMac(ct, iv, key.MacKey) : null;

            return new Tuple<EncryptionType, byte[], byte[], byte[]>(key.EncryptionType, iv, mac, ct);
        }

        public static byte[] AesCbcDecrypt(CipherString encyptedValue, SymmetricCryptoKey key)
        {
            if(encyptedValue == null)
            {
                throw new ArgumentNullException(nameof(encyptedValue));
            }

            return AesCbcDecrypt(encyptedValue.EncryptionType, encyptedValue.CipherTextBytes,
                encyptedValue.InitializationVectorBytes, encyptedValue.MacBytes, key);
        }

        public static byte[] AesCbcDecrypt(EncryptionType type, byte[] ct, byte[] iv, byte[] mac,
            SymmetricCryptoKey key)
        {
            if(key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }

            if(ct == null)
            {
                throw new ArgumentNullException(nameof(ct));
            }

            if(iv == null)
            {
                throw new ArgumentNullException(nameof(iv));
            }

            if(key.MacKey != null && mac == null)
            {
                throw new ArgumentNullException(nameof(mac));
            }

            if(key.EncryptionType != type)
            {
                throw new InvalidOperationException(nameof(type));
            }

            if(key.MacKey != null && mac != null)
            {
                var computedMacBytes = ComputeMac(ct, iv, key.MacKey);
                if(!MacsEqual(computedMacBytes, mac))
                {
                    throw new InvalidOperationException("MAC failed.");
                }
            }

            var provider = WinRTCrypto.SymmetricKeyAlgorithmProvider.OpenAlgorithm(SymmetricAlgorithm.AesCbcPkcs7);
            var cryptoKey = provider.CreateSymmetricKey(key.EncKey);
            var decryptedBytes = WinRTCrypto.CryptographicEngine.Decrypt(cryptoKey, ct, iv);
            return decryptedBytes;
        }

        public static byte[] RandomBytes(int length)
        {
            return WinRTCrypto.CryptographicBuffer.GenerateRandom(length);
        }

        public static byte[] ComputeMac(byte[] ctBytes, byte[] ivBytes, byte[] macKey)
        {
            if(ctBytes == null)
            {
                throw new ArgumentNullException(nameof(ctBytes));
            }

            if(ivBytes == null)
            {
                throw new ArgumentNullException(nameof(ivBytes));
            }

            return ComputeMac(ivBytes.Concat(ctBytes), macKey);
        }

        public static byte[] ComputeMac(IEnumerable<byte> dataBytes, byte[] macKey)
        {
            if(macKey == null)
            {
                throw new ArgumentNullException(nameof(macKey));
            }

            if(dataBytes == null)
            {
                throw new ArgumentNullException(nameof(dataBytes));
            }

            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha256);
            var hasher = algorithm.CreateHash(macKey);
            hasher.Append(dataBytes.ToArray());
            var mac = hasher.GetValueAndReset();
            return mac;
        }

        // Safely compare two MACs in a way that protects against timing attacks (Double HMAC Verification).
        // ref: https://www.nccgroup.trust/us/about-us/newsroom-and-events/blog/2011/february/double-hmac-verification/
        // ref: https://paragonie.com/blog/2015/11/preventing-timing-attacks-on-string-comparison-with-double-hmac-strategy
        public static bool MacsEqual(byte[] mac1, byte[] mac2)
        {
            var algorithm = WinRTCrypto.MacAlgorithmProvider.OpenAlgorithm(MacAlgorithm.HmacSha256);
            var hasher = algorithm.CreateHash(RandomBytes(32));

            hasher.Append(mac1);
            mac1 = hasher.GetValueAndReset();

            hasher.Append(mac2);
            mac2 = hasher.GetValueAndReset();

            if(mac1.Length != mac2.Length)
            {
                return false;
            }

            for(int i = 0; i < mac2.Length; i++)
            {
                if(mac1[i] != mac2[i])
                {
                    return false;
                }
            }

            return true;
        }

        // ref: https://tools.ietf.org/html/rfc5869
        public static byte[] HkdfExpand(byte[] prk, byte[] info, int size)
        {
            var hashLen = 32; // sha256
            var okm = new byte[size];
            var previousT = new byte[0];
            var n = (int)Math.Ceiling((double)size / hashLen);
            for(int i = 0; i < n; i++)
            {
                var t = new byte[previousT.Length + info.Length + 1];
                previousT.CopyTo(t, 0);
                info.CopyTo(t, previousT.Length);
                t[t.Length - 1] = (byte)(i + 1);
                previousT = ComputeMac(t, prk);
                previousT.CopyTo(okm, i * hashLen);
            }
            return okm;
        }
    }
}
