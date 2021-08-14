using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;

namespace IO_Game
{
    public class Game
    {
        private class Player
        {
            public Player(WebSocket s)
            {
                socket = s;
            }
            private readonly WebSocket socket;
            private async Task SocketHandler()
            {
                var buffer = new byte[1024 * 4];
                WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                while (!result.CloseStatus.HasValue)
                {
                    await socket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }
                await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        }
        public readonly ID Id;
        private List<Player> players = new List<Player>();
        public Game() { 
            
        }
        public void AddPlayer(WebSocket socket)
        {
            players.Add(new Player(socket));
        }
    }
    public class ID
    {
        readonly string Id;
        public ID()
        {
            Id = "";
        }
        public bool Matches(string id)
        {
            return Id == id;
        }
        public bool Matches(ID id)
        {
            return id.Matches(Id);
        }
    }
    public static class Server
    {
        public static bool GameExists(ID gameID) {
            for(var i = 0; i < games.Count; i++)
            {
                if (games[i].Id.Matches(gameID))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool GameExists(string gameID)
        {
            for (var i = 0; i < games.Count; i++)
            {
                if (games[i].Id.Matches(gameID))
                {
                    return true;
                }
            }
            return false;
        }
        private static List<Game> games = new List<Game>();
        public static ID FindGame()
        {
            for(var i = 0; i < games.Count; i++)
            {
                if(true/*Can game be joined?*/)
                {
                    return games[i].Id;
                }
            }
            Game g = new Game();
            games.Add(g);
            return g.Id;
        }
    }
}
