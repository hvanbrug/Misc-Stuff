I'll create a short plan outlining the explanation and examples.

Progress: plan created. Next: concise explanation with examples.

**`Func` object**:
- Description: A reference to a standalone (global) function. Use when you want to pass a named function without binding extra context.
- Example:
  - Define: `MyFunc(a, b) { MsgBox a " " b }`
  - Get reference and call: `f := Func("MyFunc")` then `f.Call("one", "two")`

**`BoundFunc` (method reference)**:
- Description: A method reference bound to an object instance (implicitly carries `this`). Use when you want the method called with its object context.
- Example:
  - Method: `class C { M(x) { MsgBox x } }`
  - Use: `obj := C.Create(); action := obj.M` ; later `action.Call("hi")` — `this` is `obj`.

**`Closure` / Lambda**:
- Description: An anonymous function that captures surrounding variables. Use to adapt signature, pass arguments, or ignore incoming args.
- Examples:
  - Capture: `n := 5; cb := (v) => MsgBox v + n`
  - Force no-arg call: `cb2 := (*) => MyFunc("fixed")` — works if caller supplies unexpected args.

When to pass which:
- Pass a `Func`/`BoundFunc` directly when the callee will invoke it with the exact signature you expect and you don't need captured variables.
- Use a `Closure` when you need to:
  - Pre-fill arguments (partial application).
  - Change the call signature (e.g., swallow or rename args).
  - Capture local variables or ensure correct `this` context if a plain function reference wouldn't supply it.
- Use `BoundFunc` when you want implicit `this` binding and no extra wrapper.

Calling and detection:
- Many APIs accept any callable: `Func`, `BoundFunc`, or `Closure`. You can check with `Type(fn)` returning `"Func"`, `"BoundFunc"`, or `"Closure"`.
- Invoke uniformly via `fn.Call(...)` (works for all three), or simply `fn(...)` in many contexts.

Common pitfalls:
- Passing a method name string (like `"M"`) is not the same as a callable; prefer `obj.M` or `Func("M")`/`Func("MyFunc")`.
- Closures capture by reference — mutated captured variables affect later calls.
- If the API will call your function with unexpected args, use `(*) => ...` to accept arbitrary args safely.

Repository note:
- In this codebase `IsAFunction` checks Type equals `"Func"`, `"BoundFunc"`, or `"Closure"`, so any of those are acceptable to `RegisterSymbol` as `action`.

---

**Short answer:** In AutoHotkey v2 functions are first-class values. A bare function name in an expression (e.g. `MyFunc`) yields a callable `Func` object; a method reference like `obj.M` yields a `BoundFunc`; an anonymous `(...) => ...` yields a `Closure`. `Func("Name")` explicitly returns a `Func` object from a string (useful when you only have the name as text). Use closures when you need to capture or adapt arguments.

- **Bare named function**
  - What it is: a `Func` object when used as a value.
  - When it works: when you refer to the function in expression context and the callee expects a callable.
  - Example:
    - Define: `MyFunc(a,b) { MsgBox a " " b }`
    - Pass: `DoSomething( MyFunc )` or `f := MyFunc`
    - Call: `f.Call("one","two")` (or `f("one","two")`)
- **`Func("Name")`**
  - What it does: returns a `Func` object by string name; useful when you only have the name at runtime.
  - When to use: when the name is in a variable: `name := "MyFunc"; f := Func(name)`
  - Note: passing the raw string `"MyFunc"` to an API that expects a callable will not work (it's just a string).
- **Method / `BoundFunc`**
  - How to get it: `action := obj.Method` (no parentheses).
  - Behavior: when called, `this` is automatically the `obj`.
  - Use when you want method context preserved without wrapping.
- **Closure / Lambda**
  - How to get it: `cb := (x,y) => MyFunc(x,y)` or to capture locals `n := 2; cb := (x) => x + n`.
  - When to use:
    - Pre-fill / adapt arguments (partial application).
    - Capture variables from surrounding scope.
    - Normalize call-signature (e.g. `(*) => ...` to accept any incoming args).
  - Example to adapt signature: `RegisterCallback( (*) => MyFunc("fixed") )` lets callers invoke with arbitrary args.
- **Calling / detection**
  - Detect: `Type(fn)` returns `"Func"`, `"BoundFunc"`, or `"Closure"`. Your codebase uses that in `IsAFunction`.
  - Invoke: `fn.Call(...)` or just `fn(...)` (both work for these callables).
- **Rules of thumb**
  - If you already have a callable and the callee will call it with the exact args you expect, pass it directly (`MyFunc`, `obj.Method`, or `closure`).
  - If you only have the name as text, use `Func(name)`.
  - If you need to change the signature, capture local state, or guarantee zero-arg invocation, wrap in a closure.
  - If you need `this` bound, prefer `obj.Method` (gives a `BoundFunc`) or a closure that calls `obj.Method` explicitly.

