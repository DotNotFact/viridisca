using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viridisca.Modules.Academic.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "academic");

            migrationBuilder.CreateTable(
                name: "groups",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    curator_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    syllabus = table.Column<string>(type: "text", nullable: true),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    minimum_required_grade = table.Column<int>(type: "integer", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    specialization = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    qualifications = table.Column<string>(type: "text", nullable: true),
                    years_of_experience = table.Column<int>(type: "integer", nullable: false),
                    biography = table.Column<string>(type: "text", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teachers", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "students",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    last_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    middle_name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    phone_number = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    birth_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    student_code = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    emergency_contact_name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    emergency_contact_phone = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: true),
                    medical_information = table.Column<string>(type: "text", nullable: true),
                    graduation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_students", x => x.uid);
                    table.ForeignKey(
                        name: "fk_students_groups_group_uid",
                        column: x => x.group_uid,
                        principalSchema: "academic",
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "teacher_groups",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_curator = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teacher_groups_groups_group_uid",
                        column: x => x.group_uid,
                        principalSchema: "academic",
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_groups_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalSchema: "academic",
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teacher_groups_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalSchema: "academic",
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "teacher_subjects",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_main_teacher = table.Column<bool>(type: "boolean", nullable: false),
                    assigned_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ended_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_subjects", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teacher_subjects_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalSchema: "academic",
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_teacher_subjects_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalSchema: "academic",
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "student_parents",
                schema: "academic",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    relation = table.Column<int>(type: "integer", nullable: false),
                    is_primary_contact = table.Column<bool>(type: "boolean", nullable: false),
                    has_legal_guardianship = table.Column<bool>(type: "boolean", nullable: false),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_parents", x => x.uid);
                    table.ForeignKey(
                        name: "fk_student_parents_students_student_uid",
                        column: x => x.student_uid,
                        principalSchema: "academic",
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_groups_code",
                schema: "academic",
                table: "groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_groups_curator_uid",
                schema: "academic",
                table: "groups",
                column: "curator_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_department_uid",
                schema: "academic",
                table: "groups",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_student_parents_student_uid_parent_user_uid",
                schema: "academic",
                table: "student_parents",
                columns: new[] { "student_uid", "parent_user_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid",
                schema: "academic",
                table: "students",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_student_code",
                schema: "academic",
                table: "students",
                column: "student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_user_uid",
                schema: "academic",
                table: "students",
                column: "user_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_code",
                schema: "academic",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_department_uid",
                schema: "academic",
                table: "subjects",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_group_uid",
                schema: "academic",
                table: "teacher_groups",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_subject_uid",
                schema: "academic",
                table: "teacher_groups",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_groups_teacher_uid_group_uid_subject_uid",
                schema: "academic",
                table: "teacher_groups",
                columns: new[] { "teacher_uid", "group_uid", "subject_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_subject_uid",
                schema: "academic",
                table: "teacher_subjects",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_subjects_teacher_uid_subject_uid",
                schema: "academic",
                table: "teacher_subjects",
                columns: new[] { "teacher_uid", "subject_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_uid",
                schema: "academic",
                table: "teachers",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_employee_code",
                schema: "academic",
                table: "teachers",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_user_uid",
                schema: "academic",
                table: "teachers",
                column: "user_uid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "student_parents",
                schema: "academic");

            migrationBuilder.DropTable(
                name: "teacher_groups",
                schema: "academic");

            migrationBuilder.DropTable(
                name: "teacher_subjects",
                schema: "academic");

            migrationBuilder.DropTable(
                name: "students",
                schema: "academic");

            migrationBuilder.DropTable(
                name: "subjects",
                schema: "academic");

            migrationBuilder.DropTable(
                name: "teachers",
                schema: "academic");

            migrationBuilder.DropTable(
                name: "groups",
                schema: "academic");
        }
    }
}
