# Website Certificate Checker

```cli
$ wcc google.com apple.com amazon.com facebook.com x.com
https://amazon.com    Expiration date: 2025-01-08 Remaining days: 218
https://x.com         Expiration date: 2024-10-30 Remaining days: 148
https://apple.com     Expiration date: 2024-08-27 Remaining days: 85
https://google.com    Expiration date: 2024-08-05 Remaining days: 62
https://facebook.com  Expiration date: 2024-06-11 Remaining days: 7
```

```cli
$ wcc --help
Usage: wcc [options] <url> [url] [url] ...
OPTIONS:
-h, --help 		 Show this help screen
-f, --file <path> 	 Get urls from file
-t, --elapsed-time 	 Show elapsed time
-14 			 Show only certificates that expire in 14 days
-30 			 Show only certificates that expire in 30 days
```
