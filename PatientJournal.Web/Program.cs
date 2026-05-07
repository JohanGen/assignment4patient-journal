using Microsoft.AspNetCore.Authentication.Cookies;
using PatientJournal.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
    });

var apiBase = builder.Configuration["ApiBaseUrl"] ?? "http://localhost:5200";
builder.Services.AddHttpClient<ApiService>(client =>
    client.BaseAddress = new Uri(apiBase));

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Cases}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();
