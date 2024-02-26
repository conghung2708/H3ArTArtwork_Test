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
    public class ArtworkRepository : Repository<Artwork>, IArtworkRepository
    {
        private readonly ApplicationDbContext _db;
        public ArtworkRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(Artwork artwork)
        {
            var artworkFromDb = _db.TblArtwork.FirstOrDefault(u => u.artworkId == artwork.artworkId);
            if (artworkFromDb != null )
            {
                artworkFromDb.title = artwork.title;
                artworkFromDb.artistID = artwork.artistID;
                artworkFromDb.price = artwork.price;
                artworkFromDb.description = artwork.description;
                artworkFromDb.isPremium = artwork.isPremium;
                artworkFromDb.categoryID = artwork.categoryID;
                if(artwork.imageUrl != null )
                {
                    artworkFromDb.imageUrl = artwork.imageUrl;

                }
            }
        }


    }
}
