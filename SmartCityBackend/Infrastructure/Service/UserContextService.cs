

using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SmartCityBackend.Infrastructure.Persistence;
using SmartCityBackend.Models;

public interface IUserContextService
{
    UserContext GetUserDetails();
}


public class UserContextService: IUserContextService
{
    
    private readonly HttpContext _context;
    private readonly DatabaseContext _databaseContext;
 
    public UserContextService(IHttpContextAccessor httpContextAccessor, DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
        _context = httpContextAccessor.HttpContext;
    }
 
    public UserContext GetUserDetails()
    {
        var userContext = new UserContext();
        string? id = _context.Request.HttpContext.User.FindFirst("Id")?.Value;
        string? email = _context.Request.HttpContext.User.FindFirst("Email")?.Value;
        string? role = _context.Request.HttpContext.User.FindFirst("Role")?.Value;
        string? preferredUsername = _context.Request.HttpContext.User.FindFirst("PreferredUsername")?.Value;
        string? givenName = _context.Request.HttpContext.User.FindFirst("GivenName")?.Value;
        string? FamilyName = _context.Request.HttpContext.User.FindFirst("FamilyName")?.Value;
        
        userContext.Id = int.Parse(id ?? string.Empty);
        userContext.Email = email;
        Task<Role?> existing = _databaseContext.Roles.SingleOrDefaultAsync(x => x.Name == role);
        userContext.Role = existing.Result!;
        userContext.PreferredUsername = preferredUsername;
        userContext.GivenName = givenName;
        userContext.FamilyName = FamilyName;
        
        return userContext;
    }
}