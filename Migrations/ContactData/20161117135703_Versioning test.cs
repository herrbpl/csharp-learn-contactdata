using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace src.Migrations.ContactData
{
    public partial class Versioningtest : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactData",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "ContactData");

            migrationBuilder.AddColumn<int>(
                name: "ChangeId",
                table: "ContactData",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "ContactData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ContactData",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidFrom",
                table: "ContactData",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "ValidUntil",
                table: "ContactData",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "ContactData",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "ContactData",
                maxLength: 6,
                nullable: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactData",
                table: "ContactData",
                column: "ChangeId");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ContactData_EmployeeId_Version",
                table: "ContactData",
                columns: new[] { "EmployeeId", "Version" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ContactData",
                table: "ContactData");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ContactData_EmployeeId_Version",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "ChangeId",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "ValidFrom",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "ValidUntil",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "Version",
                table: "ContactData");

            migrationBuilder.AddColumn<int>(
                name: "Id",
                table: "ContactData",
                nullable: false,
                defaultValue: 0)
                .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeId",
                table: "ContactData",
                maxLength: 6,
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ContactData",
                table: "ContactData",
                column: "Id");
        }
    }
}
