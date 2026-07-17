using Bfs.Iop.Test.Abstraction.Constants;
using Bfs.Iop.Test.Abstraction.Helpers;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Test.Abstraction.Shared;

/// <summary>
/// Handles EIAM (Enterprise Identity and Access Management) authentication operations in Playwright tests.
/// Provides functionality for login, profile selection, and logout processes.
/// </summary>
/// <remarks>
/// This class manages three main authentication operations:
/// 1. Initial login with username/password/TAN (TryConnectToEiam)
/// 2. Profile selection based on organization and role (ChooseProfile)
/// 3. Logout functionality (Logout)
/// 
/// Each method includes:
/// - Element wait handling
/// - Error handling with detailed messages
/// - Automatic scrolling and focus management
/// - Timeout configurations
/// 
/// The authentication flow follows the EIAM protocol with:
/// - Federation login
/// - Multi-factor authentication
/// - Role-based profile selection
/// </remarks>
/// <example>
/// var eiamLogin = new EiamLogin();
/// await eiamLogin.TryConnectToEiam(page, username, password, tan);
/// await eiamLogin.ChooseProfile(page, "Organization", "Role");
/// await eiamLogin.Logout(page);
/// </example>
public sealed class EiamLogin
{
    public static async Task<bool> ChooseProfile(IPage page, string organisation, string role)
    {
        try
        {
            await Task.Delay(2000);

            // try to find the buttons
            var buttons = await page.GetByRole(AriaRole.Button).AllAsync();

            foreach (var button in buttons)
            {
                var spanLocator = button.Locator("span");
                var spanCount = await spanLocator.CountAsync();

                if (spanCount < 2)
                {
                    continue;
                }

                var organisationText = await spanLocator.Nth(0).InnerTextAsync();
                
                if (organisationText == organisation)
                {
                    var roleText = await spanLocator.Nth(1).InnerTextAsync();

                    if (roleText.Contains(role))
                    {
                        await button.PressAsync(Keys.Enter);
                        await Task.Delay(500);
                        return true;
                    }
                }
            }

            Assert.Fail("No EIAM profiles found.");
            return false;
        }
        catch (PlaywrightException ex)
        {
            Assert.Fail($"Error in ChooseProfile: {ex.Message}");
            return false;
        }
    }

    public static async Task<bool> FillLoginMask(IPage page, string username, string password, string tan)
    {
        try
        {
            await Task.Delay(1000);
            var inputEmail = await page.WaitForSelectorAsync(CssSelectorWrapper.Wrap(AttributesAndElements.Id, Eiam.InputEmailId));

            if (inputEmail != null)
            {
                await inputEmail.FillAsync(username);
                await inputEmail.PressAsync("Enter");
            }
            else
            {
                Assert.Fail("input email not found!");
            }

            await Task.Delay(1000);

            var inputPassword = await page.WaitForSelectorAsync(CssSelectorWrapper.Wrap(AttributesAndElements.Id, Eiam.InputPasswordId));
            if (inputPassword != null)
            {
                await inputPassword.FillAsync(password);
                await inputPassword.PressAsync("Enter");
            }
            else
            {
                Assert.Fail("input password not found!");
            }

            await Task.Delay(1000);

            var inputTan = await page.WaitForSelectorAsync(CssSelectorWrapper.Wrap(AttributesAndElements.Id, Eiam.InputTanId));
            if (inputTan != null)
            {
                await inputTan.FillAsync(tan);
                await inputTan.PressAsync("Enter");
            }
            else
            {
                Assert.Fail("input tan not found!");
            }

            return true;
        }
        catch (PlaywrightException ex)
        {
            Assert.Fail($"Error in TryConnectToEiam: {ex.Message}");
        }
        return false;
    }

    private async Task SkipNotificationsPage(IPage page)
    {
        // This waiting time is necessary to stabilize the page
        await Task.Delay(2500);

        // Click on the last button
        var buttons = await page.GetByRole(AriaRole.Button).AllAsync();

        var button = buttons[buttons.Count - 1];

        await button.ClickAsync();
        await Task.Delay(500);
    }

    public async Task<bool> TryConnectToEiam(IPage page, string username, string password, string tan)
    {
        try
        {
            // Temporary code: Close the Notifications page
            //await SkipNotificationsPage(page);

            //var noticeContinue = page.GetByRole(AriaRole.Button, new() { NameRegex = new Regex("continue", RegexOptions.IgnoreCase) }); ;

            //if (noticeContinue != null)
            //{
                //await noticeContinue.ClickAsync();
                var chLogin = page.Locator(Eiam.CHLoginId);

                if (chLogin != null)
                {
                    await chLogin.ClickAsync();

                    // skip AGOV dialog:
                    await SkipNotificationsPage(page);

                    return await FillLoginMask(page, username, password, tan);
                }
                else
                {
                    Assert.Fail("Login button not found!");
                }
            //}
            //else
            //{
            //    Assert.Fail("Continue button not found!");
            //}

        }
        catch (PlaywrightException ex)
        {
            Assert.Fail($"Error in TryConnectToEiam: {ex.Message}");
        }
        return false;
    }

    public async Task Logout(IPage page)
    {
        try
        {
            TestContext.Out.WriteLine("Logout");
            var logout = await page.QuerySelectorAsync(CssSelectorWrapper.Wrap(AttributesAndElements.Id, Eiam.LogoutButtonId));
            if (logout != null) await logout.ClickAsync();
        }
        catch (PlaywrightException ex)
        {
            Assert.Fail($"Error in Logout: {ex.Message}");
        }
    }
}
