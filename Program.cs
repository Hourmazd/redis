namespace SampleRedisCacheApp
{
    using StackExchange.Redis;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Timers;

    internal class Program
    {
        // Redis Cache
        static IDatabase? redisCache = null;

        // User data
        static UserInput? userInput = null;

        static async Task Main(string[] args)
        {
            // connection string to your Redis Cache    
            string connectionString = "mohicache.redis.cache.windows.net:6380,password=BMvKm60R51Y83byXVeQhGTgw8P8aEtN3mAzCaLKOBJ4=,ssl=True,abortConnect=False"; // "REDIS_CONNECTION_STRING";

            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine("-------------- Hello AZ-204 Warrior -------------------");
            Console.WriteLine("-------------------------------------------------------");
            Console.WriteLine(string.Empty);

            var maindb = await GetMainDb(string.Empty);
            Console.WriteLine(string.Empty);
            redisCache = await GetRedisCache(connectionString);

            // Snippet below executes a PING to test the server connection
            var result = await redisCache.ExecuteAsync("ping");
            Console.WriteLine($"PING = {result.Type} : {result}");

            Console.WriteLine(string.Empty);
            Console.WriteLine("All done. Ready.");
            Console.WriteLine("------------------------------------------------------");
            Console.WriteLine(string.Empty);

            userInput = GetFakeUserInput();

            Console.WriteLine(string.Empty);

            Console.WriteLine("Starting a loop to fetch key-value ...");

            await Task.Delay(1000);

            while (true)
            {
                var value = await GetValue(userInput.Key);
            }
        }

        static UserInput GetFakeUserInput()
        {
            return new UserInput() { Key = "test-key", Value = "tets-value", Ttl = 3 };
        }

        static UserInput GetUserInput()
        {
            Console.Write("Insert the key:");
            var key = Console.ReadLine();

            Console.Write("Insert the value:");
            var value = Console.ReadLine();

            Console.Write("Insert time to live (second):");
            var ttl = Console.ReadLine();

            return new UserInput() { Key = key, Value = value, Ttl = int.Parse(ttl) };
        }

        static async Task<IDatabase> GetRedisCache(string connectionString)
        {
            Console.WriteLine("Connecting to the redis cache ...");

            try
            {
                var cache = await ConnectionMultiplexer.ConnectAsync(connectionString);

                Console.WriteLine("Redis cache is ready.");

                return cache.GetDatabase(db: 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        static async Task<string> GetMainDb(string connectionString)
        {
            Console.WriteLine("Connecting to the main db ...");

            await Task.Delay(1000);

            Console.WriteLine("DB is ready.");

            return string.Empty;
        }

        static async Task<string> GetValue(string key)
        {
            var timer = Stopwatch.StartNew();
            var fetchFromCatch = true;

            var result = await GetValueFromCache(key);

            if (result is null)
            {
                result = await GetValueFromDb(key);

                var redisResult = await SetValuetoCache(userInput.Key, userInput.Value, userInput.Ttl);

                fetchFromCatch = false;
            }

            timer.Stop();

            Console.WriteLine($"Value: {result}; Fetch time: {timer.ElapsedMilliseconds} ms; Source: {(fetchFromCatch ? "Cache" : "DB")}");

            return result;
        }

        static async Task<string> GetValueFromDb(string key)
        {
            //Console.WriteLine("Getting value from db ...");

            await Task.Delay(1000);

            return userInput.Value;
        }

        static async Task<string?> GetValueFromCache(string key)
        {
            //Console.WriteLine("Getting value from redis cache ...");

            return await redisCache.StringGetAsync(key);
        }

        static async Task<bool> SetValuetoCache(string key, string value, int ttlSecond)
        {
            //Console.WriteLine("Setting value to redis cache ...");

            // Call StringSetAsync on the IDatabase object to set the key-value
            return await redisCache.StringSetAsync(key, value, TimeSpan.FromSeconds(ttlSecond));
        }
    }

    internal class UserInput
    {
        public string? Key { get; set; }
        public string? Value { get; set; }
        public int Ttl { get; set; }
    }
}