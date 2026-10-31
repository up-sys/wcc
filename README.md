# Website Certificate Checker

```cli
$ wcc google.com apple.com amazon.com facebook.com x.com
https://amazon.com    Expiration date: 2026-11-19  Remaining days: 102  Issuer: DigiCert Inc         
https://google.com    Expiration date: 2026-10-12  Remaining days: 65   Issuer: Google Trust Services
https://apple.com     Expiration date: 2026-09-15  Remaining days: 38   Issuer: Apple Inc.           
https://x.com         Expiration date: 2026-08-26  Remaining days: 17   Issuer: Let's Encrypt        
https://facebook.com  Expiration date: 2026-08-16  Remaining days: 7    Issuer: DigiCert Inc 
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
link-local), wcc checks IPv4 and IPv6 separately and prefixes each row with
`IPv4 TLS 1.3 IPv6 TLS 1.2`, showing the negotiated SSL/TLS version after each
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
