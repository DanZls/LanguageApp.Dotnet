using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LanguageApp.Dotnet.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DictionaryRuDe",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false),
                    Term = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermMeaning = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermTranslation = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermTranslationTags = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TermTranslationAudio = table.Column<byte[]>(type: "varbinary(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DictionaryRuDe", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DictionaryRuDe");
        }
    }
}
