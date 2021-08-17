using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;
using System.Net.WebSockets;

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
                    // No cache (due to the constant changing between error pages and game pages etc)
                    context.Response.Headers.Add("Cache-Control", "no-cache");
                    context.Response.Headers.Add("Pragma", "no-cache");
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
                        // And pass it over to our amazing friend, the class Game
                        Server.GetGame(context.Request.QueryString.ToString()[1..]).AddPlayer(webSocket);
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
            });

            // This means that if nothing else happens then just use the normal file-system
            app.UseDefaultFiles();
            app.UseStaticFiles();
        }
    }
}
