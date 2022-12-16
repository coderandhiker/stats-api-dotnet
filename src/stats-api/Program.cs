using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var host = Environment.GetEnvironmentVariable("STATS_CACHE_SERVICE_HOST");
var port = Environment.GetEnvironmentVariable("STATS_CACHE_SERVICE_PORT");
var tls = Environment.GetEnvironmentVariable("REDIS_SSL");
var key = Environment.GetEnvironmentVariable("REDIS_PASSWORD");

var redis = ConnectionMultiplexer.Connect($"{host}:{port},ssl={tls},password={key}");

redis.ConnectionFailed += Cache_ConnectionFailed;

static void Cache_ConnectionFailed(object? sender, ConnectionFailedEventArgs e) => 
	Console.WriteLine($"Redis Client Error: {e}");

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
	catch(Exception ex)
	{
		Console.WriteLine(ex);
	}
	return new { todosCreated = created, todosCompleted = completed, todosDeleted = deleted };
});

app.Run();