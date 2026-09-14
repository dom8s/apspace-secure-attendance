using FYP_Dashboard.Services;

var builder = WebApplication.CreateBuilder(args);

//  MVC 
builder.Services.AddControllersWithViews();

//  Session (stores JWT token after login) 
builder.Services.AddSession(options =>
{
    options.IdleTimeout        = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly    = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddHttpContextAccessor();

//  HttpClient for API calls 
builder.Services.AddHttpClient<IApiService, ApiService>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["ApiSettings:BaseUrl"]
        ?? "https://localhost:7223/");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Auth/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthorization();

// Default route → Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

app.Run();
