public class Dictionary {
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Language { get; set; } = "";
	public ICollection<Word> Words { get; set; } = [];
}