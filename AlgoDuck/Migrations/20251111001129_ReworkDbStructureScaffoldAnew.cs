using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class ReworkDbStructureScaffoldAnew : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("category_pk", x => x.category_id);
                });

            migrationBuilder.CreateTable(
                name: "difficulty",
                columns: table => new
                {
                    difficulty_id = table.Column<Guid>(type: "uuid", nullable: false),
                    difficulty_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("difficulty_pk", x => x.difficulty_id);
                });

            migrationBuilder.CreateTable(
                name: "editor_theme",
                columns: table => new
                {
                    editor_theme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    theme_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("editor_theme_pk", x => x.editor_theme_id);
                });

            migrationBuilder.CreateTable(
                name: "language",
                columns: table => new
                {
                    language_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    version = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("language_pk", x => x.language_id);
                });

            migrationBuilder.CreateTable(
                name: "rarity",
                columns: table => new
                {
                    rarity_id = table.Column<Guid>(type: "uuid", nullable: false),
                    rarity_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("rarity_pk", x => x.rarity_id);
                });

            migrationBuilder.CreateTable(
                name: "status",
                columns: table => new
                {
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("status_pk", x => x.status_id);
                });

            migrationBuilder.CreateTable(
                name: "tag",
                columns: table => new
                {
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("tag_pk", x => x.tag_id);
                });

            migrationBuilder.CreateTable(
                name: "problem",
                columns: table => new
                {
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    difficulty_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("problem_pk", x => x.problem_id);
                    table.ForeignKey(
                        name: "problem_category_ref",
                        column: x => x.category_id,
                        principalTable: "category",
                        principalColumn: "category_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "problem_difficulty_ref",
                        column: x => x.difficulty_id,
                        principalTable: "difficulty",
                        principalColumn: "difficulty_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "item",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    price = table.Column<int>(type: "integer", nullable: false),
                    purchasable = table.Column<bool>(type: "boolean", nullable: false),
                    rarity_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("shop_pk", x => x.item_id);
                    table.ForeignKey(
                        name: "item_rarity_ref",
                        column: x => x.rarity_id,
                        principalTable: "rarity",
                        principalColumn: "rarity_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "has_tag",
                columns: table => new
                {
                    tag_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("has_tag_pk", x => new { x.tag_id, x.problem_id });
                    table.ForeignKey(
                        name: "has_tag_problem_ref",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "has_tag_tag_ref",
                        column: x => x.tag_id,
                        principalTable: "tag",
                        principalColumn: "tag_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "test_case",
                columns: table => new
                {
                    test_case_id = table.Column<Guid>(type: "uuid", nullable: false),
                    call_func = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    problem_problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    display = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    display_res = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("test_case_pk", x => x.test_case_id);
                    table.ForeignKey(
                        name: "test_case_problem",
                        column: x => x.problem_problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id");
                });

            migrationBuilder.CreateTable(
                name: "contest",
                columns: table => new
                {
                    contest_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contest_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    contest_description = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false),
                    contest_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    contest_end_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("contest_pk", x => x.contest_id);
                    table.ForeignKey(
                        name: "contest_item_ref",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "contest_problem",
                columns: table => new
                {
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    contest_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("contest_problem_pk", x => new { x.problem_id, x.contest_id });
                    table.ForeignKey(
                        name: "contest_problem_contest_ref",
                        column: x => x.contest_id,
                        principalTable: "contest",
                        principalColumn: "contest_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "contest_problem_problem_ref",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "application_user",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    coins = table.Column<int>(type: "integer", nullable: false),
                    experience = table.Column<int>(type: "integer", nullable: false),
                    amount_solved = table.Column<int>(type: "integer", nullable: false),
                    username = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    password_hash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    security_stamp = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    cohort_id = table.Column<Guid>(type: "uuid", nullable: true),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedUserName = table.Column<string>(type: "text", nullable: true),
                    NormalizedEmail = table.Column<string>(type: "text", nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("application_user_pk", x => x.user_id);
                });

            migrationBuilder.CreateTable(
                name: "cohort",
                columns: table => new
                {
                    cohort_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_by_user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("cohort_pk", x => x.cohort_id);
                    table.ForeignKey(
                        name: "FK_cohort_application_user_created_by_user_id",
                        column: x => x.created_by_user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "purchase",
                columns: table => new
                {
                    item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    selected = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("purchases_pk", x => new { x.item_id, x.user_id });
                    table.ForeignKey(
                        name: "item_purchase_ref",
                        column: x => x.item_id,
                        principalTable: "item",
                        principalColumn: "item_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "purchase_user_ref",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "session",
                columns: table => new
                {
                    session_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refresh_token_hash = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    refresh_token_salt = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    expires_at_utc = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    revoked_at_utc = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    replaced_by_session_id = table.Column<Guid>(type: "uuid", nullable: true),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("session_pk", x => x.session_id);
                    table.ForeignKey(
                        name: "session_application_user",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "session_replaced_by_session",
                        column: x => x.replaced_by_session_id,
                        principalTable: "session",
                        principalColumn: "session_id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_config",
                columns: table => new
                {
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_dark_mode = table.Column<bool>(type: "boolean", nullable: false),
                    is_high_contrast = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_config_pk", x => x.user_id);
                    table.ForeignKey(
                        name: "user_config_application_user",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateTable(
                name: "user_solution",
                columns: table => new
                {
                    solution_id = table.Column<Guid>(type: "uuid", nullable: false),
                    stars = table.Column<int>(type: "integer", nullable: false),
                    code_runtime_submitted = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    problem_id = table.Column<Guid>(type: "uuid", nullable: false),
                    status_id = table.Column<Guid>(type: "uuid", nullable: false),
                    language_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("user_solution_pk", x => x.solution_id);
                    table.ForeignKey(
                        name: "solution_user_ref",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_solution_language_ref",
                        column: x => x.language_id,
                        principalTable: "language",
                        principalColumn: "language_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_solution_problem_ref",
                        column: x => x.problem_id,
                        principalTable: "problem",
                        principalColumn: "problem_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "user_solution_status_ref",
                        column: x => x.status_id,
                        principalTable: "status",
                        principalColumn: "status_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "message",
                columns: table => new
                {
                    message_id = table.Column<Guid>(type: "uuid", nullable: false),
                    message = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    cohort_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("message_pk", x => x.message_id);
                    table.ForeignKey(
                        name: "message_cohort_ref",
                        column: x => x.cohort_id,
                        principalTable: "cohort",
                        principalColumn: "cohort_id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "message_user_ref",
                        column: x => x.user_id,
                        principalTable: "application_user",
                        principalColumn: "user_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "editor_layout",
                columns: table => new
                {
                    editor_layout_id = table.Column<Guid>(type: "uuid", nullable: false),
                    editor_theme_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_config_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("editor_layout_pk", x => x.editor_layout_id);
                    table.ForeignKey(
                        name: "editor_layout_editor_theme",
                        column: x => x.editor_theme_id,
                        principalTable: "editor_theme",
                        principalColumn: "editor_theme_id");
                    table.ForeignKey(
                        name: "editor_layout_user_config",
                        column: x => x.user_config_id,
                        principalTable: "user_config",
                        principalColumn: "user_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_application_user_cohort_id",
                table: "application_user",
                column: "cohort_id");

            migrationBuilder.CreateIndex(
                name: "IX_cohort_created_by_user_id",
                table: "cohort",
                column: "created_by_user_id");

            migrationBuilder.CreateIndex(
                name: "IX_contest_item_id",
                table: "contest",
                column: "item_id");

            migrationBuilder.CreateIndex(
                name: "IX_contest_problem_contest_id",
                table: "contest_problem",
                column: "contest_id");

            migrationBuilder.CreateIndex(
                name: "IX_editor_layout_editor_theme_id",
                table: "editor_layout",
                column: "editor_theme_id");

            migrationBuilder.CreateIndex(
                name: "IX_editor_layout_user_config_id",
                table: "editor_layout",
                column: "user_config_id");

            migrationBuilder.CreateIndex(
                name: "IX_has_tag_problem_id",
                table: "has_tag",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "IX_item_rarity_id",
                table: "item",
                column: "rarity_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_cohort_id",
                table: "message",
                column: "cohort_id");

            migrationBuilder.CreateIndex(
                name: "IX_message_user_id",
                table: "message",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_problem_category_id",
                table: "problem",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_problem_difficulty_id",
                table: "problem",
                column: "difficulty_id");

            migrationBuilder.CreateIndex(
                name: "IX_purchase_user_id",
                table: "purchase",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_replaced_by_session_id",
                table: "session",
                column: "replaced_by_session_id");

            migrationBuilder.CreateIndex(
                name: "IX_session_user_id",
                table: "session",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_test_case_problem_problem_id",
                table: "test_case",
                column: "problem_problem_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_language_id",
                table: "user_solution",
                column: "language_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_problem_id",
                table: "user_solution",
                column: "problem_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_status_id",
                table: "user_solution",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_solution_user_id",
                table: "user_solution",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "user_cohort_ref",
                table: "application_user",
                column: "cohort_id",
                principalTable: "cohort",
                principalColumn: "cohort_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_cohort_ref",
                table: "application_user");

            migrationBuilder.DropTable(
                name: "contest_problem");

            migrationBuilder.DropTable(
                name: "editor_layout");

            migrationBuilder.DropTable(
                name: "has_tag");

            migrationBuilder.DropTable(
                name: "message");

            migrationBuilder.DropTable(
                name: "purchase");

            migrationBuilder.DropTable(
                name: "session");

            migrationBuilder.DropTable(
                name: "test_case");

            migrationBuilder.DropTable(
                name: "user_solution");

            migrationBuilder.DropTable(
                name: "contest");

            migrationBuilder.DropTable(
                name: "editor_theme");

            migrationBuilder.DropTable(
                name: "user_config");

            migrationBuilder.DropTable(
                name: "tag");

            migrationBuilder.DropTable(
                name: "language");

            migrationBuilder.DropTable(
                name: "problem");

            migrationBuilder.DropTable(
                name: "status");

            migrationBuilder.DropTable(
                name: "item");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "difficulty");

            migrationBuilder.DropTable(
                name: "rarity");

            migrationBuilder.DropTable(
                name: "cohort");

            migrationBuilder.DropTable(
                name: "application_user");
        }
    }
}
