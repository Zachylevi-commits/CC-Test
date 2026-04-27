using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OpenHeavensChurch.Models;
using OpenHeavensChurch.Services;
using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Logging;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Web;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Web.Website.Controllers;

namespace OpenHeavensChurch.Controllers.Surface;

public class ContactController : SurfaceController
{
    private readonly IContactSubmissionService _submissions;
    private readonly ILogger<ContactController> _logger;

    public ContactController(
        IUmbracoContextAccessor umbracoContextAccessor,
        IUmbracoDatabaseFactory databaseFactory,
        ServiceContext services,
        AppCaches appCaches,
        IProfilingLogger profilingLogger,
        IPublishedUrlProvider publishedUrlProvider,
        IContactSubmissionService submissions,
        ILogger<ContactController> logger)
        : base(umbracoContextAccessor, databaseFactory, services, appCaches, profilingLogger, publishedUrlProvider)
    {
        _submissions = submissions;
        _logger = logger;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(ContactFormModel model, CancellationToken cancellationToken)
    {
        // Honeypot: silently drop submissions where the hidden field is filled in.
        if (!string.IsNullOrWhiteSpace(model.Website))
        {
            _logger.LogWarning("Honeypot triggered from {Ip}", HttpContext.Connection.RemoteIpAddress);
            return Redirect(model.ReturnUrl ?? "/");
        }

        if (!ModelState.IsValid)
        {
            return CurrentUmbracoPage();
        }

        var submission = new ContactSubmission
        {
            SubmittedAt = DateTime.UtcNow,
            Name = model.Name.Trim(),
            Email = model.Email.Trim(),
            Phone = string.IsNullOrWhiteSpace(model.Phone) ? null : model.Phone.Trim(),
            Topic = string.IsNullOrWhiteSpace(model.Topic) ? null : model.Topic.Trim(),
            Message = model.Message.Trim(),
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString(),
            SourceUrl = Request.Headers.Referer.ToString(),
            Status = "New"
        };

        try
        {
            await _submissions.SaveAsync(submission, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist contact submission");
            ModelState.AddModelError(string.Empty, "Sorry, we couldn't send your message. Please try again later or call us directly.");
            return CurrentUmbracoPage();
        }

        TempData["ContactSubmitted"] = true;

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToCurrentUmbracoPage();
    }
}
