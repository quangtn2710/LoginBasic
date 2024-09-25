using NetCore.Helpers;
using NetCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetCore.Services
{
    public interface IProfilService
    {
        ProfilViewModel Authenticate(string username, string password);
        ProfilViewModel GetById(int id);
    }

    public class ProfilService: IProfilService
    {
        protected readonly Microsoft.AspNetCore.Hosting.IHostingEnvironment _hostingEnvironment;

        public ProfilService(Microsoft.AspNetCore.Hosting.IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }
        public ProfilViewModel GetById(int id)
        {
            //
            return GetProfilById(id);
        }
        public ProfilViewModel GetProfilById(int id)
        {
            var rootPath = _hostingEnvironment.ContentRootPath; //get the root path

            var fullPath = Path.Combine(rootPath, "Database/Profil.json"); //combine the root path with that of our json file inside mydata directory

            var jsonData = System.IO.File.ReadAllText(fullPath); //read all the content inside the file

            if (string.IsNullOrWhiteSpace(jsonData)) return null; //if no data is present then return null or error if you wish

            var profils = JsonConvert.DeserializeObject<List<ProfilViewModel>>(jsonData); //deserialize object as a list of users in accordance with your json file

            if (profils == null || profils.Count == 0) return null; //if there's no data inside our list then return null or error if you wish

            var profil = profils.FirstOrDefault(x => x.Id == id); //filter the list to match with the first name that is being passed in

            return profil;

        }
        public ProfilViewModel Authenticate(string username, string password)
        {
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                return null;

            var profil = GetProfil().SingleOrDefault(s => s.UserName == username);

            // check if username exists
            if (profil == null)
                return null;

            // check if password is correct
            if (!VerifyPasswordHash(password, profil.PasswordHash, profil.PasswordSalt))
                return null;
            return profil;
        }

        public List<ProfilViewModel> GetProfil()
        {
            var rootPath = _hostingEnvironment.ContentRootPath; //get the root path

            var fullPath = Path.Combine(rootPath, "Database/Profil.json"); //combine the root path with that of our json file inside mydata directory

            var jsonData = System.IO.File.ReadAllText(fullPath); //read all the content inside the file

            if (string.IsNullOrWhiteSpace(jsonData)) return null; //if no data is present then return null or error if you wish

            var profils = JsonConvert.DeserializeObject<List<ProfilViewModel>>(jsonData); //deserialize object as a list of users in accordance with your json file

            if (profils == null || profils.Count == 0) return null; //if there's no data inside our list then return null or error if you wish

            return profils;
        }
        private static bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");
            if (storedHash.Length != 64) throw new ArgumentException("Invalid length of password hash (64 bytes expected).", "passwordHash");
            if (storedSalt.Length != 128) throw new ArgumentException("Invalid length of password salt (128 bytes expected).", "passwordHash");

            using (var hmac = new System.Security.Cryptography.HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }

            return true;
        }
        private static void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            if (password == null) throw new ArgumentNullException("password");
            if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Value cannot be empty or whitespace only string.", "password");

            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}