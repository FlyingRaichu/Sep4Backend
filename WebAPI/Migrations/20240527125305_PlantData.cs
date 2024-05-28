using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sep4Backend.Migrations
{
    /// <inheritdoc />
    public partial class PlantData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "plant_data",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    plant_name = table.Column<string>(type: "text", nullable: false),
                    waterConductivity = table.Column<float>(type: "real", nullable: true),
                    waterTemperature = table.Column<float>(type: "real", nullable: true),
                    waterPh = table.Column<float>(type: "real", nullable: true),
                    waterFlow = table.Column<float>(type: "real", nullable: true),
                    waterLevel = table.Column<float>(type: "real", nullable: true),
                    airTemperature = table.Column<float>(type: "real", nullable: true),
                    airHumidity = table.Column<float>(type: "real", nullable: true),
                    airCo2 = table.Column<float>(type: "real", nullable: true),
                    lightLevel = table.Column<float>(type: "real", nullable: true),
                    dewPoint = table.Column<float>(type: "real", nullable: true),
                    vpdLevel = table.Column<float>(type: "real", nullable: true),
                    datetime = table.Column<DateTime>(name: "date-time", type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_plant_data", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "plant_data");
        }
    }
}
