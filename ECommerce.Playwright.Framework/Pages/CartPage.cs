using Microsoft.Playwright;
using ECommerce.Playwright.Framework.Config;

namespace ECommerce.Playwright.Framework.Pages;

// ─────────────────────────────────────────────────────────────────────────────
//  CartPage
//  ========
//  Page Object for the Shopping Bag/Cart at /Cart.
// ─────────────────────────────────────────────────────────────────────────────

public class CartPage : BasePage
{
    // ── Locators ──────────────────────────────────────────────────────────────
    private ILocator CartItems         => Page.Locator(".cart-item-row");
    private ILocator EmptyBagMsg       => Page.Locator(".bi-bag").Locator("../.."); 
    private ILocator ContinueShoppingLink => Page.Locator("a[href='/']").Filter(new LocatorFilterOptions { HasText = "Continue Shopping" });
    private ILocator CheckoutButton    => Page.Locator("button[type='submit']").Filter(new LocatorFilterOptions { HasText = "Checkout" });
    private ILocator OrderSummaryTotal => Page.Locator(".fw-bold.fs-5").Last;
    private ILocator FreeShippingAlert => Page.Locator(".alert-info");
    private ILocator FreeShippingBadge => Page.Locator(".text-success").Filter(new LocatorFilterOptions { HasText = "Free" });
    private ILocator RemoveButtons     => Page.Locator("button[type='submit']").Filter(new LocatorFilterOptions { HasText = "Remove" });
    private ILocator CartTitle         => Page.Locator("h1");

    public CartPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{Settings.BaseUrl}/Cart");
        await WaitForLoadStateAsync();
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    public async Task ClickContinueShoppingAsync()
    {
        await ContinueShoppingLink.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task ClickCheckoutAsync()
    {
        await CheckoutButton.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task RemoveFirstItemAsync()
    {
        await RemoveButtons.First.ClickAsync();
        await WaitForLoadStateAsync();
    }

    // ── Assertions helpers ────────────────────────────────────────────────────

    public async Task<int>    GetCartItemCountAsync()    => await CartItems.CountAsync();
    public async Task<bool>   IsEmptyAsync()             => await CartItems.CountAsync() == 0;
    public async Task<string> GetTotalTextAsync()        => await OrderSummaryTotal.InnerTextAsync();
    public async Task<bool>   IsFreeShippingAlertVisibleAsync() => await FreeShippingAlert.IsVisibleAsync();
    public async Task<bool>   IsFreeShippingActiveAsync()       => await FreeShippingBadge.IsVisibleAsync();
    public async Task<bool>   IsCheckoutButtonVisibleAsync()    => await CheckoutButton.IsVisibleAsync();

    public async Task<bool> IsItemInCartAsync(string productName)
    {
        var item = CartItems.Filter(new LocatorFilterOptions { HasText = productName });
        return await item.CountAsync() > 0;
    }
}
