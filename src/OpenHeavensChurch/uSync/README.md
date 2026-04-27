# uSync seed for Open Heavens Church

This folder contains a uSync export that defines the document types, templates and data types that the views in this project expect.

On first run, uSync will import these definitions into the Umbraco back office so editors don't have to recreate them manually. The included document types are:

| Alias | Purpose | Template |
| --- | --- | --- |
| `siteRoot` | Top-level container holding global settings (logo, social links, footer columns, primary CTA) | — |
| `homePage` | The home page. Renders a `hero` Block Grid plus a `contentBlocks` Block Grid | `HomePage` |
| `contactPage` | Contact page with a content block area beside the contact form | `ContactPage` |
| `mediaPage` | Pulls latest YouTube videos and the latest songs (with streaming-platform menus) | `MediaPage` |
| `articlesPage` | Listing page; can render Umbraco children or a third-party JSON feed | `ArticlesPage` |
| `articlePage` | Individual article (when authored in Umbraco) | `ArticlePage` |

Block element types live under `ContentTypes/Blocks/Grid` (top-level grid blocks) and `ContentTypes/Blocks/List` (inner list blocks used for nesting).

> The XML files here cover the document type definitions and template registrations. Property editor configurations (Block Grid configurations, Multi-URL Picker, Rich Text Editor, etc.) are configured in `DataTypes`. Adjust the editor IDs/Aliases there if you need to match specific tenants.
