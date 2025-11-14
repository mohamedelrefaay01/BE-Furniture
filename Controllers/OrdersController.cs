using Home_furnishings.Models;
using Home_furnishings.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Home_furnishings.Controllers
{
    public class OrdersController : Controller
    {
        private readonly Context _context;

        public OrdersController(Context context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .OrderByDescending(o => o.OrderDate)
                .Select(o => new OrderViewModel
                {
                    OrderId = o.OrderId,
                    OrderDate = o.OrderDate,
                    OrderStatus = o.OrderStatus,
                    TotalAmount = o.TotalAmount,
                    ShippingAddress = o.ShippingAddress,
                    ShippingCity = o.ShippingCity,
                    PhoneNumber = o.PhoneNumber
                })
                .ToListAsync();

            return View(orders);
        }

      
        public async Task<IActionResult> Details(int id)
        {
            var order = await _context.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id);

            if (order == null)
                return NotFound();

            var viewModel = new OrderViewModel
            {
                OrderId = order.OrderId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                TotalAmount = order.TotalAmount,
                ShippingAddress = order.ShippingAddress,
                ShippingCity = order.ShippingCity,
                PhoneNumber = order.PhoneNumber,
                OrderItems = order.OrderItems.Select(oi => new OrderItemViewModel
                {
                    ProductId = oi.ProductId,
                    ProductName = oi.Product?.Name ?? "N/A",
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    Discount = oi.Discount
                }).ToList()
            };

            return View(viewModel);
        }
    }
}


