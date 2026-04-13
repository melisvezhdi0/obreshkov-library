using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ObreshkovLibrary.Models
{
    public class Loan
    {
        public int Id { get; set; }

        public int ReaderId { get; set; }
        public Reader Reader { get; set; } = null!;

        public int BookCopyId { get; set; }
        public BookCopy BookCopy { get; set; } = null!;

        public DateTime LoanDate { get; set; } = DateTime.Now;

        public DateTime DueDate { get; set; } = DateTime.Now.AddDays(30);

        public DateTime? ReturnDate { get; set; }

        [StringLength(200)]
        public string? Notes { get; set; }

        public bool IsExtended { get; set; } = false;

        public bool Reminder7DaysSent { get; set; } = false;
        public bool Reminder3DaysSent { get; set; } = false;
        public bool Reminder1DaySent { get; set; } = false;
        public DateTime? LastOverdueReminderSentOn { get; set; }
    }
}