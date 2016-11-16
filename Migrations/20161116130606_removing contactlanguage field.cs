using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Migrations
{
    public partial class removingcontactlanguagefield : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactData_Language_ContactLanguageId",
                table: "ContactData");

            migrationBuilder.DropIndex(
                name: "IX_ContactData_ContactLanguageId",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "ContactLanguageId",
                table: "ContactData");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ContactLanguageId",
                table: "ContactData",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactData_ContactLanguageId",
                table: "ContactData",
                column: "ContactLanguageId");

            migrationBuilder.AddForeignKey(
                name: "FK_ContactData_Language_ContactLanguageId",
                table: "ContactData",
                column: "ContactLanguageId",
                principalTable: "Language",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
