﻿namespace MinimalApi_Sample2.Dtos
{
    public class UserSignUpRequestDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string? PhoneNumber { get; set; } 
        public string FirstName { get; set; } 
        public string LastName { get; set; }
    }
}
