# I3Locate

A device-recovery locator for **company-owned** Windows machines. It is meant to be
launched by an RMM (or any remote script) when an asset is marked **lost or stolen**
and comes back online. It gathers as much location evidence as the hardware allows,
then writes a report the RMM can collect.

> Deploy this only on devices you own or administer, and only in line with your
> organisation's acceptable-use and privacy policies and applicable law.

## What it collects

| Method | How | Replaces (legacy) |
| --- | --- | --- |
| **Wi-Fi triangulation** | Native WLAN scan → Google Geolocation API | dead `maps.googleapis.com/.../browserlocation/json` |
| **IP-based location** (fallback) | ipinfo.io `loc` | — |
| **Public IP + network owner** | ipinfo.io (ipify fallback) | scraped `checkip.dyndns.org` / `networksolutions.com/whois` |
| **Local adapters** | `System.Net.NetworkInformation` | (kept, modernised) |
| **Screenshot** | native GDI `CopyFromScreen` | bundled `preyshot.exe` |
| **Webcam still** | OpenCV `VideoCapture` | bundled `prey-webcam.exe` / `snapshot.exe` |

The old bundled third-party executables (Prey helpers, DirectShow DLL) have been
removed — everything is now managed code or maintained NuGet packages, which is
friendlier to endpoint AV and code-signing.

## Requirements

- Windows 10/11 (x64). The tool is Windows-only: it uses the native WLAN API,
  GDI screen capture and DirectShow (via OpenCV) webcam capture.
- No runtime install needed on the endpoint when published self-contained
  (see below).

## Build

```bash
dotnet build -c Release
```

## Publish for deployment

Produces a single self-contained `I3Locate.exe` (bundles the .NET runtime and the
OpenCV native libraries — expect ~150 MB) that the RMM can drop onto the endpoint
with nothing else to install:

```bash
dotnet publish -c Release -o out
```

Deploy `out\I3Locate.exe` together with `out\appsettings.json`.

## Configuration

Settings live in `appsettings.json`, and **every** value can be overridden with an
environment variable using the `I3Locate__` prefix (handy from an RMM script) — env
vars win over the file.

| Setting | Env var | Purpose |
| --- | --- | --- |
| `GoogleGeolocationApiKey` | `I3Locate__GoogleGeolocationApiKey` | Enables precise Wi-Fi triangulation. Without it, the tool falls back to coarse IP location. |
| `IpInfoToken` | `I3Locate__IpInfoToken` | Optional ipinfo.io token (higher limits). |
| `OutputDirectory` | `I3Locate__OutputDirectory` | Where the report + images are written. Default: `%ProgramData%\I3Locate`. |
| `CaptureWebcam` | `I3Locate__CaptureWebcam` | `true`/`false`. |
| `CaptureScreenshot` | `I3Locate__CaptureScreenshot` | `true`/`false`. |
| `WebhookUrl` | `I3Locate__WebhookUrl` | Optional. The JSON report is also POSTed here. |
| `HttpTimeoutSeconds` | `I3Locate__HttpTimeoutSeconds` | Network timeout (5–120). |

### Getting a Google Geolocation API key

Create a Google Cloud project, enable the **Geolocation API**, create an API key and
restrict it to that API. See
<https://developers.google.com/maps/documentation/geolocation/overview>. This is the
supported successor to the removed browser-location endpoint the tool used to call.

## Output

Written to the output directory for the RMM to collect:

- `Report.txt` — human-readable summary (location, map link, network, Wi-Fi, system).
- `report.json` — the same data, machine-readable, for ingestion.
- `screenshot.jpg`, `webcam.jpg` — when available.

## Behaviour notes

- Every collection step is best-effort: a missing camera, no Wi-Fi, or no
  interactive desktop is recorded as a warning in the report, not a crash.
- Runs once and exits; scheduling/repetition is the RMM's job.
