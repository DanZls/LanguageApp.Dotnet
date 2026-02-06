var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;
var app = builder.Build();


app.MapGet("/",
	async () => {
		try {
			string result = await new OpenAiService(configuration).CompleteChatAsync();
			return result;
		}
		catch (Exception ex){
			return ex.Message;
		}
	}
);


app.Run();
