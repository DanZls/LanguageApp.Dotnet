var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.MapGet("/",
	async () => {
		try {
			string result = await new OpenAiService(app.Configuration).CompleteChatAsync();
			return result;
		}
		catch (Exception ex){
			return ex.Message;
		}
	}
);


app.Run();
