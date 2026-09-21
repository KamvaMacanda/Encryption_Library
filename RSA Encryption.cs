using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Encryption_Library
{
    public class RSA_Encryption
    {

        public static RSA GenerateKeyPair()
        {
            return RSA.Create(2048);
        }


        public static string GetPublicKey(RSA rsa)
        {
            return rsa.ToXmlString(false);
        }


        public static string EncryptionWithRSA(byte[] desKey, string publicKeyXml)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.FromXmlString(publicKeyXml);
                byte[] encrypted = rsa.Encrypt(desKey, RSAEncryptionPadding.OaepSHA256);
                return Convert.ToBase64String(encrypted);

            }
        }

        // Decrypting key RSA 
        public static byte[] DecryptWithRSA(string encryptedKeyBase64, RSA rsa)
        {
            byte[] encrypted = Convert.FromBase64String(encryptedKeyBase64);
            return rsa.Decrypt(encrypted, RSAEncryptionPadding.OaepSHA256);
        }

    }
}


  