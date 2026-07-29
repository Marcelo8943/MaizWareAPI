using System.Text.Json.Serialization;
using MaizWareAPI.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

const string CorsPolicyName = "OpenCorsPolicy";

builder.Services.AddDbContext<MaizWareContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("MaizWareConnection")));

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy(CorsPolicyName, policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOpenApi();

var app = builder.Build();
app.MapGet("/", () => Results.Content("""
<!doctype html>
<html lang="es">
<head>
  <meta charset="utf-8" />
  <meta name="viewport" content="width=device-width, initial-scale=1" />
  <title>MaizWareAPI Endpoints</title>
  <style>
    :root { --navy:#03045E; --title:#0077B6; --border:#00B4D8; --bg:#CAF0F8; }
    body { margin:0; font-family: Segoe UI, Arial, sans-serif; background:#f7fbfd; color:var(--navy); }
    main { max-width: 980px; margin: 0 auto; padding: 40px 22px; }
    h1 { color: var(--title); margin: 0 0 8px; }
    p { color:#496AA7; }
    .grid { display:grid; grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); gap:16px; margin-top:24px; }
    section { background:white; border:1px solid rgba(0,180,216,.35); border-radius:24px; padding:20px; box-shadow:0 12px 28px rgba(3,4,94,.08); }
    h2 { margin:0 0 14px; font-size:18px; color:var(--title); }
    a { color:var(--title); font-weight:700; text-decoration:none; }
    code { display:block; padding:8px 0; color:var(--navy); }
    .pill { display:inline-block; border-radius:999px; background:var(--bg); padding:8px 12px; margin-top:8px; }
  </style>
</head>
<body>
  <main>
    <h1>MaizWareAPI</h1>
    <p>API conectada a SQL Server MaizWare. Usa estos endpoints desde Expo o el navegador.</p>
    <a class="pill" href="/openapi/v1.json">Ver OpenAPI JSON</a>
    <div class="grid">
      <section>
        <h2>Autenticacion</h2>
        <code>POST /api/auth/login</code>
        <code>POST /api/auth/register</code>
      </section>
      <section>
        <h2>Usuarios</h2>
        <code>GET /api/users</code>
        <code>GET /api/users/{id}</code>
        <code>POST /api/users</code>
        <code>PUT /api/users/{id}</code>
        <code>DELETE /api/users/{id}</code>
        <code>GET /api/users/roles</code>
        <code>POST /api/users/{id}/roles</code>
      </section>
      <section>
        <h2>Estados de animo</h2>
        <code>GET /api/moodentries/emotions</code>
        <code>GET /api/moodentries/user/{userId}</code>
        <code>GET /api/moodentries/{id}</code>
        <code>POST /api/moodentries</code>
        <code>PUT /api/moodentries/{id}</code>
        <code>DELETE /api/moodentries/{id}</code>
      </section>
      <section>
        <h2>Chat IA</h2>
        <code>GET /api/ai-chat/user/{userId}/conversations</code>
        <code>GET /api/ai-chat/conversations/{conversationId}</code>
        <code>POST /api/ai-chat/conversations</code>
        <code>POST /api/ai-chat/conversations/{conversationId}/messages</code>
        <code>PUT /api/ai-chat/conversations/{conversationId}/close</code>
        <code>DELETE /api/ai-chat/conversations/{conversationId}</code>
      </section>
    </div>
  </main>
</body>
</html>
""", "text/html"));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors(CorsPolicyName);
app.UseAuthorization();
app.MapControllers();
app.Run();



