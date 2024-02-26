using H3ArT.DataAccess.Repository.IRepository;
using H3ArT.Models.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using H3ArT.Models.ViewModels;
using H3ArT.Utility;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace H3ArTArtwork.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
        }

        public IActionResult Index()
        {
            IEnumerable<Artwork> artworkList = _unitOfWork.ArtworkObj.GetAll(includeProperties: "category");
            return View(artworkList);
        }

        public IActionResult Details(int artworkId)
        {

            Artwork artworkFromDb = _unitOfWork.ArtworkObj.Get(u => u.artworkId == artworkId, includeProperties: "applicationUser");
            ShoppingCart shoppingCart = new()
            {
                artwork = artworkFromDb,
                count = 1,
                artworkID = artworkId,
                RelatedArtworks = _unitOfWork.ArtworkObj.GetAll(includeProperties: "category"),
                artistID = artworkFromDb.artistID
            };

            return View(shoppingCart);
        }

        [HttpPost]
        [Authorize] //return login
        public IActionResult Details(ShoppingCart shoppingCart)
        {
            // Set count to 1 to ensure it's always equal to 1
            shoppingCart.count = 1;

            //get the id
            var claimsIdentity = (ClaimsIdentity)User.Identity;
            var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;
            Artwork artwork = _unitOfWork.ArtworkObj.Get(u => u.artworkId == shoppingCart.artworkID);
            shoppingCart.buyerID = userId;
            shoppingCart.artistID = artwork.artistID;
            shoppingCart.isNew = true;

            ShoppingCart cartFromDb = _unitOfWork.ShoppingCartObj.Get(u => u.buyerID == userId && u.artworkID == shoppingCart.artworkID);   
            if (cartFromDb != null)
            {
                //cartFromDb.Count += shoppingCart.Count;

                // Update count to 1 if it's already in the cart
                cartFromDb.count = 1;
                _unitOfWork.ShoppingCartObj.Update(cartFromDb);
                _unitOfWork.Save();
            }
            else
            {
                //add cart record
                _unitOfWork.ShoppingCartObj.Add(shoppingCart);
               
                _unitOfWork.Save();
              
            }

            TempData["success"] = "Cart updated successfully";


            return RedirectToAction(nameof(Index));
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
