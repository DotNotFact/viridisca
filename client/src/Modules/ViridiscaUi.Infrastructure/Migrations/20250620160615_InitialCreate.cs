using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ViridiscaUi.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "academic_periods",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_current = table.Column<bool>(type: "boolean", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_academic_periods", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "library_resources",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    author = table.Column<string>(type: "text", nullable: true),
                    isbn = table.Column<string>(type: "text", nullable: true),
                    publisher = table.Column<string>(type: "text", nullable: true),
                    published_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    publication_year = table.Column<int>(type: "integer", nullable: true),
                    resource_type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "text", nullable: true),
                    total_copies = table.Column<int>(type: "integer", nullable: false),
                    available_copies = table.Column<int>(type: "integer", nullable: false),
                    is_digital = table.Column<bool>(type: "boolean", nullable: false),
                    digital_url = table.Column<string>(type: "text", nullable: true),
                    tags = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_library_resources", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "notification_templates",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    title_template = table.Column<string>(type: "text", nullable: false),
                    message_template = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    parameters_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_templates", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_permissions", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "persons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    first_name = table.Column<string>(type: "text", nullable: false),
                    last_name = table.Column<string>(type: "text", nullable: false),
                    middle_name = table.Column<string>(type: "text", nullable: true),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "text", nullable: true),
                    date_of_birth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    profile_image_url = table.Column<string>(type: "text", nullable: true),
                    address = table.Column<string>(type: "text", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_persons", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    role_type = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_roles", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "system_settings",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    key = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    data_type = table.Column<string>(type: "text", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    is_system = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_system_settings", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "accounts",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    is_email_confirmed = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    failed_login_attempts = table.Column<int>(type: "integer", nullable: false),
                    last_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    last_failed_login_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    locked_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_accounts", x => x.uid);
                    table.ForeignKey(
                        name: "fk_accounts_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "file_records",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    original_file_name = table.Column<string>(type: "text", nullable: false),
                    stored_file_name = table.Column<string>(type: "text", nullable: false),
                    file_path = table.Column<string>(type: "text", nullable: false),
                    content_type = table.Column<string>(type: "text", nullable: false),
                    file_size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_by_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    entity_type = table.Column<string>(type: "text", nullable: true),
                    entity_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true),
                    is_public = table.Column<bool>(type: "boolean", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_file_records", x => x.uid);
                    table.ForeignKey(
                        name: "fk_file_records_persons_uploaded_by_uid",
                        column: x => x.uploaded_by_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "library_loans",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    resource_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    loaned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    returned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    fine_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_library_loans", x => x.uid);
                    table.ForeignKey(
                        name: "fk_library_loans_library_resources_resource_uid",
                        column: x => x.resource_uid,
                        principalTable: "library_resources",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_library_loans_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "notification_settings",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    user_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    email_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    sms_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    push_notifications = table.Column<bool>(type: "boolean", nullable: false),
                    minimum_priority = table.Column<int>(type: "integer", nullable: false),
                    type_settings_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notification_settings", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notification_settings_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "notifications",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    template_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    title = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<string>(type: "text", nullable: true),
                    priority = table.Column<int>(type: "integer", nullable: false),
                    is_read = table.Column<bool>(type: "boolean", nullable: false),
                    sent_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    read_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    action_url = table.Column<string>(type: "text", nullable: true),
                    metadata_json = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_notifications", x => x.uid);
                    table.ForeignKey(
                        name: "fk_notifications_notification_templates_template_uid",
                        column: x => x.template_uid,
                        principalTable: "notification_templates",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_notifications_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "person_roles",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    role_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    context_entity_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    context_entity_type = table.Column<string>(type: "text", nullable: true),
                    context = table.Column<string>(type: "text", nullable: true),
                    assigned_by = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_person_roles", x => x.uid);
                    table.ForeignKey(
                        name: "fk_person_roles_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_person_roles_roles_role_uid",
                        column: x => x.role_uid,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    role_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    permission_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_role_permissions", x => new { x.role_uid, x.permission_uid });
                    table.ForeignKey(
                        name: "fk_role_permissions_permissions_permission_uid",
                        column: x => x.permission_uid,
                        principalTable: "permissions",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_role_permissions_roles_role_uid",
                        column: x => x.role_uid,
                        principalTable: "roles",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "assignment_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    average_grade = table.Column<decimal>(type: "numeric", nullable: false),
                    max_grade = table.Column<decimal>(type: "numeric", nullable: false),
                    min_grade = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignment_analytics", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "assignments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_score = table.Column<double>(type: "double precision", nullable: false),
                    max_points = table.Column<double>(type: "double precision", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    instructions = table.Column<string>(type: "text", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    attachments_path = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_assignments", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "attendances",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    checked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    checked_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_attendances", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "course_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    engagement_level = table.Column<decimal>(type: "numeric", nullable: false),
                    average_grade = table.Column<decimal>(type: "numeric", nullable: false),
                    average_attendance = table.Column<decimal>(type: "numeric", nullable: false),
                    average_quiz_score = table.Column<decimal>(type: "numeric", nullable: false),
                    average_exam_score = table.Column<decimal>(type: "numeric", nullable: false),
                    exam_pass_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_analytics", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "course_instances",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_enrollments = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_course_instances", x => x.uid);
                    table.ForeignKey(
                        name: "fk_course_instances_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discussions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_by_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    is_pinned = table.Column<bool>(type: "boolean", nullable: false),
                    is_locked = table.Column<bool>(type: "boolean", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    course_instance_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discussions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_discussions_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_discussions_course_instances_course_instance_uid1",
                        column: x => x.course_instance_uid1,
                        principalTable: "course_instances",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_discussions_persons_created_by_uid",
                        column: x => x.created_by_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exams",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    exam_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: false),
                    location = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    max_score = table.Column<decimal>(type: "numeric", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    course_instance_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exams", x => x.uid);
                    table.ForeignKey(
                        name: "fk_exams_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exams_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exams_course_instances_course_instance_uid1",
                        column: x => x.course_instance_uid1,
                        principalTable: "course_instances",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "lessons",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    order_index = table.Column<int>(type: "integer", nullable: false),
                    duration = table.Column<TimeSpan>(type: "interval", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lessons", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lessons_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "schedule_slots",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    room = table.Column<string>(type: "text", nullable: true),
                    location = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: true),
                    course_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_slots", x => x.uid);
                    table.ForeignKey(
                        name: "fk_schedule_slots_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "discussion_posts",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    content = table.Column<string>(type: "text", nullable: false),
                    discussion_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    author_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    parent_post_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_edited = table.Column<bool>(type: "boolean", nullable: false),
                    edited_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discussion_posts", x => x.uid);
                    table.ForeignKey(
                        name: "fk_discussion_posts_discussion_posts_parent_post_uid",
                        column: x => x.parent_post_uid,
                        principalTable: "discussion_posts",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_discussion_posts_discussions_discussion_uid",
                        column: x => x.discussion_uid,
                        principalTable: "discussions",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_discussion_posts_persons_author_uid",
                        column: x => x.author_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "exam_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    total_participants = table.Column<int>(type: "integer", nullable: false),
                    passed_count = table.Column<int>(type: "integer", nullable: false),
                    failed_count = table.Column<int>(type: "integer", nullable: false),
                    pass_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    average_score = table.Column<decimal>(type: "numeric", nullable: false),
                    highest_score = table.Column<decimal>(type: "numeric", nullable: false),
                    lowest_score = table.Column<decimal>(type: "numeric", nullable: false),
                    median_score = table.Column<decimal>(type: "numeric", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_exam_analytics_exams_exam_uid",
                        column: x => x.exam_uid,
                        principalTable: "exams",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "lesson_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    total_students = table.Column<int>(type: "integer", nullable: false),
                    present_students = table.Column<int>(type: "integer", nullable: false),
                    absent_students = table.Column<int>(type: "integer", nullable: false),
                    attendance_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    completed_students = table.Column<int>(type: "integer", nullable: false),
                    average_progress = table.Column<decimal>(type: "numeric", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lesson_analytics_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quizzes",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    instructions = table.Column<string>(type: "text", nullable: true),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    time_limit = table.Column<int>(type: "integer", nullable: true),
                    max_attempts = table.Column<int>(type: "integer", nullable: false),
                    passing_score = table.Column<double>(type: "double precision", nullable: false),
                    shuffle_questions = table.Column<bool>(type: "boolean", nullable: false),
                    shuffle_answers = table.Column<bool>(type: "boolean", nullable: false),
                    show_results_immediately = table.Column<bool>(type: "boolean", nullable: false),
                    show_correct_answers = table.Column<bool>(type: "boolean", nullable: false),
                    available_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    available_until = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    course_instance_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quizzes", x => x.uid);
                    table.ForeignKey(
                        name: "fk_quizzes_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quizzes_course_instances_course_instance_uid1",
                        column: x => x.course_instance_uid1,
                        principalTable: "course_instances",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_quizzes_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_quizzes_lessons_lesson_uid1",
                        column: x => x.lesson_uid1,
                        principalTable: "lessons",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "post_likes",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    post_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_post_likes", x => x.uid);
                    table.ForeignKey(
                        name: "fk_post_likes_discussion_posts_post_uid",
                        column: x => x.post_uid,
                        principalTable: "discussion_posts",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_post_likes_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    quiz_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    total_attempts = table.Column<int>(type: "integer", nullable: false),
                    completed_attempts = table.Column<int>(type: "integer", nullable: false),
                    passed_attempts = table.Column<int>(type: "integer", nullable: false),
                    pass_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    average_score = table.Column<decimal>(type: "numeric", nullable: false),
                    highest_score = table.Column<decimal>(type: "numeric", nullable: false),
                    lowest_score = table.Column<decimal>(type: "numeric", nullable: false),
                    average_completion_time = table.Column<decimal>(type: "numeric", nullable: false),
                    unique_participants = table.Column<int>(type: "integer", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_quiz_analytics_quizzes_quiz_uid",
                        column: x => x.quiz_uid,
                        principalTable: "quizzes",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_questions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    question_text = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    points = table.Column<double>(type: "double precision", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    quiz_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    explanation = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_questions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_quiz_questions_quizzes_quiz_uid",
                        column: x => x.quiz_uid,
                        principalTable: "quizzes",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "quiz_answers",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    answer_text = table.Column<string>(type: "text", nullable: false),
                    is_correct = table.Column<bool>(type: "boolean", nullable: false),
                    order = table.Column<int>(type: "integer", nullable: false),
                    question_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_answers", x => x.uid);
                    table.ForeignKey(
                        name: "fk_quiz_answers_quiz_questions_question_uid",
                        column: x => x.question_uid,
                        principalTable: "quiz_questions",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "courses",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    difficulty = table.Column<int>(type: "integer", nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    duration_hours = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_courses", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "curricula",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    code = table.Column<string>(type: "text", nullable: true),
                    total_credits = table.Column<int>(type: "integer", nullable: false),
                    duration_semesters = table.Column<int>(type: "integer", nullable: false),
                    duration_months = table.Column<int>(type: "integer", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    start_year = table.Column<int>(type: "integer", nullable: false),
                    end_year = table.Column<int>(type: "integer", nullable: true),
                    duration_in_semesters = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    valid_from = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    valid_to = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_curricula", x => x.uid);
                });

            migrationBuilder.CreateTable(
                name: "curriculum_subjects",
                columns: table => new
                {
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    semester = table.Column<int>(type: "integer", nullable: false),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    is_required = table.Column<bool>(type: "boolean", nullable: false),
                    is_mandatory = table.Column<bool>(type: "boolean", nullable: false),
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_curriculum_subjects", x => new { x.curriculum_uid, x.subject_uid });
                    table.ForeignKey(
                        name: "fk_curriculum_subjects_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "departments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    head_of_department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    parent_department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    faculty_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_departments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_departments_departments_parent_department_uid",
                        column: x => x.parent_department_uid,
                        principalTable: "departments",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "subjects",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    category = table.Column<string>(type: "text", nullable: true),
                    prerequisites = table.Column<string>(type: "text", nullable: true),
                    learning_outcomes = table.Column<string>(type: "text", nullable: true),
                    credits = table.Column<int>(type: "integer", nullable: false),
                    lessons_per_week = table.Column<int>(type: "integer", nullable: false),
                    hours = table.Column<int>(type: "integer", nullable: false),
                    type = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subjects", x => x.uid);
                    table.ForeignKey(
                        name: "fk_subjects_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_subjects_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "teachers",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    employee_code = table.Column<string>(type: "text", nullable: false),
                    hire_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    termination_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    qualification = table.Column<string>(type: "text", nullable: false),
                    specialization = table.Column<string>(type: "text", nullable: true),
                    salary = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    office_location = table.Column<string>(type: "text", nullable: true),
                    working_hours = table.Column<string>(type: "text", nullable: true),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_degree = table.Column<string>(type: "text", nullable: true),
                    academic_title = table.Column<string>(type: "text", nullable: true),
                    hourly_rate = table.Column<decimal>(type: "numeric", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teachers", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teachers_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_teachers_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "subject_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    total_course_instances = table.Column<int>(type: "integer", nullable: false),
                    active_course_instances = table.Column<int>(type: "integer", nullable: false),
                    total_students_enrolled = table.Column<int>(type: "integer", nullable: false),
                    average_grade = table.Column<decimal>(type: "numeric", nullable: false),
                    completion_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    total_assignments = table.Column<int>(type: "integer", nullable: false),
                    total_lessons = table.Column<int>(type: "integer", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_subject_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_subject_analytics_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "faculties",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    dean_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faculties", x => x.uid);
                    table.ForeignKey(
                        name: "fk_faculties_teachers_dean_uid",
                        column: x => x.dean_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "teacher_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    teacher_rating = table.Column<decimal>(type: "numeric", nullable: false),
                    review_count = table.Column<int>(type: "integer", nullable: false),
                    average_grade = table.Column<decimal>(type: "numeric", nullable: false),
                    average_attendance = table.Column<decimal>(type: "numeric", nullable: false),
                    average_exam_score = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_teacher_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_teacher_analytics_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_teacher_analytics_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "groups",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    year = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: false),
                    department_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    curator_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    curator_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    faculty_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_groups", x => x.uid);
                    table.ForeignKey(
                        name: "fk_groups_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_groups_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_groups_departments_department_uid",
                        column: x => x.department_uid,
                        principalTable: "departments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_groups_faculties_faculty_uid",
                        column: x => x.faculty_uid,
                        principalTable: "faculties",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_groups_teachers_curator_uid",
                        column: x => x.curator_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_groups_teachers_curator_uid1",
                        column: x => x.curator_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "group_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    average_gpa = table.Column<decimal>(type: "numeric", nullable: false),
                    average_attendance = table.Column<decimal>(type: "numeric", nullable: false),
                    average_exam_score = table.Column<decimal>(type: "numeric", nullable: false),
                    exam_pass_rate = table.Column<decimal>(type: "numeric", nullable: false),
                    average_age = table.Column<decimal>(type: "numeric", nullable: false),
                    male_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    current_course = table.Column<int>(type: "integer", nullable: false),
                    admission_year = table.Column<int>(type: "integer", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_group_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_group_analytics_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "students",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    person_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_code = table.Column<string>(type: "text", nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    graduation_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    gpa = table.Column<double>(type: "double precision", nullable: false),
                    academic_year = table.Column<int>(type: "integer", nullable: false),
                    group_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    curriculum_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    curriculum_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_students", x => x.uid);
                    table.ForeignKey(
                        name: "fk_students_curricula_curriculum_uid",
                        column: x => x.curriculum_uid,
                        principalTable: "curricula",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_students_curricula_curriculum_uid1",
                        column: x => x.curriculum_uid1,
                        principalTable: "curricula",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_students_groups_group_uid",
                        column: x => x.group_uid,
                        principalTable: "groups",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_students_persons_person_uid",
                        column: x => x.person_uid,
                        principalTable: "persons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "enrollments",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    enrollment_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completion_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    enrolled_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false),
                    final_grade = table.Column<decimal>(type: "numeric", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_enrollments", x => x.uid);
                    table.ForeignKey(
                        name: "fk_enrollments_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_enrollments_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "exam_results",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    exam_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    score = table.Column<decimal>(type: "numeric", nullable: false),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_absent = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_exam_results", x => x.uid);
                    table.ForeignKey(
                        name: "fk_exam_results_exams_exam_uid",
                        column: x => x.exam_uid,
                        principalTable: "exams",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exam_results_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_exam_results_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "lesson_progresses",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    lesson_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    completion_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    time_spent_minutes = table.Column<int>(type: "integer", nullable: false),
                    is_completed = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    rating = table.Column<int>(type: "integer", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    lesson_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_lesson_progresses", x => x.uid);
                    table.ForeignKey(
                        name: "fk_lesson_progresses_lessons_lesson_uid",
                        column: x => x.lesson_uid,
                        principalTable: "lessons",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_progresses_lessons_lesson_uid1",
                        column: x => x.lesson_uid1,
                        principalTable: "lessons",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_lesson_progresses_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_lesson_progresses_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "quiz_attempts",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    quiz_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_number = table.Column<int>(type: "integer", nullable: false),
                    started_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    completed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: true),
                    max_score = table.Column<double>(type: "double precision", nullable: false),
                    percentage = table.Column<double>(type: "double precision", nullable: true),
                    is_passed = table.Column<bool>(type: "boolean", nullable: true),
                    time_spent = table.Column<int>(type: "integer", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_attempts", x => x.uid);
                    table.ForeignKey(
                        name: "fk_quiz_attempts_quizzes_quiz_uid",
                        column: x => x.quiz_uid,
                        principalTable: "quizzes",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quiz_attempts_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quiz_attempts_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "student_analytics",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    academic_period_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    last_calculated = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    time_spent_minutes = table.Column<int>(type: "integer", nullable: false),
                    last_activity = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    attendance_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_student_analytics", x => x.uid);
                    table.ForeignKey(
                        name: "fk_student_analytics_academic_periods_academic_period_uid",
                        column: x => x.academic_period_uid,
                        principalTable: "academic_periods",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_student_analytics_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "grades",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    subject_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    teacher_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    exam_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    enrollment_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    value = table.Column<decimal>(type: "numeric", nullable: false),
                    comment = table.Column<string>(type: "text", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    issued_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_published = table.Column<bool>(type: "boolean", nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    max_value = table.Column<decimal>(type: "numeric", nullable: true),
                    weight = table.Column<decimal>(type: "numeric", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    teacher_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_grades", x => x.uid);
                    table.ForeignKey(
                        name: "fk_grades_assignments_assignment_uid",
                        column: x => x.assignment_uid,
                        principalTable: "assignments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_course_instances_course_instance_uid",
                        column: x => x.course_instance_uid,
                        principalTable: "course_instances",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_enrollments_enrollment_uid",
                        column: x => x.enrollment_uid,
                        principalTable: "enrollments",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_grades_exams_exam_uid",
                        column: x => x.exam_uid,
                        principalTable: "exams",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_grades_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_grades_subjects_subject_uid",
                        column: x => x.subject_uid,
                        principalTable: "subjects",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid",
                        column: x => x.teacher_uid,
                        principalTable: "teachers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_grades_teachers_teacher_uid1",
                        column: x => x.teacher_uid1,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.CreateTable(
                name: "quiz_student_answers",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    attempt_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    question_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    selected_answer_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    text_answer = table.Column<string>(type: "text", nullable: true),
                    is_correct = table.Column<bool>(type: "boolean", nullable: true),
                    points_earned = table.Column<double>(type: "double precision", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_quiz_student_answers", x => x.uid);
                    table.ForeignKey(
                        name: "fk_quiz_student_answers_quiz_answers_selected_answer_uid",
                        column: x => x.selected_answer_uid,
                        principalTable: "quiz_answers",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_quiz_student_answers_quiz_attempts_attempt_uid",
                        column: x => x.attempt_uid,
                        principalTable: "quiz_attempts",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_quiz_student_answers_quiz_questions_question_uid",
                        column: x => x.question_uid,
                        principalTable: "quiz_questions",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "submissions",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    student_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    assignment_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    submission_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    content = table.Column<string>(type: "text", nullable: true),
                    file_path = table.Column<string>(type: "text", nullable: true),
                    submitted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    score = table.Column<double>(type: "double precision", nullable: true),
                    feedback = table.Column<string>(type: "text", nullable: true),
                    graded_by_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    graded_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    graded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false),
                    grade_uid = table.Column<Guid>(type: "uuid", nullable: true),
                    student_uid1 = table.Column<Guid>(type: "uuid", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_submissions", x => x.uid);
                    table.ForeignKey(
                        name: "fk_submissions_assignments_assignment_uid",
                        column: x => x.assignment_uid,
                        principalTable: "assignments",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submissions_grades_grade_uid",
                        column: x => x.grade_uid,
                        principalTable: "grades",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_submissions_students_student_uid",
                        column: x => x.student_uid,
                        principalTable: "students",
                        principalColumn: "uid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_submissions_students_student_uid1",
                        column: x => x.student_uid1,
                        principalTable: "students",
                        principalColumn: "uid");
                    table.ForeignKey(
                        name: "fk_submissions_teachers_graded_by_uid",
                        column: x => x.graded_by_uid,
                        principalTable: "teachers",
                        principalColumn: "uid");
                });

            migrationBuilder.InsertData(
                table: "academic_periods",
                columns: new[] { "uid", "academic_year", "code", "created_at", "deleted_at", "description", "end_date", "is_active", "is_current", "is_deleted", "last_modified_at", "name", "start_date", "status", "type" },
                values: new object[,]
                {
                    { new Guid("aa111111-1111-1111-1111-111111111111"), 2024, "FALL2024", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2024, 12, 31, 0, 0, 0, 0, DateTimeKind.Utc), true, false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Осенний семестр 2024", new DateTime(2024, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1 },
                    { new Guid("aa222222-2222-2222-2222-222222222222"), 2024, "SPRING2025", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, new DateTime(2025, 6, 30, 0, 0, 0, 0, DateTimeKind.Utc), false, false, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Весенний семестр 2025", new DateTime(2025, 2, 1, 0, 0, 0, 0, DateTimeKind.Utc), 0, 1 }
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "uid", "code", "created_at", "deleted_at", "description", "faculty_uid", "head_of_department_uid", "is_active", "is_deleted", "last_modified_at", "name", "parent_department_uid" },
                values: new object[,]
                {
                    { new Guid("d1111111-1111-1111-1111-111111111111"), "IT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Кафедра информационных технологий и программирования", null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии", null },
                    { new Guid("d2222222-2222-2222-2222-222222222222"), "MATH", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Кафедра математики и статистики", null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Математика", null },
                    { new Guid("d3333333-3333-3333-3333-333333333333"), "PHYS", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Кафедра физики и естественных наук", null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Физика", null },
                    { new Guid("d4444444-4444-4444-4444-444444444444"), "LANG", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Кафедра иностранных языков", null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Иностранные языки", null },
                    { new Guid("d5555555-5555-5555-5555-555555555555"), "ECON", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Кафедра экономики и менеджмента", null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Экономика", null }
                });

            migrationBuilder.InsertData(
                table: "notification_templates",
                columns: new[] { "uid", "category", "created_at", "deleted_at", "description", "is_active", "is_deleted", "last_modified_at", "message_template", "name", "parameters_json", "priority", "title_template", "type" },
                values: new object[,]
                {
                    { new Guid("b1111111-1111-1111-1111-111111111111"), "Welcome", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Здравствуйте, {StudentName}! Добро пожаловать в систему управления обучением Viridisca LMS. Ваш логин: {Username}", "WelcomeStudent", null, 1, "Добро пожаловать в Viridisca LMS!", 0 },
                    { new Guid("b2222222-2222-2222-2222-222222222222"), "Academic", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Здравствуйте, {StudentName}! По предмету '{CourseName}' выставлена новая оценка: {Grade}", "GradePublished", null, 1, "Новая оценка", 0 },
                    { new Guid("b3333333-3333-3333-3333-333333333333"), "Reminder", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Здравствуйте, {StudentName}! Напоминаем, что задание '{AssignmentName}' должно быть сдано до {DueDate}", "AssignmentDue", null, 1, "Напоминание о задании", 0 }
                });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "uid", "created_at", "deleted_at", "description", "display_name", "is_deleted", "last_modified_at", "name" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Неограниченный доступ ко всем функциям", "Полный доступ к системе", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SystemAdmin.FullAccess" },
                    { new Guid("10000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр списка пользователей", "Просмотр пользователей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.View" },
                    { new Guid("10000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Создание новых пользователей", "Создание пользователей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.Create" },
                    { new Guid("10000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Изменение данных пользователей", "Редактирование пользователей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.Edit" },
                    { new Guid("10000005-0000-0000-0000-000000000005"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Удаление пользователей из системы", "Удаление пользователей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Users.Delete" },
                    { new Guid("20000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр списка студентов", "Просмотр студентов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.View" },
                    { new Guid("20000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Регистрация новых студентов", "Создание студентов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.Create" },
                    { new Guid("20000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Изменение данных студентов", "Редактирование студентов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.Edit" },
                    { new Guid("20000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Удаление студентов из системы", "Удаление студентов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Students.Delete" },
                    { new Guid("30000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр списка преподавателей", "Просмотр преподавателей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.View" },
                    { new Guid("30000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Регистрация новых преподавателей", "Создание преподавателей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.Create" },
                    { new Guid("30000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Изменение данных преподавателей", "Редактирование преподавателей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.Edit" },
                    { new Guid("30000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Удаление преподавателей из системы", "Удаление преподавателей", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teachers.Delete" },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр списка курсов", "Просмотр курсов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.View" },
                    { new Guid("40000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Создание новых курсов", "Создание курсов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.Create" },
                    { new Guid("40000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Изменение курсов", "Редактирование курсов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.Edit" },
                    { new Guid("40000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Удаление курсов", "Удаление курсов", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Courses.Delete" },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр оценок студентов", "Просмотр оценок", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.View" },
                    { new Guid("50000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Создание новых оценок", "Выставление оценок", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.Create" },
                    { new Guid("50000003-0000-0000-0000-000000000003"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Изменение оценок", "Редактирование оценок", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.Edit" },
                    { new Guid("50000004-0000-0000-0000-000000000004"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Удаление оценок", "Удаление оценок", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Grades.Delete" },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр библиотечных ресурсов", "Просмотр библиотеки", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Library.View" },
                    { new Guid("60000002-0000-0000-0000-000000000002"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Управление библиотечными ресурсами", "Управление библиотекой", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Library.Manage" }
                });

            migrationBuilder.InsertData(
                table: "persons",
                columns: new[] { "uid", "address", "created_at", "date_of_birth", "deleted_at", "email", "first_name", "is_active", "is_deleted", "last_modified_at", "last_name", "middle_name", "phone", "phone_number", "profile_image_url" },
                values: new object[,]
                {
                    { new Guid("11111111-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1985, 5, 15, 0, 0, 0, 0, DateTimeKind.Utc), null, "a.petrova@viridisca.edu", "Анна", true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Петрова", "Сергеевна", "+7 (999) 234-56-78", "+7 (999) 234-56-78", null },
                    { new Guid("22222222-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 8, 20, 0, 0, 0, 0, DateTimeKind.Utc), null, "i.ivanov@student.viridisca.edu", "Иван", true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Иванов", "Петрович", "+7 (999) 345-67-89", "+7 (999) 345-67-89", null },
                    { new Guid("33333333-0000-0000-0000-000000000002"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(2003, 12, 10, 0, 0, 0, 0, DateTimeKind.Utc), null, "m.sidorova@student.viridisca.edu", "Мария", true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Сидорова", "Александровна", "+7 (999) 456-78-90", "+7 (999) 456-78-90", null },
                    { new Guid("aaaabbbb-0000-0000-0000-000000000001"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1980, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "admin@viridisca.edu", "Системный", true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Администратор", null, "+7 (999) 123-45-67", "+7 (999) 123-45-67", null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "uid", "created_at", "deleted_at", "description", "display_name", "is_deleted", "last_modified_at", "name", "role_type" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Полный доступ ко всем функциям системы", "Системный администратор", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "SystemAdmin", null },
                    { new Guid("22222222-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Управление академическими процессами", "Начальник учебной части", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AcademicAffairsHead", null },
                    { new Guid("33333333-3333-3333-3333-333333333333"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Управление кафедрой и её ресурсами", "Заведующий кафедрой", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "DepartmentHead", null },
                    { new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Ведение занятий и оценивание студентов", "Преподаватель", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Teacher", null },
                    { new Guid("55555555-5555-5555-5555-555555555555"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Помощь в ведении занятий", "Ассистент преподавателя", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "AssistantTeacher", null },
                    { new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Обучение в системе", "Студент", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Student", null },
                    { new Guid("77777777-7777-7777-7777-777777777777"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Просмотр успеваемости ребёнка", "Родитель", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Parent", null },
                    { new Guid("88888888-8888-8888-8888-888888888888"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Управление библиотечными ресурсами", "Библиотекарь", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Librarian", null },
                    { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Техническая поддержка системы", "IT поддержка", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "ITSupport", null },
                    { new Guid("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "Финансовые операции", "Бухгалтер", false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Accountant", null }
                });

            migrationBuilder.InsertData(
                table: "system_settings",
                columns: new[] { "uid", "category", "created_at", "data_type", "deleted_at", "description", "is_deleted", "is_system", "key", "last_modified_at", "value" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Название системы", false, false, "System.Name", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Viridisca LMS" },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Версия системы", false, false, "System.Version", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "1.0.0" },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Количество кредитов по умолчанию для предмета", false, false, "Academic.DefaultCredits", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "3" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Максимальная оценка в системе", false, false, "Academic.MaxGrade", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "5.0" },
                    { new Guid("a5555555-5555-5555-5555-555555555555"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Минимальная проходная оценка", false, false, "Academic.MinPassingGrade", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "2.5" },
                    { new Guid("a6666666-6666-6666-6666-666666666666"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Максимальный срок займа библиотечного ресурса (дни)", false, false, "Library.MaxLoanDays", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "30" },
                    { new Guid("a7777777-7777-7777-7777-777777777777"), null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "string", null, "Включены ли email уведомления", false, false, "Notification.EmailEnabled", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "true" }
                });

            migrationBuilder.InsertData(
                table: "accounts",
                columns: new[] { "uid", "created_at", "deleted_at", "failed_login_attempts", "is_active", "is_deleted", "is_email_confirmed", "is_locked", "last_failed_login_at", "last_login_at", "last_modified_at", "locked_until", "password_hash", "person_uid", "username" },
                values: new object[,]
                {
                    { new Guid("aaaabbbb-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, true, false, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$FbdQ4y1BnYxWQsfAsVQUcOmKdwGIU0VL5ouvx5IYWLPoxHR.RvjLO", new Guid("aaaabbbb-0000-0000-0000-000000000001"), "admin" },
                    { new Guid("bbbbbbbb-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, true, false, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$RBY3exH72Ob74lLQ1rwcYOY0emR.sn6DoLb4NyMkIIPLKU7FNCa2e", new Guid("11111111-0000-0000-0000-000000000001"), "a.petrova" },
                    { new Guid("cccccccc-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, true, false, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$F/6mqob2KBlb7qD5yP5KnubVuHvFa0USRf.sNErKgusui1JjGnOmO", new Guid("22222222-0000-0000-0000-000000000001"), "i.ivanov" },
                    { new Guid("dddddddd-1111-1111-1111-111111111111"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 0, true, false, true, false, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, "$2a$11$0SNmWq8XnuFQmRA0XX8acuiA17NPY0EzNqCdwhPL4lkbTcTMvBHsC", new Guid("33333333-0000-0000-0000-000000000002"), "m.sidorova" }
                });

            migrationBuilder.InsertData(
                table: "curricula",
                columns: new[] { "uid", "academic_year", "code", "created_at", "deleted_at", "department_uid", "description", "duration_in_semesters", "duration_months", "duration_semesters", "end_year", "is_active", "is_deleted", "last_modified_at", "name", "start_year", "total_credits", "valid_from", "valid_to" },
                values: new object[] { new Guid("eeeeeeee-0000-0000-0000-000000000001"), 0, "IT-2021", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d1111111-1111-1111-1111-111111111111"), "Учебный план по направлению Информационные технологии", 8, 48, 0, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Информационные технологии 2021", 0, 240, new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null });

            migrationBuilder.InsertData(
                table: "groups",
                columns: new[] { "uid", "academic_period_uid", "code", "created_at", "curator_uid", "curator_uid1", "curriculum_uid", "deleted_at", "department_uid", "description", "end_date", "faculty_uid", "is_active", "is_deleted", "last_modified_at", "max_students", "name", "start_date", "status", "year" },
                values: new object[] { new Guid("dddddddd-0000-0000-0000-000000000001"), null, "IT-21-1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new Guid("d1111111-1111-1111-1111-111111111111"), null, null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 30, "Информационные технологии 2021, группа 1", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 0 });

            migrationBuilder.InsertData(
                table: "person_roles",
                columns: new[] { "uid", "assigned_at", "assigned_by", "context", "context_entity_type", "context_entity_uid", "created_at", "deleted_at", "expires_at", "is_active", "is_deleted", "last_modified_at", "person_uid", "role_uid" },
                values: new object[,]
                {
                    { new Guid("aaaabbbb-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("aaaabbbb-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111") },
                    { new Guid("bbbbbbbb-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("11111111-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444") },
                    { new Guid("cccccccc-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666") },
                    { new Guid("dddddddd-2222-2222-2222-222222222222"), new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-0000-0000-0000-000000000002"), new Guid("66666666-6666-6666-6666-666666666666") }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_uid", "role_uid", "created_at", "deleted_at", "is_deleted", "last_modified_at", "uid" },
                values: new object[,]
                {
                    { new Guid("10000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(7882), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(7883), new Guid("9c354cf4-f26b-41c8-868a-7eb887f6f5d6") },
                    { new Guid("10000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8143), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8144), new Guid("4bb10046-5f77-427b-b667-291be866cd73") },
                    { new Guid("10000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8145), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8145), new Guid("a8c9fc38-cb82-480d-bccf-98905968478a") },
                    { new Guid("10000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8146), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8146), new Guid("14c47109-399c-48dd-b3f7-fdcaf7a906c7") },
                    { new Guid("10000005-0000-0000-0000-000000000005"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8147), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8147), new Guid("c95e41c1-b5e3-4faf-96ac-6d238bf3f807") },
                    { new Guid("20000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8149), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8149), new Guid("3cc4b136-aa60-4f25-b7d7-4643d2c319cb") },
                    { new Guid("20000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8149), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8150), new Guid("873a9699-30b6-49e7-bc0d-27e28b07ba4e") },
                    { new Guid("20000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8153), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8154), new Guid("1dd81e6c-e4d0-4f76-82dc-591900ebd879") },
                    { new Guid("20000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8154), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8155), new Guid("bb8fc3a0-37d1-4433-9b3c-97e443e4c461") },
                    { new Guid("30000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8156), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8156), new Guid("c50dabbb-26ab-474f-a435-55a2a4173267") },
                    { new Guid("30000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8157), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8157), new Guid("d78921b2-6d28-43ed-bf9c-5ddbe5d61aa7") },
                    { new Guid("30000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8157), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8158), new Guid("993be324-9130-4822-9afa-098f941de441") },
                    { new Guid("30000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8158), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8159), new Guid("7c298c38-34ab-462c-955b-e049a7d614ef") },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8159), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8159), new Guid("a662e686-8f37-46ac-96cb-4fd538eb79ab") },
                    { new Guid("40000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8160), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8160), new Guid("d04c5cda-48a6-4f6f-bd3c-d1a2713d8596") },
                    { new Guid("40000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8162), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8162), new Guid("2596cb35-1c04-459e-a297-e42e98fe29ec") },
                    { new Guid("40000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8163), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8163), new Guid("5b462905-2a62-454a-8540-6a6a66540502") },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8164), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8164), new Guid("699708ae-5318-47ef-a9dc-6e1228d0bd61") },
                    { new Guid("50000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8165), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8165), new Guid("13bfc54a-ba7c-4d58-a22c-df91071157f8") },
                    { new Guid("50000003-0000-0000-0000-000000000003"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8166), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8166), new Guid("e468145b-0112-4015-983e-d2f8bfbd5d45") },
                    { new Guid("50000004-0000-0000-0000-000000000004"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8166), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8167), new Guid("2a0693e0-9e69-4b86-a3c6-b3a55e91708b") },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8169), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8170), new Guid("d550e402-e5c4-4ba0-a038-2ad47d487c62") },
                    { new Guid("60000002-0000-0000-0000-000000000002"), new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8171), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8171), new Guid("ff63ca9c-4ffa-44a8-892d-e025a9c3f41e") },
                    { new Guid("20000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8177), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8177), new Guid("50db6877-1542-4690-8c34-3d7d705f0cc5") },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8179), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8179), new Guid("1e34c511-78da-4f62-9875-99e97db2e2cd") },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8180), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8180), new Guid("aa78addb-b1a4-449e-82da-9cf171d582d4") },
                    { new Guid("50000002-0000-0000-0000-000000000002"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8181), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8181), new Guid("da4bb545-cbdf-4c25-b605-a439f21c2c7d") },
                    { new Guid("50000003-0000-0000-0000-000000000003"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8182), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8182), new Guid("ea32137d-9933-4136-b993-16ad1b242f6a") },
                    { new Guid("50000004-0000-0000-0000-000000000004"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8183), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8183), new Guid("64cb3a07-8216-466a-8cd6-d5e97d9bb09a") },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new Guid("44444444-4444-4444-4444-444444444444"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8184), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8184), new Guid("e8bb4a4e-3a95-4e4f-b726-b9b3ddfae2ec") },
                    { new Guid("40000001-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8187), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8187), new Guid("972bdf15-cbf3-4cf5-b183-17d284189b65") },
                    { new Guid("50000001-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8190), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8190), new Guid("6a666ed2-caa6-4901-b5c5-280d89bdaddb") },
                    { new Guid("60000001-0000-0000-0000-000000000001"), new Guid("66666666-6666-6666-6666-666666666666"), new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8191), null, false, new DateTime(2025, 6, 20, 16, 6, 13, 528, DateTimeKind.Utc).AddTicks(8191), new Guid("ee37e7fd-487e-4559-88a3-419a7bddc576") }
                });

            migrationBuilder.InsertData(
                table: "subjects",
                columns: new[] { "uid", "category", "code", "created_at", "credits", "curriculum_uid", "deleted_at", "department_uid", "description", "hours", "is_active", "is_deleted", "last_modified_at", "learning_outcomes", "lessons_per_week", "name", "prerequisites", "type" },
                values: new object[,]
                {
                    { new Guid("aaaaaaaa-0000-0000-0000-000000000001"), null, "CS101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4, null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "Введение в программирование на C#", 1, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Основы программирования", null, 1 },
                    { new Guid("bbbbbbbb-0000-0000-0000-000000000002"), null, "MATH201", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 5, null, null, new Guid("d2222222-2222-2222-2222-222222222222"), "Математический анализ и линейная алгебра", 1, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Высшая математика", null, 1 },
                    { new Guid("cccccccc-0000-0000-0000-000000000003"), null, "ENG101", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3, null, null, new Guid("d4444444-4444-4444-4444-444444444444"), "Базовый курс английского языка", 1, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, 1, "Английский язык", null, 2 }
                });

            migrationBuilder.InsertData(
                table: "teachers",
                columns: new[] { "uid", "academic_degree", "academic_title", "created_at", "deleted_at", "department_uid", "employee_code", "hire_date", "hourly_rate", "is_active", "is_deleted", "last_modified_at", "office_location", "person_uid", "qualification", "salary", "specialization", "termination_date", "working_hours" },
                values: new object[] { new Guid("bbbbbbbb-3333-3333-3333-333333333333"), null, null, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("d1111111-1111-1111-1111-111111111111"), "T001", new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), null, true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, new Guid("11111111-0000-0000-0000-000000000001"), "Кандидат технических наук", 75000m, null, null, null });

            migrationBuilder.InsertData(
                table: "curriculum_subjects",
                columns: new[] { "curriculum_uid", "subject_uid", "created_at", "credits", "deleted_at", "is_deleted", "is_mandatory", "is_required", "last_modified_at", "semester", "uid" },
                values: new object[,]
                {
                    { new Guid("eeeeeeee-0000-0000-0000-000000000001"), new Guid("aaaaaaaa-0000-0000-0000-000000000001"), new DateTime(2025, 6, 20, 16, 6, 14, 62, DateTimeKind.Utc).AddTicks(9737), 4, null, false, true, true, new DateTime(2025, 6, 20, 16, 6, 14, 62, DateTimeKind.Utc).AddTicks(9737), 1, new Guid("cbb8ba79-2894-424b-b382-b664405cea65") },
                    { new Guid("eeeeeeee-0000-0000-0000-000000000001"), new Guid("bbbbbbbb-0000-0000-0000-000000000002"), new DateTime(2025, 6, 20, 16, 6, 14, 63, DateTimeKind.Utc).AddTicks(239), 5, null, false, true, true, new DateTime(2025, 6, 20, 16, 6, 14, 63, DateTimeKind.Utc).AddTicks(240), 1, new Guid("ae97f61b-2889-4fc5-9606-b3bda29b257e") },
                    { new Guid("eeeeeeee-0000-0000-0000-000000000001"), new Guid("cccccccc-0000-0000-0000-000000000003"), new DateTime(2025, 6, 20, 16, 6, 14, 63, DateTimeKind.Utc).AddTicks(256), 3, null, false, false, false, new DateTime(2025, 6, 20, 16, 6, 14, 63, DateTimeKind.Utc).AddTicks(256), 2, new Guid("73d2d32a-f835-4940-ba72-3993d7fbebda") }
                });

            migrationBuilder.InsertData(
                table: "students",
                columns: new[] { "uid", "academic_year", "created_at", "curriculum_uid", "curriculum_uid1", "deleted_at", "enrollment_date", "gpa", "graduation_date", "group_uid", "is_active", "is_deleted", "last_modified_at", "person_uid", "status", "student_code" },
                values: new object[,]
                {
                    { new Guid("aaaabbbb-0000-0000-0000-000000000002"), 2025, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("eeeeeeee-0000-0000-0000-000000000001"), null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 3.7999999999999998, null, new Guid("dddddddd-0000-0000-0000-000000000001"), true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("33333333-0000-0000-0000-000000000002"), 1, "S2021002" },
                    { new Guid("ffffffff-0000-0000-0000-000000000001"), 2025, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("eeeeeeee-0000-0000-0000-000000000001"), null, null, new DateTime(2021, 9, 1, 0, 0, 0, 0, DateTimeKind.Utc), 4.2000000000000002, null, new Guid("dddddddd-0000-0000-0000-000000000001"), true, false, new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("22222222-0000-0000-0000-000000000001"), 1, "S2021001" }
                });

            migrationBuilder.CreateIndex(
                name: "ix_academic_periods_academic_year_type",
                table: "academic_periods",
                columns: new[] { "academic_year", "type" });

            migrationBuilder.CreateIndex(
                name: "ix_academic_periods_code",
                table: "academic_periods",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_person_uid",
                table: "accounts",
                column: "person_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_accounts_username",
                table: "accounts",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignment_analytics_assignment_uid",
                table: "assignment_analytics",
                column: "assignment_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_assignments_course_instance_uid",
                table: "assignments",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_assignments_lesson_uid",
                table: "assignments",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_lesson_uid",
                table: "attendances",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_lesson_uid1",
                table: "attendances",
                column: "lesson_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_attendances_student_uid_lesson_uid",
                table: "attendances",
                columns: new[] { "student_uid", "lesson_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_course_analytics_course_instance_uid",
                table: "course_analytics",
                column: "course_instance_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_course_analytics_course_uid",
                table: "course_analytics",
                column: "course_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_academic_period_uid",
                table: "course_instances",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_course_uid",
                table: "course_instances",
                column: "course_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_group_uid",
                table: "course_instances",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_subject_uid_group_uid_academic_period_uid",
                table: "course_instances",
                columns: new[] { "subject_uid", "group_uid", "academic_period_uid" });

            migrationBuilder.CreateIndex(
                name: "ix_course_instances_teacher_uid",
                table: "course_instances",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_courses_code",
                table: "courses",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_courses_department_uid",
                table: "courses",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_courses_subject_uid",
                table: "courses",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_curricula_code",
                table: "curricula",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_curricula_department_uid",
                table: "curricula",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_curriculum_subjects_subject_uid",
                table: "curriculum_subjects",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_departments_code",
                table: "departments",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_departments_faculty_uid",
                table: "departments",
                column: "faculty_uid");

            migrationBuilder.CreateIndex(
                name: "ix_departments_head_of_department_uid",
                table: "departments",
                column: "head_of_department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_departments_parent_department_uid",
                table: "departments",
                column: "parent_department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_discussion_posts_author_uid",
                table: "discussion_posts",
                column: "author_uid");

            migrationBuilder.CreateIndex(
                name: "ix_discussion_posts_discussion_uid_created_at",
                table: "discussion_posts",
                columns: new[] { "discussion_uid", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_discussion_posts_parent_post_uid",
                table: "discussion_posts",
                column: "parent_post_uid");

            migrationBuilder.CreateIndex(
                name: "ix_discussions_course_instance_uid_title",
                table: "discussions",
                columns: new[] { "course_instance_uid", "title" });

            migrationBuilder.CreateIndex(
                name: "ix_discussions_course_instance_uid1",
                table: "discussions",
                column: "course_instance_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_discussions_created_by_uid",
                table: "discussions",
                column: "created_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_course_instance_uid",
                table: "enrollments",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_enrollments_student_uid_course_instance_uid",
                table: "enrollments",
                columns: new[] { "student_uid", "course_instance_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_analytics_exam_uid",
                table: "exam_analytics",
                column: "exam_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_results_exam_uid_student_uid",
                table: "exam_results",
                columns: new[] { "exam_uid", "student_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_exam_results_student_uid",
                table: "exam_results",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exam_results_student_uid1",
                table: "exam_results",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_exams_academic_period_uid",
                table: "exams",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_course_instance_uid",
                table: "exams",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_exams_course_instance_uid1",
                table: "exams",
                column: "course_instance_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_faculties_code",
                table: "faculties",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_faculties_dean_uid",
                table: "faculties",
                column: "dean_uid");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_file_path",
                table: "file_records",
                column: "file_path");

            migrationBuilder.CreateIndex(
                name: "ix_file_records_uploaded_by_uid",
                table: "file_records",
                column: "uploaded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_assignment_uid",
                table: "grades",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_course_instance_uid",
                table: "grades",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_enrollment_uid",
                table: "grades",
                column: "enrollment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_exam_uid",
                table: "grades",
                column: "exam_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_student_uid",
                table: "grades",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_subject_uid",
                table: "grades",
                column: "subject_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_teacher_uid",
                table: "grades",
                column: "teacher_uid");

            migrationBuilder.CreateIndex(
                name: "ix_grades_teacher_uid1",
                table: "grades",
                column: "teacher_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_group_analytics_group_uid",
                table: "group_analytics",
                column: "group_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_groups_academic_period_uid",
                table: "groups",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_code",
                table: "groups",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_groups_curator_uid",
                table: "groups",
                column: "curator_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_curator_uid1",
                table: "groups",
                column: "curator_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_groups_curriculum_uid",
                table: "groups",
                column: "curriculum_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_department_uid",
                table: "groups",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_groups_faculty_uid",
                table: "groups",
                column: "faculty_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_analytics_lesson_uid",
                table: "lesson_analytics",
                column: "lesson_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_lesson_uid",
                table: "lesson_progresses",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_lesson_uid1",
                table: "lesson_progresses",
                column: "lesson_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_student_uid_lesson_uid",
                table: "lesson_progresses",
                columns: new[] { "student_uid", "lesson_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_lesson_progresses_student_uid1",
                table: "lesson_progresses",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_lessons_course_instance_uid",
                table: "lessons",
                column: "course_instance_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_person_uid",
                table: "library_loans",
                column: "person_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_loans_resource_uid",
                table: "library_loans",
                column: "resource_uid");

            migrationBuilder.CreateIndex(
                name: "ix_library_resources_isbn",
                table: "library_resources",
                column: "isbn");

            migrationBuilder.CreateIndex(
                name: "ix_notification_settings_person_uid",
                table: "notification_settings",
                column: "person_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notification_templates_name",
                table: "notification_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_notifications_person_uid_created_at",
                table: "notifications",
                columns: new[] { "person_uid", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_notifications_template_uid",
                table: "notifications",
                column: "template_uid");

            migrationBuilder.CreateIndex(
                name: "ix_permissions_name",
                table: "permissions",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_person_roles_person_uid_role_uid_context_entity_uid_context",
                table: "person_roles",
                columns: new[] { "person_uid", "role_uid", "context_entity_uid", "context_entity_type" });

            migrationBuilder.CreateIndex(
                name: "ix_person_roles_role_uid",
                table: "person_roles",
                column: "role_uid");

            migrationBuilder.CreateIndex(
                name: "ix_persons_email",
                table: "persons",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_post_likes_person_uid",
                table: "post_likes",
                column: "person_uid");

            migrationBuilder.CreateIndex(
                name: "ix_post_likes_post_uid_person_uid",
                table: "post_likes",
                columns: new[] { "post_uid", "person_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quiz_analytics_quiz_uid",
                table: "quiz_analytics",
                column: "quiz_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quiz_answers_question_uid_order",
                table: "quiz_answers",
                columns: new[] { "question_uid", "order" });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_quiz_uid_student_uid_attempt_number",
                table: "quiz_attempts",
                columns: new[] { "quiz_uid", "student_uid", "attempt_number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_student_uid",
                table: "quiz_attempts",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_attempts_student_uid1",
                table: "quiz_attempts",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_questions_quiz_uid_order",
                table: "quiz_questions",
                columns: new[] { "quiz_uid", "order" });

            migrationBuilder.CreateIndex(
                name: "ix_quiz_student_answers_attempt_uid_question_uid",
                table: "quiz_student_answers",
                columns: new[] { "attempt_uid", "question_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_quiz_student_answers_question_uid",
                table: "quiz_student_answers",
                column: "question_uid");

            migrationBuilder.CreateIndex(
                name: "ix_quiz_student_answers_selected_answer_uid",
                table: "quiz_student_answers",
                column: "selected_answer_uid");

            migrationBuilder.CreateIndex(
                name: "ix_quizzes_course_instance_uid_title",
                table: "quizzes",
                columns: new[] { "course_instance_uid", "title" });

            migrationBuilder.CreateIndex(
                name: "ix_quizzes_course_instance_uid1",
                table: "quizzes",
                column: "course_instance_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_quizzes_lesson_uid",
                table: "quizzes",
                column: "lesson_uid");

            migrationBuilder.CreateIndex(
                name: "ix_quizzes_lesson_uid1",
                table: "quizzes",
                column: "lesson_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_role_permissions_permission_uid",
                table: "role_permissions",
                column: "permission_uid");

            migrationBuilder.CreateIndex(
                name: "ix_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_schedule_slots_course_instance_uid_day_of_week_start_time",
                table: "schedule_slots",
                columns: new[] { "course_instance_uid", "day_of_week", "start_time" });

            migrationBuilder.CreateIndex(
                name: "ix_student_analytics_academic_period_uid",
                table: "student_analytics",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_student_analytics_student_uid_academic_period_uid",
                table: "student_analytics",
                columns: new[] { "student_uid", "academic_period_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_curriculum_uid",
                table: "students",
                column: "curriculum_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_curriculum_uid1",
                table: "students",
                column: "curriculum_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_students_group_uid",
                table: "students",
                column: "group_uid");

            migrationBuilder.CreateIndex(
                name: "ix_students_person_uid",
                table: "students",
                column: "person_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_students_student_code",
                table: "students",
                column: "student_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subject_analytics_subject_uid",
                table: "subject_analytics",
                column: "subject_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_code",
                table: "subjects",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_subjects_curriculum_uid",
                table: "subjects",
                column: "curriculum_uid");

            migrationBuilder.CreateIndex(
                name: "ix_subjects_department_uid",
                table: "subjects",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_assignment_uid",
                table: "submissions",
                column: "assignment_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_grade_uid",
                table: "submissions",
                column: "grade_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_graded_by_uid",
                table: "submissions",
                column: "graded_by_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_student_uid",
                table: "submissions",
                column: "student_uid");

            migrationBuilder.CreateIndex(
                name: "ix_submissions_student_uid1",
                table: "submissions",
                column: "student_uid1");

            migrationBuilder.CreateIndex(
                name: "ix_system_settings_key",
                table: "system_settings",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teacher_analytics_academic_period_uid",
                table: "teacher_analytics",
                column: "academic_period_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teacher_analytics_teacher_uid_academic_period_uid",
                table: "teacher_analytics",
                columns: new[] { "teacher_uid", "academic_period_uid" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_department_uid",
                table: "teachers",
                column: "department_uid");

            migrationBuilder.CreateIndex(
                name: "ix_teachers_employee_code",
                table: "teachers",
                column: "employee_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_teachers_person_uid",
                table: "teachers",
                column: "person_uid",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_assignment_analytics_assignments_assignment_uid",
                table: "assignment_analytics",
                column: "assignment_uid",
                principalTable: "assignments",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_course_instances_course_instance_uid",
                table: "assignments",
                column: "course_instance_uid",
                principalTable: "course_instances",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_assignments_lessons_lesson_uid",
                table: "assignments",
                column: "lesson_uid",
                principalTable: "lessons",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_attendances_lessons_lesson_uid",
                table: "attendances",
                column: "lesson_uid",
                principalTable: "lessons",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_attendances_lessons_lesson_uid1",
                table: "attendances",
                column: "lesson_uid1",
                principalTable: "lessons",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_attendances_students_student_uid",
                table: "attendances",
                column: "student_uid",
                principalTable: "students",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_analytics_course_instances_course_instance_uid",
                table: "course_analytics",
                column: "course_instance_uid",
                principalTable: "course_instances",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_analytics_courses_course_uid",
                table: "course_analytics",
                column: "course_uid",
                principalTable: "courses",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_courses_course_uid",
                table: "course_instances",
                column: "course_uid",
                principalTable: "courses",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_groups_group_uid",
                table: "course_instances",
                column: "group_uid",
                principalTable: "groups",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_subjects_subject_uid",
                table: "course_instances",
                column: "subject_uid",
                principalTable: "subjects",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_course_instances_teachers_teacher_uid",
                table: "course_instances",
                column: "teacher_uid",
                principalTable: "teachers",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_courses_departments_department_uid",
                table: "courses",
                column: "department_uid",
                principalTable: "departments",
                principalColumn: "uid",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_courses_subjects_subject_uid",
                table: "courses",
                column: "subject_uid",
                principalTable: "subjects",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_curricula_departments_department_uid",
                table: "curricula",
                column: "department_uid",
                principalTable: "departments",
                principalColumn: "uid",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_curriculum_subjects_subjects_subject_uid",
                table: "curriculum_subjects",
                column: "subject_uid",
                principalTable: "subjects",
                principalColumn: "uid",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_departments_faculties_faculty_uid",
                table: "departments",
                column: "faculty_uid",
                principalTable: "faculties",
                principalColumn: "uid");

            migrationBuilder.AddForeignKey(
                name: "fk_departments_teachers_head_of_department_uid",
                table: "departments",
                column: "head_of_department_uid",
                principalTable: "teachers",
                principalColumn: "uid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_teachers_persons_person_uid",
                table: "teachers");

            migrationBuilder.DropForeignKey(
                name: "fk_departments_teachers_head_of_department_uid",
                table: "departments");

            migrationBuilder.DropForeignKey(
                name: "fk_faculties_teachers_dean_uid",
                table: "faculties");

            migrationBuilder.DropTable(
                name: "accounts");

            migrationBuilder.DropTable(
                name: "assignment_analytics");

            migrationBuilder.DropTable(
                name: "attendances");

            migrationBuilder.DropTable(
                name: "course_analytics");

            migrationBuilder.DropTable(
                name: "curriculum_subjects");

            migrationBuilder.DropTable(
                name: "exam_analytics");

            migrationBuilder.DropTable(
                name: "exam_results");

            migrationBuilder.DropTable(
                name: "file_records");

            migrationBuilder.DropTable(
                name: "group_analytics");

            migrationBuilder.DropTable(
                name: "lesson_analytics");

            migrationBuilder.DropTable(
                name: "lesson_progresses");

            migrationBuilder.DropTable(
                name: "library_loans");

            migrationBuilder.DropTable(
                name: "notification_settings");

            migrationBuilder.DropTable(
                name: "notifications");

            migrationBuilder.DropTable(
                name: "person_roles");

            migrationBuilder.DropTable(
                name: "post_likes");

            migrationBuilder.DropTable(
                name: "quiz_analytics");

            migrationBuilder.DropTable(
                name: "quiz_student_answers");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "schedule_slots");

            migrationBuilder.DropTable(
                name: "student_analytics");

            migrationBuilder.DropTable(
                name: "subject_analytics");

            migrationBuilder.DropTable(
                name: "submissions");

            migrationBuilder.DropTable(
                name: "system_settings");

            migrationBuilder.DropTable(
                name: "teacher_analytics");

            migrationBuilder.DropTable(
                name: "library_resources");

            migrationBuilder.DropTable(
                name: "notification_templates");

            migrationBuilder.DropTable(
                name: "discussion_posts");

            migrationBuilder.DropTable(
                name: "quiz_answers");

            migrationBuilder.DropTable(
                name: "quiz_attempts");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "grades");

            migrationBuilder.DropTable(
                name: "discussions");

            migrationBuilder.DropTable(
                name: "quiz_questions");

            migrationBuilder.DropTable(
                name: "assignments");

            migrationBuilder.DropTable(
                name: "enrollments");

            migrationBuilder.DropTable(
                name: "exams");

            migrationBuilder.DropTable(
                name: "quizzes");

            migrationBuilder.DropTable(
                name: "students");

            migrationBuilder.DropTable(
                name: "lessons");

            migrationBuilder.DropTable(
                name: "course_instances");

            migrationBuilder.DropTable(
                name: "courses");

            migrationBuilder.DropTable(
                name: "groups");

            migrationBuilder.DropTable(
                name: "subjects");

            migrationBuilder.DropTable(
                name: "academic_periods");

            migrationBuilder.DropTable(
                name: "curricula");

            migrationBuilder.DropTable(
                name: "persons");

            migrationBuilder.DropTable(
                name: "teachers");

            migrationBuilder.DropTable(
                name: "departments");

            migrationBuilder.DropTable(
                name: "faculties");
        }
    }
}
