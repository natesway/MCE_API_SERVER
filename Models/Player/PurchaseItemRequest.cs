using System;
using System.Collections.Generic;
using System.Text;

namespace MCE_API_SERVER.Models.Player
{
    public class PurchaseItemRequest
    {
        public int expectedPurchasePrice { get; set; }
        public Guid itemId { get; set; }
    }
}
