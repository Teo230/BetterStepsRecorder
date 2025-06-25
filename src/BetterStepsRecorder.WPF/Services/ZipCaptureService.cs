using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BetterStepsRecorder.WPF.Services
{
    internal class ZipCaptureService : IZipCaptureService
    {
        public List<ScreenshotInfo> UnZipScreenshots(string filePath)
        {
            
        }

        public void ZipScreenshots(string filePath, List<ScreenshotInfo> screenshots)
        {
            using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Update))
            {
                var existingEntries = new HashSet<string>(zip.Entries.Select(e => e.FullName));
                var validEntries = new HashSet<string>();

                for (int i = 0; i < screenshots.Count; i++)
                {
                    // Update the Step based on the list position
                    Program._recordEvents[i].Step = i + 1;

                    var eventEntryName = $"events/event_{Program._recordEvents[i].ID}.json";

                    // Check if the entry already exists and remove it
                    var existingEntry = zip.GetEntry(eventEntryName);
                    if (existingEntry != null)
                    {
                        existingEntry.Delete(); // Remove the existing entry
                    }

                    // Serialize the RecordEvent object to JSON
                    var eventEntry = zip.CreateEntry(eventEntryName);
                    using (var entryStream = eventEntry.Open())
                    using (var writer = new StreamWriter(entryStream))
                    {
                        string json = JsonSerializer.Serialize(Program._recordEvents[i]);
                        writer.Write(json);
                    }

                    // Add the new entry to the set of valid entries
                    validEntries.Add(eventEntryName);

                    // Check for and add screenshot if not already processed
                }

                // Remove entries from the zip archive that are not in validEntries
                foreach (var entryName in existingEntries)
                {
                    if (!validEntries.Contains(entryName))
                    {
                        var entryToDelete = zip.GetEntry(entryName);
                        entryToDelete?.Delete();
                    }
                }
            }
        }
    }

    internal interface IZipCaptureService
    {
        void ZipScreenshots(string filePath, List<ScreenshotInfo> screenshots);
        List<ScreenshotInfo> UnZipScreenshots(string filePath);
    }
}
