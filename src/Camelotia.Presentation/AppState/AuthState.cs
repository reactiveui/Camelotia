namespace Camelotia.Presentation.AppState
{
    public class AuthState
    {
        public DirectAuthState DirectAuthState { get; set; } = new DirectAuthState();
        public HostAuthState HostAuthState { get; set; } = new HostAuthState();
        public OAuthState OAuthState { get; set; } = new OAuthState();
    }
}