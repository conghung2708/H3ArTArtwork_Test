using H3ArT.DataAccess.Data;
using H3ArT.Models;
using H3ArT.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace H3ArTArtwork.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]

    public class UserController : Controller
    {
        private readonly ApplicationDbContext _db;

        public UserController(ApplicationDbContext db)
        {
            _db = db;
        }
        public IActionResult Index()
        {
            return View();
        }

        #region API CALLS
        [HttpGet]
        public IActionResult GetAll()
        {
            List<ApplicationUser> UserList = _db.ApplicationUsers.ToList();

            //Get the userRole
            var userRoles = _db.UserRoles.ToList();
            //Get the Role Name
            var roles = _db.Roles.ToList();
            foreach (var user in UserList)
            {
                var roleId = userRoles.FirstOrDefault(u => u.UserId == user.Id).RoleId;
                user.Role = roles.FirstOrDefault(u => u.Id == roleId).Name;

            }
            return Json(new { data = UserList });
        }

        [HttpPost]
        public IActionResult LockUnlock([FromBody] string id)
        {
            var isAdmin = IsAdmin(id);
            if (isAdmin)
            {
                return Json(new { success = false, message = "Admin user cannot be locked/unlocked." });
            }
            var objFromDb = _db.ApplicationUsers.FirstOrDefault(u => u.Id == id);
            if (objFromDb == null)
            {
                return Json(new { success = false, message = "Error while Locking/Unclocking" });
            }

            if (objFromDb.LockoutEnd != null && objFromDb.LockoutEnd > DateTime.Now)
            {
                //User is currently locked and we need to unlock them
                objFromDb.LockoutEnd = DateTime.Now;
            }
            else
            {
                objFromDb.LockoutEnd = DateTime.Now.AddYears(1000);
            }
            _db.SaveChanges();
            return Json(new { success = true, message = "Operation Successful" });
        }

        private bool IsAdmin(string userId)
        {
            var userRole = _db.UserRoles.FirstOrDefault(u => u.UserId == userId);
            if (userRole != null)
            {
                var roleName = _db.Roles.FirstOrDefault(r => r.Id == userRole.RoleId)?.Name;
                return roleName == SD.Role_Admin;
            }
            return false;
        }
        #endregion
    }

}
