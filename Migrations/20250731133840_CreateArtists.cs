using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Sonic.API.Migrations
{
    /// <inheritdoc />
    public partial class CreateArtists : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_Events_EventsId",
                table: "EventUser");

            migrationBuilder.DropForeignKey(
                name: "FK_EventUser_Users_AttendeesId",
                table: "EventUser");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser");

            migrationBuilder.DropIndex(
                name: "IX_EventUser_EventsId",
                table: "EventUser");

            migrationBuilder.RenameTable(
                name: "EventUser",
                newName: "EventAttendees");

            migrationBuilder.RenameColumn(
                name: "EventsId",
                table: "EventAttendees",
                newName: "AttendedEventsId");

            migrationBuilder.AddColumn<string>(
                name: "InviteLink",
                table: "Events",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventAttendees",
                table: "EventAttendees",
                columns: new[] { "AttendedEventsId", "AttendeesId" });

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ExternalSources = table.Column<string>(type: "jsonb", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    Uuid = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Emoji = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "EventOrganizers",
                columns: table => new
                {
                    OrganizedEventsId = table.Column<int>(type: "integer", nullable: false),
                    OrganizersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventOrganizers", x => new { x.OrganizedEventsId, x.OrganizersId });
                    table.ForeignKey(
                        name: "FK_EventOrganizers_Events_OrganizedEventsId",
                        column: x => x.OrganizedEventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EventOrganizers_Users_OrganizersId",
                        column: x => x.OrganizersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistEvents",
                columns: table => new
                {
                    EventsId = table.Column<int>(type: "integer", nullable: false),
                    LineupId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistEvents", x => new { x.EventsId, x.LineupId });
                    table.ForeignKey(
                        name: "FK_ArtistEvents_Artists_LineupId",
                        column: x => x.LineupId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistEvents_Events_EventsId",
                        column: x => x.EventsId,
                        principalTable: "Events",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ArtistUser",
                columns: table => new
                {
                    ArtistsId = table.Column<int>(type: "integer", nullable: false),
                    MembersId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArtistUser", x => new { x.ArtistsId, x.MembersId });
                    table.ForeignKey(
                        name: "FK_ArtistUser_Artists_ArtistsId",
                        column: x => x.ArtistsId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ArtistUser_Users_MembersId",
                        column: x => x.MembersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventAttendees_AttendeesId",
                table: "EventAttendees",
                column: "AttendeesId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistEvents_LineupId",
                table: "ArtistEvents",
                column: "LineupId");

            migrationBuilder.CreateIndex(
                name: "IX_ArtistUser_MembersId",
                table: "ArtistUser",
                column: "MembersId");

            migrationBuilder.CreateIndex(
                name: "IX_EventOrganizers_OrganizersId",
                table: "EventOrganizers",
                column: "OrganizersId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendees_Events_AttendedEventsId",
                table: "EventAttendees",
                column: "AttendedEventsId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventAttendees_Users_AttendeesId",
                table: "EventAttendees",
                column: "AttendeesId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendees_Events_AttendedEventsId",
                table: "EventAttendees");

            migrationBuilder.DropForeignKey(
                name: "FK_EventAttendees_Users_AttendeesId",
                table: "EventAttendees");

            migrationBuilder.DropTable(
                name: "ArtistEvents");

            migrationBuilder.DropTable(
                name: "ArtistUser");

            migrationBuilder.DropTable(
                name: "EventOrganizers");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropPrimaryKey(
                name: "PK_EventAttendees",
                table: "EventAttendees");

            migrationBuilder.DropIndex(
                name: "IX_EventAttendees_AttendeesId",
                table: "EventAttendees");

            migrationBuilder.DropColumn(
                name: "InviteLink",
                table: "Events");

            migrationBuilder.RenameTable(
                name: "EventAttendees",
                newName: "EventUser");

            migrationBuilder.RenameColumn(
                name: "AttendedEventsId",
                table: "EventUser",
                newName: "EventsId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_EventUser",
                table: "EventUser",
                columns: new[] { "AttendeesId", "EventsId" });

            migrationBuilder.CreateIndex(
                name: "IX_EventUser_EventsId",
                table: "EventUser",
                column: "EventsId");

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_Events_EventsId",
                table: "EventUser",
                column: "EventsId",
                principalTable: "Events",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_EventUser_Users_AttendeesId",
                table: "EventUser",
                column: "AttendeesId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
