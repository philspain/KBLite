using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace KBDocumentConverterService.Converters
{
    public static class EncryptStrings
    {
        static readonly string _passwordText = "Th!s!sNotTh3KBConv3rt3rsPassword";
        static readonly string _saltText = "T4isIsDef0N0tTh3KBConvertersSALT";
        static readonly byte[] _PASSWORD = Encoding.UTF8.GetBytes(_passwordText);
        static readonly byte[] _SALT = Encoding.UTF8.GetBytes(_saltText);

        public static SymmetricAlgorithm InitSymmetric(SymmetricAlgorithm algorithm, string password, int keyBitLength)
        {
            var salt = new byte[] { 1, 2, 23, 234, 37, 48, 134, 63, 248, 4 };

            const int Iterations = 234;
            using (var rfc2898DeriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                if (!algorithm.ValidKeySize(keyBitLength))
                    throw new InvalidOperationException("Invalid size key");

                algorithm.Key = rfc2898DeriveBytes.GetBytes(keyBitLength / 8);
                algorithm.IV = rfc2898DeriveBytes.GetBytes(algorithm.BlockSize / 8);
                algorithm.Padding = PaddingMode.PKCS7;
                return algorithm;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="textToConvert"></param>
        /// <returns></returns>
        public static string EncryptToMD5String(string textToConvert)
        {
            StringBuilder stringBuilder = new StringBuilder(textToConvert.Length * 2);

            using (MD5 md5Creator = MD5.Create())
            {
                byte[] pathHash = md5Creator.ComputeHash(Encoding.UTF8.GetBytes(textToConvert));

                for (int i = 0; i < pathHash.Length; i++)
                {
                    stringBuilder.Append(pathHash[i].ToString("x2"));
                }
            }

            return stringBuilder.ToString();
        }

        /// <summary>
        /// Encrypts the provided string to an AES encrypted string using the currently 
        /// defined AES KEy.
        /// </summary>
        /// <param name="textToConvert">string</param>
        /// <returns>string</returns>
        public static string EncryptToAESString(string plainText)
        {
            byte[] encrypted;

            using (Aes aesCreator = Aes.Create())
            {
                InitSymmetric(aesCreator, _passwordText, 256);

                using (MemoryStream mStream = new MemoryStream())
                {
                    ICryptoTransform encryptor = aesCreator.CreateEncryptor(aesCreator.Key, aesCreator.IV);

                    using (CryptoStream cStream = new CryptoStream(mStream, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter sWriter = new StreamWriter(cStream))
                        {
                            sWriter.Write(plainText);
                        }

                        encrypted = mStream.ToArray();
                    }
                }
            }

            StringBuilder stringBuilder = new StringBuilder(encrypted.Length * 2);

            foreach (byte b in encrypted)
            {
                stringBuilder.Append(b.ToString("x2"));
            }

            return stringBuilder.ToString().ToUpper();
        }

        /// <summary>
        /// Takes an AES encrypted string and attempts to decrypt it using the currently
        /// defined AES Key.
        /// </summary>
        /// <param name="stringToDecrupt">string</param>
        /// <returns>string</returns>
        public static string DecryptAESString(string plainText)
        {
            string path = String.Empty;

            try
            {
                using (Aes aesCreator = Aes.Create())
                {
                    InitSymmetric(aesCreator, _passwordText, 256);
                    byte[] textBytes = HexStringToByteArray(plainText);

                    using (MemoryStream mStream = new MemoryStream(textBytes))
                    {
                        ICryptoTransform decryptor = aesCreator.CreateDecryptor(aesCreator.Key, aesCreator.IV);

                        using (CryptoStream cStream = new CryptoStream(mStream, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader sReader = new StreamReader(cStream))
                            {
                                path = sReader.ReadToEnd();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StreamWriter sw = File.AppendText("C:\\Service.txt");
                string mess = ex.Message + "\n" + ex.InnerException + "\n" + ex.Source + "\n" + ex.StackTrace;
                sw.WriteLine(mess);
                sw.Flush();
                sw.Close();
            }

            return path;
        }

        static byte[] HexStringToByteArray(string hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }
    }
}
