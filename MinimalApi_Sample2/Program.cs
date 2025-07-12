using Azure.Core;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.OutputCaching;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using MinimalApi_Sample2.Data;
using MinimalApi_Sample2.Dtos;
using MinimalApi_Sample2.Models;
using MinimalApi_Sample2.Services;
using MinimalApi_Sample2.Services.IServices;
using System.Text;
using System.Xml;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddOutputCache();

builder.Services.AddStackExchangeRedisOutputCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("redis");
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer("name=DefaultConnection")
);

builder.Services.AddIdentity<User, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddScoped<IUserAccountService, UserAccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddGrpc();




// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo { Title = "Demo Auth Service", Version = "v1" });
    option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Scheme = "Bearer",
        Type = SecuritySchemeType.ApiKey,
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. " +
                     "\r\n\r\n Enter 'Bearer' [space] and then your token in the text input below. " +
                     "\r\n\r\n Example: \"Bearer YqNHJIiokdjeopDlkw\"",
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[]{}

        }
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
    options.DefaultChallengeScheme =
    options.DefaultForbidScheme =
    options.DefaultScheme =
    options.DefaultSignInScheme =
    options.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(10),
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["JWT:SigningKey"])
        )
    };
    //options.Events = new JwtBearerEvents
    //{
    //    OnMessageReceived = context =>
    //    {
    //        StringValue authHeader;
    //        if (context.Request.Headers.TryGetValue("Authorization", out authHeader))
    //        {
    //            var accessToken = authHeader.FirstOrDefault();
    //            if (!string.IsNullOrEmpty(accessToken) && accessToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    //            {
    //                context.Token = accessToken.Substring("Bearer ".Length).Trim();
    //            }
    //        }
            
    //        return Task.CompletedTask;
    //    },
    //    OnAuthenticationFailed = context =>
    //    {
    //        context.Response.Headers.Add("Grpc-Status", StatusCode.Unauthenticated.ToString("d"));
    //        context.Response.Headers.Add("Grpc-Message", context.Exception.Message);
    //        return Task.CompletedTask;
    //    }
    //};
});

builder.Services.AddAuthorization();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await UserRoleSeed.SeedRolesAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

//app.UseOutputCache();

app.MapGrpcService<GetUserDetails>();


var message = builder.Configuration.GetValue<string>("message");
app.MapGet("/message", () => message);

app.MapGet("/", () => "User gRPC is running");


app.MapGet("/get_users", async (ApplicationDbContext context) =>
{
    var users = await context.Users.ToListAsync();

    return TypedResults.Ok(users);
}).CacheOutput(c => c.Expire(TimeSpan.FromSeconds(120)).Tag("Get-users"));


app.MapGet("/get_user/{id}", async Task<Results<Ok<User>, NotFound<string>>> (string id, ApplicationDbContext context) =>
{
    var user = await context.Users.FirstOrDefaultAsync(u => u.Id == id);

    if (user == null)
    {
        return TypedResults.NotFound("user does not exist");
    }

    return TypedResults.Ok(user);

}).WithName("GetUser");


app.MapPost("/sign_up", async (
    UserSignUpRequestDto userSignUpRequest, 
    IUserAccountService userAccountService, 
    IOutputCacheStore outputCacheStore
) =>
{
    var createUser = await userAccountService.UserSignUp(userSignUpRequest);

    if (!createUser.Success)
        return Results.BadRequest(createUser.Errors);

    //await outputCacheStore.EvictByTagAsync("Get-users", default);

    return TypedResults.Created($"/users/{createUser.Data.Id}", createUser.Data);
})
.WithName("SignUpUser");

app.MapPost("/login", async (
    UserLoginRequestDto loginRequest,
    IUserAccountService userAccountService
) =>
{
    var result = await userAccountService.UserLogIn(loginRequest);

    if (!result.Success)
        return Results.BadRequest(result.Errors);

    return Results.Ok(result.Data);
})
.WithName("LoginUser");


app.MapPatch("/update_user/{id}", async Task<Results<BadRequest<string>, NotFound<string>, NoContent>>
    (Guid id, ApplicationDbContext context, UpdateUserDto updateDto, IOutputCacheStore outputCacheStore) =>
{
    var existingUser = await context.Users.FindAsync(id);
    if (existingUser == null)
    {
        return TypedResults.BadRequest("id does not match any user");
    }

    existingUser.FirstName = updateDto.FirstName;
    existingUser.LastName = updateDto.LastName;
    existingUser.PhoneNumber = updateDto.PhoneNumber;
    existingUser.Email = updateDto.Email;
    
    await context.SaveChangesAsync();
    //await outputCacheStore.EvictByTagAsync("Get-users", default);


    return TypedResults.NoContent();
}).RequireAuthorization();

app.MapDelete("/delete_user/{id}", async Task<Results<NotFound<string>, NoContent>> (string id, ApplicationDbContext context, IOutputCacheStore outputCacheStore) =>
{
    var deletedRecord = await context.Users.Where(u => u.Id == id).ExecuteDeleteAsync();

    if (deletedRecord == 0)
    {
        return TypedResults.NotFound("user does not exist");
    }

    // await outputCacheStore.EvictByTagAsync("Get-users", default);

    return TypedResults.NoContent();
}).RequireAuthorization();

app.Run();
