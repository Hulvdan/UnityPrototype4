using System.Collections.Generic;
using System.Reactive.Subjects;

namespace BFG.Runtime.Localization {
public class LocalizationDatabase : SingletonMB<LocalizationDatabase> {
    Dictionary<string, LocalizationRecord> _translations = new();
    public Subject<Language> onLanguageChanged { get; } = new();

    public Language currentLanguage { get; private set; } = Language.En;

    protected override void SingletonAwakened() {
        _translations = new LocalizationDatabaseLoader().Load();
    }

    public string GetText(GStringKey key) {
        var rec = _translations[key.key];
        return currentLanguage switch {
            Language.En => rec.en,
            Language.Ru => rec.ru,
            _ => rec.en,
        };
    }

    public void ChangeLanguage(Language language) {
        if (currentLanguage != language) {
            currentLanguage = language;
            onLanguageChanged.OnNext(language);
        }
    }
}
}
