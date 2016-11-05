using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ASTV.Services;

namespace src.Migrations
{
    [DbContext(typeof(EmployeeContext))]
    partial class EmployeeContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("ASTV.Models.Employee.ContactData", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address1")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("Address2")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("CityOrCommune")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<int?>("ContactLanguageId");

                    b.Property<string>("ContactPerson")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("County")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<string>("EmailBusiness")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("EmailPersonal")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<int>("EmployeeId");

                    b.Property<string>("FirstName")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<string>("JobTitle")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<string>("LastName")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("MobileBusiness")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("MobilePersonal")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("PhoneBusiness")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("PhoneContactPerson")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("PhonePersonal")
                        .HasAnnotation("MaxLength", 15);

                    b.Property<string>("QuickDialBusiness")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<string>("UrlDriverLicense")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("UrlIdCard")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("UrlLivingPermit")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("ZipCode")
                        .HasAnnotation("MaxLength", 6);

                    b.HasKey("Id");

                    b.HasIndex("ContactLanguageId");

                    b.HasIndex("EmployeeId")
                        .IsUnique();

                    b.ToTable("ContactData");
                });

            modelBuilder.Entity("ASTV.Models.Employee.Education", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("ContactDataId");

                    b.Property<int?>("LevelId");

                    b.Property<string>("NameOfDegree")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("SchoolName")
                        .HasAnnotation("MaxLength", 100);

                    b.Property<string>("Specification")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("UrlDiploma")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<int?>("YearCompleted");

                    b.Property<int>("YearStarted");

                    b.HasKey("Id");

                    b.HasIndex("ContactDataId");

                    b.HasIndex("LevelId");

                    b.ToTable("Education");
                });

            modelBuilder.Entity("ASTV.Models.Employee.Employee", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("EmployeeId");

                    b.Property<string>("Name");

                    b.HasKey("Id");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("ASTV.Models.Generic.EducationLevel", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 30);

                    b.HasKey("Id");

                    b.ToTable("EducationLevel");
                });

            modelBuilder.Entity("ASTV.Models.Generic.Language", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Code")
                        .HasAnnotation("MaxLength", 3);

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 30);

                    b.HasKey("Id");

                    b.ToTable("Language");
                });

            modelBuilder.Entity("ASTV.Models.Employee.ContactData", b =>
                {
                    b.HasOne("ASTV.Models.Generic.Language", "ContactLanguage")
                        .WithMany()
                        .HasForeignKey("ContactLanguageId");

                    b.HasOne("ASTV.Models.Employee.Employee", "Employee")
                        .WithOne("ContactData")
                        .HasForeignKey("ASTV.Models.Employee.ContactData", "EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("ASTV.Models.Employee.Education", b =>
                {
                    b.HasOne("ASTV.Models.Employee.ContactData", "ContactData")
                        .WithMany("Education")
                        .HasForeignKey("ContactDataId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("ASTV.Models.Generic.EducationLevel", "Level")
                        .WithMany()
                        .HasForeignKey("LevelId");
                });
        }
    }
}
