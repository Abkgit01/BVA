using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace VoucherAutomationSystem.Migrations
{
    public partial class Newmodules : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DeptId",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "RoleLead",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Banks",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BankId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Banks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CashAdvanceActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashAdvanceId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionPerformed = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashAdvanceActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashAdvanceActions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashAdvanceActions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Departments",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Departments", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "pettyCashActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PettyCashId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionPerformed = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pettyCashActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pettyCashActions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pettyCashActions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetirementPaymentActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetirementPaymentId = table.Column<int>(type: "int", nullable: false),
                    Comment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ActionPerformed = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementPaymentActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetirementPaymentActions_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RetirementPaymentActions_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationFlows",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeptId = table.Column<int>(type: "int", nullable: false),
                    Sort = table.Column<int>(type: "int", nullable: false),
                    Lead = table.Column<bool>(type: "bit", nullable: false),
                    isFinal = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationFlows", x => x.ID);
                    table.ForeignKey(
                        name: "FK_ApplicationFlows_Departments_DeptId",
                        column: x => x.DeptId,
                        principalTable: "Departments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashAdvances",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    Retired = table.Column<bool>(type: "bit", nullable: false),
                    ExchangeRate = table.Column<int>(type: "int", nullable: false),
                    DeptId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashAdvances", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashAdvances_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashAdvances_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CashAdvances_Departments_DeptId",
                        column: x => x.DeptId,
                        principalTable: "Departments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PettyCashes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    DeptId = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashes_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PettyCashes_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PettyCashes_Departments_DeptId",
                        column: x => x.DeptId,
                        principalTable: "Departments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetirementPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TotalRetirementAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CashAdvanceAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    IsCredit = table.Column<bool>(type: "bit", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DateUpdated = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    ExchangeRate = table.Column<int>(type: "int", nullable: false),
                    CashAdvanceId = table.Column<int>(type: "int", nullable: false),
                    DeptId = table.Column<int>(type: "int", nullable: false),
                    Currency = table.Column<int>(type: "int", nullable: false),
                    CurrentStage = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetirementPayments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RetirementPayments_Departments_DeptId",
                        column: x => x.DeptId,
                        principalTable: "Departments",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashAdvanceFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashAdvanceId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashAdvanceFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashAdvanceFiles_CashAdvances_CashAdvanceId",
                        column: x => x.CashAdvanceId,
                        principalTable: "CashAdvances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CashAdvancePayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CashAdvanceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CashAdvancePayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CashAdvancePayments_CashAdvances_CashAdvanceId",
                        column: x => x.CashAdvanceId,
                        principalTable: "CashAdvances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PettyCashApprovals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    PettyCashID = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PettyCashApprovals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PettyCashApprovals_PettyCashes_PettyCashID",
                        column: x => x.PettyCashID,
                        principalTable: "PettyCashes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pettyCashFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PettyCashId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pettyCashFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pettyCashFiles_PettyCashes_PettyCashId",
                        column: x => x.PettyCashId,
                        principalTable: "PettyCashes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetirementCashBookPayments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetirePaymentId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementCashBookPayments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetirementCashBookPayments_RetirementPayments_RetirePaymentId",
                        column: x => x.RetirePaymentId,
                        principalTable: "RetirementPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RetirementPaymentFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RetirementPaymentId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RetirementPaymentFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RetirementPaymentFiles_RetirementPayments_RetirementPaymentId",
                        column: x => x.RetirementPaymentId,
                        principalTable: "RetirementPayments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "35f44906-178d-45c2-ab8a-8e6445b98aee");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "e03996ba-c221-4ea6-9e98-16b72413eebc");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "ef84a1d2-ab8d-4428-ad49-230f8c1f3500");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "3c559ad2-c822-4743-b863-c0fb13b2f36b");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "78d5aafd-8c1d-4adf-ba17-d17cd475e1e8");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 6,
                column: "ConcurrencyStamp",
                value: "5823a063-ef18-4e32-8075-72861a22b954");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationFlows_DeptId",
                table: "ApplicationFlows",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvanceActions_RoleId",
                table: "CashAdvanceActions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvanceActions_UserId",
                table: "CashAdvanceActions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvanceFiles_CashAdvanceId",
                table: "CashAdvanceFiles",
                column: "CashAdvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvancePayments_CashAdvanceId",
                table: "CashAdvancePayments",
                column: "CashAdvanceId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvances_DeptId",
                table: "CashAdvances",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvances_RoleId",
                table: "CashAdvances",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_CashAdvances_UserId",
                table: "CashAdvances",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_pettyCashActions_RoleId",
                table: "pettyCashActions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_pettyCashActions_UserId",
                table: "pettyCashActions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashApprovals_PettyCashID",
                table: "PettyCashApprovals",
                column: "PettyCashID");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashes_DeptId",
                table: "PettyCashes",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashes_RoleId",
                table: "PettyCashes",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_PettyCashes_UserId",
                table: "PettyCashes",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_pettyCashFiles_PettyCashId",
                table: "pettyCashFiles",
                column: "PettyCashId");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementCashBookPayments_RetirePaymentId",
                table: "RetirementCashBookPayments",
                column: "RetirePaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementPaymentActions_RoleId",
                table: "RetirementPaymentActions",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementPaymentActions_UserId",
                table: "RetirementPaymentActions",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementPaymentFiles_RetirementPaymentId",
                table: "RetirementPaymentFiles",
                column: "RetirementPaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementPayments_DeptId",
                table: "RetirementPayments",
                column: "DeptId");

            migrationBuilder.CreateIndex(
                name: "IX_RetirementPayments_UserId",
                table: "RetirementPayments",
                column: "UserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationFlows");

            migrationBuilder.DropTable(
                name: "Banks");

            migrationBuilder.DropTable(
                name: "CashAdvanceActions");

            migrationBuilder.DropTable(
                name: "CashAdvanceFiles");

            migrationBuilder.DropTable(
                name: "CashAdvancePayments");

            migrationBuilder.DropTable(
                name: "pettyCashActions");

            migrationBuilder.DropTable(
                name: "PettyCashApprovals");

            migrationBuilder.DropTable(
                name: "pettyCashFiles");

            migrationBuilder.DropTable(
                name: "RetirementCashBookPayments");

            migrationBuilder.DropTable(
                name: "RetirementPaymentActions");

            migrationBuilder.DropTable(
                name: "RetirementPaymentFiles");

            migrationBuilder.DropTable(
                name: "CashAdvances");

            migrationBuilder.DropTable(
                name: "PettyCashes");

            migrationBuilder.DropTable(
                name: "RetirementPayments");

            migrationBuilder.DropTable(
                name: "Departments");

            migrationBuilder.DropColumn(
                name: "DeptId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RoleLead",
                table: "AspNetUsers");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "5b7a19b6-27df-4933-8463-c206ef456533");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "c6134fb6-0e57-47d1-b1a1-24439af7a679");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "b0524b30-1dc1-473d-ab9e-676e7b60c3b9");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 4,
                column: "ConcurrencyStamp",
                value: "3700bfe0-5ba4-468e-bde5-789ec53cd38e");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 5,
                column: "ConcurrencyStamp",
                value: "1efd427a-a48c-45ed-bda4-c548458505f2");

            migrationBuilder.UpdateData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: 6,
                column: "ConcurrencyStamp",
                value: "37cc411d-505d-424f-9b6b-6e7bec945c2b");
        }
    }
}
