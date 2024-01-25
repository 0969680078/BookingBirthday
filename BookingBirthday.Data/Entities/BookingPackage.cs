﻿using BookingBirthday.Data.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingBirthday.Data.Entities
{
    public class BookingPackage
    {
        public int BookingId { get; set; }
        public string PackageId { get; set; }
        public Booking Booking { get; set; }
        public Package Package { get; set; }
        //BookingId
        //PackageId
    }
}
