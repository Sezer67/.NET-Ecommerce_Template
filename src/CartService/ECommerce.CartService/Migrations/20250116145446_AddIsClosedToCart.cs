using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ECommerce.CartService.Migrations
{
    /// <inheritdoc />
    public partial class AddIsClosedToCart : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Carts",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Carts");
        }
    }
}
