using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Sustainsys.Saml2;
using Sustainsys.Saml2.Metadata;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(o =>
{
    o.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    o.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    //o.DefaultChallengeScheme = "Saml2";
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, o =>
{
    o.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.FromMinutes(1440),
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
    };
})
.AddSaml2(options =>
{
    options.SPOptions.EntityId = new EntityId("https://localhost:44378");
    options.IdentityProviders.Add(
      new IdentityProvider(
        new EntityId("https://sts.windows.net/a35017a7-9bde-499f-97a9-bc27534e628e/"), options.SPOptions)
      {
          MetadataLocation = "https://login.microsoftonline.com/a35017a7-9bde-499f-97a9-bc27534e628e/federationmetadata/2007-06/federationmetadata.xml?appid=855198b4-3d16-4732-bab0-f6286204211e"
      });
})
.AddCookie(o =>
{
    o.Cookie.SameSite = SameSiteMode.None;
});

builder.Services.AddCors(x =>
{
    x.AddPolicy("AllowAll", builder =>
    {
        builder.AllowCredentials()
               .WithOrigins("https://localhost:4200")
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

//builder.WebHost.ConfigureKestrel(opt =>
//{
//    opt.ListenAnyIP(44378, listenOpt =>
//    {
//        listenOpt.UseHttps(
//            "c:\\webapp.pfx",
//            "mypassword"
//        );
//    });
//    opt.ListenAnyIP(55462);
//});

builder.Services.AddSession();

//builder.Services.AddSession(options =>
//{
//    options.Cookie.IsEssential = true;
//    options.Cookie.SameSite = SameSiteMode.None;
//});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseSession();

app.MapRazorPages();
app.MapControllers();

app.Run();
