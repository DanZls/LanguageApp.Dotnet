public class Translation {
	public uint Id { get; set; }
	public string Term { get; set; } = "";
	public string TermMeaning { get; set; } = "";
	public string TermTranslation { get; set; } = "";
	public string[] TermTranslationTags { get; set; } = [];
	public byte[] TermTranslationAudio { get; set; } = [];
}