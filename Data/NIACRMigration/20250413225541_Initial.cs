using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NIA_CRM.Data.NIACRMigration
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AnnualAction",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Note = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Asignee = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    AnnualStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AnnualAction", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Contacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 10, nullable: true),
                    Department = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Phone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    LinkedInUrl = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    ContactNote = table.Column<string>(type: "TEXT", nullable: true),
                    IsVip = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsArchieved = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DashboardLayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LayoutData = table.Column<string>(type: "TEXT", nullable: false),
                    UserId = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DashboardLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MemberSize = table.Column<int>(type: "INTEGER", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "TEXT", nullable: true),
                    JoinDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    MemberNote = table.Column<string>(type: "TEXT", nullable: true),
                    IsPaid = table.Column<bool>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    AddressID = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MembershipTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TypeName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    TypeDescr = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    EventDescription = table.Column<string>(type: "TEXT", nullable: true),
                    EventLocation = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    EventDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MTag",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MTagName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    MTagDescription = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MTag", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NAICSCodes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NAICSCodes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Opportunities",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    OpportunityName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    OpportunityAction = table.Column<string>(type: "TEXT", nullable: false),
                    POC = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Account = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    Interaction = table.Column<string>(type: "TEXT", nullable: true),
                    LastContact = table.Column<DateTime>(type: "TEXT", nullable: true),
                    OpportunityStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    OpportunityPriority = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Opportunities", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ProductionEmails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TemplateName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Subject = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    Body = table.Column<string>(type: "TEXT", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    EmailType = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductionEmails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sectors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SectorName = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SectorDescription = table.Column<string>(type: "TEXT", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sectors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Strategy",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    StrategyName = table.Column<string>(type: "TEXT", nullable: false),
                    StrategyAssignee = table.Column<string>(type: "TEXT", nullable: true),
                    StrategyNote = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    StrategyTerm = table.Column<int>(type: "INTEGER", nullable: false),
                    StrategyStatus = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Strategy", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "WidgetLayouts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    WidgetId = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionX = table.Column<int>(type: "INTEGER", nullable: false),
                    PositionY = table.Column<int>(type: "INTEGER", nullable: false),
                    Width = table.Column<int>(type: "INTEGER", nullable: false),
                    Height = table.Column<int>(type: "INTEGER", nullable: false),
                    IsOnDashboard = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WidgetLayouts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContactCancellations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CancellationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CancellationNote = table.Column<string>(type: "TEXT", nullable: true),
                    IsCancelled = table.Column<bool>(type: "INTEGER", nullable: false),
                    ContactID = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactCancellations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ContactCancellations_Contacts_ContactID",
                        column: x => x.ContactID,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContactLogos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ContactID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactLogos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ContactLogos_Contacts_ContactID",
                        column: x => x.ContactID,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ContactThumbnails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    ContactID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactThumbnails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ContactThumbnails_Contacts_ContactID",
                        column: x => x.ContactID,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Addresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    AddressLine1 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    AddressLine2 = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    City = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    StateProvince = table.Column<int>(type: "INTEGER", nullable: false),
                    PostalCode = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Addresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Addresses_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Cancellations",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CancellationDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CancellationNote = table.Column<string>(type: "TEXT", nullable: true),
                    IsCancelled = table.Column<bool>(type: "INTEGER", nullable: false),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "BLOB", rowVersion: true, nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UpdatedBy = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    UpdatedOn = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cancellations", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Cancellations_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberContacts",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberContacts", x => new { x.MemberId, x.ContactId });
                    table.ForeignKey(
                        name: "FK_MemberContacts_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberContacts_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberLogos",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberLogos", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MemberLogos_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemebrThumbnails",
                columns: table => new
                {
                    ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Content = table.Column<byte[]>(type: "BLOB", nullable: true),
                    MimeType = table.Column<string>(type: "TEXT", maxLength: 255, nullable: true),
                    MemberID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemebrThumbnails", x => x.ID);
                    table.ForeignKey(
                        name: "FK_MemebrThumbnails_Members_MemberID",
                        column: x => x.MemberID,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberMembershipTypes",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    MembershipTypeId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberMembershipTypes", x => new { x.MemberId, x.MembershipTypeId });
                    table.ForeignKey(
                        name: "FK_MemberMembershipTypes_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberMembershipTypes_MembershipTypes_MembershipTypeId",
                        column: x => x.MembershipTypeId,
                        principalTable: "MembershipTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MemberEvents",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    MEventID = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberEvents", x => new { x.MemberId, x.MEventID });
                    table.ForeignKey(
                        name: "FK_MemberEvents_MEvents_MEventID",
                        column: x => x.MEventID,
                        principalTable: "MEvents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MemberEvents_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MemberTags",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    MTagID = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberTags", x => new { x.MemberId, x.MTagID });
                    table.ForeignKey(
                        name: "FK_MemberTags_MTag_MTagID",
                        column: x => x.MTagID,
                        principalTable: "MTag",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberTags_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IndustryNAICSCodes",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    NAICSCodeId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IndustryNAICSCodes", x => new { x.MemberId, x.NAICSCodeId });
                    table.ForeignKey(
                        name: "FK_IndustryNAICSCodes_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IndustryNAICSCodes_NAICSCodes_NAICSCodeId",
                        column: x => x.NAICSCodeId,
                        principalTable: "NAICSCodes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Interactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    ContactId = table.Column<int>(type: "INTEGER", nullable: false),
                    InteractionDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    OpportunityId = table.Column<int>(type: "INTEGER", nullable: true),
                    InteractionNotes = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Interactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Interactions_Contacts_ContactId",
                        column: x => x.ContactId,
                        principalTable: "Contacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interactions_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Interactions_Opportunities_OpportunityId",
                        column: x => x.OpportunityId,
                        principalTable: "Opportunities",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "MemberSectors",
                columns: table => new
                {
                    MemberId = table.Column<int>(type: "INTEGER", nullable: false),
                    SectorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MemberSectors", x => new { x.MemberId, x.SectorId });
                    table.ForeignKey(
                        name: "FK_MemberSectors_Members_MemberId",
                        column: x => x.MemberId,
                        principalTable: "Members",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MemberSectors_Sectors_SectorId",
                        column: x => x.SectorId,
                        principalTable: "Sectors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Addresses_MemberId",
                table: "Addresses",
                column: "MemberId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cancellations_MemberID",
                table: "Cancellations",
                column: "MemberID");

            migrationBuilder.CreateIndex(
                name: "IX_ContactCancellations_ContactID",
                table: "ContactCancellations",
                column: "ContactID");

            migrationBuilder.CreateIndex(
                name: "IX_ContactLogos_ContactID",
                table: "ContactLogos",
                column: "ContactID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContactThumbnails_ContactID",
                table: "ContactThumbnails",
                column: "ContactID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_IndustryNAICSCodes_NAICSCodeId",
                table: "IndustryNAICSCodes",
                column: "NAICSCodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_ContactId",
                table: "Interactions",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_MemberId",
                table: "Interactions",
                column: "MemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Interactions_OpportunityId",
                table: "Interactions",
                column: "OpportunityId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberContacts_ContactId",
                table: "MemberContacts",
                column: "ContactId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberEvents_MEventID",
                table: "MemberEvents",
                column: "MEventID");

            migrationBuilder.CreateIndex(
                name: "IX_MemberLogos_MemberID",
                table: "MemberLogos",
                column: "MemberID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberMembershipTypes_MembershipTypeId",
                table: "MemberMembershipTypes",
                column: "MembershipTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Members_MemberName",
                table: "Members",
                column: "MemberName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MemberSectors_SectorId",
                table: "MemberSectors",
                column: "SectorId");

            migrationBuilder.CreateIndex(
                name: "IX_MemberTags_MTagID",
                table: "MemberTags",
                column: "MTagID");

            migrationBuilder.CreateIndex(
                name: "IX_MemebrThumbnails_MemberID",
                table: "MemebrThumbnails",
                column: "MemberID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Addresses");

            migrationBuilder.DropTable(
                name: "AnnualAction");

            migrationBuilder.DropTable(
                name: "Cancellations");

            migrationBuilder.DropTable(
                name: "ContactCancellations");

            migrationBuilder.DropTable(
                name: "ContactLogos");

            migrationBuilder.DropTable(
                name: "ContactThumbnails");

            migrationBuilder.DropTable(
                name: "DashboardLayouts");

            migrationBuilder.DropTable(
                name: "IndustryNAICSCodes");

            migrationBuilder.DropTable(
                name: "Interactions");

            migrationBuilder.DropTable(
                name: "MemberContacts");

            migrationBuilder.DropTable(
                name: "MemberEvents");

            migrationBuilder.DropTable(
                name: "MemberLogos");

            migrationBuilder.DropTable(
                name: "MemberMembershipTypes");

            migrationBuilder.DropTable(
                name: "MemberSectors");

            migrationBuilder.DropTable(
                name: "MemberTags");

            migrationBuilder.DropTable(
                name: "MemebrThumbnails");

            migrationBuilder.DropTable(
                name: "ProductionEmails");

            migrationBuilder.DropTable(
                name: "Strategy");

            migrationBuilder.DropTable(
                name: "WidgetLayouts");

            migrationBuilder.DropTable(
                name: "NAICSCodes");

            migrationBuilder.DropTable(
                name: "Opportunities");

            migrationBuilder.DropTable(
                name: "Contacts");

            migrationBuilder.DropTable(
                name: "MEvents");

            migrationBuilder.DropTable(
                name: "MembershipTypes");

            migrationBuilder.DropTable(
                name: "Sectors");

            migrationBuilder.DropTable(
                name: "MTag");

            migrationBuilder.DropTable(
                name: "Members");
        }
    }
}
