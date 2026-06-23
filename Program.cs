using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<AppDbContext>(options =>
	options.UseSqlServer(builder.Configuration.GetConnectionString("LanguageAppDatabaseConnectionString"))
);

builder.Services.AddSingleton<AiService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}


app.MapHealthChecks("/health");


app.MapGet("/test-db", async (AppDbContext db) =>
{
	try
    {
        var canConnect = await db.Database.CanConnectAsync();
        var result = await db.Database
            .SqlQueryRaw<string>("SELECT SYSTEM_USER")
            .ToListAsync();

        return Results.Ok(new {
            Status = "✅ Connected",
            LoggedInAs = result.FirstOrDefault(),
            Database = db.Database.GetDbConnection().Database
        });
    }
    catch (Exception ex)
    {
        // Walk the full exception chain
        var messages = new List<string>();
        var current = ex;
        while (current != null)
        {
            messages.Add($"{current.GetType().Name}: {current.Message}");
            current = current.InnerException;
        }
        return Results.Json(new { Errors = messages }, statusCode: 500);
    }
});


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
