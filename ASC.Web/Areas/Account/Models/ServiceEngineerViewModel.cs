using Microsoft.AspNetCore.Identity;

namespace ASC.Web.Areas.Account.Models
{
    public class ServiceEngineerViewModel
    {
        public List<IdentityUser> ServiceEngineers { get; set; }

        public ServiceEngineerRegistrationViewModel Registration { get; set; }
    }
}