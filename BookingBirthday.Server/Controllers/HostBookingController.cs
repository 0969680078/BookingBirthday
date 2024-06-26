﻿using BookingBirthday.Data.Entities;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using static NuGet.Packaging.PackagingConstants;
using System;
using BookingBirthday.Data.EF;
using Microsoft.EntityFrameworkCore;
using BookingBirthday.Server.Common;
using X.PagedList;

namespace BookingBirthday.Server.Controllers
{
    public class HostBookingController : HostBaseController
    {
        private readonly BookingDbContext _dbContext;

        public HostBookingController(BookingDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IActionResult> Index(int? page)
        {
            var user_id = int.Parse(HttpContext.Session.GetString("user_id")!);
           
            var session = HttpContext.Session;
            List<Category_requests> category_request = null!;

            category_request = _dbContext.Category_Requests
                    .Where(x => x.is_deleted_by_owner == false && x.requester_id == user_id && (x.is_approved == -1 || x.is_approved == 1))
                    .OrderByDescending(x => x.created_at)
                    .ToList();

            if (category_request != null)
            {
                var jsonNotification = JsonConvert.SerializeObject(category_request);
                session.SetString("notification", jsonNotification);
            }

            var query = from a in _dbContext.Bookings
                        join b in _dbContext.Packages
                        on a.PackageId equals b.Id
                        //join d in _dbContext.Users
                        //on c.UserId equals d.Id
                        where b.UserId == user_id
                        select new { a };
            var orders = await query.OrderByDescending(x => x.a.Date_order).Select(x => new Booking
            {
                Id = x.a.Id,                
                UserId = x.a.UserId,
                Phone = x.a.Phone,
                Email = x.a.Email,
                Note = x.a.Note,
                Reason = x.a.Reason,
                ChildDateOfBirth = x.a.ChildDateOfBirth,
                ChildGender = x.a.ChildGender,
                ChildName = x.a.ChildName,
                ChildNumber = x.a.ChildNumber,
                LocationId = x.a.LocationId,
                PackageId = x.a.PackageId,
                Date_order = x.a.Date_order,
                CheckIn = x.a.CheckIn,
                CheckOut = x.a.CheckOut,
                Date_start = x.a.Date_start,
                Date_cancel = x.a.Date_cancel,
                Total = x.a.Total,
                BookingStatus = x.a.BookingStatus,

            }).ToListAsync();
            int pageSize = 8;
            int pageNumber = page == null || page < 0 ? 1 : page.Value;
            PagedList<Booking> lst = new PagedList<Booking>(orders, pageNumber, pageSize);
            return View(lst);
        }
        [HttpPost]
        public IActionResult Edit(int Id, string status)
        {
            try
            {
                var user_id = int.Parse(HttpContext.Session.GetString("user_id")!);

                var order = _dbContext.Bookings.Find(Id);
                if (order != null)
                {
                    order.BookingStatus = status;
                    _dbContext.SaveChanges();
                    TempData["Message"] = "Cập nhật trạng thái đơn hàng thành công";
                    TempData["Success"] = true;
                }
                else
                {
                    TempData["Message"] = "Đơn hàng không tồn tại";
                    TempData["Success"] = false;
                }
                return Ok();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi";
                TempData["Success"] = false;
                return BadRequest();
            }
        }
        public IActionResult ProfileCustomer(string email)
        {
            var user = _dbContext.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                TempData["Message"] = "Tài khoản không tồn tại";
                TempData["Success"] = false;
                return RedirectToAction("Index");
            }
            return View(user);
        }

        [HttpPost]
        public IActionResult DuyetDon(int Id)
        {
            try
            {
                var data =  _dbContext.Bookings.Find(Id);
                if (data != null)
                {
                    data.BookingStatus = "Accepted";
                     _dbContext.SaveChanges();
          
                    TempData["Message"] = "Nhận đơn hàng thành công";
                    TempData["Success"] = true;
                }
                else
                {
                    TempData["Message"] = "Đơn hàng không tồn tại";
                    TempData["Success"] = false;
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi";
                TempData["Success"] = false;
                return BadRequest();
            }
        }

        [HttpPost]
        public IActionResult dathanhtoan(int Id)
        {
            try
            {
                var data = _dbContext.Bookings.Find(Id);
                if (data != null)
                {
                    data.BookingStatus = "Paid";
                    _dbContext.SaveChanges();

                    TempData["Message"] = "Chuyển trạng thái đơn thành công";
                    TempData["Success"] = true;
                }
                else
                {
                    TempData["Message"] = "Đơn hàng không tồn tại";
                    TempData["Success"] = false;
                }

                return Ok();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi";
                TempData["Success"] = false;
                return BadRequest();
            }
        }


        [HttpPost]
        public  IActionResult TuChoiDon(int Id, string? Reason)
        {
            try
            {
                var data =  _dbContext.Bookings.Find(Id);
                if (data != null)
                {
                    data.BookingStatus = "Declined";
                    data.Date_cancel = DateTime.Now;
                    data.Reason = Reason;
                     _dbContext.SaveChanges();
                    
                    TempData["Message"] = "Hủy đơn hàng thành công";
                    TempData["Success"] = true;
                }
                else
                {
                    TempData["Message"] = "Đơn hàng không tồn tại";
                    TempData["Success"] = false;
                }
                
                return Ok();
            }
            catch (Exception ex)
            {
                TempData["Message"] = "Lỗi";
                TempData["Success"] = false;
                return BadRequest();
            }

        }

        [HttpPost]
        public async Task<IActionResult> SaveCheckinTime([FromBody] CheckinViewModel model)
        {
            if (ModelState.IsValid)
            {
                var booking = await _dbContext.Bookings.FindAsync(model.BookingId);

                if (booking != null)
                {
                    // Kiểm tra điều kiện thời gian check-in so với Date_start
                    if (model.CheckinTime < booking.Date_start.AddHours(-1) || model.CheckinTime > booking.Date_start.AddHours(1))

                    {
                        TempData["Message"] = "Thời gian check-in chỉ được sớm hoặc chậm hơn so với thời gian đặt 1 giờ.";
                        TempData["Success"] = false;
                        return BadRequest(new { error = "Failed to save checkin time." });

                    }

                    // Lưu thời gian Checkin vào booking
                    booking.CheckIn = model.CheckinTime;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _dbContext.SaveChangesAsync();

                    // Trả về một phản hồi thành công (nếu cần)
                    TempData["Message"] = "Lưu checkin thành công";
                    TempData["Success"] = true;
                    return Ok(new { success = true });

                }
            }

            // Trả về một phản hồi lỗi nếu có lỗi xảy ra
            TempData["Message"] = "Lưu checkin không thành công";
            TempData["Success"] = false;
            return BadRequest(new { error = "Failed to save checkin time." });

        }


        [HttpPost]
        public async Task<IActionResult> SaveCheckoutTime([FromBody] CheckoutViewModel model)
        {
            if (ModelState.IsValid)
            {
                var booking = await _dbContext.Bookings.FindAsync(model.BookingId);

                if (booking != null)
                {
                    // Kiểm tra xem thời gian check-out có lớn hơn thời gian check-in không
                    if (model.CheckoutTime <= booking.CheckIn)
                    {
                        TempData["Message"] = "Thời gian check-out phải sau thời gian check-in.";
                        TempData["Success"] = false;
                        return BadRequest(new { error = "Failed to save checkin time." });
                    }

                    // Kiểm tra xem thời gian check-out có trong cùng một ngày với thời gian check-in không
                    if (model.CheckoutTime.Date != booking.CheckIn.Value.Date)
                    {
                        TempData["Message"] = "Thời gian check-out phải trong cùng một ngày với thời gian check-in";
                        TempData["Success"] = false;
                        return BadRequest(new { error = "Failed to save checkin time." });
                    }
                    var latestCheckoutTime = new DateTime(model.CheckoutTime.Year, model.CheckoutTime.Month, model.CheckoutTime.Day, 21, 0, 0);
                    if (model.CheckoutTime > latestCheckoutTime)
                    {
                        TempData["Message"] = "Thời gian check-out không được vượt quá 9 giờ tối.";
                        TempData["Success"] = false;
                        return BadRequest(new { error = "Checkout time cannot be later than 9 PM." });
                    }
                    // Lưu thời gian CheckOut vào booking
                    booking.CheckOut = model.CheckoutTime;

                    // Lưu thay đổi vào cơ sở dữ liệu
                    await _dbContext.SaveChangesAsync();

                    // Trả về một phản hồi thành công (nếu cần)
                    TempData["Message"] = "Lưu check-out thành công";
                    TempData["Success"] = true;
                    return Ok(new { success = true });
                }
            }

            // Trả về một phản hồi lỗi nếu có lỗi xảy ra
            TempData["Message"] = "Lưu checkout không thành công";
            TempData["Success"] = false;
            return BadRequest(new { error = "Failed to save checkin time." });
        }


        public class CheckinViewModel
        {
            public int BookingId { get; set; }
            public DateTime CheckinTime { get; set; }
        }

        public class CheckoutViewModel
        {
            public int BookingId { get; set; }
            public DateTime CheckoutTime { get; set; }
        }
    }


}
    
