using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using socket;
using System.Threading.Tasks;

public class HomeController : Controller
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationHub> _logger;

    public HomeController(IHubContext<NotificationHub> hubContext, ILogger<NotificationHub> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] string message)
    {
        _logger.LogInformation(message);
        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
        return Ok();
    }
}

