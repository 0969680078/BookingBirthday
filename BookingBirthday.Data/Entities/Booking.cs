﻿using BookingBirthday.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBirthday.Data.Entities
{
    public class Booking
    {
        public long Id { get; set; }
        public DateTime Date_order { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? Note { get; set; }
      
        public BookingStatus BookingStatus { get; set; }
        public double Total { get; set; }

        //UserId
        public int UserId { get; set; }
        public User? User { get; set; }

        //PaymentId
        public int PaymentId { get; set; }
        public Payment Payment { get; set; }



        public List<Cart>? cart { get; set; }
        public ICollection<BookingPackage> BookingPackages { get; set; }
    }
}
