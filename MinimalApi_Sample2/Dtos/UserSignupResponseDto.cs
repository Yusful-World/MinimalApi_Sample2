namespace MinimalApi_Sample2.Dtos
{
    public class UserSignupResponseDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }

        public string Email { get; set; }

        public string PhoneNumber { get; set; }
        public string Token { get; set; }
    }
}
