using H3ArT.DataAccess.Repository.IRepository;
using H3ArT.Models;
using H3ArT.Models.Models;
using H3ArT.Models.ViewModels;
using H3ArT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe.Checkout;
using System.Security.Claims;

namespace H3ArTArtwork.Areas.Customer.Controllers
{
	[Area("Customer")]
    [Authorize(Roles = "Customer, Creator")]
    public class CartController : Controller
	{
		private readonly IUnitOfWork _unitOfWork;
		[BindProperty]
		public ShoppingCartVM ShoppingCartVM { get; set; }
		public CartController(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}
		public IActionResult Index()
		{
			//get the id
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCartObj.GetAll(u => u.buyerID == userId, includeProperties: "artwork"),
				orderHeader = new()
			};
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.price = cart.artwork.price;
				ShoppingCartVM.orderHeader.orderTotal += cart.price;
			}
			return View(ShoppingCartVM);
		}

        public IActionResult Remove(int cartId)
        {
            var cartFromDb = _unitOfWork.ShoppingCartObj.Get(u => u.shoppingCartId == cartId);

            // Fetch the associated orderHeaderId using userId or any other relevant information
            var userId = cartFromDb.buyerID;
            var orderHeader = _unitOfWork.OrderHeaderObj.Get(o => o.applicationUserId == userId && o.orderStatus == SD.StatusPending);

            if (orderHeader != null)
            {
                // Remove associated OrderDetail record
                var orderDetail = _unitOfWork.OrderDetailObj.Get(od => od.artworkId == cartFromDb.artworkID
                                                                                  && od.orderHeaderId == orderHeader.Id);
                if (orderDetail != null)
                {
                    _unitOfWork.OrderDetailObj.Remove(orderDetail);
                }
            }

            _unitOfWork.ShoppingCartObj.Remove(cartFromDb);

            _unitOfWork.Save();

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Summary()
		{
			//get the id
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			ShoppingCartVM = new()
			{
				ShoppingCartList = _unitOfWork.ShoppingCartObj.GetAll(u => u.buyerID == userId, includeProperties: "artwork"),
				orderHeader = new()
			};
			ShoppingCartVM.orderHeader.applicationUser = _unitOfWork.ApplicationUserObj.Get(u => u.Id == userId);

			ShoppingCartVM.orderHeader.name = ShoppingCartVM.orderHeader.applicationUser.FullName;
			ShoppingCartVM.orderHeader.phoneNumber = ShoppingCartVM.orderHeader.applicationUser.PhoneNumber;
			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.price = cart.artwork.price;
				ShoppingCartVM.orderHeader.orderTotal += cart.price;
			}
			return View(ShoppingCartVM);
		}

		[HttpPost]
		[ActionName("Summary")]
		public IActionResult SummaryPOST()
		{
			//get the id
			var claimsIdentity = (ClaimsIdentity)User.Identity;
			var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

			//ShoppingCartVM will automatically be populated


			ShoppingCartVM.ShoppingCartList = _unitOfWork.ShoppingCartObj.GetAll(u => u.buyerID == userId, includeProperties: "artwork");


			ApplicationUser applicationUser = _unitOfWork.ApplicationUserObj.Get(u => u.Id == userId);

			ShoppingCartVM.orderHeader.orderDate = System.DateTime.Now;
			ShoppingCartVM.orderHeader.applicationUserId = userId;


			foreach (var cart in ShoppingCartVM.ShoppingCartList)
			{
				cart.price = cart.artwork.price;
				ShoppingCartVM.orderHeader.orderTotal += cart.price;
			}
			if (!ModelState.IsValid)
			{
                // If model state is not valid, return the view with validation errors
                return View(ShoppingCartVM); // or any other suitable action result
			}
            var existingOrder = _unitOfWork.OrderHeaderObj.Get(o => o.applicationUserId == userId && o.paymentStatus == SD.PaymentStatusPending);

			if (existingOrder != null)
			{
				ShoppingCartVM.orderHeader = existingOrder;
				_unitOfWork.OrderHeaderObj.Update(existingOrder);
                _unitOfWork.Save();
                var newShoppingCartItems = ShoppingCartVM.ShoppingCartList.Where(cart => cart.isNew);

                foreach (var cart in newShoppingCartItems)
                {
                    OrderDetail orderDetail = new OrderDetail
                    {
                        artworkId = cart.artworkID,
                        orderHeaderId = existingOrder.Id,
                        price = cart.price,
                        count = cart.count
                    };

                    _unitOfWork.OrderDetailObj.Add(orderDetail);
                    cart.isNew = false; // Reset isNew flag
                    _unitOfWork.ShoppingCartObj.Update(cart);
                }
            }
            else
			{ 
				//it is a regular customer account
				ShoppingCartVM.orderHeader.paymentStatus = SD.PaymentStatusPending;
				ShoppingCartVM.orderHeader.orderStatus = SD.StatusPending;

				_unitOfWork.OrderHeaderObj.Add(ShoppingCartVM.orderHeader);
                _unitOfWork.Save();
                foreach (var cart in ShoppingCartVM.ShoppingCartList)
                {
                    OrderDetail orderDetail = new()
                    {
                        artworkId = cart.artworkID,
                        orderHeaderId = ShoppingCartVM.orderHeader.Id,
                        price = cart.price,
                        count = cart.count
                    };
                    _unitOfWork.OrderDetailObj.Add(orderDetail);
                    _unitOfWork.Save();
                    cart.isNew = false; // Reset isNew flag
                    _unitOfWork.ShoppingCartObj.Update(cart);
                }

            }
			//stripe logic
			var domain = "https://localhost:7034/";
            var options = new SessionCreateOptions
            {
                SuccessUrl = domain + $"customer/cart/OrderConfirmation?id={ShoppingCartVM.orderHeader.Id}",
                CancelUrl = domain + "customer/cart/index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
            };
            foreach (var item in ShoppingCartVM.ShoppingCartList)
            {
                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(item.price * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.artwork.title
                        }
                    },
                    Quantity = item.count
                };
                options.LineItems.Add(sessionLineItem);
            }
            var service = new SessionService();
            //create sessionId and paymentIntentId
            Session session = service.Create(options);
            _unitOfWork.OrderHeaderObj.UpdateStripePaymentId(ShoppingCartVM.orderHeader.Id, session.Id, session.PaymentIntentId);
            _unitOfWork.Save();
            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
		}

		public IActionResult OrderConfirmation(int id)
		{
            OrderHeader orderHeader = _unitOfWork.OrderHeaderObj.Get(u => u.Id == id, includeProperties: "applicationUser");
           
                //this is an order by customer
                var service = new SessionService();
                Session session = service.Get(orderHeader.sessionId);

                if (session.PaymentStatus.ToLower() == "paid")
                {
                    _unitOfWork.OrderHeaderObj.UpdateStripePaymentId(id, session.Id, session.PaymentIntentId);
                    _unitOfWork.OrderHeaderObj.UpdateStatus(id, SD.StatusApproved, SD.PaymentStatusApproved);
                    _unitOfWork.Save();
                }
             
            
            List<ShoppingCart> shoppingCarts = _unitOfWork.ShoppingCartObj.GetAll(u => u.buyerID == orderHeader.applicationUserId).ToList();
            _unitOfWork.ShoppingCartObj.RemoveRange(shoppingCarts);
            _unitOfWork.Save();
            return View(id);
        }

	}
}
