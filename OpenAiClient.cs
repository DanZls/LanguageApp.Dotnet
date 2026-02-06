using OpenAI.Chat;

class OpenAiService {

	private readonly IConfiguration _configuration;



	public OpenAiService(IConfiguration configuration)
	{
		_configuration = configuration;
	}


	public async Task<string> CompleteChatAsync(string prompt = "Say 'this is a test'")
	{
		var openAiClient = new ChatClient(
			model: "gpt-4.1",
			apiKey: _configuration["OpenAiApiKey"]
		);
		ChatCompletion chatCompletion = await openAiClient.CompleteChatAsync(prompt);

		return chatCompletion.Content[0].Text;
	}
}
