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
    public class ShoppingCart
    {
        [Key]
        public int shoppingCartId { get; set; }

        public string buyerID { get; set; }
        [ForeignKey("buyerID")]
        [ValidateNever]
        public ApplicationUser applicationUser { get; set; }
        public string artistID { get; set; }

        public int artworkID { get; set; }
        [ForeignKey("artworkID")]
        [ValidateNever]
        public Artwork artwork { get; set; }

        [NotMapped]
        public IEnumerable<Artwork>? RelatedArtworks { get; set; }

        public int count { get; set; }

        [NotMapped]
        public double price { get; set; }

        public Boolean isNew { get; set; }

    }
}
