using System.Collections.Generic;
using System.Data.Entity.Migrations.Model;
using System.Data.Entity.Migrations.Sql;
using System.Security.Cryptography;
using System.Text;
using MySql.Data.EntityFramework;

namespace Qbicles.Web.Migrations
{

    class SqlGenerator : MySqlMigrationSqlGenerator
    {
        public override IEnumerable<MigrationStatement> Generate(IEnumerable<MigrationOperation> migrationOperations, string providerManifestToken)
        {
            var res = base.Generate(migrationOperations, providerManifestToken);
            foreach (var ms in res)
            {
                ms.Sql = ms.Sql.Replace("dbo.", "");
                ms.Sql = ms.Sql + ";";
            }
            return res;
        }

        protected override MigrationStatement Generate(AddForeignKeyOperation addForeignKeyOperation)
        {
            addForeignKeyOperation.PrincipalTable = addForeignKeyOperation.PrincipalTable.Replace("dbo.", "");
            addForeignKeyOperation.DependentTable = addForeignKeyOperation.DependentTable.Replace("dbo.", "");
            var foreignKeyName = $"FK_{addForeignKeyOperation.Name.Md5Hash()}";
            addForeignKeyOperation.Name = foreignKeyName;
            MigrationStatement ms = base.Generate(addForeignKeyOperation);
            return ms;
        }
        protected override MigrationStatement Generate(DropForeignKeyOperation dropForeignKeyOperation)
        {
            try
            {
                dropForeignKeyOperation.PrincipalTable = dropForeignKeyOperation.PrincipalTable.Replace("dbo.", "");
                dropForeignKeyOperation.DependentTable = dropForeignKeyOperation.DependentTable.Replace("dbo.", "");
                var foreignKeyName = $"FK_{dropForeignKeyOperation.Name.Md5Hash()}";
                dropForeignKeyOperation.Name = foreignKeyName;

                MigrationStatement ms = base.Generate(dropForeignKeyOperation);
                return ms;
            }
            catch
            {
                MigrationStatement msFkNameOnly = base.Generate(dropForeignKeyOperation);
                return msFkNameOnly;
            }

        }


    }

    public static class EncryptionMd5Hash
    {

        public static string Md5Hash(this string text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();

            //compute hash from the bytes of text  
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));

            //get hash result after compute it  
            byte[] result = md5.Hash;

            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(result[i].ToString("x2"));
            }

            return strBuilder.ToString();
        }
    }
    }
