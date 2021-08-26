using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace Keroosha.GraphQL.Web.Auth
{
    public static class PasswordToolkit
    {
        public static string EncodeSshaPassword(string plainTextPassword)
        {
            var saltBytes = GenerateSalt(4);
            var plainTextBytes = Encoding.ASCII.GetBytes(plainTextPassword);
            var bytes = ComputeSaltedSha512(plainTextBytes, saltBytes);
            return "{SSHA512}" + Convert.ToBase64String(bytes);
        }

        public static bool CheckPassword(string saltedPassword, string password)
        {
            if (!saltedPassword.StartsWith("{SSHA512}"))
                return false;
            saltedPassword = saltedPassword.Substring(9);
            var bytes = Convert.FromBase64String(saltedPassword);
            return CheckSaltedSha512Password(bytes, password);
        }

        public static bool CheckSaltedSha512Password(byte[] saltedPassword, string password)
        {
            var salt = new byte[saltedPassword.Length - 64];
            Array.Copy(saltedPassword, 64, salt, 0, salt.Length);
            var computed = ComputeSaltedSha512(Encoding.ASCII.GetBytes(password), salt);
            return saltedPassword.SequenceEqual(computed);
        }

        private static byte[] ComputeSaltedSha512(byte[] plainTextBytes, byte[] saltBytes)
        {
            using var algorithm = SHA512.Create();
            var plainTextWithSaltBytes = AppendByteArray(plainTextBytes, saltBytes);
            var saltedSha1Bytes = algorithm.ComputeHash(plainTextWithSaltBytes);
            return AppendByteArray(saltedSha1Bytes, saltBytes);
        }

        private static byte[] GenerateSalt(int saltSize)
        {
            var rng = new RNGCryptoServiceProvider();
            var buff = new byte[saltSize];
            rng.GetBytes(buff);
            return buff;
        }

        private static byte[] AppendByteArray(byte[] byteArray1, byte[] byteArray2)
        {
            var byteArrayResult =
                new byte[byteArray1.Length + byteArray2.Length];

            for (var i = 0; i < byteArray1.Length; i++)
                byteArrayResult[i] = byteArray1[i];
            for (var i = 0; i < byteArray2.Length; i++)
                byteArrayResult[byteArray1.Length + i] = byteArray2[i];

            return byteArrayResult;
        }
    }
}