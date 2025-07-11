using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infertility_Treatment_Managements.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RoleName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleID);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    ServiceID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.ServiceID);
                });

            migrationBuilder.CreateTable(
                name: "Slots",
                columns: table => new
                {
                    SlotID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SlotName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    StartTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    EndTime = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slots", x => x.SlotID);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FullName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    RoleID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResetPasswordToken = table.Column<string>(type: "text", nullable: true),
                    ResetPasswordExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserID);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleID",
                        column: x => x.RoleID,
                        principalTable: "Roles",
                        principalColumn: "RoleID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BlogPosts",
                columns: table => new
                {
                    BlogPostID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ImageUrl = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    AuthorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Category = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PublishDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ViewCount = table.Column<int>(type: "integer", nullable: false),
                    Tags = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogPosts", x => x.BlogPostID);
                    table.ForeignKey(
                        name: "FK_BlogPosts_Users_AuthorID",
                        column: x => x.AuthorID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ContentPages",
                columns: table => new
                {
                    ContentPageID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    Slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MetaDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MetaKeywords = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PageType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedByID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    LastModifiedByID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentPages", x => x.ContentPageID);
                    table.ForeignKey(
                        name: "FK_ContentPages_Users_CreatedByID",
                        column: x => x.CreatedByID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_ContentPages_Users_LastModifiedByID",
                        column: x => x.LastModifiedByID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "Doctors",
                columns: table => new
                {
                    DoctorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DoctorName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Specialization = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Doctors", x => x.DoctorID);
                    table.ForeignKey(
                        name: "FK_Doctors_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Phone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Address = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Gender = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    BloodType = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    EmergencyPhoneNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientID);
                    table.ForeignKey(
                        name: "FK_Patients_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Bookings",
                columns: table => new
                {
                    BookingID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ServiceID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DoctorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SlotID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DateBooking = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "text", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bookings", x => x.BookingID);
                    table.ForeignKey(
                        name: "FK_Bookings_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Bookings_Slots_SlotID",
                        column: x => x.SlotID,
                        principalTable: "Slots",
                        principalColumn: "SlotID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Feedbacks",
                columns: table => new
                {
                    FeedbackID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Content = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    BlogPostID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ServiceID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    FeedbackType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    AdminResponse = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RespondedByID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPublic = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Feedbacks", x => x.FeedbackID);
                    table.ForeignKey(
                        name: "FK_Feedbacks_BlogPosts_BlogPostID",
                        column: x => x.BlogPostID,
                        principalTable: "BlogPosts",
                        principalColumn: "BlogPostID");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_RespondedByID",
                        column: x => x.RespondedByID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                    table.ForeignKey(
                        name: "FK_Feedbacks_Users_UserID",
                        column: x => x.UserID,
                        principalTable: "Users",
                        principalColumn: "UserID");
                });

            migrationBuilder.CreateTable(
                name: "PatientDetails",
                columns: table => new
                {
                    PatientDetailID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TreatmentStatus = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MedicalHistory = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientDetails", x => x.PatientDetailID);
                    table.ForeignKey(
                        name: "FK_PatientDetails_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Examinations",
                columns: table => new
                {
                    ExaminationID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DoctorId = table.Column<string>(type: "character varying(50)", nullable: true),
                    PatientId = table.Column<string>(type: "text", nullable: true),
                    BookingID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExaminationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExaminationDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Result = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreateAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Examinations", x => x.ExaminationID);
                    table.ForeignKey(
                        name: "FK_Examinations_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Examinations_Doctors_DoctorId",
                        column: x => x.DoctorId,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID");
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    PaymentID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BookingID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TotalAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Method = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.PaymentID);
                    table.ForeignKey(
                        name: "FK_Payments_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Ratings",
                columns: table => new
                {
                    RatingID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DoctorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ServiceID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BookingID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    Comment = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RatingType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RatingDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsAnonymous = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ratings", x => x.RatingID);
                    table.ForeignKey(
                        name: "FK_Ratings_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ratings_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ratings_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Ratings_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentPlans",
                columns: table => new
                {
                    TreatmentPlanID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DoctorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ServiceID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Method = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PatientDetailID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EndDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TreatmentDescription = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Giaidoan = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GhiChu = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentPlans", x => x.TreatmentPlanID);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_PatientDetails_PatientDetailID",
                        column: x => x.PatientDetailID,
                        principalTable: "PatientDetails",
                        principalColumn: "PatientDetailID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentPlans_Services_ServiceID",
                        column: x => x.ServiceID,
                        principalTable: "Services",
                        principalColumn: "ServiceID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentMedications",
                columns: table => new
                {
                    MedicationID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TreatmentPlanID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DrugName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentMedications", x => x.MedicationID);
                    table.ForeignKey(
                        name: "FK_TreatmentMedications_TreatmentPlans_TreatmentPlanID",
                        column: x => x.TreatmentPlanID,
                        principalTable: "TreatmentPlans",
                        principalColumn: "TreatmentPlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentProcesses",
                columns: table => new
                {
                    TreatmentProcessID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DoctorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PatientDetailID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TreatmentPlanID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ScheduledDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Result = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentProcesses", x => x.TreatmentProcessID);
                    table.ForeignKey(
                        name: "FK_TreatmentProcesses_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentProcesses_PatientDetails_PatientDetailID",
                        column: x => x.PatientDetailID,
                        principalTable: "PatientDetails",
                        principalColumn: "PatientDetailID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TreatmentProcesses_TreatmentPlans_TreatmentPlanID",
                        column: x => x.TreatmentPlanID,
                        principalTable: "TreatmentPlans",
                        principalColumn: "TreatmentPlanID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TreatmentSteps",
                columns: table => new
                {
                    TreatmentStepID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TreatmentPlanID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StepOrder = table.Column<int>(type: "integer", nullable: false),
                    StepName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TreatmentSteps", x => x.TreatmentStepID);
                    table.ForeignKey(
                        name: "FK_TreatmentSteps_TreatmentPlans_TreatmentPlanID",
                        column: x => x.TreatmentPlanID,
                        principalTable: "TreatmentPlans",
                        principalColumn: "TreatmentPlanID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PatientID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DoctorID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BookingID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TreatmentProcessID = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    MessageForDoctor = table.Column<string>(type: "text", nullable: false),
                    Time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DoctorIsRead = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false),
                    PatientIsRead = table.Column<bool>(type: "boolean", nullable: true, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationID);
                    table.ForeignKey(
                        name: "FK_Notifications_Bookings_BookingID",
                        column: x => x.BookingID,
                        principalTable: "Bookings",
                        principalColumn: "BookingID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Doctors_DoctorID",
                        column: x => x.DoctorID,
                        principalTable: "Doctors",
                        principalColumn: "DoctorID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_Patients_PatientID",
                        column: x => x.PatientID,
                        principalTable: "Patients",
                        principalColumn: "PatientID",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Notifications_TreatmentProcesses_TreatmentProcessID",
                        column: x => x.TreatmentProcessID,
                        principalTable: "TreatmentProcesses",
                        principalColumn: "TreatmentProcessID",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogPosts_AuthorID",
                table: "BlogPosts",
                column: "AuthorID");

            migrationBuilder.CreateIndex(
                name: "IDX_Bookings_DoctorID",
                table: "Bookings",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IDX_Bookings_PatientID",
                table: "Bookings",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IDX_Bookings_ServiceID",
                table: "Bookings",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IDX_Bookings_SlotID",
                table: "Bookings",
                column: "SlotID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPages_CreatedByID",
                table: "ContentPages",
                column: "CreatedByID");

            migrationBuilder.CreateIndex(
                name: "IX_ContentPages_LastModifiedByID",
                table: "ContentPages",
                column: "LastModifiedByID");

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_UserID",
                table: "Doctors",
                column: "UserID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Examinations_BookingID",
                table: "Examinations",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_Examinations_DoctorId",
                table: "Examinations",
                column: "DoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_BlogPostID",
                table: "Feedbacks",
                column: "BlogPostID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_PatientID",
                table: "Feedbacks",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_RespondedByID",
                table: "Feedbacks",
                column: "RespondedByID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_ServiceID",
                table: "Feedbacks",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_UserID",
                table: "Feedbacks",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_BookingID",
                table: "Notifications",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_DoctorID",
                table: "Notifications",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PatientID",
                table: "Notifications",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TreatmentProcessID",
                table: "Notifications",
                column: "TreatmentProcessID");

            migrationBuilder.CreateIndex(
                name: "IX_PatientDetails_PatientID",
                table: "PatientDetails",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_UserID",
                table: "Patients",
                column: "UserID");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_BookingID",
                table: "Payments",
                column: "BookingID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_BookingID",
                table: "Ratings",
                column: "BookingID");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_DoctorID",
                table: "Ratings",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_PatientID",
                table: "Ratings",
                column: "PatientID");

            migrationBuilder.CreateIndex(
                name: "IX_Ratings_ServiceID",
                table: "Ratings",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentMedications_TreatmentPlanID",
                table: "TreatmentMedications",
                column: "TreatmentPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_DoctorID",
                table: "TreatmentPlans",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_PatientDetailID",
                table: "TreatmentPlans",
                column: "PatientDetailID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentPlans_ServiceID",
                table: "TreatmentPlans",
                column: "ServiceID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentProcesses_DoctorID",
                table: "TreatmentProcesses",
                column: "DoctorID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentProcesses_PatientDetailID",
                table: "TreatmentProcesses",
                column: "PatientDetailID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentProcesses_TreatmentPlanID",
                table: "TreatmentProcesses",
                column: "TreatmentPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_TreatmentSteps_TreatmentPlanID",
                table: "TreatmentSteps",
                column: "TreatmentPlanID");

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleID",
                table: "Users",
                column: "RoleID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentPages");

            migrationBuilder.DropTable(
                name: "Examinations");

            migrationBuilder.DropTable(
                name: "Feedbacks");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Ratings");

            migrationBuilder.DropTable(
                name: "TreatmentMedications");

            migrationBuilder.DropTable(
                name: "TreatmentSteps");

            migrationBuilder.DropTable(
                name: "BlogPosts");

            migrationBuilder.DropTable(
                name: "TreatmentProcesses");

            migrationBuilder.DropTable(
                name: "Bookings");

            migrationBuilder.DropTable(
                name: "TreatmentPlans");

            migrationBuilder.DropTable(
                name: "Slots");

            migrationBuilder.DropTable(
                name: "Doctors");

            migrationBuilder.DropTable(
                name: "PatientDetails");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
