using System.Threading.Tasks;

namespace D3DLab.Plugin
{
    public interface IPlugin {
        string Name { get; }
        string Description { get; }

        Task ExecuteAsync(IPluginContext context);
        Task CloseAsync();
    }
}