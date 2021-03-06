using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Moneyes.Server;
using Moneyes.Server.Data;
using Moneyes.Server.Services;
using System.Collections;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();

foreach (DictionaryEntry e in Environment.GetEnvironmentVariables())
{
    Console.WriteLine(e.Key + ": " + e.Value);
}

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
builder.Services.AddScoped<CustomJwtBearerEvents>();

builder.Services.AddScoped<ITokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAccessTokenService, JwtAccessTokenService>();
builder.Services.AddScoped<IRefreshTokenService, JwtRefreshTokenService>();
builder.Services.AddScoped<IRefreshTokenValidator, JwtRefreshTokenValidator>();
builder.Services.AddScoped<IAccessTokenValidator, JwtAccessTokenValidator>();
builder.Services.AddScoped<ITokenAuthenticateService, JwtTokenAuthenticateService>();
builder.Services.AddScoped<IActiveRefreshTokenService, ActiveRefreshTokenService>();

#endregion

// Email
if (!builder.Environment.IsDevelopment())
{
    builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("Mail"));
    builder.Services.AddTransient<IEmailSender, EmailSender>();
}

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

        options.EventsType = typeof(CustomJwtBearerEvents);
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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    var context = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();
    context.Database.Migrate();
}

app.Run();