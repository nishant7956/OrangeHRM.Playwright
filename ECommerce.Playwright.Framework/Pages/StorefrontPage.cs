using Microsoft.Playwright;
using ECommerce.Playwright.Framework.Config;

namespace ECommerce.Playwright.Framework.Pages;

// ─────────────────────────────────────────────────────────────────────────────
//  StorefrontPage
//  ==============
//  Page Object for the public Storefront homepage at /.
//  Covers: Hero, Category cards, Product grid, Add to Cart.
// ─────────────────────────────────────────────────────────────────────────────

public class StorefrontPage : BasePage
{
    // ── Locators ──────────────────────────────────────────────────────────────
    private ILocator HeroSection         => Page.Locator(".hero-section");
    private ILocator HeroTitle           => Page.Locator(".hero-title");
    private ILocator HeroPrimaryCtaBtn   => Page.Locator(".btn-hero-primary");
    private ILocator HeroStats           => Page.Locator(".hero-stats");
    private ILocator FeaturesBar         => Page.Locator(".features-bar");
    private ILocator CategoryCards       => Page.Locator(".category-card");
    private ILocator ProductCards        => Page.Locator(".product-card");
    private ILocator AnnouncementBar     => Page.Locator(".announcement-bar");
    private ILocator NavCartIcon         => Page.Locator(".nav-icon-btn[href*='Cart']").First;
    private ILocator NavAdminIcon        => Page.Locator(".nav-icon-btn[title*='Admin']").First;
    private ILocator NavBadge            => Page.Locator(".nav-badge").First;
    private ILocator EmptyStateMessage   => Page.Locator(".bi-bag-heart").Locator("..");
    private ILocator SuccessAlert        => Page.Locator(".alert-success");

    // Footer
    private ILocator FooterNewsletter   => Page.Locator(".newsletter-input");
    private ILocator FooterSubscribeBtn => Page.Locator(".newsletter-btn");
    private ILocator SocialLinks        => Page.Locator(".social-btn");

    public StorefrontPage(IPage page, TestSettings settings) : base(page, settings)
    {
    }

    // ── Navigation ────────────────────────────────────────────────────────────

    public async Task NavigateAsync()
    {
        await Page.GotoAsync($"{Settings.BaseUrl}/");
        await WaitForLoadStateAsync();
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    public async Task ClickShopNowAsync()
    {
        await HeroPrimaryCtaBtn.ClickAsync();
    }

    public async Task ClickAdminIconAsync()
    {
        await NavAdminIcon.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task ClickCartIconAsync()
    {
        await NavCartIcon.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task AddFirstProductToCartAsync()
    {
        var addBtn = Page.Locator(".btn-add-cart").First;
        await addBtn.ClickAsync();
        await WaitForLoadStateAsync();
    }

    public async Task AddProductToCartByNameAsync(string name)
    {
        var card = Page.Locator(".product-card").Filter(new LocatorFilterOptions { HasText = name });
        await card.Locator(".btn-add-cart").ClickAsync();
        await WaitForLoadStateAsync();
    }

    // ── Assertions helpers ────────────────────────────────────────────────────

    public async Task<bool> IsHeroVisibleAsync()           => await HeroSection.IsVisibleAsync();
    public async Task<bool> IsFeaturesBarVisibleAsync()    => await FeaturesBar.IsVisibleAsync();
    public async Task<bool> IsAnnouncementBarVisibleAsync() => await AnnouncementBar.IsVisibleAsync();
    public async Task<int>  GetProductCardCountAsync()     => await ProductCards.CountAsync();
    public async Task<int>  GetCategoryCardCountAsync()    => await CategoryCards.CountAsync();
    public async Task<string> GetHeroTitleTextAsync()      => await HeroTitle.InnerTextAsync();
    public async Task<bool> IsSuccessAlertVisibleAsync()   => await SuccessAlert.IsVisibleAsync();
    public async Task<bool> IsCartBadgeVisibleAsync()      => await NavBadge.IsVisibleAsync();
    public async Task<string> GetCartBadgeTextAsync()      => await NavBadge.InnerTextAsync();
    public async Task<int>  GetSocialLinkCountAsync()      => await SocialLinks.CountAsync();

    public async Task<bool> IsProductVisibleByNameAsync(string name)
    {
        var card = Page.Locator(".product-card").Filter(new LocatorFilterOptions { HasText = name });
        return await card.CountAsync() > 0;
    }
}
