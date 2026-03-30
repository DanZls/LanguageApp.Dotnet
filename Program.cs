using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<AiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.MapGet("/ParseDictionary", async (AiService aiService) => {
	try {
		var dictionaryParser = new DictionaryParser(aiService);
		var result = await dictionaryParser.ParseDictionaryRuUshakovAsync();
		return Results.Ok(new {result});
	}
	catch (Exception ex){
		return Results.BadRequest(ex.Message);
	}
});


app.MapGet("/CombineDictionary", async (AiService aiService) => {
	try {
		var dictionaryParser = new DictionaryParser(aiService);
		await dictionaryParser.CombineDocumentPagesAsync();
		return Results.Ok();
	}
	catch (Exception ex){
		return Results.BadRequest(ex.Message);
	}
});


app.MapGet("/CreateDictionaryRuBgFrequencyPages", async (AiService aiService) => {
	try {
		var dictionaryParser = new DictionaryParser(aiService);
		await dictionaryParser.CreateDictionaryRuBgFrequencyPagesAsync();
		return Results.Ok();
	}
	catch (Exception ex){
		return Results.BadRequest(ex.Message);
	}
});


app.Run();
