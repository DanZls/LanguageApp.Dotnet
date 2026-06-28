public class AzureSqlDatabaseService(AppDbContext db)
{
	private readonly AppDbContext _db = db;

  public async Task AddDictionaryRuDeTranslations()
	{
		string dictionaryPath = $"Resources/Processed/DictionaryRuDeFrequencyPages";
		string audioPath = "Resources/Audio/German";

		var dictionaryTranslationTerms = DictionaryParserService.ParseGeneratedDictionary(dictionaryPath, 1, 99)
			.ToArray();
		
		foreach (var translation in dictionaryTranslationTerms) {
			var audioBytes = await File.ReadAllBytesAsync($"{audioPath}/{translation.Id}--{translation.TermTranslation}.mp3");
			translation.TermTranslationAudio = audioBytes;
			_db.DictionaryRuDe.Add(translation);
		}

		await _db.SaveChangesAsync();
	}
}