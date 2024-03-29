﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;
using WebApp.Domain;

namespace Sceny.Finance.WebApp.Migrations
{
    [DbContext(typeof(FinanceContext))]
    [Migration("20200301075226_transaction_status")]
    partial class Transaction_status
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn)
                .HasAnnotation("ProductVersion", "3.1.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            modelBuilder.Entity("WebApp.Domain.Budget.BudgetItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("CategoryId")
                        .HasColumnName("category_id")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Month")
                        .HasColumnName("month")
                        .HasColumnType("timestamp without time zone");

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_budget_item");

                    b.HasIndex("CategoryId")
                        .HasName("ix_budget_item_category_id");

                    b.ToTable("budget_item");
                });

            modelBuilder.Entity("WebApp.Domain.Management.Account", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("AccountTypeId")
                        .HasColumnName("account_type_id")
                        .HasColumnType("integer");

                    b.Property<int>("AccountTypeId1")
                        .HasColumnName("account_type_id1")
                        .HasColumnType("integer");

                    b.Property<int>("CurrencyId")
                        .HasColumnName("currency_id")
                        .HasColumnType("integer");

                    b.Property<string>("Institution")
                        .HasColumnName("institution")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<string>("Number")
                        .HasColumnName("number")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_account");

                    b.HasIndex("AccountTypeId1")
                        .HasName("ix_account_account_type_id1");

                    b.HasIndex("CurrencyId")
                        .HasName("ix_account_currency_id");

                    b.ToTable("account");
                });

            modelBuilder.Entity("WebApp.Domain.Management.AccountType", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_account_type");

                    b.ToTable("account_type");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Other"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Bank"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Cash"
                        },
                        new
                        {
                            Id = 4,
                            Name = "Credit Card"
                        });
                });

            modelBuilder.Entity("WebApp.Domain.Management.Category", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<int>("KindId")
                        .HasColumnName("kind_id")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_category");

                    b.HasIndex("KindId")
                        .HasName("ix_category_kind_id");

                    b.ToTable("category");
                });

            modelBuilder.Entity("WebApp.Domain.Management.Currency", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(128)")
                        .HasMaxLength(128);

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_currency");

                    b.ToTable("currency");
                });

            modelBuilder.Entity("WebApp.Domain.Transactional.TransactionItem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnName("id")
                        .HasColumnType("integer")
                        .HasAnnotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

                    b.Property<double>("Amount")
                        .HasColumnName("amount")
                        .HasColumnType("double precision");

                    b.Property<int>("CategoryId")
                        .HasColumnName("category_id")
                        .HasColumnType("integer");

                    b.Property<DateTime>("Date")
                        .HasColumnName("date")
                        .HasColumnType("timestamp without time zone");

                    b.Property<int>("FromAccountId")
                        .HasColumnName("from_account_id")
                        .HasColumnType("integer");

                    b.Property<string>("Information")
                        .IsRequired()
                        .HasColumnName("information")
                        .HasColumnType("character varying(2000)")
                        .HasMaxLength(2000);

                    b.Property<int>("KindId")
                        .HasColumnName("kind_id")
                        .HasColumnType("integer");

                    b.Property<string>("Memo")
                        .IsRequired()
                        .HasColumnName("memo")
                        .HasColumnType("character varying(2000)")
                        .HasMaxLength(2000);

                    b.Property<int>("StatusId")
                        .HasColumnName("status_id")
                        .HasColumnType("integer");

                    b.Property<int?>("ToAccountId")
                        .HasColumnName("to_account_id")
                        .HasColumnType("integer");

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_transaction_item");

                    b.HasIndex("CategoryId")
                        .HasName("ix_transaction_item_category_id");

                    b.HasIndex("FromAccountId")
                        .HasName("ix_transaction_item_from_account_id");

                    b.HasIndex("KindId")
                        .HasName("ix_transaction_item_kind_id");

                    b.HasIndex("StatusId")
                        .HasName("ix_transaction_item_status_id");

                    b.HasIndex("ToAccountId")
                        .HasName("ix_transaction_item_to_account_id");

                    b.ToTable("transaction_item");
                });

            modelBuilder.Entity("WebApp.Domain.Transactional.TransactionKind", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_transaction_kind");

                    b.ToTable("transaction_kind");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "Income"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Expense"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Transfer"
                        });
                });

            modelBuilder.Entity("WebApp.Domain.Transactional.TransactionStatus", b =>
                {
                    b.Property<int>("Id")
                        .HasColumnName("id")
                        .HasColumnType("integer");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnName("name")
                        .HasColumnType("character varying(32)")
                        .HasMaxLength(32);

                    b.Property<uint>("xmin")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnName("xmin")
                        .HasColumnType("xid");

                    b.HasKey("Id")
                        .HasName("pk_transaction_status");

                    b.ToTable("transaction_status");

                    b.HasData(
                        new
                        {
                            Id = 1,
                            Name = "None"
                        },
                        new
                        {
                            Id = 2,
                            Name = "Pending"
                        },
                        new
                        {
                            Id = 3,
                            Name = "Cleared"
                        });
                });

            modelBuilder.Entity("WebApp.Domain.Budget.BudgetItem", b =>
                {
                    b.HasOne("WebApp.Domain.Management.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("fk_budget_item_category_category_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebApp.Domain.Management.Account", b =>
                {
                    b.HasOne("WebApp.Domain.Management.AccountType", "AccountType")
                        .WithMany()
                        .HasForeignKey("AccountTypeId1")
                        .HasConstraintName("fk_account_account_type_account_type_id1")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Domain.Management.Currency", "Currency")
                        .WithMany()
                        .HasForeignKey("CurrencyId")
                        .HasConstraintName("fk_account_currency_currency_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebApp.Domain.Management.Category", b =>
                {
                    b.HasOne("WebApp.Domain.Transactional.TransactionKind", "Kind")
                        .WithMany()
                        .HasForeignKey("KindId")
                        .HasConstraintName("fk_category_transaction_kind_kind_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("WebApp.Domain.Transactional.TransactionItem", b =>
                {
                    b.HasOne("WebApp.Domain.Management.Category", "Category")
                        .WithMany()
                        .HasForeignKey("CategoryId")
                        .HasConstraintName("fk_transaction_item_category_category_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Domain.Management.Account", "FromAccount")
                        .WithMany()
                        .HasForeignKey("FromAccountId")
                        .HasConstraintName("fk_transaction_item_account_from_account_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Domain.Transactional.TransactionKind", "Kind")
                        .WithMany()
                        .HasForeignKey("KindId")
                        .HasConstraintName("fk_transaction_item_transaction_kind_kind_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Domain.Transactional.TransactionStatus", "Status")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .HasConstraintName("fk_transaction_item_transaction_status_status_id")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("WebApp.Domain.Management.Account", "ToAccount")
                        .WithMany()
                        .HasForeignKey("ToAccountId")
                        .HasConstraintName("fk_transaction_item_account_to_account_id");
                });
#pragma warning restore 612, 618
        }
    }
}
