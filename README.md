# RSFlowControl

## Scope

This package contains some utility classes that have zero dependencies and are concerned with flow control.
When I implemented these, I could not find a simple availeble solution that fits my needs, so I implemented one.

## LeakyBucket

This class is basically a rate limiter following the [leaky bucket](https://de.wikipedia.org/wiki/Leaky-Bucket-Algorithmus) algorithm.
It does not have to be disposed, uses minimal locking while being threadsafe.

```cs
using RSFlowControl;
int capacity = 10;
int maxLeaksPerHour = 600;

var rateLimiter = new LeakyBucket(capacity, maxLeaksPerHour);
if (rateLimiter.Leak())
    // Do work
```

## ProbabilityRamp

Probabilistic rate limiting/triggering

I use this one to occasionally make a bot trigger a reaction. The idea is that there is a chance to trigger, which increases over time,
and when it does trigger, the timer is reset back to its initial value.

```cs
using RSFlowControl;

// 5% chance initially, ramping to 40% over 2 minutes
var ramp = new ProbabilityRamp(0.05, 0.4, TimeSpan.FromMinutes(2));
if (ramp.Check())
    // Do work

// Get current probability without checking
double currentProb = ramp.CurrentChance;

// Reset the timer
ramp.Reset();
```
