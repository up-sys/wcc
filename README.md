# Website Certificate Checker

```cli
$ wcc google.com apple.com amazon.com facebook.com x.com
https://amazon.com    Expiration date: 2027-04-06  Remaining days: 156  Issuer: DigiCert Inc           IPv4 TLS 1.3  IPv6          DNSSEC
https://google.com    Expiration date: 2026-12-30  Remaining days: 60   Issuer: Google Trust Services  IPv4 TLS 1.3  IPv6 TLS 1.3  DNSSEC
https://x.com         Expiration date: 2026-12-27  Remaining days: 57   Issuer: Let's Encrypt          IPv4 TLS 1.3  IPv6          DNSSEC
https://apple.com     Expiration date: 2026-12-23  Remaining days: 53   Issuer: Apple Inc.             IPv4 TLS 1.3  IPv6 TLS 1.3  DNSSEC
https://facebook.com  Expiration date: 2026-11-08  Remaining days: 7    Issuer: DigiCert Inc           IPv4 TLS 1.3  IPv6 TLS 1.3  DNSSEC
```

```cli
$ wcc --help
Description:
  Website Certificate Checker

Usage:
  wcc [<urls>...] [options]

Arguments:
  <urls>

Options:
  -f, --file <file>                      Get urls from file
  -t, --elapsed-time                     Show elapsed time
  -d, --remaining-days <remaining-days>  Show only certificates that expire in _ days
  -?, -h, --help                         Show help and usage information
  --version                              Show version information
```

When an active network interface has a usable IPv6 address (beyond loopback or
link-local), wcc checks IPv4 and IPv6 separately and adds their columns after
the issuer, followed by DNSSEC: `IPv4 TLS 1.3  IPv6 TLS 1.2  DNSSEC`. The
negotiated SSL/TLS version is shown after each
successful IP protocol check. Each IP protocol name is green when the connection
and TLS certificate validation succeed; SSL/TLS versions use the default console
color. Failed checks show only the IP protocol name in red (including missing
DNS records). IPv4 and IPv6 use separate aligned columns, so missing SSL/TLS
versions do not shift the protocol names.

Certificates are compared by SHA-256 fingerprint. If both checks succeed but the
certificates differ, a red `Certificates differ` warning appears next to the
protocol names. No comparison message appears when the certificates match or
either check fails. The expiration date and issuer belong to the certificate
that expires first. Each protocol check has a 10-second timeout, including the
TLS handshake.

IPv6 availability is detected once per run from local network interfaces; this
is not a guarantee of Internet connectivity. On IPv4-only machines, wcc keeps
the original output without the additional columns.

DNSSEC status is shown in the last aligned column, immediately after the IP
columns (or after the issuer when IP columns are hidden). The column always displays only `DNSSEC`;
its color indicates the result:

| Color | Status | Meaning |
| --- | --- | --- |
| Green | Valid | DNSSEC validation succeeded. |
| Gray | Unsigned | Answers have no authenticated chain of trust. |
| Red | Invalid | The resolver reported an explicit DNSSEC validation error. |
| Yellow | Unknown | Validation could not be determined. |

An unsigned result can also mean a signed zone without a secure delegation,
rather than no signatures.

wcc sends A and AAAA queries for each hostname to [Google Public DNS over
HTTPS](https://developers.google.com/speed/public-dns/docs/doh/json), with DNSSEC
validation enabled and client-subnet sharing disabled. Both responses must be
validated to show green DNSSEC. This checks the public resolver's view, which
may differ from local or private DNS. IP literals, nonexistent names, timeouts,
network failures, and generic DNS failures are reported as unknown. Explicit
DNSSEC errors take priority over unknown results. The check runs alongside TLS
checks with a shared 10-second deadline for both DNS queries, including on
IPv4-only machines.
