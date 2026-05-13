using FindMyDevice.Application.DTOs;

namespace FindMyDevice.Application.Interfaces
{
    public interface IApplicationVersionProvider
    {
        ApplicationVersionDto GetCurrent();
    }
}
