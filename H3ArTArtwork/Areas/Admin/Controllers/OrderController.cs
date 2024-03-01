using H3ArT.DataAccess.Repository.IRepository;
using H3ArT.Models.Models;
using H3ArT.Models.ViewModels;
using H3ArT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Collections.Generic;
using System.Security.Claims;

namespace H3ArTArtwork.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize]

    public class OrderController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        [BindProperty]
        public OrderVM OrderVM { get; set; }
        public OrderController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public IActionResult Index()
        {
            return View();
        }
        public IActionResult Details(int orderId)
        {
            OrderVM = new()
            {
                orderHeader = _unitOfWork.OrderHeaderObj.Get(u => u.Id == orderId, includeProperties: "applicationUser"),
                orderDetail = _unitOfWork.OrderDetailObj.GetAll(u => u.orderHeaderId == orderId, includeProperties: "artwork")
            };
            return View(OrderVM);
        }
        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult UpdateOrderDetail()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderObj.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.name = OrderVM.orderHeader.name;
            orderHeaderFromDb.phoneNumber = OrderVM.orderHeader.phoneNumber;

            _unitOfWork.OrderHeaderObj.Update(orderHeaderFromDb);
            _unitOfWork.Save();

            TempData["Success"] = "Order Details Updated Successfully";

            return RedirectToAction(nameof(Details), new { orderId = orderHeaderFromDb.Id });
        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult StartProcessing()
        {
            _unitOfWork.OrderHeaderObj.UpdateStatus(OrderVM.orderHeader.Id, SD.StatusInProcess);
            _unitOfWork.Save();
            TempData["Success"] = "Order Details Updated Sucessfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });

        }

        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult DoneOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderObj.Get(u => u.Id == OrderVM.orderHeader.Id);
            orderHeaderFromDb.orderStatus = SD.StatusDone;

            _unitOfWork.OrderHeaderObj.Update(orderHeaderFromDb);
            _unitOfWork.Save();
            TempData["Success"] = "Order Shipped Sucessfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });

        }


        [HttpPost]
        [Authorize(Roles = SD.Role_Admin)]
        public IActionResult CancelOrder()
        {
            var orderHeaderFromDb = _unitOfWork.OrderHeaderObj.Get(u => u.Id == OrderVM.orderHeader.Id);


            if (orderHeaderFromDb.paymentStatus == SD.PaymentStatusApproved)
            {
                var options = new RefundCreateOptions
                {
                    Reason = RefundReasons.RequestedByCustomer,
                    PaymentIntent = orderHeaderFromDb.paymentIntentId
                };
                var service = new RefundService();
                Refund refund = service.Create(options);

                _unitOfWork.OrderHeaderObj.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
            }
            else
            {
                _unitOfWork.OrderHeaderObj.UpdateStatus(orderHeaderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
            }
            var orderDetails = _unitOfWork.OrderDetailObj.GetAll(u => u.orderHeaderId == orderHeaderFromDb.Id, includeProperties: "artwork");

            // Set the isBought property of artwork to false for all items in this order
            foreach (var orderDetail in orderDetails)
            {
                // Fetch the associated artwork for this order detail

                // Update the isBought property to false
                orderDetail.artwork.isBought = false;
                _unitOfWork.ArtworkObj.Update(orderDetail.artwork);

            }
            _unitOfWork.Save();
            TempData["Success"] = "Order Cancelled Sucessfully";
            return RedirectToAction(nameof(Details), new { orderId = OrderVM.orderHeader.Id });
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll(string status)
        {
            IEnumerable<OrderHeader> orderHeaderList;

            if (User.IsInRole(SD.Role_Admin))
            {
                orderHeaderList = _unitOfWork.OrderHeaderObj.GetAll(includeProperties: "applicationUser").ToList();
            }
            else
            {
                //get the id
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var userId = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier).Value;

                orderHeaderList = _unitOfWork.OrderHeaderObj.GetAll(u => u.applicationUserId == userId, includeProperties: "applicationUser");
            }
            switch (status)
            {
                //STATUS FILTER
                case "pending":
                    orderHeaderList = orderHeaderList.Where(u => u.paymentStatus == SD.PaymentStatusPending);
                    break;
                case "inprocess":
                    orderHeaderList = orderHeaderList.Where(u => u.orderStatus == SD.StatusInProcess);
                    break;
                case "completed":
                    orderHeaderList = orderHeaderList.Where(u => u.orderStatus == SD.StatusDone);
                    break;
                case "approved":
                    orderHeaderList = orderHeaderList.Where(u => u.orderStatus == SD.StatusApproved);
                    break;
                default:
                    break;
            }
            return Json(new { data = orderHeaderList });
        }

        #endregion
    }


}
