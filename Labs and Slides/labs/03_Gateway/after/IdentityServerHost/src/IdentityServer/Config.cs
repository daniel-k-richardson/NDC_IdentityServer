using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Security.Claims;

namespace Host
{
    public class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources = new List<IdentityResource>
        {
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
            new IdentityResources.Email()
        };

        public static IEnumerable<Client> Clients = new List<Client>
        {
            new Client
            {
                ClientId = "movie_client",
                ClientName = "Moive Review App",
                AllowedGrantTypes = GrantTypes.Implicit,

                RedirectUris =
                {
                    "http://localhost:32361/signin-oidc"
                },

                PostLogoutRedirectUris =
                {
                    "http://localhost:32361/signout-callback-oidc"
                },

                AllowedScopes =
                {
                    IdentityServerConstants.StandardScopes.OpenId,
                    IdentityServerConstants.StandardScopes.Profile,
                    IdentityServerConstants.StandardScopes.Email
                }
            }
        };

        public static List<TestUser> Users = new List<TestUser>()
        {
            new TestUser {SubjectId="user1", Username="user1", Password = "user1",
                Claims =
                {
                    new Claim("name", "User One"),
                    new Claim("email", "User1@gmail.com"),
                }
            },
            new TestUser {SubjectId="user2", Username="user2", Password = "user2",
                Claims =
                {
                    new Claim("name", "User Two"),
                    new Claim("email", "User2@yahoo.com"),
                }
            },
            new TestUser {SubjectId="user3", Username="user3", Password = "user3",
                Claims =
                {
                    new Claim("name", "User Three"),
                    new Claim("email", "User3@outlook.com"),
                }
            },
            new TestUser {SubjectId="user4", Username="user4", Password = "user4",
                Claims =
                {
                    new Claim("name", "User Four"),
                    new Claim("email", "User4@gmail.com"),
                }
            },
            new TestUser {SubjectId="user5", Username="user5", Password = "user5",
                Claims =
                {
                    new Claim("name", "User Five"),
                    new Claim("email", "User5@admin.com"),
                }
            },
        };
    }
}
