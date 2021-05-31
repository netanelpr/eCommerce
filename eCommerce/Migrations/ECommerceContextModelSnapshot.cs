﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using eCommerce.DataLayer;

namespace eCommerce.Migrations
{
    [DbContext(typeof(ECommerceContext))]
    partial class ECommerceContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Relational:MaxIdentifierLength", 128)
                .HasAnnotation("ProductVersion", "5.0.6")
                .HasAnnotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn);

            modelBuilder.Entity("eCommerce.Business.MemberInfo", b =>
                {
                    b.Property<string>("Id")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("Address")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("Birthday")
                        .HasColumnType("datetime2");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Name")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.ToTable("MemberInfos");
                });

            modelBuilder.Entity("eCommerce.Business.User", b =>
                {
                    b.Property<string>("Username")
                        .HasColumnType("nvarchar(450)");

                    b.Property<string>("MemberInfoId")
                        .HasColumnType("nvarchar(450)");

                    b.HasKey("Username");

                    b.HasIndex("MemberInfoId");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("eCommerce.DataLayer.Classroom", b =>
                {
                    b.Property<int>("ClassroomId")
                        .HasColumnType("int");

                    b.HasKey("ClassroomId");

                    b.ToTable("Classroom");
                });

            modelBuilder.Entity("eCommerce.DataLayer.Course", b =>
                {
                    b.Property<int>("CourseId")
                        .HasColumnType("int");

                    b.Property<string>("CourseName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("ListPair<Classroom, Course>HolderId")
                        .HasColumnType("int");

                    b.Property<int?>("ListPair<Classroom, Course>KeyId")
                        .HasColumnType("int");

                    b.HasKey("CourseId");

                    b.HasIndex("ListPair<Classroom, Course>HolderId", "ListPair<Classroom, Course>KeyId");

                    b.ToTable("Course");
                });

            modelBuilder.Entity("eCommerce.DataLayer.ListPair<eCommerce.DataLayer.Classroom, eCommerce.DataLayer.Course>", b =>
                {
                    b.Property<int>("HolderId")
                        .HasColumnType("int");

                    b.Property<int>("KeyId")
                        .HasColumnType("int");

                    b.HasKey("HolderId", "KeyId");

                    b.HasIndex("KeyId");

                    b.ToTable("ListPairs");
                });

            modelBuilder.Entity("eCommerce.Business.User", b =>
                {
                    b.HasOne("eCommerce.Business.MemberInfo", "MemberInfo")
                        .WithMany()
                        .HasForeignKey("MemberInfoId");

                    b.Navigation("MemberInfo");
                });

            modelBuilder.Entity("eCommerce.DataLayer.Course", b =>
                {
                    b.HasOne("eCommerce.DataLayer.ListPair<eCommerce.DataLayer.Classroom, eCommerce.DataLayer.Course>", null)
                        .WithMany("ValList")
                        .HasForeignKey("ListPair<Classroom, Course>HolderId", "ListPair<Classroom, Course>KeyId");
                });

            modelBuilder.Entity("eCommerce.DataLayer.ListPair<eCommerce.DataLayer.Classroom, eCommerce.DataLayer.Course>", b =>
                {
                    b.HasOne("eCommerce.DataLayer.Classroom", "Key")
                        .WithMany()
                        .HasForeignKey("KeyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Key");
                });

            modelBuilder.Entity("eCommerce.DataLayer.ListPair<eCommerce.DataLayer.Classroom, eCommerce.DataLayer.Course>", b =>
                {
                    b.Navigation("ValList");
                });
#pragma warning restore 612, 618
        }
    }
}
