using H3ArT.Models.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3ArT.Models.ViewModels
{
    public class ArtworkVM
    {
        public Artwork artwork { get; set; }

        [ValidateNever]
        public IEnumerable<SelectListItem> categoryList { get; set; }
      
    }
}
