using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace MakeAuto
{
    public static class MyAesMan
    {
        static MyAesMan()
        {
            myAes = new AesManaged();
            InitKey();
        }
        
        private static void InitKey()
        {
            if (key == null || iv == null)
            {
                byte[] key_t = Encoding.UTF8.GetBytes(G_KEY);
                // 做一下MD5哈希，作为 key，实际可以直接使用，HASH的值太小了
                SHA512Managed provider_SHA = new SHA512Managed();
                byte[] byte_pwdSHA = provider_SHA.ComputeHash(key_t);

                key = new byte[32];
                Array.Copy(byte_pwdSHA, key, 32);
                iv = new byte[16];
                Array.Copy(Encoding.UTF8.GetBytes(G_IV), key, 16);
                myAes.Key = key;
                myAes.IV = iv;
            }
        }

        public static string DecPass(string encpass)
        {
            string plainText = DecryptStringFromBytes_Aes(Convert.FromBase64String(encpass), myAes.Key, myAes.IV);
            return plainText;

        }

        public static string EncPass(string original)
        {
            byte[] cipherText = EncryptStringToBytes_Aes(original, myAes.Key, myAes.IV);
            return Convert.ToBase64String(cipherText);
        }

        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null || plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");
            byte[] encrypted;
            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {

                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }


            // Return the encrypted bytes from the memory stream.
            return encrypted;

        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null || cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null || Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("Key");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an AesManaged object
            // with the specified key and IV.
            using (AesManaged aesAlg = new AesManaged())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;

                // Create a decrytor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {

                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }

            }

            return plaintext;
        }

        private static byte[] key;
        private static byte[] iv;
        private static AesManaged myAes;
        const string G_KEY = "12345678xyze133ttyyfahdfajyeafjaldjzdvjldjfdlajafljdfnvladnnvnswdehhe329789oweuw";
        const string G_IV = "87654321xywer123488ewfjaldjf9u238r2fsjfalsfj;al;fjaoewurwhvfadlfhadfjalkfweourw";
    }
}