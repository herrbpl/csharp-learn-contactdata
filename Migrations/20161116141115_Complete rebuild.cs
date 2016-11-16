using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace src.Migrations
{
    public partial class Completerebuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Employees_ContactData_ContactDataId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ContactDataId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "ContactDataId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "Education");

            migrationBuilder.DropTable(
                name: "Language");

            migrationBuilder.DropTable(
                name: "ContactData");

            migrationBuilder.DropTable(
                name: "EducationLevel");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "Employees",
                maxLength: 6,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContactData",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Address1 = table.Column<string>(maxLength: 255, nullable: true),
                    Address2 = table.Column<string>(maxLength: 255, nullable: true),
                    CityOrCommune = table.Column<string>(maxLength: 100, nullable: true),
                    ContactPerson = table.Column<string>(maxLength: 50, nullable: true),
                    County = table.Column<string>(maxLength: 30, nullable: true),
                    EmailBusiness = table.Column<string>(maxLength: 50, nullable: true),
                    EmailPersonal = table.Column<string>(maxLength: 50, nullable: true),
                    FirstName = table.Column<string>(maxLength: 30, nullable: true),
                    JobTitle = table.Column<string>(maxLength: 100, nullable: true),
                    LastName = table.Column<string>(maxLength: 50, nullable: true),
                    MobileBusiness = table.Column<string>(maxLength: 15, nullable: true),
                    MobilePersonal = table.Column<string>(maxLength: 15, nullable: true),
                    PhoneBusiness = table.Column<string>(maxLength: 15, nullable: true),
                    PhoneContactPerson = table.Column<string>(maxLength: 15, nullable: true),
                    PhonePersonal = table.Column<string>(maxLength: 15, nullable: true),
                    QuickDialBusiness = table.Column<string>(maxLength: 3, nullable: true),
                    Data = table.Column<string>(nullable: true),
                    UrlDriverLicense = table.Column<string>(maxLength: 255, nullable: true),
                    UrlIdCard = table.Column<string>(maxLength: 255, nullable: true),
                    UrlLivingPermit = table.Column<string>(maxLength: 255, nullable: true),
                    ZipCode = table.Column<string>(maxLength: 6, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EducationLevel",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 3, nullable: true),
                    Name = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EducationLevel", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Language",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Code = table.Column<string>(maxLength: 3, nullable: true),
                    Name = table.Column<string>(maxLength: 30, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Language", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Education",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    ContactDataId = table.Column<int>(nullable: true),
                    LevelId = table.Column<int>(nullable: true),
                    NameOfDegree = table.Column<string>(maxLength: 255, nullable: true),
                    SchoolName = table.Column<string>(maxLength: 100, nullable: true),
                    Specification = table.Column<string>(maxLength: 255, nullable: true),
                    UrlDiploma = table.Column<string>(maxLength: 255, nullable: true),
                    YearCompleted = table.Column<int>(nullable: true),
                    YearStarted = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Education", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Education_ContactData_ContactDataId",
                        column: x => x.ContactDataId,
                        principalTable: "ContactData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Education_EducationLevel_LevelId",
                        column: x => x.LevelId,
                        principalTable: "EducationLevel",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "ContactDataId",
                table: "Employees",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "Employees",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ContactDataId",
                table: "Employees",
                column: "ContactDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Education_ContactDataId",
                table: "Education",
                column: "ContactDataId");

            migrationBuilder.CreateIndex(
                name: "IX_Education_LevelId",
                table: "Education",
                column: "LevelId");

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_ContactData_ContactDataId",
                table: "Employees",
                column: "ContactDataId",
                principalTable: "ContactData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
