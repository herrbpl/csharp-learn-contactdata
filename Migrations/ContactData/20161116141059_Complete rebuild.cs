using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace src.Migrations.ContactData
{
    public partial class Completerebuild : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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
                    EmployeeId = table.Column<string>(maxLength: 6, nullable: true),
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactData");

            migrationBuilder.DropTable(
                name: "EducationLevel");

            migrationBuilder.DropTable(
                name: "Language");
        }
    }
}
