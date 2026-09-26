using System;
using System.Security.Cryptography;
using System.Text;

namespace PosApp.Services
{
    /// <summary>
    /// RSA Challenge-Response tabanlı geliştirici destek girişi.
    /// Public key uygulamaya gömülüdür. Private key yalnızca geliştiricide bulunur.
    /// Böylece kaynak koda veya binary'e erişim olsa bile kilit açılamaz.
    /// </summary>
    public static class SupportUnlockService
    {
        // RSA-2048 Public Key (XML formatı) — Sadece doğrulama için kullanılır.
        // Bu key ile imzayı DOĞRULAMAK mümkündür, imza ÜRETMEK mümkün değildir.
        private const string PublicKeyXml =
            "<RSAKeyValue>" +
            "<Modulus>q81GC95a4XxZ2slM9FKWVEiLPt9hhIs/VYSMHTnrJfgQ1QZUAUVeCalfM4wJJSpwpEJsm4XHuWFbp/xDE+l0n8b2hcwNaSd/MigjEI/PTInyTLHtJblZ37pVV3Vk40U5yQVfI56i8qE9ZDT7lv7kix2hr1Zu0u3eFy6oS13mvVZ9baDCkA+P5ZFuzmfeVwYeM37vz733I+3EGxYVr63OUmGSs7Lb6nNdofd4Hw8B12jwG/jTKB0bRt37xHyUuSvm/Yd8FOsKR8ZAudHf5E0BxxTTd0I34rTmPZy1JuWtrLPByHAKxb7Aq79OgkG5m05N1G4lZpVSwgthtaHpMIg0BQ==</Modulus>" +
            "<Exponent>AQAB</Exponent>" +
            "</RSAKeyValue>";

        /// <summary>
        /// Bu cihaza özgü, günlük değişen destek kodu üretir.
        /// Kullanıcı bu kodu geliştirici ile paylaşır.
        /// Format: "XXXXXXXX" (8 karakter büyük hex)
        /// </summary>
        public static string GenerateChallengeCode()
        {
            string machineId = GetMachineId();
            string dateStr   = DateTime.Now.ToString("yyyyMMdd");
            string raw       = machineId + "|" + dateStr;

            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(raw));
            // İlk 4 byte → 8 karakter hex
            return BitConverter.ToString(hash, 0, 4).Replace("-", "").ToUpperInvariant();
        }

        /// <summary>
        /// Geliştiriciden alınan kilit-açma kodunu doğrular.
        /// Kilit-açma kodu = Base64(RSA_Sign(privateKey, challengeCode))
        /// </summary>
        public static bool VerifyUnlockCode(string challengeCode, string unlockCodeBase64)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(challengeCode) || string.IsNullOrWhiteSpace(unlockCodeBase64))
                    return false;

                byte[] signature  = Convert.FromBase64String(unlockCodeBase64.Trim());
                byte[] dataBytes  = Encoding.UTF8.GetBytes(challengeCode);

                using var rsa = RSA.Create();
                rsa.FromXmlString(PublicKeyXml);

                // SHA-256 hash'i imzaya karşı PKCS#1 v1.5 ile doğrula
                return rsa.VerifyData(dataBytes, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Makineye özgü benzersiz kimlik döner (MachineGuid veya fallback).
        /// </summary>
        private static string GetMachineId()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(
                    @"SOFTWARE\Microsoft\Cryptography", false);
                if (key != null)
                {
                    var guid = key.GetValue("MachineGuid")?.ToString();
                    if (!string.IsNullOrEmpty(guid)) return guid;
                }
            }
            catch { /* Sessizce fallback'e geç */ }

            // Fallback: Makine adı + işletim sistemi versiyonu
            return Environment.MachineName + "|" + Environment.OSVersion.Version.ToString();
        }
    }
}
