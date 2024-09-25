using Google.Authenticator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NetCore.Helpers;
using NetCore.Models;
using NetCore.Models.Response;
using NetCore.Services;
using NetCore.ViewModels;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace NetCore.Controllers
{
    [ApiController]
    [Route("Api/[controller]")]
    public class ProfilController : ControllerBase
    {
        private readonly ILogger<ProfilController> _logger;
        private IProfilService _profilService;
        protected readonly AppSettings _appSettings;

        public ProfilController(IProfilService profilService, IOptions<AppSettings> appSettings, ILogger<ProfilController> logger)
        {
            _logger = logger;
            _profilService = profilService;
            _appSettings = appSettings.Value;
        }

        [HttpPost("SignIn")]
        public IActionResult SignIn([FromBody] LoginModel model)
        {
            var profil = _profilService.Authenticate(model.UserName, model.Password);
            if (profil == null)
            {
                return Ok(ResponseViewModel.CreateError("Tên đăng nhập hoặc mật khẩu không chính xác"));
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var expires = DateTime.UtcNow.AddDays(30);
            var key = Encoding.ASCII.GetBytes(_appSettings.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, profil.Id.ToString())
                }),
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            LoginViewModel res = new LoginViewModel();
            res.Token = tokenString;
            res.UserId = profil.Id;
            res.FullName = profil.FullName;
            res.Expires = expires;

            return Ok(ResponseViewModel.CreateSuccess(res));
        }
        [Authorize]
        [HttpGet("{id}")]
        public IActionResult GetDataById(int id)
        {
            var profil = _profilService.GetById(id);

            return Ok(ResponseViewModel.CreateSuccess(profil, ""));
        }
        [HttpGet("CreateOAuth")]
        public IActionResult CreateOAuth(int id)
        {
            var profil = _profilService.GetById(id);

            //string key = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);

            TwoFactorAuthenticator tfa = new TwoFactorAuthenticator();
            SetupCode setupInfo = tfa.GenerateSetupCode("Test Two Factor", profil.Email, profil.OAuthKey, false, 3);

            string qrCodeImageUrl = setupInfo.QrCodeSetupImageUrl;
            string manualEntrySetupCode = setupInfo.ManualEntryKey;

            OAuthViewModel oAuth = new OAuthViewModel();

            oAuth.ImageUrl = qrCodeImageUrl;
            oAuth.SetupCode = manualEntrySetupCode;
            oAuth.Key = profil.OAuthKey;

            // verify
            //TwoFactorAuthenticator tfa1 = new TwoFactorAuthenticator();
            //bool result = tfa.ValidateTwoFactorPIN(key, txtCode.Text);

            return Ok(ResponseViewModel.CreateSuccess(oAuth, ""));
        }
        [HttpPost("VerifyOAuth")]
        public IActionResult VerifyOAuth([FromBody] OAuthModel model)
        {
            var profil = _profilService.GetById(model.UserId);
            // verify
            TwoFactorAuthenticator tfa1 = new TwoFactorAuthenticator();
            bool result = tfa1.ValidateTwoFactorPIN(model.Key, model.Code);

            return Ok(ResponseViewModel.CreateSuccess(result, ""));
        }
    }
}
