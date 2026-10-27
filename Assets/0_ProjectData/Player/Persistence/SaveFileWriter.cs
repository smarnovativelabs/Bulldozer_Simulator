// Reused from the project owner's I_Am_Janitor save system.
using System;
using System.IO;

namespace Bulldozer.Player.Persistence
{
    internal readonly struct SaveWriteResult
    {
        public readonly bool Succeeded;
        public readonly string Error;
        public SaveWriteResult(bool succeeded, string error = null)
        {
            Succeeded = succeeded;
            Error = error;
        }
    }

    /// <summary>
    /// One file writer per save file. Sequence numbers come from the main-thread snapshot order,
    /// not worker scheduling order. This class makes no Unity calls and never awaits a continuation.
    /// </summary>
    internal sealed class SaveFileWriter
    {
        private readonly object writeLock = new object();
        private readonly string targetPath;
        private readonly string temporaryPath;
        private readonly string backupPath;
        private long latestAttemptedSequence;
        private long latestCommittedSequence;

        public SaveFileWriter(string targetPath, string temporaryPath, string backupPath)
        {
            this.targetPath = targetPath;
            this.temporaryPath = temporaryPath;
            this.backupPath = backupPath;
        }

        public SaveWriteResult Write(long sequence, string json, bool keepBackup)
        {
            lock (writeLock)
            {
                if (sequence <= 0) return new SaveWriteResult(false, "Invalid save sequence.");
                if (sequence <= latestAttemptedSequence)
                {
                    // A pause/focus flush can run before an older queued worker gets its turn.
                    // Even if the newer attempt failed, an old worker must not silently roll back.
                    return new SaveWriteResult(latestCommittedSequence >= sequence,
                        latestCommittedSequence >= sequence ? null : "A newer save attempt superseded this snapshot.");
                }
                latestAttemptedSequence = sequence;

                try
                {
                    string directory = Path.GetDirectoryName(targetPath);
                    if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                    File.WriteAllText(temporaryPath, json);
                    if (!File.Exists(targetPath)) File.Move(temporaryPath, targetPath);
                    else
                    {
                        try
                        {
                            File.Replace(temporaryPath, targetPath, keepBackup ? backupPath : null);
                        }
                        catch (PlatformNotSupportedException)
                        {
                            ReplaceWithBackupFallback(keepBackup);
                        }
                        catch (IOException)
                        {
                            ReplaceWithBackupFallback(keepBackup);
                        }
                    }
                    latestCommittedSequence = sequence;
                    return new SaveWriteResult(true);
                }
                catch (Exception exception)
                {
                    try { if (File.Exists(temporaryPath)) File.Delete(temporaryPath); }
                    catch (Exception) { /* Keep the last successful primary/backup untouched. */ }
                    return new SaveWriteResult(false, exception.Message);
                }
            }
        }

        private void ReplaceWithBackupFallback(bool keepBackup)
        {
            // The fallback has a wider replacement window. Require a backup before removing the
            // old primary when backups are enabled; a failed backup copy must not destroy it.
            if (keepBackup) File.Copy(targetPath, backupPath, true);
            File.Delete(targetPath);
            File.Move(temporaryPath, targetPath);
        }
    }
}

