using BaithuchanhORM.Data;
using Microsoft.EntityFrameworkCore;
using BaithuchanhORM.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ImageFileService>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// Apply pending migrations automatically on app startup and seed sample data
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    
    // Add image to existing Truyện Kiều book if it doesn't have one
    var kieuBook = await db.Books.Include(b => b.Images).FirstOrDefaultAsync(b => b.Title == "Truyện Kiều");
    if (kieuBook != null && !kieuBook.Images.Any())
    {
        var sampleImage = new BaithuchanhORM.Models.BookImage
        {
            BookId = kieuBook.Id,
            FileName = "truyen-kieu.jpg",
            FilePath = "https://coresg-normal.trae.ai/api/ide/v1/text_to_image?prompt=Vietnamese%20classic%20literature%20book%20cover%20Truyen%20Kieu&image_size=square"
        };
        db.BookImages.Add(sampleImage);
        await db.SaveChangesAsync();
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
