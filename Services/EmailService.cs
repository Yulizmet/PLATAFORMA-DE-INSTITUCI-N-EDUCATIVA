using SchoolManager.Models;

namespace SchoolManager.Services
{
    public class EmailService
    {
        private readonly IEmailSender _mailService;

        public EmailService(IEmailSender mailService)
        {
            _mailService = mailService;
        }

        public async Task SendProcedureNotification(procedure_request request, string customMessage, string statusColor, string statusTextColor)
        {
            string template = await File.ReadAllTextAsync("wwwroot/templates/Notification.html");

            string callToAction = request.ProcedureFlow.ProcedureStatus.IsTerminalState
                ? "Tu trámite ha finalizado. Puedes acudir a ventanilla por tu documentación original si aplica."
                : "Por favor, mantente al tanto de los siguientes pasos en tu portal de alumno.";

            string emailBody = template
                .Replace("{UserName}", request.Preenrollments.FirstOrDefault()?.User?.Person?.FirstName ?? "Estudiante")
                .Replace("{ProcedureName}", request.ProcedureType.Name)
                .Replace("{Folio}", request.Folio)
                .Replace("{StatusName}", request.ProcedureFlow.ProcedureStatus.Name)
                .Replace("{AdminMessage}", customMessage)
                .Replace("{StatusColor}", statusColor)
                .Replace("{StatusTextColor}", statusTextColor)
                .Replace("{CallToActionMessage}", callToAction)
                .Replace("{LoginUrl}", "https://tusitio.com/login");

            await _mailService.SendEmailAsync(request.User.Email, "Actualización de Trámite: " + request.Folio, emailBody);
        }
    }
}
