using Microsoft.AspNet.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.Hubs
{
    // The Hub class has to inherit from the Microsoft.AspNet.SignalR.Hub.
    public class ChatHub : Hub
    {
        public void SendMessage(string senderType, string message, int chatRoomId)
        {
            // Broadcast the message to all clients in the chat room
            Clients.All.broadcastMessage(senderType, message, chatRoomId);
        }
    }
}