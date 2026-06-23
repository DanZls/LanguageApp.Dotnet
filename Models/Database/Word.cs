public class Word {
	public int Id { get; set; }
	public int DictionaryId { get; set; }
	public string Term { get; set; } = "";
	public string Translation { get; set; } = "";
	public string ExampleSentence { get; set; } = "";
	public Dictionary Dictionary { get; set; } = new();
}