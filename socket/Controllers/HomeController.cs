using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using socket;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

//public class HomeController : Controller
//{
//    private readonly IHubContext<NotificationHub> _hubContext;
//    private readonly ILogger<NotificationHub> _logger;

//    public HomeController(IHubContext<NotificationHub> hubContext, ILogger<NotificationHub> logger)
//    {
//        _hubContext = hubContext;
//        _logger = logger;
//    }

//    public IActionResult Index()
//    {
//        return View();
//    }

//    [HttpPost]
//    public async Task<IActionResult> SendNotification([FromBody] string message)
//    {
//        _logger.LogInformation(message);
//        await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
//        return Ok();
//    }
//}

public class HomeController : Controller
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationHub> _logger;
    private readonly string _connectionString;

    public HomeController(IHubContext<NotificationHub> hubContext, string connectionString, ILogger<NotificationHub> logger)
    {
        _hubContext = hubContext;
        _connectionString = connectionString;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> SendNotification([FromBody] string message)
    {
        try
        {
            _logger.LogInformation(message);
            AddNotificationToDatabase(message);

            // Notify clients about the change
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error in SendNotification: {ex.Message}");
            return StatusCode(500, "Internal Server Error");
        }
    }


    private void AddNotificationToDatabase(string message)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var command = new SqlCommand("INSERT INTO Notifications (Message, CreatedAt) VALUES (@Message, @CreatedAt)", connection))
            {
                command.Parameters.AddWithValue("@Message", message);
                command.Parameters.AddWithValue("@CreatedAt", DateTime.Now);

                command.ExecuteNonQuery();
            }
        }
    }

    [HttpGet]
    public IActionResult GetNotifications()
    {
        _logger.LogInformation("GetNotifications endpoint called");
        var notifications = GetNotificationsFromDatabase();
        _logger.LogInformation("GetNotifications data"+notifications);
        return Ok(notifications);
    }

    private List<string> GetNotificationsFromDatabase()
    {
        var notifications = new List<string>();

        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var command = new SqlCommand("SELECT Message FROM Notifications", connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        notifications.Add(reader.GetString(0));
                    }
                }
            }
        }

        return notifications;
    }

    [HttpDelete]
    public IActionResult DeleteNotifications()
    {
        ClearDatabaseNotifications();
        return Ok();
    }

    private void ClearDatabaseNotifications()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var command = new SqlCommand("DELETE FROM Notifications", connection))
            {
                command.ExecuteNonQuery();
            }
        }
    }
}