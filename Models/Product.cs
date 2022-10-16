using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Service.ShoppingCartApi.Models;

public class Product
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int ProductId { get; set; }
    public string Name { get; set; }
    [Range(1, 10000)]
    public double Price { get; set; }
    public string Description { get; set; }
    public string CategoryName { get; set; }
    public string ImageUrl { get; set; }
}