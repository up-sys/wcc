# Website Certificate Checker

```cli
$ wcc google.com apple.com amazon.com facebook.com x.com
https://amazon.com    Expiration date: 2027-01-24  Remaining days: 325  Issuer: DigiCert Inc
https://apple.com     Expiration date: 2026-05-27  Remaining days: 84   Issuer: Apple Inc.
https://x.com         Expiration date: 2026-05-02  Remaining days: 59   Issuer: Let's Encrypt
https://google.com    Expiration date: 2026-04-27  Remaining days: 53   Issuer: Google Trust Services
https://facebook.com  Expiration date: 2026-03-12  Remaining days: 7    Issuer: DigiCert Inc
```

```cli
$ wcc --help
Usage:
  wcc [<urls>...] [options]

Arguments:
  <urls>

Options:
  -?, -h, --help                         Show help and usage information
  -f, --file <file>                      Get urls from file
  -t, --elapsed-time                     Show elapsed time
  -d, --remaining-days <remaining-days>  Show only certificates that expire in _ days
  --version                              Show version
```
