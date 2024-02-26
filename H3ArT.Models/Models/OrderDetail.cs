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
    public class OrderDetail
    {
        public int Id { get; set; }

        public int orderHeaderId { get; set; }
        [ForeignKey("orderHeaderId")]
        [ValidateNever]
        public OrderHeader orderHeader { get; set; }

        [Required]
        public int artworkId { get; set; }
        [ForeignKey("artworkId")]
        [ValidateNever]
        public Artwork artwork { get; set; }

        public int count { get; set; }

        //not updated
        public double price { get; set; }
    }
}
