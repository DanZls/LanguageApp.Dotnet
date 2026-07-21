using Microsoft.EntityFrameworkCore;

public class AzureSqlDatabaseService(AppDbContext db)
{
	private readonly AppDbContext _db = db;


  public async Task<TranslationDto> GetNextTranslation(
		string userEmail,
		string language1Tag,
		string language2Tag
	) {
		int hoursAfterFirstLearn = 3*24 - 6;
		int hoursAfterSecondLearn = 7*24 - 6;
		int batchMaxSize = 100;

		DateTimeOffset now = DateTimeOffset.Now;

		IQueryable<TranslationLearningInfo> translations = _db.TranslationLearningInfoTable
			.AsNoTracking()
			.Where(tlInfo => 
				(tlInfo.FirstLearnAt == null)
				|| (tlInfo.SecondLearnAt == null && tlInfo.FirstLearnAt != null && tlInfo.FirstLearnAt.Value.AddHours(hoursAfterFirstLearn) <= now)
				|| (tlInfo.ThirdLearnAt == null && tlInfo.SecondLearnAt != null && tlInfo.SecondLearnAt.Value.AddHours(hoursAfterSecondLearn) <= now)
			)
			.Where(tlInfo => tlInfo.User.Email == userEmail)
			.Where(tlInfo => tlInfo.Translation.Language1Tag == language1Tag)
			.Where(tlInfo => tlInfo.Translation.Language2Tag == language2Tag)
			.Where(tlInfo => !tlInfo.Translation.IsSkipped)
			.OrderBy(tlInfo => tlInfo.Translation.FrequencyIndex)
			.Take(batchMaxSize);

		int firstTranslationIndex = translations
			.Include(tlInfo => tlInfo.Translation)
			.First()
			.Translation.FrequencyIndex!
			.Value;

		IQueryable<TranslationDto> translationDtos = translations
			.Where(tlInfo => Math.Floor((double)tlInfo.Translation.FrequencyIndex! / batchMaxSize) == Math.Floor((double)firstTranslationIndex / batchMaxSize))
			.OrderBy(tlInfo => tlInfo.LastViewAt)
			.ThenBy(tlInfo => tlInfo.Translation.FrequencyIndex)
			.Select(tlInfo => new TranslationDto {
				TranslationId = tlInfo.TranslationId,
				TranslationLearningInfoId = tlInfo.Id,
				Term = tlInfo.Translation.Term,
				TermMeaning = tlInfo.Translation.TermMeaning,
				TermTranslation = tlInfo.Translation.TermTranslation,
				TermTranslationTags = tlInfo.Translation.TermTranslationTags,
				TermTranslationAudio = tlInfo.Translation.TermTranslationAudio,
				IsTranslationFlagged = tlInfo.Translation.IsFlagged,
				IsTranslationSkipped = tlInfo.Translation.IsSkipped,
			});

		return await translationDtos.FirstAsync();
	}


	public async Task UpdateTranslation(
		TranslationUpdateDto translationUpdate
	) {
		DateTimeOffset now = DateTimeOffset.Now;

		TranslationLearningInfo translationLearningInfo = await _db.TranslationLearningInfoTable
			.Include(tlInfo => tlInfo.Translation)
			.Where(tlInfo => tlInfo.Id == translationUpdate.TranslationLearningInfoId)
			.Where(tlInfo => tlInfo.Translation.Id == translationUpdate.TranslationId)
			.FirstAsync();

		if (translationUpdate.IsViewed != null) {
			translationLearningInfo.LastViewAt = now;
		}
		if (translationUpdate.IsLearned != null && translationUpdate.IsLearned == true) {
			if (translationLearningInfo.FirstLearnAt == null)
				translationLearningInfo.FirstLearnAt = now;
			else if (translationLearningInfo.SecondLearnAt == null)
				translationLearningInfo.SecondLearnAt = now;
			else if (translationLearningInfo.ThirdLearnAt == null)
				translationLearningInfo.ThirdLearnAt = now;
		}
		if (translationUpdate.IsTranslationFlagged != null) {
			translationLearningInfo.Translation.IsFlagged = translationUpdate.IsTranslationFlagged.Value;
		}
		if (translationUpdate.IsTranslationSkipped != null) {
			translationLearningInfo.Translation.IsSkipped = translationUpdate.IsTranslationSkipped.Value;
		}

		await _db.SaveChangesAsync();
	}


  public async Task AddUser(string userEmail)
	{
		_db.Users.Add(new User {
			Email = userEmail
		});
		await _db.SaveChangesAsync();
	}


  public async Task AddUserTranslationLearningInfo(
		string userEmail,
		string language1Tag,
		string language2Tag
	) { 
		int userId = await GetUserId(userEmail);
		var translationIds = _db.Translations
			.AsNoTracking()
			.Where(translation => (translation.Language1Tag == language1Tag) && (translation.Language2Tag == language2Tag))
			.Select(translation => translation.Id);
		foreach (var translationId in translationIds) {
			_db.TranslationLearningInfoTable.Add(new TranslationLearningInfo {
				UserId = userId,
				TranslationId = translationId,
				LastViewAt = DateTimeOffset.Now
			});
		}
		await _db.SaveChangesAsync();
	}


  public async Task AddDictionaryRuDeTranslations()
	{
		string dictionaryPath = $"Resources/Processed/DictionaryRuDeFrequencyPages";
		string audioPath = "Resources/Audio/German";
		var dictionaryTranslationTerms = DictionaryParserService.ParseGeneratedDictionary(dictionaryPath, 1, 99)
			.ToArray();
		foreach (var translation in dictionaryTranslationTerms) {
			var audioBytes = await File.ReadAllBytesAsync($"{audioPath}/{translation.Id}--{translation.TermTranslation}.mp3");
			translation.TermTranslationAudio = audioBytes;
			// _db.Dictionaries.Add(translation);
		}
		await _db.SaveChangesAsync();
	}

	private async Task<int> GetUserId (string userEmail) {
		int userId = _db.Users
			.AsNoTracking()
			.Where(user => user.Email == userEmail)
			.Select(user => user.Id)
			.First();
		return userId;
	}
}