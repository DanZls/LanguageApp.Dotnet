using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

class DictionaryParser {
	private readonly AiService _aiService;
  

	public DictionaryParser(AiService aiService) {
		_aiService = aiService;
	}


	public async Task<List<string>> GetDocumentPagesAsync(string documentPath, int linesNumberOnPage, int overlapping) {
		string[] fileLines = await File.ReadAllLinesAsync(documentPath);
		List<string> pages = [];
		int currentLineNumber = 0;
		while (currentLineNumber < fileLines.Length) {
			string pageLines = fileLines
				.Skip(currentLineNumber)
				.Take(linesNumberOnPage)
				.Aggregate((prev, next) => $"{prev}\n{next}" );
			pages.Add(pageLines);
			currentLineNumber += linesNumberOnPage - overlapping;
		}
		return pages;
	}


	public async Task CombineDocumentPagesAsync() {
		int startPage = 0;
		int endPage = 736;
		string pageFilePathTemplate = "Resources/2.Processed/DictionaryRuBgChukalovPages/DictionaryRuBgProcessed{pageNumber}.txt";
		string combinedFilePath = "Resources/2.Processed/DictionaryRuBgChukalovProcessed.txt";
		await File.WriteAllTextAsync(combinedFilePath, "");
		using StreamWriter streamWriter = File.AppendText(combinedFilePath);
		for (int pageNumber = startPage; pageNumber <= endPage; pageNumber++) {
			string pageFilePath = pageFilePathTemplate.Replace("{pageNumber}", pageNumber.ToString());
			string[] pageLines = await File.ReadAllLinesAsync(pageFilePath);
			foreach (var pageLine in pageLines) {
				if (pageLine.Trim() == "")
					continue;
				streamWriter.WriteLine(pageLine.Trim().Replace("ё", "е"));
			}
		}
	}


	public async Task ParseDictionaryRuBgChukalovAsync() {
		int[] pageNumbers = [1, 23, 75, 231, 553];
		int pageSizeLines = 150;
		int delay = 10 * 1000;
		string promptPath = "Resources/Prompts/ParseRuBgDictionaryPage2.txt";
		string documentPath = "Resources/Dictionaries/DictionaryRuBgChukalov.txt";
		List<string> pages = await GetDocumentPagesAsync(documentPath, pageSizeLines, overlapping: 5);
		string prompt = File.ReadAllText(promptPath);
		var outputDictionary = new StringBuilder();
		for (int pageNumber = 0; pageNumber < pages.Count; pageNumber++) {
			if (!pageNumbers.Contains(pageNumber))
				continue;
			string page = pages[pageNumber];
			var pageParseRequest = new ParsePageRequest();
			pageParseRequest.Prompt = prompt;
			pageParseRequest.DocumentPage = page;
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(pageParseRequest.ToJson());
			File.WriteAllText($"Resources/Output/Intermediate/DictionaryRuBgProcessed{pageNumber}.txt", aiResponse);
			using (StreamWriter writer = File.AppendText($"Resources/Output/DictionaryRuBgProcessed.Total.txt")) {
				writer.WriteLine(aiResponse);
			}
			outputDictionary.Append(aiResponse);
			await Task.Delay(delay);
		}
		var dateTime = DateTime.Now.ToString("yyyy.MM.dd-HH.mm");
		File.WriteAllText($"Resources/Output/DictionaryRuBgProcessed[{dateTime}].txt", outputDictionary.ToString());
	}


	public async Task<string> ParseDictionaryRuUshakovAsync() {
		var pageNumbers = new HashSet<int>(Enumerable.Range(0, 150 + 1));
		int pageSizeLines = 300;
		int overlapping = 15;
		int delay = 20 * 1000;

		string documentPath = "Resources/1.Raw/DictionaryRuExplanatoryUshakov.txt";
		List<string> pages = await GetDocumentPagesAsync(documentPath, pageSizeLines, overlapping);
		string prompt = File.ReadAllText("Resources/Prompts/ParseDictionaryRuUshakovPage.txt");

		var outputDictionary = new StringBuilder();
		for (int pageNumber = 0; pageNumber < pages.Count; pageNumber++) {
			if (!pageNumbers.Contains(pageNumber))
				continue;
			string page = pages[pageNumber];
			var pageParseRequest = new ParsePageRequest {
				Prompt = prompt,
				DocumentPage = page
			};
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(pageParseRequest.ToJson());
			File.WriteAllText(
				$"Resources/2.Processed/DictionaryRuUshakovPages/DictionaryRuUshakov{pageNumber}.txt", 
				aiResponse
			);
			outputDictionary.Append(aiResponse);
			await Task.Delay(delay);
		}
		var dateTime = DateTime.Now.ToString("yyyy.MM.dd-HH.mm");
		File.WriteAllText(
			$"Resources/2.Processed/Intermediate/DictionaryRuUshakovProcessed[{dateTime}].txt", 
			outputDictionary.ToString()
		);
		return "Ok";
	}


	public async Task CreateDictionaryRuBgFrequencyPagesAsync() {
		string dictionaryRuFrequencyPath = "Resources/Raw/DictionaryRuFrequency30000.txt";
		string dictionaryRuBgPath = "Resources/Processed/DictionaryRuBgChukalovProcessed.txt";
		string outputPagePathTemplate = "Resources/Processed/DictionaryRuBgFrequencyPages/{pageNumber}.txt";
		string promptPath = "Resources/Prompts/CreateDictionaryRuBgFrequencyPage.txt";
		int startPageNumber = 0;
		int endPageNumber = 19;
		int linesOnPage = 50;
		int delay = 10 * 1000;

		List<string> dictionaryRuFrequencyPages = await GetDocumentPagesAsync(dictionaryRuFrequencyPath, linesOnPage, overlapping: 0);
		
		string[] dictionaryRuBgLines = await File.ReadAllLinesAsync(dictionaryRuBgPath);
		var dictionaryRuBgLinesTable = new Dictionary<string, string>();
		foreach (string line in dictionaryRuBgLines) {
			string keyWord = line.Split(' ').First();
			dictionaryRuBgLinesTable.TryAdd(keyWord, "");
			dictionaryRuBgLinesTable[keyWord] += line + "\n";
		}

		string prompt = await File.ReadAllTextAsync(promptPath);
		for (int pageNumber = 0; pageNumber < dictionaryRuFrequencyPages.Count; pageNumber++) {
			if (pageNumber < startPageNumber || pageNumber > endPageNumber)
				continue;
			string dictionaryRuFrequencyPage = dictionaryRuFrequencyPages[pageNumber];
			string dictionaryRuBgPage = dictionaryRuFrequencyPage
				.Split("\n")
				.Select(line => line.Split(' ')[2])
				.Select(word => dictionaryRuBgLinesTable.GetValueOrDefault(word, ""))
				.Aggregate((prev, next) => $"{prev}\n{next}");
			var request = new {
				Instructions = prompt,
				WordsRu = dictionaryRuFrequencyPage,
				DictionaryRuBg = dictionaryRuBgPage
			};
			string serializedRequest = RelaxedSerializer.SerializeToJson(request);
			string aiResponse = await _aiService.CompleteChatWithOpenAiAsync(serializedRequest);
			string outputPagePath = outputPagePathTemplate.Replace("{pageNumber}", pageNumber.ToString());
			File.WriteAllText(outputPagePath, aiResponse);
			await Task.Delay(delay);
		}
	}
	
}