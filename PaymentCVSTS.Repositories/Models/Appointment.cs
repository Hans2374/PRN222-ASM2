﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace PaymentCVSTS.Repositories.Models;

public partial class Appointment
{
    public int AppointmentId { get; set; }

    public DateTime BookingDate { get; set; }

    public string Status { get; set; }

    public string PaymentStatus { get; set; }

    public decimal TotalCost { get; set; }

    public int ChildId { get; set; }

    public DateOnly CreatedDate { get; set; }

    public DateOnly ModifiedDate { get; set; }

    public string ServiceType { get; set; }

    public string VaccineId { get; set; }

    public int? ServiceId { get; set; }

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}