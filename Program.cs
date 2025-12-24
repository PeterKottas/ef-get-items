namespace FilteringTest
{
    using FilteringTest.Data;
    using Microsoft.EntityFrameworkCore;
    using System.Text.Json.Serialization;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContextFactory<LibraryDbContext>(options =>
                options.UseInMemoryDatabase("LibraryDb"));

            builder.Services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Initialize database with seed data
            InitializeDatabase(app);

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }

        private static void InitializeDatabase(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var contextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<LibraryDbContext>>();
            using var context = contextFactory.CreateDbContext();
            context.Database.EnsureCreated();
        }
    }
}
