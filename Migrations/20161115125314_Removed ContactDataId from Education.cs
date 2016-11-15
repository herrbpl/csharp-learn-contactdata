using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Migrations
{
    public partial class RemovedContactDataIdfromEducation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Education_ContactData_ContactDataId",
                table: "Education");

            migrationBuilder.AlterColumn<int>(
                name: "ContactDataId",
                table: "Education",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Education_ContactData_ContactDataId",
                table: "Education",
                column: "ContactDataId",
                principalTable: "ContactData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Education_ContactData_ContactDataId",
                table: "Education");

            migrationBuilder.AlterColumn<int>(
                name: "ContactDataId",
                table: "Education",
                nullable: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Education_ContactData_ContactDataId",
                table: "Education",
                column: "ContactDataId",
                principalTable: "ContactData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
