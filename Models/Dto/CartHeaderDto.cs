using System.ComponentModel.DataAnnotations;

namespace Mango.Service.ShoppingCartApi.Models.Dto;

public class CartHeaderDto
{
    public int CartHeaderId { get; set; }
    public string UserId { get; set; }
    public string CouponCode { get; set; }
}