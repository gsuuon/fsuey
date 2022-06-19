Requirements:
- Get the expected type out of the cache `let x : Foo = cache.Get<Foo> (somePosition)`
- at least 2 caches
- for a given type and position, i get a Some if position has an existing type of same type. Else None
- can iterate through each entire cache, across all the types

Desired:
- No casting
- Decent performance

