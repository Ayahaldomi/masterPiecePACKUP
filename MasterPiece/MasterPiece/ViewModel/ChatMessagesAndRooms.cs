using MasterPiece.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MasterPiece.ViewModel
{
    public class ChatMessagesAndRooms
    {
        public IEnumerable<ChatRoom> Rooms { get; set; }
        public IEnumerable<ChatMessage> Messages { get; set; }
    }
}