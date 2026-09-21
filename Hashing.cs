using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using BCrypt.Net;


namespace Encryption_Library
{
    public class Hashing
    {

        //Hashing the texts to add intergirty  
        // Create HMAC (Hash-based Message Authentication Code)  

        public static string CreateHMAC(string message, byte[] key)
        {

            using (HMACSHA256 hmac = new HMACSHA256(key))
            {
                byte[] hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
                return Convert.ToBase64String(hash);

            }
        }


        //Receiver recomputes it and compares 
        public static bool VerifyHmac(string message, byte[] key, string receivedHmac)
        {
            string expected = CreateHMAC(message, key);
            return expected == receivedHmac;
        }
    }
}