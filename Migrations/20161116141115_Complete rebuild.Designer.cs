using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ASTV.Services;

namespace src.Migrations
{
    [DbContext(typeof(EmployeeContext))]
    [Migration("20161116141115_Complete rebuild")]
    partial class Completerebuild
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ASTV.Models.Employee.Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EmployeeId")
                        .HasAnnotation("MaxLength", 6);

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Employees");
                });
        }
    }
}
