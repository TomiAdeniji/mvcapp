﻿using System;
using System.Security.Cryptography;
using System.Text;

namespace Qbicles.Doc.Helper
{
    internal static class EntityTagGenerator
    {
        internal static string GenerateModificationDateAndLengthXOREntityTag(DateTime modificationDate, long length)
        {
            long enityTagHash = modificationDate.ToFileTime() ^ length;
            return Convert.ToString(enityTagHash, 16);
        }

        [Obsolete("This method is not FIPS compliant.")]
        internal static string GenerateNameAndModificationDateMD5EntityTag(string name, DateTime modificationDate)
        {
            byte[] entityTagBytes = Encoding.ASCII.GetBytes(String.Format("{0}|{1}", name, modificationDate));
            return Convert.ToBase64String(new MD5CryptoServiceProvider().ComputeHash(entityTagBytes));
        }
    }
}