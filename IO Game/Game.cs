using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;

namespace IO_Game
{
    public class Game
    {
        public static class Gamemodes
        {
            static Gamemode FFA = new Gamemode("FFA");
            static Gamemode CTF = new Gamemode("CTF");
            public class Gamemode
            {
                private readonly string gamemode;
                public Gamemode(string mode)
                {
                    gamemode = mode;
                }
            }
        }
        public class Item
        {
            //...
        }
        private class Shop
        {
            public class ShopItem
            {
                private Heavy price;
                private Item item;
                public ShopItem(Heavy p, Item i)
                {
                    price = p;
                    item = i;
                }
                public Item Buy(Heavy budget)
                {
                    if(budget.Compare(price) != "<")
                    {
                        budget.Subtract(price);
                        return item;
                    }
                    throw new Exception("CANNOT_AFFORD");
                }
            }
            private List<ShopItem> shopItems = new List<ShopItem>();
            public void AddItem(ShopItem item) => shopItems.Add(item);
        }
        private class Player
        {
            private class Position
            {
                private float x;
                private float y;
                public Position()
                {
                    x = 0;
                    y = 0;
                }
                public Position(float X, float Y)
                {
                    x = X;
                    y = Y;
                }
            }
            private Task socketHandler;
            public Player(WebSocket s)
            {
                socket = s;
                socketHandler = SocketHandler();
            }
            private readonly WebSocket socket;
            private Heavy size = new Heavy();
            private Dictionary<int, Item> inventory = new Dictionary<int, Item>();
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
        public readonly Gamemodes.Gamemode gamemode;
        private Shop shop = new Shop();
        private List<Player> players = new List<Player>();
        public Game() { 
            
        }
        public Game(Gamemodes.Gamemode mode)
        {
            gamemode = mode;
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
        private static List<Game> games = new List<Game>();
        public static bool CanJoin(ID gameID) { return true; }
        public static bool CanJoin(string gameId) { return true; }
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
        public static ID FindGame()
        {
            for(var i = 0; i < games.Count; i++)
            {
                if(CanJoin(games[i].Id))
                {
                    return games[i].Id;
                }
            }
            Game g = new Game();
            games.Add(g);
            return g.Id;
        }
        public static ID FindGame(Game.Gamemodes.Gamemode gamemode)
        {
            for (var i = 0; i < games.Count; i++)
            {
                if (CanJoin(games[i].Id) && gamemode == games[i].gamemode)
                {
                    return games[i].Id;
                }
            }
            Game g = new Game(gamemode);
            games.Add(g);
            return g.Id;
        }
        public static Game GetGame(ID gameID)
        {
            for (var i = 0; i < games.Count; i++)
            {
                if (games[i].Id.Matches(gameID))
                {
                    return games[i];
                }
            }
            throw new Exception("GAME_NOT_FOUND");
        }
        public static Game GetGame(string gameID)
        {
            for (var i = 0; i < games.Count; i++)
            {
                if (games[i].Id.Matches(gameID))
                {
                    return games[i];
                }
            }
            throw new Exception("GAME_NOT_FOUND");
        }
    }
}
