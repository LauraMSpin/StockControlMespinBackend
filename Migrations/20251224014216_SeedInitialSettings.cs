using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EstoqueBackEnd.Migrations
{
    /// <inheritdoc />
    public partial class SeedInitialSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "settings",
                columns: new[] { "id", "low_stock_threshold", "company_name", "company_phone", "company_email", "company_address", "birthday_discount", "jar_discount", "created_at", "updated_at" },
                values: new object[] { Guid.NewGuid(), 10, "Mespin Doces e Salgados", null, null, null, 0m, 0m, DateTime.UtcNow, DateTime.UtcNow }
            );
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM settings");
        }
    }
}
