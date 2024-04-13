using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using StartupNNTM.Models;
using StartupNNTM.Service;
using StartupNNTM.ViewModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
var connectionString = builder.Environment.IsDevelopment()
                                 ? builder.Configuration.GetConnectionString("DataConnect")
                                 : builder.Configuration.GetConnectionString("DataConnect");
builder.Services.AddDbContext<NntmContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

#region JWT Authentication Configuration
    string issuer = builder.Configuration.GetValue<string>("Tokens:Issuer");
    string signingKey = builder.Configuration.GetValue<string>("Tokens:Key");
    byte[] signingKeyBytes = System.Text.Encoding.UTF8.GetBytes(signingKey);

    builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = issuer,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ClockSkew = System.TimeSpan.Zero,
            IssuerSigningKey = new SymmetricSecurityKey(signingKeyBytes)
        };
    })
    .AddCookie(options =>
    {
        options.LoginPath = "/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Account/AccessDenied"; // Đường dẫn đến trang truy cập bị từ chối
        options.SlidingExpiration = true; // Thời gian hết hạn tự động được gia hạn khi người dùng thực hiện các hoạt động xác thực
    });
#endregion

#region Service

    builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<NntmContext>().AddDefaultTokenProviders();
  // builder.Services.AddScoped<IHttpContextAccessor, HttpContextAccessor>();
    builder.Services.AddScoped<IAccountService, AccountService>();
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddDistributedMemoryCache();
    builder.Services.AddOptions();
    builder.Services.AddSession();
    var mailsettings = builder.Configuration.GetSection("MailSettings");
    builder.Services.Configure<MailSettings>(mailsettings);
    builder.Services.AddSingleton<ISendMailService, SendMailService>();

#endregion

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy( builder =>
    {
        builder.WithOrigins("http://localhost:4200", "https://luongxuannhat.github.io", "https://localhost:4200")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.

    app.UseSwagger();
    app.UseSwaggerUI();


app.UseStaticFiles();
app.UseForwardedHeaders();
app.UseSession();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
