using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

var host = Environment.GetEnvironmentVariable("STATS_CACHE_SERVICE_HOST") ?? throw new InvalidDataException("Must specify host");

if (!int.TryParse(Environment.GetEnvironmentVariable("STATS_CACHE_SERVICE_PORT"), out var port))
{
    port = 80;
}

if (!bool.TryParse(Environment.GetEnvironmentVariable("REDIS_SSL"), out var ssl))
{
    ssl = false;
}

var key = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

var redis = ConnectionMultiplexer.Connect(new ConfigurationOptions
{
    EndPoints =
    {
        { host, port },
    },
    Password = key,
    Ssl = ssl
});

redis.ConnectionFailed += (sender, e) => Console.WriteLine($"Redis Client Error: {e}");

app.MapGet("/stats", async () =>
{
    var created = 0;
    var completed = 0;
    var deleted = 0;
    try
    {
        var cache = redis.GetDatabase();
        created = (int)await cache.StringGetAsync("todosCreated");
        completed = (int)await cache.StringGetAsync("todosCompleted");
        deleted = (int)await cache.StringGetAsync("todosDeleted");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
    return new { todosCreated = created, todosCompleted = completed, todosDeleted = deleted };
});

app.Run();