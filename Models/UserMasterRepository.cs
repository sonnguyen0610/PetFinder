using PetFinderAPI.App_Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PetFinderAPI.Models
{
    public class UserMasterRepository : IDisposable
    {
        PetContext context = new PetContext();
        //This method is used to check and validate the user credentials
        public User ValidateUser(string username, string password)
        {
            return context.Users.FirstOrDefault(user =>
            user.UserName.Equals(username, StringComparison.OrdinalIgnoreCase)
            && user.Password == password);
        }
        public void Dispose()
        {
            context.Dispose();
        }
    }
}