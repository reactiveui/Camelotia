namespace Camelotia.Presentation.Interfaces
{
    public interface IHostAuthViewModel : IDirectAuthViewModel
    {
        string Address { get; set; }
        
        string Port { get; set; }
    }
}