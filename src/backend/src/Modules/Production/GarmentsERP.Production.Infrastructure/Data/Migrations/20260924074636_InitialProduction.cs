using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GarmentsERP.Production.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialProduction : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "production");

            migrationBuilder.CreateTable(
                name: "Buyers",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PaymentTerms = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Buyers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SewingLines",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LineName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Floor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetEfficiency = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    CapacityPerDay = table.Column<int>(type: "int", nullable: false),
                    Supervisor = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SewingLines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BuyerOrders",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuyerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StyleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    StyleName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Season = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderQuantity = table.Column<int>(type: "int", nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    OrderDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeliveryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BuyerOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BuyerOrders_Buyers_BuyerId",
                        column: x => x.BuyerId,
                        principalSchema: "production",
                        principalTable: "Buyers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CuttingOrders",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CuttingOrderNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BuyerOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StyleNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FabricType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TotalRequiredQty = table.Column<int>(type: "int", nullable: false),
                    TotalPlies = table.Column<int>(type: "int", nullable: false),
                    MarkerLength = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    FabricWidth = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EfficiencyPercentage = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    TableNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CuttingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuttingOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuttingOrders_BuyerOrders_BuyerOrderId",
                        column: x => x.BuyerOrderId,
                        principalSchema: "production",
                        principalTable: "BuyerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "HourlyProductionLogs",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SewingLineId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BuyerOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HourSlot = table.Column<int>(type: "int", nullable: false),
                    TargetQty = table.Column<int>(type: "int", nullable: false),
                    ActualOutputQty = table.Column<int>(type: "int", nullable: false),
                    DefectCount = table.Column<int>(type: "int", nullable: false),
                    RejectionCount = table.Column<int>(type: "int", nullable: false),
                    OperatorCount = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HourlyProductionLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HourlyProductionLogs_BuyerOrders_BuyerOrderId",
                        column: x => x.BuyerOrderId,
                        principalSchema: "production",
                        principalTable: "BuyerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_HourlyProductionLogs_SewingLines_SewingLineId",
                        column: x => x.SewingLineId,
                        principalSchema: "production",
                        principalTable: "SewingLines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "QualityInspections",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InspectionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InspectionType = table.Column<int>(type: "int", nullable: false),
                    BuyerOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LotOrBatchRef = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SampleSize = table.Column<int>(type: "int", nullable: false),
                    MinorDefects = table.Column<int>(type: "int", nullable: false),
                    MajorDefects = table.Column<int>(type: "int", nullable: false),
                    CriticalDefects = table.Column<int>(type: "int", nullable: false),
                    TotalPoints = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Result = table.Column<int>(type: "int", nullable: false),
                    InspectorName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    InspectedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QualityInspections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QualityInspections_BuyerOrders_BuyerOrderId",
                        column: x => x.BuyerOrderId,
                        principalSchema: "production",
                        principalTable: "BuyerOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CutBundles",
                schema: "production",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BundleNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CuttingOrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Size = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Color = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    Barcode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SerialStart = table.Column<int>(type: "int", nullable: false),
                    SerialEnd = table.Column<int>(type: "int", nullable: false),
                    CurrentSection = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CutBundles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CutBundles_CuttingOrders_CuttingOrderId",
                        column: x => x.CuttingOrderId,
                        principalSchema: "production",
                        principalTable: "CuttingOrders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BuyerOrders_BuyerId",
                schema: "production",
                table: "BuyerOrders",
                column: "BuyerId");

            migrationBuilder.CreateIndex(
                name: "IX_BuyerOrders_OrderNumber",
                schema: "production",
                table: "BuyerOrders",
                column: "OrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Buyers_Code",
                schema: "production",
                table: "Buyers",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CutBundles_CuttingOrderId",
                schema: "production",
                table: "CutBundles",
                column: "CuttingOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingOrders_BuyerOrderId",
                schema: "production",
                table: "CuttingOrders",
                column: "BuyerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_CuttingOrders_CuttingOrderNumber",
                schema: "production",
                table: "CuttingOrders",
                column: "CuttingOrderNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HourlyProductionLogs_BuyerOrderId",
                schema: "production",
                table: "HourlyProductionLogs",
                column: "BuyerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_HourlyProductionLogs_SewingLineId",
                schema: "production",
                table: "HourlyProductionLogs",
                column: "SewingLineId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_BuyerOrderId",
                schema: "production",
                table: "QualityInspections",
                column: "BuyerOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_QualityInspections_InspectionNumber",
                schema: "production",
                table: "QualityInspections",
                column: "InspectionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SewingLines_LineNumber",
                schema: "production",
                table: "SewingLines",
                column: "LineNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CutBundles",
                schema: "production");

            migrationBuilder.DropTable(
                name: "HourlyProductionLogs",
                schema: "production");

            migrationBuilder.DropTable(
                name: "QualityInspections",
                schema: "production");

            migrationBuilder.DropTable(
                name: "CuttingOrders",
                schema: "production");

            migrationBuilder.DropTable(
                name: "SewingLines",
                schema: "production");

            migrationBuilder.DropTable(
                name: "BuyerOrders",
                schema: "production");

            migrationBuilder.DropTable(
                name: "Buyers",
                schema: "production");
        }
    }
}
