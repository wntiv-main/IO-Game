using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace IO_Game
{
    // Represents all the gamemodes
    public static class Gamemodes
    {
        static readonly List<Gamemode> gamemodes = new List<Gamemode>();
        // Free for all
        static Gamemode FFA = new CreateGamemode("FFA");
        // Capture the flag
        static Gamemode CTF = new CreateGamemode("CTF");
        // Represents a single Gamemode
        public class Gamemode
        {
            protected string gamemode;
            public Gamemode(string mode)
            {
                gamemode = mode;
            }
            public override bool Equals(object mode)
            {
                if (mode is Gamemode) return gamemode == (mode as Gamemode).gamemode;
                else return this.Equals(mode);
            }
            public override int GetHashCode()
            {
                return gamemode.GetHashCode();
            }
        }
        private class CreateGamemode : Gamemode
        {
            public CreateGamemode(string mode) : base(mode)
            {
                gamemodes.Add(this);
            }
        }
        public static bool IsGamemode(Gamemode mode)
        {
            for (var i = 0; i < gamemodes.Count; i++)
            {
                if (gamemodes[i].Equals(mode))
                {
                    return true;
                }
            }
            return false;
        }
        public static bool IsGamemode(string mode){
            return IsGamemode(new Gamemode(mode));
        }
    }
    // Represents a game/arena
    public class Game
    {
        // Represents an item of some sort
        public class Item
        {
            //...
        }
        // Represents the shop in any given game
        private class Shop
        {
            // Represents an item in the shop
            public class ShopItem
            {
                // The price of the item, in a class Heavy
                private Heavy price;
                // The item, as a class Item
                private Item item;
                // Construct the variables
                public ShopItem(Heavy p, Item i)
                {
                    price = p;
                    item = i;
                }
                // Buy an item
                public Item Buy(Heavy budget)
                {
                    // Can we afford?
                    if(budget.Compare(price) != "<")
                    {
                        // Remove total sum of owed money from our budget
                        budget.Subtract(price);
                        // And return the item
                        return item;
                    }
                    // Otherwise shout at people
                    throw new Exception("CANNOT_AFFORD");
                }
            }
            // List of all the shop's items
            private List<ShopItem> shopItems = new List<ShopItem>();
            // Functionality to add MORE items
            public void AddItem(ShopItem item) => shopItems.Add(item);
        }
        // Represents a player
        public class Player
        {
            // When we create a player, they need to have a connected WebSocket
            // Not anymore tho.
            public Player()
            {
            }
            // What to do with our message????
            public string SocketHandler(string message)
            {
                // Stuff.
                Debug.WriteLine(message);
                return "hello";
            }
            // This players weight as a Heavy
            private Heavy size = new Heavy();
            // What do they have? in [slot name, item] forms
            // slot names such as "equipped.0, equipped.13, inventory.0, inventory.14"
            private Dictionary<string, Item> inventory = new Dictionary<string, Item>();
            // We need to listen to the user so we know what they're doing...
            
        }
        // Represents a position on the game's map
        private class Position
        {
            // X pos
            private float x;
            // Y pos
            private float y;
            // Construct with no params
            public Position()
            {
                x = 0;
                y = 0;
            }
            // Construct with pre-set numbers
            public Position(float X, float Y)
            {
                x = X;
                y = Y;
            }
        }
        // Game ID
        public readonly ID Id = new ID();
        // This game's gamemode
        public readonly Gamemodes.Gamemode gamemode;
        // This game's shop
        private Shop shop = new Shop();
        // List of all players
        private List<Player> players = new List<Player>();
        // Probable do something on construction
        public Game() { 
            
        }
        // If we want to construct a game of a certain gamemode
        public Game(Gamemodes.Gamemode mode)
        {
            gamemode = mode;
        }
        // Functionality to add a player to the game
        public void AddPlayer(Player player)
        {
            players.Add(player);
        }
    }
    // Represents a game ID
    public class ID
    {
        // The ID
        public readonly string Id;
        static List<string> reserved = new List<string>();
        static Random random = new Random();
        // Create a unique id
        public ID()
        {
            // Just hope we dont have more games than random values in the world.
            Id = random.Next().ToString();
            while (reserved.Contains(Id))
            {
                // Otherwise we're f***ed
                Id = random.Next().ToString();
            }
            reserved.Add(Id);
        }
        // Is this ID the same as that 'ID'?
        public bool Matches(string id)
        {
            // Maybe?
            return Id == id;
        }
        // Is this ID the same as that ID :)?
        public bool Matches(ID id)
        {
            // IDK, ask him... lol
            return id.Matches(Id);
        }
    }
    // We need somewhere to store everything...
    public static class Server
    {
        //All the games we -can- (might be able to) join
        private static List<Game> games = new List<Game>();
        // Can we join the game?
        public static bool CanJoin(ID gameID) { /*Yes, for now...*/ return true; }
        public static bool CanJoin(string gameId) { /*Yes, for now...*/ return true; }
        // Does this game even exist???
        public static bool GameExists(ID gameID) {
            // Loop through all the games
            for(var i = 0; i < games.Count; i++)
            {
                // Oh yeah, that's why I needed the clas ID.Matches() function
                if (games[i].Id.Matches(gameID))
                {
                    // Yes, that would be me
                    return true;
                }
            }
            // No, noone here likes him...
            return false;
        }
        // It's the EXACT same f***ing thing
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
        // Find me a game to play PLEASEEEEEEEEEEEEEEE...
        public static ID FindGame()
        {
            // Meet all our games
            for(var i = 0; i < games.Count; i++)
            {
                // This is {{games[i].name}}...
                // And he says...
                if(CanJoin(games[i].Id))
                {
                    // "You CAN play with me"
                    return games[i].Id;
                }
                // No, Im a sad'o who bullies people and loves exclusion
            }
            // Im sorry, but noone likes you
            // I'll genetically grow someone who does
            Game g = new Game();
            games.Add(g);
            return g.Id;
        }
        // Do the same hexing thing but this time we are the discriminatory one.
        public static ID FindGame(Gamemodes.Gamemode gamemode)
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
        // Get the Game object based upon it's ID
        public static Game GetGame(ID gameID)
        {
            // I cant be bothered commenting this.
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
