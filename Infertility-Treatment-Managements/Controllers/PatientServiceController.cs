using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Infertility_Treatment_Managements.Models;
using Infertility_Treatment_Managements.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace Infertility_Treatment_Managements.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DichVuHiemMuonController : ControllerBase
    {
        private readonly InfertilityTreatmentManagementContext _context;

        public DichVuHiemMuonController(InfertilityTreatmentManagementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy tất cả các đăng ký dịch vụ điều trị hiếm muộn
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> LayTatCaDichVuHiemMuon()
        {
            // Lấy tất cả các booking dịch vụ hiếm muộn
            var danhSachDichVu = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Where(b => b.Service.Category == "InfertilityTreatment")
                .ToListAsync();

            return danhSachDichVu.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                PatientId = b.PatientId,
                DoctorId = b.DoctorId,
                ServiceId = b.ServiceId,
                SlotId = b.SlotId,
                DateBooking = b.DateBooking,
                Description = b.Description,
                CreateAt = b.CreateAt,
                Note = b.Note,
                Patient = b.Patient != null ? new PatientBasicDTO
                {
                    Name = b.Patient.Name,
                    Email = b.Patient.Email,
                    Phone = b.Patient.Phone
                } : null,
                Doctor = b.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorName = b.Doctor.DoctorName,
                    Email = b.Doctor.Email,
                    Phone = b.Doctor.Phone,
                    Specialization = b.Doctor.Specialization
                } : null,
                Service = b.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = b.Service.ServiceId,
                    Name = b.Service.Name,
                    Description = b.Service.Description,
                    Category = b.Service.Category
                } : null
            }).ToList();
        }

        /// <summary>
        /// Lấy thông tin chi tiết một đăng ký dịch vụ điều trị hiếm muộn theo ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDTO>> LayChiTietDichVuHiemMuon(string id)
        {
            var dichVu = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Service.Category == "InfertilityTreatment");

            if (dichVu == null)
            {
                return NotFound($"Không tìm thấy đăng ký điều trị hiếm muộn với ID {id}");
            }

            return new BookingDTO
            {
                BookingId = dichVu.BookingId,
                PatientId = dichVu.PatientId,
                DoctorId = dichVu.DoctorId,
                ServiceId = dichVu.ServiceId,
                SlotId = dichVu.SlotId,
                DateBooking = dichVu.DateBooking,
                Description = dichVu.Description,
                CreateAt = dichVu.CreateAt,
                Note = dichVu.Note,
                Patient = dichVu.Patient != null ? new PatientBasicDTO
                {
                    Name = dichVu.Patient.Name,
                    Email = dichVu.Patient.Email,
                    Phone = dichVu.Patient.Phone
                } : null,
                Doctor = dichVu.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorName = dichVu.Doctor.DoctorName,
                    Email = dichVu.Doctor.Email,
                    Phone = dichVu.Doctor.Phone,
                    Specialization = dichVu.Doctor.Specialization
                } : null,
                Service = dichVu.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = dichVu.Service.ServiceId,
                    Name = dichVu.Service.Name,
                    Description = dichVu.Service.Description,
                    Category = dichVu.Service.Category
                } : null
            };
        }

        /// <summary>
        /// Lấy danh sách đăng ký dịch vụ điều trị hiếm muộn của một bệnh nhân
        /// </summary>
        [HttpGet("BenhNhan/{maBenhNhan}")]
        public async Task<ActionResult<IEnumerable<BookingDTO>>> LayDichVuHiemMuonTheoBenhNhan(string maBenhNhan)
        {
            var benhNhanTonTai = await _context.Patients.AnyAsync(p => p.PatientId == maBenhNhan);
            if (!benhNhanTonTai)
            {
                return NotFound($"Không tìm thấy bệnh nhân với ID {maBenhNhan}");
            }

            var danhSachDichVu = await _context.Bookings
                .Include(b => b.Patient)
                .Include(b => b.Doctor)
                .Include(b => b.Service)
                .Include(b => b.Slot)
                .Where(b => b.PatientId == maBenhNhan && b.Service.Category == "InfertilityTreatment")
                .ToListAsync();

            return danhSachDichVu.Select(b => new BookingDTO
            {
                BookingId = b.BookingId,
                PatientId = b.PatientId,
                DoctorId = b.DoctorId,
                ServiceId = b.ServiceId,
                SlotId = b.SlotId,
                DateBooking = b.DateBooking,
                Description = b.Description,
                CreateAt = b.CreateAt,
                Note = b.Note,
                Doctor = b.Doctor != null ? new DoctorBasicDTO
                {
                    DoctorName = b.Doctor.DoctorName,
                    Email = b.Doctor.Email,
                    Phone = b.Doctor.Phone,
                    Specialization = b.Doctor.Specialization
                } : null,
                Service = b.Service != null ? new ServiceBasicDTO
                {
                    ServiceId = b.Service.ServiceId,
                    Name = b.Service.Name,
                    Description = b.Service.Description,
                    Category = b.Service.Category
                } : null
            }).ToList();
        }

        /// <summary>
        /// Đăng ký dịch vụ điều trị hiếm muộn mới (IUI, IVF...)
        /// </summary>
        [HttpPost("DangKy")]
        public async Task<ActionResult<BookingDTO>> DangKyDichVuHiemMuon(DangKyDieuTriHiemMuonDTO thongTinDangKy)
        {
            // Kiểm tra bệnh nhân
            var benhNhan = await _context.Patients.FindAsync(thongTinDangKy.MaBenhNhan);
            if (benhNhan == null)
            {
                return BadRequest("Mã bệnh nhân không hợp lệ: Bệnh nhân không tồn tại");
            }

            // Kiểm tra bác sĩ nếu có chỉ định
            if (!string.IsNullOrEmpty(thongTinDangKy.MaBacSi))
            {
                var bacSi = await _context.Doctors.FindAsync(thongTinDangKy.MaBacSi);
                if (bacSi == null)
                {
                    return BadRequest("Mã bác sĩ không hợp lệ: Bác sĩ không tồn tại");
                }
            }

            // Kiểm tra dịch vụ (phải là dịch vụ điều trị hiếm muộn)
            var dichVu = await _context.Services.FindAsync(thongTinDangKy.MaDichVu);
            if (dichVu == null)
            {
                return BadRequest("Mã dịch vụ không hợp lệ: Dịch vụ không tồn tại");
            }

            if (dichVu.Category != "InfertilityTreatment")
            {
                return BadRequest("Dịch vụ được chọn không phải là dịch vụ điều trị hiếm muộn");
            }

            // Kiểm tra khung giờ nếu có chỉ định
            if (!string.IsNullOrEmpty(thongTinDangKy.MaKhungGio))
            {
                var khungGio = await _context.Slots.FindAsync(thongTinDangKy.MaKhungGio);
                if (khungGio == null)
                {
                    return BadRequest("Mã khung giờ không hợp lệ: Khung giờ không tồn tại");
                }
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Tạo đăng ký mới
                var dangKy = new Booking
                {
                    BookingId = Guid.NewGuid().ToString(),
                    PatientId = thongTinDangKy.MaBenhNhan,
                    DoctorId = thongTinDangKy.MaBacSi,
                    ServiceId = thongTinDangKy.MaDichVu,
                    SlotId = thongTinDangKy.MaKhungGio,
                    DateBooking = thongTinDangKy.NgayHen ?? DateTime.Now.AddDays(1),
                    Description = thongTinDangKy.MoTa ?? $"Đăng ký {dichVu.Name}",
                    CreateAt = DateTime.Now,
                    Note = thongTinDangKy.GhiChu
                };

                _context.Bookings.Add(dangKy);
                await _context.SaveChangesAsync();

                // Tùy chọn: Tạo chi tiết bệnh nhân nếu chưa có
                var chiTietBenhNhan = await _context.PatientDetails
                    .FirstOrDefaultAsync(pd => pd.PatientId == benhNhan.PatientId);

                if (chiTietBenhNhan == null)
                {
                    chiTietBenhNhan = new PatientDetail
                    {
                        PatientDetailId = Guid.NewGuid().ToString(),
                        PatientId = benhNhan.PatientId,
                        TreatmentStatus = "Đã đăng ký"
                    };

                    _context.PatientDetails.Add(chiTietBenhNhan);
                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();

                // Lấy thông tin đầy đủ của đăng ký với các thực thể liên quan
                var dangKyDayDu = await _context.Bookings
                    .Include(b => b.Patient)
                    .Include(b => b.Doctor)
                    .Include(b => b.Service)
                    .Include(b => b.Slot)
                    .FirstOrDefaultAsync(b => b.BookingId == dangKy.BookingId);

                return Ok(new BookingDTO
                {
                    BookingId = dangKyDayDu.BookingId,
                    PatientId = dangKyDayDu.PatientId,
                    DoctorId = dangKyDayDu.DoctorId,
                    ServiceId = dangKyDayDu.ServiceId,
                    SlotId = dangKyDayDu.SlotId,
                    DateBooking = dangKyDayDu.DateBooking,
                    Description = dangKyDayDu.Description,
                    CreateAt = dangKyDayDu.CreateAt,
                    Note = dangKyDayDu.Note,
                    Patient = dangKyDayDu.Patient != null ? new PatientBasicDTO
                    {
                        Name = dangKyDayDu.Patient.Name,
                        Email = dangKyDayDu.Patient.Email,
                        Phone = dangKyDayDu.Patient.Phone
                    } : null,
                    Doctor = dangKyDayDu.Doctor != null ? new DoctorBasicDTO
                    {
                        DoctorName = dangKyDayDu.Doctor.DoctorName,
                        Email = dangKyDayDu.Doctor.Email,
                        Phone = dangKyDayDu.Doctor.Phone,
                        Specialization = dangKyDayDu.Doctor.Specialization
                    } : null,
                    Service = dangKyDayDu.Service != null ? new ServiceBasicDTO
                    {
                        ServiceId = dangKyDayDu.Service.ServiceId,
                        Name = dangKyDayDu.Service.Name,
                        Description = dangKyDayDu.Service.Description,
                        Category = dangKyDayDu.Service.Category
                    } : null
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Cập nhật thông tin đăng ký dịch vụ điều trị hiếm muộn
        /// </summary>
        [HttpPut("CapNhat/{id}")]
        public async Task<IActionResult> CapNhatDichVuHiemMuon(string id, CapNhatDieuTriHiemMuonDTO thongTinCapNhat)
        {
            var dangKy = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Service.Category == "InfertilityTreatment");

            if (dangKy == null)
            {
                return NotFound($"Không tìm thấy đăng ký điều trị hiếm muộn với ID {id}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Cập nhật bác sĩ nếu có cung cấp
                if (!string.IsNullOrEmpty(thongTinCapNhat.MaBacSi) && thongTinCapNhat.MaBacSi != dangKy.DoctorId)
                {
                    var bacSi = await _context.Doctors.FindAsync(thongTinCapNhat.MaBacSi);
                    if (bacSi == null)
                    {
                        return BadRequest("Mã bác sĩ không hợp lệ: Bác sĩ không tồn tại");
                    }
                    dangKy.DoctorId = thongTinCapNhat.MaBacSi;
                }

                // Cập nhật ngày hẹn nếu có cung cấp
                if (thongTinCapNhat.NgayHen.HasValue)
                {
                    dangKy.DateBooking = thongTinCapNhat.NgayHen.Value;
                }

                // Cập nhật khung giờ nếu có cung cấp
                if (!string.IsNullOrEmpty(thongTinCapNhat.MaKhungGio) && thongTinCapNhat.MaKhungGio != dangKy.SlotId)
                {
                    var khungGio = await _context.Slots.FindAsync(thongTinCapNhat.MaKhungGio);
                    if (khungGio == null)
                    {
                        return BadRequest("Mã khung giờ không hợp lệ: Khung giờ không tồn tại");
                    }
                    dangKy.SlotId = thongTinCapNhat.MaKhungGio;
                }

                // Cập nhật mô tả và ghi chú nếu có cung cấp
                if (!string.IsNullOrEmpty(thongTinCapNhat.MoTa))
                {
                    dangKy.Description = thongTinCapNhat.MoTa;
                }

                if (!string.IsNullOrEmpty(thongTinCapNhat.GhiChu))
                {
                    dangKy.Note = thongTinCapNhat.GhiChu;
                }

                _context.Entry(dangKy).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"Đã xảy ra lỗi: {ex.Message}");
            }
        }

        /// <summary>
        /// Hủy đăng ký dịch vụ điều trị hiếm muộn
        /// </summary>
        [HttpDelete("Huy/{id}")]
        public async Task<IActionResult> HuyDichVuHiemMuon(string id)
        {
            var dangKy = await _context.Bookings
                .Include(b => b.Service)
                .FirstOrDefaultAsync(b => b.BookingId == id && b.Service.Category == "InfertilityTreatment");

            if (dangKy == null)
            {
                return NotFound($"Không tìm thấy đăng ký điều trị hiếm muộn với ID {id}");
            }

            _context.Bookings.Remove(dangKy);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        /// <summary>
        /// Lấy danh sách các dịch vụ điều trị hiếm muộn có sẵn
        /// </summary>
        [HttpGet("DanhSachDichVu")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<DichVuDTO>>> LayDanhSachDichVuHiemMuon()
        {
            var danhSachDichVu = await _context.Services
                .Where(s => s.Category == "InfertilityTreatment" && s.Status == "Active")
                .ToListAsync();

            return danhSachDichVu.Select(s => new DichVuDTO
            {
                MaDichVu = s.ServiceId,
                TenDichVu = s.Name,
                MoTa = s.Description,
                Gia = s.Price,
                TrangThai = s.Status,
                LoaiDichVu = s.Category
            }).ToList();
        }
    }

    // DTO classes for Infertility Treatment
    public class DangKyDieuTriHiemMuonDTO
    {
        public string MaBenhNhan { get; set; }
        public string MaDichVu { get; set; }
        public string MaBacSi { get; set; }
        public string MaKhungGio { get; set; }
        public DateTime? NgayHen { get; set; }
        public string MoTa { get; set; }
        public string GhiChu { get; set; }
    }

    public class CapNhatDieuTriHiemMuonDTO
    {
        public string MaBacSi { get; set; }
        public string MaKhungGio { get; set; }
        public DateTime? NgayHen { get; set; }
        public string MoTa { get; set; }
        public string GhiChu { get; set; }
    }

    public class DichVuDTO
    {
        public string MaDichVu { get; set; }
        public string TenDichVu { get; set; }
        public string MoTa { get; set; }
        public decimal? Gia { get; set; }
        public string TrangThai { get; set; }
        public string LoaiDichVu { get; set; }
    }

    public class DichVuCoBanDTO
    {
        public string MaDichVu { get; set; }
        public string TenDichVu { get; set; }
        public string MoTa { get; set; }
        public string LoaiDichVu { get; set; }
    }
}