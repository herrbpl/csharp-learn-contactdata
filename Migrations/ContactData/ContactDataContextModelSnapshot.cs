﻿using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using ASTV.Services;

namespace src.Migrations.ContactData
{
    [DbContext(typeof(ContactDataContext))]
    partial class ContactDataContextModelSnapshot : ModelSnapshot
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

                    b.Property<string>("ContactPerson")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("County")
                        .HasAnnotation("MaxLength", 30);

                    b.Property<string>("EmailBusiness")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("EmailPersonal")
                        .HasAnnotation("MaxLength", 50);

                    b.Property<string>("EmployeeId")
                        .HasAnnotation("MaxLength", 6);

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

                    b.Property<string>("Serialized")
                        .HasColumnName("Data");

                    b.Property<string>("UrlDriverLicense")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("UrlIdCard")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("UrlLivingPermit")
                        .HasAnnotation("MaxLength", 255);

                    b.Property<string>("ZipCode")
                        .HasAnnotation("MaxLength", 6);

                    b.HasKey("Id");

                    b.ToTable("ContactData");
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
        }
    }
}