﻿// <auto-generated />
using System;
using Equinor.ProCoSys.IPO.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Equinor.ProCoSys.IPO.Infrastructure.Migrations
{
    [DbContext(typeof(IPOContext))]
    partial class IPOContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate.History", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1024)
                        .HasColumnType("nvarchar(1024)");

                    b.Property<string>("EventType")
                        .IsRequired()
                        .HasColumnType("nvarchar(max)");

                    b.Property<Guid>("ObjectGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("ObjectType")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<Guid>("SourceGuid")
                        .HasColumnType("uniqueidentifier");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("SourceGuid")
                        .HasDatabaseName("IX_History_SourceGuid_ASC");

                    b.ToTable("History", null, t =>
                        {
                            t.HasCheckConstraint("constraint_history_check_valid_event_type", "EventType in ('IpoCompleted','IpoAccepted','IpoSigned','IpoUnsigned','IpoUncompleted','IpoUnaccepted','IpoCreated','IpoEdited','AttachmentUploaded','AttachmentRemoved','CommentAdded','CommentRemoved','IpoCanceled','AttendedStatusUpdated','NoteUpdated','ScopeHandedOver')");
                        });
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Attachment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<Guid>("BlobStorageId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("FileName")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("InvitationId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("ModifiedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("InvitationId");

                    b.ToTable("Attachments", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.CommPkg", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CommPkgNo")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int?>("InvitationId")
                        .IsRequired()
                        .HasColumnType("int");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<bool>("RfocAccepted")
                        .HasColumnType("bit");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Status")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("System")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("InvitationId");

                    b.HasIndex("ProjectId");

                    b.HasIndex("Plant", "InvitationId");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Plant", "InvitationId"), new[] { "CommPkgNo" });

                    b.ToTable("CommPkgs", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Comment", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CommentText")
                        .IsRequired()
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<int>("InvitationId")
                        .HasColumnType("int");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("InvitationId");

                    b.ToTable("Comments", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("AcceptedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("AcceptedBy")
                        .HasColumnType("int");

                    b.Property<DateTime?>("CompletedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("CompletedBy")
                        .HasColumnType("int");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasMaxLength(4096)
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime>("EndTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<Guid>("Guid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Location")
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<Guid>("MeetingId")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime?>("ModifiedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<Guid>("ObjectGuid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime>("StartTimeUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("Status")
                        .HasColumnType("int");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(250)
                        .HasColumnType("nvarchar(250)");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("AcceptedBy");

                    b.HasIndex("CompletedBy");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("Plant")
                        .HasFilter("[Status] <> 3");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Plant"), new[] { "Description", "Status" });

                    b.HasIndex("ProjectId");

                    b.ToTable("Invitations", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.McPkg", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("CommPkgNo")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("InvitationId")
                        .HasColumnType("int");

                    b.Property<string>("McPkgNo")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int>("ProjectId")
                        .HasColumnType("int");

                    b.Property<bool>("RfocAccepted")
                        .HasColumnType("bit");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("System")
                        .IsRequired()
                        .HasMaxLength(40)
                        .HasColumnType("nvarchar(40)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("InvitationId");

                    b.HasIndex("ProjectId");

                    b.HasIndex("Plant", "InvitationId");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Plant", "InvitationId"), new[] { "CommPkgNo" });

                    b.ToTable("McPkgs", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Participant", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<bool>("Attended")
                        .HasColumnType("bit");

                    b.Property<Guid?>("AzureOid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FirstName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("FunctionalRoleCode")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("InvitationId")
                        .HasColumnType("int");

                    b.Property<bool>("IsAttendedTouched")
                        .HasColumnType("bit");

                    b.Property<string>("LastName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("ModifiedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<string>("Note")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("Organization")
                        .HasColumnType("int");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<DateTime?>("SignedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("SignedBy")
                        .HasColumnType("int");

                    b.Property<int>("SortKey")
                        .HasColumnType("int");

                    b.Property<int>("Type")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("SignedBy");

                    b.HasIndex("InvitationId", "Plant");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("InvitationId", "Plant"), new[] { "AzureOid", "FunctionalRoleCode", "Organization", "SignedAtUtc", "SortKey", "Type", "SignedBy" });

                    b.ToTable("Participants", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<Guid>("Guid")
                        .HasColumnType("uniqueidentifier");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.Property<DateTime?>("ModifiedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("UserName")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.HasIndex("Guid")
                        .IsUnique();

                    b.HasIndex("ModifiedById");

                    b.ToTable("Persons", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.SavedFilter", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Criteria")
                        .IsRequired()
                        .HasMaxLength(8000)
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("DefaultFilter")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<int>("PersonId")
                        .HasColumnType("int");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<int?>("ProjectId")
                        .HasColumnType("int");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Title")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("PersonId");

                    b.HasIndex("ProjectId");

                    b.ToTable("SavedFilters", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate.Project", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("CreatedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int>("CreatedById")
                        .HasColumnType("int");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasMaxLength(1000)
                        .HasColumnType("nvarchar(1000)");

                    b.Property<bool>("IsClosed")
                        .HasColumnType("bit");

                    b.Property<DateTime?>("ModifiedAtUtc")
                        .HasColumnType("datetime2");

                    b.Property<int?>("ModifiedById")
                        .HasColumnType("int");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("nvarchar(30)");

                    b.Property<string>("Plant")
                        .IsRequired()
                        .HasMaxLength(255)
                        .HasColumnType("nvarchar(255)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.HasKey("Id");

                    b.HasIndex("CreatedById");

                    b.HasIndex("ModifiedById");

                    b.HasIndex("Name")
                        .HasDatabaseName("IX_Projects_Name_ASC");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Name"), new[] { "Plant" });

                    b.HasIndex("Plant")
                        .HasDatabaseName("IX_Projects_Plant_ASC");

                    SqlServerIndexBuilderExtensions.IncludeProperties(b.HasIndex("Plant"), new[] { "Name", "IsClosed", "CreatedAtUtc", "ModifiedAtUtc" });

                    b.ToTable("Projects", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.SettingAggregate.Setting", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Code")
                        .IsRequired()
                        .HasMaxLength(64)
                        .HasColumnType("nvarchar(64)");

                    b.Property<byte[]>("RowVersion")
                        .IsConcurrencyToken()
                        .ValueGeneratedOnAddOrUpdate()
                        .HasColumnType("rowversion");

                    b.Property<string>("Value")
                        .IsRequired()
                        .HasMaxLength(128)
                        .HasColumnType("nvarchar(128)");

                    b.HasKey("Id");

                    b.ToTable("Setting", (string)null);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.HistoryAggregate.History", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Attachment", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", null)
                        .WithMany("Attachments")
                        .HasForeignKey("InvitationId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.CommPkg", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", null)
                        .WithMany("CommPkgs")
                        .HasForeignKey("InvitationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Comment", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", null)
                        .WithMany("Comments")
                        .HasForeignKey("InvitationId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("AcceptedBy")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CompletedBy")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.McPkg", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", null)
                        .WithMany("McPkgs")
                        .HasForeignKey("InvitationId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Participant", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", null)
                        .WithMany("Participants")
                        .HasForeignKey("InvitationId")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("SignedBy")
                        .OnDelete(DeleteBehavior.NoAction);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.SavedFilter", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction);

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany("SavedFilters")
                        .HasForeignKey("PersonId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate.Project", null)
                        .WithMany()
                        .HasForeignKey("ProjectId")
                        .OnDelete(DeleteBehavior.NoAction);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.ProjectAggregate.Project", b =>
                {
                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("CreatedById")
                        .OnDelete(DeleteBehavior.NoAction)
                        .IsRequired();

                    b.HasOne("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", null)
                        .WithMany()
                        .HasForeignKey("ModifiedById")
                        .OnDelete(DeleteBehavior.NoAction);
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.InvitationAggregate.Invitation", b =>
                {
                    b.Navigation("Attachments");

                    b.Navigation("CommPkgs");

                    b.Navigation("Comments");

                    b.Navigation("McPkgs");

                    b.Navigation("Participants");
                });

            modelBuilder.Entity("Equinor.ProCoSys.IPO.Domain.AggregateModels.PersonAggregate.Person", b =>
                {
                    b.Navigation("SavedFilters");
                });
#pragma warning restore 612, 618
        }
    }
}
