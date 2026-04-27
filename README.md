# Open Heavens Church website

Umbraco 17 LTS site for Open Heavens Church, backed by Microsoft SQL Server. The codebase is organised around a modular block-based content model so editors can build pages from interchangeable, nestable blocks.

## Stack

- **Umbraco CMS 17.0.0** (LTS) on **.NET 9**
- **Microsoft SQL Server** via `Umbraco.Cms.Persistence.SqlServer`
- **uSync 17** to ship document types, data types and templates as code
- **NPoco** for custom database tables (used for the contact form table)

## Solution layout

```
OpenHeavensChurch.sln
src/OpenHeavensChurch/
├── Composing/                   IComposers, notifications, NPoco migrations
├── Controllers/Surface/         Contact form surface controller (form post handler)
├── Models/                      DTOs, form models, NPoco entities
├── Services/                    YouTube, Songs, Articles, Contact submission services
├── Views/
│   ├── MasterLayout/_Master.cshtml
│   ├── Partials/                Navigation, Footer, Forms, block dispatchers
│   │   ├── blockgrid/           Block Grid renderer + per-element components
│   │   └── blocklist/           Block List renderer (used for nested inner blocks)
│   ├── HomePage.cshtml
│   ├── ContactPage.cshtml
│   ├── MediaPage.cshtml
│   ├── ArticlesPage.cshtml
│   └── ArticlePage.cshtml
├── uSync/v17/                   Seed for document types, data types, templates
├── wwwroot/                     CSS and JS
├── appsettings.json
└── Program.cs
```

## Pages

| Doc type | Template | Purpose |
| --- | --- | --- |
| `homePage` | `HomePage.cshtml` | Hero block grid + content block grid |
| `contactPage` | `ContactPage.cshtml` | Contact details (block grid) + contact form that writes to SQL |
| `mediaPage` | `MediaPage.cshtml` | Latest YouTube videos + latest songs with per-platform "Listen on…" menus |
| `articlesPage` | `ArticlesPage.cshtml` | Articles listing — pulls from Umbraco children OR an external JSON feed |
| `articlePage` | `ArticlePage.cshtml` | Individual article (when authored in Umbraco) |

## Modular blocks

The Block Grid driver lives in `Views/Partials/blockgrid/`:

- `default.cshtml` — entry point per page property
- `areas.cshtml` — recursive area renderer (this is what lets blocks nest other blocks)
- `Components/{alias}.cshtml` — one partial per block element type, dispatched by alias

Block element types include:

- **Content:** `heroBlock`, `richTextBlock`, `imageBlock`, `quoteBlock`, `callToActionBlock`, `videoEmbedBlock`
- **Containers (nestable):** `columnsBlock` (areas), `accordionBlock` (panels via Block List), `tabsBlock` (panels via Block List)
- **Church-specific:** `serviceTimesBlock`, `eventCardBlock`, `teamMemberBlock`
- **Dynamic feeds:** `youTubeFeedBlock`, `songsListBlock`, `articleListBlock`, `contactFormBlock`

Inner Block List elements (used inside other blocks): `richTextBlock`, `iconCardBlock`, `buttonGroupBlock`, `serviceTimeBlock`, `accordionPanelBlock`, `tabPanelBlock`, `footerColumnBlock`.

Several blocks expose their own inner `BlockList` property so editors can drop arbitrary content inside them — including additional containers — giving you fully recursive nesting.

## Contact form → SQL Server

- The `ContactPage` template renders `Views/Partials/Forms/ContactForm.cshtml`.
- POSTs to `ContactSurfaceController.Submit` (`Controllers/Surface/ContactSurfaceController.cs`).
- The controller validates the form (with anti-forgery + honeypot) and calls `IContactSubmissionService.SaveAsync`, which uses NPoco against the `OHC_ContactSubmissions` table.
- The table is created on first run by `AddContactSubmissionsTable` (an Umbraco `MigrationBase`) executed by `RunMigrationsNotificationHandler` on `UmbracoApplicationStartedNotification`.

## YouTube + songs

- `YouTubeService` calls the YouTube Data API v3 search endpoint with the configured channel id and caches results in-memory.
- `SongsService` loads a JSON feed (`OpenHeavensChurch:Songs:FeedUrl`) where each song advertises a list of streaming platforms (Spotify, Apple Music, YouTube Music, Amazon Music, Deezer, Tidal, SoundCloud, Bandcamp, Pandora). The UI renders an accessible "Listen on…" menu per song.

## Articles

`ArticlesService` resolves articles from one of two sources, controlled per-page (`articleSource`) or per-block (`source`):

1. **Umbraco** — children of the listing page that use the `articlePage` doc type.
2. **External** — a JSON feed at the configured URL with a simple, documented schema.

Tag filtering and pagination work for both.

## Configuration (`appsettings.json`)

Replace placeholders before deploying:

- `ConnectionStrings:umbracoDbDSN` — Microsoft SQL Server connection string
- `OpenHeavensChurch:YouTube:ApiKey` and `:ChannelId`
- `OpenHeavensChurch:Songs:FeedUrl`
- `OpenHeavensChurch:Articles:ExternalFeedUrl`
- `OpenHeavensChurch:Contact:NotifyEmail`

## First run

1. Update the connection string in `appsettings.json` (or set `appsettings.Development.json`).
2. `dotnet run` from `src/OpenHeavensChurch/`.
3. Walk through the Umbraco install wizard, then log into the back office.
4. uSync will import the document types, data types and templates from `uSync/v17/` automatically.
5. Create the site root, then a Home Page, Contact Page, Media Page and Articles Page beneath it.
