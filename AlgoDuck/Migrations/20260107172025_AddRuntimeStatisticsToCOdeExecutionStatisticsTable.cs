using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoDuck.Migrations
{
    /// <inheritdoc />
    public partial class AddRuntimeStatisticsToCOdeExecutionStatisticsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "execution_timestamp",
                table: "code_execution_statistics");

            migrationBuilder.AddColumn<long>(
                name: "execution_end_ns",
                table: "code_execution_statistics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "execution_start_ns",
                table: "code_execution_statistics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "exit_code",
                table: "code_execution_statistics",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "jvm_peak_mem_kb",
                table: "code_execution_statistics",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<byte>(
                name: "type",
                table: "code_execution_statistics",
                type: "smallint",
                nullable: false,
                defaultValue: (byte)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "execution_end_ns",
                table: "code_execution_statistics");

            migrationBuilder.DropColumn(
                name: "execution_start_ns",
                table: "code_execution_statistics");

            migrationBuilder.DropColumn(
                name: "exit_code",
                table: "code_execution_statistics");

            migrationBuilder.DropColumn(
                name: "jvm_peak_mem_kb",
                table: "code_execution_statistics");

            migrationBuilder.DropColumn(
                name: "type",
                table: "code_execution_statistics");

            migrationBuilder.AddColumn<DateTime>(
                name: "execution_timestamp",
                table: "code_execution_statistics",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }
    }
}
