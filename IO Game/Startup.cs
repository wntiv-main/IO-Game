using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace IO_Game
{
    // What to do on server startup...
    public class Startup
    {
        // Something that looks pretty useful. But IDK.
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // Stores all the config values???
        public IConfiguration Configuration { get; }

        // Add services to be used in here (I don't think we need any)
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddControllers();
        }

        // Configure what to do when we recieve a request. This basically defines the order in which 
        // things need to be done to the request.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // If we're still coding this then...
            if (env.IsDevelopment())
            {
                // Give us nice error messages that tell us what went wrong.
                app.UseDeveloperExceptionPage();
            }

            // WebSockets are useful maybe...
            app.UseWebSockets();

            // I think this makes http go to https???
            app.UseHttpsRedirection();
            
            // Allows us to redirect users based on whatever, such as redirecting users trying to access a
            // non-existing game to an error page.
            app.UseRouting();

            // I think this just means that the client has to be a good boy.
            app.UseAuthorization();

            // This is where we define all of our redirecting, or "Routing"
            app.UseEndpoints(endpoints =>
            {
                // If the user is trying to go to /game or something like that
                endpoints.MapGet("/game", async context => {
                    // If the game doesn't exist
                    if (!context.Request.QueryString.HasValue || !Server.GameExists(context.Request.QueryString.ToString()[1..]))
                    {
                        // Write to the response based on the contents of this -v error page.
                        StreamReader sr = File.OpenText(env.WebRootPath + "/errors/GAME_NOT_FOUND.html");
                        string strContents = sr.ReadToEnd();
                        await context.Response.WriteAsync(strContents);
                        sr.Close();
                    }
                    // If it is a request to start a WebSocket then...
                    else if (context.WebSockets.IsWebSocketRequest)
                    {
                        // Accept the request
                        WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                        // Create a player representing the user
                        Game.Player player = new Game.Player();
                        // Add them to the right game
                        Server.GetGame(context.Request.QueryString.ToString()[1..]).AddPlayer(player);
                        // And handle any requests from the socket
                        await SocketHandler(webSocket, player);
                    }
                    else
                    {
                        // If its a valid game and not a WebSocket, then we can safely send the standard page.
                        StreamReader sr = File.OpenText(env.WebRootPath + "/game/index.html");
                        string strContents = sr.ReadToEnd();
                        await context.Response.WriteAsync(strContents);
                        sr.Close();
                    }
                });
                endpoints.MapPost("/findGame", async context =>
                {
                    Stream read = context.Request.BodyReader.AsStream();
                    byte[] buffer = new byte[1024 * 4];
                    read.Read(buffer, 0, 1024 * 4);
                    string body = System.Text.Encoding.UTF8.GetString(buffer).Replace("\0", "").Trim();
                    if (body.Length > 0 && Gamemodes.IsGamemode(body))
                    {
                        await context.Response.WriteAsync(Server.FindGame(new Gamemodes.Gamemode(body)).Id);
                    }
                    else
                    {
                        await context.Response.WriteAsync(Server.FindGame().Id);
                    }
                });
            });

            // This means that if nothing else happens then just use the normal file-system
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
        private async Task SocketHandler(WebSocket socket, Game.Player player)
        {
            try {
                // Store the result of the socket
                var inBuffer = new byte[1024 * 4];
                WebSocketReceiveResult result;
                // Wait for the result
                result = await socket.ReceiveAsync(new ArraySegment<byte>(inBuffer), CancellationToken.None);
                // And as long as the socket is open...
                while (!result.CloseStatus.HasValue)
                {
                    // Get the message and see what class Player needs to do with it
                    Message response = player.SocketHandler(System.Text.Json.JsonSerializer.Deserialize<Message>(System.Text.Encoding.UTF8.GetString(inBuffer).Replace("\0", "")));
                    // If we need to send anything back...
                    if (response.Type.Length > 0)
                    {
                        string reply = System.Text.Json.JsonSerializer.Serialize(response);
                        // Then send it back
                        var outBuffer = System.Text.Encoding.UTF8.GetBytes(reply);
                        await socket.SendAsync(new ArraySegment<byte>(outBuffer, 0, outBuffer.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                    }
                    // Wait for the next result
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(inBuffer), CancellationToken.None);
                }
                // If you die, I die too.  LOL.
                await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                player.joinedGame.RemovePlayer(player);
            }
            catch(Exception e)
            {
                await socket.CloseAsync(WebSocketCloseStatus.InvalidMessageType, "Could not process the request.", CancellationToken.None);
            }
        }
    }
}
