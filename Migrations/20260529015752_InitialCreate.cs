using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esseti.Migrations
{
    /// <summary>
    /// Migracja inicjalizująca bazę danych — tworzy wszystkie tabele i relacje.
    /// </summary>
    public partial class InitialCreate : Migration
    {
        /// <summary>
        /// Tworzy początkowy schemat bazy danych ze wszystkimi tabelami, kluczami i indeksami.
        /// </summary>
        /// <param name="migrationBuilder">Builder do konstruowania operacji migracji.</param>
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "activity",
                columns: table => new
                {
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    address_line = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    postal_code = table.Column<string>(type: "TEXT", nullable: true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false),
                    time = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    person_in_charge_name = table.Column<string>(type: "TEXT", nullable: true),
                    person_in_charge_phone = table.Column<string>(type: "TEXT", nullable: true),
                    person_in_charge_email = table.Column<string>(type: "TEXT", nullable: true),
                    additional_information = table.Column<string>(type: "TEXT", nullable: true),
                    is_repeatable = table.Column<bool>(type: "INTEGER", nullable: false),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_activity", x => x.activity_id);
                });

            migrationBuilder.CreateTable(
                name: "authority_role",
                columns: table => new
                {
                    role_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    permissions = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_authority_role", x => x.role_id);
                });

            migrationBuilder.CreateTable(
                name: "college",
                columns: table => new
                {
                    college_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    name_short = table.Column<string>(type: "TEXT", nullable: true),
                    college_avatar = table.Column<byte[]>(type: "BLOB", nullable: true),
                    address_line = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    postal_code = table.Column<string>(type: "TEXT", nullable: true),
                    phone = table.Column<string>(type: "TEXT", nullable: true),
                    NIP = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_college", x => x.college_id);
                });

            migrationBuilder.CreateTable(
                name: "section",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    short_name = table.Column<string>(type: "TEXT", nullable: false),
                    meetings = table.Column<string>(type: "TEXT", nullable: true),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_section", x => x.section_id);
                });

            migrationBuilder.CreateTable(
                name: "trip",
                columns: table => new
                {
                    trip_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    trip_photo = table.Column<byte[]>(type: "BLOB", nullable: true),
                    date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_trip", x => x.trip_id);
                });

            migrationBuilder.CreateTable(
                name: "user_account",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    email = table.Column<string>(type: "TEXT", nullable: false),
                    password_hash = table.Column<string>(type: "TEXT", nullable: false),
                    system_role = table.Column<string>(type: "TEXT", nullable: false),
                    is_verified = table.Column<bool>(type: "INTEGER", nullable: false),
                    created_at = table.Column<DateTime>(type: "TEXT", nullable: true),
                    last_login = table.Column<DateTime>(type: "TEXT", nullable: true),
                    updated_at = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_user_account", x => x.account_id);
                });

            migrationBuilder.CreateTable(
                name: "college_department",
                columns: table => new
                {
                    college_department_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    college_id = table.Column<int>(type: "INTEGER", nullable: false),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    address_line = table.Column<string>(type: "TEXT", nullable: true),
                    city = table.Column<string>(type: "TEXT", nullable: true),
                    postal_code = table.Column<string>(type: "TEXT", nullable: true),
                    phone = table.Column<string>(type: "TEXT", nullable: true),
                    email = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_college_department", x => x.college_department_id);
                    table.ForeignKey(
                        name: "f_k_college_department_college_college_id",
                        column: x => x.college_id,
                        principalTable: "college",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "account_college",
                columns: table => new
                {
                    account_id = table.Column<int>(type: "INTEGER", nullable: false),
                    college_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_account_college", x => new { x.account_id, x.college_id });
                    table.ForeignKey(
                        name: "FK_account_college_college_college_id",
                        column: x => x.college_id,
                        principalTable: "college",
                        principalColumn: "college_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_account_college_user_account_account_id",
                        column: x => x.account_id,
                        principalTable: "user_account",
                        principalColumn: "account_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "member",
                columns: table => new
                {
                    member_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    account_id = table.Column<int>(type: "INTEGER", nullable: true),
                    role_id = table.Column<int>(type: "INTEGER", nullable: true),
                    index_number = table.Column<string>(type: "TEXT", nullable: true),
                    first_name = table.Column<string>(type: "TEXT", nullable: false),
                    last_name = table.Column<string>(type: "TEXT", nullable: false),
                    major = table.Column<string>(type: "TEXT", nullable: true),
                    phone_number = table.Column<string>(type: "TEXT", nullable: true),
                    member_avatar = table.Column<byte[]>(type: "BLOB", nullable: true),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false),
                    join_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_member", x => x.member_id);
                    table.ForeignKey(
                        name: "f_k_member__user_accounts_account_id",
                        column: x => x.account_id,
                        principalTable: "user_account",
                        principalColumn: "account_id");
                    table.ForeignKey(
                        name: "f_k_member_authority_role_role_id",
                        column: x => x.role_id,
                        principalTable: "authority_role",
                        principalColumn: "role_id");
                });

            migrationBuilder.CreateTable(
                name: "club_info",
                columns: table => new
                {
                    club_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: true),
                    department_id = table.Column<int>(type: "INTEGER", nullable: true),
                    club_room = table.Column<string>(type: "TEXT", nullable: true),
                    supervisor_name = table.Column<string>(type: "TEXT", nullable: true),
                    supervisor_email = table.Column<string>(type: "TEXT", nullable: true),
                    supervisor_phone = table.Column<string>(type: "TEXT", nullable: true),
                    meetings_schedule = table.Column<string>(type: "TEXT", nullable: true),
                    short_name = table.Column<string>(type: "TEXT", nullable: true),
                    club_photo = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_club_info", x => x.club_id);
                    table.ForeignKey(
                        name: "f_k_club_info__college_departments_department_id",
                        column: x => x.department_id,
                        principalTable: "college_department",
                        principalColumn: "college_department_id");
                });

            migrationBuilder.CreateTable(
                name: "activity_member",
                columns: table => new
                {
                    activity_id = table.Column<int>(type: "INTEGER", nullable: false),
                    member_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_activity_member", x => new { x.activity_id, x.member_id });
                    table.ForeignKey(
                        name: "FK_activity_member_activity_activity_id",
                        column: x => x.activity_id,
                        principalTable: "activity",
                        principalColumn: "activity_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_activity_member_member_member_id",
                        column: x => x.member_id,
                        principalTable: "member",
                        principalColumn: "member_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    name = table.Column<string>(type: "TEXT", nullable: false),
                    description = table.Column<string>(type: "TEXT", nullable: true),
                    additional_information = table.Column<string>(type: "TEXT", nullable: true),
                    person_in_charge_id = table.Column<int>(type: "INTEGER", nullable: true),
                    github = table.Column<string>(type: "TEXT", nullable: true),
                    estimated_time = table.Column<int>(type: "INTEGER", nullable: true),
                    date_start = table.Column<DateTime>(type: "TEXT", nullable: true),
                    date_end = table.Column<DateTime>(type: "TEXT", nullable: true),
                    is_active = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_project", x => x.project_id);
                    table.ForeignKey(
                        name: "f_k_project__members_person_in_charge_id",
                        column: x => x.person_in_charge_id,
                        principalTable: "member",
                        principalColumn: "member_id");
                });

            migrationBuilder.CreateTable(
                name: "section_member",
                columns: table => new
                {
                    section_id = table.Column<int>(type: "INTEGER", nullable: false),
                    member_id = table.Column<int>(type: "INTEGER", nullable: false),
                    role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_section_member", x => new { x.section_id, x.member_id });
                    table.ForeignKey(
                        name: "f_k_section_member__members_member_id",
                        column: x => x.member_id,
                        principalTable: "member",
                        principalColumn: "member_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_section_member_section_section_id",
                        column: x => x.section_id,
                        principalTable: "section",
                        principalColumn: "section_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "club_trip",
                columns: table => new
                {
                    club_id = table.Column<int>(type: "INTEGER", nullable: false),
                    trip_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_club_trip", x => new { x.club_id, x.trip_id });
                    table.ForeignKey(
                        name: "FK_club_trip_club_info_club_id",
                        column: x => x.club_id,
                        principalTable: "club_info",
                        principalColumn: "club_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_club_trip_trip_trip_id",
                        column: x => x.trip_id,
                        principalTable: "trip",
                        principalColumn: "trip_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "member_club",
                columns: table => new
                {
                    club_id = table.Column<int>(type: "INTEGER", nullable: false),
                    member_id = table.Column<int>(type: "INTEGER", nullable: false),
                    club_role = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("p_k_member_club", x => new { x.club_id, x.member_id });
                    table.ForeignKey(
                        name: "f_k_member_club__members_member_id",
                        column: x => x.member_id,
                        principalTable: "member",
                        principalColumn: "member_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "f_k_member_club_club_info_club_id",
                        column: x => x.club_id,
                        principalTable: "club_info",
                        principalColumn: "club_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_club",
                columns: table => new
                {
                    club_id = table.Column<int>(type: "INTEGER", nullable: false),
                    project_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_club", x => new { x.club_id, x.project_id });
                    table.ForeignKey(
                        name: "FK_project_club_club_info_club_id",
                        column: x => x.club_id,
                        principalTable: "club_info",
                        principalColumn: "club_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_club_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_member",
                columns: table => new
                {
                    member_id = table.Column<int>(type: "INTEGER", nullable: false),
                    project_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_member", x => new { x.member_id, x.project_id });
                    table.ForeignKey(
                        name: "FK_project_member_member_member_id",
                        column: x => x.member_id,
                        principalTable: "member",
                        principalColumn: "member_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_member_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "project_sections",
                columns: table => new
                {
                    project_id = table.Column<int>(type: "INTEGER", nullable: false),
                    section_id = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_project_sections", x => new { x.project_id, x.section_id });
                    table.ForeignKey(
                        name: "FK_project_sections_project_project_id",
                        column: x => x.project_id,
                        principalTable: "project",
                        principalColumn: "project_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_project_sections_section_section_id",
                        column: x => x.section_id,
                        principalTable: "section",
                        principalColumn: "section_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_account_college_college_id",
                table: "account_college",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_activity_member_member_id",
                table: "activity_member",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_authority_role_name",
                table: "authority_role",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_club_info_department_id",
                table: "club_info",
                column: "department_id");

            migrationBuilder.CreateIndex(
                name: "IX_club_trip_trip_id",
                table: "club_trip",
                column: "trip_id");

            migrationBuilder.CreateIndex(
                name: "IX_college_department_college_id",
                table: "college_department",
                column: "college_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_account_id",
                table: "member",
                column: "account_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_role_id",
                table: "member",
                column: "role_id");

            migrationBuilder.CreateIndex(
                name: "IX_member_club_member_id",
                table: "member_club",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_person_in_charge_id",
                table: "project",
                column: "person_in_charge_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_club_project_id",
                table: "project_club",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_member_project_id",
                table: "project_member",
                column: "project_id");

            migrationBuilder.CreateIndex(
                name: "IX_project_sections_section_id",
                table: "project_sections",
                column: "section_id");

            migrationBuilder.CreateIndex(
                name: "IX_section_member_member_id",
                table: "section_member",
                column: "member_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_account_email",
                table: "user_account",
                column: "email",
                unique: true);
        }

        /// <summary>
        /// Cofa migrację — usuwa wszystkie tabele utworzone w metodzie Up.
        /// </summary>
        /// <param name="migrationBuilder">Builder do konstruowania operacji migracji.</param>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "account_college");

            migrationBuilder.DropTable(
                name: "activity_member");

            migrationBuilder.DropTable(
                name: "club_trip");

            migrationBuilder.DropTable(
                name: "member_club");

            migrationBuilder.DropTable(
                name: "project_club");

            migrationBuilder.DropTable(
                name: "project_member");

            migrationBuilder.DropTable(
                name: "project_sections");

            migrationBuilder.DropTable(
                name: "section_member");

            migrationBuilder.DropTable(
                name: "activity");

            migrationBuilder.DropTable(
                name: "trip");

            migrationBuilder.DropTable(
                name: "club_info");

            migrationBuilder.DropTable(
                name: "project");

            migrationBuilder.DropTable(
                name: "section");

            migrationBuilder.DropTable(
                name: "college_department");

            migrationBuilder.DropTable(
                name: "member");

            migrationBuilder.DropTable(
                name: "college");

            migrationBuilder.DropTable(
                name: "user_account");

            migrationBuilder.DropTable(
                name: "authority_role");
        }
    }
}


