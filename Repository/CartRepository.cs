using AutoMapper;
using Mango.Service.ShoppingCartApi.Data;
using Mango.Service.ShoppingCartApi.Models;
using Mango.Service.ShoppingCartApi.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Service.ShoppingCartApi.Repository;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _applicationDbContext;
    private readonly IMapper _mapper;

    public CartRepository(ApplicationDbContext applicationDbContext, IMapper mapper)
    {
        _applicationDbContext = applicationDbContext;
        _mapper = mapper;
    }

    public async Task<bool> ApplyCoupon(string userId, string couponCode)
    {
        var cartFromDb = await _applicationDbContext.CartHeaders
            .FirstOrDefaultAsync(u => u.UserId == userId);
        cartFromDb.CouponCode = couponCode;
        _applicationDbContext.CartHeaders.Update(cartFromDb);
        await _applicationDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ClearCart(string userId)
    {
        var cartHeaderFromDb = await _applicationDbContext.CartHeaders
            .FirstOrDefaultAsync(u => u.UserId == userId);
        if (cartHeaderFromDb != null)
        {
            _applicationDbContext.CartDetails
                .RemoveRange(_applicationDbContext.CartDetails.Where(u => 
                                u.CartHeaderId == cartHeaderFromDb.CartHeaderId));
            _applicationDbContext.CartHeaders.Remove(cartHeaderFromDb);
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<CartDto> CreateUpdateCart(CartDto cartDto)
    {
        Cart cart = _mapper.Map<Cart>(cartDto);

        //check if product exists in database, if not create it!
        var productInDb = await _applicationDbContext.Products.FirstOrDefaultAsync(u =>
            u.ProductId == cartDto.CartDetails.FirstOrDefault().ProductId);
        
        if (productInDb == null)
        {
            _applicationDbContext.Products.Add(cart.CartDetails.FirstOrDefault().Product);
            await _applicationDbContext.SaveChangesAsync();
        }

        //check if header is null
        var cartHeaderInDb = await _applicationDbContext.CartHeaders.AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == cartDto.CartHeader.UserId);
        
        if (cartHeaderInDb == null)
        {
            //create header and details
            _applicationDbContext.CartHeaders.Add(cart.CartHeader);
            await _applicationDbContext.SaveChangesAsync();
            cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.CartHeaderId;
            cart.CartDetails.FirstOrDefault().Product = null;
            _applicationDbContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
            await _applicationDbContext.SaveChangesAsync();
        }
        else
        {
            //if header is not null check if details has same product
            var cartDetailsInDb = await _applicationDbContext.CartDetails.AsNoTracking()
                .FirstOrDefaultAsync(u => u.ProductId == cart.CartDetails.FirstOrDefault().ProductId 
                && u.CartHeaderId == cartHeaderInDb.CartHeaderId);
            
            if (cartDetailsInDb == null)
            {
                //create details
                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.CartHeaderId;
                cart.CartDetails.FirstOrDefault().Product = null;
                _applicationDbContext.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _applicationDbContext.SaveChangesAsync();
            }
            else
            {
                //update the count / cart details
                cart.CartDetails.FirstOrDefault().Product = null;
                cart.CartDetails.FirstOrDefault().Count += cartDetailsInDb.Count;
                cart.CartDetails.FirstOrDefault().CartDetailsId = cartDetailsInDb.CartDetailsId;
                cart.CartDetails.FirstOrDefault().CartHeaderId = cartDetailsInDb.CartHeaderId;
                _applicationDbContext.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                await _applicationDbContext.SaveChangesAsync();
            }
        }
        return _mapper.Map<CartDto>(cart);
    }

    public async Task<CartDto> GetCartByUserId(string userId)
    {
        Cart cart = new()
        {
            CartHeader = await _applicationDbContext.CartHeaders
                .FirstOrDefaultAsync(u => u.UserId == userId)
        };

        cart.CartDetails = _applicationDbContext.CartDetails
            .Where(u => u.CartHeaderId == cart.CartHeader.CartHeaderId)
            .Include(u=>u.Product);

        return _mapper.Map<CartDto>(cart);
    }

    public async Task<bool> RemoveCoupon(string userId)
    {
        var cartFromDb = await _applicationDbContext.CartHeaders
            .FirstOrDefaultAsync(u => u.UserId == userId);
        cartFromDb.CouponCode = "";
        _applicationDbContext.CartHeaders.Update(cartFromDb);
        await _applicationDbContext.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RemoveFromCart(int cartDetailsId)
    {
        try
        {
            CartDetails cartDetails = await _applicationDbContext.CartDetails
                .FirstOrDefaultAsync(u => u.CartDetailsId == cartDetailsId);

            int totalCountOfCartItems = _applicationDbContext.CartDetails
                .Where(u => u.CartHeaderId == cartDetails.CartHeaderId).Count();

            _applicationDbContext.CartDetails.Remove(cartDetails);
            if (totalCountOfCartItems == 1)
            {
                var cartHeaderToRemove = await _applicationDbContext.CartHeaders
                    .FirstOrDefaultAsync(u => u.CartHeaderId == cartDetails.CartHeaderId);
                _applicationDbContext.CartHeaders.Remove(cartHeaderToRemove);
            }
            await _applicationDbContext.SaveChangesAsync();
            return true;
        }
        catch(Exception e)
        {
            return false;
        }
    }
}