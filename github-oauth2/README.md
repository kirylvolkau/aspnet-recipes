# OAuth2 with GitHub
OAuth 2 with GitHub was implemented in [amount] steps:
1. Register an OAuth app in your GitHub profile.
2. Set `GitHub:ClientID` and `GitHub:ClientSecret` using `dotnet user-secret set [secret]`.
3. Configure authentication
```c#
services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = "GitHub";
                })
                .AddCookie()
                .AddOAuth("GitHub", options =>
                    {
                        options.ClientId = Configuration["GitHub:ClientId"];
                        options.ClientSecret = Configuration["GitHub:ClientSecret"];
                        options.CallbackPath = new PathString("/github-oauth");
                        options.AuthorizationEndpoint = "https://github.com/login/oauth/authorize";
                        options.TokenEndpoint = "https://github.com/login/oauth/access_token";
                        options.UserInformationEndpoint = "https://api.github.com/user";
                        options.SaveTokens = true;
                        options.ClaimActions.MapJsonKey(ClaimTypes.NameIdentifier, "id");
                        options.ClaimActions.MapJsonKey(ClaimTypes.Name, "name");
                        options.ClaimActions.MapJsonKey("urn:github:login", "login");
                        options.ClaimActions.MapJsonKey("urn:github:url", "html_url");
                        options.ClaimActions.MapJsonKey("urn:github:avatar", "avatar_url");
                        options.Events = new OAuthEvents
                        {
                            OnCreatingTicket = async context =>
                            {
                                var request = new HttpRequestMessage(HttpMethod.Get,
                                    context.Options.UserInformationEndpoint);
                                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                                request.Headers.Authorization =
                                    new AuthenticationHeaderValue("Bearer", context.AccessToken);
                                var response = await context.Backchannel.SendAsync(request,
                                    HttpCompletionOption.ResponseHeadersRead, context.HttpContext.RequestAborted);
                                response.EnsureSuccessStatusCode();
                                var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                                context.RunClaimActions(json.RootElement);
                            }
                        };
                    }
                );
```
4. Add authentication middleware.
5. Use `Octokit` NuGet package to get data from your github account:
```c#
 public async Task OnGetAsync()
        {
            if (User.Identity.IsAuthenticated)
            {
                string accessToken = await HttpContext.GetTokenAsync("access_token");
                var github = new GitHubClient(new ProductHeaderValue("AspNetCoreGitHubAuth"), new InMemoryCredentialStore(new Credentials(accessToken)));
                Repositories = await github.Repository.GetAllForCurrent();
                StarredRepos = await github.Activity.Starring.GetAllForCurrent();
                Followers = await github.User.Followers.GetAllForCurrent();
                Following = await github.User.Followers.GetAllFollowingForCurrent();
            }
        }
```
This repository followed original article: https://www.red-gate.com/simple-talk/dotnet/net-development/oauth-2-0-with-github-in-asp-net-core/?utm_source=simpletalkdotnet&utm_medium=pubemail&utm_content=07142020-slota3&utm_term=simpletalkmain
<br/>

**Additional reading:** <br/>
* https://digitalmccullough.com/posts/aspnetcore-auth-system-demystified.html 
* https://andrewlock.net/introduction-to-authentication-with-asp-net-core/
