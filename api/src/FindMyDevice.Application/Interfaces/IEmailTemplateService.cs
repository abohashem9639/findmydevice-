namespace FindMyDevice.Application.Interfaces
{
    public interface IEmailTemplateService
    {
        Task<string> RenderAsync(string templateName, object model, string? language = null, CancellationToken cancellationToken = default);
    }
}
