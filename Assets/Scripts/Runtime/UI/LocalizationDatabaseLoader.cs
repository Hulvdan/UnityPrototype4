using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using UnityEngine;

namespace BFG.Runtime {
public class LocalizationDatabaseLoader {
    public Dictionary<string, LocalizationRecord> Load() {
        var resource = Resources.Load("Localization") as TextAsset;
        if (resource == null) {
            Debug.LogError("Could not load 'Localization' resource file!");
            return null;
        }

        var reader = new StringReader(resource.ToString());
        var config = new CsvConfiguration(CultureInfo.InvariantCulture) {
            HasHeaderRecord = true,
            PrepareHeaderForMatch = args => args.Header.ToLower(),
        };

        var translations = new Dictionary<string, LocalizationRecord>();
        using (var csv = new CsvReader(reader, config)) {
            var records = csv.GetRecords<LocalizationRecordRaw>();
            foreach (var rec in records) {
                translations.Add(rec.key, new() {
                    En = rec.en,
                    Ru = rec.ru,
                });
            }
        }

        return translations;
    }

    // ReSharper disable once ClassNeverInstantiated.Local
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Local")]
    class LocalizationRecordRaw {
        public string key { get; set; }
        public string en { get; set; }
        public string ru { get; set; }
    }
}
}
