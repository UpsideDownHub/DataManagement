using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DataManagement
{
    class Program
    {
        private const int Keysize = 256;
        private const int DerivationIterations = 1000;
        private const string Key = "FelipeMae";
        public static string path = Path.Combine("C:\\Desenvolvimento\\github.com-UpsideDownHub\\ProjectVikins\\ProjectVikins\\teste");

        //trocar
        //1;;true;;0;0;1;3;3;4;2;5
        static PropertyInfo[] classeInfo = typeof(Player).GetProperties();
        static string classKey = classeInfo[0].Name;


        static void Main(string[] args)
        {
            Console.Write("Arquivo:");
            path = Path.Combine(path, Console.ReadLine() + ".txt");


            string data;
            int i;

            while (true)
            {
                i = 0;
                data = "";

                var action = Console.ReadLine();
                if (action == "select")
                {
                    Console.WriteLine(GetCurrentData());
                }
                else if (action.Contains("insert"))
                {
                    string newData = "";
                    data = GetCurrentData();
                    StreamWriter sWriter = new StreamWriter(path);
                    try
                    {
                        var _data = action.Replace("insert ", "").Split(';');
                        foreach (var variavel in classeInfo)
                        {
                            if (!string.IsNullOrWhiteSpace(_data[i]))
                                newData += variavel.Name + "=" + _data[i] + ";";
                            i++;
                        }
                        data = CorrectFormat(data);
                        var cryptoData = Encrypt(data + newData + "\n", Key);
                        sWriter.WriteLine(cryptoData);
                    }
                    finally
                    {
                        sWriter.Close();
                    }
                }
                else if (action.Contains("delete"))
                {
                    data = GetCurrentData();
                    var _data = action.Replace("delete ", "");
                    if (!data.Contains(classKey + "=" + _data))
                        Console.WriteLine(classKey + " não encontrado!");
                    else
                    {
                        StreamWriter sWriter = new StreamWriter(path);

                        try
                        {
                            int pFrom = data.IndexOf(classKey + "=" + _data);
                            var partData = data.Substring(pFrom, data.Length - pFrom);
                            int _pTo = partData.IndexOf("\n");
                            var a = partData.Substring(0, _pTo);
                            var b = data.Remove(pFrom, a.Length);

                            b = CorrectFormat(b);
                            var cryptoData = Encrypt(b, Key);
                            sWriter.WriteLine(cryptoData);
                        }
                        finally
                        {
                            sWriter.Close();
                        }
                    }
                }
                else if (action.Contains("update"))
                {
                    data = GetCurrentData();
                    var _data = action.Replace("update ", "").Split('~')[0];
                    if (!data.Contains(classKey + "=" + _data))
                        Console.WriteLine(classKey + " não encontrado!");
                    else
                    {
                        StreamWriter sWriter = new StreamWriter(path);

                        try
                        {
                            int pFrom = data.IndexOf(classKey + "=" + _data);
                            var partData = data.Substring(pFrom, data.Length - pFrom);
                            int _pTo = partData.IndexOf("\n");
                            var a = partData.Substring(0, _pTo);
                            var b = data.Remove(pFrom, a.Length);

                            string newData = "";
                            var __data = action.Split('~')[1].Split(';');
                            foreach (var variavel in classeInfo)
                            {
                                if (!string.IsNullOrWhiteSpace(__data[i]))
                                    newData += variavel.Name + "=" + __data[i] + ";";
                                i++;
                            }

                            b = CorrectFormat(b + newData);
                            var cryptoData = Encrypt(b, Key);
                            sWriter.WriteLine(cryptoData);
                        }
                        finally
                        {
                            sWriter.Close();
                        }
                    }
                }
                else if (action == "clear")
                {
                    StreamWriter sWriter = new StreamWriter(path);
                    sWriter.Write("");
                    sWriter.Close();
                }
                else if (action == "stop" || action == "close")
                    break;
                else
                {
                    Console.WriteLine("Comando Inválido");
                }
            }
        }

        public static string GetCurrentData()
        {
            string currentData = "";
            StreamReader sReader = new StreamReader(path);
            while (true)
            {
                var a = sReader.ReadLine();
                if (a == null)
                    break;
                currentData += Decrypt(a, Key);
                if (a.Contains("\0"))
                    break;
            }
            sReader.Close();
            return currentData;
        }

        public static string CorrectFormat(string data)
        {
            while (true)
            {
                if (!data.Contains("\n\n")) break;
                data = data.Replace("\n\n", "\n");
            }
            return data;
        }

        public static string Encrypt(string plainText, string passPhrase)
        {

            var saltStringBytes = Generate256BitsOfRandomEntropy();
            var ivStringBytes = Generate256BitsOfRandomEntropy();
            var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
                            {
                                cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
                                cryptoStream.FlushFinalBlock();

                                var cipherTextBytes = saltStringBytes;
                                cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
                                cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Convert.ToBase64String(cipherTextBytes);
                            }
                        }
                    }
                }
            }
        }

        public static string Decrypt(string cipherText, string passPhrase)
        {
            var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);

            var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(Keysize / 8).ToArray();

            var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(Keysize / 8).Take(Keysize / 8).ToArray();

            var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((Keysize / 8) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((Keysize / 8) * 2)).ToArray();

            using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, DerivationIterations))
            {
                var keyBytes = password.GetBytes(Keysize / 8);
                using (var symmetricKey = new RijndaelManaged())
                {
                    symmetricKey.BlockSize = 256;
                    symmetricKey.Mode = CipherMode.CBC;
                    symmetricKey.Padding = PaddingMode.PKCS7;
                    using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
                    {
                        using (var memoryStream = new MemoryStream(cipherTextBytes))
                        {
                            using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
                            {
                                var plainTextBytes = new byte[cipherTextBytes.Length];
                                var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
                                memoryStream.Close();
                                cryptoStream.Close();
                                return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
                            }
                        }
                    }
                }
            }
        }

        private static byte[] Generate256BitsOfRandomEntropy()
        {
            var randomBytes = new byte[32];
            using (var rngCsp = new RNGCryptoServiceProvider())
            {
                rngCsp.GetBytes(randomBytes);
            }
            return randomBytes;
        }
    }
}
