using Microsoft.AspNetCore.Authentication.Cookies;
using Sustainsys.Saml2.Metadata;
using Sustainsys.Saml2;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(sharedOptions =>
{
    sharedOptions.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    sharedOptions.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    sharedOptions.DefaultChallengeScheme = "Saml2";
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
.AddCookie();

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

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.Run();
