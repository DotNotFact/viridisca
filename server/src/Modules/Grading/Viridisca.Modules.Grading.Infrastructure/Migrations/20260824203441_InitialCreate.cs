using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viridisca.Modules.Grading.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "grading");

            migrationBuilder.CreateTable(
                name: "grades",
                schema: "grading",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    value = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    issued_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grades", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "grade_comments",
                schema: "grading",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<string>(type: "text", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_comments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grade_comments_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalSchema: "grading",
                        principalTable: "grades",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grade_revisions",
                schema: "grading",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    previous_value = table.Column<decimal>(type: "numeric", nullable: false),
                    new_value = table.Column<decimal>(type: "numeric", nullable: false),
                    previous_description = table.Column<string>(type: "text", nullable: false),
                    new_description = table.Column<string>(type: "text", nullable: false),
                    revision_reason = table.Column<string>(type: "text", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grade_revisions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grade_revisions_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalSchema: "grading",
                        principalTable: "grades",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_grade_comments_grade_uid",
                schema: "grading",
                table: "grade_comments",
                column: "grade_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grade_revisions_grade_uid",
                schema: "grading",
                table: "grade_revisions",
                column: "grade_uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "grade_comments",
                schema: "grading");

            migrationBuilder.DropTable(
                name: "grade_revisions",
                schema: "grading");

            migrationBuilder.DropTable(
                name: "grades",
                schema: "grading");
        }
    }
}
