using Mango.MessageBus;
using Mango.Service.ShoppingCartApi.Messages;
using Mango.Service.ShoppingCartApi.Models.Dto;
using Mango.Service.ShoppingCartApi.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Service.ShoppingCartApi.Controllers;


[Route("api/cart")]
public class CartApiController : ControllerBase
{
    private readonly ICartRepository _cartRepository;
    private readonly IMessageBus _messageBus;
    protected ResponseDto _responseDto;

    public CartApiController(ICartRepository cartRepository, IMessageBus messageBus)
    {
        _cartRepository = cartRepository;
        _messageBus = messageBus;
        _responseDto = new ResponseDto();
    }

    [HttpGet("getcart/{userId}")]
    public async Task<object> GetCart(string userId)
    {
        try
        {
            CartDto cartDto = await _cartRepository.GetCartByUserId(userId);
            _responseDto.Result = cartDto;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }

    [HttpPost("addcart")]
    public async Task<object> AddCart([FromBody] CartDto cartDto)
    {
        try
        {
            CartDto cart = await _cartRepository.CreateUpdateCart(cartDto);
            _responseDto.Result = cart;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }

    [HttpPost("updatecart")]
    public async Task<object> UpdateCart([FromBody] CartDto cartDto)
    {
        try
        {
            CartDto cart = await _cartRepository.CreateUpdateCart(cartDto);
            _responseDto.Result = cart;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }

    [HttpPost("removecart")]
    public async Task<object> RemoveCart([FromBody]int cartId)
    {
        try
        {
            bool isSuccess = await _cartRepository.RemoveFromCart(cartId);
            _responseDto.Result = isSuccess;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }

    [HttpPost("applycoupon")]
    public async Task<object> ApplyCoupon([FromBody]CartDto cartDto)
    {
        try
        {
            bool isSuccess = await _cartRepository.ApplyCoupon(cartDto.CartHeader.UserId,
                cartDto.CartHeader.CouponCode);
            _responseDto.Result = isSuccess;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }

    [HttpPost("removecoupon")]
    public async Task<object> RemoveCoupon([FromBody]string userId)
    {
        try
        {
            bool isSuccess = await _cartRepository.RemoveCoupon(userId);
            _responseDto.Result = isSuccess;
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }

    [HttpPost("checkout")]
    public async Task<object> Checkout([FromBody]CheckoutHeaderDto checkoutHeader)
    {
        try
        {
            CartDto cartDto = await _cartRepository.GetCartByUserId(checkoutHeader.UserId);
            if (cartDto == null)
            {
                return BadRequest();
            }
            checkoutHeader.CartDetails = cartDto.CartDetails;
            // logic to add message to process order
            await _messageBus.PublishMessage(checkoutHeader, "checkoutmessagetopic");
        }
        catch(Exception ex)
        {
            _responseDto.IsSuccess = false;
            _responseDto.ErrorMessages = new List<string> { ex.ToString() };
        }
        return _responseDto;
    }
}