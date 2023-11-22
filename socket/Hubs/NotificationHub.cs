using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using System.Data.SqlClient;

//public class NotificationHub : Hub
//{
//    public async Task SendNotification(string message)
//    {
//        await Clients.All.SendAsync("ReceiveNotification", message);
//    }
//}

public class NotificationHub : Hub
{
    private readonly string _connectionString;

    public NotificationHub(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task SendMessage(string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", message);
    }

    public async Task CheckAndSendDatabaseChangeNotification()
    {
        var hasChanges = CheckDatabaseChanges();

        if (hasChanges)
        {
            var message = "Database has new notifications";
            await Clients.All.SendAsync("ReceiveNotification", message);

            // Clear notifications after sending
            ClearDatabaseNotifications();
        }
    }

    private bool CheckDatabaseChanges()
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();

            using (var command = new SqlCommand("SELECT COUNT(*) FROM Notifications", connection))
            {
                var result = command.ExecuteScalar();
                return (int)result > 0;
            }
        }
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

    public override Task OnConnectedAsync()
    {
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
        return base.OnConnectedAsync();
    }

    public override Task OnDisconnectedAsync(Exception exception)
    {
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
        return base.OnDisconnectedAsync(exception);
    }
}
