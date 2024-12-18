using RealtyAPI;

var builder = WebApplication.CreateBuilder(args);

var startup = new Startup();
startup.ConfigureServices(builder.Services);

var app = builder.Build();
startup.Configure(app);


app.UseHttpsRedirection();

app.MapControllers();

app.Urls.Add("http://localhost:8000");
app.Run();
