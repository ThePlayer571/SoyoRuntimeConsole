# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

SoyoRuntimeConsole is a Unity runtime debug console package (`com.github.theplayer571.soyo-runtime-console`, Unity
2022.3+). It provides a command-line interface for Unity games with incremental parsing, autocomplete, and a pluggable
parameter handler system.

## Build & Test

- **Open in Unity**: Open the root folder as a Unity project (2022.3+).
- **Run tests**: In Unity, open **Window → General → Test Runner**, select **EditMode**, and run tests under
  `ThePlayer571.SoyoRuntimeConsole.Editor.Tests`. Tests are Editor-only (NUnit, defined in `Tests/Editor/`).

## Architecture

The core package lives under `Packages/com.github.theplayer571.soyo-runtime-console/` with three assemblies:

| Assembly                                       | Path            | Role                                                       |
|------------------------------------------------|-----------------|------------------------------------------------------------|
| `ThePlayer571.SoyoRuntimeConsole`              | `Runtime/`      | Core library                                               |
| `ThePlayer571.SoyoRuntimeConsole.Editor`       | `Editor/`       | Editor-only code (currently empty)                         |
| `ThePlayer571.SoyoRuntimeConsole.Editor.Tests` | `Tests/Editor/` | Editor tests (NUnit, constrained to `UNITY_INCLUDE_TESTS`) |