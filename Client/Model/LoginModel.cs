namespace BravusApp.Client.Model
{
    public class LoginModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Cpf { get; set; } = "";
        
        [System.ComponentModel.DataAnnotations.Required]
        public string Password { get; set; } = "";

        public LoginModel() { }
    }
}
