using H3ArT.DataAccess.Data;
using H3ArT.DataAccess.Repository.IRepository;
using H3ArT.Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace H3ArT.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        //Eg public ICategoryRepository CategoryRepository {get;private set;}
        public ICategoryRepository CategoryObj { get; private set; }
        public IArtworkRepository ArtworkObj { get; private set; }
        public IShoppingCartRepository ShoppingCartObj { get; private set; }
        public IApplicationUserRepository ApplicationUserObj { get; private set; }
        public IOrderHeaderRepository OrderHeaderObj { get; private set; }
        public IOrderDetailRepository OrderDetailObj { get; private set; }
        private readonly ApplicationDbContext _db;

        public UnitOfWork(ApplicationDbContext db) 
        {
            _db = db;
            CategoryObj = new CategoryRepository(_db);
            ArtworkObj = new ArtworkRepository(_db);
            ShoppingCartObj = new ShoppingCartRepository(_db);
            ApplicationUserObj = new ApplicationUserRepository(_db);
            OrderHeaderObj = new OrderHeaderRepository(_db);
            OrderDetailObj = new OrderDetailRepository(_db);
            //Eg Category = new CategoryRepository(_db);

        }



        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
