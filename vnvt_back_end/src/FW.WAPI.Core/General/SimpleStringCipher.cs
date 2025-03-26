using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace FW.WAPI.Core.General
{
    /// <summary>
    /// Can be used to simply encrypt/decrypt texts.
    /// </summary>
    public class SimpleStringCipher
    {
        public static SimpleStringCipher Instance { get; }

        /// <summary>
        /// This constant string is used as a "salt" value for the PasswordDeriveBytes function calls.
        /// This size of the IV (in bytes) must = (keysize / 8).  Default keysize is 256, so the IV must be
        /// 32 bytes long.  Using a 16 character string here gives us 32 bytes when converted to a byte array.
        /// </summary>
        public byte[] InitVectorBytes;

        /// <summary>
        /// Default password to encrypt/decrypt texts.
        /// It's recommented to set to another value for security.
        /// Default value: "gsKnGZ041HLL4IM8"
        /// </summary>
        public static string DefaultPassPhrase { get; set; }

        /// <summary>
        /// Default value: Encoding.ASCII.GetBytes("jkE49230Tf093b42")
        /// </summary>
        public static byte[] DefaultInitVectorBytes { get; set; }

        /// <summary>
        /// Default value: Encoding.ASCII.GetBytes("hgt!16kl")
        /// </summary>
        public static byte[] DefaultSalt { get; set; }

        /// <summary>
        /// This constant is used to determine the keysize of the encryption algorithm.
        /// </summary>
        public const int Keysize = 256;

        static SimpleStringCipher()
        {
            DefaultPassPhrase = "gsKnGZ041HLL4IM8";
            DefaultInitVectorBytes = Encoding.ASCII.GetBytes("jkE49230Tf093b42");
            DefaultSalt = Encoding.ASCII.GetBytes("hgt!16kl");
            Instance = new SimpleStringCipher();
        }

        public SimpleStringCipher()
        {
            InitVectorBytes = DefaultInitVectorBytes;
        }

        public string Encrypt(string plainText, string passPhrase = null, byte[] salt = null)
        {
            if (plainText == null)
            {
                return null;
            }

            if (passPhrase == null)
            {
                passPhrase = DefaultPassPhrase;
            }

            if (salt == null)
            {
                salt = DefaultSalt;
            }

            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, salt))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = Aes.Create())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, InitVectorBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();
                                var cipherTextBytes = memoryStream.ToArray();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public string Decrypt(string cipherText, string passPhrase = null, byte[] salt = null)
        {
            if (string.IsNullOrEmpty(cipherText))
            {
                return null;
            }

            if (passPhrase == null)
            {
                passPhrase = DefaultPassPhrase;
            }

            if (salt == null)
            {
                salt = DefaultSalt;
            }

            var cipherTextBytes = Convert.FromBase64String(cipherText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, salt))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = Aes.Create())
                {
                    symmetricKey.Mode = CipherMode.CBC;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, InitVectorBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        //public static dynamic EncryptStringAES(string cipherText, string key, string ivPhase)
        //{
        //    var keybytes = Encoding.UTF8.GetBytes(key);
        //    var iv = Encoding.UTF8.GetBytes(ivPhase);

        //    var encrypted = Convert.FromBase64String(cipherText);
        //    var decriptedFromString = DecryptStringFromBytes(encrypted, keybytes, iv);
        //    return decriptedFromString;
        //    //return string.Format(decriptedFromJavascript);
        //    //return JsonConvert.SerializeObject(decriptedFromJavascript);
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="cipherText"></param>
        /// <param name="key"></param>
        /// <param name="ivPhase"></param>
        /// <returns></returns>
        public static dynamic DecryptStringAES(string cipherText, string key, string ivPhase)
        {
            var keybytes = Encoding.UTF8.GetBytes(key);
            var iv = Encoding.UTF8.GetBytes(ivPhase);

            var encrypted = Convert.FromBase64String(cipherText);
            var decriptedFromString = DecryptStringFromBytes(encrypted, keybytes, iv);
            return decriptedFromString;
            //return string.Format(decriptedFromJavascript);
            //return JsonConvert.SerializeObject(decriptedFromJavascript);
        }

        private static string DecryptStringFromBytes(byte[] cipherText, byte[] key, byte[] iv)
        {
            // Check arguments.  
            if (cipherText == null || cipherText.Length <= 0)
            {
                throw new ArgumentNullException("cipherText");
            }
            if (key == null || key.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }
            if (iv == null || iv.Length <= 0)
            {
                throw new ArgumentNullException("key");
            }

            // Declare the string used to hold  
            // the decrypted text.  
            string plaintext = null;

            // Create an RijndaelManaged object  
            // with the specified key and IV.  
            using (var rijAlg = new RijndaelManaged())
            {
                //Settings  
                rijAlg.Mode = CipherMode.CBC;
                rijAlg.Padding = PaddingMode.PKCS7;
                rijAlg.FeedbackSize = 128;

                rijAlg.Key = key;
                rijAlg.IV = iv;

                // Create a decrytor to perform the stream transform.  
                var decryptor = rijAlg.CreateDecryptor(rijAlg.Key, rijAlg.IV);

                try
                {
                    // Create the streams used for decryption.  
                    using (var msDecrypt = new MemoryStream(cipherText))
                    {
                        using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {

                            using (var srDecrypt = new StreamReader(csDecrypt))
                            {
                                // Read the decrypted bytes from the decrypting stream  
                                // and place them in a string.  
                                plaintext = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                catch
                {
                    plaintext = "keyError";
                }
            }

            return plaintext;
        }
    }
}