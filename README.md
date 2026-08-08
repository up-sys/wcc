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
