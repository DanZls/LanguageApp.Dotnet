using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<AiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.MapHealthChecks("/health");


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


app.MapGet("/ParseDictionaryRuOzhegov", async (AiService aiService) => {
	try {
		var dictionaryParser = new DictionaryParser(aiService);
		var result = await dictionaryParser.ParseDictRuOzhegovAsync();
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


app.MapGet("/CreateDictionaryRuBgFrequencyPages", async (
	AiService aiService,
	string? pageNumbers,
	int? pageNumber,
	int? startPageNumber,
	int? endPageNumber
) => {
	try {
		var dictionaryParser = new DictionaryParser(aiService);
		IEnumerable<int>? parsedPageNumbers = null;
		if (!string.IsNullOrWhiteSpace(pageNumbers)) {
			parsedPageNumbers = pageNumbers
				.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
				.Select(int.Parse)
				.Distinct()
				.OrderBy(n => n)
				.ToArray();
		}

		await dictionaryParser.CreateDictionaryRuBgFrequencyPagesAsync(
			pageNumber: pageNumber,
			pageNumbers: parsedPageNumbers,
			startPageNumber: startPageNumber ?? 200,
			endPageNumber: endPageNumber ?? 1000
		);
		return Results.Ok();
	}
	catch (Exception ex){
		return Results.BadRequest(ex.Message);
	}
});


app.Run();
