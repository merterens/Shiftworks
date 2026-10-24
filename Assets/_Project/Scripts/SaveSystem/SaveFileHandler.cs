using System;
using System.IO;
using UnityEngine;

namespace Shiftworks.SaveSystem
{
    /// <summary>
    /// Pure file I/O for the save system — no game state, no Unity object references, just
    /// "write this JSON safely" / "read this JSON safely". Kept separate from SaveManager so
    /// the actual capture/restore logic never has to think about file paths or disk errors.
    ///
    /// Write safety: writes to a .tmp file first, backs up whatever the real save file
    /// currently holds (so one bad write can never destroy the only good save on disk),
    /// then only replaces the real file once the temp write fully succeeded. A failure at
    /// any point leaves the previous save file exactly as it was.
    /// </summary>
    public static class SaveFileHandler
    {
        private const string SaveFileName = "shiftworks_save.json";
        private const string TempSuffix = ".tmp";
        private const string BackupSuffix = ".bak";

        public static string SaveFilePath => Path.Combine(Application.persistentDataPath, SaveFileName);
        private static string TempFilePath => SaveFilePath + TempSuffix;
        private static string BackupFilePath => SaveFilePath + BackupSuffix;

        public static bool HasSaveFile => File.Exists(SaveFilePath);

        public static bool TryWrite(SaveGameData data, out string error)
        {
            error = null;

            try
            {
                string json = JsonUtility.ToJson(data, true);

                File.WriteAllText(TempFilePath, json);

                if (File.Exists(SaveFilePath))
                {
                    File.Copy(SaveFilePath, BackupFilePath, overwrite: true);
                }

                // File.Copy + Delete rather than File.Replace: works the same whether or not
                // SaveFilePath already exists (File.Replace requires the destination to
                // already exist), and is equally atomic-enough for a single local save slot.
                File.Copy(TempFilePath, SaveFilePath, overwrite: true);
                File.Delete(TempFilePath);

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Debug.LogError($"[Shiftworks] Kayıt yazılamadı: {ex}");
                return false;
            }
        }

        public static bool TryRead(out SaveGameData data, out string error)
        {
            data = null;
            error = null;

            if (!File.Exists(SaveFilePath))
            {
                error = "Kayıt dosyası bulunamadı.";
                return false;
            }

            try
            {
                string json = File.ReadAllText(SaveFilePath);
                data = JsonUtility.FromJson<SaveGameData>(json);

                if (data == null)
                {
                    error = "Kayıt dosyası boş veya okunamadı.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                Debug.LogError($"[Shiftworks] Kayıt okunamadı (bozuk JSON olabilir): {ex}");
                data = null;
                return false;
            }
        }
    }
}
