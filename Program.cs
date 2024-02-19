using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Phasmophobia_Save_Decoder
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string saveDir = $@"{userProfile}\AppData\LocalLow\Kinetic Games\Phasmophobia\";
            string saveFile = $@"{userProfile}\AppData\LocalLow\Kinetic Games\Phasmophobia\SaveFile.txt";
            string saveFileDecrypted = $@"{userProfile}\AppData\LocalLow\Kinetic Games\Phasmophobia\SaveFile_Decrypted.txt";
            string saveFileEncrypted = $@"{userProfile}\AppData\LocalLow\Kinetic Games\Phasmophobia\SaveFile_Encrypted.txt";

            Console.WriteLine(" Phasmophobia Save Decoder/Encoder by Psystec");
            Console.WriteLine(" --------------------------------------------");
            Console.WriteLine("Discord: https://discord.gg/EyRgFdA");
            Console.WriteLine("GitHub: https://github.com/Psystec");
            Console.WriteLine();

            if (!File.Exists(saveFile))
            {
                Console.WriteLine($"Phasmophobia save file does not exist: '{saveFile}");
                Console.WriteLine($"Press any key to exit . . .");
                Console.ReadLine();
                Environment.Exit(0);
            }

            if (File.Exists(saveFileDecrypted))
                File.Delete(saveFileDecrypted);

            string decryptedText = Decrypt(saveFile);
            File.WriteAllText(saveFileDecrypted, decryptedText);

            Console.WriteLine("Edit 'SaveFile_Decrypted.txt' and Save then, type 'CONTINUE' and press 'Enter'");
            Process.Start(Environment.GetEnvironmentVariable("WINDIR") + @"\explorer.exe", saveDir);
            Console.Write("Type 'CONTINUE' (Case Sensitive): ");
            string command = Console.ReadLine();

            if (command == "CONTINUE")
            {
                string data = File.ReadAllText(saveFileDecrypted);
                Encrypt(saveFileEncrypted, data);
                File.Move(saveFile, saveFile + "_backup_" + DateTime.Now.ToString("yyyyMMddHHmmss"));
                File.Move(saveFileEncrypted, saveFile);
                File.Delete(saveFileDecrypted);
            }
            else
            {
                Console.WriteLine("Canceled...");
            }

            Console.WriteLine();
            Console.WriteLine("Thank you for using Phasmophobia Save Decoder/Encoder by Psystec.");
            Console.WriteLine("Discord: https://discord.gg/EyRgFdA");
            Console.WriteLine("GitHub: https://github.com/Psystec");
            Console.ReadLine();
        }

        static string Decrypt(string file)
        {
            byte[] data = File.ReadAllBytes(file);
            byte[] password = Encoding.ASCII.GetBytes("t36gref9u84y7f43g");

            // Extract the IV from the encrypted data
            byte[] iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, 16);

            // Create a new AES cipher object
            using (var aes = new AesManaged())
            {
                aes.Key = new Rfc2898DeriveBytes(password, iv, 100).GetBytes(16);
                aes.IV = iv;
                aes.Mode = CipherMode.CBC;

                // Decrypt the data
                using (var decryptor = aes.CreateDecryptor())
                using (var inputStream = new MemoryStream(data, 16, data.Length - 16))
                using (var cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
                using (var reader = new StreamReader(cryptoStream))
                {
                    string decryptedData = reader.ReadToEnd();
                    while (decryptedData[decryptedData.Length - 1] != '}')
                    {
                        decryptedData = decryptedData.Substring(0, decryptedData.Length - 1);
                    }
                    return decryptedData;
                }
            }
        }

        static void Encrypt(string file, string data)
        {
            // Replace single quotes with double quotes and True/False with true/false
            data = data.Replace("'", "\"").Replace("True", "true").Replace("False", "false");

            // Set the password
            byte[] password = Encoding.UTF8.GetBytes("t36gref9u84y7f43g");

            // Generate a random IV
            byte[] iv = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(iv);
            }

            // Create a new AES cipher object
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = new Rfc2898DeriveBytes(password, iv, 100).GetBytes(16);
                aesAlg.IV = iv;
                aesAlg.Mode = CipherMode.CBC;

                // Create a encryptor to perform the stream transform
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    // prepend the IV
                    msEncrypt.Write(iv, 0, iv.Length);
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(data);
                        }
                    }

                    byte[] encryptedData = msEncrypt.ToArray();

                    // Write the encrypted data to the file
                    File.WriteAllBytes(file, encryptedData);
                }
            }
        }
    }
}
