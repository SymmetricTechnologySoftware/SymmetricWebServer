using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserShared;
using Nancy.ModelBinding;

namespace WebServer.Modules.Users
{
    public class UserRestModule : BaseModule
    {
        public UserRestModule()
        {
            Get["/getbasicusers"] = parameters =>
            {
                List<User> users;
                string message = Globals.UserDB.LoadUsers(out users);
                if (String.IsNullOrWhiteSpace(message))
                {
                    List<RestUser> lst = new List<RestUser>();
                    foreach (User usr in users)
                    {
                        lst.Add(RestUser.UserToRest(usr, false));
                    }
                    return Response.AsJson(lst);
                }

                Response error = new Response();
                error.StatusCode = HttpStatusCode.NotImplemented;
                error.ReasonPhrase = message;
                return error;
            };

            Get["/getusers"] = parameters =>
            {
                Response error;
                if (!Program.Handle.EnableGetUsers)
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.NotAcceptable;
                    return error;
                }

                List<User> users;
                string message = Globals.UserDB.LoadUsers(out users);
                if (String.IsNullOrWhiteSpace(message))
                {
                    List<RestUser> lst = new List<RestUser>();
                    foreach (User usr in users)
                    {
                        lst.Add(RestUser.UserToRest(usr));
                    }
                    return Response.AsJson(lst); 
                }
                error = new Response();
                error.StatusCode = HttpStatusCode.NotImplemented;
                error.ReasonPhrase = message;
                return error;
            };

            Post["/remotelogin"] = parameters =>
            {
                Response error;

                int id = 0;
                if (!int.TryParse(this.Request.Form.id, out id))
                {
                    return HttpStatusCode.BadRequest;
                }

                string password = this.Request.Form.password;
                if (this.LoginUser(id, password))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.OK);
                }
                else
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.NotFound;
                    error.ReasonPhrase = "Username and password do not match";
                    return error;
                }
            };

            Post["/remotelogout"] = parameters =>
            {
                this.Logout();
                return HttpStatusCode.OK;
            };

            Post["/deleteuser"] = parameters =>
            {
                RestUser restUser = this.Bind<RestUser>();
                Response error;
                if (!this.IsValidUser())
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.Unauthorized;
                    error.ReasonPhrase = "Please log in to continue.";
                    return error;
                }

                if (this.SecurityLevel < restUser.SecurityLevel)
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.Unauthorized;
                    error.ReasonPhrase = "You are not authorised to do this.";
                    return error;
                }

                string message;
                if (Globals.UserDB.DeleteUser(restUser.UserID, out message))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.OK);
                }
                else
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.ExpectationFailed;
                    error.ReasonPhrase = message;
                    return error;
                }
            };

            Post["/addedituser"] = parameters =>
            {
                RestUser restUser = this.Bind<RestUser>();
                Response error;
                string errormessage;
                if(!CheckModifyUser(restUser.UserID, restUser.SecurityLevel, out errormessage))
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.Unauthorized;
                    error.ReasonPhrase = errormessage;
                    return error;
                }

                errormessage = Globals.UserDB.AddEditUser(RestUser.RestToUser(restUser), false, true, false);

                if (String.IsNullOrWhiteSpace(errormessage))
                {
                    return Negotiate.WithStatusCode(HttpStatusCode.OK);
                }
                else
                {
                    error = new Response();
                    error.StatusCode = HttpStatusCode.ExpectationFailed;
                    error.ReasonPhrase = errormessage;
                    return error;
                }
            };


            Get["/readuserlastupdated"] = parameters =>
            {
                Response error;
                string message;
                DateTime dtTime = Globals.UserDB.ReadUserLastUpdated(out message);
                if (String.IsNullOrWhiteSpace(message))
                {
                    return Response.AsJson(new RestUserLastUpdated() { Date = dtTime });
                }
                error = new Response();
                error.StatusCode = HttpStatusCode.NotImplemented;
                error.ReasonPhrase = message;
                return error;
            };
        }
    }
}
