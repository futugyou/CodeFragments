# http log

```text
Request -->
*   [Start notification]    // "Start processing HTTP request ..." (1)
*   [Request headers]       // "Request Headers: ..." (2)
      --> Additional Handler #1 -->
        --> .... -->
          --> Additional Handler #N -->
*           [Start notification]    // "Sending HTTP request ..." (3)
*           [Request headers]       // "Request Headers: ..." (4)
                --> Primary Handler -->
                      --------Transport--layer------->
                                          // Server sends response
                      <-------Transport--layer--------
                <-- Primary Handler <--
*           [Stop notification]    // "Received HTTP response ..." (5)
*           [Response headers]     // "Response Headers: ..." (6)
          <-- Additional Handler #N <--
        <-- .... <--
      <-- Additional Handler #1 <--
*   [Stop notification]    // "End processing HTTP request ..." (7)
*   [Response headers]     // "Response Headers: ..." (8)
Response <--
```