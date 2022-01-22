using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moneyes.Server;
using Moneyes.Server.Data;
using Moneyes.Server.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders()
    .AddDefaultUI();
builder.Services.AddControllersWithViews();

#region JWT

var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>();
builder.Services.AddSingleton(jwtSettings);

builder.Services.AddScoped<CustomCookieAuthenticationEvents>();

builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
builder.Services.AddScoped<IRefreshTokenService, JwtRefreshTokenService>();
builder.Services.AddScoped<IRefreshTokenValidator, JwtRefreshTokenValidator>();
builder.Services.AddScoped<IAccessTokenValidator, JwtAccessTokenValidator>();
builder.Services.AddScoped<ITokenAuthenticateService, JwtTokenAuthenticateService>();

#endregion

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "smart";
    options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
})
    // use smart policy scheme to forward to the correct authentication scheme
    .AddPolicyScheme("smart", null, options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader?.StartsWith("Bearer ") == true)
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }
            return IdentityConstants.ApplicationScheme;
        };
    })
    .AddCookie(IdentityConstants.ApplicationScheme, o =>
    {
        o.LoginPath = new PathString("/Account/Login");
        o.EventsType = typeof(CustomCookieAuthenticationEvents);
    })
    .AddCookie(IdentityConstants.ExternalScheme, o =>
    {
        o.Cookie.Name = IdentityConstants.ExternalScheme;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
    {
        o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
        o.Events = new CookieAuthenticationEvents
        {
            OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
        };
    })
    .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
    {
        o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
        o.ExpireTimeSpan = TimeSpan.FromMinutes(5);
    })
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.AccessTokenSecret))
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                context.HttpContext.Items["Properties"] = context.Properties;
                context.HttpContext.Features.Set(context.Properties);

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    })
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();