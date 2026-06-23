public class Client {
	public int Id { get; set; }
	public string Name { get; set; } = "";
	public string Email { get; set; } = "";
	public string NativeLanguage { get; set; } = "";
	public string LearningLanguage { get; set; } = "";
	public ICollection<Dictionary> Dictionaries { get; set; } = [];
}