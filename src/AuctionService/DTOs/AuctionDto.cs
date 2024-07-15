using AucionService.Entities;

namespace AuctionService.DTOs
{
    public class AuctionDto
    {
        public Guid Id { get; set; }
        public int ReservePrice { get; set; }
        public string Seller { get; set; }
        public string Winner { get; set; }
        public int SoldAmount { get; set; }
        public int CurrentHighBid { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }
        public DateTime AuctionEnd { get; set; }
        public string  Status { get; set; }
        public Item Item { get; set; }
    }
}
