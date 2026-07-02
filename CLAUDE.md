# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SoyoRuntimeConsole is a Unity runtime debug console package (`com.github.theplayer571.soyo-runtime-console`, Unity
2022.3+). It provides a command-line interface for Unity games with incremental parsing, autocomplete, and a pluggable
parameter handler system.

## Build & Test

- **Open in Unity**: The user opens the root folder as a Unity project (2022.3+).
- **Test authorship**: **All tests are written by Claude (AI).** The user does not write tests — Claude is responsible
  for writing, updating, and maintaining all test code.
- **Run tests**: Tests are run **manually by the user** in the Unity Editor. Claude does NOT run tests — it has no
  Unity integration and cannot drive the Editor. The user opens **Window → General → Test Runner**, selects
  **EditMode**, and runs tests under `ThePlayer571.SoyoRuntimeConsole.Editor.Tests`. Tests are Editor-only (NUnit,
  defined in `Tests/Editor/`).
- **After making changes**: Claude should remind the user to run the tests manually to verify.

## Samples

Samples are authored under `Assets/Samples/` during development. The canonical package location is
`Packages/com.github.theplayer571.soyo-runtime-console/Samples~` (Unity's `Samples~` convention — hidden from
the Editor, surfaced in Package Manager for import). If a Sample isn't found in one location, check the other.

## Language

- **Plan mode**: When entering plan mode (`EnterPlanMode`), always write the plan in **Chinese (中文)**. The user
  reads Chinese — mixed-language or English plans are difficult to follow.

## Scope of Work

**Only write code and tests.** Do not modify documentation files (e.g. `README.md`, `CHANGELOG.md`, etc.) or any
other non-code/non-test files unless the user explicitly asks. If a change you make would warrant a documentation
update, mention it to the user and let them decide — do not update docs yourself.

## Important: TODOList.md

Do **not** read `TODOList.md` at the repo root. It is the user's personal scratch file and is not intended for
Claude to consume.

## Architecture

The core package lives under `Packages/com.github.theplayer571.soyo-runtime-console/` with three assemblies:

| Assembly                                       | Path            | Role                                                       |
|------------------------------------------------|-----------------|------------------------------------------------------------|
| `ThePlayer571.SoyoRuntimeConsole`              | `Runtime/`      | Core library                                               |
| `ThePlayer571.SoyoRuntimeConsole.Editor`       | `Editor/`       | Editor-only code (currently empty)                         |
| `ThePlayer571.SoyoRuntimeConsole.Editor.Tests` | `Tests/Editor/` | Editor tests (NUnit, constrained to `UNITY_INCLUDE_TESTS`) |

## Null-Handling Convention

This project does **NOT** use C# nullable reference types (`#nullable`). Instead, all **public and protected members**
must annotate nullability using these attributes from `System.Diagnostics.CodeAnalysis`:

| Attribute | Usage |
|-----------|-------|
| `[DisallowNull]` | Input parameter/return value **must not** be null |
| `[AllowNull]` | Input parameter **may** be null (even if the type is non-nullable) |
| `[NotNull]` | Return value **will not** be null (even if the type is nullable) |
| `[MaybeNull]` | Return value **may** be null (even if the type is non-nullable) |

**Rules:**
- Every public/protected method parameter and return value must have one of these attributes (unless the nullability
  is obvious from context, such as `void` return or value-type parameters).
- **Never** use `string?`, `T?` (on reference types), or any nullable reference type annotation (`?` on ref types).
- Nullable **value types** (`int?`, `ConsoleKey?`, `bool?`) are fine — they are `Nullable<T>`, unrelated to the
  nullable reference types feature.
- Do **not** write runtime null checks for parameters already marked `[DisallowNull]` — let the CLR throw
  `ArgumentNullException` or `NullReferenceException` naturally. The attribute serves as the documentation.
- Private/internal members may use the same convention but it's less critical.