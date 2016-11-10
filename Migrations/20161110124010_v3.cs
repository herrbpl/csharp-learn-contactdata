using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace src.Migrations
{
    public partial class v3 : Migration
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

            migrationBuilder.AddColumn<int>(
                name: "EmployeeId",
                table: "ContactData",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ContactData_EmployeeId",
                table: "ContactData",
                column: "EmployeeId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ContactData_Employees_EmployeeId",
                table: "ContactData",
                column: "EmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ContactData_Employees_EmployeeId",
                table: "ContactData");

            migrationBuilder.DropIndex(
                name: "IX_ContactData_EmployeeId",
                table: "ContactData");

            migrationBuilder.DropColumn(
                name: "EmployeeId",
                table: "ContactData");

            migrationBuilder.AddColumn<int>(
                name: "ContactDataId",
                table: "Employees",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ContactDataId",
                table: "Employees",
                column: "ContactDataId");

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
