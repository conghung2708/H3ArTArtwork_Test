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
    public class Artwork
    {
        [Key]
        public int artworkId { get; set; }
        [MaxLength(30)]
        public string title { get; set; }
        [Required]
        public string description { get; set; }


        public string artistID { get; set; }

        [ForeignKey("artistID")]
        [ValidateNever]  
        public ApplicationUser applicationUser { get; set; }
        [ValidateNever]
        public string? imageUrl { get; set; }
        [Required]
        public double price { get; set; }
        [Required]
        public Boolean isPremium { get; set; }
        public int categoryID { get; set; }
        [ForeignKey("categoryID")]
        [ValidateNever]
        public Category category { get; set; }
        public bool isBought { get; set; }
    }
}
