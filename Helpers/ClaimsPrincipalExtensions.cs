using System.Security.Claims;

namespace SchoolManager.Helpers
{
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Devuelve el UserId del usuario autenticado (claim "UserId").
        /// Lanza InvalidOperationException si no está autenticado o el claim no existe.
        /// </summary>
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("UserId")
                ?? throw new InvalidOperationException("El claim 'UserId' no está presente. ¿El usuario está autenticado?");

            return int.Parse(value);
        }

        /// <summary>
        /// Intenta obtener el UserId. Devuelve null si no está disponible.
        /// </summary>
        public static int? TryGetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue("UserId");
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}