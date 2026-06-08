using ChurchApi.Models;
using ChurchApi.Services;
using ChurchApi.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // options.UseSqlite("Data Source=church.db");
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IDonationService, DonationService>();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// app.MapGet("/", () =>
// {
//     return "Church API is running!";
// });

// app.MapGet("/member/{id}", (IMemberService memberService, int id) =>
// {
//     var member = memberService.GetMember(id);
//     return member is null
//         ? Results.NotFound()
//         : Results.Ok(member);

// });

// app.MapGet("/members", (IMemberService memberService) =>
// {
//     return memberService.GetMembers();
// });

// app.MapPost("/member", (IMemberService memberService, Member member) =>
// {
//     memberService.AddMember(member);
//     return Results.Created($"/member/{member.Name}", member);
// });

// app.MapPut("/member", (IMemberService memberService, Member member) =>
// {
//     var result = memberService.UpdateMember(member);
//     return result is null
//         ? Results.NotFound()
//         : Results.Ok(result);
// });

// app.MapDelete("/member/{id}", (IMemberService memberService, int id) =>
// {
//     var result = memberService.DeleteMember(id);
//     return result is null
//         ? Results.NotFound()
//         : Results.Ok(result);
// });

app.Run();
