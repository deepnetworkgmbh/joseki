using Microsoft.EntityFrameworkCore.Migrations;

namespace joseki.db.Migrations
{
#pragma warning disable SA1413, SA1601, CS1591
    public partial class Component_User_Role_Relations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ComponentUserRoleRelations",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(maxLength: 36, nullable: true),
                    RoleId = table.Column<string>(maxLength: 36, nullable: true),
                    ComponentId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComponentUserRoleRelations", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ComponentUserRoleRelations");
        }
    }
#pragma warning restore SA1413, SA1601, CS1591
}
