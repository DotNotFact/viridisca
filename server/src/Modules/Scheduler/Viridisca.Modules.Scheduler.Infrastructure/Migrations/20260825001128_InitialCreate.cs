using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viridisca.Modules.Scheduler.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "scheduler");

            migrationBuilder.CreateTable(
                name: "schedule_slots",
                schema: "scheduler",
                columns: table => new
                {
                    uid = table.Column<Guid>(type: "uuid", nullable: false),
                    course_instance_uid = table.Column<Guid>(type: "uuid", nullable: false),
                    day_of_week = table.Column<int>(type: "integer", nullable: false),
                    start_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    end_time = table.Column<TimeSpan>(type: "interval", nullable: false),
                    room = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    start_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    end_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    max_students = table.Column<int>(type: "integer", nullable: true),
                    created_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    last_modified_at_utc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_schedule_slots", x => x.uid);
                });

            migrationBuilder.CreateIndex(
                name: "ix_schedule_slots_course_instance_uid_day_of_week_start_time",
                schema: "scheduler",
                table: "schedule_slots",
                columns: new[] { "course_instance_uid", "day_of_week", "start_time" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "schedule_slots",
                schema: "scheduler");
        }
    }
}
