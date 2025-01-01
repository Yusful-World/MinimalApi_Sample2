using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using MinimalApi_Sample2.Data;
using MinimalApi_Sample2.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("name=DefaultConnection")
);



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 8).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

var message = builder.Configuration.GetValue<string>("message");
app.MapGet("/message", () => message);




app.MapGet("/users", async (ApplicationDbContext context) =>
{
    var users = await context.Users.ToListAsync();
    
    return TypedResults.Ok(users);
});


app.MapGet("/users/{id:int}", async Task<Results<Ok<User>, NotFound<string>>> (int id, ApplicationDbContext context) =>
{
    var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
    {
        return TypedResults.NotFound("user does not exist");
    }

    return TypedResults.Ok(user);

}).WithName("GetUser");


app.MapPost("/users", async (User user, ApplicationDbContext context) =>
{
    context.Add(user);
    await context.SaveChangesAsync();
    
    return TypedResults.Created($"/users/{user.Id}", user);

    //OR
    //return TypedResults.CreatedAtRoute(user, "GetUser", new { id = user.Id });
}).WithName("CreateUser");



app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
