using System.Text;
using System.Web;
using System.Web.Security;
using System.Security.Cryptography;
using System;

namespace Qbicles.Models
{
    public static class EncryptionService
    {
        private const string CRYPTOGRAPHY = "2354CA87-8A03-4720-AFB4-D3839B05E1B7";
        private const string PURPOSE = "3573611B-D763-4D97-A91D-94D3E9D60D02";

        /// <summary>
        /// Encrypt the given string using the specified key.
        /// </summary>
        /// <param name="str2Encrypt">The string to be encrypted.</param>
        /// <returns>The encrypted string.</returns>
        public static string Encrypt(this object str2Encrypt)
        {
            try
            {
                if (string.IsNullOrEmpty(str2Encrypt.ToString()))
                    return string.Empty;

                byte[] stream = Encoding.Unicode.GetBytes(str2Encrypt.ToString().CryptographyEncrypt());
                byte[] encodedValue = MachineKey.Protect(stream, PURPOSE);
                return HttpServerUtility.UrlTokenEncode(encodedValue);
            }
            catch
            {
                return str2Encrypt.ToString();
            }
        }

        /// <summary>
        /// Decrypt the given string using the specified key.
        /// </summary>
        /// <param name="str2Decrypted">The string to be decrypted.</param>
        /// <returns>The decrypted string.</returns>
        public static string Decrypt(this string str2Decrypted)
        {
            try
            {
                if (string.IsNullOrEmpty(str2Decrypted))
                    return string.Empty;

                byte[] stream = HttpServerUtility.UrlTokenDecode(str2Decrypted);
                byte[] decodedValue = MachineKey.Unprotect(stream, PURPOSE);
                return Encoding.Unicode.GetString(decodedValue).CryptographyDecrypt();
            }
            catch
            {
                return "0";
            }
        }

        public static int Decrypt2Int(this string str2Decrypted)
        {
            try
            {
                if (string.IsNullOrEmpty(str2Decrypted))
                    return 0;

                byte[] stream = HttpServerUtility.UrlTokenDecode(str2Decrypted);
                byte[] decodedValue = MachineKey.Unprotect(stream, PURPOSE);
                return int.Parse(Encoding.Unicode.GetString(decodedValue).CryptographyDecrypt());
            }
            catch
            {
                return 0;
            }
        }

        private static string CryptographyEncrypt(this string str2Encrypt)
        {
            try
            {
                byte[] byteHash, byteBuff;

                var objHashMD5 = new MD5CryptoServiceProvider();
                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(CRYPTOGRAPHY));
                objHashMD5 = null;

                var objDESCrypto = new TripleDESCryptoServiceProvider
                {
                    Key = byteHash,
                    Mode = CipherMode.ECB //CBC, CFB
                };

                byteBuff = Encoding.ASCII.GetBytes(str2Encrypt);
                return Convert.ToBase64String(objDESCrypto.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            catch
            {
                return str2Encrypt;
            }
        }

        private static string CryptographyDecrypt(this string str2Decrypted)
        {
            try
            {
                byte[] byteHash, byteBuff;

                var objHashMD5 = new MD5CryptoServiceProvider();
                byteHash = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(CRYPTOGRAPHY));
                objHashMD5 = null;

                var objDESCrypto = new TripleDESCryptoServiceProvider
                {
                    Key = byteHash,
                    Mode = CipherMode.ECB //CBC, CFB
                };

                byteBuff = Convert.FromBase64String(str2Decrypted);
                string strDecryptedRef = Encoding.ASCII.GetString(objDESCrypto.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                objDESCrypto = null;

                return strDecryptedRef;
            }
            catch
            {
                return str2Decrypted;
            }

        }

        public static string EncryptCore(this object strToEncrypt)
        {
            try
            {
                TripleDESCryptoServiceProvider objDESCrypto = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;

                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(CRYPTOGRAPHY));
                objHashMD5 = null;
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB; //CBC, CFB

                byteBuff = ASCIIEncoding.ASCII.GetBytes(strToEncrypt.ToString());
                return Convert.ToBase64String(objDESCrypto.CreateEncryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
            }
            catch
            {
                return strToEncrypt.ToString();
            }
        }
        public static string DecryptCore(this object strDecrypted)
        {
            try
            {
                TripleDESCryptoServiceProvider objDESCrypto = new TripleDESCryptoServiceProvider();
                MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();

                byte[] byteHash, byteBuff;

                byteHash = objHashMD5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(CRYPTOGRAPHY));
                objHashMD5 = null;
                objDESCrypto.Key = byteHash;
                objDESCrypto.Mode = CipherMode.ECB; //CBC, CFB

                byteBuff = Convert.FromBase64String(strDecrypted.ToString());
                string strDecryptedRef = ASCIIEncoding.ASCII.GetString(objDESCrypto.CreateDecryptor().TransformFinalBlock(byteBuff, 0, byteBuff.Length));
                objDESCrypto = null;

                return strDecryptedRef;
            }
            catch
            {
                return strDecrypted.ToString();
            }

        }
    }
}
