using System.Collections.Generic;
using System.Reactive.Subjects;

namespace BFG.Runtime.Localization {
public class LocalizationDatabase : SingletonMB<LocalizationDatabase> {
    Dictionary<string, LocalizationRecord> _translations = new();
    public Subject<Language> onLanguageChanged { get; } = new();

    public Language CurrentLanguage { get; private set; } = Language.EN;

    protected override void SingletonAwakened() {
        _translations = new LocalizationDatabaseLoader().Load();
    }

    public string GetText(GStringKey key) {
        var rec = _translations[key.Key];
        return CurrentLanguage switch {
            Language.EN => rec.En,
            Language.RU => rec.Ru,
            _ => rec.En,
        };
    }

    public void ChangeLanguage(Language language) {
        if (CurrentLanguage != language) {
            CurrentLanguage = language;
            onLanguageChanged.OnNext(language);
        }
    }
}
}
