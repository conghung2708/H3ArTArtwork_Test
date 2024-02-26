using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3ArT.Models.Models
{
    public class OrderHeader
    {
        public int Id { get; set; }

        [ValidateNever]
        public string applicationUserId { get; set; }
        [ForeignKey("applicationUserId")]
        [ValidateNever]
        public ApplicationUser applicationUser { get; set; }

        public DateTime orderDate { get; set; }
        public double orderTotal { get; set; }

        public string? orderStatus { get; set; }
        public string? paymentStatus { get; set;}

        public DateTime paymentDate { get; set; }

        public string? sessionId { get; set; }
        public string? paymentIntentId { get; set; }

        [Required]
        public string name { get; set; }
        [Required]
        public string phoneNumber { get; set; }
    }
}
