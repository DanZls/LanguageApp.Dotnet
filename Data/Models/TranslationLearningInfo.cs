public class TranslationLearningInfo {
	public int Id { get; set; }
	public int UserId { get; set; } = new();
	public int TranslationId { get; set; } = new();
	public string? DictionaryName { get; set; }
	public string? FirstLearnDateTime { get; set; }
	public string? SecondLearnDateTime { get; set; }
	public string? ThirdLearnDateTime { get; set; }

	public User User { get; set; } = new();
	public Translation Translation { get; set; } = new();
}