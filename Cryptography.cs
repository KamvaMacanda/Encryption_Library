using System.Security.Cryptography; 

namespace Encryption_Library
{
    public class Cryptography
    {
        public static string Encrption(string plainText, byte[] key, byte[] iv)
        {
            using (DES des = DES.Create())
            {
                des.Key = key;
                des.IV = iv;

                ICryptoTransform encryptor = des.CreateEncryptor(des.Key, des.IV);

                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }



        public static string Decryprt(string cipherText, byte[] key, byte[] iv)
        {
            using (DES des = DES.Create())
            {
                des.Key = key;
                des.IV = iv;

                ICryptoTransform decryptor = des.CreateDecryptor(des.Key, des.IV);

                byte[] buffer = Convert.FromBase64String(cipherText);

                using (MemoryStream ms = new MemoryStream(buffer)) // fixed: pass buffer in
                {
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader sr = new StreamReader(cs))
                        {
                            return sr.ReadToEnd();
                        }
                    }
                }
            }
        }
        public static (byte[] key, byte[] iv) GenerateKeyAndIV()
        {
            using (DES des = DES.Create())
            {
                des.GenerateKey();
                des.GenerateIV();

                return (des.Key, des.IV);
            }
        }






    }
}