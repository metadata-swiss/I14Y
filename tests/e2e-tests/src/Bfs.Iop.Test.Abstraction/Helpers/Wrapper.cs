using Bfs.Iop.Test.Abstraction.Constants;
using Microsoft.Playwright;
using System.Text;
using System.Text.RegularExpressions;

namespace Bfs.Iop.Test.Abstraction.Helpers;

/// <summary>
/// A wrapper class for Playwright interactions, providing high-level methods for web element manipulation and testing.
/// </summary>
public sealed partial class Wrapper
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex WhiteSpaceRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex HyphenRegex();

    private readonly IPage _page;

    public Wrapper(IPage page)
    {
        _page = page;
    }

    #region Navigation

    /// <summary>
    /// Waits until the browser URL exactly matches the expected URL. Fails the test if timeout occurs.
    /// </summary>
    public async Task WaitUntilUrl(string expectedUrl)
    {
        try
        {
            await _page.WaitForURLAsync(url => url == expectedUrl, new PageWaitForURLOptions
            {
                Timeout = WrapperConstants.DEFAULT_TIMEOUT
            });
        }
        catch (Exception)
        {
            Assert.Fail($"url not found! {expectedUrl}");
        }
    }

    /// <summary>
    /// Returns the current URL of the page.
    /// </summary>
    public string ReadCurrentUrl()
    {
        return _page.Url;
    }

    /// <summary>
    /// Reloads the current page and waits until network activity is idle.
    /// </summary>
    public async Task Reload()
    {
        await _page.ReloadAsync(new PageReloadOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle
        });
    }

    #endregion Navigation

    #region Element Finding

    public async Task<ILocator?> GetLocatorById(string id)
    {
        try
        {
            var locator = _page.Locator($"#{id}");

            // If two controllers have the same id, two Locator objects will be found
            var count = await locator.CountAsync();

            if (count == 1)
            {
                return locator;
            }
            else
            {
                TestContext.Out.WriteLine($"More than one controller with the id '{id}' was found.");
            };
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"Timeout finding element {id}: {ex.Message}");
        }
        catch (PlaywrightException ex)
        {
            TestContext.Out.WriteLine($"Playwright error for element {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Cannot find element {id}: {ex.Message}");
        }

        return null;
    }

    /// <summary>
    /// Finds an element on the page by its ID
    /// </summary>
    /// <param name="id">The ID of the element to find</param>
    /// <returns>The element handle if found, null otherwise</returns>
    public async Task<IElementHandle?> FindElementById(string id)
    {
        try
        {
            return await _page.WaitForSelectorAsync(
                CssSelectorWrapper.Wrap(AttributesAndElements.Id, id),
                new()
                {
                    State = WaitForSelectorState.Attached,
                    Timeout = WrapperConstants.ELEMENT_TIMEOUT
                });
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"Timeout finding element {id}: {ex.Message}");
        }
        catch (PlaywrightException ex)
        {
            TestContext.Out.WriteLine($"Playwright error for element {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Cannot find element {id}: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Finds an element using a CSS selector. Returns the element handle if found, null otherwise.
    /// </summary>
    public async Task<IElementHandle?> FindElementByCssSelector(string id)
    {
        try
        {
            return await _page.WaitForSelectorAsync(id, new()
            {
                State = WaitForSelectorState.Attached,
                Timeout = WrapperConstants.ELEMENT_TIMEOUT
            });
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"Timeout finding element {id}: {ex.Message}");
        }
        catch (PlaywrightException ex)
        {
            TestContext.Out.WriteLine($"Playwright error for element {id}: {ex.Message}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Cannot find element {id}: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Finds a child element with a specific class within a parent element identified by ID.
    /// </summary>
    public async Task<IElementHandle?> FindNestedElement(string parentId, string childClass)
    {
        try
        {
            return await _page.WaitForSelectorAsync($"#{parentId} .{childClass}", new()
            {
                State = WaitForSelectorState.Attached,
                Timeout = WrapperConstants.ELEMENT_TIMEOUT
            });
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"Timeout finding nested element {parentId}.{childClass}: {ex.Message}");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Error finding nested element {parentId}.{childClass}: {ex.Message}");
        }
        return null;
    }

    /// <summary>
    /// Checks if an element with the specified ID is visible on the page.
    /// </summary>
    public async Task<bool> IsElementVisibleById(string elementId)
    {
        try
        {
            var element = await _page.QuerySelectorAsync($"#{elementId}");
            return element != null && await element.IsVisibleAsync();
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"Timeout finding element {elementId}: {ex.Message}");
            return false;
        }
        catch (PlaywrightException ex)
        {
            TestContext.Out.WriteLine($"Playwright error for element {elementId}: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Error checking element visibility {elementId}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Waits until an element matching the CSS selector is visible.
    /// </summary>
    public async Task ElementIsVisibleByCssSelector(string name)
    {
        await _page.WaitForSelectorAsync(name, new() { State = WaitForSelectorState.Visible, Timeout = WrapperConstants.ELEMENT_TIMEOUT });
    }

    /// <summary>
    /// Checks if an element is disabled. Returns true if element is null or disabled.
    /// </summary>
    public async Task<bool> IsDisabled(IElementHandle? element)
    {
        if (element == null)
        {
            return true;
        }

        return await element.IsDisabledAsync();
    }

    #endregion Element Finding

    #region Scrolling

    /// <summary>
    /// Scrolls to make an element with the specified ID visible in the viewport.
    /// </summary>
    public async Task ScrollIntoViewById(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        var element = await FindElementById(elementId);

        if (element != null)
        {
            await element.ScrollIntoViewIfNeededAsync();
        }
    }

    /// <summary>
    /// Scrolls to make an element with the specified control name visible.
    /// </summary>
    public async Task ScrollIntoViewByControlName(string controlName)
    {
        await _page.WaitForSelectorAsync(
            CssSelectorWrapper.Wrap(AttributesAndElements.ControlName, controlName),
            new() { State = WaitForSelectorState.Visible, Timeout = WrapperConstants.ELEMENT_TIMEOUT });
        await _page.Locator(CssSelectorWrapper.Wrap(AttributesAndElements.ControlName, controlName)).ScrollIntoViewIfNeededAsync();
    }

    /// <summary>
    /// Scrolls to position an element with specified control name at the top of the viewport.
    /// </summary>
    public async Task ScrollOnTopByControlName(string controlName)
    {
        try
        {
            await _page.WaitForSelectorAsync(
                CssSelectorWrapper.Wrap(AttributesAndElements.ControlName, controlName),
                new() { State = WaitForSelectorState.Visible, Timeout = WrapperConstants.ELEMENT_TIMEOUT });
            var element = await _page.Locator(CssSelectorWrapper.Wrap(AttributesAndElements.ControlName, controlName)).ElementHandleAsync();
            await element.EvaluateAsync("element => element.scrollIntoView(true)");
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"The element with the control name '{controlName}' was not found or is not visible. {ex.Message}");
        }
    }

    /// <summary>
    /// Scrolls to position an element with specified ID at the top of the viewport.
    /// </summary>
    public async Task ScrollOnTopById(string elementId)
    {
        try
        {
            await _page.WaitForSelectorAsync(
                CssSelectorWrapper.Wrap(AttributesAndElements.Id, elementId),
                new() { State = WaitForSelectorState.Visible, Timeout = WrapperConstants.ELEMENT_TIMEOUT });
            var element = await _page.Locator(CssSelectorWrapper.Wrap(AttributesAndElements.Id, elementId)).ElementHandleAsync();
            await element.EvaluateAsync("element => element.scrollIntoView(true)");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"The element with the id '{elementId}'create a error. {ex.Message}");
        }
    }

    /// <summary>
    /// Scrolls down a specific container on the page
    /// </summary>
    /// <param name="containerSelector">CSS selector for the container to be scrolledr</param>
    /// <param name="scrollToBottom">If true, scroll to the end of the container</param>
    /// <param name="pixels">Number of pixels to scroll when not scrolling to the end</param>
    public async Task ScrollPageDown(ScrollMethod scrollMethod = ScrollMethod.Auto, int pixels = 500, bool scrollToBottom = true)
    {
        if (scrollMethod == ScrollMethod.Auto)
        {
            try
            {
                if (scrollToBottom)
                    await _page.EvaluateAsync(@"window.scrollTo(0, document.body.scrollHeight)");
                else
                    await _page.EvaluateAsync($"window.scrollBy(0, {pixels})");

                await Wait500();
                return;
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Standard scrolling failed, try other methods: {ex.Message}");
            }

            try
            {
                string script = scrollToBottom
                    ? "document.documentElement.scrollTo(0, document.documentElement.scrollHeight)"
                    : $"document.documentElement.scrollBy(0, {pixels})";

                await _page.EvaluateAsync(script);
                await Wait500();
                return;
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"HTML element scrolling failed: {ex.Message}");
            }

            try
            {
                if (scrollToBottom)
                {
                    // Press several end keys to ensure that we arrive at the end.
                    for (int i = 0; i < 5; i++)
                    {
                        await _page.Keyboard.PressAsync("End");
                        await Wait100();
                    }
                }
                else
                {
                    await _page.Keyboard.PressAsync("PageDown");
                }

                await Wait500();
                return;
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Keyboard scrolling failed: {ex.Message}");
            }
        }
        else
        {
            try
            {
                switch (scrollMethod)
                {
                    case ScrollMethod.WindowScroll:
                        if (scrollToBottom)
                            await _page.EvaluateAsync(@"window.scrollTo(0, document.body.scrollHeight)");
                        else
                            await _page.EvaluateAsync($"window.scrollBy(0, {pixels})");
                        break;

                    case ScrollMethod.HtmlScroll:
                        string script = scrollToBottom
                            ? "document.documentElement.scrollTo(0, document.documentElement.scrollHeight)"
                            : $"document.documentElement.scrollBy(0, {pixels})";

                        await _page.EvaluateAsync(script);
                        break;

                    case ScrollMethod.KeyboardScroll:
                        if (scrollToBottom)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                await _page.Keyboard.PressAsync("End");
                                await Wait100();
                            }
                        }
                        else
                        {
                            await _page.Keyboard.PressAsync("PageDown");
                        }
                        break;

                    case ScrollMethod.MainContent:
                        string contentScript = scrollToBottom
                            ? @"
                            const content = document.querySelector('main') || 
                                           document.querySelector('.content') || 
                                           document.querySelector('.main-content');
                            if (content) content.scrollTo(0, content.scrollHeight);
                          "
                            : $@"
                            const content = document.querySelector('main') || 
                                           document.querySelector('.content') || 
                                           document.querySelector('.main-content');
                            if (content) content.scrollBy(0, {pixels});
                          ";

                        await _page.EvaluateAsync(contentScript);
                        break;
                }

                await Wait500();
            }
            catch (Exception ex)
            {
                TestContext.Out.WriteLine($"Scrolling with method {scrollMethod} failed: {ex.Message}");
                throw;
            }
        }

    }
    /// <summary>
    /// Available methods for scrolling the page
    /// </summary>
    public enum ScrollMethod
    {
        /// <summary>Tries different methods automatically</summary>
        Auto,

        /// <summary>Uses window.scrollTo/scrollBy</summary>
        WindowScroll,

        /// <summary>Scrolls the HTML element directly</summary>
        HtmlScroll,

        /// <summary>Verwendet Tastenanschläge zum Scrollen</summary>
        KeyboardScroll,

        /// <summary>Versucht, den Hauptinhalt zu finden und zu scrollen</summary>
        MainContent
    }


    /// <summary>
    /// Debugging method to collect information about scrollable elements on the page
    /// </summary>
    public async Task<string> DebugScrollableElements()
    {
        string script = @"
        function isScrollable(element) {
            const style = window.getComputedStyle(element);
            const overflow = style.getPropertyValue('overflow');
            const overflowY = style.getPropertyValue('overflow-y');
            
            return (overflow === 'auto' || overflow === 'scroll' || 
                    overflowY === 'auto' || overflowY === 'scroll') &&
                   element.scrollHeight > element.clientHeight;
        }

        const scrollableElements = [];
        const allElements = document.querySelectorAll('*');
        
        allElements.forEach(el => {
            if (isScrollable(el)) {
                scrollableElements.push({
                    tag: el.tagName,
                    id: el.id,
                    class: el.className,
                    scrollHeight: el.scrollHeight,
                    clientHeight: el.clientHeight
                });
            }
        });
        
        return JSON.stringify(scrollableElements);
    ";

        string result = await _page.EvaluateAsync<string>(script);
        TestContext.Out.WriteLine("Scrollable elements on the page:");
        TestContext.Out.WriteLine(result);

        return result;
    }
    #endregion Scrolling

    #region Input Handling

    /// <summary>
    /// Fills an input field with a value and presses Enter.
    /// </summary>
    public async Task FillInputAndEnterById(string elementId, string value)
    {
        var locator = await GetLocatorById(elementId);

        if (locator is not null)
        {
            var options = new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = WrapperConstants.ELEMENT_TIMEOUT
            };
            await locator.WaitForAsync(options);

            await locator.ClickAsync();
            await locator.FillAsync(value);
            await locator.PressAsync(Keys.Enter);
            //await locator.BlurAsync();
        }
        else
        {
            var element = await FindElementById(elementId);

            if (element is not null)
            {
                await element.FillAsync(value);
                await element.PressAsync(Keys.Enter);
            }
        }
    }

    /// <summary>
    /// Fills an input field with a value without pressing Enter.
    /// </summary>
    public async Task FillInputById(string elementId, string value)
    {
        var locator = await GetLocatorById(elementId);

        if (locator is not null)
        {
            var options = new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = WrapperConstants.ELEMENT_TIMEOUT
            };
            await locator.WaitForAsync(options);

            await locator.ClickAsync();
            await locator.FillAsync(value);
            await locator.BlurAsync();
        }
        else
        {
            var element = await FindElementById(elementId);

            if (element is not null)
            {
                await element.FillAsync(value);
            }
        }
    }

    /// <summary>
    /// Clears the content of an input field.
    /// </summary>
    public async Task ClearInputById(string elementId)
    {
        var locator = await GetLocatorById(elementId);

        if (locator is not null)
        {
            await locator.ClickAsync();
            await locator.ClearAsync();
        }
        else
        {
            var element = await FindElementById(elementId);

            if (element is not null)
            {
                await element.FillAsync(string.Empty);
            }
        }
    }

    /// <summary>
    /// Performs a search operation using an input field.
    /// </summary>
    public async Task SearchById(string id, string value)
    {
        await FillInputAndEnterById(id, value);
    }

    public async Task SetInputFileByCssSelector(string cssSelector, string path)
    {
        await _page.SetInputFilesAsync(cssSelector, path);
    }

    #endregion Input Handling

    #region Button and Control Actions

    /// <summary>
    /// Clicks a button element identified by its ID. Makes multiple attempts if initial click fails.
    /// </summary>
    /// <param name="elementId">The ID of the button element to click</param>
    /// <exception cref="Exception">Thrown when clicking fails after multiple attempts or element is disabled</exception>
    public async Task ClickButtonById(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        var element = await FindElementById(elementId);
        if (!(await IsDisabled(element)))
        {
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    await element!.ClickAsync(new() { Force = true, Timeout = WrapperConstants.ELEMENT_TIMEOUT });
                    return;
                }
                catch (Exception) when (i < 2)
                {
                    await Wait1000();
                }
            }
            throw new Exception($"Failed to click element {elementId} after 3 attempts");
        }
    }

    /// <summary>
    /// Clicks on a tab element (wrapper for ClickButtonById).
    /// </summary>
    public async Task ClickTabsById(string elementId)
    {
        await ClickButtonById(elementId);
    }

    /// <summary>
    /// Clicks on a checkbox element (wrapper for ClickButtonById).
    /// </summary>
    public async Task ClickCheckBoxById(string elementId)
    {
        await ClickButtonById(elementId);
    }

    #endregion Button and Control Actions

    #region Selection and Dropdown Handling

    /// <summary>
    /// Opens a dropdown and selects a specific option by ID.
    /// It only works for dropdowns with a single selection possibility.
    /// </summary>
    public async Task ChooseFirstOptionById(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        var element = await FindElementById(elementId);
        if (element != null)
        {
            await element.ClickAsync();
            await ElementIsVisibleByCssSelector(AttributesAndElements.MatOption);
            var firstOption = await FindElementByCssSelector(AttributesAndElements.MatOption);
            if (firstOption != null)
            {
                await firstOption.ClickAsync();
            }
        }
    }

    /// <summary>
    /// Opens a dropdown and selects a specific option by ID.
    /// It only works for dropdowns with a single selection possibility.
    /// </summary>
    public async Task SelectOptionById(string elementId, string selectionId)
    {
        await WaitForElementToBeStable(elementId);
        var element = await FindElementById(elementId);
        if (element != null)
        {
            await element.ClickAsync();
            await ElementIsVisibleByCssSelector(AttributesAndElements.MatOption);

            var option = await FindElementById(selectionId);
            if (option != null)
            {
                await option.ClickAsync();
            }
        }
    }

    /// <summary>
    /// Types in an autocomplete field and selects the first suggestion.
    /// </summary>
    public async Task SelectFirstAutocompleteById(string elementId, string value)
    {
        await WaitForElementToBeStable(elementId);
        var element = await FindElementById(elementId);
        if (element != null)
        {
            await element.FillAsync(value);
            await ElementIsVisibleByCssSelector(AttributesAndElements.MatOption);

            var firstOption = await FindElementByCssSelector(AttributesAndElements.MatOption);
            if (firstOption != null)
            {
                await firstOption.ClickAsync();
            }
        }
    }

    /// <summary>
    /// Selects multiple options from a multi-select dropdown.
    /// </summary>
    public async Task SelectOptionsById(string elementId, string[] selectionsId)
    {
        await WaitForElementToBeStable(elementId);
        var element = await FindElementById(elementId);
        if (element != null)
        {
            await element.ClickAsync();
            await ElementIsVisibleByCssSelector(AttributesAndElements.MatOption);

            foreach (var itemId in selectionsId)
            {
                var lastOption = await FindElementById(itemId);
                if (lastOption != null)
                {
                    await lastOption.ClickAsync();
                }
            }

            await element.PressAsync(Keys.Escape);
        }
    }

    #endregion Selection and Dropdown Handling

    #region Table Operations

    /// <summary>
    /// Counts the number of rows in a table.
    /// </summary>
    public async Task<int> SumRowFromTable(string id)
    {
        return await _page.Locator($"#{id} tbody tr").CountAsync();
    }

    /// <summary>
    /// Adds a new row to an editable table, fills it with values, and saves it.
    /// </summary>
    public async Task FillEditTableRow(string addRowId, string saveRowId, int index, KeyValuePair<string, string>[] items)
    {
        await ScrollIntoViewById(addRowId);
        await WaitForSpinnerToDisappear();
        await Wait1000();
        await ClickButtonById(addRowId);
        await WaitForElementToBeStable(addRowId);

        foreach (var item in items)
        {
            await ScrollIntoViewById(item.Key + index.ToString());
            await WaitForElementToBeStable(item.Key + index.ToString());
            await FillInputAndEnterById(item.Key + index.ToString(), item.Value);
            await Wait500();
        }

        await ScrollIntoViewById(saveRowId + index.ToString());
        await WaitForElementToBeStable(saveRowId + index.ToString());
        await ClickButtonById(saveRowId + index.ToString());
        await WaitForSpinnerToDisappear();
    }

    /// <summary>
    /// Adds and fills a row in a linked editable table.
    /// </summary>
    public async Task FillLinkedEditTableRow(string addRowId, int index, string saveRowId, KeyValuePair<string, string>[] items)
    {
        await ScrollIntoViewById(addRowId);
        await WaitForSpinnerToDisappear();
        await Wait1000();
        await ClickButtonById(addRowId);
        await WaitForElementToBeStable(addRowId);

        foreach (var item in items)
        {
            await ScrollIntoViewById(item.Key + index.ToString());
            await WaitForElementToBeStable(item.Key + index.ToString());
            await FillInputAndEnterById(item.Key + index.ToString(), item.Value);
            await Wait500();
            await ScrollIntoViewById($"{saveRowId}{index}");
            await ClickButtonById($"{saveRowId}{index}");
        }

        await WaitForSpinnerToDisappear();
    }

    /// <summary>
    /// Adds a row with dropdown selection and fills it with values.
    /// </summary>
    public async Task FillEditTableWithPulldownRow(string addRowId, string saveRowId, int index, string PulldownId, string selectionId, KeyValuePair<string, string>[] items)
    {
        await ScrollIntoViewById(addRowId);
        await ClickButtonById(addRowId);
        await WaitForElementToBeStable(addRowId);

        await SelectOptionById(PulldownId + index.ToString(), selectionId);

        foreach (var item in items)
        {
            await FillInputAndEnterById(item.Key + index.ToString(), item.Value);
        }

        await ClickButtonById(saveRowId + index.ToString());
    }

    #endregion Table Operations

    #region Reading Content

    /// <summary>
    /// Reads the text content of a chip element.
    /// </summary>
    public async Task<string> ReadChip(string chipId)
    {
        var spanElement = await FindNestedElement(chipId, AttributesAndElements.ChipSpan);
        if (spanElement == null)
        {
            throw new Exception($"Chip text element not found for chip {chipId}");
        }

        return await spanElement.TextContentAsync() + string.Empty;
    }

    /// <summary>
    /// Reads the text content of a div element.
    /// </summary>
    public async Task<string> ReadDiv(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        var divElement = await FindElementById(elementId);

        return divElement != null ?
            (await divElement.TextContentAsync() ?? string.Empty) :
            string.Empty;
    }

    /// <summary>
    /// Reads the text content of an anchor element using CSS selector a#elementId.
    /// </summary>
    public async Task<string> ReadAnchorTextById(string elementId)
    {
        try
        {
            var selector = $"a#{elementId}";
            await _page.WaitForSelectorAsync(selector, new()
            {
                State = WaitForSelectorState.Visible,
                Timeout = WrapperConstants.ELEMENT_TIMEOUT
            });

            var text = await _page.EvaluateAsync<string>($"document.querySelector('{selector}').textContent.trim()");
            return text ?? string.Empty;
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Error reading anchor text with ID '{elementId}': {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Reads the value of an input field.
    /// </summary>
    public async Task<string> ReadInput(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        var inputElement = await FindElementById(elementId);
        if (inputElement == null)
        {
            return string.Empty;
        }

        return await inputElement.EvaluateAsync<string>("el => el.value") ?? string.Empty;
    }

    /// <summary>
    /// Reads the selected text of a select element.
    /// </summary>
    public async Task<string> ReadSelect(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        var selectElement = await FindElementById(elementId);
        if (selectElement == null)
        {
            return string.Empty;
        }

        string selectedText = await selectElement.EvaluateAsync<string>(@"el => {
        const selected = el.options[el.selectedIndex];
        return selected ? selected.text : '';
    }") ?? string.Empty;

        return selectedText;
    }

    /// <summary>
    /// Retrieves a value from the browser's localStorage by key.
    /// </summary>
    public async Task<string> ReadLocalStorage(string key)
    {
        string accessToken = string.Empty;
        try
        {
            accessToken = await _page.EvaluateAsync<string>($"() => localStorage.getItem('{key}')");
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"ReadlocalStorage: {ex.Message}");
        }

        return accessToken;
    }

    /// <summary>
    /// Reads the already selected text of a mat-select element
    /// </summary>
    /// <param name="elementId">The ID of the mat-select element.</param>
    /// <returns>The currently selected text, or an empty string if nothing is selected</returns>
    public async Task<string> ReadMatSelect(string elementId)
    {
        await WaitForElementToBeStable(elementId);
        try
        {
            var selectedText = await _page.EvaluateAsync<string>($@"() => {{
            const selectElement = document.getElementById('{elementId}');
            if (!selectElement) return '';
            
            const valueTextElement = selectElement.querySelector('.mat-mdc-select-value-text');
            if (!valueTextElement) return '';
            
            return valueTextElement.textContent.trim();
        }}");

            return selectedText ?? string.Empty;
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Error reading selected value from mat-select with ID '{elementId}': {ex.Message}");
            return string.Empty;
        }
    }
    #endregion Reading Content

    #region Waiting

    /// <summary>
    /// Waits until a loading spinner disappears from the page.
    /// </summary>
    public async Task WaitForSpinnerToDisappear()
    {
        var spinner = _page.Locator(".ob-overlay:visible").First;

        if (!await spinner.IsVisibleAsync())
        {
            return;
        }

        try
        {
            await spinner.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = WrapperConstants.OVERLAY_TIMEOUT
            });
        }
        catch (TimeoutException)
        {
            TestContext.Out.WriteLine("Visible Oblique overlay did not close within 5 seconds; continuing with the next local UI check.");
        }
    }

    /// <summary>
    /// Waits until an Angular reactive-form input has completed validation and is valid.
    /// </summary>
    public async Task WaitForInputValidationById(string elementId)
    {
        var validInput = _page.Locator($"[id='{elementId}'].ng-valid");

        await validInput.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = WrapperConstants.ELEMENT_TIMEOUT
        });
    }
    /// <summary>
    /// Waits until notification alerts disappear, with fallback to forcibly remove them.
    /// </summary>
    public async Task WaitForNotificationToDisappear()
    {
        // Prüfe die Anzahl der sichtbaren Alerts
        var alertCount = await _page.Locator("ob-alert.ob-notification").CountAsync();

        if (alertCount > 0)
        {
            try
            {
                // Warte darauf, dass der erste Alert verschwindet
                var firstAlert = _page.Locator("ob-alert.ob-notification").First;
                await firstAlert.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Hidden,
                    Timeout = WrapperConstants.DEFAULT_TIMEOUT
                });
            }
            catch (TimeoutException)
            {
                TestContext.Out.WriteLine("Alert timeout - trying to continue");
                try
                {
                    await _page.EvaluateAsync(@"() => {
                    document.querySelectorAll('ob-alert.ob-notification').forEach(alert => alert.remove());
                }");
                }
                catch
                {
                    TestContext.Out.WriteLine("Could not force remove alerts");
                }
            }
        }
    }

    /// <summary>
    /// Waits until the requested element is visible. Angular applications may keep background requests active,
    /// so network idleness is deliberately not used as a page-ready signal.
    /// </summary>
    public async Task WaitForElementToBeStable(string id)
    {
        try
        {
            await _page.WaitForSelectorAsync(
                CssSelectorWrapper.Wrap(AttributesAndElements.Id, id),
                new()
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = WrapperConstants.ELEMENT_TIMEOUT
                });

            await Wait100();
        }
        catch (TimeoutException ex)
        {
            TestContext.Out.WriteLine($"UI wait timed out after {WrapperConstants.ELEMENT_TIMEOUT}ms: element '{id}' was not visible. {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// Wait for specific time periods in milliseconds.
    /// </summary>
    public async Task Wait100()
    {
        await Wait(100);
    }

    /// <summary>
    /// Wait for specific time periods in milliseconds.
    /// </summary>
    public async Task Wait500()
    {
        await Wait(500);
    }

    /// <summary>
    /// Wait for specific time periods in milliseconds.
    /// </summary>
    public async Task Wait1000()
    {
        await Wait(1000);
    }

    /// <summary>
    /// Wait for specific time periods in milliseconds.
    /// </summary>
    public async Task Wait1500()
    {
        await Wait(1500);
    }

    /// <summary>
    /// Wait for specific time periods in milliseconds.
    /// </summary>
    public async Task Wait3000()
    {
        await Wait(3000);
    }

    /// <summary>
    /// Wait for specific time periods in milliseconds.
    /// </summary>
    public async Task Wait3500()
    {
        await Wait(3500);
    }

    private async Task Wait(int timeout)
    {
        await _page.WaitForTimeoutAsync(timeout);
    }

    #endregion Waiting

    #region Validation

    /// <summary>
    /// Validates the content of a specified HTML div element against an expected value.
    /// Supports both strict and partial matching of the content.
    /// </summary>
    /// <param name="elementId">The ID of the HTML div element to validate</param>
    /// <param name="expectedValue">The expected content value to compare against</param>
    /// <param name="strict">
    /// If true, checks for exact match between expected and actual values.
    /// If false, checks if the actual value contains the expected value.
    /// </param>
    /// <returns>
    /// true - if validation succeeds according to strict/non-strict comparison
    /// false - if validation fails according to strict/non-strict comparison
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when elementId or expectedValue is null</exception>
    public async Task<bool> ValidateDivContent(string elementId, string expectedValue, bool strict = true)
        => await ValidateContent(elementId, expectedValue, strict, ReadDiv, "div");

    /// <summary>
    /// Validates the content of a specified chip element against an expected value.
    /// Supports both strict and partial matching of the content.
    /// </summary>
    /// <param name="chipId">The ID of the chip element to validate</param>
    /// <param name="expectedValue">The expected content value to compare against</param>
    /// <param name="exactMatch">
    /// If true, checks for exact match between expected and actual values.
    /// If false, checks if the actual value contains the expected value.
    /// </param>
    /// <returns>
    /// true - if validation succeeds according to strict/non-strict comparison
    /// false - if validation fails according to strict/non-strict comparison
    /// </returns>
    /// <exception cref="ArgumentNullException">Thrown when chipId or expectedValue is null</exception>
    public async Task<bool> ValidateChipContent(string chipId, string expectedValue, bool exactMatch = true)
        => await ValidateContent(chipId, expectedValue, exactMatch, ReadChip, "chip");

    /// <summary>
    /// Normalizes text by trimming, replacing multiple spaces with a single space, and normalizing Unicode.
    /// </summary>
    public string NormalizeText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        text = text.Trim();
        text = WhiteSpaceRegex().Replace(text, "");
        text = HyphenRegex().Replace(text, "");
        text = text.Replace("|", "");
        text = text.Replace("\u2010", ""); // Very special case, that may happen when reading values using playwright

        text = text.Normalize(NormalizationForm.FormC);

        return text;
    }

    private async Task<bool> ValidateContent(
        string id,
        string expectedValue,
        bool strict,
        Func<string, Task<string>> readMethod,
        string elementType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id, nameof(id));

        try
        {
            var actualValue = await readMethod(id);
            if (string.IsNullOrWhiteSpace(actualValue))
            {
                return string.IsNullOrWhiteSpace(expectedValue);
            }

            actualValue = NormalizeText(actualValue);
            var expectedNormalized = NormalizeText(expectedValue);

            TestContext.Out.WriteLine($"actualValue:{actualValue}, expectedValue:{expectedNormalized}");

            return strict
                ? expectedNormalized.Equals(actualValue)
                : actualValue.Contains(expectedNormalized);
        }
        catch (Exception ex)
        {
            TestContext.Out.WriteLine($"Validate{elementType}Content failed for {elementType} '{id}'. Error: {ex.Message}");
            return false;
        }
    }

    #endregion Validation

    #region Download

    /// <summary>
    /// Clicks the element identified by <paramref name="elementId"/> to trigger a file download,
    /// waits for the download to complete within the default timeout, and returns the
    /// path to the downloaded file in Playwright’s temporary download directory.
    /// </summary>
    /// <param name="elementId">
    /// The ID of the clickable element (e.g. a button or link) that initiates the download.
    /// </param>
    /// <returns>
    /// A <see cref="string"/> representing the full file system path where Playwright
    /// has stored the downloaded file temporarily.
    /// </returns>
    public async Task<string> ClickDownloadById(string elementId)
    {
        var download = await _page.RunAndWaitForDownloadAsync(
        async () => await ClickButtonById(elementId),
        new PageRunAndWaitForDownloadOptions { Timeout = WrapperConstants.DEFAULT_TIMEOUT }
    );

        var tempPath = await download.PathAsync();
        return tempPath;
    }

    
    #endregion Download

    #region Key

    public async Task PressKey(string key)
    {
        await _page.Keyboard.PressAsync(key);
    }

    #endregion Key
}