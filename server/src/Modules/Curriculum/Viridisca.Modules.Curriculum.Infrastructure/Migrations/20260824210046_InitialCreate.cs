using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viridisca.Modules.Curriculum.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "curriculum");

            migrationBuilder.CreateTable(
                name: "academic_periods",
                schema: "curriculum",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academic_periods", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "course_instances",
                schema: "curriculum",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_enrollments = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_instances", x => x.uid);
                    table.ForeignKey(
                        name: "fk_course_instances_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalSchema: "curriculum",
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                schema: "curriculum",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_score = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_assignments_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalSchema: "curriculum",
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                schema: "curriculum",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: true),
                    file_path = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<decimal>(type: "numeric(6,2)", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submissions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_submissions_assignments_assignment_uid",
                        column: x => x.assignment_uid,
                        principalSchema: "curriculum",
                        principalTable: "assignments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_academic_periods_code",
                schema: "curriculum",
                table: "academic_periods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignments_course_instance_uid",
                schema: "curriculum",
                table: "assignments",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_academic_period_uid",
                schema: "curriculum",
                table: "course_instances",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_group_uid",
                schema: "curriculum",
                table: "course_instances",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_subject_uid",
                schema: "curriculum",
                table: "course_instances",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_teacher_uid",
                schema: "curriculum",
                table: "course_instances",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_assignment_uid_student_uid",
                schema: "curriculum",
                table: "submissions",
                columns: new[] { "assignment_uid", "student_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_submissions_graded_by_uid",
                schema: "curriculum",
                table: "submissions",
                column: "graded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_student_uid",
                schema: "curriculum",
                table: "submissions",
                column: "student_uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "submissions",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "assignments",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "course_instances",
                schema: "curriculum");

            migrationBuilder.DropTable(
                name: "academic_periods",
                schema: "curriculum");
        }
    }
}
