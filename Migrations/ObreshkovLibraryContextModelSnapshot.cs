using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using ObreshkovLibrary.Data;

#nullable disable

namespace ObreshkovLibrary.Migrations
{
    [DbContext(typeof(ObreshkovLibraryContext))]
    partial class ObreshkovLibraryContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "8.0.23");

            modelBuilder.Entity("ObreshkovLibrary.Models.Book", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("Author")
                        .IsRequired()
                        .HasMaxLength(120)
                        .HasColumnType("TEXT");

                    b.Property<int?>("CategoryId")
                        .HasColumnType("INTEGER");

                    b.Property<string>("CoverUrl")
                        .HasColumnType("TEXT");

                    b.Property<string>("Description")
                        .HasMaxLength(1000)
                        .HasColumnType("TEXT");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<int>("Tags")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(160)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Year")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("CategoryId");

                    b.ToTable("Books");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.BookCopy", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookId")
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("BookId");

                    b.ToTable("BookCopies");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("TEXT");

                    b.Property<int?>("ParentCategoryId")
                        .HasColumnType("INTEGER");

                    b.HasKey("Id");

                    b.HasIndex("ParentCategoryId");

                    b.ToTable("Categories");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Client", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<string>("CardNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("CreatedOn")
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<int?>("Grade")
                        .IsRequired()
                        .HasColumnType("INTEGER");

                    b.Property<bool>("IsActive")
                        .HasColumnType("INTEGER");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("TEXT");

                    b.Property<string>("MiddleName")
                        .HasMaxLength(50)
                        .HasColumnType("TEXT");

                    b.Property<string>("PhoneNumber")
                        .IsRequired()
                        .HasMaxLength(20)
                        .HasColumnType("TEXT");

                    b.Property<string>("Section")
                        .IsRequired()
                        .HasMaxLength(2)
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("CardNumber")
                        .IsUnique();

                    b.ToTable("Clients");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Loan", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<int>("BookCopyId")
                        .HasColumnType("INTEGER");

                    b.Property<int>("ClientId")
                        .HasColumnType("INTEGER");

                    b.Property<DateTime>("DueDate")
                        .HasColumnType("TEXT");

                    b.Property<DateTime>("LoanDate")
                        .HasColumnType("TEXT");

                    b.Property<string>("Notes")
                        .HasMaxLength(200)
                        .HasColumnType("TEXT");

                    b.Property<DateTime?>("ReturnDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("ClientId");

                    b.HasIndex("BookCopyId", "ReturnDate");

                    b.ToTable("Loans");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Book", b =>
                {
                    b.HasOne("ObreshkovLibrary.Models.Category", "Category")
                        .WithMany("Books")
                        .HasForeignKey("CategoryId")
                        .OnDelete(DeleteBehavior.Restrict);

                    b.Navigation("Category");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.BookCopy", b =>
                {
                    b.HasOne("ObreshkovLibrary.Models.Book", "Book")
                        .WithMany("Copies")
                        .HasForeignKey("BookId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Book");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Category", b =>
                {
                    b.HasOne("ObreshkovLibrary.Models.Category", "ParentCategory")
                        .WithMany("Children")
                        .HasForeignKey("ParentCategoryId");

                    b.Navigation("ParentCategory");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Loan", b =>
                {
                    b.HasOne("ObreshkovLibrary.Models.BookCopy", "BookCopy")
                        .WithMany()
                        .HasForeignKey("BookCopyId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ObreshkovLibrary.Models.Client", "Client")
                        .WithMany()
                        .HasForeignKey("ClientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BookCopy");

                    b.Navigation("Client");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Book", b =>
                {
                    b.Navigation("Copies");
                });

            modelBuilder.Entity("ObreshkovLibrary.Models.Category", b =>
                {
                    b.Navigation("Books");

                    b.Navigation("Children");
                });
        }
    }
}
