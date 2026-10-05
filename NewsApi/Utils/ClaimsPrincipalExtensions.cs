using System.Security.Claims;

namespace NewsApi.Utils
{
    // Удобные методы, чтобы в контроллерах писать User.GetUserId() вместо разбора claims
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            return int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        public static string GetUserRole(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Role)!;
        }
    }
}