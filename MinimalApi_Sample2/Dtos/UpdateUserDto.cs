﻿namespace MinimalApi_Sample2.Dtos
{
    public class UpdateUserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string PhoneNumber { get; set; }

        public string? Role { get; set; }

    }
}
