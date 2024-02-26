    using H3ArT.DataAccess.Repository.IRepository;
    using H3ArT.Models.Models;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using H3ArT.Models.ViewModels;
    using System.Security.Claims;


    namespace H3ArTArtwork.Areas.Creator.Controllers
    {
        [Area("Creator")]
        public class ArtworkController : Controller
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IWebHostEnvironment _webHostEnvironment;
            public ArtworkController(IUnitOfWork unitOfWork, IWebHostEnvironment webHostEnvironment)
            {
                _unitOfWork = unitOfWork;
                _webHostEnvironment = webHostEnvironment;
            }
            public IActionResult Index()
            {
                //get the id
          
                return View();
            }

            public IActionResult Upsert(int? id)
            {

                ArtworkVM artworkVM = new()
                {
                    categoryList = _unitOfWork.CategoryObj.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.categoryName,
                        Value = u.categoryId.ToString(),
                    }),
                    artwork = new Artwork()
                };

                if (id == null || id == 0)
                {
                //create
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
                artworkVM.artwork.artistID = userId;
                artworkVM.artwork.applicationUser = _unitOfWork.ApplicationUserObj.Get(u => u.Id == userId);
                
                return View(artworkVM);
                }
                else
                {
                    //update
                    artworkVM.artwork = _unitOfWork.ArtworkObj.Get(u => u.artworkId == id, includeProperties: "category,applicationUser");
                    return View(artworkVM);
                }

            }

        [HttpPost]
        public IActionResult Upsert(ArtworkVM artworkVM, IFormFile? file)
        {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            try
            {
                if (ModelState.IsValid)
                {
                    string wwwRootPath = _webHostEnvironment.WebRootPath;
                    
                    if (file != null)
                    {
                        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                        string productPath = Path.Combine(wwwRootPath, @"image\artwork");

                        if (!string.IsNullOrEmpty(artworkVM.artwork.imageUrl))
                        {
                            // Delete the old image
                            var oldImagePath = Path.Combine(wwwRootPath, artworkVM.artwork.imageUrl.TrimStart('\\'));

                            if (System.IO.File.Exists(oldImagePath))
                            {
                                System.IO.File.Delete(oldImagePath);
                            }
                        }
                        using (var fileStream = new FileStream(Path.Combine(productPath, fileName), FileMode.Create))
                        {
                            file.CopyTo(fileStream);
                        }

                        artworkVM.artwork.imageUrl = @"\image\artwork\" + fileName;
                    }
                    if (artworkVM.artwork.artworkId == 0)
                    {
               
                        // Add product
                        _unitOfWork.ArtworkObj.Add(artworkVM.artwork);
                        _unitOfWork.Save();
                        artworkVM.artwork.artistID = userId;
                        _unitOfWork.ArtworkObj.Update(artworkVM.artwork);

                        _unitOfWork.Save();


                        TempData["success"] = "Artwork created successfully";
                    }
                    else
                    {
                        // Update product
                        _unitOfWork.ArtworkObj.Update(artworkVM.artwork);

                        _unitOfWork.Save();

                        TempData["success"] = "Artwork updated successfully";
                    }
                    return RedirectToAction("Index", "Artwork");
                }
                else
                {
                    artworkVM.categoryList = _unitOfWork.CategoryObj.GetAll().Select(u => new SelectListItem
                    {
                        Text = u.categoryName,
                        Value = u.categoryId.ToString(),
                    });

                    return View(artworkVM);
                }
            }
            catch (Exception ex)
            {
                // Log the exception or handle it appropriately
                TempData["error"] = $"An error occurred: {ex.Message}";
                return RedirectToAction("Index", "Artwork");
            }
        }


        #region API CALLS
        [HttpGet]
            public IActionResult GetAll()
            {
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

            List<Artwork> artworkList = _unitOfWork.ArtworkObj.GetAll(u => u.artistID == userId,includeProperties: "category,applicationUser").ToList();
                return Json(new { data = artworkList });
            }

            [HttpDelete]
            public IActionResult Delete(int? id)
            {
                var productToBeDeleted = _unitOfWork.ArtworkObj.Get(u => u.artworkId == id);
                if (productToBeDeleted == null)
                {
                    return Json(new { success = false, message = "Error during deleting" });
                }

                if (!string.IsNullOrEmpty(productToBeDeleted.imageUrl))
                {
                    var oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, productToBeDeleted.imageUrl.TrimStart('\\'));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                _unitOfWork.ArtworkObj.Remove(productToBeDeleted);
                _unitOfWork.Save();

                List<Artwork> listProduct = _unitOfWork.ArtworkObj.GetAll(includeProperties: "category,applicationUser").ToList();
                return Json(new { success = true, message = "Delete Successful" });
            }
            #endregion

        }
    }
